/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	internal delegate bool Loop(Hdb hw);

	/// <summary>
	/// </summary>
	internal class Hdb
	{
		public uint Pos;
		public uint Delta;
		public ushort SLen;
		public ushort SampleLength;
		public int SBeg;
		public int SampleStart;
		public byte Vol;
		public byte Mode;
		public Loop Loop;
		public Cdb C;
	}
}
