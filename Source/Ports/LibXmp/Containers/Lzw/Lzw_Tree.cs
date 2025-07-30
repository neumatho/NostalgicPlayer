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
		public ref CPointer<Lzw_Code> Codes => ref _Codes;
		private CPointer<Lzw_Code> _Codes;
		public uint Bits { get; set; }
		public uint Length { get; set; }
		public uint MaxLength { get; set; }
		public uint DefaultLength { get; set; }
		public uint AllocLength { get; set; }
		public uint Previous_Code { get; set; }
		public bool New_Inc { get; set; }
		public c_int Flags { get; set; }
		public uint8 Previous_First_Char { get; set; }
	}
}
