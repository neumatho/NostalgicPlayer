﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal static class PreComp_Lut
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly int16[] Cubic_Spline_Lut0 =
		[
			0, -8, -16, -24, -32, -40, -47, -55,
			-63, -71, -78, -86, -94, -101, -109, -117,
			-124, -132, -139, -146, -154, -161, -169, -176,
			-183, -190, -198, -205, -212, -219, -226, -233,
			-240, -247, -254, -261, -268, -275, -282, -289,
			-295, -302, -309, -316, -322, -329, -336, -342,
			-349, -355, -362, -368, -375, -381, -388, -394,
			-400, -407, -413, -419, -425, -432, -438, -444,
			-450, -456, -462, -468, -474, -480, -486, -492,
			-498, -504, -510, -515, -521, -527, -533, -538,
			-544, -550, -555, -561, -566, -572, -577, -583,
			-588, -594, -599, -604, -610, -615, -620, -626,
			-631, -636, -641, -646, -651, -656, -662, -667,
			-672, -677, -682, -686, -691, -696, -701, -706,
			-711, -715, -720, -725, -730, -734, -739, -744,
			-748, -753, -757, -762, -766, -771, -775, -780,
			-784, -788, -793, -797, -801, -806, -810, -814,
			-818, -822, -826, -831, -835, -839, -843, -847,
			-851, -855, -859, -863, -866, -870, -874, -878,
			-882, -886, -889, -893, -897, -900, -904, -908,
			-911, -915, -918, -922, -925, -929, -932, -936,
			-939, -943, -946, -949, -953, -956, -959, -962,
			-966, -969, -972, -975, -978, -981, -984, -987,
			-991, -994, -997, -999, -1002, -1005, -1008, -1011,
			-1014, -1017, -1020, -1022, -1025, -1028, -1031, -1033,
			-1036, -1039, -1041, -1044, -1047, -1049, -1052, -1054,
			-1057, -1059, -1062, -1064, -1066, -1069, -1071, -1074,
			-1076, -1078, -1080, -1083, -1085, -1087, -1089, -1092,
			-1094, -1096, -1098, -1100, -1102, -1104, -1106, -1108,
			-1110, -1112, -1114, -1116, -1118, -1120, -1122, -1124,
			-1125, -1127, -1129, -1131, -1133, -1134, -1136, -1138,
			-1139, -1141, -1143, -1144, -1146, -1147, -1149, -1150,
			-1152, -1153, -1155, -1156, -1158, -1159, -1161, -1162,
			-1163, -1165, -1166, -1167, -1169, -1170, -1171, -1172,
			-1174, -1175, -1176, -1177, -1178, -1179, -1180, -1181,
			-1182, -1184, -1185, -1186, -1187, -1187, -1188, -1189,
			-1190, -1191, -1192, -1193, -1194, -1195, -1195, -1196,
			-1197, -1198, -1198, -1199, -1200, -1200, -1201, -1202,
			-1202, -1203, -1204, -1204, -1205, -1205, -1206, -1206,
			-1207, -1207, -1208, -1208, -1208, -1209, -1209, -1210,
			-1210, -1210, -1211, -1211, -1211, -1212, -1212, -1212,
			-1212, -1212, -1213, -1213, -1213, -1213, -1213, -1213,
			-1213, -1213, -1214, -1214, -1214, -1214, -1214, -1214,
			-1214, -1214, -1213, -1213, -1213, -1213, -1213, -1213,
			-1213, -1213, -1212, -1212, -1212, -1212, -1211, -1211,
			-1211, -1211, -1210, -1210, -1210, -1209, -1209, -1209,
			-1208, -1208, -1207, -1207, -1207, -1206, -1206, -1205,
			-1205, -1204, -1204, -1203, -1202, -1202, -1201, -1201,
			-1200, -1199, -1199, -1198, -1197, -1197, -1196, -1195,
			-1195, -1194, -1193, -1192, -1192, -1191, -1190, -1189,
			-1188, -1187, -1187, -1186, -1185, -1184, -1183, -1182,
			-1181, -1180, -1179, -1178, -1177, -1176, -1175, -1174,
			-1173, -1172, -1171, -1170, -1169, -1168, -1167, -1166,
			-1165, -1163, -1162, -1161, -1160, -1159, -1158, -1156,
			-1155, -1154, -1153, -1151, -1150, -1149, -1148, -1146,
			-1145, -1144, -1142, -1141, -1140, -1138, -1137, -1135,
			-1134, -1133, -1131, -1130, -1128, -1127, -1125, -1124,
			-1122, -1121, -1119, -1118, -1116, -1115, -1113, -1112,
			-1110, -1109, -1107, -1105, -1104, -1102, -1101, -1099,
			-1097, -1096, -1094, -1092, -1091, -1089, -1087, -1085,
			-1084, -1082, -1080, -1079, -1077, -1075, -1073, -1071,
			-1070, -1068, -1066, -1064, -1062, -1061, -1059, -1057,
			-1055, -1053, -1051, -1049, -1047, -1046, -1044, -1042,
			-1040, -1038, -1036, -1034, -1032, -1030, -1028, -1026,
			-1024, -1022, -1020, -1018, -1016, -1014, -1012, -1010,
			-1008, -1006, -1004, -1002, -999, -997, -995, -993,
			-991, -989, -987, -985, -982, -980, -978, -976,
			-974, -972, -969, -967, -965, -963, -961, -958,
			-956, -954, -952, -950, -947, -945, -943, -941,
			-938, -936, -934, -931, -929, -927, -924, -922,
			-920, -918, -915, -913, -911, -908, -906, -903,
			-901, -899, -896, -894, -892, -889, -887, -884,
			-882, -880, -877, -875, -872, -870, -867, -865,
			-863, -860, -858, -855, -853, -850, -848, -845,
			-843, -840, -838, -835, -833, -830, -828, -825,
			-823, -820, -818, -815, -813, -810, -808, -805,
			-803, -800, -798, -795, -793, -790, -787, -785,
			-782, -780, -777, -775, -772, -769, -767, -764,
			-762, -759, -757, -754, -751, -749, -746, -744,
			-741, -738, -736, -733, -730, -728, -725, -723,
			-720, -717, -715, -712, -709, -707, -704, -702,
			-699, -696, -694, -691, -688, -686, -683, -680,
			-678, -675, -672, -670, -667, -665, -662, -659,
			-657, -654, -651, -649, -646, -643, -641, -638,
			-635, -633, -630, -627, -625, -622, -619, -617,
			-614, -611, -609, -606, -603, -601, -598, -595,
			-593, -590, -587, -585, -582, -579, -577, -574,
			-571, -569, -566, -563, -561, -558, -555, -553,
			-550, -547, -545, -542, -539, -537, -534, -531,
			-529, -526, -523, -521, -518, -516, -513, -510,
			-508, -505, -502, -500, -497, -495, -492, -489,
			-487, -484, -481, -479, -476, -474, -471, -468,
			-466, -463, -461, -458, -455, -453, -450, -448,
			-445, -442, -440, -437, -435, -432, -430, -427,
			-424, -422, -419, -417, -414, -412, -409, -407,
			-404, -402, -399, -397, -394, -392, -389, -387,
			-384, -382, -379, -377, -374, -372, -369, -367,
			-364, -362, -359, -357, -354, -352, -349, -347,
			-345, -342, -340, -337, -335, -332, -330, -328,
			-325, -323, -320, -318, -316, -313, -311, -309,
			-306, -304, -302, -299, -297, -295, -292, -290,
			-288, -285, -283, -281, -278, -276, -274, -272,
			-269, -267, -265, -263, -260, -258, -256, -254,
			-251, -249, -247, -245, -243, -240, -238, -236,
			-234, -232, -230, -228, -225, -223, -221, -219,
			-217, -215, -213, -211, -209, -207, -205, -202,
			-200, -198, -196, -194, -192, -190, -188, -186,
			-184, -182, -180, -178, -176, -175, -173, -171,
			-169, -167, -165, -163, -161, -159, -157, -156,
			-154, -152, -150, -148, -146, -145, -143, -141,
			-139, -137, -136, -134, -132, -130, -129, -127,
			-125, -124, -122, -120, -119, -117, -115, -114,
			-112, -110, -109, -107, -106, -104, -102, -101,
			-99, -98, -96, -95, -93, -92, -90, -89,
			-87, -86, -84, -83, -82, -80, -79, -77,
			-76, -75, -73, -72, -70, -69, -68, -67,
			-65, -64, -63, -61, -60, -59, -58, -57,
			-55, -54, -53, -52, -51, -49, -48, -47,
			-46, -45, -44, -43, -42, -41, -40, -39,
			-38, -37, -36, -35, -34, -33, -32, -31,
			-30, -29, -28, -27, -26, -26, -25, -24,
			-23, -22, -22, -21, -20, -19, -19, -18,
			-17, -16, -16, -15, -14, -14, -13, -13,
			-12, -11, -11, -10, -10, -9, -9, -8,
			-8, -7, -7, -6, -6, -6, -5, -5,
			-4, -4, -4, -3, -3, -3, -2, -2,
			-2, -2, -2, -1, -1, -1, -1, -1,
			0, 0, 0, 0, 0, 0, 0, 0
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly int16[] Cubic_Spline_Lut1 =
		[
			16384, 16384, 16384, 16384, 16384, 16383, 16382, 16381,
			16381, 16381, 16380, 16379, 16379, 16377, 16377, 16376,
			16374, 16373, 16371, 16370, 16369, 16366, 16366, 16364,
			16361, 16360, 16358, 16357, 16354, 16351, 16349, 16347,
			16345, 16342, 16340, 16337, 16335, 16331, 16329, 16326,
			16322, 16320, 16317, 16314, 16309, 16307, 16304, 16299,
			16297, 16293, 16290, 16285, 16282, 16278, 16274, 16269,
			16265, 16262, 16257, 16253, 16247, 16244, 16239, 16235,
			16230, 16225, 16220, 16216, 16211, 16206, 16201, 16196,
			16191, 16185, 16180, 16174, 16169, 16163, 16158, 16151,
			16146, 16140, 16133, 16128, 16122, 16116, 16109, 16104,
			16097, 16092, 16085, 16077, 16071, 16064, 16058, 16052,
			16044, 16038, 16030, 16023, 16015, 16009, 16002, 15995,
			15988, 15980, 15973, 15964, 15957, 15949, 15941, 15934,
			15926, 15918, 15910, 15903, 15894, 15886, 15877, 15870,
			15861, 15853, 15843, 15836, 15827, 15818, 15810, 15801,
			15792, 15783, 15774, 15765, 15756, 15747, 15738, 15729,
			15719, 15709, 15700, 15691, 15681, 15672, 15662, 15652,
			15642, 15633, 15623, 15613, 15602, 15592, 15582, 15572,
			15562, 15552, 15540, 15530, 15520, 15509, 15499, 15489,
			15478, 15467, 15456, 15446, 15433, 15423, 15412, 15401,
			15390, 15379, 15367, 15356, 15345, 15333, 15321, 15310,
			15299, 15287, 15276, 15264, 15252, 15240, 15228, 15216,
			15205, 15192, 15180, 15167, 15155, 15143, 15131, 15118,
			15106, 15094, 15081, 15067, 15056, 15043, 15031, 15017,
			15004, 14992, 14979, 14966, 14953, 14940, 14927, 14913,
			14900, 14887, 14874, 14860, 14846, 14833, 14819, 14806,
			14793, 14778, 14764, 14752, 14737, 14723, 14709, 14696,
			14681, 14668, 14653, 14638, 14625, 14610, 14595, 14582,
			14567, 14553, 14538, 14523, 14509, 14494, 14480, 14465,
			14450, 14435, 14420, 14406, 14391, 14376, 14361, 14346,
			14330, 14316, 14301, 14285, 14270, 14254, 14239, 14223,
			14208, 14192, 14177, 14161, 14146, 14130, 14115, 14099,
			14082, 14067, 14051, 14035, 14019, 14003, 13986, 13971,
			13955, 13939, 13923, 13906, 13890, 13873, 13857, 13840,
			13823, 13808, 13791, 13775, 13758, 13741, 13724, 13707,
			13691, 13673, 13657, 13641, 13623, 13607, 13589, 13572,
			13556, 13538, 13521, 13504, 13486, 13469, 13451, 13435,
			13417, 13399, 13383, 13365, 13347, 13330, 13312, 13294,
			13277, 13258, 13241, 13224, 13205, 13188, 13170, 13152,
			13134, 13116, 13098, 13080, 13062, 13044, 13026, 13008,
			12989, 12971, 12953, 12934, 12916, 12898, 12879, 12860,
			12842, 12823, 12806, 12787, 12768, 12750, 12731, 12712,
			12694, 12675, 12655, 12637, 12618, 12599, 12580, 12562,
			12542, 12524, 12504, 12485, 12466, 12448, 12427, 12408,
			12390, 12370, 12351, 12332, 12312, 12293, 12273, 12254,
			12235, 12215, 12195, 12176, 12157, 12137, 12118, 12097,
			12079, 12059, 12039, 12019, 11998, 11980, 11960, 11940,
			11920, 11900, 11880, 11860, 11839, 11821, 11801, 11780,
			11761, 11741, 11720, 11700, 11680, 11660, 11640, 11619,
			11599, 11578, 11559, 11538, 11518, 11498, 11477, 11457,
			11436, 11415, 11394, 11374, 11354, 11333, 11313, 11292,
			11272, 11251, 11231, 11209, 11189, 11168, 11148, 11127,
			11107, 11084, 11064, 11043, 11023, 11002, 10982, 10959,
			10939, 10918, 10898, 10876, 10856, 10834, 10814, 10792,
			10772, 10750, 10728, 10708, 10687, 10666, 10644, 10623,
			10602, 10581, 10560, 10538, 10517, 10496, 10474, 10453,
			10431, 10410, 10389, 10368, 10346, 10325, 10303, 10283,
			10260, 10239, 10217, 10196, 10175, 10152, 10132, 10110,
			10088, 10068, 10045, 10023, 10002, 9981, 9959, 9936,
			9915, 9893, 9872, 9851, 9829, 9806, 9784, 9763,
			9742, 9720, 9698, 9676, 9653, 9633, 9611, 9589,
			9567, 9545, 9523, 9501, 9479, 9458, 9436, 9414,
			9392, 9370, 9348, 9326, 9304, 9282, 9260, 9238,
			9216, 9194, 9172, 9150, 9128, 9106, 9084, 9062,
			9040, 9018, 8996, 8974, 8951, 8929, 8907, 8885,
			8863, 8841, 8819, 8797, 8775, 8752, 8730, 8708,
			8686, 8664, 8642, 8620, 8597, 8575, 8553, 8531,
			8509, 8487, 8464, 8442, 8420, 8398, 8376, 8353,
			8331, 8309, 8287, 8265, 8242, 8220, 8198, 8176,
			8154, 8131, 8109, 8087, 8065, 8042, 8020, 7998,
			7976, 7954, 7931, 7909, 7887, 7865, 7842, 7820,
			7798, 7776, 7754, 7731, 7709, 7687, 7665, 7643,
			7620, 7598, 7576, 7554, 7531, 7509, 7487, 7465,
			7443, 7421, 7398, 7376, 7354, 7332, 7310, 7288,
			7265, 7243, 7221, 7199, 7177, 7155, 7132, 7110,
			7088, 7066, 7044, 7022, 7000, 6978, 6956, 6934,
			6911, 6889, 6867, 6845, 6823, 6801, 6779, 6757,
			6735, 6713, 6691, 6669, 6647, 6625, 6603, 6581,
			6559, 6537, 6515, 6493, 6472, 6450, 6428, 6406,
			6384, 6362, 6340, 6318, 6297, 6275, 6253, 6231,
			6209, 6188, 6166, 6144, 6122, 6101, 6079, 6057,
			6035, 6014, 5992, 5970, 5949, 5927, 5905, 5884,
			5862, 5841, 5819, 5797, 5776, 5754, 5733, 5711,
			5690, 5668, 5647, 5625, 5604, 5582, 5561, 5540,
			5518, 5497, 5476, 5454, 5433, 5412, 5390, 5369,
			5348, 5327, 5305, 5284, 5263, 5242, 5221, 5199,
			5178, 5157, 5136, 5115, 5094, 5073, 5052, 5031,
			5010, 4989, 4968, 4947, 4926, 4905, 4885, 4864,
			4843, 4822, 4801, 4780, 4760, 4739, 4718, 4698,
			4677, 4656, 4636, 4615, 4595, 4574, 4553, 4533,
			4512, 4492, 4471, 4451, 4431, 4410, 4390, 4370,
			4349, 4329, 4309, 4288, 4268, 4248, 4228, 4208,
			4188, 4167, 4147, 4127, 4107, 4087, 4067, 4047,
			4027, 4007, 3988, 3968, 3948, 3928, 3908, 3889,
			3869, 3849, 3829, 3810, 3790, 3771, 3751, 3732,
			3712, 3693, 3673, 3654, 3634, 3615, 3595, 3576,
			3557, 3538, 3518, 3499, 3480, 3461, 3442, 3423,
			3404, 3385, 3366, 3347, 3328, 3309, 3290, 3271,
			3252, 3233, 3215, 3196, 3177, 3159, 3140, 3121,
			3103, 3084, 3066, 3047, 3029, 3010, 2992, 2974,
			2955, 2937, 2919, 2901, 2882, 2864, 2846, 2828,
			2810, 2792, 2774, 2756, 2738, 2720, 2702, 2685,
			2667, 2649, 2631, 2614, 2596, 2579, 2561, 2543,
			2526, 2509, 2491, 2474, 2456, 2439, 2422, 2405,
			2387, 2370, 2353, 2336, 2319, 2302, 2285, 2268,
			2251, 2234, 2218, 2201, 2184, 2167, 2151, 2134,
			2117, 2101, 2084, 2068, 2052, 2035, 2019, 2003,
			1986, 1970, 1954, 1938, 1922, 1906, 1890, 1874,
			1858, 1842, 1826, 1810, 1794, 1779, 1763, 1747,
			1732, 1716, 1701, 1685, 1670, 1654, 1639, 1624,
			1608, 1593, 1578, 1563, 1548, 1533, 1518, 1503,
			1488, 1473, 1458, 1444, 1429, 1414, 1400, 1385,
			1370, 1356, 1342, 1327, 1313, 1298, 1284, 1270,
			1256, 1242, 1228, 1214, 1200, 1186, 1172, 1158,
			1144, 1131, 1117, 1103, 1090, 1076, 1063, 1049,
			1036, 1022, 1009, 996, 983, 970, 956, 943,
			930, 917, 905, 892, 879, 866, 854, 841,
			828, 816, 803, 791, 778, 766, 754, 742,
			729, 717, 705, 693, 681, 669, 658, 646,
			634, 622, 611, 599, 588, 576, 565, 553,
			542, 531, 520, 508, 497, 486, 475, 464,
			453, 443, 432, 421, 411, 400, 389, 379,
			369, 358, 348, 338, 327, 317, 307, 297,
			287, 277, 268, 258, 248, 238, 229, 219,
			210, 200, 191, 182, 172, 163, 154, 145,
			136, 127, 118, 109, 100, 92, 83, 75,
			66, 58, 49, 41, 32, 24, 16, 8
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly int16[] Cubic_Spline_Lut2 =
		[
			0, 8, 16, 24, 32, 41, 49, 58,
			66, 75, 83, 92, 100, 109, 118, 127,
			136, 145, 154, 163, 172, 182, 191, 200,
			210, 219, 229, 238, 248, 258, 268, 277,
			287, 297, 307, 317, 327, 338, 348, 358,
			369, 379, 389, 400, 411, 421, 432, 443,
			453, 464, 475, 486, 497, 508, 520, 531,
			542, 553, 565, 576, 588, 599, 611, 622,
			634, 646, 658, 669, 681, 693, 705, 717,
			729, 742, 754, 766, 778, 791, 803, 816,
			828, 841, 854, 866, 879, 892, 905, 917,
			930, 943, 956, 970, 983, 996, 1009, 1022,
			1036, 1049, 1063, 1076, 1090, 1103, 1117, 1131,
			1144, 1158, 1172, 1186, 1200, 1214, 1228, 1242,
			1256, 1270, 1284, 1298, 1313, 1327, 1342, 1356,
			1370, 1385, 1400, 1414, 1429, 1444, 1458, 1473,
			1488, 1503, 1518, 1533, 1548, 1563, 1578, 1593,
			1608, 1624, 1639, 1654, 1670, 1685, 1701, 1716,
			1732, 1747, 1763, 1779, 1794, 1810, 1826, 1842,
			1858, 1874, 1890, 1906, 1922, 1938, 1954, 1970,
			1986, 2003, 2019, 2035, 2052, 2068, 2084, 2101,
			2117, 2134, 2151, 2167, 2184, 2201, 2218, 2234,
			2251, 2268, 2285, 2302, 2319, 2336, 2353, 2370,
			2387, 2405, 2422, 2439, 2456, 2474, 2491, 2509,
			2526, 2543, 2561, 2579, 2596, 2614, 2631, 2649,
			2667, 2685, 2702, 2720, 2738, 2756, 2774, 2792,
			2810, 2828, 2846, 2864, 2882, 2901, 2919, 2937,
			2955, 2974, 2992, 3010, 3029, 3047, 3066, 3084,
			3103, 3121, 3140, 3159, 3177, 3196, 3215, 3233,
			3252, 3271, 3290, 3309, 3328, 3347, 3366, 3385,
			3404, 3423, 3442, 3461, 3480, 3499, 3518, 3538,
			3557, 3576, 3595, 3615, 3634, 3654, 3673, 3693,
			3712, 3732, 3751, 3771, 3790, 3810, 3829, 3849,
			3869, 3889, 3908, 3928, 3948, 3968, 3988, 4007,
			4027, 4047, 4067, 4087, 4107, 4127, 4147, 4167,
			4188, 4208, 4228, 4248, 4268, 4288, 4309, 4329,
			4349, 4370, 4390, 4410, 4431, 4451, 4471, 4492,
			4512, 4533, 4553, 4574, 4595, 4615, 4636, 4656,
			4677, 4698, 4718, 4739, 4760, 4780, 4801, 4822,
			4843, 4864, 4885, 4905, 4926, 4947, 4968, 4989,
			5010, 5031, 5052, 5073, 5094, 5115, 5136, 5157,
			5178, 5199, 5221, 5242, 5263, 5284, 5305, 5327,
			5348, 5369, 5390, 5412, 5433, 5454, 5476, 5497,
			5518, 5540, 5561, 5582, 5604, 5625, 5647, 5668,
			5690, 5711, 5733, 5754, 5776, 5797, 5819, 5841,
			5862, 5884, 5905, 5927, 5949, 5970, 5992, 6014,
			6035, 6057, 6079, 6101, 6122, 6144, 6166, 6188,
			6209, 6231, 6253, 6275, 6297, 6318, 6340, 6362,
			6384, 6406, 6428, 6450, 6472, 6493, 6515, 6537,
			6559, 6581, 6603, 6625, 6647, 6669, 6691, 6713,
			6735, 6757, 6779, 6801, 6823, 6845, 6867, 6889,
			6911, 6934, 6956, 6978, 7000, 7022, 7044, 7066,
			7088, 7110, 7132, 7155, 7177, 7199, 7221, 7243,
			7265, 7288, 7310, 7332, 7354, 7376, 7398, 7421,
			7443, 7465, 7487, 7509, 7531, 7554, 7576, 7598,
			7620, 7643, 7665, 7687, 7709, 7731, 7754, 7776,
			7798, 7820, 7842, 7865, 7887, 7909, 7931, 7954,
			7976, 7998, 8020, 8042, 8065, 8087, 8109, 8131,
			8154, 8176, 8198, 8220, 8242, 8265, 8287, 8309,
			8331, 8353, 8376, 8398, 8420, 8442, 8464, 8487,
			8509, 8531, 8553, 8575, 8597, 8620, 8642, 8664,
			8686, 8708, 8730, 8752, 8775, 8797, 8819, 8841,
			8863, 8885, 8907, 8929, 8951, 8974, 8996, 9018,
			9040, 9062, 9084, 9106, 9128, 9150, 9172, 9194,
			9216, 9238, 9260, 9282, 9304, 9326, 9348, 9370,
			9392, 9414, 9436, 9458, 9479, 9501, 9523, 9545,
			9567, 9589, 9611, 9633, 9653, 9676, 9698, 9720,
			9742, 9763, 9784, 9806, 9829, 9851, 9872, 9893,
			9915, 9936, 9959, 9981, 10002, 10023, 10045, 10068,
			10088, 10110, 10132, 10152, 10175, 10196, 10217, 10239,
			10260, 10283, 10303, 10325, 10346, 10368, 10389, 10410,
			10431, 10453, 10474, 10496, 10517, 10538, 10560, 10581,
			10602, 10623, 10644, 10666, 10687, 10708, 10728, 10750,
			10772, 10792, 10814, 10834, 10856, 10876, 10898, 10918,
			10939, 10959, 10982, 11002, 11023, 11043, 11064, 11084,
			11107, 11127, 11148, 11168, 11189, 11209, 11231, 11251,
			11272, 11292, 11313, 11333, 11354, 11374, 11394, 11415,
			11436, 11457, 11477, 11498, 11518, 11538, 11559, 11578,
			11599, 11619, 11640, 11660, 11680, 11700, 11720, 11741,
			11761, 11780, 11801, 11821, 11839, 11860, 11880, 11900,
			11920, 11940, 11960, 11980, 11998, 12019, 12039, 12059,
			12079, 12097, 12118, 12137, 12157, 12176, 12195, 12215,
			12235, 12254, 12273, 12293, 12312, 12332, 12351, 12370,
			12390, 12408, 12427, 12448, 12466, 12485, 12504, 12524,
			12542, 12562, 12580, 12599, 12618, 12637, 12655, 12675,
			12694, 12712, 12731, 12750, 12768, 12787, 12806, 12823,
			12842, 12860, 12879, 12898, 12916, 12934, 12953, 12971,
			12989, 13008, 13026, 13044, 13062, 13080, 13098, 13116,
			13134, 13152, 13170, 13188, 13205, 13224, 13241, 13258,
			13277, 13294, 13312, 13330, 13347, 13365, 13383, 13399,
			13417, 13435, 13451, 13469, 13486, 13504, 13521, 13538,
			13556, 13572, 13589, 13607, 13623, 13641, 13657, 13673,
			13691, 13707, 13724, 13741, 13758, 13775, 13791, 13808,
			13823, 13840, 13857, 13873, 13890, 13906, 13923, 13939,
			13955, 13971, 13986, 14003, 14019, 14035, 14051, 14067,
			14082, 14099, 14115, 14130, 14146, 14161, 14177, 14192,
			14208, 14223, 14239, 14254, 14270, 14285, 14301, 14316,
			14330, 14346, 14361, 14376, 14391, 14406, 14420, 14435,
			14450, 14465, 14480, 14494, 14509, 14523, 14538, 14553,
			14567, 14582, 14595, 14610, 14625, 14638, 14653, 14668,
			14681, 14696, 14709, 14723, 14737, 14752, 14764, 14778,
			14793, 14806, 14819, 14833, 14846, 14860, 14874, 14887,
			14900, 14913, 14927, 14940, 14953, 14966, 14979, 14992,
			15004, 15017, 15031, 15043, 15056, 15067, 15081, 15094,
			15106, 15118, 15131, 15143, 15155, 15167, 15180, 15192,
			15205, 15216, 15228, 15240, 15252, 15264, 15276, 15287,
			15299, 15310, 15321, 15333, 15345, 15356, 15367, 15379,
			15390, 15401, 15412, 15423, 15433, 15446, 15456, 15467,
			15478, 15489, 15499, 15509, 15520, 15530, 15540, 15552,
			15562, 15572, 15582, 15592, 15602, 15613, 15623, 15633,
			15642, 15652, 15662, 15672, 15681, 15691, 15700, 15709,
			15719, 15729, 15738, 15747, 15756, 15765, 15774, 15783,
			15792, 15801, 15810, 15818, 15827, 15836, 15843, 15853,
			15861, 15870, 15877, 15886, 15894, 15903, 15910, 15918,
			15926, 15934, 15941, 15949, 15957, 15964, 15973, 15980,
			15988, 15995, 16002, 16009, 16015, 16023, 16030, 16038,
			16044, 16052, 16058, 16064, 16071, 16077, 16085, 16092,
			16097, 16104, 16109, 16116, 16122, 16128, 16133, 16140,
			16146, 16151, 16158, 16163, 16169, 16174, 16180, 16185,
			16191, 16196, 16201, 16206, 16211, 16216, 16220, 16225,
			16230, 16235, 16239, 16244, 16247, 16253, 16257, 16262,
			16265, 16269, 16274, 16278, 16282, 16285, 16290, 16293,
			16297, 16299, 16304, 16307, 16309, 16314, 16317, 16320,
			16322, 16326, 16329, 16331, 16335, 16337, 16340, 16342,
			16345, 16347, 16349, 16351, 16354, 16357, 16358, 16360,
			16361, 16364, 16366, 16366, 16369, 16370, 16371, 16373,
			16374, 16376, 16377, 16377, 16379, 16379, 16380, 16381,
			16381, 16381, 16382, 16383, 16384, 16384, 16384, 16384
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly int16[] Cubic_Spline_Lut3 =
		[
			0, 0, 0, 0, 0, 0, 0, 0,
			0, -1, -1, -1, -1, -1, -2, -2,
			-2, -2, -2, -3, -3, -3, -4, -4,
			-4, -5, -5, -6, -6, -6, -7, -7,
			-8, -8, -9, -9, -10, -10, -11, -11,
			-12, -13, -13, -14, -14, -15, -16, -16,
			-17, -18, -19, -19, -20, -21, -22, -22,
			-23, -24, -25, -26, -26, -27, -28, -29,
			-30, -31, -32, -33, -34, -35, -36, -37,
			-38, -39, -40, -41, -42, -43, -44, -45,
			-46, -47, -48, -49, -51, -52, -53, -54,
			-55, -57, -58, -59, -60, -61, -63, -64,
			-65, -67, -68, -69, -70, -72, -73, -75,
			-76, -77, -79, -80, -82, -83, -84, -86,
			-87, -89, -90, -92, -93, -95, -96, -98,
			-99, -101, -102, -104, -106, -107, -109, -110,
			-112, -114, -115, -117, -119, -120, -122, -124,
			-125, -127, -129, -130, -132, -134, -136, -137,
			-139, -141, -143, -145, -146, -148, -150, -152,
			-154, -156, -157, -159, -161, -163, -165, -167,
			-169, -171, -173, -175, -176, -178, -180, -182,
			-184, -186, -188, -190, -192, -194, -196, -198,
			-200, -202, -205, -207, -209, -211, -213, -215,
			-217, -219, -221, -223, -225, -228, -230, -232,
			-234, -236, -238, -240, -243, -245, -247, -249,
			-251, -254, -256, -258, -260, -263, -265, -267,
			-269, -272, -274, -276, -278, -281, -283, -285,
			-288, -290, -292, -295, -297, -299, -302, -304,
			-306, -309, -311, -313, -316, -318, -320, -323,
			-325, -328, -330, -332, -335, -337, -340, -342,
			-345, -347, -349, -352, -354, -357, -359, -362,
			-364, -367, -369, -372, -374, -377, -379, -382,
			-384, -387, -389, -392, -394, -397, -399, -402,
			-404, -407, -409, -412, -414, -417, -419, -422,
			-424, -427, -430, -432, -435, -437, -440, -442,
			-445, -448, -450, -453, -455, -458, -461, -463,
			-466, -468, -471, -474, -476, -479, -481, -484,
			-487, -489, -492, -495, -497, -500, -502, -505,
			-508, -510, -513, -516, -518, -521, -523, -526,
			-529, -531, -534, -537, -539, -542, -545, -547,
			-550, -553, -555, -558, -561, -563, -566, -569,
			-571, -574, -577, -579, -582, -585, -587, -590,
			-593, -595, -598, -601, -603, -606, -609, -611,
			-614, -617, -619, -622, -625, -627, -630, -633,
			-635, -638, -641, -643, -646, -649, -651, -654,
			-657, -659, -662, -665, -667, -670, -672, -675,
			-678, -680, -683, -686, -688, -691, -694, -696,
			-699, -702, -704, -707, -709, -712, -715, -717,
			-720, -723, -725, -728, -730, -733, -736, -738,
			-741, -744, -746, -749, -751, -754, -757, -759,
			-762, -764, -767, -769, -772, -775, -777, -780,
			-782, -785, -787, -790, -793, -795, -798, -800,
			-803, -805, -808, -810, -813, -815, -818, -820,
			-823, -825, -828, -830, -833, -835, -838, -840,
			-843, -845, -848, -850, -853, -855, -858, -860,
			-863, -865, -867, -870, -872, -875, -877, -880,
			-882, -884, -887, -889, -892, -894, -896, -899,
			-901, -903, -906, -908, -911, -913, -915, -918,
			-920, -922, -924, -927, -929, -931, -934, -936,
			-938, -941, -943, -945, -947, -950, -952, -954,
			-956, -958, -961, -963, -965, -967, -969, -972,
			-974, -976, -978, -980, -982, -985, -987, -989,
			-991, -993, -995, -997, -999, -1002, -1004, -1006,
			-1008, -1010, -1012, -1014, -1016, -1018, -1020, -1022,
			-1024, -1026, -1028, -1030, -1032, -1034, -1036, -1038,
			-1040, -1042, -1044, -1046, -1047, -1049, -1051, -1053,
			-1055, -1057, -1059, -1061, -1062, -1064, -1066, -1068,
			-1070, -1071, -1073, -1075, -1077, -1079, -1080, -1082,
			-1084, -1085, -1087, -1089, -1091, -1092, -1094, -1096,
			-1097, -1099, -1101, -1102, -1104, -1105, -1107, -1109,
			-1110, -1112, -1113, -1115, -1116, -1118, -1119, -1121,
			-1122, -1124, -1125, -1127, -1128, -1130, -1131, -1133,
			-1134, -1135, -1137, -1138, -1140, -1141, -1142, -1144,
			-1145, -1146, -1148, -1149, -1150, -1151, -1153, -1154,
			-1155, -1156, -1158, -1159, -1160, -1161, -1162, -1163,
			-1165, -1166, -1167, -1168, -1169, -1170, -1171, -1172,
			-1173, -1174, -1175, -1176, -1177, -1178, -1179, -1180,
			-1181, -1182, -1183, -1184, -1185, -1186, -1187, -1187,
			-1188, -1189, -1190, -1191, -1192, -1192, -1193, -1194,
			-1195, -1195, -1196, -1197, -1197, -1198, -1199, -1199,
			-1200, -1201, -1201, -1202, -1202, -1203, -1204, -1204,
			-1205, -1205, -1206, -1206, -1207, -1207, -1207, -1208,
			-1208, -1209, -1209, -1209, -1210, -1210, -1210, -1211,
			-1211, -1211, -1211, -1212, -1212, -1212, -1212, -1213,
			-1213, -1213, -1213, -1213, -1213, -1213, -1213, -1214,
			-1214, -1214, -1214, -1214, -1214, -1214, -1214, -1213,
			-1213, -1213, -1213, -1213, -1213, -1213, -1213, -1212,
			-1212, -1212, -1212, -1212, -1211, -1211, -1211, -1210,
			-1210, -1210, -1209, -1209, -1208, -1208, -1208, -1207,
			-1207, -1206, -1206, -1205, -1205, -1204, -1204, -1203,
			-1202, -1202, -1201, -1200, -1200, -1199, -1198, -1198,
			-1197, -1196, -1195, -1195, -1194, -1193, -1192, -1191,
			-1190, -1189, -1188, -1187, -1187, -1186, -1185, -1184,
			-1182, -1181, -1180, -1179, -1178, -1177, -1176, -1175,
			-1174, -1172, -1171, -1170, -1169, -1167, -1166, -1165,
			-1163, -1162, -1161, -1159, -1158, -1156, -1155, -1153,
			-1152, -1150, -1149, -1147, -1146, -1144, -1143, -1141,
			-1139, -1138, -1136, -1134, -1133, -1131, -1129, -1127,
			-1125, -1124, -1122, -1120, -1118, -1116, -1114, -1112,
			-1110, -1108, -1106, -1104, -1102, -1100, -1098, -1096,
			-1094, -1092, -1089, -1087, -1085, -1083, -1080, -1078,
			-1076, -1074, -1071, -1069, -1066, -1064, -1062, -1059,
			-1057, -1054, -1052, -1049, -1047, -1044, -1041, -1039,
			-1036, -1033, -1031, -1028, -1025, -1022, -1020, -1017,
			-1014, -1011, -1008, -1005, -1002, -999, -997, -994,
			-991, -987, -984, -981, -978, -975, -972, -969,
			-966, -962, -959, -956, -953, -949, -946, -943,
			-939, -936, -932, -929, -925, -922, -918, -915,
			-911, -908, -904, -900, -897, -893, -889, -886,
			-882, -878, -874, -870, -866, -863, -859, -855,
			-851, -847, -843, -839, -835, -831, -826, -822,
			-818, -814, -810, -806, -801, -797, -793, -788,
			-784, -780, -775, -771, -766, -762, -757, -753,
			-748, -744, -739, -734, -730, -725, -720, -715,
			-711, -706, -701, -696, -691, -686, -682, -677,
			-672, -667, -662, -656, -651, -646, -641, -636,
			-631, -626, -620, -615, -610, -604, -599, -594,
			-588, -583, -577, -572, -566, -561, -555, -550,
			-544, -538, -533, -527, -521, -515, -510, -504,
			-498, -492, -486, -480, -474, -468, -462, -456,
			-450, -444, -438, -432, -425, -419, -413, -407,
			-400, -394, -388, -381, -375, -368, -362, -355,
			-349, -342, -336, -329, -322, -316, -309, -302,
			-295, -289, -282, -275, -268, -261, -254, -247,
			-240, -233, -226, -219, -212, -205, -198, -190,
			-183, -176, -169, -161, -154, -146, -139, -132,
			-124, -117, -109, -101, -94, -86, -78, -71,
			-63, -55, -47, -40, -32, -24, -16, -8
		];
	}
}
