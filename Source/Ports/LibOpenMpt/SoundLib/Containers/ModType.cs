/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum ModType
	{
		None = 0x00,
		Mod = 0x01,
		S3M = 0x02,
		Xm = 0x04,
		Med = 0x08,
		Mtm = 0x10,
		It = 0x20,
		_669 = 0x40,
		Ult = 0x80,
		Stm = 0x100,
		Far = 0x200,
		Dtm = 0x400,
		Amf = 0x800,
		Ams = 0x1000,
		Dsm = 0x2000,
		Mdl = 0x4000,
		Okt = 0x8000,
		Mid = 0x10000,
		Dmf = 0x20000,
		Ptm = 0x40000,
		Dbm = 0x80000,
		Mt2 = 0x100000,
		Amf0 = 0x200000,
		Psm = 0x400000,
		J2B = 0x800000,
		Mpt = 0x1000000,
		Imf = 0x2000000,
		Digi = 0x4000000,
		Stp = 0x8000000,
		Plm = 0x10000000,
		Sfx = 0x20000000,

		Mod_PC = Mod | Xm
	}
}
