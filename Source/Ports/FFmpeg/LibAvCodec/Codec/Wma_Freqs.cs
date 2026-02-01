/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Wma_Freqs
	{
		public static readonly uint16_t[] ff_Wma_Critical_Freqs =
		[
			  100,   200,  300,  400,  510,  630,   770,   920,
			 1080,  1270, 1480, 1720, 2000, 2320,  2700,  3150,
			 3700,  4400, 5300, 6400, 7700, 9500, 12000, 15500,
			24500
		];
	}
}
