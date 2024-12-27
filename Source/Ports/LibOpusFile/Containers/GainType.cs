/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpusFile.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum GainType
	{
		/// <summary>
		/// Gain offset type that indicates that the provided offset is
		/// relative to the header gain.
		/// This is the default
		/// </summary>
		Header = 0,

		/// <summary>
		/// Gain offset type that indicates that the provided offset is
		/// relative to the R128_ALBUM_GAIN value (if any), in addition to
		/// the header gain
		/// </summary>
		Album = 3007,

		/// <summary>
		/// Gain offset type that indicates that the provided offset is
		/// relative to the R128_TRACK_GAIN value (if any), in addition to
		/// the header gain
		/// </summary>
		Track = 3008,

		/// <summary>
		/// Gain offset type that indicates that the provided offset should
		/// be used as the gain directly, without applying any the header or
		/// track gains
		/// </summary>
		Absolute = 3009
	}
}
