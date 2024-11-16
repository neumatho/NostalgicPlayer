/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter
{
	/// <summary>
	/// Base class for all the formats
	/// </summary>
	internal abstract class ProWizardConverterWorker31SamplesBase : ProWizardConverterWorkerBase
	{
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
			if ((positionList == Span<byte>.Empty) || (positionList.Length == 0) || moduleStream.EndOfStream)
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
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected override byte GetRestartPosition(ModuleStream moduleStream)
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
		#endregion
	}
}
