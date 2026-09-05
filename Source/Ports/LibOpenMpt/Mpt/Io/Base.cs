/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Base
	{
		public const size_t BufferSize_Small = 4 * 1024;
		public const size_t BufferSize_Normal = 64 * 1024;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsReadSeekable(this Stream f)
		{
			return new FileOperationsStdIstream(f).IsReadSeekable();
		}
	}
}
