﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigitalSoundStudio
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
			// Tuning 0, normal
			{
				1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016,  960,  906,
				 856,  808,  762,  720,  678,  640,  604,  570,  538,  508,  480,  453,
				 428,  404,  381,  360,  339,  320,  302,  285,  269,  254,  240,  226,
				 214,  202,  190,  180,  170,  160,  151,  143,  135,  127,  120,  113
			},

			// Tuning 1
			{
				1700, 1604, 1514, 1430, 1348, 1274, 1202, 1134, 1070, 1010,  954,  900,
				 850,  802,  757,  715,  674,  637,  601,  567,  535,  505,  477,  450,
				 425,  401,  379,  357,  337,  318,  300,  284,  268,  253,  239,  225,
				 213,  201,  189,  179,  169,  159,  150,  142,  134,  126,  119,  113
			},

			// Tuning 2
			{
				1688, 1592, 1504, 1418, 1340, 1264, 1194, 1126, 1064, 1004,  948,  894,
				 844,  796,  752,  709,  670,  632,  597,  563,  532,  502,  474,  447,
				 422,  398,  376,  355,  335,  316,  298,  282,  266,  251,  237,  224,
				 211,  199,  188,  177,  167,  158,  149,  141,  133,  125,  118,  112
			},

			// Tuning 3
			{
				1676, 1582, 1492, 1408, 1330, 1256, 1184, 1118, 1056,  996,  940,  888,
				 838,  791,  746,  704,  665,  628,  592,  559,  528,  498,  470,  444,
				 419,  395,  373,  352,  332,  314,  296,  280,  264,  249,  235,  222,
				 209,  198,  187,  176,  166,  157,  148,  140,  132,  125,  118,  111
			},

			// Tuning 4
			{
				1664, 1570, 1482, 1398, 1320, 1246, 1176, 1110, 1048,  990,  934,  882,
				 832,  785,  741,  699,  660,  623,  588,  555,  524,  495,  467,  441,
				 416,  392,  370,  350,  330,  312,  294,  278,  262,  247,  233,  220,
				 208,  196,  185,  175,  165,  156,  147,  139,  131,  124,  117,  110
			},

			// Tuning 5
			{
				1652, 1558, 1472, 1388, 1310, 1238, 1168, 1102, 1040,  982,  926,  874,
				 826,  779,  736,  694,  655,  619,  584,  551,  520,  491,  463,  437,
				 413,  390,  368,  347,  328,  309,  292,  276,  260,  245,  232,  219,
				 206,  195,  184,  174,  164,  155,  146,  138,  130,  123,  116,  109
			},

			// Tuning 6
			{
				1640, 1548, 1460, 1378, 1302, 1228, 1160, 1094, 1032,  974,  920,  868,
				 820,  774,  730,  689,  651,  614,  580,  547,  516,  487,  460,  434,
				 410,  387,  365,  345,  325,  307,  290,  274,  258,  244,  230,  217,
				 205,  193,  183,  172,  163,  154,  145,  137,  129,  122,  115,  109
			},

			// Tuning 7
			{
				1628, 1536, 1450, 1368, 1292, 1220, 1150, 1086, 1026,  968,  914,  862,
				 814,  768,  725,  684,  646,  610,  575,  543,  513,  484,  457,  431,
				 407,  384,  363,  342,  323,  305,  288,  272,  256,  242,  228,  216,
				 204,  192,  181,  171,  161,  152,  144,  136,  128,  121,  114,  108
			},

			// Tuning -8
			{
				1814, 1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016,  960,
				 907,  856,  808,  762,  720,  678,  640,  604,  570,  538,  508,  480,
				 453,  428,  404,  381,  360,  339,  320,  302,  285,  269,  254,  240,
				 226,  214,  202,  190,  180,  170,  160,  151,  143,  135,  127,  120
			},

			// Tuning -7
			{
				1800, 1700, 1604, 1514, 1430, 1350, 1272, 1202, 1134, 1070, 1010,  954,
				 900,  850,  802,  757,  715,  675,  636,  601,  567,  535,  505,  477,
				 450,  425,  401,  379,  357,  337,  318,  300,  284,  268,  253,  238,
				 225,  212,  200,  189,  179,  169,  159,  150,  142,  134,  126,  119
			},

			// Tuning -6
			{
				1788, 1688, 1592, 1504, 1418, 1340, 1264, 1194, 1126, 1064, 1004,  948,
				 894,  844,  796,  752,  709,  670,  632,  597,  563,  532,  502,  474,
				 447,  422,  398,  376,  355,  335,  316,  298,  282,  266,  251,  237,
				 223,  211,  199,  188,  177,  167,  158,  149,  141,  133,  125,  118
			},

			// Tuning -5
			{
				1774, 1676, 1582, 1492, 1408, 1330, 1256, 1184, 1118, 1056,  996,  940,
				 887,  838,  791,  746,  704,  665,  628,  592,  559,  528,  498,  470,
				 444,  419,  395,  373,  352,  332,  314,  296,  280,  264,  249,  235,
				 222,  209,  198,  187,  176,  166,  157,  148,  140,  132,  125,  118
			},

			// Tuning -4
			{
				1762, 1664, 1570, 1482, 1398, 1320, 1246, 1176, 1110, 1048,  988,  934,
				 881,  832,  785,  741,  699,  660,  623,  588,  555,  524,  494,  467,
				 441,  416,  392,  370,  350,  330,  312,  294,  278,  262,  247,  233,
				 220,  208,  196,  185,  175,  165,  156,  147,  139,  131,  123,  117
			},

			// Tuning -3
			{
				1750, 1652, 1558, 1472, 1388, 1310, 1238, 1168, 1102, 1040,  982,  926,
				 875,  826,  779,  736,  694,  655,  619,  584,  551,  520,  491,  463,
				 437,  413,  390,  368,  347,  328,  309,  292,  276,  260,  245,  232,
				 219,  206,  195,  184,  174,  164,  155,  146,  138,  130,  123,  116
			},

			// Tuning -2
			{
				1736, 1640, 1548, 1460, 1378, 1302, 1228, 1160, 1094, 1032,  974,  920,
				 868,  820,  774,  730,  689,  651,  614,  580,  547,  516,  487,  460,
				 434,  410,  387,  365,  345,  325,  307,  290,  274,  258,  244,  230,
				 217,  205,  193,  183,  172,  163,  154,  145,  137,  129,  122,  115
			},

			// Tuning -1
			{
				1724, 1628, 1536, 1450, 1368, 1292, 1220, 1150, 1086, 1026,  968,  914,
				 862,  814,  768,  725,  684,  646,  610,  575,  543,  513,  484,  457,
				 431,  407,  384,  363,  342,  323,  305,  288,  272,  256,  242,  228,
				 216,  203,  192,  181,  171,  161,  152,  144,  136,  128,  121,  114
			},
		};



		/********************************************************************/
		/// <summary>
		/// Period limits
		/// </summary>
		/********************************************************************/
		public static readonly ushort[,] PeriodLimits =
		{
			{ 1712, 113 },
			{ 1700, 113 },
			{ 1688, 112 },
			{ 1676, 111 },
			{ 1664, 110 },
			{ 1652, 109 },
			{ 1640, 109 },
			{ 1628, 108 },
			{ 1814, 120 },
			{ 1800, 119 },
			{ 1788, 118 },
			{ 1774, 118 },
			{ 1762, 117 },
			{ 1750, 116 },
			{ 1736, 115 },
			{ 1724, 114 }
		};
	}
}