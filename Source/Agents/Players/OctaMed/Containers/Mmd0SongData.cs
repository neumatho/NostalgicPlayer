/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// MMD0 song data structure
	/// </summary>
	internal class Mmd0SongData
	{
		public ushort NumBlocks;
		public ushort SongLen;
		public readonly byte[] PlaySeq = new byte[256];
		public ushort DefTempo;
		public sbyte PlayTransp;
		public MmdFlag Flags;
		public MmdFlag2 Flags2;
		public byte Tempo2;
		public readonly byte[] TrkVol = new byte[16];
		public byte MasterVol;
		public byte NumSamples;
	}
}
