/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer
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
		public static readonly ushort[] Periods =
		[
			                                                               32256, 30464, 28672,
			27136, 25600, 24192, 22784, 21504, 20352, 19200, 18048, 17689, 16128, 15232, 14336,
			13568, 12800, 12096, 11392, 10752, 10176,  9600,  9024,  8512,  8064,  7616,  7168,
			 6784,  6400,  6048,  5696,  5376,  5088,  4800,  4512,  4256,  4032,  3808,  3584,
			 3392,  3200,  3024,  2848,  2688,  2544,  2400,  2256,  2128,  2016,  1904,  1792,
			 1696,  1600,  1512,  1424,  1344,  1272,  1200,  1128,  1064,  1008,   952,   896,
			  848,   800,   756,   712,   672,   636,   600,   564,   532,   504,   476,   448,
			  424,   400,   378,   356,   336,   318,   300,   282,   266,   252,   238,   224,
			  212,   200,   189,   178,   168,   159,   150,   141,   133
		];



		/********************************************************************/
		/// <summary>
		/// Frequency ratio table
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] FrequencyRatio =
		[
			1, 1,
			107, 101,
			55, 49,
			44, 37,
			160, 127,
			4, 3,
			140, 99,
			218, 146,
			100, 63,
			111, 66,
			98, 55,
			168, 89,
			2, 1
		];



		/********************************************************************/
		/// <summary>
		/// Empty sample
		/// </summary>
		/********************************************************************/
		public static readonly Waveform EmptySample = new Waveform
		{
			Offset = -1,
			Data = new sbyte[256]
		};
	}
}
