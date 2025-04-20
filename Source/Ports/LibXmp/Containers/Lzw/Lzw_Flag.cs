/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Lzw
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Lzw_Flag
	{
		public static int MaxBits(int x) => x & 15;
		public const int SymQuirks = 0x100;

		public static readonly int Sym = MaxBits(13) | SymQuirks;
	}
}
