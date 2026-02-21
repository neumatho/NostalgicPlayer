/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Vlc_
	{
		/// <summary>
		/// The maximum currently needed is 1296 by rv34
		/// </summary>
		private const c_int LocalBuf_Elems = 1500;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Vlc_Init<TBits, TCodes>(Vlc vlc, c_int nb_Bits, c_int nb_Codes, CPointer<TBits> bits, CPointer<TCodes> codes, VlcInit flags) where TBits : INumber<TBits> where TCodes : INumber<TCodes>
		{
			return FF_Vlc_Init_Sparse<TBits, TCodes, c_int>(vlc, nb_Bits, nb_Codes, bits, codes, null, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Build VLC decoding tables suitable for use with get_vlc2()
		/// </summary>
		/********************************************************************/
		public static c_int FF_Vlc_Init_Sparse<TBits, TCodes, TSymbols>(Vlc vlc, c_int nb_Bits, c_int nb_Codes, CPointer<TBits> bits, CPointer<TCodes> codes, CPointer<TSymbols> symbols, VlcInit flags) where TBits : INumber<TBits> where TCodes : INumber<TCodes> where TSymbols : INumber<TSymbols> //XX 250
		{
			VlcCode[] localBuf = ArrayHelper.InitializeArray<VlcCode>(LocalBuf_Elems);
			CPointer<VlcCode> buf = localBuf;

			c_int ret = Vlc_Common_Init(vlc, nb_Bits, nb_Codes, ref buf, flags);

			if (ret < 0)
				return ret;

			c_int j = 0;

			c_int Copy(Func<c_uint, bool> condition)
			{
				for (c_int i = 0; i < nb_Codes; i++)
				{
					Get_Data(out TBits l, bits, i);
					c_uint len = c_uint.CreateChecked(l);

					if (!condition(len))
						continue;

					if ((len > (3 * nb_Bits)) || (len > 32))
					{
						Log.Av_Log(null, Log.Av_Log_Error, "Too long VLC (%u) in vlc_init\n", len);

						if (buf != localBuf)
							Mem.Av_Free(buf);

						return Error.EINVAL;
					}

					buf[j].Bits = (uint8_t)len;

					Get_Data(out TCodes c, codes, i);
					buf[j].Code = c_uint.CreateChecked(c);

					if (buf[j].Code >= (1L << buf[j].Bits))
					{
						Log.Av_Log(null, Log.Av_Log_Error, "Invalid code %lld for %d in vlc_init\n", buf[j].Code, i);

						if (buf != localBuf)
							Mem.Av_Free(buf);

						return Error.EINVAL;
					}

					if ((flags & VlcInit.Input_Le) != 0)
						buf[j].Code = BitSwap_32(buf[j].Code);
					else
						buf[j].Code <<= 32 - buf[j].Bits;

					if (symbols.IsNotNull)
					{
						Get_Data(out TSymbols s, symbols, i);
						buf[j].Symbol = VlcBaseType.CreateChecked(s);
					}
					else
						buf[j].Symbol = (VlcBaseType)i;

					j++;
				}

				return 0;
			}

			Copy((len) => len > (c_uint)nb_Bits);

			// qsort is the slowest part of vlc_init, and could probably be improved or avoided
			QSort.Av_QSort<VlcCode>(buf, j, Compare_VlcSpec);

			Copy((len) => (len != 0) && (len <= nb_Bits));

			nb_Codes = j;

			return Vlc_Common_End(vlc, nb_Bits, nb_Codes, buf, flags, localBuf);
		}



		/********************************************************************/
		/// <summary>
		/// Build VLC decoding tables suitable for use with get_vlc2()
		///
		/// This function takes lengths and symbols and calculates the codes
		/// from them. For this the input lengths and symbols have to be
		/// sorted according to "left nodes in the corresponding tree first"
		/// </summary>
		/********************************************************************/
		public static c_int FF_Vlc_Init_From_Lengths<TLens, TSymbols>(Vlc vlc, c_int nb_Bits, c_int nb_Codes, CPointer<TLens> lens, CPointer<TSymbols> symbols, c_int offset, VlcInit flags, IClass logCtx) where TLens : INumber<TLens> where TSymbols : INumber<TSymbols>//XX 306
		{
			VlcCode[] localBuf = ArrayHelper.InitializeArray<VlcCode>(LocalBuf_Elems);
			CPointer<VlcCode> buf = localBuf;
			c_int len_Max = Macros.FFMin(32, 3 * nb_Bits);

			c_int ret = Vlc_Common_Init(vlc, nb_Bits, nb_Codes, ref buf, flags);

			if (ret < 0)
				return ret;

			c_int j = 0;
			uint64_t code = 0;

			for (c_int i = 0; i < nb_Codes; i++, lens++)
			{
				c_int len = c_int.CreateChecked(lens[0]);

				if (len > 0)
				{
					c_uint sym;

					buf[j].Bits = (uint8_t)len;

					if (symbols.IsNotNull)
					{
						Get_Data(out TSymbols s, symbols, i);
						sym = c_uint.CreateChecked(s);
					}
					else
						sym = (c_uint)i;

					buf[j].Symbol = (VlcBaseType)(sym + offset);
					buf[j++].Code = (uint32_t)code;
				}
				else if (len < 0)
					len = -len;
				else
					continue;

				if ((len > len_Max) || ((code & ((1U << (32 - len)) - 1)) != 0))
				{
					Log.Av_Log(logCtx, Log.Av_Log_Error, "Invalid VLC (length %u)\n", len);

					goto Fail;
				}

				code += 1U << (32 - len);

				if (code > (c_uint.MaxValue + 1UL))
				{
					Log.Av_Log(logCtx, Log.Av_Log_Error, "Overdetermined VLC tree\n");

					goto Fail;
				}
			}

			return Vlc_Common_End(vlc, nb_Bits, j, buf, flags, localBuf);

			Fail:
			if (buf != localBuf)
				Mem.Av_Free(buf);

			return Error.InvalidData;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_Vlc_Free(Vlc vlc)//XX 580
		{
			Mem.Av_FreeP(ref vlc.Table);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Get_Data<T>(out T v, CPointer<T> table, c_int i)//XX 41
		{
			CPointer<T> ptr = table + i;

			v = ptr[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Alloc_Table(Vlc vlc, c_int size, c_int use_Static)//XX 60
		{
			c_int index = vlc.Table_Size;

			vlc.Table_Size += size;

			if (vlc.Table_Size > vlc.Table_Allocated)
			{
				if (use_Static != 0)
					throw new ApplicationException();	// Cannot do anything, vlc_init() is used with too little memory

				vlc.Table_Allocated += (1 << vlc.Bits);

				vlc.Table = Mem.Av_Realloc_FObj(vlc.Table, (size_t)vlc.Table_Allocated);

				if (vlc.Table.IsNull)
				{
					vlc.Table_Allocated = 0;
					vlc.Table_Size = 0;

					return Error.ENOMEM;
				}

				for (c_int i = vlc.Table_Allocated - (1 << vlc.Bits); i < vlc.Table_Allocated; i++)
					vlc.Table[i].Clear();
			}

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint32_t BitSwap_32(uint32_t x)//XX 82
		{
			return ((uint32_t)Reverse.ff_Reverse[x & 0xff] << 24) |
				   ((uint32_t)Reverse.ff_Reverse[(x >> 8) & 0xff] << 16) |
				   ((uint32_t)Reverse.ff_Reverse[(x >> 16) & 0xff] << 8) |
				   Reverse.ff_Reverse[x >> 24];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Vlc_Common_Init(Vlc vlc, c_int nb_Bits, c_int nb_Codes, ref CPointer<VlcCode> buf, VlcInit flags)//XX 98
		{
			vlc.Bits = nb_Bits;
			vlc.Table_Size = 0;

			if ((flags & VlcInit.Use_Static) == 0)
			{
				vlc.Table = null;
				vlc.Table_Allocated = 0;
			}

			if (nb_Codes > LocalBuf_Elems)
			{
				buf = Mem.Av_MAlloc_ArrayObj<VlcCode>((size_t)nb_Codes);

				if (buf.IsNull)
					return Error.ENOMEM;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Compare_VlcSpec(VlcCode a, VlcCode b)//XX 118
		{
			VlcCode sa = a, sb = b;

			return (c_int)(sa.Code >> 1) - (c_int)(sb.Code >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// Build VLC decoding tables suitable for use with get_vlc()
		/// </summary>
		/********************************************************************/
		private static c_int Build_Table(Vlc vlc, c_int table_Nb_Bits, c_int nb_Codes, CPointer<VlcCode> codes, VlcInit flags)//XX 138
		{
			if (table_Nb_Bits > 30)
				return Error.EINVAL;

			c_int table_Size = 1 << table_Nb_Bits;
			c_int table_Index = Alloc_Table(vlc, table_Size, (c_int)(flags & VlcInit.Use_Static));

			Log.FF_DLog(null, "new table index=%d size=%d\n", table_Index, table_Size);

			if (table_Index < 0)
				return table_Index;

			CPointer<VlcElem> table = vlc.Table + table_Index;

			// First pass: map codes and compute auxiliary table sizes
			for (c_int i = 0; i < nb_Codes; i++)
			{
				c_int n = codes[i].Bits;
				uint32_t code = codes[i].Code;
				c_int symbol = codes[i].Symbol;

				Log.FF_TLog(null, "i=%d n=%d code=0x%llx\n", i, n, code);

				if (n <= table_Nb_Bits)
				{
					// No need to add another table
					c_int j = (c_int)(code >> (32 - table_Nb_Bits));
					c_int nb = 1 << (table_Nb_Bits - n);
					c_int inc = 1;

					if ((flags & VlcInit.Output_Le) != 0)
					{
						j = (c_int)BitSwap_32(code);
						inc = 1 << n;
					}

					for (c_int k = 0; k < nb; k++)
					{
						c_int bits = table[j].U1.Len;
						c_int oldSym = table[j].U1.Sym;

						Log.FF_TLog(null, "%4x: code=%d n=%d\n", j, i, n);

						if (((bits != 0) || (oldSym != 0)) && ((bits != n) || (oldSym != symbol)))
						{
							Log.Av_Log(null, Log.Av_Log_Error, "incorrect codes\n");

							return Error.InvalidData;
						}

						table[j].U1.Len = (VlcBaseType)n;
						table[j].U1.Sym = (VlcBaseType)symbol;

						j += inc;
					}
				}
				else
				{
					// Fill auxiliary table recursively
					c_int k;

					n -= table_Nb_Bits;
					uint32_t code_Prefix = code >> (32 - table_Nb_Bits);
					c_int subtable_Bits = n;

					codes[i].Bits = (uint8_t)n;
					codes[i].Code = code << table_Nb_Bits;

					for (k = i + 1; k < nb_Codes; k++)
					{
						n = codes[k].Bits - table_Nb_Bits;

						if (n <= 0)
							break;

						code = codes[k].Code;

						if ((code >> (32 - table_Nb_Bits)) != code_Prefix)
							break;

						codes[k].Bits = (uint8_t)n;
						codes[k].Code = code << table_Nb_Bits;

						subtable_Bits = Macros.FFMax(subtable_Bits, n);
					}

					subtable_Bits = Macros.FFMin(subtable_Bits, table_Nb_Bits);
					c_int j = (c_int)((flags & VlcInit.Output_Le) != 0 ? BitSwap_32(code_Prefix) >> (32 - table_Nb_Bits) : code_Prefix);

					table[j].U1.Len = (VlcBaseType)(-subtable_Bits);

					Log.FF_DLog(null, "%4x: n=%d (subtable)\n", j, codes[i].Bits + table_Nb_Bits);

					c_int index = Build_Table(vlc, subtable_Bits, k - i, codes + i, flags);

					if (index < 0)
						return index;

					// Note: realloc has been done, so reload tables
					table = vlc.Table + table_Index;

					table[j].U1.Sym = (VlcBaseType)index;

					if (table[j].U1.Sym != index)
					{
						Log.AvPriv_Request_Sample(null, "strange codes");

						return Error.PatchWelcome;
					}

					i = k - 1;
				}
			}

			for (c_int i = 0; i < table_Size; i++)
			{
				if (table[i].U1.Len == 0)
					table[i].U1.Sym = -1;
			}

			return table_Index;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Vlc_Common_End(Vlc vlc, c_int nb_Bits, c_int nb_Codes, CPointer<VlcCode> codes, VlcInit flags, CPointer<VlcCode> localBuf)//XX 229
		{
			c_int ret = Build_Table(vlc, nb_Bits, nb_Codes, codes, flags);

			if ((flags & VlcInit.Use_Static) != 0)
			{
				if ((vlc.Table_Size != vlc.Table_Allocated) && ((flags & (VlcInit.Static_Overlong & ~VlcInit.Use_Static)) == 0))
					Log.Av_Log(null, Log.Av_Log_Error, "needed %d had %d\n", vlc.Table_Size, vlc.Table_Allocated);
			}
			else
			{
				if (codes != localBuf)
					Mem.Av_Free(codes);

				if (ret < 0)
				{
					Mem.Av_FreeP(ref vlc.Table);

					return ret;
				}
			}

			return 0;
		}
		#endregion
	}
}
