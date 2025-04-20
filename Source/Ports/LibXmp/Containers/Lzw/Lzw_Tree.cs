/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Lzw
{
	/// <summary>
	/// 
	/// </summary>
	internal class Lzw_Tree
	{
		public CPointer<Lzw_Code> Codes;
		public uint Bits;
		public uint Length;
		public uint MaxLength;
		public uint DefaultLength;
		public uint AllocLength;
		public uint Previous_Code;
		public bool New_Inc;
		public c_int Flags;
		public uint8 Previous_First_Char;
	}
}
