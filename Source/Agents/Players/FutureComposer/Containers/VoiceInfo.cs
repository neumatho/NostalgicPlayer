/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.FutureComposer.Containers
{
	/// <summary>
	/// Voice information structure
	/// </summary>
	internal class VoiceInfo
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
		public IChannel Channel;
	}
}
