/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// StarTrekker AM sample structure
	/// </summary>
	internal class AmSample
	{
		public ushort Mark { get; set; }
		public ushort StartAmp { get; set; }
		public ushort Attack1Level { get; set; }
		public ushort Attack1Speed { get; set; }
		public ushort Attack2Level { get; set; }
		public ushort Attack2Speed { get; set; }
		public ushort SustainLevel { get; set; }
		public ushort DecaySpeed { get; set; }
		public ushort SustainTime { get; set; }
		public ushort ReleaseSpeed { get; set; }
		public ushort Waveform { get; set; }
		public ushort PitchFall { get; set; }
		public short VibAmp { get; set; }
		public ushort VibSpeed { get; set; }
		public ushort BaseFreq { get; set; }
	}
}
