/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C.Std.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	///
	/// </summary>
	internal static class Random
	{
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static T Random_<T, TValue>(IEngine_Traits<TValue> rng) where T : INumber<T>, IShiftOperators<T, c_int, T>, IBitwiseOperators<T, T, T> where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue>
		{
			c_uint rng_Bits = (c_uint)rng.Result_Bits;
			T result = T.Zero;
			size_t sizeT = (size_t)Marshal.SizeOf<T>();

			for (size_t entropy = 0; entropy < (sizeT * 8); entropy += rng_Bits)
			{
				if (rng_Bits < (sizeT * 8))
				{
					c_uint shift_Bits = (c_uint)(rng_Bits % (sizeT * 8));
					result = (result << (c_int)shift_Bits) ^ T.CreateTruncating(rng.Invoke());
				}
				else
					result = T.CreateTruncating(rng.Invoke());
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static T Random_<T, TValue>(IEngine_Traits<TValue> rng, size_t required_Entropy_Bits) where T : INumber<T>, IShiftOperators<T, c_int, T>, IBitwiseOperators<T, T, T> where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue>
		{
			c_uint rng_Bits = (c_uint)rng.Result_Bits;
			T result = T.Zero;
			size_t sizeT = (size_t)Marshal.SizeOf<T>();

			for (size_t entropy = 0; entropy < Math.Min(required_Entropy_Bits, sizeT * 8); entropy += rng_Bits)
			{
				if (rng_Bits < (sizeT * 8))
				{
					c_uint shift_Bits = (c_uint)(rng_Bits % (sizeT * 8));
					result = (result << (c_int)shift_Bits) ^ T.CreateTruncating(rng.Invoke());
				}
				else
					result = T.CreateTruncating(rng.Invoke());
			}

			if (required_Entropy_Bits >= (sizeT * 8))
				return result;
			else
				return result & ((T.One << (c_int)required_Entropy_Bits) - T.One);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static T Random_<T, TValue>(IUniform_Random_Bit_Generator<TValue> rng, T min, T max) where T : IBinaryInteger<T>, IMinMaxValue<T> where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue>
		{
			if (typeof(T) == typeof(uint8))
			{
				Uniform_Int_Distribution<c_uint> dis = new Uniform_Int_Distribution<c_uint>(c_uint.CreateTruncating(min), c_uint.CreateTruncating(max));

				return T.CreateTruncating(dis.Invoke(rng));
			}
			else if (typeof(T) == typeof(int8))
			{
				Uniform_Int_Distribution<c_int> dis = new Uniform_Int_Distribution<c_int>(c_int.CreateTruncating(min), c_int.CreateTruncating(max));

				return T.CreateTruncating(dis.Invoke(rng));
			}
			else
			{
				Uniform_Int_Distribution<T> dis = new Uniform_Int_Distribution<T>(min, max);

				return dis.Invoke(rng);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static T Random_<T, TValue>(IEngine_Traits<TValue> rng, T min, T max) where T : IFloatingPoint<T> where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue>
		{
			Uniform_Real_Distribution<T> dis = new Uniform_Real_Distribution<T>(min, max);

			return dis.Invoke(rng);
		}
	}
}
