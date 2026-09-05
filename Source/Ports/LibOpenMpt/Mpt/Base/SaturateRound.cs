/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	internal static class SaturateRound
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TDst Saturate_Trunc<TDst, TSrc>(TSrc src) where TSrc : INumber<TSrc> where TDst : INumber<TDst>, IMinMaxValue<TDst>
		{
			if (src >= TSrc.CreateChecked(TDst.MaxValue))
				return TDst.MaxValue;

			if (src <= TSrc.CreateChecked(TDst.MinValue))
				return TDst.MinValue;

			return TDst.CreateChecked(src);
		}



		/********************************************************************/
		/// <summary>
		/// Rounds given double value to nearest integer value of type T.
		/// Out-of-range values are saturated to the specified integer type's
		/// limits
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TDst Saturate_Round<TDst, TSrc>(TSrc val) where TSrc : INumber<TSrc> where TDst : INumber<TDst>, IMinMaxValue<TDst>
		{
			return Saturate_Trunc<TDst, TSrc>(TSrc.CreateChecked(CMath.round((c_double.CreateChecked(val)))));
		}
	}
}
