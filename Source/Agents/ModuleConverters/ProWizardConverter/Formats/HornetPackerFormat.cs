/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Hornet Packer
	/// </summary>
	internal class HornetPackerFormat : ProRunner1Format
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT11;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x43c)
				return false;

			// Check ID
			moduleStream.Seek(0x438, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x48525421)		// HRT!
				return false;

			// Check sample information
			moduleStream.Seek(20, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(22, SeekOrigin.Current);

				// Check sample size
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x8000)
					return false;

				samplesSize += temp * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop values
				uint temp1 = moduleStream.Read_B_UINT16();
				uint temp2 = moduleStream.Read_B_UINT16();

				if (temp != 0)
				{
					if ((temp - (int)(temp1 + temp2)) < -0x10)
						return false;
				}
				else
				{
					if ((temp1 + temp2) > 2)
						return false;
				}
			}

			// Check number of positions
			moduleStream.Seek(0x3b6, SeekOrigin.Begin);

			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength > 0x7f) || (positionListLength == 0x00))
				return false;

			// Check NTK byte
			if (moduleStream.Read_UINT8() > 0x7f)
				return false;

			// Check number of patterns
			if (FindHighestPatternNumber(moduleStream, 0x3b8, 128) > 64)
				return false;

			// Check the module length
			if ((0x43c + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			uint biggestPattern = numberOfPatterns * 256U;

			// Check the first 5 patterns
			moduleStream.Seek(0x43c, SeekOrigin.Begin);

			ushort numToCheck = (ushort)(biggestPattern < 0x500 ? biggestPattern : 0x500);
			bool gotNotes = false;

			for (int i = 0; i < numToCheck; i++)
			{
				uint temp1 = moduleStream.Read_B_UINT32();
				if (temp1 != 0)
				{
					// The unused nibble has to be zero
					if ((temp1 & 0x0000f000) != 0)
						return false;

					// Check note
					ushort temp = (ushort)((temp1 & 0x00ff0000) >> 16);
					if (temp > 0x48)
						return false;

					// Check sample number
					temp = (ushort)((temp1 & 0xff000000) >> 24);
					if (temp > 0x3e)
						return false;

					gotNotes = true;
				}
			}

			// Did we get any notes?
			if (!gotNotes)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			for (int i = 0; i < 31; i++)
			{
				byte[] name = new byte[22];
				moduleStream.Read(name, 0, 18);
				moduleStream.Seek(4, SeekOrigin.Current);

				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = name,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = fineTune
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			return GetAllPatterns(moduleStream, 2);
		}
		#endregion
	}
}
