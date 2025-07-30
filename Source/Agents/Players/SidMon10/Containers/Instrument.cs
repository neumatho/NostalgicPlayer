/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon10.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument : IDeepCloneable<Instrument>
	{
		public uint WaveformNumber { get; set; }		// >= 16 -> sample number
		public byte[] Arpeggio { get; set; } = new byte[16];
		public byte AttackSpeed { get; set; }
		public byte AttackMax { get; set; }
		public byte DecaySpeed { get; set; }
		public byte DecayMin { get; set; }
		public byte SustainTime { get; set; }
		public byte ReleaseSpeed { get; set; }
		public byte ReleaseMin { get; set; }
		public byte PhaseShift { get; set; }
		public byte PhaseSpeed { get; set; }
		public byte FineTune { get; set; }
		public sbyte PitchFall { get; set; }
		public ushort Volume { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Instrument MakeDeepClone()
		{
			return (Instrument)MemberwiseClone();
		}
	}
}
