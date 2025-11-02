/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace NostalgicPlayer.Kit.C.Test.Math_
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public abstract class TestMathBase
	{
		/********************************************************************/
		/// <summary>
		/// Convert a double to its binary representation as a 64-bit
		/// unsigned integer
		/// </summary>
		/********************************************************************/
		protected uint64_t To_Ordered(c_double d)
		{
			return BitConverter.DoubleToUInt64Bits(d);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected uint64_t Ulp_Diff(c_double a, c_double b)
		{
			uint64_t oa = To_Ordered(a);
			uint64_t ob = To_Ordered(b);

			return oa > ob ? oa - ob : ob - oa;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected bool Nearly_Equal_Ulps(c_double a, c_double b, uint64_t max_Ulps)
		{
			if (c_double.IsNaN(a) && c_double.IsNaN(b))
				return true;

			if (c_double.IsInfinity(a) || (c_double.IsInfinity(b)))
				return a == b;

			return Ulp_Diff(a, b) <= max_Ulps;
		}
	}
}
