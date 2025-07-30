/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon20.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument : IDeepCloneable<Instrument>
	{
		public byte WaveformListNumber { get; set; }
		public byte WaveformListLength { get; set; }
		public byte WaveformListSpeed { get; set; }
		public byte WaveformListDelay { get; set; }
		public byte ArpeggioNumber { get; set; }
		public byte ArpeggioLength { get; set; }
		public byte ArpeggioSpeed { get; set; }
		public byte ArpeggioDelay { get; set; }
		public byte VibratoNumber { get; set; }
		public byte VibratoLength { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoDelay { get; set; }
		public sbyte PitchBendSpeed { get; set; }
		public byte PitchBendDelay { get; set; }
		public byte AttackMax { get; set; }
		public byte AttackSpeed { get; set; }
		public byte DecayMin { get; set; }
		public byte DecaySpeed { get; set; }
		public byte SustainTime { get; set; }
		public byte ReleaseMin { get; set; }
		public byte ReleaseSpeed { get; set; }

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
