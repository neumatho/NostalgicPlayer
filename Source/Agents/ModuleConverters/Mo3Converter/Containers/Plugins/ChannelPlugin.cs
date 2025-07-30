/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Plugins
{
	/// <summary>
	/// Indicate which plug-in should be used for each channel
	/// </summary>
	internal class ChannelPlugin : IPlugin
	{
		public uint[] Plugins { get; set; }
	}
}
