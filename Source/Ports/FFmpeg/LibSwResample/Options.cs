/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample
{
	/// <summary>
	/// 
	/// </summary>
	public static class Options
	{
		private const c_double C_30DB = Mathematics.M_Sqrt1_2;

		private const AvOptFlag Param = AvOptFlag.Audio_Param;

		private static readonly AvOption[] options =
		[
			new AvOption("isr", "set input sample rate", nameof(SwrContext.In_Sample_Rate), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, 0, c_int.MaxValue, Param),
			new AvOption("in_sample_rate", "set input sample rate", nameof(SwrContext.In_Sample_Rate), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, 0, c_int.MaxValue, Param),
			new AvOption("osr", "set output sample rate", nameof(SwrContext.Out_Sample_Rate), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, 0, c_int.MaxValue, Param),
			new AvOption("out_sample_rate", "set output sample rate", nameof(SwrContext.Out_Sample_Rate), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, 0, c_int.MaxValue, Param),
			new AvOption("isf", "set input sample format", nameof(SwrContext.In_Sample_Fmt), AvOptionType.Sample_Fmt, new AvOption.DefaultValueUnion { I64 = (int64_t)AvSampleFormat.None }, -1, c_int.MaxValue, Param),
			new AvOption("in_sample_fmt", "set input sample format", nameof(SwrContext.In_Sample_Fmt), AvOptionType.Sample_Fmt, new AvOption.DefaultValueUnion { I64 = (int64_t)AvSampleFormat.None }, -1, c_int.MaxValue, Param),
			new AvOption("osf", "set output sample format", nameof(SwrContext.Out_Sample_Fmt), AvOptionType.Sample_Fmt, new AvOption.DefaultValueUnion { I64 = (int64_t)AvSampleFormat.None }, -1, c_int.MaxValue, Param),
			new AvOption("out_sample_fmt", "set output sample format", nameof(SwrContext.Out_Sample_Fmt), AvOptionType.Sample_Fmt, new AvOption.DefaultValueUnion { I64 = (int64_t)AvSampleFormat.None }, -1, c_int.MaxValue, Param),
			new AvOption("tsf", "set internal sample format", nameof(SwrContext.User_Int_Sample_Fmt), AvOptionType.Sample_Fmt, new AvOption.DefaultValueUnion { I64 = (int64_t)AvSampleFormat.None }, -1, c_int.MaxValue, Param),
			new AvOption("internal_sample_fmt", "set internal sample format", nameof(SwrContext.User_Int_Sample_Fmt), AvOptionType.Sample_Fmt, new AvOption.DefaultValueUnion { I64 = (int64_t)AvSampleFormat.None }, -1, c_int.MaxValue, Param),
			new AvOption("ichl", "set input channel layout", nameof(SwrContext.User_In_ChLayout), AvOptionType.ChLayout, new AvOption.DefaultValueUnion { Str = null }, 0, 0, Param, "chlayout"),
			new AvOption("in_chlayout", "set input channel layout", nameof(SwrContext.User_In_ChLayout), AvOptionType.ChLayout, new AvOption.DefaultValueUnion { Str = null }, 0, 0, Param, "chlayout"),
			new AvOption("ochl", "set output channel layout", nameof(SwrContext.User_Out_ChLayout), AvOptionType.ChLayout, new AvOption.DefaultValueUnion { Str = null }, 0, 0, Param, "chlayout"),
			new AvOption("out_chlayout", "set output channel layout", nameof(SwrContext.User_Out_ChLayout), AvOptionType.ChLayout, new AvOption.DefaultValueUnion { Str = null }, 0, 0, Param, "chlayout"),
			new AvOption("uchl", "set used channel layout", nameof(SwrContext.User_Used_ChLayout), AvOptionType.ChLayout, new AvOption.DefaultValueUnion { Str = null }, 0, 0, Param, "chlayout"),
			new AvOption("used_chlayout", "set used channel layout", nameof(SwrContext.User_Used_ChLayout), AvOptionType.ChLayout, new AvOption.DefaultValueUnion { Str = null }, 0, 0, Param, "chlayout"),
			new AvOption("clev", "set center mix level", nameof(SwrContext.CLev), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = C_30DB }, -32, 32, Param),
			new AvOption("center_mix_level", "set center mix level", nameof(SwrContext.CLev), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = C_30DB }, -32, 32, Param),
			new AvOption("slev", "set surround mix level", nameof(SwrContext.SLev), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = C_30DB }, -32, 32, Param),
			new AvOption("surround_mix_level", "set surround mix Level", nameof(SwrContext.SLev), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = C_30DB }, -32, 32, Param),
			new AvOption("lfe_mix_level", "set LFE mix level", nameof(SwrContext.Lfe_Mix_Level), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 0 }, -32, 32, Param),
			new AvOption("rmvol", "set rematrix volume", nameof(SwrContext.Rematrix_Volume), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 1.0 }, -1000, 1000, Param),
			new AvOption("rematrix_volume", "set rematrix volume", nameof(SwrContext.Rematrix_Volume), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 1.0 }, -1000, 1000, Param),
			new AvOption("rematrix_maxval", "set rematrix maxval", nameof(SwrContext.Rematrix_MaxVal), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 0.0 }, 0, 1000, Param),

			new AvOption("flags", "set flags", nameof(SwrContext.Flags), AvOptionType.Flags, new AvOption.DefaultValueUnion { I64 = 0 }, 0, c_uint.MaxValue, Param, "flags"),
			new AvOption("swr_flags", "set flags", nameof(SwrContext.Flags), AvOptionType.Flags, new AvOption.DefaultValueUnion { I64 = 0 }, 0, c_uint.MaxValue, Param, "flags"),
			new AvOption("res", "force resampling", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrFlag.Resample }, c_int.MinValue, c_int.MaxValue, Param, "flags"),

			new AvOption("dither_scale", "set dither scale", $"{nameof(SwrContext.Dither)}.{nameof(DitherContext.Scale)}", AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 1 }, 0, c_int.MaxValue, Param),

			new AvOption("dither_method", "set dither method", nameof(SwrContext.User_Dither_Method), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.None }, 0, (c_int)SwrDitherType.Nb - 1, Param, "dither_method"),
			new AvOption("rectangular", "select rectangular dither", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Rectangular}, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),
			new AvOption("triangular", "select triangular dither", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Triangular }, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),
			new AvOption("triangular_hp", "select triangular dither with high pass", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Triangular_HighPass }, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),
			new AvOption("lipshitz", "select Lipshitz noise shaping dither", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Ns_Lipshitz }, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),
			new AvOption("shibata", "select Shibata noise shaping dither", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Ns_Shibata }, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),
			new AvOption("low_shibata", "select low Shibata noise shaping dither", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Ns_Low_Shibata }, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),
			new AvOption("high_shibata", "select high Shibata noise shaping dither", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Ns_High_Shibata }, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),
			new AvOption("f_weighted", "select f-weighted noise shaping dither", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Ns_F_Weighted }, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),
			new AvOption("modified_e_weighted", "select modified-e-weighted noise shaping dither", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Ns_Modified_E_Weighted }, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),
			new AvOption("improved_e_weighted", "select improved-e-weighted noise shaping dither", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrDitherType.Ns_Improved_E_Weighted }, c_int.MinValue, c_int.MaxValue, Param, "dither_method"),

			new AvOption("filter_size", "set swr resampling filter size", nameof(SwrContext.Filter_Size), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 32 }, 0, c_int.MaxValue, Param),
			new AvOption("phase_shift", "set swr resampling phase shift", nameof(SwrContext.Phase_Shift), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 10 }, 0, 24, Param),
			new AvOption("linear_interp", "enable linear interpolation", nameof(SwrContext.Linear_Interp), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 1 }, 0, 1, Param),
			new AvOption("exact_rational", "enable exact rational", nameof(SwrContext.Exact_Rational), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 1 }, 0, 1, Param),
			new AvOption("cutoff", "set cutoff frequency ratio", nameof(SwrContext.Cutoff), AvOptionType.Double, new AvOption.DefaultValueUnion { Dbl = 0.0 }, 0, 1, Param),

			// Duplicate option in order to work with avconv
			new AvOption("resample_cutoff", "set cutoff frequency ratio", nameof(SwrContext.Cutoff), AvOptionType.Double, new AvOption.DefaultValueUnion { Dbl = 0.0 }, 0, 1, Param),

			new AvOption("resampler", "set resampling Engine", nameof(SwrContext.Engine), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, 0, (c_int)SwrEngine.Nb - 1, Param, "resampler"),
			new AvOption("swr", "select SW Resampler", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrEngine.Swr }, c_int.MinValue, c_int.MaxValue, Param, "resampler"),
			new AvOption("soxr", "select SoX Resampler", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrEngine.Soxr }, c_int.MinValue, c_int.MaxValue, Param, "resampler"),
			new AvOption("precision", "set soxr resampling precision (in bits)", nameof(SwrContext.Precision), AvOptionType.Double, new AvOption.DefaultValueUnion { Dbl = 20.0 }, 15.0, 33.0, Param),
			new AvOption("cheby", "enable soxr Chebyshev passband & higher-precision irrational ratio approximation", nameof(SwrContext.Cheby), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 0 }, 0, 1, Param),
			new AvOption("min_comp", "set minimum difference between timestamps and audio data (in seconds) below which no timestamp compensation of either kind is applied", nameof(SwrContext.Min_Compensation), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = CMath.FLT_MAX }, 0, CMath.FLT_MAX, Param),
			new AvOption("min_hard_comp", "set minimum difference between timestamps and audio data (in seconds) to trigger padding/trimming the data.", nameof(SwrContext.Min_Hard_Compensation), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 0.1 }, 0, c_int.MaxValue, Param),
			new AvOption("comp_duration", "set duration (in seconds) over which data is stretched/squeezed to make it match the timestamps.", nameof(SwrContext.Soft_Compensation_Duration), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 1 }, 0, c_int.MaxValue, Param),
			new AvOption("max_soft_comp", "set maximum factor by which data is stretched/squeezed to make it match the timestamps.", nameof(SwrContext.Max_Soft_Compensation), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 0 }, c_int.MinValue, c_int.MaxValue, Param),
			new AvOption("async", "simplified 1 parameter audio timestamp matching, 0(disabled), 1(filling and trimming), >1(maximum stretch/squeeze in samples per second)", nameof(SwrContext.Async), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 0 }, c_int.MinValue, c_int.MaxValue, Param),
			new AvOption("first_pts", "Assume the first pts should be this value (in samples).", nameof(SwrContext.FirstPts_In_Samples), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = UtilConstants.Av_NoPts_Value }, int64_t.MinValue, int64_t.MaxValue, Param),

			new AvOption("matrix_encoding", "set matrixed stereo encoding", nameof(SwrContext.Matrix_Encoding), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = (int64_t)AvMatrixEncoding.None }, (c_int)AvMatrixEncoding.None, (c_int)AvMatrixEncoding.Nb - 1, Param, "matrix_encoding"),
			new AvOption("none", "select none", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvMatrixEncoding.None }, c_int.MinValue, c_int.MaxValue, Param, "matrix_encoding"),
			new AvOption("dolby", "select Dolby", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvMatrixEncoding.Dolby }, c_int.MinValue, c_int.MaxValue, Param, "matrix_encoding"),
			new AvOption("dplii", "select Dolby Pro Logic II", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvMatrixEncoding.DplII }, c_int.MinValue, c_int.MaxValue, Param, "matrix_encoding"),

			new AvOption("filter_type", "select swr filter type", nameof(SwrContext.Filter_Type), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrFilterType.Kaiser }, (c_int)SwrFilterType.Cubic, (c_int)SwrFilterType.Kaiser, Param, "filter_type"),
			new AvOption("cubic", "select cubic", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrFilterType.Cubic }, c_int.MinValue, c_int.MaxValue, Param, "filter_type"),
			new AvOption("blackman_nuttall", "select Blackman Nuttall windowed sinc", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrFilterType.Blackman_Nuttall }, c_int.MinValue, c_int.MaxValue, Param, "filter_type"),
			new AvOption("kaiser", "select Kaiser windowed sinc", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)SwrFilterType.Kaiser }, c_int.MinValue, c_int.MaxValue, Param, "filter_type"),

			new AvOption("kaiser_beta", "set swr Kaiser window beta", nameof(SwrContext.Kaiser_Beta), AvOptionType.Double, new AvOption.DefaultValueUnion { Dbl = 9 }, 2, 16, Param),

			new AvOption("output_sample_bits", "set swr number of output sample bits", $"{nameof(SwrContext.Dither)}.{nameof(DitherContext.Output_Sample_Bits)}", AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, 0, 64, Param),
			new AvOption()
		];

		private static readonly AvClass av_Class = new AvClass
		{
			Class_Name = "SWResampler".ToCharPointer(),
			Item_Name = Context_To_Name,
			Option = options,
			Version = Version.Version_Int,
			Log_Level_Offset_Name = nameof(SwrContext.Log_Level_Offset),
			Parent_Log_Context_Name = nameof(SwrContext.Log_Ctx),
			Category = AvClassCategory.SwResample
		};

		/********************************************************************/
		/// <summary>
		/// Allocate SwrContext.
		///
		/// If you use this function you will need to set the parameters
		/// (manually or with swr_alloc_set_opts2()) before calling
		/// swr_init()
		/// </summary>
		/********************************************************************/
		public static SwrContext Swr_Alloc()
		{
			SwrContext s = Mem.Av_MAlloczObj<SwrContext>();

			if (s != null)
			{
				av_Class.CopyTo(s.Av_Class);
				Opt.Av_Opt_Set_Defaults(s);

				s.FirstPts = UtilConstants.Av_NoPts_Value;
			}

			return s;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Context_To_Name(IClass ptr)//XX 129
		{
			return "SWR".ToCharPointer();
		}
		#endregion
	}
}
