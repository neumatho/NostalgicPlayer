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
	/// <summary>
	/// </summary>
	internal class Cdb
	{
		public sbyte MacroRun;
		public sbyte EfxRun;
		public byte NewStyleMacro;
		public byte PrevNote;
		public byte CurrNote;
		public byte Velocity;
		public byte FineTune;
		public bool KeyUp;
		public byte ReallyWait;
		public uint MacroPtr;
		public ushort MacroStep;
		public ushort MacroWait;
		public ushort MacroNum;
		public short Loop;

		public uint CurAddr;
		public uint SaveAddr;
		public ushort CurrLength;
		public ushort SaveLen;

		public ushort WaitDmaCount;

		public byte EnvReset;
		public byte EnvTime;
		public byte EnvRate;
		public sbyte EnvEndVol;
		public sbyte CurVol;

		public short VibOffset;
		public sbyte VibWidth;
		public bool VibFlag;
		public byte VibReset;
		public byte VibTime;

		public byte PortaReset;
		public byte PortaTime;
		public ushort CurPeriod;
		public ushort DestPeriod;
		public ushort PortaPer;
		public short PortaRate;

		public byte AddBeginTime;
		public byte AddBeginReset;
		public ushort ReturnPtr;
		public ushort ReturnStep;
		public int AddBegin;

		public byte SfxFlag;
		public byte SfxPriority;
		public short SfxLockTime;
		public uint SfxCode;

		public Hdb hw;
	}
}
