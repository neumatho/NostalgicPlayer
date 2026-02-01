/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvIamfSubmixLayoutType
	{
		/// <summary>
		/// The layout follows the loudspeaker sound system convention of ITU-2051-3.
		/// AVIAMFSubmixLayout.sound_system must be set
		/// </summary>
		Loudspeakers = 2,

		/// <summary>
		/// The layout is binaural.
		///
		/// Note: AVIAMFSubmixLayout.sound_system may be set to
		/// AV_CHANNEL_LAYOUT_BINAURAL to simplify API usage, but it's not mandatory
		/// </summary>
		Binaural = 3
	}
}
