/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ActivisionPro.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public byte SpeedCounter;
		public byte SpeedCounter2;
		public byte MaxSpeedCounter;
		public byte TickCounter;

		public byte[] PositionList;
		public sbyte PositionListPosition;
		public bool PositionListLoopEnabled;
		public byte PositionListLoopCount;
		public sbyte PositionListLoopStart;

		public byte TrackNumber;
		public byte TrackPosition;
		public byte LoopTrackCounter;
		
		public byte Note;
		public sbyte Transpose;
		public short FineTune;
		public byte NoteAndFlag;
		public ushort Period;

		public sbyte InstrumentNumber;
		public Instrument Instrument;

		public byte SampleNumber;
		public sbyte[] SampleData;
		public ushort SampleLength;
		public ushort SampleLoopStart;
		public ushort SampleLoopLength;

		public byte EnabledEffectsFlag;
		public bool StopResetEffect;
		public byte StopResetEffectDelay;

		public Envelope Envelope;
		public byte EnvelopePosition;
		public sbyte EnvelopeWaitCounter;
		public byte EnvelopeLoopCount;

		public bool Mute;
		public ushort Volume;
		public byte TrackVolume;
		
		public byte PortamentoValue;

		public ushort VibratoSpeed;
		public byte VibratoDelay;
		public sbyte VibratoDepth;
		public bool VibratoDirection;
		public bool VibratoCountDirection;
		public byte VibratoCounterMax;
		public byte VibratoCounter;

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
