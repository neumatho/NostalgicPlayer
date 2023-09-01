/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Virt
{
	/// <summary>
	/// 
	/// </summary>
	internal enum Virt_Action
	{
		Cut = Xmp_Inst_Nna.Cut,
		Cont = Xmp_Inst_Nna.Cont,
		Off = Xmp_Inst_Nna.Off,
		Fade = Xmp_Inst_Nna.Fade,

		Active = 0x100,
		Invalid = -1
	}
}
