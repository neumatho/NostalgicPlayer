﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Tables_Pulses_Per_Block
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint8[][] Silk_Pulses_Per_Block_iCDF =
		[
			[
				125,     51,     26,     18,     15,     12,     11,     10,
				  9,      8,      7,      6,      5,      4,      3,      2,
				  1,      0
			],
			[
				198,    105,     45,     22,     15,     12,     11,     10,
				  9,      8,      7,      6,      5,      4,      3,      2,
				  1,      0
			],
			[
				213,    162,    116,     83,     59,     43,     32,     24,
				 18,     15,     12,      9,      7,      6,      5,      3,
				  2,      0
			],
			[
				239,    187,    116,     59,     28,     16,     11,     10,
				  9,      8,      7,      6,      5,      4,      3,      2,
				  1,      0
			],
			[
				250,    229,    188,    135,     86,     51,     30,     19,
				 13,     10,      8,      6,      5,      4,      3,      2,
				  1,      0
			],
			[
				249,    235,    213,    185,    156,    128,    103,     83,
				 66,     53,     42,     33,     26,     21,     17,     13,
				 10,      0
			],
			[
				254,    249,    235,    206,    164,    118,     77,     46,
				 27,     16,     10,      7,      5,      4,      3,      2,
				  1,      0
			],
			[
				255,    253,    249,    239,    220,    191,    156,    119,
				 85,     57,     37,     23,     15,     10,      6,      4,
				  2,      0
			],
			[
				255,    253,    251,    246,    237,    223,    203,    179,
				152,    124,     98,     75,     55,     40,     29,     21,
				 15,      0
			],
			[
				255,    254,    253,    247,    220,    162,    106,     67,
				 42,     28,     18,     12,      9,      6,      4,      3,
				  2,      0
			]
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint8[] Silk_Shell_Code_Table0 =
		[
			128,      0,    214,     42,      0,    235,    128,     21,
			  0,    244,    184,     72,     11,      0,    248,    214,
			128,     42,      7,      0,    248,    225,    170,     80,
			 25,      5,      0,    251,    236,    198,    126,     54,
			 18,      3,      0,    250,    238,    211,    159,     82,
			 35,     15,      5,      0,    250,    231,    203,    168,
			128,     88,     53,     25,      6,      0,    252,    238,
			216,    185,    148,    108,     71,     40,     18,      4,
			  0,    253,    243,    225,    199,    166,    128,     90,
			 57,     31,     13,      3,      0,    254,    246,    233,
			212,    183,    147,    109,     73,     44,     23,     10,
			  2,      0,    255,    250,    240,    223,    198,    166,
			128,     90,     58,     33,     16,      6,      1,      0,
			255,    251,    244,    231,    210,    181,    146,    110,
			 75,     46,     25,     12,      5,      1,      0,    255,
			253,    248,    238,    221,    196,    164,    128,     92,
			 60,     35,     18,      8,      3,      1,      0,    255,
			253,    249,    242,    229,    208,    180,    146,    110,
			 76,     48,     27,     14,      7,      3,      1,      0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint8[] Silk_Shell_Code_Table1 =
		[
			129,      0,    207,     50,      0,    236,    129,     20,
			  0,    245,    185,     72,     10,      0,    249,    213,
			129,     42,      6,      0,    250,    226,    169,     87,
			 27,      4,      0,    251,    233,    194,    130,     62,
			 20,      4,      0,    250,    236,    207,    160,     99,
			 47,     17,      3,      0,    255,    240,    217,    182,
			131,     81,     41,     11,      1,      0,    255,    254,
			233,    201,    159,    107,     61,     20,      2,      1,
			  0,    255,    249,    233,    206,    170,    128,     86,
			 50,     23,      7,      1,      0,    255,    250,    238,
			217,    186,    148,    108,     70,     39,     18,      6,
			  1,      0,    255,    252,    243,    226,    200,    166,
			128,     90,     56,     30,     13,      4,      1,      0,
			255,    252,    245,    231,    209,    180,    146,    110,
			 76,     47,     25,     11,      4,      1,      0,    255,
			253,    248,    237,    219,    194,    163,    128,     93,
			 62,     37,     19,      8,      3,      1,      0,    255,
			254,    250,    241,    226,    205,    177,    145,    111,
			 79,     51,     30,     15,      6,      2,      1,      0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint8[] Silk_Shell_Code_Table2 =
		[
			129,      0,    203,     54,      0,    234,    129,     23,
			  0,    245,    184,     73,     10,      0,    250,    215,
			129,     41,      5,      0,    252,    232,    173,     86,
			 24,      3,      0,    253,    240,    200,    129,     56,
			 15,      2,      0,    253,    244,    217,    164,     94,
			 38,     10,      1,      0,    253,    245,    226,    189,
			132,     71,     27,      7,      1,      0,    253,    246,
			231,    203,    159,    105,     56,     23,      6,      1,
			  0,    255,    248,    235,    213,    179,    133,     85,
			 47,     19,      5,      1,      0,    255,    254,    243,
			221,    194,    159,    117,     70,     37,     12,      2,
			  1,      0,    255,    254,    248,    234,    208,    171,
			128,     85,     48,     22,      8,      2,      1,      0,
			255,    254,    250,    240,    220,    189,    149,    107,
			 67,     36,     16,      6,      2,      1,      0,    255,
			254,    251,    243,    227,    201,    166,    128,     90,
			 55,     29,     13,      5,      2,      1,      0,    255,
			254,    252,    246,    234,    213,    183,    147,    109,
			 73,     43,     22,     10,      4,      2,      1,      0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint8[] Silk_Shell_Code_Table3 =
		[
			130,      0,    200,     58,      0,    231,    130,     26,
			  0,    244,    184,     76,     12,      0,    249,    214,
			130,     43,      6,      0,    252,    232,    173,     87,
			 24,      3,      0,    253,    241,    203,    131,     56,
			 14,      2,      0,    254,    246,    221,    167,     94,
			 35,      8,      1,      0,    254,    249,    232,    193,
			130,     65,     23,      5,      1,      0,    255,    251,
			239,    211,    162,     99,     45,     15,      4,      1,
			  0,    255,    251,    243,    223,    186,    131,     74,
			 33,     11,      3,      1,      0,    255,    252,    245,
			230,    202,    158,    105,     57,     24,      8,      2,
			  1,      0,    255,    253,    247,    235,    214,    179,
			132,     84,     44,     19,      7,      2,      1,      0,
			255,    254,    250,    240,    223,    196,    159,    112,
			 69,     36,     15,      6,      2,      1,      0,    255,
			254,    253,    245,    231,    209,    176,    136,     93,
			 55,     27,     11,      3,      2,      1,      0,    255,
			254,    253,    252,    239,    221,    194,    158,    117,
			 76,     42,     18,      4,      3,      2,      1,      0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint8[][] Silk_Rate_Levels_iCDF =
		[
			[
				241,    190,    178,    132,     87,     74,     41,     14,
				  0
			],
			[
				223,    193,    157,    140,    106,     57,     39,     18,
				  0
			]
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint8[] Silk_Shell_Code_Table_Offsets =
		[
			  0,      0,      2,      5,      9,     14,     20,     27,
			 35,     44,     54,     65,     77,     90,    104,    119,
			135
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint8[] Silk_Sign_iCDF =
		[
			254,     49,     67,     77,     82,     93,     99,
			198,     11,     18,     24,     31,     36,     45,
			255,     46,     66,     78,     87,     94,    104,
			208,     14,     21,     32,     42,     51,     66,
			255,     94,    104,    109,    112,    115,    118,
			248,     53,     69,     80,     88,     95,    102
		];
	}
}
