/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Do not derive from this interface, but use IAgentGuiSettings instead
	/// in GuiKit if you have some settings in your agent
	/// </summary>
	public interface IAgentSettings
	{
		/// <summary>
		/// Return the NostalgicPlayer_Current_Version constant defined above
		/// </summary>
		int NostalgicPlayerVersion { get; }

		/// <summary>
		/// Returns an unique ID for this setting agent
		/// </summary>
		Guid SettingAgentId { get; }
	}
}
