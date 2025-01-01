/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Scales
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float FromDb(c_float x)
		{
			return (c_float)(Math.Exp(x * 0.11512925f));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float ToBark(c_float n)
		{
			return (c_float)(13.1f * Math.Atan(0.00074f * n) + 2.24f * Math.Atan(n *n * 1.85e-8f) + 1e-4f * n);
		}
	}
}
