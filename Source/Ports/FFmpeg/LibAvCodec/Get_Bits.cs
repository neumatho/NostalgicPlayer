/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// Bitstream reader API header
	/// </summary>
	public static class Get_Bits
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Get_Bits_Count(GetBitContext gb)//XX 250
		{
			return gb.Index;
		}



		/********************************************************************/
		/// <summary>
		/// Read 1-25 bits
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint _Get_Bits(GetBitContext s, c_int n)//XX 333
		{
			c_uint index = (c_uint)s.Index;
			c_uint cache = IntReadWrite.Av_RB32(s.Buffer + (index >> 3)) << ((c_int)index & 7);
			c_uint tmp = cache >> (32 - n);
			s.Index += n;

			return tmp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Skip_Bits(GetBitContext s, c_int n)//XX 379
		{
			s.Index += n;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint Get_Bits1(GetBitContext s)//XX 386
		{
			c_uint index = (c_uint)s.Index;
			uint8_t result = s.Buffer[index >> 3];

			result <<= (uint8_t)(index & 7);
			result >>= 8 - 1;

			index++;
			s.Index = (c_int)index;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Read 0-32 bits
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint Get_Bits_Long(GetBitContext s, c_int n)//XX 419
		{
			if (n == 0)
				return 0;
			else if ((n <= CodecConstants.Min_Cache_Bits))
				return _Get_Bits(s, n);
			else
			{
				c_uint ret = _Get_Bits(s, 16) << (n - 16);

				return ret | _Get_Bits(s, n - 16);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize GetBitContext
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Init_Get_Bits(GetBitContext s, CPointer<uint8_t> buffer, c_int bit_Size)//XX 512
		{
			c_int ret = 0;

			if ((bit_Size >= (c_int.MaxValue - Macros.FFMax(7, Defs.Av_Input_Buffer_Padding_Size * 8))) || (bit_Size < 0) || buffer.IsNull)
			{
				bit_Size = 0;
				buffer.SetToNull();

				ret = Error.InvalidData;
			}

			s.Buffer = buffer;
			s.Size_In_Bits = bit_Size;
			s.Size_In_Bits_Plus8 = bit_Size + 8;
			s.Index = 0;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<uint8_t> Align_Get_Bits(GetBitContext s)//XX 555
		{
			c_int n = -Get_Bits_Count(s) & 7;

			if (n != 0)
				Skip_Bits(s, n);

			return s.Buffer + (s.Index >> 3);
		}



		/********************************************************************/
		/// <summary>
		/// Parse a vlc code
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Get_Vlc2(GetBitContext s, CPointer<VlcElem> table, c_int bits, c_int max_Depth)//XX 646
		{
			c_uint re_Index = (c_uint)s.Index;
			c_uint re_Cache = IntReadWrite.Av_RB32(s.Buffer + (re_Index >> 3)) << ((c_int)re_Index & 7);

			c_uint index = re_Cache >> (32 - bits);
			c_int code = table[index].U1.Sym;
			c_int n = table[index].U1.Len;

			if ((max_Depth > 1) && (n < 0))
			{
				re_Index += (c_uint)bits;
				re_Cache = IntReadWrite.Av_RB32(s.Buffer + (re_Index >> 3)) << ((c_int)re_Index & 7);

				c_int nb_Bits = -n;

				index = (c_uint)((re_Cache >> (32 - nb_Bits)) + code);
				code = table[index].U1.Sym;
				n = table[index].U1.Len;

				if ((max_Depth > 2) && (n < 0))
				{
					re_Index += (c_uint)nb_Bits;
					re_Cache = IntReadWrite.Av_RB32(s.Buffer + (re_Index >> 3)) << ((c_int)re_Index & 7);

					nb_Bits = -n;

					index = (c_uint)((re_Cache >> (32 - nb_Bits)) + code);
					code = table[index].U1.Sym;
					n = table[index].U1.Len;
				}
			}

			re_Cache <<= n;
			re_Index += (c_uint)n;

			s.Index = (c_int)re_Index;

			return code;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Get_Bits_Left(GetBitContext gb)//XX 689
		{
			return gb.Size_In_Bits - Get_Bits_Count(gb);
		}
	}
}
