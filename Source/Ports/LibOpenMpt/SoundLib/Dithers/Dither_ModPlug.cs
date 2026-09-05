/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Dither_ModPlug : IDither<Modplug_Dither>
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Modplug_Dither Prng_Init<TRd_Result>(IEngine_Traits<TRd_Result> rd) where TRd_Result : IBinaryInteger<TRd_Result>, IUnsignedNumber<TRd_Result>
		{
			return new Modplug_Dither(0, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public MixSampleInt Process(uint32 targetbits, MixSampleInt sample, Modplug_Dither prng)
		{
			if (targetbits == 0)
				return sample;
			else if (((c_int)targetbits + MixSample.MixSampleIntTraits.Mix_Headroom_Bits + 1) >= 32)
				return sample;
			else
			{
				sample += Arithmetic_Shift.RShift_Signed((int32)Random.Random_<uint32, uint32>(prng), (c_int)targetbits + MixSample.MixSampleIntTraits.Mix_Headroom_Bits + 1);

				return sample;
			}
		}
	}
}
