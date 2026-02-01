/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Simple arithmetic expression evaluator.
	///
	/// See http://joe.hotchkiss.com/programming/eval/eval.html
	/// </summary>
	public static class Eval
	{
		private const c_int Vars = 10;

		#region Parser class
		private class Parser : AvClass
		{
			public AvClass Class => this;
			public c_int Stack_Index;
			public CPointer<char> S;
			public CPointer<c_double> Const_Values;
			public CPointer<CPointer<char>> Const_Names;	// NULL terminated
			public CPointer<UtilFunc.Func1_Delegate> Funcs1;			// NULL terminated
			public CPointer<CPointer<char>> Func1_Names;	// NULL terminated
			public CPointer<UtilFunc.Func2_Delegate> Funcs2;			// NULL terminated
			public CPointer<CPointer<char>> Func2_Names;	// NULL terminated
			public IOpaque Opaque;
			public c_int Log_Offset;
			public IContext Log_Ctx;
			public CPointer<c_double> Var;
			public CPointer<FFSfc64> Prng_State;
		}
		#endregion

		#region Prefix class
		private class Prefix
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Prefix(c_double bin_Val, c_double dec_Val, int8_t exp)
			{
				Bin_Val = bin_Val;
				Dec_Val = dec_Val;
				Exp = exp;
			}

			public c_double Bin_Val { get; }
			public c_double Dec_Val { get; }
			public int8_t Exp { get; }
		}
		#endregion

		#region Constant class
		private class Constant
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Constant(string name, c_double value)
			{
				Name = name.ToCharPointer();
				Value = value;
			}

			public CPointer<char> Name { get; }
			public c_double Value { get; }
		}
		#endregion

		private static readonly AvClass eval_Class = new AvClass
		{
			Class_Name = "Eval".ToCharPointer(),
			Item_Name = Log.Av_Default_Item_Name,
			Option = null,
			Version = Version.Version_Int,
			Log_Level_Offset_Name = null,
			Parent_Log_Context_Name = null
		};

		private static readonly Prefix[] si_Prefixes = BuildPrefixes();
		private static readonly Constant[] constants = BuildConstants();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Prefix[] BuildPrefixes()
		{
			Prefix[] arr = new Prefix['z' - 'E' + 1];

			arr['y'-'E'] = new Prefix(8.271806125530276749e-25, 1e-24, -24);
			arr['z'-'E'] = new Prefix(8.4703294725430034e-22, 1e-21, -21);
			arr['a'-'E'] = new Prefix(8.6736173798840355e-19, 1e-18, -18);
			arr['f'-'E'] = new Prefix(8.8817841970012523e-16, 1e-15, -15);
			arr['p'-'E'] = new Prefix(9.0949470177292824e-13, 1e-12, -12);
			arr['n'-'E'] = new Prefix(9.3132257461547852e-10, 1e-9,  -9);
			arr['u'-'E'] = new Prefix(9.5367431640625e-7, 1e-6, -6);
			arr['m'-'E'] = new Prefix(9.765625e-4, 1e-3, -3);
			arr['c'-'E'] = new Prefix(9.8431332023036951e-3, 1e-2, -2);
			arr['d'-'E'] = new Prefix(9.921256574801246e-2, 1e-1, -1);
			arr['h'-'E'] = new Prefix(1.0159366732596479e2, 1e2, 2);
			arr['k'-'E'] = new Prefix(1.024e3, 1e3, 3);
			arr['K'-'E'] = new Prefix(1.024e3, 1e3, 3);
			arr['M'-'E'] = new Prefix(1.048576e6, 1e6, 6);
			arr['G'-'E'] = new Prefix(1.073741824e9, 1e9, 9);
			arr['T'-'E'] = new Prefix(1.099511627776e12, 1e12, 12);
			arr['P'-'E'] = new Prefix(1.125899906842624e15, 1e15, 15);
			arr['E'-'E'] = new Prefix(1.152921504606847e18, 1e18, 18);
			arr['Z'-'E'] = new Prefix(1.1805916207174113e21, 1e21, 21);
			arr['Y'-'E'] = new Prefix(1.2089258196146292e24, 1e24, 24);

			for (c_int i = 0; i < arr.Length; i++)
			{
				if (arr[i] == null)
					arr[i] = new Prefix(0, 0, 0);
			}

			return arr;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Constant[] BuildConstants()
		{
			return
			[
				new Constant("E", Math.E),
				new Constant("PI", Math.PI),
				new Constant("PHI", Mathematics.M_Phi),
				new Constant("QP2LAMBDA", AvConstants.FF_QP2Lambda)
			];
		}



		/********************************************************************/
		/// <summary>
		/// Parse the string in numstr and return its value as a double. If
		/// the string is empty, contains only whitespaces, or does not
		/// contain an initial substring that has the expected syntax for a
		/// floating-point number, no conversion is performed. In this case,
		/// returns a value of zero and the value returned in tail is the
		/// value of numstr
		/// </summary>
		/********************************************************************/
		public static c_double Av_Strtod(CPointer<char> numStr, out CPointer<char> tail)
		{
			c_double d;
			CPointer<char> next;

			if ((numStr[0] == '0') && ((numStr[1] | 0x20) == 'x'))
				d = CString.strtoul(numStr, out next, 16, out bool _);
			else
				d = CString.strtod(numStr, out next);

			// If parsing succeeded, check for and interpret postfixes
			if (next != numStr)
			{
				if ((next[0] == 'd') && (next[1] == 'B'))
				{
					// Treat dB as decibels instead of decibytes
					d = FFMath.FF_Exp10(d / 20);
					next += 2;
				}
				else if ((next[0] >= 'E') && (next[0] <= 'z'))
				{
					c_int e = si_Prefixes[next[0] - 'E'].Exp;

					if (e != 0)
					{
						if (next[1] == 'i')
						{
							d *= si_Prefixes[next[0] - 'E'].Bin_Val;
							next += 2;
						}
						else
						{
							d *= si_Prefixes[next[0] - 'E'].Dec_Val;
							next++;
						}
					}
				}

				if (next[0] == 'B')
				{
					d *= 8;
					next++;
				}
			}

			// If requested, fill in tail with the position after the last parsed
			// character
			tail = next;

			return d;
		}



		/********************************************************************/
		/// <summary>
		/// Free a parsed expression previously created with av_expr_parse()
		/// </summary>
		/********************************************************************/
		public static void Av_Expr_Free(AvExpr e)//XX 358
		{
			if (e == null)
				return;

			Av_Expr_Free(e.Param[0]);
			Av_Expr_Free(e.Param[1]);
			Av_Expr_Free(e.Param[2]);
			Mem.Av_FreeP(ref e.Var);
			Mem.Av_FreeP(ref e.Prng_State);
			Mem.Av_FreeP(ref e);
		}



		/********************************************************************/
		/// <summary>
		/// Parse an expression
		/// </summary>
		/********************************************************************/
		public static c_int Av_Expr_Parse(out AvExpr expr, CPointer<char> s, CPointer<CPointer<char>> const_Names, CPointer<CPointer<char>> func1_Names, CPointer<UtilFunc.Func1_Delegate> funcs1, CPointer<CPointer<char>> func2_Names, CPointer<UtilFunc.Func2_Delegate> funcs2, c_int log_Offset, IContext log_Ctx)//XX 710
		{
			expr = null;

			Parser p = new Parser();
			AvExpr e = null;
			CPointer<char> w = Mem.Av_MAlloc<char>(CString.strlen(s) + 1);
			CPointer<char> wp = w;
			CPointer<char> s0 = s;
			c_int ret = 0;

			if (w == null)
				return Error.ENOMEM;

			while (s[0] != 0)
			{
				if (!AvString.Av_IsSpace(s[0, 1]))
					wp[0, 1] = s[-1];
			}

			wp[0, 1] = '\0';

			eval_Class.CopyTo(p.Class);
			p.Stack_Index = 100;
			p.S = w;
			p.Const_Names = const_Names;
			p.Funcs1 = funcs1;
			p.Func1_Names = func1_Names;
			p.Funcs2 = funcs2;
			p.Func2_Names = func2_Names;
			p.Log_Offset = log_Offset;
			p.Log_Ctx = log_Ctx;

			ret = Parse_Expr(out e, p);
			if (ret < 0)
				goto End;

			if (p.S[0] != 0)
			{
				Log.Av_Log(p, Log.Av_Log_Error, "Invalid chars '%s' at the end of expression '%s'\n", p.S, s0);

				ret = Error.EINVAL;
				goto End;
			}

			if (!Verify_Expr(e))
			{
				ret = Error.EINVAL;
				goto End;
			}

			e.Var = Mem.Av_MAllocz<c_double>(Vars);
			e.Prng_State = Mem.Av_MAlloczObj<FFSfc64>(Vars);

			if (e.Var.IsNull || e.Prng_State.IsNull)
			{
				ret = Error.ENOMEM;
				goto End;
			}

			expr = e;
			e = null;

			End:
			Av_Expr_Free(e);
			Mem.Av_Free(w);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Evaluate a previously parsed expression
		/// </summary>
		/********************************************************************/
		public static c_double Av_Expr_Eval(AvExpr e, CPointer<c_double> const_Values, IOpaque opaque)//XX 792
		{
			Parser p = new Parser
			{
				Const_Values = const_Values,
				Opaque = opaque,
				Var = e.Var,
				Prng_State = e.Prng_State
			};
			eval_Class.CopyTo(p.Class);

			return Eval_Expr(p, e);
		}



		/********************************************************************/
		/// <summary>
		/// Parse and evaluate an expression.
		/// Note, this is significantly slower than av_expr_eval()
		/// </summary>
		/********************************************************************/
		public static c_int Av_Expr_Parse_And_Eval(out c_double d, CPointer<char> s, CPointer<CPointer<char>> const_Names, CPointer<c_double> const_Values, CPointer<CPointer<char>> func1_Names, CPointer<UtilFunc.Func1_Delegate> funcs1, CPointer<CPointer<char>> func2_Names, CPointer<UtilFunc.Func2_Delegate> funcs2, IOpaque opaque, c_int log_Offset, IContext log_Ctx)
		{
			c_int ret = Av_Expr_Parse(out AvExpr e, s, const_Names, func1_Names, funcs1, func2_Names, funcs2, log_Offset, log_Ctx);
			if (ret < 0)
			{
				d = c_double.NaN;
				return ret;
			}

			d = Av_Expr_Eval(e, const_Values, opaque);
			Av_Expr_Free(e);

			return c_double.IsNaN(d) ? Error.EINVAL : 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Is_Identifier_Char(char c)//XX 146
		{
			return ((c_uint)(c - '0') <= 9U) || ((c_uint)(c - 'a') <= 25U) || ((c_uint)(c - 'A') <= 25U) || (c == '_');
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool StrMatch(CPointer<char> s, CPointer<char> prefix)//XX 148
		{
			c_int i;

			for (i = 0; prefix[i] != 0; i++)
			{
				if (prefix[i] != s[i])
					return false;
			}

			// Return 1 only if the s identifier is terminated
			return !Is_Identifier_Char(s[i]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool StrMatch(CPointer<char> s, string prefix)
		{
			return StrMatch(s, prefix.ToCharPointer());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_double ETime(c_double v)
		{
			return Time.Av_GetTime() * 0.000001;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_double Eval_Expr(Parser p, AvExpr e)//XX 186
		{
			uint64_t Compute_Next_Random()
			{
				c_int idx = Common.Av_Clip((c_int)Eval_Expr(p, e.Param[0]), 0, Vars - 1);
				FFSfc64 s = p.Prng_State[idx];
				uint64_t r;

				if (s.Counter == 0)
				{
					r = CMath.isnan(p.Var[idx]) ? 0 : (uint64_t)p.Var[idx];
					Sfc64.FF_Sfc64_Init(s, r, r, r, 12);
				}

				r = Sfc64.FF_Sfc64_Get(s);
				p.Var[idx] = r;

				return r;
			}

			switch (e.Type)
			{
				case AvExpr.ExprType.Value:
					return e.Value;

				case AvExpr.ExprType.Const:
					return e.Value * p.Const_Values[e.Const_Index];

				case AvExpr.ExprType.Func0:
					return e.Value * e.A.Func0(Eval_Expr(p, e.Param[0]));

				case AvExpr.ExprType.Func1:
					return e.Value * e.A.Func1(p.Opaque, Eval_Expr(p, e.Param[0]));

				case AvExpr.ExprType.Func2:
					return e.Value * e.A.Func2(p.Opaque, Eval_Expr(p, e.Param[0]), Eval_Expr(p, e.Param[1]));

				case AvExpr.ExprType.Squish:
					return 1 / (1 + CMath.exp(4 * Eval_Expr(p, e.Param[0])));

				case AvExpr.ExprType.Gauss:
				{
					c_double d = Eval_Expr(p, e.Param[0]);

					return CMath.exp(-d * d / 2) / CMath.sqrt(2 * Math.PI);
				}

				case AvExpr.ExprType.Ld:
					return e.Value * p.Var[Common.Av_Clip((c_int)Eval_Expr(p, e.Param[0]), 0, Vars - 1)];

				case AvExpr.ExprType.IsNan:
					return e.Value * (CMath.isnan(Eval_Expr(p, e.Param[0])) ? 1 : 0);

				case AvExpr.ExprType.IsInf:
					return e.Value * (CMath.isinf(Eval_Expr(p, e.Param[0])) ? 1 : 0);

				case AvExpr.ExprType.Floor:
					return e.Value * CMath.floor(Eval_Expr(p, e.Param[0]));

				case AvExpr.ExprType.Ceil:
					return e.Value * CMath.ceil(Eval_Expr(p, e.Param[0]));

				case AvExpr.ExprType.Trunc:
					return e.Value * CMath.trunc(Eval_Expr(p, e.Param[0]));

				case AvExpr.ExprType.Round:
					return e.Value * CMath.round(Eval_Expr(p, e.Param[0]));

				case AvExpr.ExprType.Sgn:
					return e.Value * Macros.FFDiffSign(Eval_Expr(p, e.Param[0]), 0);

				case AvExpr.ExprType.Sqrt:
					return e.Value * CMath.sqrt(Eval_Expr(p, e.Param[0]));

				case AvExpr.ExprType.Not:
					return e.Value * (Eval_Expr(p, e.Param[0]) == 0 ? 1 : 0);

				case AvExpr.ExprType.If:
					return e.Value * (Eval_Expr(p, e.Param[0]) != 0 ? Eval_Expr(p, e.Param[1]) : e.Param[2] != null ? Eval_Expr(p, e.Param[2]) : 0);

				case AvExpr.ExprType.IfNot:
					return e.Value * (Eval_Expr(p, e.Param[0]) == 0 ? Eval_Expr(p, e.Param[1]) : e.Param[2] != null ? Eval_Expr(p, e.Param[2]) : 0);

				case AvExpr.ExprType.Clip:
				{
					c_double x = Eval_Expr(p, e.Param[0]);
					c_double min = Eval_Expr(p, e.Param[1]);
					c_double max = Eval_Expr(p, e.Param[2]);

					if (CMath.isnan(min) || CMath.isnan(max) || CMath.isnan(x) || (min > max))
						return c_double.NaN;

					return e.Value * Common.Av_ClipD(Eval_Expr(p, e.Param[0]), min, max);
				}

				case AvExpr.ExprType.Between:
				{
					c_double d = Eval_Expr(p, e.Param[0]);

					return e.Value * ((d >= Eval_Expr(p, e.Param[1])) && (d <= Eval_Expr(p, e.Param[2])) ? 1 : 0);
				}

				case AvExpr.ExprType.Lerp:
				{
					c_double v0 = Eval_Expr(p, e.Param[0]);
					c_double v1 = Eval_Expr(p, e.Param[1]);
					c_double f = Eval_Expr(p, e.Param[2]);

					return v0 + (v1 - v0) * f;
				}

				case AvExpr.ExprType.Print:
				{
					c_double x = Eval_Expr(p, e.Param[0]);

					c_int level = e.Param[1] != null ? Common.Av_Clip((c_int)Eval_Expr(p, e.Param[1]), c_int.MinValue, c_int.MaxValue) : Log.Av_Log_Info;
					Log.Av_Log(p, level, "%f\n", x);

					return x;
				}

				case AvExpr.ExprType.Random:
				{
					uint64_t r = Compute_Next_Random();

					return r * (1.0 / uint64_t.MaxValue);
				}

				case AvExpr.ExprType.RandomI:
				{
					c_double min = Eval_Expr(p, e.Param[1]);
					c_double max = Eval_Expr(p, e.Param[2]);

					uint64_t r = Compute_Next_Random();

					return min + (max - min) * r / uint64_t.MaxValue;
				}

				case AvExpr.ExprType.While:
				{
					c_double d = c_double.NaN;

					while (Eval_Expr(p, e.Param[0]) != 0)
						d = Eval_Expr(p, e.Param[1]);

					return d;
				}

				case AvExpr.ExprType.Taylor:
				{
					c_double t = 1, d = 0;
					c_double x = Eval_Expr(p, e.Param[1]);
					c_int id = e.Param[2] != null ? Common.Av_Clip((c_int)Eval_Expr(p, e.Param[2]), 0, Vars - 1) : 0;
					c_double var0 = p.Var[id];

					for (c_int i = 0; i < 1000; i++)
					{
						c_double ld = d;
						p.Var[id] = i;
						c_double v = Eval_Expr(p, e.Param[0]);
						d += t * v;

						if ((ld == d) && (v != 0))
							break;

						t *= x / (i + 1);
					}

					p.Var[id] = var0;

					return d;
				}

				case AvExpr.ExprType.Root:
				{
					c_double low = -1, high = -1;
					c_double low_V = -CMath.DBL_MAX, high_V = CMath.DBL_MAX;
					c_double var0 = p.Var[0];
					c_double x_Max = Eval_Expr(p, e.Param[1]);

					for (c_int i = -1; i < 1024; i++)
					{
						if (i < 255)
							p.Var[0] = Reverse.ff_Reverse[i & 255] * x_Max / 255;
						else
						{
							p.Var[0] = x_Max * CMath.pow(0.9, i - 255);

							if ((i & 1) != 0)
								p.Var[0] *= -1;

							if ((i & 2) != 0)
								p.Var[0] += low;
							else
								p.Var[0] += high;
						}

						c_double v = Eval_Expr(p, e.Param[0]);

						if ((v <= 0) && (v > low_V))
						{
							low = p.Var[0];
							low_V = v;
						}

						if ((v >= 0) && (v < high_V))
						{
							high = p.Var[0];
							high_V = v;
						}

						if ((low >= 0) && (high >= 0))
						{
							for (c_int j = 0; j < 1000; j++)
							{
								p.Var[0] = (low + high) * 0.5;

								if ((low == p.Var[0]) || (high == p.Var[0]))
									break;

								v = Eval_Expr(p, e.Param[0]);

								if (v <= 0)
									low = p.Var[0];

								if (v >= 0)
									high = p.Var[0];

								if (CMath.isnan(v))
								{
									low = high = v;
									break;
								}
							}

							break;
						}
					}

					p.Var[0] = var0;

					return -low_V < high_V ? low : high;
				}

				default:
				{
					c_double d = Eval_Expr(p, e.Param[0]);
					c_double d2 = Eval_Expr(p, e.Param[1]);

					switch (e.Type)
					{
						case AvExpr.ExprType.Mod:
							return e.Value * (d - CMath.floor(d2 != 0 ? d / d2 : d * c_double.PositiveInfinity) * d2);

						case AvExpr.ExprType.Gcd:
							return e.Value * Mathematics.Av_Gcd((int64_t)d, (int64_t)d2);

						case AvExpr.ExprType.Max:
							return e.Value * (d > d2 ? d : d2);

						case AvExpr.ExprType.Min:
							return e.Value * (d < d2 ? d : d2);

						case AvExpr.ExprType.Eq:
							return e.Value * (d == d2 ? 1.0 : 0.0);

						case AvExpr.ExprType.Gt:
							return e.Value * (d > d2 ? 1.0 : 0.0);

						case AvExpr.ExprType.Gte:
							return e.Value * (d >= d2 ? 1.0 : 0.0);

						case AvExpr.ExprType.Lt:
							return e.Value * (d < d2 ? 1.0 : 0.0);

						case AvExpr.ExprType.Lte:
							return e.Value * (d <= d2 ? 1.0 : 0.0);

						case AvExpr.ExprType.Pow:
							return e.Value * CMath.pow(d, d2);

						case AvExpr.ExprType.Mul:
							return e.Value * (d * d2);

						case AvExpr.ExprType.Div:
							return e.Value * (d2 != 0 ? (d / d2) : d * c_double.PositiveInfinity);

						case AvExpr.ExprType.Add:
							return e.Value * (d + d2);

						case AvExpr.ExprType.Last:
							return e.Value * d2;

						case AvExpr.ExprType.St:
						{
							c_int index = Common.Av_Clip((c_int)d, 0, Vars - 1);
							p.Prng_State[index].Counter = 0;

							return e.Value * (p.Var[index] = d2);
						}

						case AvExpr.ExprType.Hypot:
							return e.Value * CMath.hypot(d, d2);

						case AvExpr.ExprType.Atan2:
							return e.Value * CMath.atan2(d, d2);

						case AvExpr.ExprType.BitAnd:
							return CMath.isnan(d) || CMath.isnan(d2) ? c_double.NaN : e.Value * ((c_long)d & (c_long)d2);

						case AvExpr.ExprType.BitOr:
							return CMath.isnan(d) || CMath.isnan(d2) ? c_double.NaN : e.Value * ((c_long)d | (c_long)d2);
					}

					break;
				}
			}

			return c_double.NaN;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Primary(out AvExpr e, Parser p)//XX 369
		{
			e = null;

			AvExpr d = Mem.Av_MAlloczObj<AvExpr>();
			CPointer<char> next = p.S, s0 = p.S;
			c_int ret;

			if (d == null)
				return Error.ENOMEM;

			// Number
			d.Value = Av_Strtod(p.S, out next);
			if (next != p.S)
			{
				d.Type = AvExpr.ExprType.Value;
				p.S = next;
				e = d;

				return 0;
			}

			d.Value = 1;

			// Named constants
			for (c_int i = 0; p.Const_Names.IsNotNull && (p.Const_Names[i] != null); i++)
			{
				if (StrMatch(p.S, p.Const_Names[i]))
				{
					p.S += CString.strlen(p.Const_Names[i]);
					d.Type = AvExpr.ExprType.Const;
					d.Const_Index = i;
					e = d;

					return 0;
				}
			}

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(constants); i++)
			{
				if (StrMatch(p.S, constants[i].Name))
				{
					p.S += CString.strlen(constants[i].Name);
					d.Type = AvExpr.ExprType.Value;
					d.Value = constants[i].Value;
					e = d;

					return 0;
				}
			}

			p.S = CString.strchr(p.S, '(');

			if (p.S.IsNull)
			{
				Log.Av_Log(p, Log.Av_Log_Error, "Undefined constant or missing '(' in '%s'\n", s0);

				p.S = next;
				Mem.Av_Free(d);

				return Error.ENOMEM;
			}

			p.S++;	// "("

			if (next[0] == '(')	// Special case do-nothing
			{
				Mem.Av_FreeP(ref d);

				ret = Parse_Expr(out d, p);
				if (ret < 0)
					return ret;

				if (p.S[0] != ')')
				{
					Log.Av_Log(p, Log.Av_Log_Error, "Missing ')' in '%s'\n", s0);

					Av_Expr_Free(d);

					return Error.EINVAL;
				}

				p.S++;	// ")"
				e = d;

				return 0;
			}

			ret = Parse_Expr(out d.Param[0], p);
			if (ret < 0)
			{
				Av_Expr_Free(d);

				return ret;
			}

			if (p.S[0] == ',')
			{
				p.S++;	// ","
				Parse_Expr(out d.Param[1], p);
			}

			if (p.S[0] == ',')
			{
				p.S++;	// ","
				Parse_Expr(out d.Param[2], p);
			}

			if (p.S[0] != ')')
			{
				Log.Av_Log(p, Log.Av_Log_Error, "Missing ')' or too many args in '%s'\n", s0);

				Av_Expr_Free(d);

				return Error.EINVAL;
			}

			p.S++;	// ")"

			d.Type = AvExpr.ExprType.Func0;

			if (StrMatch(next, "sinh"))
				d.A.Func0 = CMath.sinh;
			else if (StrMatch(next, "cosh"))
				d.A.Func0 = CMath.cosh;
			else if (StrMatch(next, "tanh"))
				d.A.Func0 = CMath.tanh;
			else if (StrMatch(next, "sin"))
				d.A.Func0 = CMath.sin;
			else if (StrMatch(next, "cos"))
				d.A.Func0 = CMath.cos;
			else if (StrMatch(next, "tan"))
				d.A.Func0 = CMath.tan;
			else if (StrMatch(next, "atan"))
				d.A.Func0 = CMath.atan;
			else if (StrMatch(next, "asin"))
				d.A.Func0 = CMath.asin;
			else if (StrMatch(next, "acos"))
				d.A.Func0 = CMath.acos;
			else if (StrMatch(next, "exp"))
				d.A.Func0 = CMath.exp;
			else if (StrMatch(next, "log"))
				d.A.Func0 = CMath.log;
			else if (StrMatch(next, "abs"))
				d.A.Func0 = CMath.abs;
			else if (StrMatch(next, "time"))
				d.A.Func0 = ETime;
			else if (StrMatch(next, "squish"))
				d.Type = AvExpr.ExprType.Squish;
			else if (StrMatch(next, "gauss"))
				d.Type = AvExpr.ExprType.Gauss;
			else if (StrMatch(next, "mod"))
				d.Type = AvExpr.ExprType.Mod;
			else if (StrMatch(next, "max"))
				d.Type = AvExpr.ExprType.Max;
			else if (StrMatch(next, "min"))
				d.Type = AvExpr.ExprType.Min;
			else if (StrMatch(next, "eq"))
				d.Type = AvExpr.ExprType.Eq;
			else if (StrMatch(next, "gte"))
				d.Type = AvExpr.ExprType.Gte;
			else if (StrMatch(next, "gt"))
				d.Type = AvExpr.ExprType.Gt;
			else if (StrMatch(next, "lte"))
				d.Type = AvExpr.ExprType.Lte;
			else if (StrMatch(next, "lt"))
				d.Type = AvExpr.ExprType.Lt;
			else if (StrMatch(next, "ld"))
				d.Type = AvExpr.ExprType.Ld;
			else if (StrMatch(next, "isnan"))
				d.Type = AvExpr.ExprType.IsNan;
			else if (StrMatch(next, "isinf"))
				d.Type = AvExpr.ExprType.IsInf;
			else if (StrMatch(next, "st"))
				d.Type = AvExpr.ExprType.St;
			else if (StrMatch(next, "while"))
				d.Type = AvExpr.ExprType.While;
			else if (StrMatch(next, "taylor"))
				d.Type = AvExpr.ExprType.Taylor;
			else if (StrMatch(next, "root"))
				d.Type = AvExpr.ExprType.Root;
			else if (StrMatch(next, "floor"))
				d.Type = AvExpr.ExprType.Floor;
			else if (StrMatch(next, "ceil"))
				d.Type = AvExpr.ExprType.Ceil;
			else if (StrMatch(next, "trunc"))
				d.Type = AvExpr.ExprType.Trunc;
			else if (StrMatch(next, "round"))
				d.Type = AvExpr.ExprType.Round;
			else if (StrMatch(next, "sqrt"))
				d.Type = AvExpr.ExprType.Sqrt;
			else if (StrMatch(next, "not"))
				d.Type = AvExpr.ExprType.Not;
			else if (StrMatch(next, "pow"))
				d.Type = AvExpr.ExprType.Pow;
			else if (StrMatch(next, "print"))
				d.Type = AvExpr.ExprType.Print;
			else if (StrMatch(next, "random"))
				d.Type = AvExpr.ExprType.Random;
			else if (StrMatch(next, "randomi"))
				d.Type = AvExpr.ExprType.RandomI;
			else if (StrMatch(next, "hypot"))
				d.Type = AvExpr.ExprType.Hypot;
			else if (StrMatch(next, "gcd"))
				d.Type = AvExpr.ExprType.Gcd;
			else if (StrMatch(next, "if"))
				d.Type = AvExpr.ExprType.If;
			else if (StrMatch(next, "ifnot"))
				d.Type = AvExpr.ExprType.IfNot;
			else if (StrMatch(next, "bitand"))
				d.Type = AvExpr.ExprType.BitAnd;
			else if (StrMatch(next, "bitor"))
				d.Type = AvExpr.ExprType.BitOr;
			else if (StrMatch(next, "between"))
				d.Type = AvExpr.ExprType.Between;
			else if (StrMatch(next, "clip"))
				d.Type = AvExpr.ExprType.Clip;
			else if (StrMatch(next, "atan2"))
				d.Type = AvExpr.ExprType.Atan2;
			else if (StrMatch(next, "lerp"))
				d.Type = AvExpr.ExprType.Lerp;
			else if (StrMatch(next, "sgn"))
				d.Type = AvExpr.ExprType.Sgn;
			else
			{
				for (c_int i = 0; p.Func1_Names.IsNotNull && (p.Func1_Names[i] != null); i++)
				{
					if (StrMatch(next, p.Func1_Names[i]))
					{
						d.A.Func1 = p.Funcs1[i];
						d.Type = AvExpr.ExprType.Func1;
						d.Const_Index = i;
						e = d;

						return 0;
					}
				}

				for (c_int i = 0; p.Func2_Names.IsNotNull && (p.Func2_Names[i] != null); i++)
				{
					if (StrMatch(next, p.Func2_Names[i]))
					{
						d.A.Func2 = p.Funcs2[i];
						d.Type = AvExpr.ExprType.Func2;
						d.Const_Index = i;
						e = d;

						return 0;
					}
				}

				Log.Av_Log(p, Log.Av_Log_Error, "Unknown function in '%s'\n", s0);

				Av_Expr_Free(d);

				return Error.EINVAL;
			}

			e = d;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvExpr Make_Eval_Expr(AvExpr.ExprType type, c_int value, AvExpr p0, AvExpr p1)//XX 530
		{
			AvExpr e = Mem.Av_MAlloczObj<AvExpr>();
			if (e == null)
				return null;

			e.Type = type;
			e.Value = value;
			e.Param[0] = p0;
			e.Param[1] = p1;

			return e;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Pow(out AvExpr e, Parser p, out c_int sign)//XX 542
		{
			sign = (p.S[0] == '+' ? 1 : 0) - (p.S[0] == '-' ? 1 : 0);
			p.S += sign & 1;

			return Parse_Primary(out e, p);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_dB(out AvExpr e, Parser p, out c_int sign)//XX 549
		{
			// Do not filter out the negative sign when parsing a dB value.
			// For example, -3dB is not the same as -(3dB)
			if (p.S[0] == '-')
			{
				c_double ignored = CString.strtod(p.S, out CPointer<char> next);

				if ((next != p.S) && (next[0] == 'd') && (next[1] == 'B'))
				{
					sign = 0;

					return Parse_Primary(out e, p);
				}
			}

			return Parse_Pow(out e, p, out sign);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Factor(out AvExpr e, Parser p)//XX 564
		{
			e = null;

			c_int ret = Parse_dB(out AvExpr e0, p, out c_int sign);
			if (ret < 0)
				return ret;

			while (p.S[0] == '^')
			{
				AvExpr e1 = e0;
				p.S++;

				ret = Parse_dB(out AvExpr e2, p, out c_int sign2);
				if (ret < 0)
				{
					Av_Expr_Free(e1);
					return ret;
				}

				e0 = Make_Eval_Expr(AvExpr.ExprType.Pow, 1, e1, e2);
				if (e0 == null)
				{
					Av_Expr_Free(e1);
					Av_Expr_Free(e2);

					return Error.ENOMEM;
				}

				if (e0.Param[1] != null)
					e0.Param[1].Value *= (sign2 | 1);
			}

			if (e0 != null)
				e0.Value *= (sign | 1);

			e = e0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Term(out AvExpr e, Parser p)//XX 591
		{
			e = null;

			c_int ret = Parse_Factor(out AvExpr e0, p);
			if (ret < 0)
				return ret;

			while ((p.S[0] == '*') || (p.S[0] == '/'))
			{
				c_int c = p.S[0, 1];
				AvExpr e1 = e0;

				ret = Parse_Term(out AvExpr e2, p);
				if (ret < 0)
				{
					Av_Expr_Free(e1);
					return ret;
				}

				e0 = Make_Eval_Expr(c == '*' ? AvExpr.ExprType.Mul : AvExpr.ExprType.Div, 1, e1, e2);
				if (e0 == null)
				{
					Av_Expr_Free(e1);
					Av_Expr_Free(e2);

					return Error.ENOMEM;
				}
			}

			e = e0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_SubExpr(out AvExpr e, Parser p)//XX 615
		{
			e = null;

			c_int ret = Parse_Term(out AvExpr e0, p);
			if (ret < 0)
				return ret;

			while ((p.S[0] == '+') || (p.S[0] == '-'))
			{
				AvExpr e1 = e0;

				ret = Parse_Term(out AvExpr e2, p);
				if (ret < 0)
				{
					Av_Expr_Free(e1);
					return ret;
				}

				e0 = Make_Eval_Expr(AvExpr.ExprType.Add, 1, e1, e2);
				if (e0 == null)
				{
					Av_Expr_Free(e1);
					Av_Expr_Free(e2);

					return Error.ENOMEM;
				}
			}

			e = e0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Expr(out AvExpr e, Parser p)//XX 639
		{
			e = null;

			if (p.Stack_Index <= 0)	// Protect against stack overflow
				return Error.EINVAL;

			p.Stack_Index--;

			c_int ret = Parse_SubExpr(out AvExpr e0, p);
			if (ret < 0)
				return ret;

			while (p.S[0] == ';')
			{
				p.S++;

				AvExpr e1 = e0;

				ret = Parse_SubExpr(out AvExpr e2, p);
				if (ret < 0)
				{
					Av_Expr_Free(e1);
					return ret;
				}

				e0 = Make_Eval_Expr(AvExpr.ExprType.Last, 1, e1, e2);
				if (e0 == null)
				{
					Av_Expr_Free(e1);
					Av_Expr_Free(e2);

					return Error.ENOMEM;
				}
			}

			p.Stack_Index++;
			e = e0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Verify_Expr(AvExpr e)//XX 669
		{
			if (e == null)
				return false;

			switch (e.Type)
			{
				case AvExpr.ExprType.Value:
				case AvExpr.ExprType.Const:
					return true;

				case AvExpr.ExprType.Func0:
				case AvExpr.ExprType.Func1:
				case AvExpr.ExprType.Squish:
				case AvExpr.ExprType.Ld:
				case AvExpr.ExprType.Gauss:
				case AvExpr.ExprType.IsNan:
				case AvExpr.ExprType.IsInf:
				case AvExpr.ExprType.Floor:
				case AvExpr.ExprType.Ceil:
				case AvExpr.ExprType.Trunc:
				case AvExpr.ExprType.Round:
				case AvExpr.ExprType.Sqrt:
				case AvExpr.ExprType.Not:
				case AvExpr.ExprType.Random:
				case AvExpr.ExprType.Sgn:
					return Verify_Expr(e.Param[0]) && (e.Param[1] == null);

				case AvExpr.ExprType.Print:
					return Verify_Expr(e.Param[0]) && ((e.Param[1] == null) || Verify_Expr(e.Param[1]));

				case AvExpr.ExprType.If:
				case AvExpr.ExprType.IfNot:
				case AvExpr.ExprType.Taylor:
					return Verify_Expr(e.Param[0]) && Verify_Expr(e.Param[1]) && ((e.Param[2] == null) || Verify_Expr(e.Param[2]));

				case AvExpr.ExprType.Between:
				case AvExpr.ExprType.Clip:
				case AvExpr.ExprType.Lerp:
				case AvExpr.ExprType.RandomI:
					return Verify_Expr(e.Param[0]) && Verify_Expr(e.Param[1]) && Verify_Expr(e.Param[2]);

				default:
					return Verify_Expr(e.Param[0]) && Verify_Expr(e.Param[1]) && (e.Param[2] == null);
			}
		}
		#endregion
	}
}
