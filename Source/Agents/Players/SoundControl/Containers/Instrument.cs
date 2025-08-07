/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument
	{
		public Envelope Envelope { get; set; }
		public SampleCommandInfo[] SampleCommands { get; set; }
	}
}
