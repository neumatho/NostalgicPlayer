/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Utility.Interfaces
{
	/// <summary>
	/// Holds different system paths
	/// </summary>
	public interface IPlatformPath
	{
		/// <summary>
		/// Return the path to where the settings should be stored
		/// </summary>
		string SettingsPath { get; }
	}
}
