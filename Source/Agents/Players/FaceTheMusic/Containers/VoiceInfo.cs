/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int ChannelNumber { get; set; }

		public Sample CurrentSample { get; set; }
		public short SampleNumber { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint SampleStartOffset { get; set; }
		public uint SampleCalculateOffset { get; set; }
		public ushort SampleOneshotLength { get; set; }
		public uint SampleLoopStart { get; set; }
		public ushort SampleLoopLength { get; set; }
		public uint SampleTotalLength { get; set; }

		public ushort Volume { get; set; }
		public ushort NoteIndex { get; set; }
		public bool RetrigSample { get; set; }

		public int DetuneIndex { get; set; }

		public Track Track { get; set; }
		public uint TrackPosition { get; set; }
		public short RowsLeftToSkip { get; set; }

		public short PortamentoTicks { get; set; }
		public uint PortamentoNote { get; set; }
		public ushort PortamentoEndNote { get; set; }

		public uint VolumeDownVolume { get; set; }
		public ushort VolumeDownSpeed { get; set; }

		public SoundEffectState SoundEffectState { get; set; }
		public LfoState[] LfoStates { get; set; } = new LfoState[4];

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(VoiceInfo destination)
		{
			destination.CurrentSample = CurrentSample;
			destination.SampleNumber = SampleNumber;
			destination.SampleData = SampleData;
			destination.SampleStartOffset = SampleStartOffset;
			destination.SampleCalculateOffset = SampleCalculateOffset;
			destination.SampleOneshotLength = SampleOneshotLength;
			destination.SampleLoopStart = SampleLoopStart;
			destination.SampleLoopLength = SampleLoopLength;
			destination.SampleTotalLength = SampleTotalLength;

			destination.Volume = Volume;
			destination.NoteIndex = NoteIndex;
			destination.RetrigSample = RetrigSample;

			destination.DetuneIndex = DetuneIndex;

			destination.Track = Track;
			destination.TrackPosition = TrackPosition;
			destination.RowsLeftToSkip = RowsLeftToSkip;

			destination.PortamentoTicks = PortamentoTicks;
			destination.PortamentoNote = PortamentoNote;
			destination.PortamentoEndNote = PortamentoEndNote;

			destination.VolumeDownVolume = VolumeDownVolume;
			destination.VolumeDownSpeed = VolumeDownSpeed;

			destination.SoundEffectState = SoundEffectState.MakeDeepClone();
			destination.LfoStates = ArrayHelper.CloneObjectArray(LfoStates);
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			clone.SoundEffectState = SoundEffectState.MakeDeepClone();
			clone.LfoStates = ArrayHelper.CloneObjectArray(LfoStates);

			return clone;
		}
	}
}
