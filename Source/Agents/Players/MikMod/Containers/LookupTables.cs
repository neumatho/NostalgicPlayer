/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.MikMod.Containers
{
	/// <summary>
	/// Holds different lookup tables
	/// </summary>
	internal static class LookupTables
	{
		public static readonly ushort[] OldPeriods = new ushort[SharedConstant.Octave * 2]
		{
			0x6b00, 0x6800, 0x6500, 0x6220, 0x5f50, 0x5c80,
			0x5a00, 0x5740, 0x54d0, 0x5260, 0x5010, 0x4dc0,
			0x4b90, 0x4960, 0x4750, 0x4540, 0x4350, 0x4160,
			0x3f90, 0x3dc0, 0x3c10, 0x3a40, 0x38b0, 0x3700
		};

		public static readonly byte[] VibratoTable = new byte[128]
		{
			  0,   6,  13,  19,  25,  31,  37,  44,  50,  56,  62,  68,  74,  80,  86,  92,
			 98, 103, 109, 115, 120, 126, 131, 136, 142, 147, 152, 157, 162, 167, 171, 176,
			180, 185, 189, 193, 197, 201, 205, 208, 212, 215, 219, 222, 225, 228, 231, 233,
			236, 238, 240, 242, 244, 246, 247, 249, 250, 251, 252, 253, 254, 254, 255, 255,
			255, 255, 255, 254, 254, 253, 252, 251, 250, 249, 247, 246, 244, 242, 240, 238,
			236, 233, 231, 228, 225, 222, 219, 215, 212, 208, 205, 201, 197, 193, 189, 185,
			180, 176, 171, 167, 162, 157, 152, 147, 142, 136, 131, 126, 120, 115, 109, 103,
			 98,  92,  86,  80,  74,  68,  62,  56,  50,  44,  37,  31,  25,  19,  13,   6
		};

		public static readonly ushort[] LogTab = new ushort[104]
		{
			Constant.LogFac * 907, Constant.LogFac * 900, Constant.LogFac * 894, Constant.LogFac * 887,
			Constant.LogFac * 881, Constant.LogFac * 875, Constant.LogFac * 868, Constant.LogFac * 862,
			Constant.LogFac * 856, Constant.LogFac * 850, Constant.LogFac * 844, Constant.LogFac * 838,
			Constant.LogFac * 832, Constant.LogFac * 826, Constant.LogFac * 820, Constant.LogFac * 814,
			Constant.LogFac * 808, Constant.LogFac * 802, Constant.LogFac * 796, Constant.LogFac * 791,
			Constant.LogFac * 785, Constant.LogFac * 779, Constant.LogFac * 774, Constant.LogFac * 768,
			Constant.LogFac * 762, Constant.LogFac * 757, Constant.LogFac * 752, Constant.LogFac * 746,
			Constant.LogFac * 741, Constant.LogFac * 736, Constant.LogFac * 730, Constant.LogFac * 725,
			Constant.LogFac * 720, Constant.LogFac * 715, Constant.LogFac * 709, Constant.LogFac * 704,
			Constant.LogFac * 699, Constant.LogFac * 694, Constant.LogFac * 689, Constant.LogFac * 684,
			Constant.LogFac * 678, Constant.LogFac * 675, Constant.LogFac * 670, Constant.LogFac * 665,
			Constant.LogFac * 660, Constant.LogFac * 655, Constant.LogFac * 651, Constant.LogFac * 646,
			Constant.LogFac * 640, Constant.LogFac * 636, Constant.LogFac * 632, Constant.LogFac * 628,
			Constant.LogFac * 623, Constant.LogFac * 619, Constant.LogFac * 614, Constant.LogFac * 610,
			Constant.LogFac * 604, Constant.LogFac * 601, Constant.LogFac * 597, Constant.LogFac * 592,
			Constant.LogFac * 588, Constant.LogFac * 584, Constant.LogFac * 580, Constant.LogFac * 575,
			Constant.LogFac * 570, Constant.LogFac * 567, Constant.LogFac * 563, Constant.LogFac * 559,
			Constant.LogFac * 555, Constant.LogFac * 551, Constant.LogFac * 547, Constant.LogFac * 543,
			Constant.LogFac * 538, Constant.LogFac * 535, Constant.LogFac * 532, Constant.LogFac * 528,
			Constant.LogFac * 524, Constant.LogFac * 520, Constant.LogFac * 516, Constant.LogFac * 513,
			Constant.LogFac * 508, Constant.LogFac * 505, Constant.LogFac * 502, Constant.LogFac * 498,
			Constant.LogFac * 494, Constant.LogFac * 491, Constant.LogFac * 487, Constant.LogFac * 484,
			Constant.LogFac * 480, Constant.LogFac * 477, Constant.LogFac * 474, Constant.LogFac * 470,
			Constant.LogFac * 467, Constant.LogFac * 463, Constant.LogFac * 460, Constant.LogFac * 457,
			Constant.LogFac * 453, Constant.LogFac * 450, Constant.LogFac * 447, Constant.LogFac * 443,
			Constant.LogFac * 440, Constant.LogFac * 437, Constant.LogFac * 434, Constant.LogFac * 431
		};

		public static readonly sbyte[] PanbrelloTable = new sbyte[256]
		{
			  0,   2,   3,   5,   6,   8,   9,  11,  12,  14,  16,  17,  19,  20,  22,  23,
			 24,  26,  27,  29,  30,  32,  33,  34,  36,  37,  38,  39,  41,  42,  43,  44,
			 45,  46,  47,  48,  49,  50,  51,  52,  53,  54,  55,  56,  56,  57,  58,  59,
			 59,  60,  60,  61,  61,  62,  62,  62,  63,  63,  63,  64,  64,  64,  64,  64,
			 64,  64,  64,  64,  64,  64,  63,  63,  63,  62,  62,  62,  61,  61,  60,  60,
			 59,  59,  58,  57,  56,  56,  55,  54,  53,  52,  51,  50,  49,  48,  47,  46,
			 45,  44,  43,  42,  41,  39,  38,  37,  36,  34,  33,  32,  30,  29,  27,  26,
			 24,  23,  22,  20,  19,  17,  16,  14,  12,  11,   9,   8,   6,   5,   3,   2,
			  0, - 2, - 3, - 5, - 6, - 8, - 9, -11, -12, -14, -16, -17, -19, -20, -22, -23,
			-24, -26, -27, -29, -30, -32, -33, -34, -36, -37, -38, -39, -41, -42, -43, -44,
			-45, -46, -47, -48, -49, -50, -51, -52, -53, -54, -55, -56, -56, -57, -58, -59,
			-59, -60, -60, -61, -61, -62, -62, -62, -63, -63, -63, -64, -64, -64, -64, -64,
			-64, -64, -64, -64, -64, -64, -63, -63, -63, -62, -62, -62, -61, -61, -60, -60,
			-59, -59, -58, -57, -56, -56, -55, -54, -53, -52, -51, -50, -49, -48, -47, -46,
			-45, -44, -43, -42, -41, -39, -38, -37, -36, -34, -33, -32, -30, -29, -27, -26,
			-24, -23, -22, -20, -19, -17, -16, -14, -12, -11, - 9, - 8, - 6, - 5, - 3, - 2
		};

		// tempo[0] = 256; tempo[i] = floor(128 /i)
		public static readonly int[] FarTempos = new int[16]
		{
			256, 128, 64, 42, 32, 25, 21, 18, 16, 14, 12, 11, 10, 9, 9, 8
		};
	}
}
