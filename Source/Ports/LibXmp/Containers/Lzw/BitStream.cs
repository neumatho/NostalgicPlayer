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
	internal class BitStream
	{
		public uint32 Buf;
		public size_t Num_Read;
		public size_t Max_Read;
		public c_int Bits;
	}
}
