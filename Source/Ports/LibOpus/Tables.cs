﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus
{
	/// <summary>
	/// Different tables
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly byte[] Trim_Icdf = [ 126, 124, 119, 109, 87, 41, 19, 9, 4, 2, 0 ];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly byte[] Spread_Icdf = [ 25, 23, 2, 0 ];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly byte[] Tapset_Icdf = [ 2, 1, 0 ];



		/********************************************************************/
		/// <summary>
		/// Mean energy in each band quantized in Q4 and converted back to
		/// float
		/// </summary>
		/********************************************************************/
		public static readonly opus_val16[] EMeans =
		[
			6.437500f, 6.250000f, 5.750000f, 5.312500f, 5.062500f,
			4.812500f, 4.500000f, 4.375000f, 4.875000f, 4.687500f,
			4.562500f, 4.437500f, 4.875000f, 4.625000f, 4.312500f,
			4.500000f, 4.375000f, 4.625000f, 4.750000f, 4.437500f,
			3.750000f, 3.750000f, 3.750000f, 3.750000f, 3.750000f
		];



		/********************************************************************/
		/// <summary>
		/// TF change table. Positive values mean better frequency resolution
		/// (longer effective window), whereas negative values mean better
		/// time resolution (shorter effective window). The second index is
		/// computed as:
		/// 4*isTransient + 2*tf_select + per_band_flag
		/// </summary>
		/********************************************************************/
		public static readonly sbyte[][] Tf_Select_Table =
		[
			// isTransient=0   isTransient=1
			[ 0, -1, 0, -1,    0, -1, 0, -1 ],	// 2.5 ms
			[ 0, -1, 0, -2,    1,  0, 1, -1 ],	// 5 ms
			[ 0, -2, 0, -3,    2,  0, 1, -1 ],	// 10 ms
			[ 0, -2, 0, -3,    3,  0, 1, -1 ],	// 20 ms
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_int16[] Eband5ms =
		[
			// 0  200 400 600 800  1k 1.2 1.4 1.6  2k 2.4 2.8 3.2  4k 4.8 5.6 6.8  8k 9.6 12k 15.6
			   0,  1,  2,  3,  4,  5,  6,  7,  8, 10, 12, 14, 16, 20, 24, 28, 34, 40, 48, 60, 78, 100
		];



		/********************************************************************/
		/// <summary>
		/// Alternate tuning (partially derived from Vorbis)
		///
		/// Bit allocation table in units of 1/32 bit/sample (0.1875 dB SNR)
		/// </summary>
		/********************************************************************/
		public static readonly byte[] Band_Allocation =
		[
			// 0   200  400  600  800   1k  1.2  1.4  1.6   2k  2.4  2.8  3.2   4k  4.8  5.6  6.8   8k  9.6  12k  15.6
			   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
			  90,  80,  75,  69,  63,  56,  49,  40,  34,  29,  20,  18,  10,   0,   0,   0,   0,   0,   0,   0,   0,
			 110, 100,  90,  84,  78,  71,  65,  58,  51,  45,  39,  32,  26,  20,  12,   0,   0,   0,   0,   0,   0,
			 118, 110, 103,  93,  86,  80,  75,  70,  65,  59,  53,  47,  40,  31,  23,  15,   4,   0,   0,   0,   0,
			 126, 119, 112, 104,  95,  89,  83,  78,  72,  66,  60,  54,  47,  39,  32,  25,  17,  12,   1,   0,   0,
			 134, 127, 120, 114, 103,  97,  91,  85,  78,  72,  66,  60,  54,  47,  41,  35,  29,  23,  16,  10,   1,
			 144, 137, 130, 124, 113, 107, 101,  95,  88,  82,  76,  70,  64,  57,  51,  45,  39,  33,  26,  15,   1,
			 152, 145, 138, 132, 123, 117, 111, 105,  98,  92,  86,  80,  74,  67,  61,  55,  49,  43,  36,  20,   1,
			 162, 155, 148, 142, 133, 127, 121, 115, 108, 102,  96,  90,  84,  77,  71,  65,  59,  53,  46,  30,   1,
			 172, 165, 158, 152, 143, 137, 131, 125, 118, 112, 106, 100,  94,  87,  81,  75,  69,  63,  56,  45,  20,
			 200, 200, 200, 200, 200, 200, 200, 200, 198, 193, 188, 183, 178, 173, 168, 163, 158, 153, 148, 129, 104,
		];
	}
}
