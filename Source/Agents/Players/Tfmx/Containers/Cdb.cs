/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// </summary>
	internal class Cdb : IDeepCloneable<Cdb>
	{
		public sbyte MacroRun { get; set; }
		public sbyte EfxRun { get; set; }
		public byte NewStyleMacro { get; set; }
		public byte PrevNote { get; set; }
		public byte CurrNote { get; set; }
		public byte Velocity { get; set; }
		public byte FineTune { get; set; }
		public bool KeyUp { get; set; }
		public byte ReallyWait { get; set; }
		public uint MacroPtr { get; set; }
		public ushort MacroStep { get; set; }
		public ushort MacroWait { get; set; }
		public ushort MacroNum { get; set; }
		public short Loop { get; set; }

		public uint CurAddr { get; set; }
		public uint SaveAddr { get; set; }
		public ushort CurrLength { get; set; }
		public ushort SaveLen { get; set; }

		public ushort WaitDmaCount { get; set; }

		public byte EnvReset { get; set; }
		public byte EnvTime { get; set; }
		public byte EnvRate { get; set; }
		public sbyte EnvEndVol { get; set; }
		public sbyte CurVol { get; set; }

		public short VibOffset { get; set; }
		public sbyte VibWidth { get; set; }
		public bool VibFlag { get; set; }
		public byte VibReset { get; set; }
		public byte VibTime { get; set; }

		public byte PortaReset { get; set; }
		public byte PortaTime { get; set; }
		public ushort CurPeriod { get; set; }
		public ushort DestPeriod { get; set; }
		public ushort PortaPer { get; set; }
		public short PortaRate { get; set; }

		public byte AddBeginTime { get; set; }
		public byte AddBeginReset { get; set; }
		public ushort ReturnPtr { get; set; }
		public ushort ReturnStep { get; set; }
		public int AddBegin { get; set; }

		public byte SfxFlag { get; set; }
		public byte SfxPriority { get; set; }
		public short SfxLockTime { get; set; }
		public uint SfxCode { get; set; }

		public Hdb Hw { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Cdb MakeDeepClone()
		{
			return (Cdb)MemberwiseClone();
		}
	}
}
