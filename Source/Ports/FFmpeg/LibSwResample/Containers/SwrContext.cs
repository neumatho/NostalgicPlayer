/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class SwrContext : AvClass, IOptionContext
	{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
		/// <summary>
		/// AVClass used for AVOption and av_log()
		/// </summary>
		internal AvClass Av_Class => this;

		/// <summary>
		/// Logging level offset
		/// </summary>
		internal c_int Log_Level_Offset;

		/// <summary>
		/// Parent logging context
		/// </summary>
		internal IClass Log_Ctx;

		/// <summary>
		/// Input sample format
		/// </summary>
		internal AvSampleFormat In_Sample_Fmt;

		/// <summary>
		/// Internal sample format (AV_SAMPLE_FMT_FLTP or AV_SAMPLE_FMT_S16P)
		/// </summary>
		internal AvSampleFormat Int_Sample_Fmt;

		/// <summary>
		/// Output sample format
		/// </summary>
		internal AvSampleFormat Out_Sample_Fmt;

		/// <summary>
		/// Number of used input channels (mapped channel count if channel_map, otherwise in.ch_count)
		/// </summary>
		internal AvChannelLayout Used_Ch_Layout = new AvChannelLayout();

		/// <summary>
		/// Input channel layout
		/// </summary>
		internal readonly AvChannelLayout In_Ch_Layout = new AvChannelLayout();

		/// <summary>
		/// Output channel layout
		/// </summary>
		internal AvChannelLayout Out_Ch_Layout = new AvChannelLayout();

		/// <summary>
		/// Input sample rate
		/// </summary>
		internal c_int In_Sample_Rate;

		/// <summary>
		/// Output sample rate
		/// </summary>
		internal c_int Out_Sample_Rate;

		/// <summary>
		/// Miscellaneous flags such as SWR_FLAG_RESAMPLE
		/// </summary>
		internal SwrFlag Flags;

		/// <summary>
		/// Surround mixing level
		/// </summary>
		internal c_float SLev;

		/// <summary>
		/// Center mixing level
		/// </summary>
		internal c_float CLev;

		/// <summary>
		/// LFE mixing level
		/// </summary>
		internal c_float Lfe_Mix_Level;

		/// <summary>
		/// Rematrixing volume coefficient
		/// </summary>
		internal c_float Rematrix_Volume;

		/// <summary>
		/// Maximum value for rematrixing output
		/// </summary>
		internal c_float Rematrix_MaxVal;

		/// <summary>
		/// Matrixed stereo encoding
		/// </summary>
		internal AvMatrixEncoding Matrix_Encoding;

		/// <summary>
		/// Channel index (or -1 if muted channel) map
		/// </summary>
		internal CPointer<c_int> Channel_Map;

		/// <summary>
		/// 
		/// </summary>
		internal SwrEngine Engine;

		/// <summary>
		/// User set used channel layout
		/// </summary>
		internal readonly AvChannelLayout User_Used_ChLayout = new AvChannelLayout();

		/// <summary>
		/// User set input channel layout
		/// </summary>
		internal readonly AvChannelLayout User_In_ChLayout = new AvChannelLayout();

		/// <summary>
		/// User set output channel layout
		/// </summary>
		internal readonly AvChannelLayout User_Out_ChLayout = new AvChannelLayout();

		/// <summary>
		/// User set internal sample format
		/// </summary>
		internal AvSampleFormat User_Int_Sample_Fmt;

		/// <summary>
		/// User set dither method
		/// </summary>
		internal SwrDitherType User_Dither_Method;

		/// <summary>
		/// 
		/// </summary>
		internal readonly DitherContext Dither = new DitherContext();

		/// <summary>
		/// Length of each FIR filter in the resampling filterbank relative to the cutoff frequency
		/// </summary>
		internal c_int Filter_Size;

		/// <summary>
		/// Log2 of the number of entries in the resampling polyphase filterbank
		/// </summary>
		internal c_int Phase_Shift;

		/// <summary>
		/// If 1 then the resampling FIR filter will be linearly interpolated
		/// </summary>
		internal c_int Linear_Interp;

		/// <summary>
		/// If 1 then enable non power of 2 phase_count
		/// </summary>
		internal c_int Exact_Rational;

		/// <summary>
		/// Resampling cutoff frequency (swr: 6dB point; soxr: 0dB point). 1.0 corresponds to half the output sample rate
		/// </summary>
		internal c_double Cutoff;

		/// <summary>
		/// Swr resampling filter type
		/// </summary>
		internal SwrFilterType Filter_Type;

		/// <summary>
		/// Swr beta value for Kaiser window (only applicable if filter_type == AV_FILTER_TYPE_KAISER)
		/// </summary>
		internal c_double Kaiser_Beta;

		/// <summary>
		/// Soxr resampling precision (in bits)
		/// </summary>
		internal c_double Precision;

		/// <summary>
		/// Soxr: if 1 then passband rolloff will be none (Chebyshev) ＆ irrational ratio approximation precision will be higher
		/// </summary>
		internal c_int Cheby;

		/// <summary>
		/// Swr minimum below which no compensation will happen
		/// </summary>
		internal c_float Min_Compensation;

		/// <summary>
		/// Swr minimum below which no silence inject / sample drop will happen
		/// </summary>
		internal c_float Min_Hard_Compensation;

		/// <summary>
		/// Swr duration over which soft compensation is applied
		/// </summary>
		internal c_float Soft_Compensation_Duration;

		/// <summary>
		/// Swr maximum soft compensation in seconds over soft_compensation_duration
		/// </summary>
		internal c_float Max_Soft_Compensation;

		/// <summary>
		/// Swr simple 1 parameter async, similar to ffmpegs -async
		/// </summary>
		internal c_float Async;

		/// <summary>
		/// Swr first pts in samples
		/// </summary>
		internal int64_t FirstPts_In_Samples;

		/// <summary>
		/// 1 if resampling must come first, 0 if rematrixing
		/// </summary>
		internal c_int Resample_First;

		/// <summary>
		/// Flag to indicate if rematrixing is needed (basically if input and output layouts mismatch)
		/// </summary>
		internal c_int Rematrix;

		/// <summary>
		/// Flag to indicate that a custom matrix has been defined
		/// </summary>
		internal c_int Rematrix_Custom;

		/// <summary>
		/// Input audio data
		/// </summary>
		internal readonly AudioData In = new AudioData();

		/// <summary>
		/// Post-input audio data: used for rematrix/resample
		/// </summary>
		internal readonly AudioData PostIn = new AudioData();

		/// <summary>
		/// Intermediate audio data (postin/preout)
		/// </summary>
		internal readonly AudioData MidBuf = new AudioData();

		/// <summary>
		/// Pre-output audio data: used for rematrix/resample
		/// </summary>
		internal readonly AudioData PreOut = new AudioData();

		/// <summary>
		/// Converted output audio data
		/// </summary>
		internal readonly AudioData Out = new AudioData();

		/// <summary>
		/// Cached audio data (convert and resample purpose)
		/// </summary>
		internal readonly AudioData In_Buffer = new AudioData();

		/// <summary>
		/// Temporary with silence
		/// </summary>
		internal readonly AudioData Silence = new AudioData();

		/// <summary>
		/// Temporary used to discard output
		/// </summary>
		internal readonly AudioData Drop_Temp = new AudioData();

		/// <summary>
		/// Cached buffer position
		/// </summary>
		internal c_int In_Buffer_Index;

		/// <summary>
		/// Cached buffer length
		/// </summary>
		internal c_int In_Buffer_Count;

		/// <summary>
		/// 1 if the input end was reach before the output end, 0 otherwise
		/// </summary>
		internal c_int Resample_In_Constraint;

		/// <summary>
		/// 1 if data is to be flushed and no further input is expected
		/// </summary>
		internal c_int Flushed;

		/// <summary>
		/// Output PTS
		/// </summary>
		internal int64_t OutPts;

		/// <summary>
		/// First PTS
		/// </summary>
		internal int64_t FirstPts;

		/// <summary>
		/// Number of output samples to drop
		/// </summary>
		internal c_int Drop_Output;

		/// <summary>
		/// Soxr 0.1.1: needed to fixup delayed_samples after flush has been called
		/// </summary>
		internal c_double Delayed_Samples_Fixup;

		/// <summary>
		/// Input conversion context
		/// </summary>
		internal AudioConvert In_Convert;

		/// <summary>
		/// Output conversion context
		/// </summary>
		internal AudioConvert Out_Convert;

		/// <summary>
		/// Full conversion context (single conversion for input and output)
		/// </summary>
		internal AudioConvert Full_Convert;

		/// <summary>
		/// Resampling context
		/// </summary>
		internal ResampleContext Resample;

		/// <summary>
		/// Resampler virtual function table
		/// </summary>
		internal Resampler Resampler;

		/// <summary>
		/// Floating point rematrixing coefficients
		/// </summary>
		internal readonly c_double[][] Matrix = ArrayHelper.Initialize2Arrays<c_double>(SwrConstants.Swr_Ch_Max, SwrConstants.Swr_Ch_Max);

		/// <summary>
		/// Lists of input channels per output channel that have non zero rematrixing coefficients
		/// </summary>
		internal readonly uint8_t[][] Matrix_Ch = ArrayHelper.Initialize2Arrays<uint8_t>(SwrConstants.Swr_Ch_Max, SwrConstants.Swr_Ch_Max + 1);

		/// <summary>
		/// 
		/// </summary>
		internal CPointer<uint8_t> Native_Matrix;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
	}
}
