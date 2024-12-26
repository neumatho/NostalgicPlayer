/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.BenDaglish.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public bool ChannelEnabled;

		public byte[] PositionList;
		public int CurrentPosition;
		public int NextPosition;

		public int PlayingTrack;
		public byte[] Track;
		public int NextTrackPosition;

		public bool SwitchToNextPosition;
		public byte TrackLoopCounter;
		public byte TicksLeftForNextTrackCommand;

		public sbyte Transpose;
		public byte TransposedNote;
		public byte PreviousTransposedNote;
		public bool UseNewNote;

		public byte Portamento1Enabled;
		public bool Portamento2Enabled;
		public byte PortamentoStartDelay;
		public byte PortamentoDuration;
		public sbyte PortamentoDeltaNoteNumber;

		public byte PortamentoControlFlag;
		public byte PortamentoStartDelayCounter;
		public byte PortamentoDurationCounter;
		public int PortamentoAddValue;

		public bool VolumeFadeEnabled;
		public byte VolumeFadeInitSpeed;
		public byte VolumeFadeDuration;
		public short VolumeFadeInitAddValue;

		public bool VolumeFadeRunning;
		public byte VolumeFadeSpeed;
		public byte VolumeFadeSpeedCounter;
		public byte VolumeFadeDurationCounter;
		public short VolumeFadeAddValue;
		public short VolumeFadeValue;

		public ushort ChannelVolume;
		public ushort ChannelVolumeSlideSpeed;
		public short ChannelVolumeSlideAddValue;

		public Sample SampleInfo;
		public Sample SampleInfo2;

		public byte[] SampleMapping = new byte[10];

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			clone.SampleMapping = ArrayHelper.CloneArray(SampleMapping);

			return clone;
		}
	}
}
