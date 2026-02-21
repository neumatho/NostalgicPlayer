/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample
{
	/// <summary>
	/// Audio conversion
	/// </summary>
	internal static class AudioConvert_
	{
		private static readonly SwrFunc.Conv_Func_Type_Delegate[] fmt_Pair_To_Conv_Functions = BuildFunctions();

		/********************************************************************/
		/// <summary>
		/// Return array with function pointers
		/// </summary>
		/********************************************************************/
		private static SwrFunc.Conv_Func_Type_Delegate[] BuildFunctions()
		{
			SwrFunc.Conv_Func_Type_Delegate[] result = new SwrFunc.Conv_Func_Type_Delegate[(c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Nb];

			result[(c_int)AvSampleFormat.U8 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.U8)] = Conv_U8_To_U8;
			result[(c_int)AvSampleFormat.S16 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.U8)] = Conv_U8_To_S16;
			result[(c_int)AvSampleFormat.S32 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.U8)] = Conv_U8_To_S32;
			result[(c_int)AvSampleFormat.Flt + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.U8)] = Conv_U8_To_Flt;
			result[(c_int)AvSampleFormat.Dbl + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.U8)] = Conv_U8_To_Dbl;
			result[(c_int)AvSampleFormat.S64 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.U8)] = Conv_U8_To_S64;

			result[(c_int)AvSampleFormat.U8 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S16)] = Conv_S16_To_U8;
			result[(c_int)AvSampleFormat.S16 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S16)] = Conv_S16_To_S16;
			result[(c_int)AvSampleFormat.S32 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S16)] = Conv_S16_To_S32;
			result[(c_int)AvSampleFormat.Flt + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S16)] = Conv_S16_To_Flt;
			result[(c_int)AvSampleFormat.Dbl + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S16)] = Conv_S16_To_Dbl;
			result[(c_int)AvSampleFormat.S64 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S16)] = Conv_S16_To_S64;

			result[(c_int)AvSampleFormat.U8 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S32)] = Conv_S32_To_U8;
			result[(c_int)AvSampleFormat.S16 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S32)] = Conv_S32_To_S16;
			result[(c_int)AvSampleFormat.S32 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S32)] = Conv_S32_To_S32;
			result[(c_int)AvSampleFormat.Flt + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S32)] = Conv_S32_To_Flt;
			result[(c_int)AvSampleFormat.Dbl + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S32)] = Conv_S32_To_Dbl;
			result[(c_int)AvSampleFormat.S64 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S32)] = Conv_S32_To_S64;

			result[(c_int)AvSampleFormat.U8 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Flt)] = Conv_Flt_To_U8;
			result[(c_int)AvSampleFormat.S16 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Flt)] = Conv_Flt_To_S16;
			result[(c_int)AvSampleFormat.S32 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Flt)] = Conv_Flt_To_S32;
			result[(c_int)AvSampleFormat.Flt + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Flt)] = Conv_Flt_To_Flt;
			result[(c_int)AvSampleFormat.Dbl + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Flt)] = Conv_Flt_To_Dbl;
			result[(c_int)AvSampleFormat.S64 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Flt)] = Conv_Flt_To_S64;

			result[(c_int)AvSampleFormat.U8 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Dbl)] = Conv_Dbl_To_U8;
			result[(c_int)AvSampleFormat.S16 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Dbl)] = Conv_Dbl_To_S16;
			result[(c_int)AvSampleFormat.S32 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Dbl)] = Conv_Dbl_To_S32;
			result[(c_int)AvSampleFormat.Flt + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Dbl)] = Conv_Dbl_To_Flt;
			result[(c_int)AvSampleFormat.Dbl + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Dbl)] = Conv_Dbl_To_Dbl;
			result[(c_int)AvSampleFormat.S64 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.Dbl)] = Conv_Dbl_To_S64;

			result[(c_int)AvSampleFormat.U8 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S64)] = Conv_S64_To_U8;
			result[(c_int)AvSampleFormat.S16 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S64)] = Conv_S64_To_S16;
			result[(c_int)AvSampleFormat.S32 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S64)] = Conv_S64_To_S32;
			result[(c_int)AvSampleFormat.Flt + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S64)] = Conv_S64_To_Flt;
			result[(c_int)AvSampleFormat.Dbl + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S64)] = Conv_S64_To_Dbl;
			result[(c_int)AvSampleFormat.S64 + ((c_int)AvSampleFormat.Nb * (c_int)AvSampleFormat.S64)] = Conv_S64_To_S64;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Create an audio sample format converter context
		/// </summary>
		/********************************************************************/
		public static AudioConvert Swri_Audio_Convert_Alloc(AvSampleFormat out_Fmt, AvSampleFormat in_Fmt, c_int channels, CPointer<c_int> ch_Map, c_int flags)//XX 145
		{
			SwrFunc.Conv_Func_Type_Delegate f = fmt_Pair_To_Conv_Functions[(c_int)SampleFmt.Av_Get_Packed_Sample_Fmt(out_Fmt) + ((c_int)AvSampleFormat.Nb * (c_int)SampleFmt.Av_Get_Packed_Sample_Fmt(in_Fmt))];

			if (f == null)
				return null;

			AudioConvert ctx = Mem.Av_MAlloczObj<AudioConvert>();

			if (ctx == null)
				return null;

			if (channels == 1)
			{
				in_Fmt = SampleFmt.Av_Get_Planar_Sample_Fmt(in_Fmt);
				out_Fmt = SampleFmt.Av_Get_Planar_Sample_Fmt(out_Fmt);
			}

			ctx.Channels = channels;
			ctx.Conv_F = f;
			ctx.Ch_Map = ch_Map;

			if ((in_Fmt == AvSampleFormat.U8) || (in_Fmt == AvSampleFormat.U8P))
				CMemory.memset<uint8_t>(ctx.Silence, 0x80, (size_t)ctx.Silence.Length);

			if ((out_Fmt == in_Fmt) && ch_Map.IsNull)
			{
				switch (SampleFmt.Av_Get_Bytes_Per_Sample(in_Fmt))
				{
					case 1:
					{
						ctx.Simd_F = Cpy1;
						break;
					}

					case 2:
					{
						ctx.Simd_F = Cpy2;
						break;
					}

					case 4:
					{
						ctx.Simd_F = Cpy4;
						break;
					}

					case 8:
					{
						ctx.Simd_F = Cpy8;
						break;
					}
				}
			}

			return ctx;
		}



		/********************************************************************/
		/// <summary>
		/// Free audio sample format converter context and set the pointer
		/// to NULL
		/// </summary>
		/********************************************************************/
		public static void Swri_Audio_Convert_Free(ref AudioConvert ctx)//XX 190
		{
			Mem.Av_FreeP(ref ctx);
		}



		/********************************************************************/
		/// <summary>
		/// Convert between audio sample formats
		/// </summary>
		/********************************************************************/
		public static c_int Swri_Audio_Convert(AudioConvert ctx, AudioData @out, AudioData @in, c_int len)//XX 195
		{
			c_int off = 0;
			c_int os = (@out.Planar != 0 ? 1 : @out.Ch_Count) * @out.Bps;

			if ((ctx.Simd_F != null) && ctx.Ch_Map.IsNull)
			{
				off = len & ~15;

				if (off > 0)
				{
					if (@out.Planar == @in.Planar)
					{
						c_int planes = @out.Planar != 0 ? @out.Ch_Count : 1;

						for (c_int ch = 0; ch < planes; ch++)
							ctx.Simd_F(@out.Ch[ch], @in.Ch[ch], off * (@out.Planar != 0 ? 1 : @out.Ch_Count));
					}
					else
						ctx.Simd_F(@out.Ch[0], @in.Ch[0], off);
				}

				if (off == len)
					return 0;
			}

			for (c_int ch = 0; ch < ctx.Channels; ch++)
			{
				c_int iCh = ctx.Ch_Map.IsNotNull ? ctx.Ch_Map[ch] : ch;
				c_int @is = iCh < 0 ? 0 : (@in.Planar != 0 ? 1 : @in.Ch_Count) * @in.Bps;
				CPointer<uint8_t> pi = iCh < 0 ? ctx.Silence : @in.Ch[iCh];
				CPointer<uint8_t> po = @out.Ch[ch];

				if (po.IsNull)
					continue;

				CPointer<uint8_t> end = po + (os * len);
				ctx.Conv_F(po + (off * os), pi + (off * @is), @is, os, end);
			}

			return 0;
		}

		#region Private methods

		#region Convert from U8
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_U8_To_U8(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<uint8_t> ppo = po;
			CPointer<uint8_t> ppi = pi;
			CPointer<uint8_t> pend = end;

			os /= sizeof(uint8_t);
			@is /= sizeof(uint8_t);

			CPointer<uint8_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_U8_To_S16(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int16_t> ppo = po.Cast<uint8_t, int16_t>();
			CPointer<uint8_t> ppi = pi;
			CPointer<int16_t> pend = end.Cast<uint8_t, int16_t>();

			os /= sizeof(int16_t);
			@is /= sizeof(uint8_t);

			CPointer<int16_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (int16_t)((ppi[0, @is] - 0x80U) << 8);
				ppo[0, os] = (int16_t)((ppi[0, @is] - 0x80U) << 8);
				ppo[0, os] = (int16_t)((ppi[0, @is] - 0x80U) << 8);
				ppo[0, os] = (int16_t)((ppi[0, @is] - 0x80U) << 8);
			}

			while (ppo < pend)
				ppo[0, os] = (int16_t)((ppi[0, @is] - 0x80U) << 8);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_U8_To_S32(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int32_t> ppo = po.Cast<uint8_t, int32_t>();
			CPointer<uint8_t> ppi = pi;
			CPointer<int32_t> pend = end.Cast<uint8_t, int32_t>();

			os /= sizeof(int32_t);
			@is /= sizeof(uint8_t);

			CPointer<int32_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (int32_t)((ppi[0, @is] - 0x80U) << 24);
				ppo[0, os] = (int32_t)((ppi[0, @is] - 0x80U) << 24);
				ppo[0, os] = (int32_t)((ppi[0, @is] - 0x80U) << 24);
				ppo[0, os] = (int32_t)((ppi[0, @is] - 0x80U) << 24);
			}

			while (ppo < pend)
				ppo[0, os] = (int32_t)((ppi[0, @is] - 0x80U) << 24);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_U8_To_S64(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int64_t> ppo = po.Cast<uint8_t, int64_t>();
			CPointer<uint8_t> ppi = pi;
			CPointer<int64_t> pend = end.Cast<uint8_t, int64_t>();

			os /= sizeof(int64_t);
			@is /= sizeof(uint8_t);

			CPointer<int64_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is] - 0x80U) << 56);
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is] - 0x80U) << 56);
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is] - 0x80U) << 56);
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is] - 0x80U) << 56);
			}

			while (ppo < pend)
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is] - 0x80U) << 56);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_U8_To_Flt(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_float> ppo = po.Cast<uint8_t, c_float>();
			CPointer<uint8_t> ppi = pi;
			CPointer<c_float> pend = end.Cast<uint8_t, c_float>();

			os /= sizeof(c_float);
			@is /= sizeof(uint8_t);

			CPointer<c_float> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0f / (1 << 7));
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0f / (1 << 7));
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0f / (1 << 7));
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0f / (1 << 7));
			}

			while (ppo < pend)
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0f / (1 << 7));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_U8_To_Dbl(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_double> ppo = po.Cast<uint8_t, c_double>();
			CPointer<uint8_t> ppi = pi;
			CPointer<c_double> pend = end.Cast<uint8_t, c_double>();

			os /= sizeof(c_double);
			@is /= sizeof(uint8_t);

			CPointer<c_double> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0 / (1 << 7));
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0 / (1 << 7));
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0 / (1 << 7));
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0 / (1 << 7));
			}

			while (ppo < pend)
				ppo[0, os] = (ppi[0, @is] - 0x80U) * (1.0 / (1 << 7));
		}
		#endregion

		#region Convert from S16
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S16_To_U8(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<uint8_t> ppo = po;
			CPointer<int16_t> ppi = pi.Cast<uint8_t, int16_t>();
			CPointer<uint8_t> pend = end;

			os /= sizeof(uint8_t);
			@is /= sizeof(int16_t);

			CPointer<uint8_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 8) + 0x80);
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 8) + 0x80);
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 8) + 0x80);
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 8) + 0x80);
			}

			while (ppo < pend)
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 8) + 0x80);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S16_To_S16(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int16_t> ppo = po.Cast<uint8_t, int16_t>();
			CPointer<int16_t> ppi = pi.Cast<uint8_t, int16_t>();
			CPointer<int16_t> pend = end.Cast<uint8_t, int16_t>();

			os /= sizeof(int16_t);
			@is /= sizeof(int16_t);

			CPointer<int16_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S16_To_S32(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int32_t> ppo = po.Cast<uint8_t, int32_t>();
			CPointer<int16_t> ppi = pi.Cast<uint8_t, int16_t>();
			CPointer<int32_t> pend = end.Cast<uint8_t, int32_t>();

			os /= sizeof(int32_t);
			@is /= sizeof(int16_t);

			CPointer<int32_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is] * (1 << 16);
				ppo[0, os] = ppi[0, @is] * (1 << 16);
				ppo[0, os] = ppi[0, @is] * (1 << 16);
				ppo[0, os] = ppi[0, @is] * (1 << 16);
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is] * (1 << 16);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S16_To_S64(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int64_t> ppo = po.Cast<uint8_t, int64_t>();
			CPointer<int16_t> ppi = pi.Cast<uint8_t, int16_t>();
			CPointer<int64_t> pend = end.Cast<uint8_t, int64_t>();

			os /= sizeof(int64_t);
			@is /= sizeof(int16_t);

			CPointer<int64_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 48);
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 48);
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 48);
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 48);
			}

			while (ppo < pend)
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 48);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S16_To_Flt(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_float> ppo = po.Cast<uint8_t, c_float>();
			CPointer<int16_t> ppi = pi.Cast<uint8_t, int16_t>();
			CPointer<c_float> pend = end.Cast<uint8_t, c_float>();

			os /= sizeof(c_float);
			@is /= sizeof(int16_t);

			CPointer<c_float> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is] * (1.0f / (1 << 15));
				ppo[0, os] = ppi[0, @is] * (1.0f / (1 << 15));
				ppo[0, os] = ppi[0, @is] * (1.0f / (1 << 15));
				ppo[0, os] = ppi[0, @is] * (1.0f / (1 << 15));
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is] * (1.0f / (1 << 15));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S16_To_Dbl(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_double> ppo = po.Cast<uint8_t, c_double>();
			CPointer<int16_t> ppi = pi.Cast<uint8_t, int16_t>();
			CPointer<c_double> pend = end.Cast<uint8_t, c_double>();

			os /= sizeof(c_double);
			@is /= sizeof(int16_t);

			CPointer<c_double> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is] * (1.0 / (1 << 15));
				ppo[0, os] = ppi[0, @is] * (1.0 / (1 << 15));
				ppo[0, os] = ppi[0, @is] * (1.0 / (1 << 15));
				ppo[0, os] = ppi[0, @is] * (1.0 / (1 << 15));
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is] * (1.0 / (1 << 15));
		}
		#endregion

		#region Convert from S32
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S32_To_U8(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<uint8_t> ppo = po;
			CPointer<int32_t> ppi = pi.Cast<uint8_t, int32_t>();
			CPointer<uint8_t> pend = end;

			os /= sizeof(uint8_t);
			@is /= sizeof(int32_t);

			CPointer<uint8_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 24) + 0x80);
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 24) + 0x80);
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 24) + 0x80);
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 24) + 0x80);
			}

			while (ppo < pend)
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 24) + 0x80);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S32_To_S16(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int16_t> ppo = po.Cast<uint8_t, int16_t>();
			CPointer<int32_t> ppi = pi.Cast<uint8_t, int32_t>();
			CPointer<int16_t> pend = end.Cast<uint8_t, int16_t>();

			os /= sizeof(int16_t);
			@is /= sizeof(int32_t);

			CPointer<int16_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 16);
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 16);
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 16);
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 16);
			}

			while (ppo < pend)
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 16);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S32_To_S32(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int32_t> ppo = po.Cast<uint8_t, int32_t>();
			CPointer<int32_t> ppi = pi.Cast<uint8_t, int32_t>();
			CPointer<int32_t> pend = end.Cast<uint8_t, int32_t>();

			os /= sizeof(int32_t);
			@is /= sizeof(int32_t);

			CPointer<int32_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S32_To_S64(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int64_t> ppo = po.Cast<uint8_t, int64_t>();
			CPointer<int32_t> ppi = pi.Cast<uint8_t, int32_t>();
			CPointer<int64_t> pend = end.Cast<uint8_t, int64_t>();

			os /= sizeof(int64_t);
			@is /= sizeof(int32_t);

			CPointer<int64_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 32);
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 32);
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 32);
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 32);
			}

			while (ppo < pend)
				ppo[0, os] = (int64_t)((uint64_t)(ppi[0, @is]) << 32);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S32_To_Flt(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_float> ppo = po.Cast<uint8_t, c_float>();
			CPointer<int32_t> ppi = pi.Cast<uint8_t, int32_t>();
			CPointer<c_float> pend = end.Cast<uint8_t, c_float>();

			os /= sizeof(c_float);
			@is /= sizeof(int32_t);

			CPointer<c_float> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is] * (1.0f / (1U << 31));
				ppo[0, os] = ppi[0, @is] * (1.0f / (1U << 31));
				ppo[0, os] = ppi[0, @is] * (1.0f / (1U << 31));
				ppo[0, os] = ppi[0, @is] * (1.0f / (1U << 31));
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is] * (1.0f / (1U << 31));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S32_To_Dbl(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_double> ppo = po.Cast<uint8_t, c_double>();
			CPointer<int32_t> ppi = pi.Cast<uint8_t, int32_t>();
			CPointer<c_double> pend = end.Cast<uint8_t, c_double>();

			os /= sizeof(c_double);
			@is /= sizeof(int32_t);

			CPointer<c_double> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is] * (1.0 / (1U << 31));
				ppo[0, os] = ppi[0, @is] * (1.0 / (1U << 31));
				ppo[0, os] = ppi[0, @is] * (1.0 / (1U << 31));
				ppo[0, os] = ppi[0, @is] * (1.0 / (1U << 31));
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is] * (1.0 / (1U << 31));
		}
		#endregion

		#region Convert from S64
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S64_To_U8(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<uint8_t> ppo = po;
			CPointer<int64_t> ppi = pi.Cast<uint8_t, int64_t>();
			CPointer<uint8_t> pend = end;

			os /= sizeof(uint8_t);
			@is /= sizeof(int64_t);

			CPointer<uint8_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 56) + 0x80);
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 56) + 0x80);
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 56) + 0x80);
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 56) + 0x80);
			}

			while (ppo < pend)
				ppo[0, os] = (uint8_t)((ppi[0, @is] >> 56) + 0x80);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S64_To_S16(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int16_t> ppo = po.Cast<uint8_t, int16_t>();
			CPointer<int64_t> ppi = pi.Cast<uint8_t, int64_t>();
			CPointer<int16_t> pend = end.Cast<uint8_t, int16_t>();

			os /= sizeof(int16_t);
			@is /= sizeof(int64_t);

			CPointer<int16_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 48);
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 48);
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 48);
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 48);
			}

			while (ppo < pend)
				ppo[0, os] = (int16_t)(ppi[0, @is] >> 48);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S64_To_S32(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int32_t> ppo = po.Cast<uint8_t, int32_t>();
			CPointer<int64_t> ppi = pi.Cast<uint8_t, int64_t>();
			CPointer<int32_t> pend = end.Cast<uint8_t, int32_t>();

			os /= sizeof(int32_t);
			@is /= sizeof(int64_t);

			CPointer<int32_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (int32_t)(ppi[0, @is] >> 32);
				ppo[0, os] = (int32_t)(ppi[0, @is] >> 32);
				ppo[0, os] = (int32_t)(ppi[0, @is] >> 32);
				ppo[0, os] = (int32_t)(ppi[0, @is] >> 32);
			}

			while (ppo < pend)
				ppo[0, os] = (int32_t)(ppi[0, @is] >> 32);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S64_To_S64(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int64_t> ppo = po.Cast<uint8_t, int64_t>();
			CPointer<int64_t> ppi = pi.Cast<uint8_t, int64_t>();
			CPointer<int64_t> pend = end.Cast<uint8_t, int64_t>();

			os /= sizeof(int64_t);
			@is /= sizeof(int64_t);

			CPointer<int64_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S64_To_Flt(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_float> ppo = po.Cast<uint8_t, c_float>();
			CPointer<int64_t> ppi = pi.Cast<uint8_t, int64_t>();
			CPointer<c_float> pend = end.Cast<uint8_t, c_float>();

			os /= sizeof(c_float);
			@is /= sizeof(int64_t);

			CPointer<c_float> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is] * (1.0f / ((uint64_t)1 << 63));
				ppo[0, os] = ppi[0, @is] * (1.0f / ((uint64_t)1 << 63));
				ppo[0, os] = ppi[0, @is] * (1.0f / ((uint64_t)1 << 63));
				ppo[0, os] = ppi[0, @is] * (1.0f / ((uint64_t)1 << 63));
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is] * (1.0f / ((uint64_t)1 << 63));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_S64_To_Dbl(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_double> ppo = po.Cast<uint8_t, c_double>();
			CPointer<int64_t> ppi = pi.Cast<uint8_t, int64_t>();
			CPointer<c_double> pend = end.Cast<uint8_t, c_double>();

			os /= sizeof(c_double);
			@is /= sizeof(int64_t);

			CPointer<c_double> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is] * (1.0 / ((uint64_t)1 << 63));
				ppo[0, os] = ppi[0, @is] * (1.0 / ((uint64_t)1 << 63));
				ppo[0, os] = ppi[0, @is] * (1.0 / ((uint64_t)1 << 63));
				ppo[0, os] = ppi[0, @is] * (1.0 / ((uint64_t)1 << 63));
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is] * (1.0 / ((uint64_t)1 << 63));
		}
		#endregion

		#region Convert from Flt
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Flt_To_U8(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<uint8_t> ppo = po;
			CPointer<c_float> ppi = pi.Cast<uint8_t, c_float>();
			CPointer<uint8_t> pend = end;

			os /= sizeof(uint8_t);
			@is /= sizeof(c_float);

			CPointer<uint8_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrintf(ppi[0, @is] * (1 << 7)) + 0x80);
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrintf(ppi[0, @is] * (1 << 7)) + 0x80);
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrintf(ppi[0, @is] * (1 << 7)) + 0x80);
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrintf(ppi[0, @is] * (1 << 7)) + 0x80);
			}

			while (ppo < pend)
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrintf(ppi[0, @is] * (1 << 7)) + 0x80);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Flt_To_S16(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int16_t> ppo = po.Cast<uint8_t, int16_t>();
			CPointer<c_float> ppi = pi.Cast<uint8_t, c_float>();
			CPointer<int16_t> pend = end.Cast<uint8_t, int16_t>();

			os /= sizeof(int16_t);
			@is /= sizeof(c_float);

			CPointer<int16_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrintf(ppi[0, @is] * (1 << 15)));
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrintf(ppi[0, @is] * (1 << 15)));
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrintf(ppi[0, @is] * (1 << 15)));
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrintf(ppi[0, @is] * (1 << 15)));
			}

			while (ppo < pend)
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrintf(ppi[0, @is] * (1 << 15)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Flt_To_S32(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int32_t> ppo = po.Cast<uint8_t, int32_t>();
			CPointer<c_float> ppi = pi.Cast<uint8_t, c_float>();
			CPointer<int32_t> pend = end.Cast<uint8_t, int32_t>();

			os /= sizeof(int32_t);
			@is /= sizeof(c_float);

			CPointer<int32_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrintf(ppi[0, @is] * (1U << 31)));
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrintf(ppi[0, @is] * (1U << 31)));
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrintf(ppi[0, @is] * (1U << 31)));
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrintf(ppi[0, @is] * (1U << 31)));
			}

			while (ppo < pend)
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrintf(ppi[0, @is] * (1U << 31)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Flt_To_S64(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int64_t> ppo = po.Cast<uint8_t, int64_t>();
			CPointer<c_float> ppi = pi.Cast<uint8_t, c_float>();
			CPointer<int64_t> pend = end.Cast<uint8_t, int64_t>();

			os /= sizeof(int64_t);
			@is /= sizeof(c_float);

			CPointer<int64_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = CMath.llrintf(ppi[0, @is] * ((uint64_t)1 << 63));
				ppo[0, os] = CMath.llrintf(ppi[0, @is] * ((uint64_t)1 << 63));
				ppo[0, os] = CMath.llrintf(ppi[0, @is] * ((uint64_t)1 << 63));
				ppo[0, os] = CMath.llrintf(ppi[0, @is] * ((uint64_t)1 << 63));
			}

			while (ppo < pend)
				ppo[0, os] = CMath.llrintf(ppi[0, @is] * ((uint64_t)1 << 63));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Flt_To_Flt(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_float> ppo = po.Cast<uint8_t, c_float>();
			CPointer<c_float> ppi = pi.Cast<uint8_t, c_float>();
			CPointer<c_float> pend = end.Cast<uint8_t, c_float>();

			os /= sizeof(c_float);
			@is /= sizeof(c_float);

			CPointer<c_float> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Flt_To_Dbl(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_double> ppo = po.Cast<uint8_t, c_double>();
			CPointer<c_float> ppi = pi.Cast<uint8_t, c_float>();
			CPointer<c_double> pend = end.Cast<uint8_t, c_double>();

			os /= sizeof(c_double);
			@is /= sizeof(c_float);

			CPointer<c_double> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is];
		}
		#endregion

		#region Convert from Dbl
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Dbl_To_U8(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<uint8_t> ppo = po;
			CPointer<c_double> ppi = pi.Cast<uint8_t, c_double>();
			CPointer<uint8_t> pend = end;

			os /= sizeof(uint8_t);
			@is /= sizeof(c_double);

			CPointer<uint8_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrint(ppi[0, @is] * (1 << 7)) + 0x80);
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrint(ppi[0, @is] * (1 << 7)) + 0x80);
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrint(ppi[0, @is] * (1 << 7)) + 0x80);
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrint(ppi[0, @is] * (1 << 7)) + 0x80);
			}

			while (ppo < pend)
				ppo[0, os] = Common.Av_Clip_UInt8(CMath.lrint(ppi[0, @is] * (1 << 7)) + 0x80);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Dbl_To_S16(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int16_t> ppo = po.Cast<uint8_t, int16_t>();
			CPointer<c_double> ppi = pi.Cast<uint8_t, c_double>();
			CPointer<int16_t> pend = end.Cast<uint8_t, int16_t>();

			os /= sizeof(int16_t);
			@is /= sizeof(c_double);

			CPointer<int16_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrint(ppi[0, @is] * (1 << 15)));
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrint(ppi[0, @is] * (1 << 15)));
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrint(ppi[0, @is] * (1 << 15)));
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrint(ppi[0, @is] * (1 << 15)));
			}

			while (ppo < pend)
				ppo[0, os] = Common.Av_Clip_Int16(CMath.lrint(ppi[0, @is] * (1 << 15)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Dbl_To_S32(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int32_t> ppo = po.Cast<uint8_t, int32_t>();
			CPointer<c_double> ppi = pi.Cast<uint8_t, c_double>();
			CPointer<int32_t> pend = end.Cast<uint8_t, int32_t>();

			os /= sizeof(int32_t);
			@is /= sizeof(c_double);

			CPointer<int32_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrint(ppi[0, @is] * (1U << 31)));
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrint(ppi[0, @is] * (1U << 31)));
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrint(ppi[0, @is] * (1U << 31)));
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrint(ppi[0, @is] * (1U << 31)));
			}

			while (ppo < pend)
				ppo[0, os] = Common.Av_ClipL_Int32(CMath.llrint(ppi[0, @is] * (1U << 31)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Dbl_To_S64(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<int64_t> ppo = po.Cast<uint8_t, int64_t>();
			CPointer<c_double> ppi = pi.Cast<uint8_t, c_double>();
			CPointer<int64_t> pend = end.Cast<uint8_t, int64_t>();

			os /= sizeof(int64_t);
			@is /= sizeof(c_double);

			CPointer<int64_t> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = CMath.llrint(ppi[0, @is] * ((uint64_t)1 << 63));
				ppo[0, os] = CMath.llrint(ppi[0, @is] * ((uint64_t)1 << 63));
				ppo[0, os] = CMath.llrint(ppi[0, @is] * ((uint64_t)1 << 63));
				ppo[0, os] = CMath.llrint(ppi[0, @is] * ((uint64_t)1 << 63));
			}

			while (ppo < pend)
				ppo[0, os] = CMath.llrint(ppi[0, @is] * ((uint64_t)1 << 63));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Dbl_To_Flt(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_float> ppo = po.Cast<uint8_t, c_float>();
			CPointer<c_double> ppi = pi.Cast<uint8_t, c_double>();
			CPointer<c_float> pend = end.Cast<uint8_t, c_float>();

			os /= sizeof(c_float);
			@is /= sizeof(c_double);

			CPointer<c_float> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = (c_float)ppi[0, @is];
				ppo[0, os] = (c_float)ppi[0, @is];
				ppo[0, os] = (c_float)ppi[0, @is];
				ppo[0, os] = (c_float)ppi[0, @is];
			}

			while (ppo < pend)
				ppo[0, os] = (c_float)ppi[0, @is];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Conv_Dbl_To_Dbl(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end)
		{
			CPointer<c_double> ppo = po.Cast<uint8_t, c_double>();
			CPointer<c_double> ppi = pi.Cast<uint8_t, c_double>();
			CPointer<c_double> pend = end.Cast<uint8_t, c_double>();

			os /= sizeof(c_double);
			@is /= sizeof(c_double);

			CPointer<c_double> pend2 = pend - (3 * os);

			while (ppo < pend2)
			{
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
				ppo[0, os] = ppi[0, @is];
			}

			while (ppo < pend)
				ppo[0, os] = ppi[0, @is];
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Cpy1(CPointer<uint8_t> dst, CPointer<uint8_t> src, c_int len)//XX 132
		{
			CMemory.memcpy(dst, src, (size_t)len);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Cpy2(CPointer<uint8_t> dst, CPointer<uint8_t> src, c_int len)//XX 135
		{
			CMemory.memcpy(dst, src, (size_t)(2 * len));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Cpy4(CPointer<uint8_t> dst, CPointer<uint8_t> src, c_int len)//XX 138
		{
			CMemory.memcpy(dst, src, (size_t)(4 * len));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Cpy8(CPointer<uint8_t> dst, CPointer<uint8_t> src, c_int len)//XX 141
		{
			CMemory.memcpy(dst, src, (size_t)(8 * len));
		}
		#endregion
	}
}
