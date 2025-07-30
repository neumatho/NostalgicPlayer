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
		public bool ChannelEnabled { get; set; }

		public byte[] PositionList { get; set; }
		public int CurrentPosition { get; set; }
		public int NextPosition { get; set; }

		public int PlayingTrack { get; set; }
		public byte[] Track { get; set; }
		public int NextTrackPosition { get; set; }

		public bool SwitchToNextPosition { get; set; }
		public byte TrackLoopCounter { get; set; }
		public byte TicksLeftForNextTrackCommand { get; set; }

		public sbyte Transpose { get; set; }
		public byte TransposedNote { get; set; }
		public byte PreviousTransposedNote { get; set; }
		public bool UseNewNote { get; set; }

		public byte Portamento1Enabled { get; set; }
		public bool Portamento2Enabled { get; set; }
		public byte PortamentoStartDelay { get; set; }
		public byte PortamentoDuration { get; set; }
		public sbyte PortamentoDeltaNoteNumber { get; set; }

		public byte PortamentoControlFlag { get; set; }
		public byte PortamentoStartDelayCounter { get; set; }
		public byte PortamentoDurationCounter { get; set; }
		public int PortamentoAddValue { get; set; }

		public bool VolumeFadeEnabled { get; set; }
		public byte VolumeFadeInitSpeed { get; set; }
		public byte VolumeFadeDuration { get; set; }
		public short VolumeFadeInitAddValue { get; set; }

		public bool VolumeFadeRunning { get; set; }
		public byte VolumeFadeSpeed { get; set; }
		public byte VolumeFadeSpeedCounter { get; set; }
		public byte VolumeFadeDurationCounter { get; set; }
		public short VolumeFadeAddValue { get; set; }
		public short VolumeFadeValue { get; set; }

		public ushort ChannelVolume { get; set; }
		public ushort ChannelVolumeSlideSpeed { get; set; }
		public short ChannelVolumeSlideAddValue { get; set; }

		public Sample SampleInfo { get; set; }
		public Sample SampleInfo2 { get; set; }

		public byte[] SampleMapping { get; set; } = new byte[10];

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
