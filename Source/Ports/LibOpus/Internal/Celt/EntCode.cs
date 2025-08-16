/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class EntCode
	{
		private static readonly c_uint[] correction =
		[
			35733, 38967, 42495, 46340,
			50535, 55109, 60097, 65535
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ec_Ilog(opus_uint32 _v)
		{
			c_int ret = _v != 0 ? 1 : 0;
			c_int m = ((_v & 0xffff0000) != 0 ? 1 : 0) << 4;
			_v >>= m;
			ret |= m;
			m = ((_v & 0xff00) != 0 ? 1 : 0) << 3;
			_v >>= m;
			ret |= m;
			m = ((_v & 0xf0) != 0 ? 1 : 0) << 2;
			_v >>= m;
			ret |= m;
			m = ((_v & 0xc) != 0 ? 1 : 0) << 1;
			_v >>= m;
			ret |= m;
			ret += (_v & 0x2) != 0 ? 1 : 0;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// This is a faster version of ec_tell_frac() that takes advantage
		/// of the low (1/8 bit) resolution to use just a linear function
		/// followed by a lookup to determine the exact transition thresholds
		/// </summary>
		/********************************************************************/
		public static opus_uint32 Ec_Tell_Frac(Ec_Ctx _this)
		{
			opus_uint32 nbits = (opus_uint32)(_this.nbits_total << Constants.BitRes);
			c_int l = Ec_Ilog(_this.rng);
			opus_uint32 r = _this.rng >> (l - 16);
			c_uint b = (r >> 12) - 8;
			b += r > correction[b] ? 1U : 0;
			l = (c_int)((l << 3) + b);

			return (opus_uint32)(nbits - l);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_uint32 Ec_Range_Bytes(Ec_Ctx _this)
		{
			return _this.offs;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<byte> Ec_Get_Buffer(Ec_Ctx _this)
		{
			return _this.buf;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Ec_Get_Error(Ec_Ctx _this)
		{
			return _this.error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Ec_Tell(Ec_Ctx _this)
		{
			return _this.nbits_total - Ec_Ilog(_this.rng);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_uint32 Celt_UDiv(opus_uint32 n, opus_uint32 d)
		{
			return n / d;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Celt_SUDiv(opus_int32 n, opus_int32 d)
		{
			return n / d;
		}
	}
}
