/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IInterpolator
	{
		void Init(ModChannel c, CResampler resampler, c_uint numSamples);
		void Interpolate<TIn, TTraits>(Span<mixsample_t> outSample, CPointer<TIn> inBuffer, uint32 posLo) where TIn : unmanaged where TTraits : struct, IMixerTraits<TIn>;
		void Done(ModChannel channel);

	}
}
