/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Dither_None : IDither<Dither_None.Prng_Type>
	{
		/// <summary>
		/// TNE: The original uses an unnamed empty struct as its prng_type
		/// </summary>
		internal readonly struct Prng_Type
		{
		}

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Prng_Type Prng_Init<TRd_Result>(IEngine_Traits<TRd_Result> rd) where TRd_Result : IBinaryInteger<TRd_Result>, IUnsignedNumber<TRd_Result>
		{
			return new Prng_Type();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public MixSampleInt Process(uint32 targetbits, MixSampleInt sample, Prng_Type prng)
		{
			return sample;
		}
	}
}
