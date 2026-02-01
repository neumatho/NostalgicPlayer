/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Options_Table
	{
		private const int Default = 0;
		private const AvOptFlag E = AvOptFlag.Encoding_Param;
		private const AvOptFlag D = AvOptFlag.Decoding_Param;

		public static readonly AvOption[] AvFormat_Options =
		[
			new AvOption("avioflags", null, nameof(AvFormatContext.AvIo_Flags), AvOptionType.Flags, new AvOption.DefaultValueUnion { I64 = Default }, c_int.MinValue, c_int.MaxValue, D | E, "avioflags"),
			new AvOption("direct", "reduce buffering", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvIoFlag.Direct }, c_int.MinValue, c_int.MaxValue, D | E, "avioflags"),
			new AvOption("probesize", "set probing size", nameof(AvFormatContext.ProbeSize), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = 5000000 }, 32, long.MaxValue, D),
			new AvOption("formatprobesize", "number of bytes to probe file format", nameof(AvFormatContext.Format_ProbeSize), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = Internal.Probe_Buf_Max }, 0, c_int.MaxValue - 1, D),
			new AvOption("packetsize", "set packet size", nameof(AvFormatContext.Packet_Size), AvOptionType.UInt, new AvOption.DefaultValueUnion { I64 = Default }, 0, c_int.MaxValue, E),
			new AvOption("fflags", null, nameof(AvFormatContext.Flags), AvOptionType.Flags, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.Auto_Bsf }, c_int.MinValue, c_int.MaxValue, D | E, "fflags"),
			new AvOption("flush_packets", "reduce the latency by flushing out packets immediately", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.Flush_Packets }, c_int.MinValue, c_int.MaxValue, E, "fflags"),
			new AvOption("ignidx", "ignore index", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.IgnIdx }, c_int.MinValue, c_int.MaxValue, D, "fflags"),
			new AvOption("genpts", "generate pts", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.GenPts }, c_int.MinValue, c_int.MaxValue, D, "fflags"),
			new AvOption("nofillin", "do not fill in missing values that can be exactly calculated", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.NoFillIn }, c_int.MinValue, c_int.MaxValue, D, "fflags"),
			new AvOption("noparse", "disable AVParsers, this needs nofillin too", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.NoParse }, c_int.MinValue, c_int.MaxValue, D, "fflags"),
			new AvOption("igndts", "ignore dts", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.IgnDts }, c_int.MinValue, c_int.MaxValue, D, "fflags"),
			new AvOption("discardcorrupt", "discard corrupted frames", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.Discard_Corrupt }, c_int.MinValue, c_int.MaxValue, D, "fflags"),
			new AvOption("sortdts", "try to interleave outputted packets by dts", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.Sort_Dts }, c_int.MinValue, c_int.MaxValue, D, "fflags"),
			new AvOption("fastseek", "fast but inaccurate seeks", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.Fast_Seek }, c_int.MinValue, c_int.MaxValue, D, "fflags"),
			new AvOption("nobuffer", "reduce the latency introduced by optional buffering", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.NoBuffer }, 0, c_int.MaxValue, D, "fflags"),
			new AvOption("bitexact", "do not write random/volatile data", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.BitExact }, 0, 0, E, "fflags"),
			new AvOption("autobsf", "add needed bsfs automatically", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtFlag.Auto_Bsf }, 0, 0, E, "fflags"),
			new AvOption("seek2any", "allow seeking to non-keyframes on demuxer level when supported", nameof(AvFormatContext.Seek2Any), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 0 }, 0, 1, D),
			new AvOption("analyzeduration", "specify how many microseconds are analyzed to probe the input", nameof(AvFormatContext.Max_Analyze_Duration), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = 0 }, 0, long.MaxValue, D),
			new AvOption("cryptokey", "decryption key", nameof(AvFormatContext.Key), AvOptionType.Binary, new AvOption.DefaultValueUnion { Dbl = 0 }, 0, 0, D),
			new AvOption("indexmem", "max memory used for timestamp index (per stream)", nameof(AvFormatContext.Max_Index_Size), AvOptionType.UInt, new AvOption.DefaultValueUnion { I64 = 1 << 20 }, 0, c_int.MaxValue, D),
			new AvOption("rtbufsize", "max memory used for buffering real-time frames", nameof(AvFormatContext.Max_Picture_Buffer), AvOptionType.UInt, new AvOption.DefaultValueUnion { I64 = 3041280 }, 0, c_int.MaxValue, D),
