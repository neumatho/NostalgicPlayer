﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.FutureComposer.Containers
{
	/// <summary>
	/// Voice information structure
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public sbyte PitchBendSpeed;
		public byte PitchBendTime;
		public ushort SongPos;
		public sbyte CurNote;
		public byte[] VolumeSeq;
		public byte VolumeBendSpeed;
		public byte VolumeBendTime;
		public ushort VolumeSeqPos;
		public sbyte SoundTranspose;
		public byte VolumeCounter;
		public byte VolumeSpeed;
		public byte VolSusCounter;
		public byte SusCounter;
		public sbyte VibSpeed;
		public sbyte VibDepth;
		public sbyte VibValue;
		public sbyte VibDelay;
		public Pattern CurPattern;
		public bool VolBendFlag;
		public bool PortFlag;
		public ushort PatternPos;
		public bool PitchBendFlag;
		public sbyte PattTranspose;
		public sbyte Transpose;
		public sbyte Volume;
		public byte VibFlag;
		public byte Portamento;
		public ushort FrequencySeqStartOffset;
		public ushort FrequencySeqPos;
		public ushort Pitch;

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
