/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holds information where a single sample is stored in
	/// a converted stream
	/// </summary>
	public class ConvertSamplePosition
	{
		/// <summary>
		/// Where the sample starts in the virtual stream
		/// </summary>
		internal long StartPosition { get; set; }

		/// <summary>
		/// Number of bytes the sample takes up in both the virtual
		/// stream and the sample stream
		/// </summary>
		internal long Length { get; set; }

		/// <summary>
		/// Where the sample data starts in the sample stream
		/// </summary>
		internal long SampleStreamStartPosition { get; set; }

		/// <summary>
		/// Where the sample ends in the virtual stream (exclusive)
		/// </summary>
		internal long EndPosition => StartPosition + Length;
	}
}
