/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type can receive mixer channel enable/disable state changes.
	/// This is separate from the player's channel mute state and represents
	/// the user's mixer settings.
	/// </summary>
	public interface IMixerChannelChangeVisualAgent : IVisualAgent
	{
		/// <summary>
		/// Called when the mixer channel enabled states change.
		/// This is called when the user toggles channels in the mixer window.
		/// </summary>
		/// <param name="channelsEnabled">Array indicating which channels are enabled in the mixer</param>
		void MixerChannelsEnabledChanged(bool[] channelsEnabled);
	}
}
