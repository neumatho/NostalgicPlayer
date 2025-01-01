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
	internal static class Os
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double Rint(c_double x)
		{
			return Math.Floor(x + 0.5f);
		}
	}
}
