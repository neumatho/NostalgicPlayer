/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class Vlc
	{
		/// <summary>
		/// 
		/// </summary>
		public c_int Bits;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<VlcElem> Table;

		/// <summary>
		/// 
		/// </summary>
		public c_int Table_Size;

		/// <summary>
		/// 
		/// </summary>
		public c_int Table_Allocated;
	}
}
