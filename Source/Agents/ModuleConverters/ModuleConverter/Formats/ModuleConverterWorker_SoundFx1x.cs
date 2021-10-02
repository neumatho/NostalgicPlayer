/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats
{
	/// <summary>
	/// Can convert SoundFX 1.x to SoundFX 2.0 format
	/// </summary>
	internal class ModuleConverterWorker_SoundFx1x : ModuleConverterAgentBase
	{
		#region IModuleConverterAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			long fileSize = moduleStream.Length;
			if (fileSize < 80)
				return AgentResult.Unknown;

			// Read the sample size table
			uint[] sampleSizes = new uint[15];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT32s(sampleSizes, 0, 15);

			// Check the mark
			if (moduleStream.Read_B_UINT32() != 0x534f4e47)		// SONG
				return AgentResult.Unknown;

			// Check the sample sizes
			uint total = 0;
			for (int i = 0; i < 15; i++)
				total += sampleSizes[i];

			if (total > fileSize)
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Copy the sample size table
			uint[] sampleSizes = new uint[15];

			moduleStream.ReadArray_B_UINT32s(sampleSizes, 0, 15);
			converterStream.WriteArray_B_UINT32s(sampleSizes, 15);

			// Fill the rest of the sample size table with zeros
			for (int i = 0; i < 16; i++)
				converterStream.Write_B_UINT32(0);

			// Write the ID mark
			moduleStream.Seek(4, SeekOrigin.Current);
			converterStream.Write_B_UINT32(0x534f3331);			// SO31

			// Copy the delay value and pads
			Helpers.CopyData(moduleStream, converterStream, 2 + 14);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Copy the first 15 samples information
			Helpers.CopyData(moduleStream, converterStream, 30 * 15);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLEINFO;
				return AgentResult.Error;
			}

			// Write 16 empty samples
			byte[] name = new byte[22];
			for (int i = 0; i < 16; i++)
			{
				converterStream.Write(name, 0, 22);		// Name
				converterStream.Write_B_UINT16(1);				// Length
				converterStream.Write_B_UINT16(0);				// Volume
				converterStream.Write_B_UINT16(0);				// Loop start
				converterStream.Write_B_UINT16(0);				// Loop length
			}

			// Copy the song length
			byte songLength = moduleStream.Read_UINT8();
			moduleStream.Seek(1, SeekOrigin.Current);
			converterStream.Write_UINT8(songLength);
			converterStream.Write_UINT8(0);

			// Copy the orders
			byte[] orders = new byte[128];
			moduleStream.Read(orders, 0, 128);
			converterStream.Write(orders, 0, 128);

			// Write pad
			converterStream.Write_B_UINT32(0);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Find highest pattern number
			ushort maxPattern = 0;

			for (int i = 0; i < songLength; i++)
			{
				if (orders[i] > maxPattern)
					maxPattern = orders[i];
			}

			maxPattern++;

			// Copy the patterns
			Helpers.CopyData(moduleStream, converterStream, maxPattern * 4 * 4 * 64);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_PATTERNS;
				return AgentResult.Error;
			}

			// Copy sample data
			for (int i = 0; i < 15; i++)
			{
				int length = (int)sampleSizes[i];
				if (length != 0)
				{
					// Check to see if we miss too much from the last sample
					if (moduleStream.Length - moduleStream.Position < (length - 512))
					{
						errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
						return AgentResult.Error;
					}

					moduleStream.SetSampleDataInfo(i, length);
					converterStream.WriteSampleDataMarker(i, length);
				}
			}

			return AgentResult.Ok;
		}
		#endregion
	}
}
