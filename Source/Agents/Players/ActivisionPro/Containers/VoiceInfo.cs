/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ActivisionPro.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public byte SpeedCounter { get; set; }
		public byte SpeedCounter2 { get; set; }
		public byte MaxSpeedCounter { get; set; }
		public byte TickCounter { get; set; }

		public byte[] PositionList { get; set; }
		public sbyte PositionListPosition { get; set; }
		public bool PositionListLoopEnabled { get; set; }
		public byte PositionListLoopCount { get; set; }
		public sbyte PositionListLoopStart { get; set; }

		public byte TrackNumber { get; set; }
		public byte TrackPosition { get; set; }
		public byte LoopTrackCounter { get; set; }
		
		public byte Note { get; set; }
		public sbyte Transpose { get; set; }
		public short FineTune { get; set; }
		public byte NoteAndFlag { get; set; }
		public ushort Period { get; set; }

		public sbyte InstrumentNumber { get; set; }
		public Instrument Instrument { get; set; }

		public byte SampleNumber { get; set; }
		public sbyte[] SampleData { get; set; }
		public ushort SampleLength { get; set; }
		public ushort SampleLoopStart { get; set; }
		public ushort SampleLoopLength { get; set; }

		public byte EnabledEffectsFlag { get; set; }
		public bool StopResetEffect { get; set; }
		public byte StopResetEffectDelay { get; set; }

		public Envelope Envelope { get; set; }
		public byte EnvelopePosition { get; set; }
		public sbyte EnvelopeWaitCounter { get; set; }
		public byte EnvelopeLoopCount { get; set; }

		public bool Mute { get; set; }
		public ushort Volume { get; set; }
		public byte TrackVolume { get; set; }
		
		public byte PortamentoValue { get; set; }

		public ushort VibratoSpeed { get; set; }
		public byte VibratoDelay { get; set; }
		public sbyte VibratoDepth { get; set; }
		public bool VibratoDirection { get; set; }
		public bool VibratoCountDirection { get; set; }
		public byte VibratoCounterMax { get; set; }
		public byte VibratoCounter { get; set; }

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
