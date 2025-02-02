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
		public int ChannelNumber;

		public Sample CurrentSample;
		public short SampleNumber;
		public sbyte[] SampleData;
		public uint SampleStartOffset;
		public uint SampleCalculateOffset;
		public ushort SampleOneshotLength;
		public uint SampleLoopStart;
		public ushort SampleLoopLength;
		public uint SampleTotalLength;

		public ushort Volume;
		public ushort NoteIndex;
		public bool RetrigSample;

		public int DetuneIndex;

		public Track Track;
		public uint TrackPosition;
		public short RowsLeftToSkip;

		public short PortamentoTicks;
		public uint PortamentoNote;
		public ushort PortamentoEndNote;

		public uint VolumeDownVolume;
		public ushort VolumeDownSpeed;

		public SoundEffectState SoundEffectState;
		public LfoState[] LfoStates = new LfoState[4];

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
