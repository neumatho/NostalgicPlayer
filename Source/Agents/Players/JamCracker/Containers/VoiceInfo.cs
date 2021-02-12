/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Mixer;

namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker.Containers
{
	/// <summary>
	/// Voice info structure
	/// </summary>
	internal class VoiceInfo
	{
		public ushort WaveOffset = 0;
		public ushort Dmacon;
		public Channel Channel;
		public ushort InsLen;
		public sbyte[] InsAddress;
		public sbyte[] RealInsAddress;
		public sbyte[] WaveBuffer = new sbyte[0x40];
		public int PerIndex;
		public ushort[] Pers = new ushort[3];
		public short Por;
		public short DeltaPor;
		public short PorLevel;
		public short Vib;
		public short DeltaVib;
		public short Vol;
		public short DeltaVol;
		public ushort VolLevel;
		public ushort Phase;
		public short DeltaPhase;
		public byte VibCnt;
		public byte VibMax;
		public byte Flags;
	}
}
