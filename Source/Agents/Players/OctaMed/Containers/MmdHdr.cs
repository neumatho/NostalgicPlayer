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
	/// Module header structure
	/// </summary>
	internal class MmdHdr
	{
		public uint Id;
		public uint ModLen;
		public uint SongOffs;
		public ushort PSecNum;
		public ushort PSeq;
		public uint BlocksOffs;
		public byte MmdFlags;
		public uint SamplesOffs;
		public uint ExpDataOffs;
		public ushort PState;
		public ushort PBlock;
		public ushort PLine;
		public ushort PSeqNum;
		public short ActPlayLine;
		public byte Counter;
		public byte ExtraSongs;
	}
}
