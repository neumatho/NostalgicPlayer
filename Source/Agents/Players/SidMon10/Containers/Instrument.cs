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
		public uint WaveformNumber;		// >= 16 -> sample number
		public byte[] Arpeggio = new byte[16];
		public byte AttackSpeed;
		public byte AttackMax;
		public byte DecaySpeed;
		public byte DecayMin;
		public byte SustainTime;
		public byte ReleaseSpeed;
		public byte ReleaseMin;
		public byte PhaseShift;
		public byte PhaseSpeed;
		public byte FineTune;
		public sbyte PitchFall;
		public ushort Volume;

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
