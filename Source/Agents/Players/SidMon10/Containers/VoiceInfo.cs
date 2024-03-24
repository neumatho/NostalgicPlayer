/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon10.Containers
{
	/// <summary>
	/// Holds playing information for a single channel
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int SequenceIndex;
		public Sequence CurrentSequence;
		public int RowIndex;
		public Track CurrentTrack;
		public Instrument CurrentInstrument;
		public int InstrumentNumber;
		public ushort NotePeriod;
		public byte BendTo;
		public sbyte BendSpeed;
		public sbyte NoteOffset;
		public byte ArpeggioIndex;
		public EnvelopeState EnvelopeInProgress;
		public byte RowCount;
		public byte Volume;
		public byte SustainControl;
		public ushort PitchControl;
		public byte PhaseIndex;
		public byte PhaseSpeed;
		public ushort PitchFallControl;
		public byte WaveIndex;
		public byte WaveformNumber;
		public byte WaveSpeed;
		public bool LoopControl;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			return (VoiceInfo)MemberwiseClone();
		}
	}
}
