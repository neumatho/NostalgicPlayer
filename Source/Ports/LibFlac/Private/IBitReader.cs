/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Private
{
	/// <summary>
	/// Common interface for all BitReader implementations
	/// </summary>
	internal interface IBitReader
	{
		/// <summary>
		/// This is far the most heavily used reader call. It ain't pretty
		/// but it's fast
		/// </summary>
		Flac__bool Read_Rice_Signed_Block(int[] vals, uint32_t offset, uint32_t nVals, uint32_t parameter);
	}
}
