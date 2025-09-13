/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public SinglePositionInfo[] PositionList { get; set; }
		public byte[] TrackData { get; set; }
		public int TrackPosition { get; set; }
		public byte DelayCounter { get; set; }

		public ushort InstrumentNumber { get; set; }
		public Instrument Instrument { get; set; }
		public sbyte InstrumentTranspose { get; set; }

		public ushort SampleNumber { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint SampleOffset { get; set; }
		public ushort SampleLength { get; set; }
		public uint SampleLoopStart { get; set; }
		public ushort SampleLoopLength { get; set; }

		public ushort Note { get; set; }
		public sbyte NoteTranspose { get; set; }
		public ushort NotePeriod { get; set; }

		public ushort FinalNote { get; set; }
		public ushort FinalPeriod { get; set; }
		public short FinalVolume { get; set; }
		public ushort GlobalVoiceVolume { get; set; }

		public EnvelopeState EnvelopeState { get; set; }
		public ushort SustainCounter { get; set; }

		public byte SampleNumberListSpeedCounter { get; set; }
		public short SampleNumberListPosition { get; set; }

		public byte ArpeggioListSpeedCounter { get; set; }
		public short ArpeggioListPosition { get; set; }

		public byte FrequencyListSpeedCounter { get; set; }
		public short FrequencyListPosition { get; set; }

		public Effect Effect { get; set; }
		public ushort EffectArgument { get; set; }

		public byte PortamentoDelayCounter { get; set; }
		public short PortamentoValue { get; set; }

		public ushort TonePortamentoEndPeriod { get; set; }
		public short TonePortamentoIncrementValue { get; set; }

		public byte VibratoEffectArgument { get; set; }
		public sbyte VibratoTableIndex { get; set; }

		public byte TremoloEffectArgument { get; set; }
		public sbyte TremoloTableIndex { get; set; }
		public ushort TremoloVolume { get; set; }	// This is never set in the player. However, the player sets TremoloTableIndex when setting the volume, so I think this is a bug and this value should be set instead

		public ushort SampleOffsetEffectArgument { get; set; }
		public ushort NoteDelayCounter { get; set; }

		public ushort RestartDelayCounter { get; set; }
		public sbyte[] RestartSampleData { get; set; }
		public uint RestartSampleOffset { get; set; }
		public ushort RestartSampleLength { get; set; }

		public bool TrigSample { get; set; }

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
