/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.FutureComposer.Containers
{
	/// <summary>
	/// Voice information structure
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public sbyte PitchBendSpeed { get; set; }
		public byte PitchBendTime { get; set; }
		public ushort SongPos { get; set; }
		public sbyte CurNote { get; set; }
		public byte[] VolumeSeq { get; set; }
		public byte VolumeBendSpeed { get; set; }
		public byte VolumeBendTime { get; set; }
		public ushort VolumeSeqPos { get; set; }
		public sbyte SoundTranspose { get; set; }
		public byte VolumeCounter { get; set; }
		public byte VolumeSpeed { get; set; }
		public byte VolSusCounter { get; set; }
		public byte SusCounter { get; set; }
		public sbyte VibSpeed { get; set; }
		public sbyte VibDepth { get; set; }
		public sbyte VibValue { get; set; }
		public sbyte VibDelay { get; set; }
		public Pattern CurPattern { get; set; }
		public bool VolBendFlag { get; set; }
		public bool PortFlag { get; set; }
		public ushort PatternPos { get; set; }
		public bool PitchBendFlag { get; set; }
		public sbyte PattTranspose { get; set; }
		public sbyte Transpose { get; set; }
		public sbyte Volume { get; set; }
		public byte VibFlag { get; set; }
		public byte Portamento { get; set; }
		public ushort FrequencySeqStartOffset { get; set; }
		public ushort FrequencySeqPos { get; set; }
		public ushort Pitch { get; set; }

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
