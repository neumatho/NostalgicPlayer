/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// AVPicture types, pixel formats and basic image planes manipulation
	/// </summary>
	public enum AvPictureType
	{
		/// <summary>
		/// Undefined
		/// </summary>
		None = 0,

		/// <summary>
		/// Intra
		/// </summary>
		I,

		/// <summary>
		/// Predicted
		/// </summary>
		P,

		/// <summary>
		/// Bi-dir predicted
		/// </summary>
		B,

		/// <summary>
		/// S(GMC)-VOP MPEG-4
		/// </summary>
		S,

		/// <summary>
		/// Switching Intra
		/// </summary>
		SI,

		/// <summary>
		/// Switching Predicted
		/// </summary>
		SP,

		/// <summary>
		/// BI type
		/// </summary>
		BI
	}
}
