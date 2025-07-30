/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers
{
	/// <summary>
	/// Ins structure
	/// </summary>
	internal class Ins
	{
		public string Name { get; set; }

		public InsStep[] Steps { get; set; }

		// Filter - Amp
		public TimeLev[] Amp { get; set; }
		public TimeLev[] Filter { get; set; }

		public byte AmpPoints { get; set; }
		public byte FilterPoints { get; set; }

		public byte FilterMode { get; set; }
		public byte ClipMode { get; set; }
		public byte Boost { get; set; }

		public byte Sps { get; set; }				// PAL-screen per step
		public byte Res { get; set; }				// Resonance

		public byte VibS { get; set; }
		public byte VibD { get; set; }

		public byte PwmS { get; set; }
		public byte PwmD { get; set; }

		public byte Loop { get; set; }
		public byte Len { get; set; }
	}
}
