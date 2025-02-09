﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	// Although derived separately, the pulse vector coding scheme is equivalent to
	// a Pyramid Vector Quantizer \cite{Fis86}.
	// Some additional notes about an early version appear at
	// https://people.xiph.org/~tterribe/notes/cwrs.html, but the codebook ordering
	// and the definitions of some terms have evolved since that was written.
	//
	// The conversion from a pulse vector to an integer index (encoding) and back
	// (decoding) is governed by two related functions, V(N,K) and U(N,K).
	//
	// V(N,K) = the number of combinations, with replacement, of N items, taken K
	// at a time, when a sign bit is added to each item taken at least once (i.e.,
	// the number of N-dimensional unit pulse vectors with K pulses).
	// One way to compute this is via
	// V(N,K) = K>0 ? sum(k=1...K,2**k*choose(N,k)*choose(K-1,k-1)) : 1,
	// where choose() is the binomial function.
	// A table of values for N<10 and K<10 looks like:
	// V[10][10] = {
	//    {1,  0,   0,    0,    0,     0,     0,      0,      0,       0},
	//    {1,  2,   2,    2,    2,     2,     2,      2,      2,       2},
	//    {1,  4,   8,   12,   16,    20,    24,     28,     32,      36},
	//    {1,  6,  18,   38,   66,   102,   146,    198,    258,     326},
	//    {1,  8,  32,   88,  192,   360,   608,    952,   1408,    1992},
	//    {1, 10,  50,  170,  450,  1002,  1970,   3530,   5890,    9290},
	//    {1, 12,  72,  292,  912,  2364,  5336,  10836,  20256,   35436},
	//    {1, 14,  98,  462, 1666,  4942, 12642,  28814,  59906,  115598},
	//    {1, 16, 128,  688, 2816,  9424, 27008,  68464, 157184,  332688},
	//    {1, 18, 162,  978, 4482, 16722, 53154, 148626, 374274,  864146}
	// };
	//
	// U(N,K) = the number of such combinations wherein N-1 objects are taken at
	// most K-1 at a time.
	// This is given by
	// U(N,K) = sum(k=0...K-1,V(N-1,k))
	//        = K>0 ? (V(N-1,K-1) + V(N,K-1))/2 : 0.
	// The latter expression also makes clear that U(N,K) is half the number of such
	// combinations wherein the first object is taken at least once.
	// Although it may not be clear from either of these definitions, U(N,K) is the
	// natural function to work with when enumerating the pulse vector codebooks,
	// not V(N,K).
	// U(N,K) is not well-defined for N=0, but with the extension
	// U(0,K) = K>0 ? 0 : 1,
	// the function becomes symmetric: U(N,K) = U(K,N), with a similar table:
	// U[10][10] = {
	//    {1, 0,  0,   0,    0,    0,     0,     0,      0,      0},
	//    {0, 1,  1,   1,    1,    1,     1,     1,      1,      1},
	//    {0, 1,  3,   5,    7,    9,    11,    13,     15,     17},
	//    {0, 1,  5,  13,   25,   41,    61,    85,    113,    145},
	//    {0, 1,  7,  25,   63,  129,   231,   377,    575,    833},
	//    {0, 1,  9,  41,  129,  321,   681,  1289,   2241,   3649},
	//    {0, 1, 11,  61,  231,  681,  1683,  3653,   7183,  13073},
	//    {0, 1, 13,  85,  377, 1289,  3653,  8989,  19825,  40081},
	//    {0, 1, 15, 113,  575, 2241,  7183, 19825,  48639, 108545},
	//    {0, 1, 17, 145,  833, 3649, 13073, 40081, 108545, 265729}
	// };
	//
	// With this extension, V(N,K) may be written in terms of U(N,K):
	// V(N,K) = U(N,K) + U(N,K+1)
	// for all N>=0, K>=0.
	// Thus U(N,K+1) represents the number of combinations where the first element
	// is positive or zero, and U(N,K) represents the number of combinations where
	// it is negative.
	//
	// With a large enough table of U(N,K) values, we could write O(N) encoding
	// and O(min(N*log(K),N+K)) decoding routines, but such a table would be
	// prohibitively large for small embedded devices (K may be as large as 32767
	// for small N, and N may be as large as 200).
	//
	// Both functions obey the same recurrence relation:
	// V(N,K) = V(N-1,K) + V(N,K-1) + V(N-1,K-1),
	// U(N,K) = U(N-1,K) + U(N,K-1) + U(N-1,K-1),
	// for all N>0, K>0, with different initial conditions at N=0 or K=0.
	// This allows us to construct a row of one of the tables above given the
	// previous row or the next row.
	//
	// Thus we can derive O(NK) encoding and decoding routines with O(K) memory
	// using only addition and subtraction.
	//
	// When encoding, we build up from the U(2,K) row and work our way forwards.
	// When decoding, we need to start at the U(N,K) row and work our way backwards,
	// which requires a means of computing U(N,K).
	// U(N,K) may be computed from two previous values with the same N:
	// U(N,K) = ((2*N-1)*U(N,K-1) - U(N,K-2))/(K-1) + U(N,K-2)
	// for all N>1, and since U(N,K) is symmetric, a similar relation holds for two
	// previous values with the same K:
	// U(N,K>1) = ((2*K-1)*U(N-1,K) - U(N-2,K))/(N-1) + U(N-2,K)
	// for all K>1.
	// This allows us to construct an arbitrary row of the U(N,K) table by starting
	// with the first two values, which are constants.
	// This saves roughly 2/3 the work in our O(NK) decoding routine, but costs O(K)
	// multiplications.
	// Similar relations can be derived for V(N,K), but are not used here.
	//
	// For N>0 and K>0, U(N,K) and V(N,K) take on the form of an (N-1)-degree
	// polynomial for fixed N.
	// The first few are
	// U(1,K) = 1,
	// U(2,K) = 2*K-1,
	// U(3,K) = (2*K-2)*K+1,
	// U(4,K) = (((4*K-6)*K+8)*K-3)/3,
	// U(5,K) = ((((2*K-4)*K+10)*K-8)*K+3)/3,
	// and
	// V(1,K) = 2,
	// V(2,K) = 4*K,
	// V(3,K) = 4*K*K+2,
	// V(4,K) = 8*(K*K+2)*K/3,
	// V(5,K) = ((4*K*K+20)*K*K+6)/3,
	// for all K>0.
	// This allows us to derive O(N) encoding and O(N*log(K)) decoding routines for
	// small N (and indeed decoding is also O(N) for N<3).
	//
	// @ARTICLE{Fis86,
	// author="Thomas R. Fischer",
	// title="A Pyramid Vector Quantizer",
	// journal="IEEE Transactions on Information Theory",
	// volume="IT-32",
	// number=4,
	// pages="568--583",
	// month=Jul,
	// year=1986
	// }
	internal static class Cwrs
	{
		#region Tables
		/// <summary>
		/// For each V(N,K) supported, we will access element U(min(N,K+1),max(N,K+1)).
		/// Thus, the number of entries in row I is the larger of the maximum number of
		/// pulses we will ever allocate for a given N=I (K=128, or however many fit in
		/// 32 bits, whichever is smaller), plus one, and the maximum N for which
		/// K=I-1 pulses fit in 32 bits.
		/// The largest band size in an Opus Custom mode is 208.
		/// Otherwise, we can limit things to the set of N which can be achieved by
		/// splitting a band from a standard Opus mode: 176, 144, 96, 88, 72, 64, 48,
		/// 44, 36, 32, 24, 22, 18, 16, 8, 4, 2)
		/// </summary>
		private static readonly opus_uint32[] celt_Pvq_U_Data =
		[
			// N=0, K=0...176:
			1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

			// N=1, K=1...176:
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,

			// N=2, K=2...176:
			3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37, 39, 41,
			43, 45, 47, 49, 51, 53, 55, 57, 59, 61, 63, 65, 67, 69, 71, 73, 75, 77, 79,
			81, 83, 85, 87, 89, 91, 93, 95, 97, 99, 101, 103, 105, 107, 109, 111, 113,
			115, 117, 119, 121, 123, 125, 127, 129, 131, 133, 135, 137, 139, 141, 143,
			145, 147, 149, 151, 153, 155, 157, 159, 161, 163, 165, 167, 169, 171, 173,
			175, 177, 179, 181, 183, 185, 187, 189, 191, 193, 195, 197, 199, 201, 203,
			205, 207, 209, 211, 213, 215, 217, 219, 221, 223, 225, 227, 229, 231, 233,
			235, 237, 239, 241, 243, 245, 247, 249, 251, 253, 255, 257, 259, 261, 263,
			265, 267, 269, 271, 273, 275, 277, 279, 281, 283, 285, 287, 289, 291, 293,
			295, 297, 299, 301, 303, 305, 307, 309, 311, 313, 315, 317, 319, 321, 323,
			325, 327, 329, 331, 333, 335, 337, 339, 341, 343, 345, 347, 349, 351,

			// N=3, K=3...176:
			13, 25, 41, 61, 85, 113, 145, 181, 221, 265, 313, 365, 421, 481, 545, 613,
			685, 761, 841, 925, 1013, 1105, 1201, 1301, 1405, 1513, 1625, 1741, 1861,
			1985, 2113, 2245, 2381, 2521, 2665, 2813, 2965, 3121, 3281, 3445, 3613, 3785,
			3961, 4141, 4325, 4513, 4705, 4901, 5101, 5305, 5513, 5725, 5941, 6161, 6385,
			6613, 6845, 7081, 7321, 7565, 7813, 8065, 8321, 8581, 8845, 9113, 9385, 9661,
			9941, 10225, 10513, 10805, 11101, 11401, 11705, 12013, 12325, 12641, 12961,
			13285, 13613, 13945, 14281, 14621, 14965, 15313, 15665, 16021, 16381, 16745,
			17113, 17485, 17861, 18241, 18625, 19013, 19405, 19801, 20201, 20605, 21013,
			21425, 21841, 22261, 22685, 23113, 23545, 23981, 24421, 24865, 25313, 25765,
			26221, 26681, 27145, 27613, 28085, 28561, 29041, 29525, 30013, 30505, 31001,
			31501, 32005, 32513, 33025, 33541, 34061, 34585, 35113, 35645, 36181, 36721,
			37265, 37813, 38365, 38921, 39481, 40045, 40613, 41185, 41761, 42341, 42925,
			43513, 44105, 44701, 45301, 45905, 46513, 47125, 47741, 48361, 48985, 49613,
			50245, 50881, 51521, 52165, 52813, 53465, 54121, 54781, 55445, 56113, 56785,
			57461, 58141, 58825, 59513, 60205, 60901, 61601,

			// N=4, K=4...176:
			63, 129, 231, 377, 575, 833, 1159, 1561, 2047, 2625, 3303, 4089, 4991, 6017,
			7175, 8473, 9919, 11521, 13287, 15225, 17343, 19649, 22151, 24857, 27775,
			30913, 34279, 37881, 41727, 45825, 50183, 54809, 59711, 64897, 70375, 76153,
			82239, 88641, 95367, 102425, 109823, 117569, 125671, 134137, 142975, 152193,
			161799, 171801, 182207, 193025, 204263, 215929, 228031, 240577, 253575,
			267033, 280959, 295361, 310247, 325625, 341503, 357889, 374791, 392217,
			410175, 428673, 447719, 467321, 487487, 508225, 529543, 551449, 573951,
			597057, 620775, 645113, 670079, 695681, 721927, 748825, 776383, 804609,
			833511, 863097, 893375, 924353, 956039, 988441, 1021567, 1055425, 1090023,
			1125369, 1161471, 1198337, 1235975, 1274393, 1313599, 1353601, 1394407,
			1436025, 1478463, 1521729, 1565831, 1610777, 1656575, 1703233, 1750759,
			1799161, 1848447, 1898625, 1949703, 2001689, 2054591, 2108417, 2163175,
			2218873, 2275519, 2333121, 2391687, 2451225, 2511743, 2573249, 2635751,
			2699257, 2763775, 2829313, 2895879, 2963481, 3032127, 3101825, 3172583,
			3244409, 3317311, 3391297, 3466375, 3542553, 3619839, 3698241, 3777767,
			3858425, 3940223, 4023169, 4107271, 4192537, 4278975, 4366593, 4455399,
			4545401, 4636607, 4729025, 4822663, 4917529, 5013631, 5110977, 5209575,
			5309433, 5410559, 5512961, 5616647, 5721625, 5827903, 5935489, 6044391,
			6154617, 6266175, 6379073, 6493319, 6608921, 6725887, 6844225, 6963943,
			7085049, 7207551,

			// N=5, K=5...176:
			321, 681, 1289, 2241, 3649, 5641, 8361, 11969, 16641, 22569, 29961, 39041,
			50049, 63241, 78889, 97281, 118721, 143529, 172041, 204609, 241601, 283401,
			330409, 383041, 441729, 506921, 579081, 658689, 746241, 842249, 947241,
			1061761, 1186369, 1321641, 1468169, 1626561, 1797441, 1981449, 2179241,
			2391489, 2618881, 2862121, 3121929, 3399041, 3694209, 4008201, 4341801,
			4695809, 5071041, 5468329, 5888521, 6332481, 6801089, 7295241, 7815849,
			8363841, 8940161, 9545769, 10181641, 10848769, 11548161, 12280841, 13047849,
			13850241, 14689089, 15565481, 16480521, 17435329, 18431041, 19468809,
			20549801, 21675201, 22846209, 24064041, 25329929, 26645121, 28010881,
			29428489, 30899241, 32424449, 34005441, 35643561, 37340169, 39096641,
			40914369, 42794761, 44739241, 46749249, 48826241, 50971689, 53187081,
			55473921, 57833729, 60268041, 62778409, 65366401, 68033601, 70781609,
			73612041, 76526529, 79526721, 82614281, 85790889, 89058241, 92418049,
			95872041, 99421961, 103069569, 106816641, 110664969, 114616361, 118672641,
			122835649, 127107241, 131489289, 135983681, 140592321, 145317129, 150160041,
			155123009, 160208001, 165417001, 170752009, 176215041, 181808129, 187533321,
			193392681, 199388289, 205522241, 211796649, 218213641, 224775361, 231483969,
			238341641, 245350569, 252512961, 259831041, 267307049, 274943241, 282741889,
			290705281, 298835721, 307135529, 315607041, 324252609, 333074601, 342075401,
			351257409, 360623041, 370174729, 379914921, 389846081, 399970689, 410291241,
			420810249, 431530241, 442453761, 453583369, 464921641, 476471169, 488234561,
			500214441, 512413449, 524834241, 537479489, 550351881, 563454121, 576788929,
			590359041, 604167209, 618216201, 632508801,

			// N=6, K=6...96:
			1683, 3653, 7183, 13073, 22363, 36365, 56695, 85305, 124515, 177045, 246047,
			335137, 448427, 590557, 766727, 982729, 1244979, 1560549, 1937199, 2383409,
			2908411, 3522221, 4235671, 5060441, 6009091, 7095093, 8332863, 9737793,
			11326283, 13115773, 15124775, 17372905, 19880915, 22670725, 25765455,
			29189457, 32968347, 37129037, 41699767, 46710137, 52191139, 58175189,
			64696159, 71789409, 79491819, 87841821, 96879431, 106646281, 117185651,
			128542501, 140763503, 153897073, 167993403, 183104493, 199284183, 216588185,
			235074115, 254801525, 275831935, 298228865, 322057867, 347386557, 374284647,
			402823977, 433078547, 465124549, 499040399, 534906769, 572806619, 612825229,
			655050231, 699571641, 746481891, 795875861, 847850911, 902506913, 959946283,
			1020274013, 1083597703, 1150027593, 1219676595, 1292660325, 1369097135,
			1449108145, 1532817275, 1620351277, 1711839767, 1807415257, 1907213187,
			2011371957, 2120032959,

			// N=7, K=7...54
			8989, 19825, 40081, 75517, 134245, 227305, 369305, 579125, 880685, 1303777,
			1884961, 2668525, 3707509, 5064793, 6814249, 9041957, 11847485, 15345233,
			19665841, 24957661, 31388293, 39146185, 48442297, 59511829, 72616013,
			88043969, 106114625, 127178701, 151620757, 179861305, 212358985, 249612805,
			292164445, 340600625, 395555537, 457713341, 527810725, 606639529, 695049433,
			793950709, 904317037, 1027188385, 1163673953, 1314955181, 1482288821,
			1667010073, 1870535785, 2094367717,

			// N=8, K=8...37
			48639, 108545, 224143, 433905, 795455, 1392065, 2340495, 3800305, 5984767,
			9173505, 13726991, 20103025, 28875327, 40754369, 56610575, 77500017,
			104692735, 139703809, 184327311, 240673265, 311207743, 398796225, 506750351,
			638878193, 799538175, 993696769, 1226990095, 1505789553, 1837271615,
			2229491905U,

			// N=9, K=9...28:
			265729, 598417, 1256465, 2485825, 4673345, 8405905, 14546705, 24331777,
			39490049, 62390545, 96220561, 145198913, 214828609, 312193553, 446304145,
			628496897, 872893441, 1196924561, 1621925137, 2173806145U,

			// N=10, K=10...24:
			1462563, 3317445, 7059735, 14218905, 27298155, 50250765, 89129247, 152951073,
			254831667, 413442773, 654862247, 1014889769, 1541911931, 2300409629U,
			3375210671U,

			// N=11, K=11...19:
			8097453, 18474633, 39753273, 81270333, 158819253, 298199265, 540279585,
			948062325, 1616336765,

			// N=12, K=12...18:
			45046719, 103274625, 224298231, 464387817, 921406335, 1759885185,
			3248227095U,

			// N=13, K=13...16:
			251595969, 579168825, 1267854873, 2653649025U,

			// N=14, K=14:
			1409933619
		];

		private static readonly CPointer<opus_uint32>[] celt_Pvq_U_Row =
		[
			new CPointer<opus_uint32>(celt_Pvq_U_Data, 0),  new CPointer<opus_uint32>(celt_Pvq_U_Data, 176), new CPointer<opus_uint32>(celt_Pvq_U_Data, 351),
			new CPointer<opus_uint32>(celt_Pvq_U_Data, 525), new CPointer<opus_uint32>(celt_Pvq_U_Data, 698), new CPointer<opus_uint32>(celt_Pvq_U_Data, 870),
			new CPointer<opus_uint32>(celt_Pvq_U_Data, 1041), new CPointer<opus_uint32>(celt_Pvq_U_Data, 1131), new CPointer<opus_uint32>(celt_Pvq_U_Data, 1178),
			new CPointer<opus_uint32>(celt_Pvq_U_Data, 1207), new CPointer<opus_uint32>(celt_Pvq_U_Data, 1226), new CPointer<opus_uint32>(celt_Pvq_U_Data, 1240),
			new CPointer<opus_uint32>(celt_Pvq_U_Data, 1248), new CPointer<opus_uint32>(celt_Pvq_U_Data, 1254), new CPointer<opus_uint32>(celt_Pvq_U_Data, 1257)
		];
		#endregion

		/********************************************************************/
		/// <summary>
		/// U(N,K) = U(K,N) := N>0?K>0?U(N-1,K)+U(N,K-1)+U(N-1,K-1):0:K>0?1:0
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static opus_uint32 Celt_Pvq_U(c_int _n, c_int _k)
		{
			return celt_Pvq_U_Row[Arch.IMIN(_n, _k)][Arch.IMAX(_n, _k)];
		}



		/********************************************************************/
		/// <summary>
		/// V(N,K) := U(N,K)+U(N,K+1) = the number of PVQ codewords for a
		/// band of size N with K pulses allocated to it
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_uint32 Celt_Pvq_V(c_int _n, c_int _k)
		{
			return Celt_Pvq_U(_n, _k) + Celt_Pvq_U(_n, _k + 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint32 Icwrs(c_int _n, CPointer<c_int> _y)
		{
			c_int j = _n - 1;
			opus_uint32 i = _y[j] < 0 ? 1U : 0U;
			c_int k = Math.Abs(_y[j]);

			do
			{
				j--;
				i += Celt_Pvq_U(_n - j, k);
				k += Math.Abs(_y[j]);

				if (_y[j] < 0)
					i += Celt_Pvq_U(_n - j, k + 1);
			}
			while (j > 0);

			return i;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Encode_Pulses(CPointer<c_int> _y, c_int _n, c_int _k, Ec_Enc _enc)
		{
			EntEnc.Ec_Enc_UInt(_enc, Icwrs(_n, _y), Celt_Pvq_V(_n, _k));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_val32 Cwrsi(c_int _n, c_int _k, opus_uint32 _i, CPointer<c_int> _y)
		{
			opus_uint32 p;
			c_int s;
			c_int k0;
			opus_int16 val;
			opus_val32 yy = 0;

			while (_n > 2)
			{
				opus_uint32 q;

				// Lots of pulses case:
				if (_k >= _n)
				{
					CPointer<opus_uint32> row = celt_Pvq_U_Row[_n];

					// Are the pulses in this dimension negative?
					p = row[_k + 1];
					s = -(_i >= p ? 1 : 0);
					_i -= (opus_uint32)(p & s);

					// Count how many pulses were placed in this dimension
					k0 = _k;
					q = row[_n];

					if (q > _i)
					{
						_k = _n;

						do
						{
							p = celt_Pvq_U_Row[--_k][_n];
						}
						while (p > _i);
					}
					else
					{
						for (p = row[_k]; p > _i; p = row[_k])
							_k--;
					}

					_i -= p;
					val = (opus_int16)((k0 - _k + s) ^ s);
					_y[0, 1] = val;
					yy = Arch.MAC16_16(yy, val, val);
				}
				// Lots of dimensions case:
				else
				{
					// Are there any pulses in this dimension at all?
					p = celt_Pvq_U_Row[_k][_n];
					q = celt_Pvq_U_Row[_k + 1][_n];

					if ((p <= _i) && (_i < q))
					{
						_i -= p;
						_y[0, 1] = 0;
					}
					else
					{
						// Are the pulses in this dimension negative?
						s = -(_i >= q ? 1 : 0);
						_i -= (opus_uint32)(q & s);

						// Count how many pulses were placed in this dimension
						k0 = _k;

						do
						{
							p = celt_Pvq_U_Row[--_k][_n];
						}
						while (p > _i);

						_i -= p;
						val = (opus_int16)((k0 - _k + s) ^ s);
						_y[0, 1] = val;
						yy = Arch.MAC16_16(yy, val, val);
					}
				}

				_n--;
			}

			// n == 2
			p = (c_uint)(2 * _k + 1);
			s = -(_i >= p ? 1 : 0);
			_i -= (opus_uint32)(p & s);
			k0 = _k;
			_k = (c_int)((_i + 1) >> 1);

			if (_k != 0)
				_i -= (opus_uint32)(2 * _k - 1);

			val = (opus_int16)((k0 - _k + s) ^ s);
			_y[0, 1] = val;
			yy = Arch.MAC16_16(yy, val, val);

			// n == 1
			s = -(c_int)(_i);
			val = (opus_int16)((_k + s) ^ s);
			_y[0, 1] = val;
			yy = Arch.MAC16_16(yy, val, val);

			return yy;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_val32 Decode_Pulses(CPointer<c_int> _y, c_int _n, c_int _k, Ec_Dec _dec)
		{
			return Cwrsi(_n, _k, EntDec.Ec_Dec_UInt(_dec, Celt_Pvq_V(_n, _k)), _y);
		}
	}
}
