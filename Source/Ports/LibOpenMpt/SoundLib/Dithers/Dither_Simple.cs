/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers
{
	/// <summary>
	/// TNE: The original is Dither_SimpleImpl, which is a template on the
	/// three values below. libopenmpt only ever uses the default
	/// instantiation, so they are kept as fields instead of as type
	/// arguments. They are not const, since that would make the branches
	/// they guard unreachable code
	/// </summary>
	internal class Dither_Simple : IDither<Fast_Engine>
	{
		private static readonly c_int ditherDepth = 1;
		private static readonly bool triangular = false;
		private static readonly bool shaped = true;

		private int32 error = 0;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Fast_Engine Prng_Init<TRd_Result>(IEngine_Traits<TRd_Result> rd) where TRd_Result : IBinaryInteger<TRd_Result>, IUnsignedNumber<TRd_Result>
		{
			return Seed.Make_Prng<Fast_Engine, TRd_Result>(rd);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public MixSampleInt Process(uint32 targetbits, MixSampleInt sample, Fast_Engine prng)
		{
			if (targetbits == 0)
				return sample;
			else
			{
				c_int rShift = (c_int)(32 - targetbits) - MixSample.MixSampleIntTraits.Mix_Headroom_Bits;

				if (rShift <= 1)
				{
					// Nothing to dither
					return sample;
				}
				else
				{
					c_int rShiftPositive = rShift > 1 ? rShift : 1;
					c_int round_Mask = ~((1 << rShiftPositive) - 1);
					c_int round_Offset = 1 << (rShiftPositive - 1);
					c_int noise_Bits = rShiftPositive + (ditherDepth - 1);
					c_int noise_Bias = 1 << (noise_Bits - 1);
					int32 e = error;
					c_uint uNoise = 0;

					if (triangular)
						uNoise = (Random.Random_<c_uint, uint16>(prng, (size_t)noise_Bits) + Random.Random_<c_uint, uint16>(prng, (size_t)noise_Bits)) >> 1;
					else
						uNoise = Random.Random_<c_uint, uint16>(prng, (size_t)noise_Bits);

					c_int noise = (c_int)uNoise - noise_Bias;	// Un-bias
					c_int val = sample;

					if (shaped)
						val += e >> 1;

					c_int rounded = (val + noise + round_Offset) & round_Mask;

					e = val - rounded;
					sample = rounded;
					error = e;

					return sample;
				}
			}
		}
	}
}
