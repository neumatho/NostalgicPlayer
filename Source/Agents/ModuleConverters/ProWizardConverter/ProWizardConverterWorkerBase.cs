/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter
{
	/// <summary>
	/// Base class for all the formats
	/// </summary>
	internal abstract class ProWizardConverterWorkerBase : ModuleConverterAgentBase
	{
		protected class SampleInfo
		{
			public byte[] Name;
			public ushort Length;
			public ushort LoopStart;
			public ushort LoopLength;
			public byte Volume;
			public byte FineTune;
		}

		#region Periods
		protected static readonly byte[,] periods = new byte[45, 2]
		{
			{ 0x3, 0x58 }, { 0x3, 0x28 }, { 0x2, 0xfa }, { 0x2, 0xd0 }, { 0x2, 0xa6 }, { 0x2, 0x80 }, { 0x2, 0x5c }, { 0x2, 0x3a }, { 0x2, 0x1a },
			{ 0x1, 0xfc }, { 0x1, 0xe0 }, { 0x1, 0xc5 }, { 0x1, 0xac }, { 0x1, 0x94 }, { 0x1, 0x7d }, { 0x1, 0x68 }, { 0x1, 0x53 }, { 0x1, 0x40 },
			{ 0x1, 0x2e }, { 0x1, 0x1d }, { 0x1, 0x0d }, { 0x0, 0xfe }, { 0x0, 0xf0 }, { 0x0, 0xe2 }, { 0x0, 0xd6 }, { 0x0, 0xca }, { 0x0, 0xbe },
			{ 0x0, 0xb4 }, { 0x0, 0xaa }, { 0x0, 0xa0 }, { 0x0, 0x97 }, { 0x0, 0x8f }, { 0x0, 0x87 }, { 0x0, 0x7f }, { 0x0, 0x78 }, { 0x0, 0x71 },
			{ 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }, { 0x0, 0x71 }
		};
		#endregion

		#region Tuning periods
		protected static short[,] tuningPeriods = new short[16, 36]
		{
			{
				856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453,
				428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240, 226,
				214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113
			},
			{
				850, 802, 757, 715, 674, 637, 601, 567, 535, 505, 477, 450,
				425, 401, 379, 357, 337, 318, 300, 284, 268, 253, 239, 225,
				213, 201, 189, 179, 169, 159, 150, 142, 134, 126, 119, 113
			},
			{
				844, 796, 752, 709, 670, 632, 597, 563, 532, 502, 474, 447,
				422, 398, 376, 355, 335, 316, 298, 282, 266, 251, 237, 224,
				211, 199, 188, 177, 167, 158, 149, 141, 133, 125, 118, 112
			},
			{
				838, 791, 746, 704, 665, 628, 592, 559, 528, 498, 470, 444,
				419, 395, 373, 352, 332, 314, 296, 280, 264, 249, 235, 222,
				209, 198, 187, 176, 166, 157, 148, 140, 132, 125, 118, 111
			},
			{
				832, 785, 741, 699, 660, 623, 588, 555, 524, 495, 467, 441,
				416, 392, 370, 350, 330, 312, 294, 278, 262, 247, 233, 220,
				208, 196, 185, 175, 165, 156, 147, 139, 131, 124, 117, 110
			},
			{
				826, 779, 736, 694, 655, 619, 584, 551, 520, 491, 463, 437,
				413, 390, 368, 347, 328, 309, 292, 276, 260, 245, 232, 219,
				206, 195, 184, 174, 164, 155, 146, 138, 130, 123, 116, 109
			},
			{
				820, 774, 730, 689, 651, 614, 580, 547, 516, 487, 460, 434,
				410, 387, 365, 345, 325, 307, 290, 274, 258, 244, 230, 217,
				205, 193, 183, 172, 163, 154, 145, 137, 129, 122, 115, 109
			},
			{
				814, 768, 725, 684, 646, 610, 575, 543, 513, 484, 457, 431,
				407, 384, 363, 342, 323, 305, 288, 272, 256, 242, 228, 216,
				204, 192, 181, 171, 161, 152, 144, 136, 128, 121, 114, 108
			},
			{
				907, 856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480,
				453, 428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240,
				226, 214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120
			},
			{
				900, 850, 802, 757, 715, 675, 636, 601, 567, 535, 505, 477,
				450, 425, 401, 379, 357, 337, 318, 300, 284, 268, 253, 238,
				225, 212, 200, 189, 179, 169, 159, 150, 142, 134, 126, 119
			},
			{
				894, 844, 796, 752, 709, 670, 632, 597, 563, 532, 502, 474,
				447, 422, 398, 376, 355, 335, 316, 298, 282, 266, 251, 237,
				223, 211, 199, 188, 177, 167, 158, 149, 141, 133, 125, 118
			},
			{
				887, 838, 791, 746, 704, 665, 628, 592, 559, 528, 498, 470,
				444, 419, 395, 373, 352, 332, 314, 296, 280, 264, 249, 235,
				222, 209, 198, 187, 176, 166, 157, 148, 140, 132, 125, 118
			},
			{
				881, 832, 785, 741, 699, 660, 623, 588, 555, 524, 494, 467,
				441, 416, 392, 370, 350, 330, 312, 294, 278, 262, 247, 233,
				220, 208, 196, 185, 175, 165, 156, 147, 139, 131, 123, 117
			},
			{
				875, 826, 779, 736, 694, 655, 619, 584, 551, 520, 491, 463,
				437, 413, 390, 368, 347, 328, 309, 292, 276, 260, 245, 232,
				219, 206, 195, 184, 174, 164, 155, 146, 138, 130, 123, 116
			},
			{
				868, 820, 774, 730, 689, 651, 614, 580, 547, 516, 487, 460,
				434, 410, 387, 365, 345, 325, 307, 290, 274, 258, 244, 230,
				217, 205, 193, 183, 172, 163, 154, 145, 137, 129, 122, 115
			},
			{
				862, 814, 768, 725, 684, 646, 610, 575, 543, 513, 484, 457,
				431, 407, 384, 363, 342, 323, 305, 288, 272, 256, 242, 228,
				216, 203, 192, 181, 171, 161, 152, 144, 136, 128, 121, 114
			}
		};
		#endregion

		protected const int MaxNumberOfMissingBytes = 256;

		protected uint[] sampleLengths;
		protected ushort numberOfPatterns;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			if (CheckModule(moduleStream))
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the converted module without samples if
		/// possible. 0 means unknown
		/// </summary>
		/********************************************************************/
		public override int ConvertedModuleLength(PlayerFileInfo fileInfo)
		{
			if (numberOfPatterns == 0)
				return 0;

			return 1084 + numberOfPatterns * 1024;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			byte[] zeroBuf = new byte[128];

			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First do any preparations
			sampleLengths = new uint[31];

			if (!PrepareConversion(moduleStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADERINFO;
				return AgentResult.Error;
			}

			// Write module name
			byte[] name = GetModuleName(moduleStream);
			converterStream.Write(name ?? zeroBuf, 0, 20);

			// Write sample information
			int sampleCount = 0;

			foreach (SampleInfo sampleInfo in GetSamples(moduleStream))
			{
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				converterStream.Write(sampleInfo.Name ?? zeroBuf, 0, 22);
				converterStream.Write_B_UINT16(sampleInfo.Length);
				converterStream.Write_UINT8(sampleInfo.FineTune);
				converterStream.Write_UINT8(sampleInfo.Volume);
				converterStream.Write_B_UINT16(sampleInfo.LoopStart);
				converterStream.Write_B_UINT16(sampleInfo.LoopLength);

				sampleLengths[sampleCount++] = (uint)sampleInfo.Length * 2;
			}

			// If no samples has been returned, something is wrong
			if (sampleCount == 0)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLEINFO;
				return AgentResult.Error;
			}

			// Write empty samples for the rest
			for (int i = sampleCount; i < 31; i++)
			{
				// Write sample name + size + fine tune + volume + repeat point
				converterStream.Write(zeroBuf, 0, 28);

				// Write repeat length
				converterStream.Write_B_UINT16(1);
			}

			// Write song length
			Span<byte> positionList = GetPositionList(moduleStream);
			if ((positionList == null) || (positionList.Length == 0) || moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADERINFO;
				return AgentResult.Error;
			}

			converterStream.Write_UINT8((byte)positionList.Length);

			// Write restart position
			converterStream.Write_UINT8(GetRestartPosition(moduleStream));

			// Write position list
			converterStream.Write(positionList);
			if (positionList.Length < 128)
				converterStream.Write(zeroBuf, 0, 128 - positionList.Length);

			// Write mark
			converterStream.Write_B_UINT32(GetMark());

			// Write the patterns
			foreach (byte[] patternData in GetPatterns(moduleStream))
			{
				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_ERR_LOADING_PATTERNS;
					return AgentResult.Error;
				}

				converterStream.Write(patternData);
			}

			// At last, write the sample data
			if (!WriteSampleData(moduleStream, converterStream))
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
				return AgentResult.Error;
			}

			errorMessage = string.Empty;

			return AgentResult.Ok;
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected abstract bool CheckModule(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected virtual bool PrepareConversion(ModuleStream moduleStream)
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module if any
		/// </summary>
		/********************************************************************/
		protected virtual byte[] GetModuleName(ModuleStream moduleStream)
		{
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected abstract IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected abstract Span<byte> GetPositionList(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected virtual byte GetRestartPosition(ModuleStream moduleStream)
		{
			return 0x7f;
		}



		/********************************************************************/
		/// <summary>
		/// Return the ID mark
		/// </summary>
		/********************************************************************/
		protected virtual uint GetMark()
		{
			return 0x4d2e4b2e;		// M.K.
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected abstract IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected abstract bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Find the highest pattern number by looking in the position list
		/// at the offset given
		/// </summary>
		/********************************************************************/
		protected ushort FindHighestPatternNumber(ModuleStream moduleStream, int positionListOffset, int positionListLength)
		{
			byte[] positionList = new byte[positionListLength];

			moduleStream.Seek(positionListOffset, SeekOrigin.Begin);
			moduleStream.Read(positionList, 0, positionListLength);

			return FindHighestPatternNumber(positionList);
		}



		/********************************************************************/
		/// <summary>
		/// Find the highest pattern number by looking in the position list
		/// at the offset given
		/// </summary>
		/********************************************************************/
		protected ushort FindHighestPatternNumber(Span<byte> positionList)
		{
			numberOfPatterns = 0;
			foreach (byte patternNumber in positionList)
			{
				if (patternNumber > numberOfPatterns)
					numberOfPatterns = patternNumber;
			}

			numberOfPatterns++;

			return numberOfPatterns;
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		protected byte[] BuildPositionList<T>(int numberOfPositions, Func<int, T> getNumber) where T : struct
		{
			return BuildPositionList(numberOfPositions, out _, getNumber);
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		protected byte[] BuildPositionList<T>(int numberOfPositions, out List<T> usedNumbers, Func<int, T> getNumber) where T : struct
		{
			byte[] positionList = new byte[numberOfPositions];

			// Begin to create the position table
			usedNumbers = new List<T>();
			numberOfPatterns = 0;

			for (int i = 0; i < numberOfPositions; i++)
			{
				T nextNumber = getNumber(i);

				// Check to see if this track combination has already been used previously
				bool found = false;

				for (int j = 0; j < usedNumbers.Count; j++)
				{
					if (usedNumbers[j].Equals(nextNumber))
					{
						// Found an equal track combination
						positionList[i] = (byte)j;
						found = true;
						break;
					}
				}

				if (!found)
				{
					positionList[i] = (byte)numberOfPatterns++;
					usedNumbers.Add(nextNumber);
				}
			}

			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Will save marks for all the samples. Note that the position of
		/// the stream has to be on the first sample
		/// </summary>
		/********************************************************************/
		protected bool SaveMarksForAllSamples(ModuleStream moduleStream, ConverterStream converterStream)
		{
			for (int i = 0; i < sampleLengths.Length; i++)
			{
				int length = (int)sampleLengths[i];
				if (length != 0)
				{
					// Check to see if we miss too much from the last sample
					if (moduleStream.Length - moduleStream.Position < (length - MaxNumberOfMissingBytes))
						return false;

					moduleStream.SetSampleDataInfo(i, length);
					converterStream.WriteSampleDataMarker(i, length);
				}
			}

			return true;
		}
		#endregion
	}
}
