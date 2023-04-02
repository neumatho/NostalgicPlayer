/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic10.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument : IDeepCloneable<Instrument>
	{
		public short Number;

		public byte AttackStep;
		public byte AttackDelay;
		public byte DecayStep;
		public byte DecayDelay;
		public ushort Sustain;
		public byte ReleaseStep;
		public byte ReleaseDelay;
		public byte Volume;
		public byte VibratoWait;
		public byte VibratoStep;
		public byte VibratoLength;
		public sbyte BendRate;
		public byte Portamento;
		public bool IsSample;
		public byte TableDelay;
		public byte[] Arpeggio = new byte[8];
		public ushort SampleLength;
		public ushort RepeatStart;
		public ushort RepeatLength;
		public byte[] Table;
		public sbyte[] SampleData;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Instrument MakeDeepClone()
		{
			Instrument clone = (Instrument)MemberwiseClone();

			clone.Arpeggio = ArrayHelper.CloneArray(Arpeggio);

			return clone;
		}
	}
}
