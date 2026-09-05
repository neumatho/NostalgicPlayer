/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Bit
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Rotr_Impl<T>(T x, c_int r) where T : IBinaryInteger<T>, IUnsignedNumber<T>//XX 332
		{
			c_int n = Marshal.SizeOf<T>() * 8;

			return (x << (n - r)) | (x >> r);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Rotl<T>(T x, c_int s) where T : IBinaryInteger<T>, IUnsignedNumber<T>//XX 338
		{
			c_int n = Marshal.SizeOf<T>() * 8;
			c_int r = s % n;

			return s < 0 ? Rotr_Impl(x, -s) : (x >> (n - r)) | (x << r);
		}
	}
}
