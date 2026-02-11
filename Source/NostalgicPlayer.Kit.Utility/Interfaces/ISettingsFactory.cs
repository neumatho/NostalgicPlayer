/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Utility.Interfaces
{
	/// <summary>
	/// Factory for creating ISettings instances
	/// </summary>
	public interface ISettingsFactory
	{
		/// <summary>
		/// Creates a new ISettings instance
		/// </summary>
		ISettings CreateSettings();
	}
}
