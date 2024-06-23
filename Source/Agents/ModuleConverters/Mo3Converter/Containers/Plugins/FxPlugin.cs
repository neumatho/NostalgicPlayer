/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Plugins
{
	/// <summary>
	/// Mixer effect plugin. Only stores the plugin data
	/// </summary>
	internal class FxPlugin : IPlugin
	{
		public byte Plugin;
		public byte[] Data;
	}
}
