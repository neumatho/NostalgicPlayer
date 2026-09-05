/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers
{
	/// <summary>
	/// TNE: The original constructor takes the random device that the prng
	/// is seeded from. A constructor cannot be generic in C#, so the caller
	/// creates the prng with TDither.Prng_Init() and passes it in
	/// </summary>
	internal class MultiChannelDither<TDither, TPrng> where TDither : IDither<TPrng>, new()
	{
		private readonly vector<TDither> ditherChannels;
		private readonly TPrng prng;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MultiChannelDither(TPrng prng_, size_t channels)
		{
			ditherChannels = new vector<TDither>(channels);
			prng = prng_;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			for (size_t channel = 0; channel < ditherChannels.size(); ++channel)
				ditherChannels[channel] = new TDither();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t GetChannels()
		{
			return ditherChannels.size();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public MixSampleInt Process(uint32 targetbits, size_t channel, MixSampleInt sample)
		{
			return ditherChannels[channel].Process(targetbits, sample, prng);
		}
	}
}
