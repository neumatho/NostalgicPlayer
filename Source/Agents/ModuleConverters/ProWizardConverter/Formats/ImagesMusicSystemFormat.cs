/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Images Music System
	///
	/// Based on the loader from LibXmp
	/// </summary>
	internal class ImagesMusicSystemFormat : ProWizardConverterWorker31SamplesBase
	{
		private static readonly short[] extendedPeriods =
		[
			1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016,  960,  906,
			 856,  808,  762,  720,  678,  640,  604,  570,  538,  508,  480,  453,
			 428,  404,  381,  360,  339,  320,  302,  285,  269,  254,  240,  226,
			 214,  202,  190,  180,  170,  160,  151,  143,  135,  127,  120,  113,
			   0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
			   0,    0,    0,    0
		];

		private ushort realNumberOfPatterns;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT70;
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

			moduleStream.Seek(20, SeekOrigin.Begin);

			byte[] buffer = new byte[128];
			int sampleSize = 0;

			for (int i = 0; i < 31; i++)
			{
				moduleStream.ReadInto(buffer, 0, 20);

				moduleStream.Seek(2, SeekOrigin.Current);
				ushort length = moduleStream.Read_B_UINT16();

				moduleStream.Seek(1, SeekOrigin.Current);
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return false;

				sampleSize += length * 2;

				if (!TestName(buffer))
					return false;

				if (volume > 0x40)
					return false;

				if (length > 0x8000)
					return false;

				if (loopStart > length)
					return false;

				if ((length != 0) && (loopLength > (2 * length)))
					return false;
			}

			if (sampleSize < 8)
				return false;

			byte numberOfPositions = moduleStream.Read_UINT8();
			byte zero = moduleStream.Read_UINT8();

			moduleStream.ReadInto(buffer, 0, 128);

			uint magic = moduleStream.Read_B_UINT32();

			if (moduleStream.EndOfStream)
				return false;

			if (zero > 1)		// Not sure what this is
				return false;

			if ((magic & 0x000000ff) != 0x3c)
				return false;

			if ((numberOfPositions > 0x7f) || (numberOfPositions == 0))
				return false;

			if (FindHighestPatternNumber(buffer) > 0x7f)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module if any
		/// </summary>
		/********************************************************************/
		protected override byte[] GetModuleName(ModuleStream moduleStream)
		{
			byte[] moduleName = new byte[20];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(moduleName, 0, 20);

			return moduleName;
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
				moduleStream.ReadInto(name, 0, 20);

				byte fineTune = (byte)((-(sbyte)moduleStream.Read_B_UINT16()) & 0x0f);
				ushort length = moduleStream.Read_B_UINT16();
				byte volume = (byte)moduleStream.Read_B_UINT16();
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
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected override Span<byte> GetPositionList(ModuleStream moduleStream)
		{
			byte numberOfPositions = moduleStream.Read_UINT8();
			moduleStream.Seek(1, SeekOrigin.Current);

			byte[] positionList = new byte[numberOfPositions];
			moduleStream.ReadInto(positionList, 0, numberOfPositions);

			moduleStream.Seek(128 - numberOfPositions, SeekOrigin.Current);

			// We don't want to save unused patterns
			realNumberOfPatterns = numberOfPatterns;
			FindHighestPatternNumber(positionList);

			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];

			// Skip mark
			moduleStream.Seek(4, SeekOrigin.Current);

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 64 * 4; j++)
				{
					// Pattern format:
					//
					// 0000 0000  0000 0000  0000 0000
					//  |\     /  \  / \  /  \       /
					//  | note    ins   fx   parameter
					// ins
					//
					// 0x3f is a blank note
					byte byt1;

					// Get sample number
					byte temp = moduleStream.Read_UINT8();

					if ((temp & 0x40) != 0)
						byt1 = 0x10;
					else
						byt1 = 0x00;

					// Low 4 bit of sample number, effect + effect value
					byte byt3 = moduleStream.Read_UINT8();
					byte byt4 = moduleStream.Read_UINT8();

					// Get note
					byte byt2 = (byte)(temp & 0x3f);
					if (byt2 != 0x3f)
					{
						byt1 |= (byte)((extendedPeriods[byt2] & 0x0f00) >> 8);
						byt2 = (byte)(extendedPeriods[byt2] & 0xff);
					}
					else
						byt2 = 0x00;

					// According to Asle:
					// "Just note that pattern break effect command (D**) uses
					// HEX values in UNIC format (while it is DEC values in PTK).
					// Thus, it has to be converted!"
					//
					// Is this valid for IMS as well? --claudio
					if ((byt3 & 0x0f) == 0x0d)
						byt4 = (byte)((byt4 / 10) * 0x10 + (byt4 % 10));

					// Beast-Busters.ims uses 800 for sub-song stop marker.
					// Convert this to F00 instead
					if (((byt3 & 0x0f) == 0x08) && (byt4 == 0x00))
						byt3 = (byte)((byt3 & 0xf0) | 0x0f);

					// Copy the pattern data
					pattern[j * 4] = byt1;
					pattern[j * 4 + 1] = byt2;
					pattern[j * 4 + 2] = byt3;
					pattern[j * 4 + 3] = byt4;
				}

				yield return pattern;
			}

			// If extra patterns are stored, skip them
			if (numberOfPatterns < realNumberOfPatterns)
				moduleStream.Seek((realNumberOfPatterns - numberOfPatterns) * 0x300, SeekOrigin.Current);
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, ConverterStream converterStream)
		{
			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Check the name for invalid characters
		/// </summary>
		/********************************************************************/
		private bool TestName(byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				if (buffer[i] > 0x7f)
					return false;

				if ((buffer[i] > 0) && (buffer[i] < 32))
					return false;
			}

			return true;
		}
		#endregion
	}
}
