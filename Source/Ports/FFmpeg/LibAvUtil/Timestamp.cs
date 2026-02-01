/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Timestamp
	{
		/// <summary>
		/// 
		/// </summary>
		public const c_int Av_Ts_Max_String_Size = 32;

		/********************************************************************/
		/// <summary>
		/// Fill the provided buffer with a string containing a timestamp
		/// representation
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<char> Av_Ts_Make_String(CPointer<char> buf, int64_t ts)//XX 43
		{
			if (ts == UtilConstants.Av_NoPts_Value)
				CString.snprintf(buf, Av_Ts_Max_String_Size, "NOPTS");
			else
				CString.snprintf(buf, Av_Ts_Max_String_Size, "%lld", ts);

			return buf;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<char> Av_Ts2Str(int64_t ts)//XX 54
		{
			return Av_Ts_Make_String(new CPointer<char>(Av_Ts_Max_String_Size), ts);
		}



		/********************************************************************/
		/// <summary>
		/// Fill the provided buffer with a string containing a timestamp
		/// representation
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<char> Av_Ts_Make_Time_String(CPointer<char> buf, int64_t ts, AvRational tb)//XX 73
		{
			return Av_Ts_Make_Time_String2(buf, ts, tb);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<char> Av_Ts2TimeStr(int64_t ts, AvRational tb)//XX 83
		{
			return Av_Ts_Make_Time_String(new CPointer<char>(Av_Ts_Max_String_Size), ts, tb);
		}



		/********************************************************************/
		/// <summary>
		/// Fill the provided buffer with a string containing a timestamp
		/// representation
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Ts_Make_Time_String2(CPointer<char> buf, int64_t ts, AvRational tb)
		{
			if (ts == UtilConstants.Av_NoPts_Value)
				CString.snprintf(buf, Av_Ts_Max_String_Size, "NOPTS");
			else
			{
				c_double val = Rational.Av_Q2D(tb) * ts;
				c_double log = (CMath.fpclassify(val) == CMath.FP_ZERO ? c_double.NegativeInfinity : CMath.floor(CMath.log10(CMath.fabs(val))));
				c_int precision = (c_int)((CMath.isfinite(log) && (log < 0)) ? -log + 5 : 6);

				c_int last = CString.snprintf(buf, Av_Ts_Max_String_Size, "%.*f", precision, val);
				last = Macros.FFMin(last, Av_Ts_Max_String_Size - 1) - 1;

				for (; (last != 0) && (buf[last] == '0'); last--)
				{
				}

				for (; (last != 0) && (buf[last] != 'f') && ((buf[last] < '0' || buf[0] > '9')); last--)
				{
				}

				buf[last + 1] = '\0';
			}

			return buf;
		}
	}
}
