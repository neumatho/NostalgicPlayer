/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers
{
	/// <summary>
	/// 
	/// </summary>
	internal readonly struct FastSincInterpolation : IInterpolator
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(ModChannel c, CResampler resampler, c_uint numSamples)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Interpolate<TIn, TTraits>(Span<mixsample_t> outSample, CPointer<TIn> inBuffer, uint32 posLo) where TIn : unmanaged where TTraits : struct, IMixerTraits<TIn>
		{
			CPointer<int16> lut = new CPointer<int16>(Tables.FastSincTable) + ((posLo >> 22) & 0x3fc);

			for (c_int i = 0; i < TTraits.NumChannelsIn; i++)
			{
				outSample[i] = ((lut[0] * TTraits.Convert(inBuffer[i - TTraits.NumChannelsIn])) +
								(lut[1] * TTraits.Convert(inBuffer[i])) +
								(lut[2] * TTraits.Convert(inBuffer[i + TTraits.NumChannelsIn])) +
								(lut[3] * TTraits.Convert(inBuffer[i + (2 * TTraits.NumChannelsIn)]))) / 16384;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Done(ModChannel channel)
		{
		}
	}
}
