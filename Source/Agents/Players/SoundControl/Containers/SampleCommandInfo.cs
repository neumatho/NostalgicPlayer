/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers
{
	/// <summary>
	/// Holds information about a single sample command
	/// </summary>
	internal class SampleCommandInfo
	{
		public SampleCommand Command { get; set; }
		public ushort Argument1 { get; set; }
		public ushort Argument2 { get; set; }
	}
}
