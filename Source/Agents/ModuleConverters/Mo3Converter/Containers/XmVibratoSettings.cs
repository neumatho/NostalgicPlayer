/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Only used in XM modules
	/// </summary>
	internal class XmVibratoSettings
	{
		public byte Type { get; set; }
		public byte Sweep { get; set; }
		public byte Depth { get; set; }
		public byte Rate { get; set; }
	}
}
