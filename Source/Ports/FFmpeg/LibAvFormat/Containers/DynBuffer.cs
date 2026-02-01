/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class DynBuffer : IOptionContext
	{
		/// <summary>
		/// 
		/// </summary>
		public c_int Pos;

		/// <summary>
		/// 
		/// </summary>
		public c_int Size;

		/// <summary>
		/// 
		/// </summary>
		public c_int Allocated_Size;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Buffer;

		/// <summary>
		/// 
		/// </summary>
		public c_int Io_Buffer_Size;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Io_Buffer;
	}
}
