/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// MMD0 sample structure
	/// </summary>
	internal class Mmd0Sample
	{
		public ushort Rep { get; set; }
		public ushort RepLen { get; set; }
		public byte MidiCh { get; set; }
		public byte MidiPreset { get; set; }
		public byte Volume { get; set; }
		public sbyte STrans { get; set; }
	}
}