//			new AvOption("fdebug", "print specific debug info", nameof(AvFormatContext.Debug), AvOptionType.Flags, new AvOption.DefaultValueUnion { I64 = Default }, 0, c_int.MaxValue, E | D, "fdebug"),
//			new AvOption("ts", null, null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = 0 /* FF_FDEBUG_TS */ }, c_int.MinValue, c_int.MaxValue, E | D, "fdebug"),
			new AvOption("max_delay", "maximum muxing or demuxing delay in microseconds", nameof(AvFormatContext.Max_Delay), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = -1 }, -1, c_int.MaxValue, E | D),
			new AvOption("start_time_realtime", "wall-clock time when stream begins (PTS==0)", nameof(AvFormatContext.Start_Time_Realtime), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = UtilConstants.Av_NoPts_Value }, long.MinValue, long.MaxValue, E),
			new AvOption("fpsprobesize", "number of frames used to probe fps", nameof(AvFormatContext.Fps_Probe_Size), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = -1 }, -1, c_int.MaxValue - 1, D),
			new AvOption("audio_preload", "microseconds by which audio packets should be interleaved earlier", nameof(AvFormatContext.Audio_Preload), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, 0, c_int.MaxValue - 1, E),
			new AvOption("chunk_duration", "microseconds for each chunk", nameof(AvFormatContext.Max_Chunk_Duration), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, 0, c_int.MaxValue - 1, E),
			new AvOption("chunk_size", "size in bytes for each chunk", nameof(AvFormatContext.Max_Chunk_Size), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, 0, c_int.MaxValue - 1, E),
			new AvOption("f_err_detect", "set error detection flags (deprecated; use err_detect, save via avconv)", nameof(AvFormatContext.Error_Recognition), AvOptionType.Flags, new AvOption.DefaultValueUnion { I64 = (int64_t)AvEf.CrcCheck }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("err_detect", "set error detection flags", nameof(AvFormatContext.Error_Recognition), AvOptionType.Flags, new AvOption.DefaultValueUnion { I64 = (int64_t)AvEf.CrcCheck }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("crccheck", "verify embedded CRCs", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvEf.CrcCheck }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("bitstream", "detect bitstream specification deviations", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvEf.Bitstream }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("buffer", "detect improper bitstream length", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvEf.Buffer }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("explode", "abort decoding on minor error detection", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvEf.Explode }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("ignore_err", "ignore errors", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvEf.Ignore_Err }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("careful", "consider things that violate the spec, are fast to check and have not been seen in the wild as errors", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvEf.Careful }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("compliant", "consider all spec non compliancies as errors", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)(AvEf.Compliant | AvEf.Careful) }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("aggressive", "consider things that a sane encoder shouldn't do as an error", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)(AvEf.Aggressive | AvEf.Compliant | AvEf.Careful) }, c_int.MinValue, c_int.MaxValue, D, "err_detect"),
			new AvOption("use_wallclock_as_timestamps", "use wallclock as timestamps", nameof(AvFormatContext.Use_Wallclock_As_Timestamps), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 0 }, 0, 1, D),
			new AvOption("skip_initial_bytes", "set number of bytes to skip before reading header and frames", nameof(AvFormatContext.Skip_Initial_Bytes), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = 0 }, 0, (c_double)long.MaxValue - 1, D),
			new AvOption("correct_ts_overflow", "correct single timestamp overflows", nameof(AvFormatContext.Correct_Ts_Overflow), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 1 }, 0, 1, D),
			new AvOption("flush_packets", "enable flushing of the I/O context after each packet", nameof(AvFormatContext.Flush_Packets), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = -1 }, -1, 1, E),
			new AvOption("metadata_header_padding", "set number of bytes to be written as padding in a metadata header", nameof(AvFormatContext.Metadata_Header_Padding), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = -1 }, -1, c_int.MaxValue, E),
			new AvOption("output_ts_offset", "set output timestamp offset", nameof(AvFormatContext.Output_Ts_Offset), AvOptionType.Duration, new AvOption.DefaultValueUnion { I64 = 0 }, long.MinValue, long.MaxValue, E),
			new AvOption("max_interleave_delta", "maximum buffering duration for interleaving", nameof(AvFormatContext.Max_Interleave_Delta), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = 10000000 }, 0, long.MaxValue, E),
			new AvOption("f_strict", "how strictly to follow the standards (deprecated; use strict, save via avconv)", nameof(AvFormatContext.Strict_Std_Compliance), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = Default }, c_int.MinValue, c_int.MaxValue, D | E, "strict"),
			new AvOption("strict", "how strictly to follow the standards", nameof(AvFormatContext.Strict_Std_Compliance), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = Default }, c_int.MinValue, c_int.MaxValue, D | E, "strict"),
			new AvOption("very", "strictly conform to a older more strict version of the spec or reference software", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)FFCompliance.Very_Strict }, c_int.MinValue, c_int.MaxValue, D | E, "strict"),
			new AvOption("strict", "strictly conform to all the things in the spec no matter what the consequences", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)FFCompliance.Strict }, c_int.MinValue, c_int.MaxValue, D | E, "strict"),
			new AvOption("normal", null, null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)FFCompliance.Normal }, c_int.MinValue, c_int.MaxValue, D | E, "strict"),
			new AvOption("unofficial", "allow unofficial extensions", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)FFCompliance.Unofficial }, c_int.MinValue, c_int.MaxValue, D | E, "strict"),
			new AvOption("experimental", "allow non-standardized experimental variants", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)FFCompliance.Experimental }, c_int.MinValue, c_int.MaxValue, D | E, "strict"),
			new AvOption("max_ts_probe", "maximum number of packets to read while waiting for the first timestamp", nameof(AvFormatContext.Max_Ts_Probe), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 50 }, 0, c_int.MaxValue, D),
			new AvOption("avoid_negative_ts", "shift timestamps so they start at 0", nameof(AvFormatContext.Avoid_Negative_Ts), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = -1 }, -1, 2, E, "avoid_negative_ts"),
			new AvOption("auto", "enabled when required by target format", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtAvoidNegTs.Auto }, c_int.MinValue, c_int.MaxValue, E, "avoid_negative_ts"),
			new AvOption("disabled", "do not change timestamps", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtAvoidNegTs.Disabled }, c_int.MinValue, c_int.MaxValue, E, "avoid_negative_ts"),
			new AvOption("make_non_negative", "shift timestamps so they are non negative", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtAvoidNegTs.Make_Non_Negative }, c_int.MinValue, c_int.MaxValue, E, "avoid_negative_ts"),
			new AvOption("make_zero", "shift timestamps so they start at 0", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)AvFmtAvoidNegTs.Make_Zero }, c_int.MinValue, c_int.MaxValue, E, "avoid_negative_ts"),
			new AvOption("dump_separator", "set information dump field separator", nameof(AvFormatContext.Dump_Separator), AvOptionType.String, new AvOption.DefaultValueUnion { Str = ", ".ToCharPointer() }, 0, 0, D | E),
			new AvOption("codec_whitelist", "List of decoders that are allowed to be used", nameof(AvFormatContext.Codec_Whitelist), AvOptionType.String, new AvOption.DefaultValueUnion { Str = null }, 0, 0, D),
			new AvOption("format_whitelist", "List of demuxers that are allowed to be used", nameof(AvFormatContext.Format_Whitelist), AvOptionType.String, new AvOption.DefaultValueUnion { Str = null }, 0, 0, D),
			new AvOption("protocol_whitelist", "List of protocols that are allowed to be used", nameof(AvFormatContext.Protocol_Whitelist), AvOptionType.String, new AvOption.DefaultValueUnion { Str = null }, 0, 0, D),
			new AvOption("protocol_blacklist", "List of protocols that are not allowed to be used", nameof(AvFormatContext.Protocol_Blacklist), AvOptionType.String, new AvOption.DefaultValueUnion { Str = null }, 0, 0, D),
			new AvOption("max_streams", "maximum number of streams", nameof(AvFormatContext.Max_Streams), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 1000 }, 0, c_int.MaxValue, D),
			new AvOption("skip_estimate_duration_from_pts", "skip duration calculation in estimate_timings_from_pts", nameof(AvFormatContext.Skip_Estimate_Duration_From_Pts), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 0 }, 0, 1, D),
			new AvOption("max_probe_packets", "Maximum number of packets to probe a codec", nameof(AvFormatContext.Max_Probe_Packets), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 2500 }, 0, c_int.MaxValue, D),
			new AvOption("duration_probesize", "Maximum number of bytes to probe the durations of the streams in estimate_timings_from_pts", nameof(AvFormatContext.Duration_ProbeSize), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = 0 }, 0, long.MaxValue, D),
			new AvOption()
		];
	}
}
