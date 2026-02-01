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
	public class AvExpr
	{
		internal enum ExprType
		{
			Value,
			Const,
			Func0,
			Func1,
			Func2,
			Squish,
			Gauss,
			Ld,
			IsNan,
			IsInf,
			Mod,
			Max,
			Min,
			Eq,
			Gt,
			Gte,
			Lte,
			Lt,
			Pow,
			Mul,
			Div,
			Add,
			Last,
			St,
			While,
			Taylor,
			Root,
			Floor,
			Ceil,
			Trunc,
			Round,
			Sqrt,
			Not,
			Random,
			Hypot,
			Gcd,
			If,
			IfNot,
			Print,
			BitAnd,
			BitOr,
			Between,
			Clip,
			Atan2,
			Lerp,
			Sgn,
			RandomI
		}

		/// <summary>
		/// 
		/// </summary>
		internal ExprType Type;

		/// <summary>
		/// 
		/// </summary>
		internal c_double Value;

		/// <summary>
		/// 
		/// </summary>
		internal c_int Const_Index;

		/// <summary>
		/// 
		/// </summary>
		internal (
			UtilFunc.Func0_Delegate Func0,
			UtilFunc.Func1_Delegate Func1,
			UtilFunc.Func2_Delegate Func2
		) A;

		/// <summary>
		/// 
		/// </summary>
		internal readonly CPointer<AvExpr> Param = new CPointer<AvExpr>(3);

		/// <summary>
		/// 
		/// </summary>
		internal CPointer<c_double> Var;

		/// <summary>
		/// 
		/// </summary>
		internal CPointer<FFSfc64> Prng_State;
	}
}
