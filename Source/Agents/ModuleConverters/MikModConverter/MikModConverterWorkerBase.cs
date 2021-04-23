/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter
{
	/// <summary>
	/// Base class for all the formats
	/// </summary>
	internal abstract class MikModConverterWorkerBase : IModuleConverterAgent
	{
		protected Module of;
		protected string originalFormat;

		#region IModuleConverterAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Identify(PlayerFileInfo fileInfo);



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public AgentResult Convert(PlayerFileInfo fileInfo, WriterStream writerStream, out string errorMessage)
		{
			// Load and convert the module into internal structures
			if (!ConvertModule(fileInfo.ModuleStream, out errorMessage))
				return AgentResult.Error;

			if (!ConvertToUniMod(fileInfo.ModuleStream, writerStream, out errorMessage))
				return AgentResult.Error;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public string OriginalFormat => originalFormat;
		#endregion

		/********************************************************************/
		/// <summary>
		/// Load the module into internal structures
		/// </summary>
		/********************************************************************/
		protected abstract bool LoadModule(ModuleStream moduleStream, MUniTrk uniTrk, out string errorMessage);

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
			byte maxChan = SharedConstant.UF_MaxChan;

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

			// Clean up
			uniTrk.UniCleanup();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the UniMod structures to a real module
		/// </summary>
		/********************************************************************/
		private bool ConvertToUniMod(ModuleStream moduleStream, WriterStream writerStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			MUniTrk uniTrk = new MUniTrk();

			// Fill in the UniMod header
			writerStream.Write_B_UINT32(0x4e50554e);			// Mark (NPUN)
			writerStream.Write_B_UINT16(0x100);					// Version
			writerStream.Write_B_UINT16((ushort)of.Flags);
			writerStream.Write_UINT8(of.NumChn);
			writerStream.Write_UINT8(of.NumVoices);
			writerStream.Write_B_UINT16(of.NumPos);
			writerStream.Write_B_UINT16(of.NumPat);
			writerStream.Write_B_UINT16(of.NumTrk);
			writerStream.Write_B_UINT16(of.NumIns);
			writerStream.Write_B_UINT16(of.NumSmp);
			writerStream.Write_B_UINT16(of.RepPos);
			writerStream.Write_UINT8(of.InitSpeed);
			writerStream.Write_UINT8((byte)of.InitTempo);
			writerStream.Write_UINT8(of.InitVolume);
			writerStream.Write_B_UINT16(of.BpmLimit);

			// Fill in the strings
			writerStream.WriteString(of.SongName);
			writerStream.WriteString(of.Comment);

			// Copy pattern positions
			writerStream.WriteArray_B_UINT16s(of.Positions, of.NumPos);

			// Copy panning positions
			writerStream.WriteArray_B_UINT16s(of.Panning, of.NumChn);

			// Copy the channel volumes
			writerStream.Write(of.ChanVol, 0, of.NumChn);

			// Copy the sample information
			for (int i = 0; i < of.NumSmp; i++)
			{
				Sample samp = of.Samples[i];

				writerStream.Write_B_UINT16((ushort)samp.Flags);
				writerStream.Write_B_UINT32(samp.Speed);
				writerStream.Write_UINT8(samp.Volume);
				writerStream.Write_B_UINT16((ushort)samp.Panning);
				writerStream.Write_B_UINT32(samp.Length);
				writerStream.Write_B_UINT32(samp.LoopStart);
				writerStream.Write_B_UINT32(samp.LoopEnd);
				writerStream.Write_B_UINT32(samp.SusBegin);
				writerStream.Write_B_UINT32(samp.SusEnd);
				writerStream.Write_UINT8(samp.GlobVol);
				writerStream.Write_UINT8((byte)samp.VibFlags);
				writerStream.Write_UINT8(samp.VibType);
				writerStream.Write_UINT8(samp.VibSweep);
				writerStream.Write_UINT8(samp.VibDepth);
				writerStream.Write_UINT8(samp.VibRate);
				writerStream.WriteString(samp.SampleName);
			}

			// Copy the instrument information
			if ((of.Flags & ModuleFlag.Inst) != 0)
			{
				for (int i = 0; i < of.NumIns; i++)
				{
					Instrument inst = of.Instruments[i];

					writerStream.Write_UINT8((byte)inst.Flags);
					writerStream.Write_UINT8((byte)inst.NnaType);
					writerStream.Write_UINT8((byte)inst.Dca);
					writerStream.Write_UINT8((byte)inst.Dct);
					writerStream.Write_UINT8(inst.GlobVol);
					writerStream.Write_B_UINT16((ushort)inst.Panning);
					writerStream.Write_UINT8(inst.PitPanSep);
					writerStream.Write_UINT8(inst.PitPanCenter);
					writerStream.Write_UINT8(inst.RVolVar);
					writerStream.Write_UINT8(inst.RPanVar);
					writerStream.Write_B_UINT16(inst.VolFade);

					writerStream.Write_UINT8((byte)inst.VolFlg);
					writerStream.Write_UINT8(inst.VolPts);
					writerStream.Write_UINT8(inst.VolSusBeg);
					writerStream.Write_UINT8(inst.VolSusEnd);
					writerStream.Write_UINT8(inst.VolBeg);
					writerStream.Write_UINT8(inst.VolEnd);

					for (int j = 0; j < SharedConstant.EnvPoints; j++)
					{
						writerStream.Write_B_UINT16((ushort)inst.VolEnv[j].Pos);
						writerStream.Write_B_UINT16((ushort)inst.VolEnv[j].Val);
					}

					writerStream.Write_UINT8((byte)inst.PanFlg);
					writerStream.Write_UINT8(inst.PanPts);
					writerStream.Write_UINT8(inst.PanSusBeg);
					writerStream.Write_UINT8(inst.PanSusEnd);
					writerStream.Write_UINT8(inst.PanBeg);
					writerStream.Write_UINT8(inst.PanEnd);

					for (int j = 0; j < SharedConstant.EnvPoints; j++)
					{
						writerStream.Write_B_UINT16((ushort)inst.PanEnv[j].Pos);
						writerStream.Write_B_UINT16((ushort)inst.PanEnv[j].Val);
					}

					writerStream.Write_UINT8((byte)inst.PitFlg);
					writerStream.Write_UINT8(inst.PitPts);
					writerStream.Write_UINT8(inst.PitSusBeg);
					writerStream.Write_UINT8(inst.PitSusEnd);
					writerStream.Write_UINT8(inst.PitBeg);
					writerStream.Write_UINT8(inst.PitEnd);

					for (int j = 0; j < SharedConstant.EnvPoints; j++)
					{
						writerStream.Write_B_UINT16((ushort)inst.PitEnv[j].Pos);
						writerStream.Write_B_UINT16((ushort)inst.PitEnv[j].Val);
					}

					writerStream.WriteArray_B_UINT16s(inst.SampleNumber, SharedConstant.InstNotes);
					writerStream.Write(inst.SampleNote, 0, SharedConstant.InstNotes);

					writerStream.WriteString(inst.InsName);
				}
			}

			// Copy number of rows in each pattern
			writerStream.WriteArray_B_UINT16s(of.PattRows, of.NumPat);

			// Copy indexes to the tracks
			writerStream.WriteArray_B_UINT16s(of.Patterns, of.NumPat * of.NumChn);

			// Copy the tracks
			for (int i = 0; i < of.NumTrk; i++)
			{
				if (of.Tracks[i] != null)
				{
					// Store the length
					ushort temp = uniTrk.UniTrkLen(of.Tracks[i]);

					writerStream.Write_B_UINT16(temp);
					writerStream.Write(of.Tracks[i], 0, temp);
				}
				else
					writerStream.Write_B_UINT16(0);
			}

			// Copy the samples
			for (int i = 0; i < of.NumSmp; i++)
			{
				Sample samp = of.Samples[i];
				uint temp = samp.Length;

				if (temp != 0)
				{
					if (samp.SeekPos != 0)
						moduleStream.Seek(samp.SeekPos, SeekOrigin.Begin);

					if ((samp.Flags & SampleFlag.ItPacked) != 0)
					{
						int endOffset = (int)((i == of.NumSmp - 1) ? moduleStream.Length : of.Samples[i + 1].SeekPos);

						while (moduleStream.Position < endOffset)
						{
							// Copy the packed length
							ushort packLen = moduleStream.Read_L_UINT16();
							writerStream.Write_L_UINT16(packLen);

							// Copy the sample
							byte[] tempBuffer = new byte[packLen];

							moduleStream.Read(tempBuffer, 0, packLen);
							writerStream.Write(tempBuffer, 0, packLen);
						}
					}
					else
					{
						if ((samp.Flags & SampleFlag.Stereo) != 0)
							temp *= 2;

						if ((samp.Flags & SampleFlag._16Bits) != 0)
							temp *= 2;

						// Copy the samples
						byte[] tempBuffer = new byte[temp];

						moduleStream.Read(tempBuffer, 0, (int)temp);
						writerStream.Write(tempBuffer, 0, (int)temp);
					}
				}
			}

			return true;
		}
		#endregion
	}
}
