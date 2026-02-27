/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Factories
{
	/// <summary>
	/// Use this to create an instance of MixerConfiguration
	/// </summary>
	public interface IMixerConfigurationFactory
	{
		/// <summary>
		/// Create a new instance based on the current settings
		/// </summary>
		MixerConfiguration Create();
	}
}
