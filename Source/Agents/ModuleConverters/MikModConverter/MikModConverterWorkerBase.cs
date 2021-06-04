/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter
{
	/// <summary>
	/// Base class for all the formats
	/// </summary>
	internal abstract class MikModConverterWorkerBase : ModuleConverterAgentBase
	{
		protected Module of;
		protected string originalFormat;

		protected readonly bool curious = false;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			// Load and convert the module into internal structures
			if (ConvertModuleData)
			{
				if (!ConvertModule(fileInfo.ModuleStream, out errorMessage))
					return AgentResult.Error;

				if (!ConvertToUniMod(fileInfo.ModuleStream, converterStream, out errorMessage))
					return AgentResult.Error;
			}
			else
			{
				// This converter extract the data from some kind of container
				if (!ExtractModule(fileInfo.ModuleStream, converterStream, out errorMessage))
					return AgentResult.Error;
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => originalFormat;
		#endregion

		/********************************************************************/
		/// <summary>
		/// Tells if the converter loads data into the Module (of) or not
		/// </summary>
		/********************************************************************/
		protected virtual bool ConvertModuleData => true;



		/********************************************************************/
		/// <summary>
		/// Load the module into internal structures
		/// </summary>
		/********************************************************************/
		protected virtual bool LoadModule(ModuleStream moduleStream, MUniTrk uniTrk, out string errorMessage)
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// Extract a module from a container and store it in the converter
		/// stream
		/// </summary>
		/********************************************************************/
		protected virtual bool ExtractModule(ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			throw new NotImplementedException();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will convert the module to UniMod structures
		/// </summary>
		/********************************************************************/
		private bool ConvertModule(ModuleStream moduleStream, out string errorMessage)
		{
			MUniTrk uniTrk = new MUniTrk();

			if (!uniTrk.UniInit())
			{
				errorMessage = Resources.IDS_MIKCONV_ERR_INITIALIZE;
				return false;
			}

			try
			{
				originalFormat = null;

				// Initialize the UniMod structure
				of = new Module();

				of.BpmLimit = 33;
				of.InitVolume = 128;

				// Init volume and panning array
				for (int t = 0; t < SharedConstant.UF_MaxChan; t++)
				{
					of.ChanVol[t] = 64;
					of.Panning[t] = (ushort)(((t + 1) & 2) != 0 ? SharedConstant.Pan_Right : SharedConstant.Pan_Left);
				}

				if (!LoadModule(moduleStream, uniTrk, out errorMessage))
					return false;

				// If the module doesn't have any specific panning, create a
				// MOD-like panning, with the channels half-separated
				if ((of.Flags & ModuleFlag.Panning) == 0)
				{
					for (int t = 0; t < of.NumChn; t++)
						of.Panning[t] = (ushort)(((t + 1) & 2) != 0 ? SharedConstant.Pan_HalfRight : SharedConstant.Pan_HalfLeft);
				}

				// Find number of channels to use
				byte maxChan = 120;		// The maximum number of channels to use (including NNA)

				if (((of.Flags & ModuleFlag.Nna) == 0) && (of.NumChn < maxChan))
					maxChan = of.NumChn;
				else
				{
					if ((of.NumVoices != 0) && (of.NumVoices < maxChan))
						maxChan = of.NumVoices;
				}

				if (maxChan < of.NumChn)
					of.Flags |= ModuleFlag.Nna;

				of.NumVoices = maxChan;
			}
			finally
			{
				// Clean up
				uniTrk.UniCleanup();
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the UniMod structures to a real module
		/// </summary>
		/********************************************************************/
		private bool ConvertToUniMod(ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			MUniTrk uniTrk = new MUniTrk();

			// Fill in the UniMod header
			converterStream.Write_B_UINT32(0x4e50554e);			// Mark (NPUN)
			converterStream.Write_B_UINT16(0x0100);				// Version
			converterStream.Write_B_UINT16((ushort)of.Flags);
			converterStream.Write_UINT8(of.NumChn);
			converterStream.Write_UINT8(of.NumVoices);
			converterStream.Write_B_UINT16(of.NumPos);
			converterStream.Write_B_UINT16(of.NumPat);
			converterStream.Write_B_UINT16(of.NumTrk);
			converterStream.Write_B_UINT16(of.NumIns);
			converterStream.Write_B_UINT16(of.NumSmp);
			converterStream.Write_B_UINT16(of.RepPos);
			converterStream.Write_UINT8(of.InitSpeed);
			converterStream.Write_UINT8((byte)of.InitTempo);
			converterStream.Write_UINT8(of.InitVolume);
			converterStream.Write_B_UINT16(of.BpmLimit);

			// Fill in the strings
			converterStream.WriteString(of.SongName);
			converterStream.WriteString(of.Comment);

			// Copy pattern positions
			converterStream.WriteArray_B_UINT16s(of.Positions, of.NumPos);

			// Copy panning positions
			converterStream.WriteArray_B_UINT16s(of.Panning, of.NumChn);

			// Copy the channel volumes
			converterStream.Write(of.ChanVol, 0, of.NumChn);

			// Copy the sample information
			for (int i = 0; i < of.NumSmp; i++)
			{
				Sample samp = of.Samples[i];

				converterStream.Write_B_UINT16((ushort)samp.Flags);
				converterStream.Write_B_UINT32(samp.Speed);
				converterStream.Write_UINT8(samp.Volume);
				converterStream.Write_B_UINT16((ushort)samp.Panning);
				converterStream.Write_B_UINT32(samp.Length);
				converterStream.Write_B_UINT32(samp.LoopStart);
				converterStream.Write_B_UINT32(samp.LoopEnd);
				converterStream.Write_B_UINT32(samp.SusBegin);
				converterStream.Write_B_UINT32(samp.SusEnd);
				converterStream.Write_UINT8(samp.GlobVol);
				converterStream.Write_UINT8((byte)samp.VibFlags);
				converterStream.Write_UINT8(samp.VibType);
				converterStream.Write_UINT8(samp.VibSweep);
				converterStream.Write_UINT8(samp.VibDepth);
				converterStream.Write_UINT8(samp.VibRate);
				converterStream.WriteString(samp.SampleName);
			}

			// Copy the instrument information
			if ((of.Flags & ModuleFlag.Inst) != 0)
			{
				for (int i = 0; i < of.NumIns; i++)
				{
					Instrument inst = of.Instruments[i];

					converterStream.Write_UINT8((byte)inst.Flags);
					converterStream.Write_UINT8((byte)inst.NnaType);
					converterStream.Write_UINT8((byte)inst.Dca);
					converterStream.Write_UINT8((byte)inst.Dct);
					converterStream.Write_UINT8(inst.GlobVol);
					converterStream.Write_B_UINT16((ushort)inst.Panning);
					converterStream.Write_UINT8(inst.PitPanSep);
					converterStream.Write_UINT8(inst.PitPanCenter);
					converterStream.Write_UINT8(inst.RVolVar);
					converterStream.Write_UINT8(inst.RPanVar);
					converterStream.Write_B_UINT16(inst.VolFade);

					converterStream.Write_UINT8((byte)inst.VolFlg);
					converterStream.Write_UINT8(inst.VolPts);
					converterStream.Write_UINT8(inst.VolSusBeg);
					converterStream.Write_UINT8(inst.VolSusEnd);
					converterStream.Write_UINT8(inst.VolBeg);
					converterStream.Write_UINT8(inst.VolEnd);

					for (int j = 0; j < SharedConstant.EnvPoints; j++)
					{
						converterStream.Write_B_UINT16((ushort)inst.VolEnv[j].Pos);
						converterStream.Write_B_UINT16((ushort)inst.VolEnv[j].Val);
					}

					converterStream.Write_UINT8((byte)inst.PanFlg);
					converterStream.Write_UINT8(inst.PanPts);
					converterStream.Write_UINT8(inst.PanSusBeg);
					converterStream.Write_UINT8(inst.PanSusEnd);
					converterStream.Write_UINT8(inst.PanBeg);
					converterStream.Write_UINT8(inst.PanEnd);

					for (int j = 0; j < SharedConstant.EnvPoints; j++)
					{
						converterStream.Write_B_UINT16((ushort)inst.PanEnv[j].Pos);
						converterStream.Write_B_UINT16((ushort)inst.PanEnv[j].Val);
					}

					converterStream.Write_UINT8((byte)inst.PitFlg);
					converterStream.Write_UINT8(inst.PitPts);
					converterStream.Write_UINT8(inst.PitSusBeg);
					converterStream.Write_UINT8(inst.PitSusEnd);
					converterStream.Write_UINT8(inst.PitBeg);
					converterStream.Write_UINT8(inst.PitEnd);

					for (int j = 0; j < SharedConstant.EnvPoints; j++)
					{
						converterStream.Write_B_UINT16((ushort)inst.PitEnv[j].Pos);
						converterStream.Write_B_UINT16((ushort)inst.PitEnv[j].Val);
					}

					converterStream.WriteArray_B_UINT16s(inst.SampleNumber, SharedConstant.InstNotes);
					converterStream.Write(inst.SampleNote, 0, SharedConstant.InstNotes);

					converterStream.WriteString(inst.InsName);
				}
			}

			// Copy number of rows in each pattern
			converterStream.WriteArray_B_UINT16s(of.PattRows, of.NumPat);

			// Copy indexes to the tracks
			converterStream.WriteArray_B_UINT16s(of.Patterns, of.NumPat * of.NumChn);

			// Copy the tracks
			for (int i = 0; i < of.NumTrk; i++)
			{
				if (of.Tracks[i] != null)
				{
					// Store the length
					ushort temp = uniTrk.UniTrkLen(of.Tracks[i]);

					converterStream.Write_B_UINT16(temp);
					converterStream.Write(of.Tracks[i], 0, temp);
				}
				else
					converterStream.Write_B_UINT16(0);
			}

			// Copy the samples
			for (int i = 0; i < of.NumSmp; i++)
			{
				Sample samp = of.Samples[i];
				uint temp = samp.Length;

				if (temp != 0)
				{
					if ((samp.Flags & SampleFlag._16Bits) != 0)
						temp *= 2;

					if (samp.SeekPos != 0)
						moduleStream.Seek(samp.SeekPos, SeekOrigin.Begin);

					if ((samp.Flags & SampleFlag.ItPacked) != 0)
					{
						int endOffset = (int)((i == of.NumSmp - 1) ? moduleStream.Length : of.Samples[i + 1].SeekPos);

						// Set marker in converted stream
						long curPos = moduleStream.Position;
						moduleStream.SetSampleDataInfo(i, (int)temp);
						converterStream.WriteSampleDataMarker(i, (int)temp);
						moduleStream.Position = curPos;

						while (moduleStream.Position < endOffset)
						{
							// Copy the packed length
							ushort packLen = moduleStream.Read_L_UINT16();
							converterStream.Write_L_UINT16(packLen);

							// Skip the data
							moduleStream.Seek(packLen, SeekOrigin.Current);
						}
					}
					else
					{
						// Mark the sample data
						moduleStream.SetSampleDataInfo(i, (int)temp);
						converterStream.WriteSampleDataMarker(i, (int)temp);
					}
				}
			}

			return true;
		}
		#endregion
	}
}
