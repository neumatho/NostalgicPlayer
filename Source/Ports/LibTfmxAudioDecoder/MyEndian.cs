/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder
{
	/// <summary>
	/// 
	/// </summary>
	internal static class MyEndian
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static udword ReadBEUdword(CPointer<ubyte> ptr, udword offset)
		{
			return (udword)((ptr[offset] << 24) + (ptr[offset + 1] << 16) + (ptr[offset + 2] << 8) + ptr[offset + 3]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uword ReadBEUword(CPointer<ubyte> ptr, udword offset)
		{
			return (uword)((ptr[offset] << 8) + ptr[offset + 1]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uword MakeWord(ubyte hi, ubyte lo)
		{
			return (uword)((hi << 8) | lo);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static udword MakeDword(ubyte hiHi, ubyte hiLo, ubyte hi, ubyte lo)
		{
			return (udword)((MakeWord(hiHi, hiLo) << 16) | MakeWord(hi, lo));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static udword ByteSwap(udword someDword)
		{
			return ((someDword & 0xff000000) >> 24) | ((someDword & 0x00ff0000) >> 8) | ((someDword & 0x0000ff00) << 8) | ((someDword & 0x000000ff) << 24);
		}
	}
}
