/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon10.Containers
{
	/// <summary>
	/// Holds playing information for a single channel
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int SequenceIndex { get; set; }
		public Sequence CurrentSequence { get; set; }
		public int RowIndex { get; set; }
		public Track CurrentTrack { get; set; }
		public Instrument CurrentInstrument { get; set; }
		public int InstrumentNumber { get; set; }
		public ushort NotePeriod { get; set; }
		public byte BendTo { get; set; }
		public sbyte BendSpeed { get; set; }
		public sbyte NoteOffset { get; set; }
		public byte ArpeggioIndex { get; set; }
		public EnvelopeState EnvelopeInProgress { get; set; }
		public byte RowCount { get; set; }
		public byte Volume { get; set; }
		public byte SustainControl { get; set; }
		public ushort PitchControl { get; set; }
		public byte PhaseIndex { get; set; }
		public byte PhaseSpeed { get; set; }
		public ushort PitchFallControl { get; set; }
		public byte WaveIndex { get; set; }
		public byte WaveformNumber { get; set; }
		public byte WaveSpeed { get; set; }
		public bool LoopControl { get; set; }

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
