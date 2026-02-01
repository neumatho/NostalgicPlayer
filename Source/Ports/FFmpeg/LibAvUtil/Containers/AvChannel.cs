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
	public enum AvChannel
	{
		/// <summary>
		/// Invalid channel index
		/// </summary>
		None = -1,

		/// <summary>
		/// 
		/// </summary>
		Front_Left,

		/// <summary>
		/// 
		/// </summary>
		Front_Right,

		/// <summary>
		/// 
		/// </summary>
		Front_Center,

		/// <summary>
		/// 
		/// </summary>
		Low_Frequency,

		/// <summary>
		/// 
		/// </summary>
		Back_Left,

		/// <summary>
		/// 
		/// </summary>
		Back_Right,

		/// <summary>
		/// 
		/// </summary>
		Front_Left_Of_Center,

		/// <summary>
		/// 
		/// </summary>
		Front_Right_Of_Center,

		/// <summary>
		/// 
		/// </summary>
		Back_Center,

		/// <summary>
		/// 
		/// </summary>
		Side_Left,

		/// <summary>
		/// 
		/// </summary>
		Side_Right,

		/// <summary>
		/// 
		/// </summary>
		Top_Center,

		/// <summary>
		/// 
		/// </summary>
		Top_Front_Left,

		/// <summary>
		/// 
		/// </summary>
		Top_Front_Center,

		/// <summary>
		/// 
		/// </summary>
		Top_Front_Right,

		/// <summary>
		/// 
		/// </summary>
		Top_Back_Left,

		/// <summary>
		/// 
		/// </summary>
		Top_Back_Center,

		/// <summary>
		/// 
		/// </summary>
		Top_Back_Right,

		/// <summary>
		/// 
		/// </summary>
		Stereo_Left = 29,

		/// <summary>
		/// 
		/// </summary>
		Stereo_Right,

		/// <summary>
		/// 
		/// </summary>
		Wide_Left,

		/// <summary>
		/// 
		/// </summary>
		Wide_Right,

		/// <summary>
		/// 
		/// </summary>
		Surround_Direct_Left,

		/// <summary>
		/// 
		/// </summary>
		Surround_Direct_Right,

		/// <summary>
		/// 
		/// </summary>
		Low_Frequency_2,

		/// <summary>
		/// 
		/// </summary>
		Top_Side_Left,

		/// <summary>
		/// 
		/// </summary>
		Top_Side_Right,

		/// <summary>
		/// 
		/// </summary>
		Bottom_Front_Center,

		/// <summary>
		/// 
		/// </summary>
		Bottom_Front_Left,

		/// <summary>
		/// 
		/// </summary>
		Bottom_Front_Right,

		/// <summary>
		/// +90 degrees, Lss, SiL
		/// </summary>
		Side_Surround_Left,

		/// <summary>
		/// -90 degrees, Rss, SiR
		/// </summary>
		Side_Surround_Right,

		/// <summary>
		/// +110 degrees, Lvs, TpLS
		/// </summary>
		Top_Surround_Left,

		/// <summary>
		/// -110 degrees, Rvs, TpRS
		/// </summary>
		Top_Surround_Right,

		/// <summary>
		/// 
		/// </summary>
		Binaural_Left = 61,

		/// <summary>
		/// 
		/// </summary>
		Binaural_Right,

		/// <summary>
		/// Channel is empty can be safely skipped
		/// </summary>
		Unused = 0x200,

		/// <summary>
		/// Channel contains data, but its position is unknown
		/// </summary>
		Unknown = 0x300,

		/// <summary>
		/// Range of channels between AV_CHAN_AMBISONIC_BASE and
		/// AV_CHAN_AMBISONIC_END represent Ambisonic components using the ACN system.
		///
		/// Given a channel id `‹i›` between AV_CHAN_AMBISONIC_BASE and
		/// AV_CHAN_AMBISONIC_END (inclusive), the ACN index of the channel `‹n›` is
		/// `‹n› = ‹i› - AV_CHAN_AMBISONIC_BASE`.
		///
		/// Note these values are only used for AV_CHANNEL_ORDER_CUSTOM channel
		/// orderings, the AV_CHANNEL_ORDER_AMBISONIC ordering orders the channels
		/// implicitly by their position in the stream
		/// </summary>
		Ambisonic_Base = 0x400,

		/// <summary>
		/// Leave space for 1024 ids, which correspond to maximum order-32 harmonics,
		/// which should be enough for the foreseeable use cases
		/// </summary>
		Ambisonic_End = 0x7ff
	}
}
