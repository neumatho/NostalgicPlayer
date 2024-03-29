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
		public byte WaveformListNumber;
		public byte WaveformListLength;
		public byte WaveformListSpeed;
		public byte WaveformListDelay;
		public byte ArpeggioNumber;
		public byte ArpeggioLength;
		public byte ArpeggioSpeed;
		public byte ArpeggioDelay;
		public byte VibratoNumber;
		public byte VibratoLength;
		public byte VibratoSpeed;
		public byte VibratoDelay;
		public sbyte PitchBendSpeed;
		public byte PitchBendDelay;
		public byte AttackMax;
		public byte AttackSpeed;
		public byte DecayMin;
		public byte DecaySpeed;
		public byte SustainTime;
		public byte ReleaseMin;
		public byte ReleaseSpeed;

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
