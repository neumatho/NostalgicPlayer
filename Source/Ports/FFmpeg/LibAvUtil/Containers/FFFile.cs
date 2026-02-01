/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FFFile
	{
		/// <summary>
		/// 
		/// </summary>
		public size_t Buf_Size;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Buf;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> RPos;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> REnd;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> ShEnd;

		/// <summary>
		/// 
		/// </summary>
		public ptrdiff_t ShLim;

		/// <summary>
		/// 
		/// </summary>
		public ptrdiff_t ShCnt;

		/// <summary>
		/// 
		/// </summary>
		public object Cookie;

		/// <summary>
		/// 
		/// </summary>
		public UtilFunc.Read_Delegate Read;
	}
}
