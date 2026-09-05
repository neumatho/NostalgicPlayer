/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Table containing all mixer functions
	/// </summary>
	internal static class MixFuncTable
	{
		/// <summary>
		/// Table index bits:
		/// [b1-b0] format (8-bit-mono, 16-bit-mono, 8-bit-stereo, 16-bit-stereo)
		/// [b2]    ramp
		/// [b3]    filter
		/// [b6-b4] src type
		/// </summary>
		[Flags]
		public enum FunctionIndex : uint32
		{
			_16Bit = 0x01,
			Stereo = 0x02,
			Ramp = 0x04,
			Filter = 0x08
		}

		/// <summary>
		/// SRC index
		/// </summary>
		public enum ResamplingIndex
		{
			NoInterpolation = 0x00,
			Linear = 0x10,
			FastSinc = 0x20,
			Kaiser = 0x30,
			FirFilter = 0x40,
			AmigaBlep = 0x50
		}

		public delegate void MixFuncInterface(ModChannel chn, CResampler resampler, CPointer<mixsample_t> outBuffer, c_uint numSamples);

		public static readonly MixFuncInterface[] Functions = new MixFuncInterface[6 * 16];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		static MixFuncTable()
		{
			AddResampling<NoInterpolation>(ResamplingIndex.NoInterpolation);
			AddResampling<LinearInterpolation>(ResamplingIndex.Linear);
			AddResampling<FastSincInterpolation>(ResamplingIndex.FastSinc);
			AddResampling<PolyphaseInterpolation>(ResamplingIndex.Kaiser);
			AddResampling<FirFilterInterpolation>(ResamplingIndex.FirFilter);
//XX			AddResampling<AmigaBlepInterpolation>(ResamplingIndex.AmigaBlep);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void AddResampling<TI>(ResamplingIndex b) where TI : struct, IInterpolator
		{
			AddFilter<TI, NoFilter>((c_int)b);
			AddFilter<TI, ResonantFilter>((c_int)b | 0x08);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void AddFilter<TI, TF>(c_int b) where TI : struct, IInterpolator where TF : struct, IFilter
		{
			AddRamp<TI, TF, MixMonoNoRamp, MixStereoNoRamp>(b);
			AddRamp<TI, TF, MixMonoRamp, MixStereoRamp>(b | 0x04);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void AddRamp<TI, TF, TMono, TStereo>(c_int b) where TI : struct, IInterpolator where TF : struct, IFilter where TMono : struct, IMixer where TStereo : struct, IMixer
		{
			Functions[b | 0x00] = MixerInterface.SampleLoop<int8, Int8MToIntS, TI, TF, TMono>;
			Functions[b | 0x01] = MixerInterface.SampleLoop<int16, Int16MToIntS, TI, TF, TMono>;
			Functions[b | 0x02] = MixerInterface.SampleLoop<int8, Int8SToIntS, TI, TF, TStereo>;
			Functions[b | 0x03] = MixerInterface.SampleLoop<int16, Int16SToIntS, TI, TF, TStereo>;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static ResamplingIndex ResamplingModeToMixFlags(ResamplingMode resamplingMode)
		{
			switch (resamplingMode)
			{
				case ResamplingMode.Nearest:
					return ResamplingIndex.NoInterpolation;

				case ResamplingMode.Linear:
					return ResamplingIndex.Linear;

				case ResamplingMode.Cubic:
					return ResamplingIndex.FastSinc;

				case ResamplingMode.Sinc8Lp:
					return ResamplingIndex.Kaiser;

				case ResamplingMode.Sinc8:
					return ResamplingIndex.FirFilter;

				case ResamplingMode.Amiga:
					return ResamplingIndex.AmigaBlep;
			}

			return ResamplingIndex.NoInterpolation;
		}
	}
}
