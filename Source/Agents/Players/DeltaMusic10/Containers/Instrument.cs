/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic10.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument
	{
		/********************************************************************/
		/// <summary>
		/// Make a copy of the given instrument and return it
		/// </summary>
		/********************************************************************/
		public Instrument MakeCopy()
		{
			Instrument dest = new Instrument
			{
				Number = Number,

				AttackStep = AttackStep,
				AttackDelay = AttackDelay,
				DecayStep = DecayStep,
				DecayDelay = DecayDelay,
				Sustain = Sustain,
				ReleaseStep = ReleaseStep,
				ReleaseDelay = ReleaseDelay,
				Volume = Volume,
				VibratoWait = VibratoWait,
				VibratoStep = VibratoStep,
				VibratoLength = VibratoLength,
				BendRate = BendRate,
				Portamento = Portamento,
				IsSample = IsSample,
				TableDelay = TableDelay,
				SampleLength = SampleLength,
				RepeatStart = RepeatStart,
				RepeatLength = RepeatLength,
				Table = Table,
				SampleData = SampleData
			};

			Array.Copy(Arpeggio, dest.Arpeggio, 8);

			return dest;
		}

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
		public readonly byte[] Arpeggio = new byte[8];
		public ushort SampleLength;
		public ushort RepeatStart;
		public ushort RepeatLength;
		public byte[] Table;
		public sbyte[] SampleData;
	}
}
