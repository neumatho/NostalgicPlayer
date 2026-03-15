/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int ChannelNumber { get; set; }
		public TimeSpan? RestartTime { get; set; }

		public Waveform Sample1 { get; set; }
		public uint Sample1Offset { get; set; }	// Only used in frequency mapped mode
		public short Sample1Number { get; set; }
		public Waveform Sample2 { get; set; }
		public short Sample2Number { get; set; }

		public sbyte[][] AudioBuffer { get; set; }
		public byte UseAudioBuffer { get; set; }

		public Stack<object> Stack { get; set; } = new Stack<object>();

		public TrackData Track { get; set; }
		public int TrackPosition { get; set; }
		public byte TickCounter { get; set; }

		public bool NewNote { get; set; }
		public sbyte Transpose { get; set; }
		public ushort NotePeriod { get; set; }
		public ushort TargetPeriod { get; set; }
		public byte CurrentVolume { get; set; }
		public byte FinalVolume { get; set; }
		public byte MasterVolume { get; set; }
		public ResetFlag ResetFlags { get; set; }

		public byte PortamentoTickCounter { get; set; }
		public byte PortamentoIncrement { get; set; }
		public bool PortamentoDirection { get; set; }
		public byte PortamentoDelay { get; set; }
		public byte PortamentoDuration { get; set; }

		public Table VolumeEnvelope { get; set; }
		public byte VolumeEnvelopePosition { get; set; }
		public byte VolumeEnvelopeTickCounter { get; set; }
		public byte VolumeEnvelopeDelta { get; set; }

		public Table PeriodTable { get; set; }
		public byte PeriodTablePosition { get; set; }
		public byte PeriodTableTickCounter { get; set; }
		public byte PeriodTableCommand { get; set;}

		public Table WaveformTable { get; set; }
		public byte WaveformTablePosition { get; set; }

		public byte WaveformStartPosition { get; set; }
		public byte WaveformPosition { get; set; }
		public byte WaveformTickCounter { get; set; }
		public byte WaveformIncrement { get; set; }
		public byte WaveformMask { get; set; }
		public SynthesisFlag SynthesisMode { get; set; }
		public byte MorphSpeed { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			clone.AudioBuffer = ArrayHelper.CloneArray(AudioBuffer);
			clone.Stack = new Stack<object>(Stack);

			return clone;
		}
	}
}
