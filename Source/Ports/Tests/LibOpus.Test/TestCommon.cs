/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test
{
	/// <summary>
	/// Base class with some helper methods and variables
	/// </summary>
	public abstract class TestCommon
	{
		/// <summary></summary>
		protected opus_uint32 Rz, Rw;

		/// <summary></summary>
		protected opus_uint32 iseed;

		/********************************************************************/
		/// <summary>
		/// MWC RNG of George Marsaglia
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected opus_uint32 Fast_Rand()
		{
			Rz = 36969 * (Rz & 65535) + (Rz >> 16);
			Rw = 18000 * (Rw & 65535) + (Rw >> 16);

			return (Rz << 16) + Rw;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Test_Failed()
		{
			Assert.Fail("A fatal error was detected");
		}
	}
}
