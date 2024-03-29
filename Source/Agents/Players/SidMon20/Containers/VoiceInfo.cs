/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon20.Containers
{
	/// <summary>
	/// Holds playing information for a single channel
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public Sequence[] SequenceList;
		public sbyte[] SampleData;
		public uint SampleLength;
		public ushort SamplePeriod;
		public short SampleVolume;
		public EnvelopeState EnvelopeState;
		public ushort SustainCounter;
		public Instrument Instrument;
		public sbyte[] LoopSample;
		public uint LoopOffset;
		public uint LoopLength;
		public ushort OriginalNote;

		public ushort WaveListDelay;
		public short WaveListOffset;
		public ushort ArpeggioDelay;
		public short ArpeggioOffset;
		public ushort VibratoDelay;
		public short VibratoOffset;

		public ushort CurrentNote;
		public ushort CurrentInstrument;
		public ushort CurrentEffect;
		public ushort CurrentEffectArg;

		public ushort PitchBendCounter;
		public short InstrumentTranspose;
		public short PitchBendValue;
		public ushort NoteSlideNote;
		public short NoteSlideSpeed;
		public int TrackPosition;
		public byte[] CurrentTrack;
		public ushort EmptyNotesCounter;
		public short NoteTranspose;
		public ushort CurrentSample;

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
