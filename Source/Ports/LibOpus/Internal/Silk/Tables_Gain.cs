/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Tables_Gain
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[][] Silk_Gain_iCDF =
		[
			[
				224,    112,     44,     15,      3,      2,      1,      0
			],
			[
				254,    237,    192,    132,     70,     23,      4,      0
			],
			[
				255,    252,    226,    155,     61,     11,      2,      0
			]
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly opus_uint8[] Silk_Delta_Gain_iCDF =
		[
			250,    245,    234,    203,     71,     50,     42,     38,
			 35,     33,     31,     29,     28,     27,     26,     25,
			 24,     23,     22,     21,     20,     19,     18,     17,
			 16,     15,     14,     13,     12,     11,     10,      9,
			  8,      7,      6,      5,      4,      3,      2,      1,
			  0
		];
	}
}
