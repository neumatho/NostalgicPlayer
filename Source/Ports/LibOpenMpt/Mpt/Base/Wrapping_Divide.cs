/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Wrapping_Divide
	{
		/********************************************************************/
		/// <summary>
		/// Modulo with more intuitive behaviour for some contexts:
		/// Instead of being symmetrical around 0, the pattern for positive
		/// numbers is repeated in the negative range.
		/// For example, wrapping_modulo(-1, m) == (m - 1).
		/// Behaviour is undefined if m‹=0
		/// </summary>
		/********************************************************************/
		public static T Wrapping_Modulo<T, M>(T x, M m) where T : INumber<T>, IModulusOperators<T, M, T> where M : INumber<M>, ISubtractionOperators<M, T, T>
		{
			return (x >= T.Zero) ? (x % m) : (m - M.One - ((-T.One - x) % m));
		}



		/********************************************************************/
		/// <summary>
		/// Same as above, but for a signed value with an unsigned modulus.
		/// C＃ has no equivalent to the C++ usual arithmetic conversions, so
		/// this combination needs its own overload, which calculates in the
		/// unsigned domain, just like C++ would
		/// </summary>
		/********************************************************************/
		public static uint32 Wrapping_Modulo(int32 x, uint32 m)
		{
			return (x >= 0) ? ((uint32)x % m) : (m - 1 - ((uint32)(-1 - x) % m));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static T Wrapping_Divide_<T, D>(T x, D d) where T : INumber<T>, IDivisionOperators<T, D, T> where D : INumber<D>
		{
			return (x >= T.Zero) ? (x / d) : (((x + T.One) / d) - T.One);
		}
	}
}
