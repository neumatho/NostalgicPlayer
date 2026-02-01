/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Tx
	{
		private static readonly FFTxCodelet ff_Tx_Null_Def = new FFTxCodelet();

		private static readonly FFTxCodelet[] ff_Tx_Null_List =
		[
			ff_Tx_Null_Def
		];

		private static readonly FFTxCodelet[][] codelet_List =
		[
			Tx_Float.ff_Tx_Codelet_List_Float_C,
			Tx_Double.ff_Tx_Codelet_List_Double_C,
			Tx_Int32.ff_Tx_Codelet_List_Int32_C,
			ff_Tx_Null_List
		];

		private static readonly c_int codelet_List_Num = (c_int)Macros.FF_Array_Elems(codelet_List);

		private static readonly AvCpuFlag cpu_Slow_Mask = 0;

		private static readonly c_int[][] cpu_Slow_Penalties =
		[
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		static Tx()
		{
			ff_Tx_Null_Def.Name = "null".ToCharPointer();
			ff_Tx_Null_Def.Function = FF_Tx_Null;
			ff_Tx_Null_Def.Type = AvTxType.Any;
			ff_Tx_Null_Def.Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Aligned | AvTxFlags.FF_Out_Of_Place | AvTxFlags.Inplace;
			ff_Tx_Null_Def.Factors[0] = UtilConstants.Tx_Factor_Any;
			ff_Tx_Null_Def.Min_Len = 1;
			ff_Tx_Null_Def.Max_Len = 1;
			ff_Tx_Null_Def.Init = FF_Tx_Null_Init;
			ff_Tx_Null_Def.Cpu_Flags = AvCpuFlag.FF_All;
			ff_Tx_Null_Def.Prio = FFTxCodeletPriority.Max;
		}



		/********************************************************************/
		/// <summary>
		/// This function generates a Ruritanian PFA input map into s.Map
		/// </summary>
		/********************************************************************/
		public static c_int FF_Tx_Gen_Pfa_Input_Map(AvTxContext s, FFTxCodeletOptions opts, c_int d1, c_int d2)//XX 44
		{
			c_int sl = d1 * d2;

			s.Map = Mem.Av_MAlloc<c_int>((size_t)s.Len);

			if (s.Map.IsNull)
				return Error.ENOMEM;

			for (c_int k = 0; k < s.Len; k += sl)
			{
				if ((s.Inv != 0) || ((opts != null) && (opts.Map_Dir == FFTxMapDirection.Scatter)))
				{
					for (c_int m = 0; m < d2; m++)
					{
						for (c_int n = 0; n < d1; n++)
							s.Map[k + (((m * d1) + (n * d2)) % sl)] = (m * d1) + n;
					}
				}
				else
				{
					for (c_int m = 0; m < d2; m++)
					{
						for (c_int n = 0; n < d1; n++)
							s.Map[k + (m * d1) + n] = ((m * d1) + (n * d2)) % sl;
					}
				}

				if (s.Inv != 0)
				{
					for (c_int w = 1; w <= (sl >> 1); w++)
						Macros.FFSwap(ref s.Map[k + w], ref s.Map[k + sl - w]);
				}
			}

			s.Map_Dir = opts != null ? opts.Map_Dir : FFTxMapDirection.Gather;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Generates the PFA permutation table into AVTXContext->pfatab. The
		/// end table is appended to the start table. The `inv` flag should
		/// only be enabled if the lookup tables of subtransforms won't get
		/// flattened
		/// </summary>
		/********************************************************************/
		public static c_int FF_Tx_Gen_Compound_Mapping(AvTxContext s, FFTxCodeletOptions opts, c_int inv, c_int n, c_int m)//XX 75
		{
			c_int len = n * m;		// Will not be equal to s->len for MDCTs

			// Make sure the numbers are coprime
			if (Mathematics.Av_Gcd(n, m) != 1)
				return Error.EINVAL;

			c_int m_Inv = MulInv(m, n);
			c_int n_Inv = MulInv(n, m);

			s.Map = Mem.Av_MAlloc<c_int>((size_t)(2 * len));

			if (s.Map.IsNull)
				return Error.ENOMEM;

			CPointer<c_int> in_Map = s.Map;
			CPointer<c_int> out_Map = s.Map + len;

			// Ruritanian map for input, CRT map for output, can be swapped
			if ((opts != null) && (opts.Map_Dir == FFTxMapDirection.Scatter))
			{
				for (c_int j = 0; j < m; j++)
				{
					for (c_int i = 0; i < n; i++)
					{
						in_Map[((i * m) + (j * n)) % len] = (j * n) + i;
						out_Map[((i * m * m_Inv) + (j * n * n_Inv)) % len] = (i * m) + j;
					}
				}
			}
			else
			{
				for (c_int j = 0; j < m; j++)
				{
					for (c_int i = 0; i < n; i++)
					{
						in_Map[(j * n) + i] = ((i * m) + (j * n)) % len;
						out_Map[((i * m * m_Inv) + (j * n * n_Inv)) % len] = (i * m) + j;
					}
				}
			}

			if (inv != 0)
			{
				for (c_int i = 0; i < m; i++)
				{
					CPointer<c_int> @in = in_Map + (i * n) + 1;	// Skip the DC

					for (c_int j = 0; j < ((n - 1) >> 1); j++)
						Macros.FFSwap(ref @in[j], ref @in[n - j - 2]);
				}
			}

			s.Map_Dir = opts != null ? opts.Map_Dir : FFTxMapDirection.Gather;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Tx_Gen_PTwo_RevTab(AvTxContext s, FFTxCodeletOptions opts)//XX 136
		{
			c_int len = s.Len;

			s.Map = Mem.Av_MAlloc<c_int>((size_t)len);

			if (s.Map.IsNull)
				return Error.ENOMEM;

			if ((opts != null) && (opts.Map_Dir == FFTxMapDirection.Scatter))
			{
				for (c_int i = 0; i < s.Len; i++)
					s.Map[-Split_Radix_Permutation(i, len, s.Inv) & (len - 1)] = i;
			}
			else
			{
				for (c_int i = 0; i < s.Len; i++)
					s.Map[i] = -Split_Radix_Permutation(i, len, s.Inv) & (len - 1);
			}

			s.Map_Dir = (opts != null) ? opts.Map_Dir : FFTxMapDirection.Gather;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Generates an index into AVTXContext->inplace_idx that if followed
		/// in the specific order, allows the revtab to be done in-place.
		/// The sub-transform and its map should already be initialized
		/// </summary>
		/********************************************************************/
		public static c_int FF_Tx_Gen_Inplace_Map(AvTxContext s, c_int len)//XX 156
		{
			c_int out_Map_Idx = 0;

			if (s.Sub.IsNull || s.Sub[0].Map.IsNull)
				return Error.EINVAL;

			s.Map = Mem.Av_MAllocz<c_int>((size_t)len);

			if (s.Map.IsNull)
				return Error.ENOMEM;

			CPointer<c_int> src_Map = s.Sub[0].Map;

			// The first coefficient is always already in-place
			for (c_int src = 1; src < s.Len; src++)
			{
				c_int dst = src_Map[src];
				c_int found = 0;

				if (dst <= src)
					continue;

				// This just checks if a closed loop has been encountered before,
				// and if so, skips it, since to fully permute a loop we must only
				// enter it once
				do
				{
					for (c_int j = 0; j < out_Map_Idx; j++)
					{
						if (dst == s.Map[j])
						{
							found = 1;
							break;
						}
					}

					dst = src_Map[dst];
				}
				while ((dst != src) && (found == 0));

				if (found == 0)
					s.Map[out_Map_Idx++] = src;
			}

			s.Map[out_Map_Idx++] = 0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Clear the context by freeing all tables, maps and subtransforms
		/// </summary>
		/********************************************************************/
		public static void FF_Tx_Clear_Ctx(AvTxContext s)//XX 290
		{
			Reset_Ctx(s, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Frees a context and sets *ctx to NULL, does nothing when
		/// *ctx == NULL
		/// </summary>
		/********************************************************************/
		public static void Av_Tx_Uninit(ref AvTxContext ctx)//XX 295
		{
			if (ctx == null)
				return;

			Reset_Ctx(ctx, 1);

			Mem.Av_FreeP(ref ctx);
		}



		/********************************************************************/
		/// <summary>
		/// Attempt to factorize a length into 2 integers such that
		/// len / dst1 == dst2, where dst1 and dst2 are coprime
		/// </summary>
		/********************************************************************/
		public static c_int FF_Tx_Decompose_Length(CPointer<c_int> dst, AvTxType type, c_int len, c_int inv)//XX 412
		{
			c_int nb_Decomp = 0;
			FFTxLenDecomp[] ld = new FFTxLenDecomp[UtilConstants.Tx_Max_Decompositions];
			c_int codelet_List_Idx = codelet_List_Num;

			AvCpuFlag cpu_Flags = Cpu.Av_Get_Cpu_Flags();

			// Loop through all codelets in all codelet lists to find matches
			// to the requirements
			while (codelet_List_Idx-- != 0)
			{
				FFTxCodelet[] list = codelet_List[codelet_List_Idx];

				foreach (FFTxCodelet cd in list)
				{
					c_int fl = len;
					c_int skip = 0;
					c_int factors_Product = 1, factors_Mod = 0;

					if (nb_Decomp >= UtilConstants.Tx_Max_Decompositions)
						goto Sort;

					// Check if the type matches
					if ((cd.Type != AvTxType.Any) && (type != cd.Type))
						continue;

					// Check direction for non-orthogonal codelets
					if ((((cd.Flags & AvTxFlags.FF_Forward_Only) != 0) && (inv != 0)) || (((cd.Flags & (AvTxFlags.FF_Inverse_Only | AvTxFlags.Full_Imdct)) != 0) && (inv == 0)) ||
						(((cd.Flags & (AvTxFlags.FF_Forward_Only | AvTxFlags.Real_To_Real)) != 0) && (inv != 0)) || (((cd.Flags & (AvTxFlags.FF_Forward_Only | AvTxFlags.Real_To_Imaginary)) != 0) && (inv != 0)))
		                continue;

					// Check if the CPU supports the required ISA
					if ((cd.Cpu_Flags != AvCpuFlag.FF_All) && ((cpu_Flags & (cd.Cpu_Flags & ~cpu_Slow_Mask)) == 0))
						continue;

					for (c_int i = 0; i < UtilConstants.Tx_Max_Factors; i++)
					{
						if ((cd.Factors[i] == 0) || (fl == 1))
							break;

						if (cd.Factors[i] == UtilConstants.Tx_Factor_Any)
						{
							factors_Mod++;
							factors_Product *= fl;
						}
						else if ((fl % cd.Factors[i]) == 0)
						{
							factors_Mod++;

							if (cd.Factors[i] == 2)
							{
								c_int b = IntMath.FF_Ctz(fl);
								fl >>= b;
								factors_Product <<= b;
							}
							else
							{
								do
								{
									fl /= cd.Factors[i];
									factors_Product *= cd.Factors[i];
								}
								while ((fl % cd.Factors[i]) == 0);
							}
						}
					}

					// Disqualify if factor requirements are not satisfied or if trivial
					if ((factors_Mod < cd.Nb_Factors) || (len == factors_Product))
						continue;

					if (Mathematics.Av_Gcd(factors_Product, fl) != 1)
						continue;

					// Check if length is supported and factorization was successful
					if ((factors_Product < cd.Min_Len) || ((cd.Max_Len != UtilConstants.Tx_Len_Unlimited) && (factors_Product > cd.Max_Len)))
						continue;

					c_int prio = Get_Codelet_Prio(cd, cpu_Flags, factors_Product) * factors_Product;

					// Check for duplicates
					for (c_int i = 0; i < nb_Decomp; i++)
					{
						if (factors_Product == ld[i].Len)
						{
							// Update priority if new one is higher
							if (prio > ld[i].Prio)
								ld[i].Prio = prio;

							skip = 1;
							break;
						}
					}

					// Add decomposition if unique
					if (skip == 0)
					{
						ld[nb_Decomp].Cd = cd;
						ld[nb_Decomp].Len = factors_Product;
						ld[nb_Decomp].Len2 = fl;
						ld[nb_Decomp].Prio = prio;

						nb_Decomp++;
					}
				}
			}

			if (nb_Decomp == 0)
				return Error.EINVAL;

			Sort:
			QSort.Av_QSort<FFTxLenDecomp>(ld, nb_Decomp, Cmp_Decomp);

			for (c_int i = 0; i < nb_Decomp; i++)
			{
				if (ld[i].Cd.Nb_Factors > 1)
					dst[i] = ld[i].Len2;
				else
					dst[i] = ld[i].Len;
			}

			return nb_Decomp;
		}



		/********************************************************************/
		/// <summary>
		/// Generate a default map (0->len or 0, (len-1)->1 for inverse
		/// transforms) for a context
		/// </summary>
		/********************************************************************/
		public static c_int FF_Tx_Gen_Default_Map(AvTxContext s, FFTxCodeletOptions opts)//XX 525
		{
			s.Map = Mem.Av_MAlloc<c_int>((size_t)s.Len);

			if (s.Map.IsNull)
				return Error.ENOMEM;

			s.Map[0] = 0;	// DC is always at the start

			if (s.Inv != 0)	// Reversing the ACs flips the transform direction
			{
				for (c_int i = 1; i < s.Len; i++)
					s.Map[i] = s.Len - i;
			}
			else
			{
				for (c_int i = 1; i < s.Len; i++)
					s.Map[i] = i;
			}

			s.Map_Dir = FFTxMapDirection.Gather;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Create a subtransform in the current context with the given
		/// parameters. The flags parameter from FFTXCodelet.init() should be
		/// preserved as much as that's possible.
		/// MUST be called during the sub() callback of each codelet
		/// </summary>
		/********************************************************************/
		public static c_int FF_Tx_Init_SubTx<T>(AvTxContext s, AvTxType type, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, T scale) where T : INumber<T> //XX 712
		{
			c_int ret = 0;
			CPointer<AvTxContext> sub = null;
			CPointer<TxCodeletMatch> cd_Matches = null;
			c_uint cd_Matches_Size = 0;
			c_int codelet_List_Idx = codelet_List_Num;
			c_int nb_Cd_Matches = 0;

			// We still accept functions marked with SLOW, even if the CPU is
			// marked with the same flag, but we give them lower priority
			AvCpuFlag cpu_Flags = Cpu.Av_Get_Cpu_Flags();

			// Flags the transform wants
			AvTxFlags req_Flags = flags;

			// Flags the codelet may require to be present
			AvTxFlags inv_Req_Flags = AvTxFlags.Full_Imdct | AvTxFlags.Real_To_Real | AvTxFlags.Real_To_Imaginary | AvTxFlags.FF_Preshuffle | AvTxFlags.FF_Asm_Call;

			// Unaligned codelets are compatible with the aligned flag
			if ((req_Flags & AvTxFlags.FF_Aligned) != 0)
				req_Flags |= AvTxFlags.Unaligned;

			// If either flag is set, both are okay, so don't check for an exact match
			if (((req_Flags & AvTxFlags.Inplace) != 0) && ((req_Flags & AvTxFlags.FF_Out_Of_Place) != 0))
				req_Flags &= ~(AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place);

			if (((req_Flags & AvTxFlags.FF_Aligned) != 0) && ((req_Flags & AvTxFlags.Unaligned) != 0))
				req_Flags &= ~(AvTxFlags.FF_Aligned | AvTxFlags.Unaligned);

			// Loop through all codelets in all codelet lists to find matches
			// to the requirements
			while (codelet_List_Idx-- != 0)
			{
				FFTxCodelet[] list = codelet_List[codelet_List_Idx];

				foreach (FFTxCodelet cd in list)
				{
					// Check if the type matches
					if ((cd.Type != AvTxType.Any) && (type != cd.Type))
						continue;

					// Check direction for non-orthogonal codelets
					if ((((cd.Flags & AvTxFlags.FF_Forward_Only) != 0) && (inv != 0)) ||
					    (((cd.Flags & (AvTxFlags.FF_Inverse_Only | AvTxFlags.Full_Imdct)) != 0) && (inv == 0)) ||
					    (((cd.Flags & (AvTxFlags.FF_Forward_Only | AvTxFlags.Real_To_Real)) != 0) && (inv != 0)) ||
					    (((cd.Flags & (AvTxFlags.FF_Forward_Only | AvTxFlags.Real_To_Imaginary)) != 0) && (inv != 0)))
						continue;

					// Check if the requested flags match from both sides
					if (((req_Flags & cd.Flags) != req_Flags) || ((inv_Req_Flags & cd.Flags) != (req_Flags & inv_Req_Flags)))
						continue;

					// Check if length is supported
					if ((len < cd.Min_Len) || ((cd.Max_Len != -1) && (len > cd.Max_Len)))
						continue;

					// Check if the CPU supports the required ISA
					if ((cd.Cpu_Flags != AvCpuFlag.FF_All) && ((cpu_Flags & (cd.Cpu_Flags & ~cpu_Slow_Mask)) == 0))
						continue;

					// Check for factors
					if (Check_Cd_Factors(cd, len) == 0)
						continue;

					// Realloc array and append
					CPointer<TxCodeletMatch> cd_Tmp = Mem.Av_Fast_ReallocObj(cd_Matches, ref cd_Matches_Size, (size_t)nb_Cd_Matches + 1);

					if (cd_Tmp.IsNull)
					{
						Mem.Av_Free(cd_Matches);

						return Error.ENOMEM;
					}

					cd_Matches = cd_Tmp;
					cd_Matches[nb_Cd_Matches].Cd = cd;
					cd_Matches[nb_Cd_Matches].Prio = Get_Codelet_Prio(cd, cpu_Flags, len);

					nb_Cd_Matches++;
				}
			}

			if (nb_Cd_Matches == 0)
				return Error.ENOSYS;

			// Sort the list
			QSort.Av_QSort(cd_Matches, nb_Cd_Matches, Cmp_Matches);

			if (s.Sub.IsNull)
			{
				s.Sub = sub = Mem.Av_MAlloczObj<AvTxContext>(UtilConstants.Tx_Max_Sub);

				if (sub.IsNull)
				{
					ret = Error.ENOMEM;

					goto End;
				}
			}

			// Attempt to initialize each
			for (c_int i = 0; i < nb_Cd_Matches; i++)
			{
				FFTxCodelet cd = cd_Matches[i].Cd;
				AvTxContext sCtx = s.Sub[s.Nb_Sub];

				sCtx.Len = len;
				sCtx.Inv = inv;
				sCtx.Type = type;
				sCtx.Flags = cd.Flags | flags;
				sCtx.Cd_Self = cd;

				s.Fn[s.Nb_Sub] = cd.Function;
				s.Cd[s.Nb_Sub] = cd;

				ret = 0;

				if (cd.Init != null)
				{
					object o = scale;
					ret = cd.Init(sCtx, cd, flags, opts, len, inv, o);
					scale = (T)o;
				}

				if (ret >= 0)
				{
					if ((opts != null) && (opts.Map_Dir != FFTxMapDirection.None) && (sCtx.Map_Dir == FFTxMapDirection.None))
					{
						// If a specific map direction was requested, and it doesn't
						// exist, create one
						sCtx.Map = Mem.Av_MAlloc<c_int>((size_t)len);

						if (sCtx.Map.IsNull)
						{
							ret = Error.ENOMEM;

							goto End;
						}

						for (c_int i_ = 0; i_ < len; i_++)
							sCtx.Map[i_] = i_;
					}
					else if ((opts != null) && (opts.Map_Dir != sCtx.Map_Dir))
					{
						CPointer<c_int> tmp = Mem.Av_MAlloc<c_int>((size_t)len);

						if (tmp.IsNull)
						{
							ret = Error.ENOMEM;

							goto End;
						}

						CMemory.memcpy(tmp, sCtx.Map, (size_t)len);

						for (c_int i_ = 0; i_ < len; i_++)
							sCtx.Map[tmp[i_]] = i_;

						Mem.Av_Free(tmp);
					}

					s.Nb_Sub++;

					goto End;
				}

				s.Fn[s.Nb_Sub] = null;
				s.Cd[s.Nb_Sub] = null;

				Reset_Ctx(sCtx, 0);

				if (ret == Error.ENOMEM)
					break;
			}

			if (s.Nb_Sub == 0)
				Mem.Av_FreeP(ref s.Sub);

			End:
			Mem.Av_Free(cd_Matches);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a transform context with the given configuration
		/// (i)MDCTs with an odd length are currently not supported
		/// </summary>
		/********************************************************************/
		public static c_int Av_Tx_Init<T>(out AvTxContext ctx, out UtilFunc.Av_Tx_Fn tx, AvTxType type, c_int inv, c_int len, ref T scale, AvTxFlags flags) where T : INumber<T>//XX 903
		{
			return Av_Tx_Init(out ctx, out tx, type, inv, len, ref scale, true, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a transform context with the given configuration
		/// (i)MDCTs with an odd length are currently not supported
		/// </summary>
		/********************************************************************/
		public static c_int Av_Tx_Init<T>(out AvTxContext ctx, out UtilFunc.Av_Tx_Fn tx, AvTxType type, c_int inv, c_int len, AvTxFlags flags) where T : INumber<T>//XX 903
		{
			T scale = T.Zero;

			return Av_Tx_Init(out ctx, out tx, type, inv, len, ref scale, false, flags);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Type_Is(AvTxType type, params AvTxType[] x)//XX 28
		{
			return x.Contains(type);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the modular multiplicative inverse
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int MulInv(c_int n, c_int m)//XX 34
		{
			n = n % m;

			for (c_int x = 1; x < m; x++)
			{
				if (((n * x) % m) == 1)
					return x;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int Split_Radix_Permutation(c_int i, c_int len, c_int inv)//XX 125
		{
			len >>= 1;

			if (len <= 1)
				return i & 1;

			if ((i & len) == 0)
				return Split_Radix_Permutation(i, len, inv) * 2;

			len >>= 1;

			return (Split_Radix_Permutation(i, len, inv) * 4) + 1 - (2 * (((i & len) == 0 ? 1 : 0) ^ inv));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Reset_Ctx(AvTxContext s, c_int free_Sub)//XX 264
		{
			if (s == null)
				return;

			if (s.Sub.IsNotNull)
			{
				for (c_int i = 0; i < UtilConstants.Tx_Max_Sub; i++)
					Reset_Ctx(s.Sub[i], free_Sub + 1);
			}

			if ((s.Cd_Self != null) && (s.Cd_Self.Uninit != null))
				s.Cd_Self.Uninit(s);

			if (free_Sub != 0)
				Mem.Av_FreeP(ref s.Sub);

			Mem.Av_FreeP(ref s.Map);
			Mem.Av_FreeP(ref s.Exp);
			Mem.Av_FreeP(ref s.Tmp);

			// Nothing else needs to be reset, it gets overwritten if another
			// ff_tx_init_subtx() call is made
			s.Nb_Sub = 0;
			s.Opaque = null;

			CMemory.memset<UtilFunc.Av_Tx_Fn>(s.Fn, null, (size_t)s.Fn.Length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int FF_Tx_Null_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 304
		{
			// Can only handle one sample+type to one sample+type transforms
			if (Type_Is(s.Type, AvTxType.Double_Mdct, AvTxType.Float_Mdct, AvTxType.Int32_Mdct) || Type_Is(s.Type, AvTxType.Double_Rdft, AvTxType.Float_Rdft, AvTxType.Int32_Rdft))
				return Error.EINVAL;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Null transform when the length is 1
		/// </summary>
		/********************************************************************/
		private static void FF_Tx_Null(AvTxContext s, IPointer @out, IPointer @in, ptrdiff_t stride)//XX 315
		{
			CMemory.memcpy(@out, @in, 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Codelet_Prio(FFTxCodelet cd, AvCpuFlag cpu_Flags, c_int len)//XX 367
		{
			c_int prio = cd.Prio;
			c_int max_Factor = 0;

			// If the CPU has a SLOW flag, and the instruction is also flagged
			// as being slow for such, reduce its priority
			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(cpu_Slow_Penalties); i++)
			{
				if (((cpu_Flags & cd.Cpu_Flags) & (AvCpuFlag)cpu_Slow_Penalties[i][0]) != 0)
					prio -= cpu_Slow_Penalties[i][1];
			}

			// Prioritize aligned-only codelets
			if (((cd.Flags & AvTxFlags.FF_Aligned) != 0) && ((cd.Flags & AvTxFlags.Unaligned) == 0))
				prio += 64;

			// Codelets for specific lengths are generally faster
			if ((len == cd.Min_Len) && (len == cd.Max_Len))
				prio += 64;

			// Forward-only or inverse-only transforms are generally better
			if ((cd.Flags & (AvTxFlags.FF_Forward_Only | AvTxFlags.FF_Inverse_Only)) != 0)
				prio += 64;

			// Larger factors are generally better
			for (c_int i = 0; i < UtilConstants.Tx_Max_Sub; i++)
				max_Factor = Macros.FFMax(cd.Factors[i], max_Factor);

			if (max_Factor != 0)
				prio += 16 * max_Factor;

			return prio;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Cmp_Decomp(FFTxLenDecomp a, FFTxLenDecomp b)//XX 407
		{
			return Macros.FFDiffSign(b.Prio, a.Prio);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Cmp_Matches(TxCodeletMatch a, TxCodeletMatch b)//XX 670
		{
			return Macros.FFDiffSign(b.Prio, a.Prio);
		}



		/********************************************************************/
		/// <summary>
		/// We want all factors to completely cover the length
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int Check_Cd_Factors(FFTxCodelet cd, c_int len)//XX 676
		{
			c_int matches = 0, any_Flag = 0;

			for (c_int i = 0; i < UtilConstants.Tx_Max_Factors; i++)
			{
				c_int factor = cd.Factors[i];

				if (factor == UtilConstants.Tx_Factor_Any)
				{
					any_Flag = 1;
					matches++;

					continue;
				}
				else if ((len <= 1) || (factor == 0))
					break;
				else if (factor == 2)	// Fast path
				{
					c_int bits_2 = IntMath.FF_Ctz(len);

					if (bits_2 == 0)
						continue;	// Factor not supported

					len >>= bits_2;
					matches++;
				}
				else
				{
					c_int res = len % factor;

					if (res != 0)
						continue;	// Factor not supported

					while (res == 0)
					{
						len /= factor;
						res = len % factor;
					}

					matches++;
				}
			}

			return (cd.Nb_Factors <= matches) && ((any_Flag != 0) || (len == 1)) ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a transform context with the given configuration
		/// (i)MDCTs with an odd length are currently not supported
		/// </summary>
		/********************************************************************/
		private static c_int Av_Tx_Init<T>(out AvTxContext ctx, out UtilFunc.Av_Tx_Fn tx, AvTxType type, c_int inv, c_int len, ref T scale, bool hasScale, AvTxFlags flags) where T : INumber<T>//XX 903
		{
			ctx = null;
			tx = null;

			AvTxContext tmp = new AvTxContext();
			const c_double default_Scale_D = 1.0;
			const c_float default_Scale_F = 1.0f;

			if ((len == 0) || (type >= AvTxType.Nb))
				return Error.EINVAL;

			if ((flags & AvTxFlags.Unaligned) == 0)
				flags |= AvTxFlags.FF_Aligned;

			if ((flags & AvTxFlags.Inplace) == 0)
				flags |= AvTxFlags.FF_Out_Of_Place;

			if (!hasScale && ((type == AvTxType.Double_Mdct) || (type == AvTxType.Double_Dct) || (type == AvTxType.Double_Dct_I) || (type == AvTxType.Double_Dst_I) || (type == AvTxType.Double_Rdft)))
				scale = T.CreateChecked(default_Scale_D);
			else if (!hasScale && !Type_Is(type, AvTxType.Double_Fft, AvTxType.Float_Fft, AvTxType.Int32_Fft))
				scale = T.CreateChecked(default_Scale_F);

			c_int ret = FF_Tx_Init_SubTx(tmp, type, flags, null, len, inv, scale);

			if (ret < 0)
				return ret;

			ctx = tmp.Sub[0];
			tx = tmp.Fn[0];

			return ret;
		}
		#endregion
	}
}
