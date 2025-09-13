/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic10.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument : IDeepCloneable<Instrument>
	{
		public short Number { get; set; }

		public byte AttackStep { get; set; }
		public byte AttackDelay { get; set; }
		public byte DecayStep { get; set; }
		public byte DecayDelay { get; set; }
		public ushort Sustain { get; set; }
		public byte ReleaseStep { get; set; }
		public byte ReleaseDelay { get; set; }
		public byte Volume { get; set; }
		public byte VibratoWait { get; set; }
		public byte VibratoStep { get; set; }
		public byte VibratoLength { get; set; }
		public sbyte BendRate { get; set; }
		public byte Portamento { get; set; }
		public bool IsSample { get; set; }
		public byte TableDelay { get; set; }
		public byte[] Arpeggio { get; set; } = new byte[8];
		public ushort SampleLength { get; set; }
		public ushort RepeatStart { get; set; }
		public ushort RepeatLength { get; set; }
		public byte[] Table { get; set; }
		public sbyte[] SampleData { get; set; }

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
