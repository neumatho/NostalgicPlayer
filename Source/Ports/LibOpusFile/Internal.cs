/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.OpusFile
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Internal
	{
		public const ogg_int64_t Op_Int64_Max = 2 * (((ogg_int64_t)1 << 62) - 1) | 1;
		public const ogg_int64_t Op_Int64_Min = -Op_Int64_Max - 1;
		public const ogg_int32_t Op_Int32_Max = 2 * (((ogg_int32_t)1 << 30) - 1) | 1;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Op_Min<T>(T _a, T _b) where T : INumber<T>
		{
			return _a < _b ? _a : _b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Op_Max<T>(T _a, T _b) where T : INumber<T>
		{
			return _a > _b ? _a : _b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Op_Clamp<T>(T _lo, T _x, T _hi) where T : INumber<T>
		{
			return Op_Max(_lo, Op_Min(_x, _hi));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int64 Op_Adv_Offset(opus_int64 _offset, c_int _amount)
		{
			return Op_Min(_offset, Op_Int64_Max - _amount) + _amount;
		}



		/********************************************************************/
		/// <summary>
		/// A version of strncasecmp() that is guaranteed to only ignore the
		/// case of ASCII characters
		/// </summary>
		/********************************************************************/
		public static c_int Op_Strncasecmp(string _a, Pointer<byte> _b)
		{
			c_int _n = _a.Length;

			for (c_int i = 0; i < _n; i++)
			{
				c_int a = _a[i];
				c_int b = _b[i];

				if ((a >= 'a') && (a <= 'z'))
					a -= 'a' - 'A';

				if ((b >= 'a') && (b <= 'z'))
					b -= 'a' - 'A';

				c_int d = a - b;
				if (d != 0)
					return d;
			}

			return 0;
		}
	}
}
