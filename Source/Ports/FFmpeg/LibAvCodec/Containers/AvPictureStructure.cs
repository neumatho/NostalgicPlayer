/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvPictureStructure
	{
		/// <summary>
		/// Unknown
		/// </summary>
		Unknown,

		/// <summary>
		/// Coded as top field
		/// </summary>
		Top_Field,

		/// <summary>
		/// Coded as bottom field
		/// </summary>
		Bottom_Field,

		/// <summary>
		/// Coded as frame
		/// </summary>
		Frame
	}
}
