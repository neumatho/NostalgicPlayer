/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers
{
	/// <summary>
	/// Holds all the loaded data
	/// </summary>
	internal class ModuleData
	{
		public ushort Speed { get; set; }				// Only used from 4.0 player

		public PositionList PositionList { get; set; }
		public byte[][] Tracks { get; set; }
		public Instrument[] Instruments { get; set; }
		public Sample[] Samples { get; set; }
	}
}
