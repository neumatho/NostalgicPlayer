/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers
{
	/// <summary>
	/// Resonant filter
	/// </summary>
	internal struct ResonantFilter : IFilter
	{
		[InlineArray(2)]
		private struct History
		{
			private mixsample_t _element0;
		}

		[InlineArray(2)]
		private struct ChannelHistory
		{
			private History _element0;
		}

		/// <summary>
		/// To avoid a precision loss in the state variables especially with
		/// quiet samples at low cutoff and high mix rate, we pre-amplify
		/// the sample
		/// </summary>
		private const c_int Mixing_Filter_Preamp = 256;

		/// <summary>
		/// Filter history
		/// </summary>
		private ChannelHistory fy;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(ModChannel chn, c_int numChannelsIn)
		{
			for (c_int i = 0; i < numChannelsIn; i++)
			{
				fy[i][0] = chn.nFilter_Y[i][0];
				fy[i][1] = chn.nFilter_Y[i][1];
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Filter(Span<mixsample_t> outSample, ModChannel chn, c_int numChannelsIn)
		{
			for (c_int i = 0; i < numChannelsIn; i++)
			{
				mixsample_t inputAmp = outSample[i] * Mixing_Filter_Preamp;
				mixsample_t val = (mixsample_t)(Arithmetic_Shift.RShift_Signed(
												Util.Mul32To64(inputAmp, chn.nFilter_A0) +
												Util.Mul32To64(ClipFilter(fy[i][0]), chn.nFilter_B0) +
												Util.Mul32To64(ClipFilter(fy[i][1]), chn.nFilter_B1) +
												(1 << (Mixer.Mixing_Filter_Precision - 1)), Mixer.Mixing_Filter_Precision));

				fy[i][1] = fy[i][0];
				fy[i][0] = val - (inputAmp & chn.nFilter_HP);

				outSample[i] = val / Mixing_Filter_Preamp;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Done(ModChannel channel, c_int numChannelsIn)
		{
			for (c_int i = 0; i < numChannelsIn; i++)
			{
				channel.nFilter_Y[i][0] = fy[i][0];
				channel.nFilter_Y[i][1] = fy[i][1];
			}
		}



		/********************************************************************/
		/// <summary>
		/// Filter values are clipped to double the input range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static mixsample_t ClipFilter(mixsample_t x)
		{
			return OpenMpt.Clamp(x, int16.MinValue * 2 * Mixing_Filter_Preamp, int16.MaxValue * 2 * Mixing_Filter_Preamp);
		}
	}
}
