/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon20.Containers
{
	/// <summary>
	/// Holds playing information for a single channel
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public Sequence[] SequenceList { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint SampleLength { get; set; }
		public ushort SamplePeriod { get; set; }
		public short SampleVolume { get; set; }
		public EnvelopeState EnvelopeState { get; set; }
		public ushort SustainCounter { get; set; }
		public Instrument Instrument { get; set; }
		public sbyte[] LoopSample { get; set; }
		public uint LoopOffset { get; set; }
		public uint LoopLength { get; set; }
		public ushort OriginalNote { get; set; }

		public ushort WaveListDelay { get; set; }
		public short WaveListOffset { get; set; }
		public ushort ArpeggioDelay { get; set; }
		public short ArpeggioOffset { get; set; }
		public ushort VibratoDelay { get; set; }
		public short VibratoOffset { get; set; }

		public ushort CurrentNote { get; set; }
		public ushort CurrentInstrument { get; set; }
		public ushort CurrentEffect { get; set; }
		public ushort CurrentEffectArg { get; set; }

		public ushort PitchBendCounter { get; set; }
		public short InstrumentTranspose { get; set; }
		public short PitchBendValue { get; set; }
		public ushort NoteSlideNote { get; set; }
		public short NoteSlideSpeed { get; set; }
		public int TrackPosition { get; set; }
		public byte[] CurrentTrack { get; set; }
		public ushort EmptyNotesCounter { get; set; }
		public short NoteTranspose { get; set; }
		public ushort CurrentSample { get; set; }

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
