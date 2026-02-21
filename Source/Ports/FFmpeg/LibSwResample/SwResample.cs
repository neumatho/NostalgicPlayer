/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
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
	public static class SwResample
	{
		private const c_int Align = 32;

		/********************************************************************/
		/// <summary>
		/// Allocate SwrContext if needed and set/reset common parameters.
		///
		/// This function does not require *ps to be allocated with
		/// swr_alloc(). On the other hand, swr_alloc() can use
		/// swr_alloc_set_opts2() to set the parameters on the allocated
		/// context
		/// </summary>
		/********************************************************************/
		public static c_int Swr_Alloc_Set_Opts2(ref SwrContext ps, AvChannelLayout out_Ch_Layout, AvSampleFormat out_Sample_Fmt, c_int out_Sample_Rate, AvChannelLayout in_Ch_Layout, AvSampleFormat in_Sample_Fmt, c_int in_Sample_Rate, c_int log_Offset, IClass log_Ctx)//XX 40
		{
			SwrContext s = ps;

			if (s == null)
				s = Options.Swr_Alloc();

			if (s == null)
				return Error.ENOMEM;

			ps = s;

			s.Log_Level_Offset = log_Offset;
			s.Log_Ctx = log_Ctx;

			c_int ret = Opt.Av_Opt_Set_ChLayout(s, "ochl".ToCharPointer(), out_Ch_Layout, AvOptSearch.None);

			if (ret < 0)
				goto Fail;

			ret = Opt.Av_Opt_Set_Int(s, "osf".ToCharPointer(), (int64_t)out_Sample_Fmt, AvOptSearch.None);

			if (ret < 0)
				goto Fail;

			ret = Opt.Av_Opt_Set_Int(s, "osr".ToCharPointer(), out_Sample_Rate, AvOptSearch.None);

			if (ret < 0)
				goto Fail;

			ret = Opt.Av_Opt_Set_ChLayout(s, "ichl".ToCharPointer(), in_Ch_Layout, AvOptSearch.None);

			if (ret < 0)
				goto Fail;

			ret = Opt.Av_Opt_Set_Int(s, "isf".ToCharPointer(), (int64_t)in_Sample_Fmt, AvOptSearch.None);

			if (ret < 0)
				goto Fail;

			ret = Opt.Av_Opt_Set_Int(s, "isr".ToCharPointer(), in_Sample_Rate, AvOptSearch.None);

			if (ret < 0)
				goto Fail;

			return 0;

			Fail:
			Log.Av_Log(s, Log.Av_Log_Error, "Failed to set option\n");

			Swr_Free(ref ps);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Free the given SwrContext and set the pointer to NULL
		/// </summary>
		/********************************************************************/
		public static void Swr_Free(ref SwrContext ss)//XX 119
		{
			SwrContext s = ss;

			if (s != null)
			{
				Clear_Context(s);

				Channel_Layout.Av_Channel_Layout_Uninit(s.User_In_ChLayout);
				Channel_Layout.Av_Channel_Layout_Uninit(s.User_Out_ChLayout);
				Channel_Layout.Av_Channel_Layout_Uninit(s.User_Used_ChLayout);

				if (s.Resampler != null)
					s.Resampler.Free(ref s.Resample);
			}

			Mem.Av_FreeP(ref ss);
		}



		/********************************************************************/
		/// <summary>
		/// Closes the context so that swr_is_initialized() returns 0.
		///
		/// The context can be brought back to life by running swr_init(),
		/// swr_init() can also be used without swr_close().
		/// This function is mainly provided for simplifying the usecase
		/// where one tries to support libavresample and libswresample
		/// </summary>
		/********************************************************************/
		public static void Swr_Close(SwrContext s)//XX 134
		{
			Clear_Context(s);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize context after user parameters have been set.
		/// Note that the context must be configured using the AVOption API
		/// </summary>
		/********************************************************************/
		public static c_int Swr_Init(SwrContext s)//XX 138
		{
			c_int ret;
			CPointer<char> l1 = new CPointer<char>(1024);
			CPointer<char> l2 = new CPointer<char>(1024);

			Clear_Context(s);

			if ((c_uint)s.In_Sample_Fmt >= (c_uint)AvSampleFormat.Nb)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Requested input sample format %d is invalid\n", s.In_Sample_Fmt);

				return Error.EINVAL;
			}

			if ((c_uint)s.Out_Sample_Fmt >= (c_uint)AvSampleFormat.Nb)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Requested output sample format %d is invalid\n", s.Out_Sample_Fmt);

				return Error.EINVAL;
			}

			if (s.In_Sample_Rate <= 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Requested input sample rate %d is invalid\n", s.In_Sample_Rate);

				return Error.EINVAL;
			}

			if (s.Out_Sample_Rate <= 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Requested output sample rate %d is invalid\n", s.Out_Sample_Rate);

				return Error.EINVAL;
			}

			s.Out.Ch_Count = s.User_Out_ChLayout.Nb_Channels;
			s.In.Ch_Count = s.User_In_ChLayout.Nb_Channels;

			if (((ret = Channel_Layout.Av_Channel_Layout_Check(s.User_In_ChLayout)) == 0) || (s.User_In_ChLayout.Nb_Channels > SwrConstants.Swr_Ch_Max))
			{
				if (ret != 0)
					Channel_Layout.Av_Channel_Layout_Describe(s.User_In_ChLayout, l1, (size_t)l1.Length);

				Log.Av_Log(s, Log.Av_Log_Warning, "Input channel layout \"%s\" is invalid or unsupported.\n", ret != 0 ? l1 : string.Empty);

				return Error.EINVAL;
			}

			if (((ret = Channel_Layout.Av_Channel_Layout_Check(s.User_Out_ChLayout)) == 0) || (s.User_Out_ChLayout.Nb_Channels > SwrConstants.Swr_Ch_Max))
			{
				if (ret != 0)
					Channel_Layout.Av_Channel_Layout_Describe(s.User_Out_ChLayout, l2, (size_t)l2.Length);

				Log.Av_Log(s, Log.Av_Log_Warning, "Output channel layout \"%s\" is invalid or unsupported.\n", ret != 0 ? l2 : string.Empty);

				return Error.EINVAL;
			}

			ret = Channel_Layout.Av_Channel_Layout_Copy(s.In_Ch_Layout, s.User_In_ChLayout);
			ret |= Channel_Layout.Av_Channel_Layout_Copy(s.Out_Ch_Layout, s.User_Out_ChLayout);
			ret |= Channel_Layout.Av_Channel_Layout_Copy(s.Used_Ch_Layout, s.User_Used_ChLayout);

			if (ret < 0)
				return ret;

			s.Int_Sample_Fmt = s.User_Int_Sample_Fmt;

			s.Dither.Method = s.User_Dither_Method;

			switch (s.Engine)
			{
				case SwrEngine.Swr:
				{
					s.Resampler = Resample.swri_Resampler;
					break;
				}

				default:
				{
					Log.Av_Log(s, Log.Av_Log_Error, "Requested resampling engine is unavailable\n");

					return Error.EINVAL;
				}
			}

			if (Channel_Layout.Av_Channel_Layout_Check(s.Used_Ch_Layout) == 0)
				Channel_Layout.Av_Channel_Layout_Default(ref s.Used_Ch_Layout, s.In.Ch_Count);

			if (s.Used_Ch_Layout.Nb_Channels != s.In_Ch_Layout.Nb_Channels)
				Channel_Layout.Av_Channel_Layout_Uninit(s.In_Ch_Layout);

			if (s.Used_Ch_Layout.Order == AvChannelOrder.Unspec)
				Channel_Layout.Av_Channel_Layout_Default(ref s.Used_Ch_Layout, s.Used_Ch_Layout.Nb_Channels);

			if (s.In_Ch_Layout.Order == AvChannelOrder.Unspec)
			{
				ret = Channel_Layout.Av_Channel_Layout_Copy(s.In_Ch_Layout, s.Used_Ch_Layout);

				if (ret < 0)
					return ret;
			}

			if (s.Out_Ch_Layout.Order == AvChannelOrder.Unspec)
				Channel_Layout.Av_Channel_Layout_Default(ref s.Out_Ch_Layout, s.Out_Ch_Layout.Nb_Channels);

			s.Rematrix = (Channel_Layout.Av_Channel_Layout_Compare(s.Out_Ch_Layout, s.In_Ch_Layout) != 0) || (s.Rematrix_Volume != 1.0f) || (s.Rematrix_Custom != 0) ? 1 : 0;

			if (s.Int_Sample_Fmt == AvSampleFormat.None)
			{
				// 16 bit or less to 16 bit or less with the same sample rate
				if ((SampleFmt.Av_Get_Bytes_Per_Sample(s.In_Sample_Fmt) <= 2) && (SampleFmt.Av_Get_Bytes_Per_Sample(s.Out_Sample_Fmt) <= 2) && (s.Out_Sample_Rate == s.In_Sample_Rate))
				{
					s.Int_Sample_Fmt = AvSampleFormat.S16P;
				}
				// 8 -> 8, 16 -> 8, 8 -> 16
				else if ((SampleFmt.Av_Get_Bytes_Per_Sample(s.In_Sample_Fmt) + SampleFmt.Av_Get_Bytes_Per_Sample(s.Out_Sample_Fmt) <= 3))
					s.Int_Sample_Fmt = AvSampleFormat.S16P;
				else if ((SampleFmt.Av_Get_Bytes_Per_Sample(s.In_Sample_Fmt) <= 2) && (s.Rematrix == 0) && (s.Out_Sample_Rate == s.In_Sample_Rate) && ((s.Flags & SwrFlag.Resample) == 0))
					s.Int_Sample_Fmt = AvSampleFormat.S16P;
				else if ((SampleFmt.Av_Get_Planar_Sample_Fmt(s.In_Sample_Fmt) == AvSampleFormat.S32P) && (SampleFmt.Av_Get_Planar_Sample_Fmt(s.Out_Sample_Fmt) == AvSampleFormat.S32P) && (s.Rematrix == 0) && (s.Out_Sample_Rate == s.In_Sample_Rate) && ((s.Flags & SwrFlag.Resample) == 0) && (s.Engine != SwrEngine.Soxr))
					s.Int_Sample_Fmt = AvSampleFormat.S32P;
				else if (SampleFmt.Av_Get_Bytes_Per_Sample(s.In_Sample_Fmt) <= 4)
					s.Int_Sample_Fmt = AvSampleFormat.FltP;
				else
					s.Int_Sample_Fmt = AvSampleFormat.DblP;
			}

			Log.Av_Log(s, Log.Av_Log_Debug, "Using %s internally between filters\n", SampleFmt.Av_Get_Sample_Fmt_Name(s.Int_Sample_Fmt));

			if ((s.Int_Sample_Fmt != AvSampleFormat.S16P) && (s.Int_Sample_Fmt != AvSampleFormat.S32P) && (s.Int_Sample_Fmt != AvSampleFormat.S64P) && (s.Int_Sample_Fmt != AvSampleFormat.FltP) && (s.Int_Sample_Fmt != AvSampleFormat.DblP))
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Requested sample format %s is not supported internally, s16p/s32p/s64p/fltp/dblp are supported\n", SampleFmt.Av_Get_Sample_Fmt_Name(s.Int_Sample_Fmt));

				return Error.EINVAL;
			}

			Set_AudioData_Fmt(s.In, s.In_Sample_Fmt);
			Set_AudioData_Fmt(s.Out, s.Out_Sample_Fmt);

			if (s.FirstPts_In_Samples != UtilConstants.Av_NoPts_Value)
			{
				if ((s.Async == 0) && (s.Min_Compensation >= (CMath.FLT_MAX / 2)))
					s.Async = 1;

				if (s.FirstPts == UtilConstants.Av_NoPts_Value)
					s.FirstPts = s.OutPts = s.FirstPts_In_Samples * s.Out_Sample_Rate;
				else
					s.FirstPts = UtilConstants.Av_NoPts_Value;

				if (s.Async != 0)
				{
					if (s.Min_Compensation >= (CMath.FLT_MAX / 2))
						s.Min_Compensation = 0.001f;

					if (s.Async > 1.0001f)
						s.Max_Soft_Compensation = s.Async / s.In_Sample_Rate;
				}
			}

			if ((s.Out_Sample_Rate != s.In_Sample_Rate) || ((s.Flags & SwrFlag.Resample) != 0))
			{
				s.Resample = s.Resampler.Init(s.Resample, s.Out_Sample_Rate, s.In_Sample_Rate, s.Filter_Size, s.Phase_Shift, s.Linear_Interp, s.Cutoff, s.Int_Sample_Fmt, s.Filter_Type, s.Kaiser_Beta, s.Precision, s.Cheby, s.Exact_Rational);

				if (s.Resample == null)
				{
					Log.Av_Log(s, Log.Av_Log_Error, "Failed to initialize resampler\n");

					return Error.ENOMEM;
				}
			}
			else
				s.Resampler.Free(ref s.Resample);

			if ((s.Int_Sample_Fmt != AvSampleFormat.S16P) && (s.Int_Sample_Fmt != AvSampleFormat.S32P) && (s.Int_Sample_Fmt != AvSampleFormat.FltP) && (s.Int_Sample_Fmt != AvSampleFormat.DblP) && (s.Resample != null))
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Resampling only supported with internal s16p/s32p/fltp/dblp\n");

				ret = Error.EINVAL;
				goto Fail;
			}

			if (s.In.Ch_Count == 0)
				s.In.Ch_Count = s.In_Ch_Layout.Nb_Channels;

			if (Channel_Layout.Av_Channel_Layout_Check(s.Used_Ch_Layout) == 0)
				Channel_Layout.Av_Channel_Layout_Default(ref s.Used_Ch_Layout, s.In.Ch_Count);

			if (s.Out.Ch_Count == 0)
				s.Out.Ch_Count = s.Out_Ch_Layout.Nb_Channels;

			if (s.In.Ch_Count == 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Input channel count and layout are unset\n");

				ret = Error.EINVAL;
				goto Fail;
			}

			Channel_Layout.Av_Channel_Layout_Describe(s.Out_Ch_Layout, l2, (size_t)l2.Length);
			Channel_Layout.Av_Channel_Layout_Describe(s.In_Ch_Layout, l1, (size_t)l1.Length);

			if ((s.In_Ch_Layout.Order != AvChannelOrder.Unspec) && (s.Used_Ch_Layout.Nb_Channels != s.In_Ch_Layout.Nb_Channels))
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Input channel layout %s mismatches specified channel count %d\n", l1, s.Used_Ch_Layout.Nb_Channels);

				ret = Error.EINVAL;
				goto Fail;
			}

			if (((s.Out_Ch_Layout.Order == AvChannelOrder.Unspec) || (s.In_Ch_Layout.Order == AvChannelOrder.Unspec)) && (s.Used_Ch_Layout.Nb_Channels != s.Out.Ch_Count) && (s.Rematrix_Custom == 0))
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Rematrix is needed between %s and %s but there is not enough information to do it\n", l1, l2);

				ret = Error.EINVAL;
				goto Fail;
			}

			s.In.CopyTo(s.In_Buffer);
			s.In.CopyTo(s.Silence);
			s.Out.CopyTo(s.Drop_Temp);

			ret = Dither.Swri_Dither_Init(s, s.Out_Sample_Fmt, s.Int_Sample_Fmt);

			if (ret < 0)
				goto Fail;

			if ((s.Resample == null) && (s.Rematrix == 0) && s.Channel_Map.IsNull && (s.Dither.Method == SwrDitherType.None))
			{
				s.Full_Convert = AudioConvert_.Swri_Audio_Convert_Alloc(s.Out_Sample_Fmt, s.In_Sample_Fmt, s.In.Ch_Count, null, 0);

				return 0;
			}

			s.In_Convert = AudioConvert_.Swri_Audio_Convert_Alloc(s.Int_Sample_Fmt, s.In_Sample_Fmt, s.Used_Ch_Layout.Nb_Channels, s.Channel_Map, 0);
			s.Out_Convert = AudioConvert_.Swri_Audio_Convert_Alloc(s.Out_Sample_Fmt, s.Int_Sample_Fmt, s.Out.Ch_Count, null, 0);

			if ((s.In_Convert == null) || (s.Out_Convert == null))
			{
				ret = Error.ENOMEM;

				goto Fail;
			}

			s.In.CopyTo(s.PostIn);
			s.Out.CopyTo(s.PreOut);
			s.In.CopyTo(s.MidBuf);

			if (s.Channel_Map.IsNotNull)
			{
				s.PostIn.Ch_Count = s.MidBuf.Ch_Count = s.Used_Ch_Layout.Nb_Channels;

				if (s.Resample != null)
					s.In_Buffer.Ch_Count = s.Used_Ch_Layout.Nb_Channels;
			}

			if (s.Resample_First == 0)
			{
				s.MidBuf.Ch_Count = s.Out.Ch_Count;

				if (s.Resample != null)
					s.In_Buffer.Ch_Count = s.Out.Ch_Count;
			}

			Set_AudioData_Fmt(s.PostIn, s.Int_Sample_Fmt);
			Set_AudioData_Fmt(s.MidBuf, s.Int_Sample_Fmt);
			Set_AudioData_Fmt(s.PreOut, s.Int_Sample_Fmt);

			if (s.Resample != null)
				Set_AudioData_Fmt(s.In_Buffer, s.Int_Sample_Fmt);

			s.PreOut.CopyTo(s.Dither.Noise);
			s.PreOut.CopyTo(s.Dither.Temp);

			if (s.Dither.Method > SwrDitherType.Ns)
			{
				s.Dither.Noise.Bps = 4;
				s.Dither.Noise.Fmt = AvSampleFormat.FltP;
				s.Dither.Noise_Scale = 1;
			}

			if ((s.Rematrix != 0) || (s.Dither.Method != SwrDitherType.None))
			{
				ret = Rematrix.Swri_Rematrix_Init(s);

				if (ret < 0)
					goto Fail;
			}

			return 0;

			Fail:
			Swr_Close(s);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static c_int Swri_Realloc_Audio(AudioData a, c_int count)//XX 400
		{
			if ((count < 0) || (count > (c_int.MaxValue / 2 / a.Bps / a.Ch_Count)))
				return Error.EINVAL;

			if (a.Count >= count)
				return 0;

			count *= 2;

			c_int countB = Macros.FFAlign(count * a.Bps, Align);
			AudioData old = a.MakeDeepClone();

			a.Data = Mem.Av_CAlloc<uint8_t>((size_t)(countB * a.Ch_Count));

			if (a.Data.IsNull)
				return Error.ENOMEM;

			for (c_int i = 0; i < a.Ch_Count; i++)
			{
				a.Ch[i] = a.Data + (i * (a.Planar != 0 ? countB : a.Bps));

				if ((a.Count != 0) && (a.Planar != 0))
					CMemory.memcpy(a.Ch[i], old.Ch[i], (size_t)(a.Count * a.Bps));
			}

			if ((a.Count != 0) && (a.Planar == 0))
				CMemory.memcpy(a.Ch[0], old.Ch[0], (size_t)(a.Count * a.Ch_Count * a.Bps));

			Mem.Av_FreeP(ref old.Data);

			a.Count = count;

			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// Check whether an swr context has been initialized or not
		/// </summary>
		/********************************************************************/
		public static c_int Swr_Is_Initialized(SwrContext s)//XX 713
		{
			return s.In_Buffer.Ch_Count != 0 ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Convert audio.
		///
		/// in and in_count can be set to 0 to flush the last few samples
		/// out at the end.
		///
		/// If more input is provided than output space, then the input will
		/// be buffered. You can avoid this buffering by using
		/// swr_get_out_samples() to retrieve an upper bound on the required
		/// number of output samples for the given number of input samples.
		/// Conversion will run directly without copying whenever possible
		/// </summary>
		/********************************************************************/
		public static c_int Swr_Convert(SwrContext s, CPointer<CPointer<uint8_t>> out_Arg, c_int out_Count, CPointer<CPointer<uint8_t>> in_Arg, c_int in_Count)//XX 717
		{
			const c_int Max_Drop_Step = 16384;

			AudioData @in = s.In;
			AudioData @out = s.Out;

			if (Swr_Is_Initialized(s) == 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Context has not been initialized\n");

				return Error.EINVAL;
			}

			while (s.Drop_Output > 0)
			{
				CPointer<uint8_t>[] tmp_Arg = new CPointer<uint8_t>[SwrConstants.Swr_Ch_Max];

				c_int ret = Swri_Realloc_Audio(s.Drop_Temp, Macros.FFMin(s.Drop_Output, Max_Drop_Step));

				if (ret < 0)
					return ret;

				ReverseFill_AudioData(s.Drop_Temp, tmp_Arg);
				s.Drop_Output *= -1;

				ret = Swr_Convert(s, tmp_Arg, Macros.FFMin(-s.Drop_Output, Max_Drop_Step), in_Arg, in_Count);
				s.Drop_Output *= -1;
				in_Count = 0;

				if (ret > 0)
				{
					s.Drop_Output -= ret;

					if ((s.Drop_Output == 0) && out_Arg.IsNull)
						return 0;

					continue;
				}

				return 0;
			}

			if (in_Arg.IsNull)
			{
				if (s.Resample != null)
				{
					if (s.Flushed == 0)
						s.Resampler.Flush(s);

					s.Resample_In_Constraint = 0;
					s.Flushed = 1;
				}
				else if (s.In_Buffer_Count == 0)
					return 0;
			}
			else
				Fill_AudioData(@in, in_Arg);

			Fill_AudioData(@out, out_Arg);

			if (s.Resample != null)
			{
				c_int ret = Swr_Convert_Internal(s, @out, out_Count, @in, in_Count);

				if ((ret > 0) && (s.Drop_Output == 0))
					s.OutPts += ret * (int64_t)s.In_Sample_Rate;

				return ret;
			}
			else
			{
				AudioData tmp = @in.MakeDeepClone();
				c_int ret2 = 0;
				c_int ret;
				c_int size = Macros.FFMin(out_Count, s.In_Buffer_Count);

				if (size != 0)
				{
					Buf_Set(tmp, s.In_Buffer, s.In_Buffer_Index);

					ret = Swr_Convert_Internal(s, @out, size, tmp, size);

					if (ret < 0)
						return ret;

					ret2 = ret;
					s.In_Buffer_Count -= ret;
					s.In_Buffer_Index += ret;

					Buf_Set(@out, @out, ret);
					out_Count -= ret;

					if (s.In_Buffer_Count == 0)
						s.In_Buffer_Index = 0;
				}

				if (in_Count != 0)
				{
					size = s.In_Buffer_Index + s.In_Buffer_Count + in_Count - out_Count;

					if (in_Count > out_Count)
					{
						if ((size > s.In_Buffer.Count) && ((s.In_Buffer_Count + in_Count - out_Count) <= s.In_Buffer_Index))
						{
							Buf_Set(tmp, s.In_Buffer, s.In_Buffer_Index);
							Copy(s.In_Buffer, tmp, s.In_Buffer_Count);

							s.In_Buffer_Index = 0;
						}
						else
						{
							ret = Swri_Realloc_Audio(s.In_Buffer, size);

							if (ret < 0)
								return ret;
						}
					}

					if (out_Count != 0)
					{
						size = Macros.FFMin(in_Count, out_Count);

						ret = Swr_Convert_Internal(s, @out, size, @in, size);

						if (ret < 0)
							return ret;

						Buf_Set(@in, @in, ret);
						in_Count -= ret;
						ret2 += ret;
					}

					if (in_Count != 0)
					{
						Buf_Set(tmp, s.In_Buffer, s.In_Buffer_Index + s.In_Buffer_Count);
						Copy(tmp, @in, in_Count);

						s.In_Buffer_Count += in_Count;
					}
				}

				if ((ret2 > 0) && (s.Drop_Output == 0))
					s.OutPts += ret2 * (int64_t)s.In_Sample_Rate;

				return ret2;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Set_AudioData_Fmt(AudioData a, AvSampleFormat fmt)//XX 80
		{
			a.Fmt = fmt;
			a.Bps = SampleFmt.Av_Get_Bytes_Per_Sample(fmt);
			a.Planar = SampleFmt.Av_Sample_Fmt_Is_Planar(fmt);

			if (a.Ch_Count == 1)
				a.Planar = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Temp(AudioData a)//XX 88
		{
			Mem.Av_Free(a.Data);

			a.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Clear_Context(SwrContext s)//XX 93
		{
			s.In_Buffer_Index = 0;
			s.In_Buffer_Count = 0;
			s.Resample_In_Constraint = 0;

			CMemory.memset<CPointer<uint8_t>>(s.In.Ch, null, (size_t)s.In.Ch.Length);
			CMemory.memset<CPointer<uint8_t>>(s.Out.Ch, null, (size_t)s.Out.Ch.Length);

			Free_Temp(s.PostIn);
			Free_Temp(s.MidBuf);
			Free_Temp(s.PreOut);
			Free_Temp(s.In_Buffer);
			Free_Temp(s.Silence);
			Free_Temp(s.Drop_Temp);
			Free_Temp(s.Dither.Noise);
			Free_Temp(s.Dither.Temp);

			Channel_Layout.Av_Channel_Layout_Uninit(s.In_Ch_Layout);
			Channel_Layout.Av_Channel_Layout_Uninit(s.Out_Ch_Layout);
			Channel_Layout.Av_Channel_Layout_Uninit(s.Used_Ch_Layout);

			AudioConvert_.Swri_Audio_Convert_Free(ref s.In_Convert);
			AudioConvert_.Swri_Audio_Convert_Free(ref s.Out_Convert);
			AudioConvert_.Swri_Audio_Convert_Free(ref s.Full_Convert);
			Rematrix.Swri_Rematrix_Free(s);

			s.Delayed_Samples_Fixup = 0;
			s.Flushed = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Copy(AudioData @out, AudioData @in, c_int count)//XX 432
		{
			if (@out.Planar != 0)
			{
				for (c_int ch = 0; ch < @out.Ch_Count; ch++)
					CMemory.memcpy(@out.Ch[ch], @in.Ch[ch], (size_t)(count * @out.Bps));
			}
			else
				CMemory.memcpy(@out.Ch[0], @in.Ch[0], (size_t)(count * @out.Ch_Count * @out.Bps));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Fill_AudioData(AudioData @out, CPointer<CPointer<uint8_t>> in_Arg)//XX 445
		{
			if (in_Arg.IsNull)
				CMemory.memset<CPointer<uint8_t>>(@out.Ch, null, (size_t)@out.Ch.Length);
			else if (@out.Planar != 0)
			{
				for (c_int i = 0; i < @out.Ch_Count; i++)
					@out.Ch[i] = in_Arg[i];
			}
			else
			{
				for (c_int i = 0; i < @out.Ch_Count; i++)
					@out.Ch[i] = in_Arg[0] + (i * @out.Bps);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReverseFill_AudioData(AudioData @out, CPointer<CPointer<uint8_t>> in_Arg)//XX 459
		{
			if (@out.Planar != 0)
			{
				for (c_int i = 0; i < @out.Ch_Count; i++)
					in_Arg[i] = @out.Ch[i];
			}
			else
				in_Arg[0] = @out.Ch[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Buf_Set(AudioData @out, AudioData @in, c_int count)//XX 473
		{
			if (@in.Planar != 0)
			{
				for (c_int ch = 0; ch < @out.Ch_Count; ch++)
					@out.Ch[ch] = @in.Ch[ch] + (count * @out.Bps);
			}
			else
			{
				for (c_int ch = @out.Ch_Count - 1; ch >= 0; ch--)
					@out.Ch[ch] = @in.Ch[0] + ((ch + (count * @out.Ch_Count)) * @out.Bps);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Resample_(SwrContext s, AudioData out_Param, c_int out_Count, AudioData in_Param, c_int in_Count)//XX 488
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Swr_Convert_Internal(SwrContext s, AudioData @out, c_int out_Count, AudioData @in, c_int in_Count)//XX 583
		{
			if (s.Full_Convert != null)
			{
				AudioConvert_.Swri_Audio_Convert(s.Full_Convert, @out, @in, in_Count);

				return out_Count;
			}

			c_int ret = Swri_Realloc_Audio(s.PostIn, in_Count);

			if (ret < 0)
				return ret;

			if (s.Resample_First != 0)
			{
				ret = Swri_Realloc_Audio(s.MidBuf, out_Count);

				if (ret < 0)
					return ret;
			}
			else
			{
				ret = Swri_Realloc_Audio(s.MidBuf, in_Count);

				if (ret < 0)
					return ret;
			}

			ret = Swri_Realloc_Audio(s.PreOut, out_Count);

			if (ret < 0)
				return ret;

			AudioData postIn = s.PostIn;

			AudioData midBuf_Tmp = s.MidBuf.MakeDeepClone();
			AudioData midBuf = midBuf_Tmp;

			AudioData preOut_Tmp = s.PreOut.MakeDeepClone();
			AudioData preOut = preOut_Tmp;

			if ((s.Int_Sample_Fmt == s.In_Sample_Fmt) && (s.In.Planar != 0) && s.Channel_Map.IsNull)
				postIn = @in;

			if (s.Resample_First != 0 ? s.Resample == null : s.Rematrix == 0)
				midBuf = postIn;

			if (s.Resample_First != 0 ? s.Rematrix == 0 : s.Resample == null)
				preOut = midBuf;

			if ((s.Int_Sample_Fmt == s.Out_Sample_Fmt) && (s.Out.Planar != 0) && (!((s.Out_Sample_Fmt == AvSampleFormat.S32P) && ((s.Dither.Output_Sample_Bits & 31) != 0))))
			{
				if (preOut == @in)
				{
					out_Count = Macros.FFMin(out_Count, in_Count);
					Copy(@out, @in, out_Count);

					return out_Count;
				}
				else if (preOut == postIn)
					preOut = midBuf = postIn = @out;
				else if (preOut == midBuf)
					preOut = midBuf = @out;
				else
					preOut = @out;
			}

			if (@in != postIn)
				AudioConvert_.Swri_Audio_Convert(s.In_Convert, postIn, @in, in_Count);

			if (s.Resample_First != 0)
			{
				if (postIn != midBuf)
				{
					out_Count = Resample_(s, midBuf, out_Count, postIn, in_Count);

					if (out_Count < 0)
						return out_Count;
				}

				if (midBuf != preOut)
					Rematrix.Swri_Rematrix(s, preOut, midBuf, out_Count, preOut == @out ? 1 : 0);
			}
			else
			{
				if (postIn != midBuf)
					Rematrix.Swri_Rematrix(s, midBuf, postIn, in_Count, midBuf == @out ? 1 : 0);

				if (midBuf != preOut)
				{
					out_Count = Resample_(s, preOut, out_Count, midBuf, in_Count);

					if (out_Count < 0)
						return out_Count;
				}
			}

			if ((preOut != @out) && (out_Count != 0))
			{
				AudioData conv_Src = preOut;

				if (s.Dither.Method != SwrDitherType.None)
				{
					c_int dither_Count = Macros.FFMax(out_Count, 1 << 16);

					if (preOut == @in)
					{
						conv_Src = s.Dither.Temp;

						ret = Swri_Realloc_Audio(s.Dither.Temp, dither_Count);

						if (ret < 0)
							return ret;
					}

					ret = Swri_Realloc_Audio(s.Dither.Noise, dither_Count);

					if (ret < 0)
						return ret;

					if (ret != 0)
					{
						for (c_int ch = 0; ch < s.Dither.Noise.Ch_Count; ch++)
						{
							ret = Dither.Swri_Get_Dither(s, s.Dither.Noise.Ch[ch], s.Dither.Noise.Count, (c_uint)(((12345678913579UL * (c_ulong)ch) + 3141592) % 2718281828U), s.Dither.Noise.Fmt);

							if (ret < 0)
								return ret;
						}
					}

					if ((s.Dither.Noise_Pos + out_Count) > s.Dither.Noise.Count)
						s.Dither.Noise_Pos = 0;

					if (s.Dither.Method < SwrDitherType.Ns)
					{
						throw new NotImplementedException();
					}
					else
					{
						throw new NotImplementedException();
/*						switch (s.Int_Sample_Fmt)
						{
							case AvSampleFormat.S16P:
							{
								Swri_Noise_Shaping_Int16(s, conv_Src, preOut, s.Dither.Noise, out_Count);
								break;
							}

							case AvSampleFormat.S32P:
							{
								Swri_Noise_Shaping_Int32(s, conv_Src, preOut, s.Dither.Noise, out_Count);
								break;
							}

							case AvSampleFormat.FltP:
							{
								Swri_Noise_Shaping_Float(s, conv_Src, preOut, s.Dither.Noise, out_Count);
								break;
							}

							case AvSampleFormat.DblP:
							{
								Swri_Noise_Shaping_Double(s, conv_Src, preOut, s.Dither.Noise, out_Count);
								break;
							}
						}*/
					}

//					s.Dither.Noise_Pos += out_Count;
				}

				AudioConvert_.Swri_Audio_Convert(s.Out_Convert, @out, conv_Src, out_Count);
			}

			return out_Count;
		}
		#endregion
	}
}
