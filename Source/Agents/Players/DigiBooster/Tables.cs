﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBooster
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// Periods
		/// </summary>
		/********************************************************************/
		public static readonly ushort[,] Periods =
		{
			// Tuning -8
			{
				907, 856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480,
				453, 428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240,
				226, 214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120
			},

			// Tuning -7
			{
				900, 850, 802, 757, 715, 675, 636, 601, 567, 535, 505, 477,
				450, 425, 401, 379, 357, 337, 318, 300, 284, 268, 253, 238,
				225, 212, 200, 189, 179, 169, 159, 150, 142, 134, 126, 119
			},

			// Tuning -6
			{
				894, 844, 796, 752, 709, 670, 632, 597, 563, 532, 502, 474,
				447, 422, 398, 376, 355, 335, 316, 298, 282, 266, 251, 237,
				223, 211, 199, 188, 177, 167, 158, 149, 141, 133, 125, 118
			},

			// Tuning -5
			{
				 887, 838, 791, 746, 704, 665, 628, 592, 559, 528, 498, 470,
				 444, 419, 395, 373, 352, 332, 314, 296, 280, 264, 249, 235,
				 222, 209, 198, 187, 176, 166, 157, 148, 140, 132, 125, 118
			},

			// Tuning -4
			{
				881, 832, 785, 741, 699, 660, 623, 588, 555, 524, 494, 467,
				441, 416, 392, 370, 350, 330, 312, 294, 278, 262, 247, 233,
				220, 208, 196, 185, 175, 165, 156, 147, 139, 131, 123, 117
			},

			// Tuning -3
			{
				875, 826, 779, 736, 694, 655, 619, 584, 551, 520, 491, 463,
				437, 413, 390, 368, 347, 328, 309, 292, 276, 260, 245, 232,
				219, 206, 195, 184, 174, 164, 155, 146, 138, 130, 123, 116
			},

			// Tuning -2
			{
				868, 820, 774, 730, 689, 651, 614, 580, 547, 516, 487, 460,
				434, 410, 387, 365, 345, 325, 307, 290, 274, 258, 244, 230,
				217, 205, 193, 183, 172, 163, 154, 145, 137, 129, 122, 115
			},

			// Tuning -1
			{
				862, 814, 768, 725, 684, 646, 610, 575, 543, 513, 484, 457,
				431, 407, 384, 363, 342, 323, 305, 288, 272, 256, 242, 228,
				216, 203, 192, 181, 171, 161, 152, 144, 136, 128, 121, 114
			},

			// Tuning 0, normal
			{
				856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453,
				428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240, 226,
				214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113
			},

			// Tuning 1
			{
				850, 802, 757, 715, 674, 637, 601, 567, 535, 505, 477, 450,
				425, 401, 379, 357, 337, 318, 300, 284, 268, 253, 239, 225,
				213, 201, 189, 179, 169, 159, 150, 142, 134, 126, 119, 113
			},

			// Tuning 2
			{
				844, 796, 752, 709, 670, 632, 597, 563, 532, 502, 474, 447,
				422, 398, 376, 355, 335, 316, 298, 282, 266, 251, 237, 224,
				211, 199, 188, 177, 167, 158, 149, 141, 133, 125, 118, 112
			},

			// Tuning 3
			{
				838, 791, 746, 704, 665, 628, 592, 559, 528, 498, 470, 444,
				419, 395, 373, 352, 332, 314, 296, 280, 264, 249, 235, 222,
				209, 198, 187, 176, 166, 157, 148, 140, 132, 125, 118, 111
			},

			// Tuning 4
			{
				832, 785, 741, 699, 660, 623, 588, 555, 524, 495, 467, 441,
				416, 392, 370, 350, 330, 312, 294, 278, 262, 247, 233, 220,
				208, 196, 185, 175, 165, 156, 147, 139, 131, 124, 117, 110
			},

			// Tuning 5
			{
				826, 779, 736, 694, 655, 619, 584, 551, 520, 491, 463, 437,
				413, 390, 368, 347, 328, 309, 292, 276, 260, 245, 232, 219,
				206, 195, 184, 174, 164, 155, 146, 138, 130, 123, 116, 109
			},

			// Tuning 6
			{
				820, 774, 730, 689, 651, 614, 580, 547, 516, 487, 460, 434,
				410, 387, 365, 345, 325, 307, 290, 274, 258, 244, 230, 217,
				205, 193, 183, 172, 163, 154, 145, 137, 129, 122, 115, 109
			},

			// Tuning 7
			{
				814, 768, 725, 684, 646, 610, 575, 543, 513, 484, 457, 431,
				407, 384, 363, 342, 323, 305, 288, 272, 256, 242, 228, 216,
				204, 192, 181, 171, 161, 152, 144, 136, 128, 121, 114, 108
			}
		};



		/********************************************************************/
		/// <summary>
		/// Arpeggio
		/// </summary>
		/********************************************************************/
		public static readonly byte[] ArpeggioList =
		{
			0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1
		};



		/********************************************************************/
		/// <summary>
		/// Vibrato sinus curve
		/// </summary>
		/********************************************************************/
		public static readonly byte[] VibratoSin =
		{
			0x00, 0x18, 0x31, 0x4a, 0x61, 0x78, 0x8d, 0xa1, 0xb4, 0xc5, 0xd4, 0xe0, 0xeb, 0xf4, 0xfa, 0xfd,
			0xff, 0xfd, 0xfa, 0xf4, 0xeb, 0xe0, 0xd4, 0xc5, 0xb4, 0xa1, 0x8d, 0x78, 0x61, 0x4a, 0x31, 0x18
		};



		/********************************************************************/
		/// <summary>
		/// BCD convert table
		/// </summary>
		/********************************************************************/
		public static readonly byte[] Hex =
		{
			 0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 0, 0, 0, 0, 0, 0,
			10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 0, 0, 0, 0, 0, 0,
			20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 0, 0, 0, 0, 0, 0,
			30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 0, 0, 0, 0, 0, 0,
			40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 0, 0, 0, 0, 0, 0,
			50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 0, 0, 0, 0, 0, 0,
			60, 61, 62, 63
		};
	}
}