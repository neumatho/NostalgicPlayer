/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Location of chroma samples.
	///
	/// Illustration showing the location of the first (top left) chroma sample of the
	/// image, the left shows only luma, the right
	/// shows the location of the chroma sample, the 2 could be imagined to overlay
	/// each other but are drawn separately due to limitations of ASCII
	///
	///                 1st 2nd       1st 2nd horizontal luma sample positions
	///                  v   v         v   v
	///                  ______        ______
	/// 1st luma line > |X   X ...    |3 4 X ...     X are luma samples,
	///                 |             |1 2           1-6 are possible chroma positions
	/// 2nd luma line > |X   X ...    |5 6 X ...     0 is undefined/unknown position
	/// </summary>
	public enum AvChromaLocation
	{
		/// <summary>
		/// 
		/// </summary>
		Unspecified = 0,

		/// <summary>
		/// MPEG-2/4 4:2:0, H.264 default for 4:2:0
		/// </summary>
		Left = 1,

		/// <summary>
		/// MPEG-1 4:2:0, JPEG 4:2:0, H.263 4:2:0
		/// </summary>
		Center = 2,

		/// <summary>
		/// ITU-R 601, SMPTE 274M 296M S314M(DV 4:1:1), mpeg2 4:2:2
		/// </summary>
		TopLeft = 3,

		/// <summary>
		/// 
		/// </summary>
		Top = 4,

		/// <summary>
		/// 
		/// </summary>
		BottomLeft = 5,

		/// <summary>
		/// 
		/// </summary>
		Bottom = 6,

		/// <summary>
		/// Not part of ABI
		/// </summary>
		Nb
	}
}
