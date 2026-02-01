/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Wma
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Wma_Init(AvCodecContext avCtx, c_int flags2)//XX 79
		{
			WmaCodecContext s = (WmaCodecContext)avCtx.Priv_Data;
			c_int channels = avCtx.Ch_Layout.Nb_Channels;

			if ((avCtx.Sample_Rate > 50000) || (channels > 2) || (avCtx.Bit_Rate <= 0))
				return -1;

			if (avCtx.Codec.Id == AvCodecId.WmaV1)
				s.Version = 1;
			else
				s.Version = 2;

			// Compute MDCT block size
			s.Frame_Len_Bits = Wma_Common.FF_Wma_Get_Frame_Len_Bits(avCtx.Sample_Rate, s.Version, 0);

			s.Next_Block_Len_Bits = s.Frame_Len_Bits;
			s.Prev_Block_Len_Bits = s.Frame_Len_Bits;
			s.Block_Len_Bits = s.Frame_Len_Bits;

			s.Frame_Len = 1 << s.Frame_Len_Bits;

			if (s.Use_Variable_Block_Len != 0)
			{
				c_int nb = ((flags2 >> 3) & 3) + 1;

				if ((avCtx.Bit_Rate / channels) >= 32000)
					nb += 2;

				c_int nb_Max = s.Frame_Len_Bits - WmaConstants.Block_Min_Bits;

				if (nb > nb_Max)
					nb = nb_Max;

				s.Nb_Block_Sizes = nb + 1;
			}
			else
				s.Nb_Block_Sizes = 1;

			// Init rate dependent parameters
			s.Use_Noise_Coding = 1;
			c_float high_Freq = avCtx.Sample_Rate * 0.5f;

			// If version 2, then the rates are normalized
			c_int sample_Rate1 = avCtx.Sample_Rate;

			if (s.Version == 2)
			{
				if (sample_Rate1 >= 44100)
					sample_Rate1 = 44100;
				else if (sample_Rate1 >= 22050)
					sample_Rate1 = 22050;
				else if (sample_Rate1 >= 16000)
					sample_Rate1 = 16000;
				else if (sample_Rate1 >= 11025)
					sample_Rate1 = 11025;
				else if (sample_Rate1 >= 8000)
					sample_Rate1 = 8000;
			}

			c_float bps = (c_float)avCtx.Bit_Rate / (channels * avCtx.Sample_Rate);
			s.Byte_Offset_Bits = IntMath.Av_Log2((c_uint)((bps * s.Frame_Len / 8.0) + 0.5)) + 2;

			if ((s.Byte_Offset_Bits + 3) > CodecConstants.Min_Cache_Bits)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "byte_offset_bits %d is too large\n", s.Byte_Offset_Bits);

				return Error.PatchWelcome;
			}

			// Compute high frequency value and choose if noise coding should
			// be activated
			c_float bps1 = bps;

			if (channels == 2)
				bps1 = bps * 1.6f;

			if (sample_Rate1 == 44100)
			{
				if (bps1 >= 0.61f)
					s.Use_Noise_Coding = 0;
				else
					high_Freq = high_Freq * 0.4f;
			}
			else if (sample_Rate1 == 22050)
			{
				if (bps1 >= 1.16f)
					s.Use_Noise_Coding = 0;
				else if (bps1 >= 0.72f)
					high_Freq = high_Freq * 0.7f;
				else
					high_Freq = high_Freq * 0.6f;
			}
			else if (sample_Rate1 == 16000)
			{
				if (bps >= 0.5f)
					high_Freq = high_Freq * 0.5f;
				else
					high_Freq = high_Freq * 0.3f;
			}
			else if (sample_Rate1 == 11025)
				high_Freq = high_Freq * 0.7f;
			else if (sample_Rate1 == 8000)
			{
				if (bps <= 0.625f)
					high_Freq = high_Freq * 0.5f;
				else if (bps > 0.75f)
					s.Use_Noise_Coding = 0;
				else
					high_Freq = high_Freq * 0.65f;
			}
			else
			{
				if (bps >= 0.8f)
					high_Freq = high_Freq * 0.75f;
				else if (bps >= 0.6f)
					high_Freq = high_Freq * 0.6f;
				else
					high_Freq = high_Freq * 0.5f;
			}

			Log.FF_DLog(s.AvCtx, "flags2=0x%s\n", flags2);
			Log.FF_DLog(s.AvCtx, "version=%d channels=%d sample_rate=%d bitrate=%lld block_align=%d\n", s.Version, channels, avCtx.Sample_Rate, avCtx.Bit_Rate, avCtx.Block_Align);
			Log.FF_DLog(s.AvCtx, "bps=%f bps1=%f high_freq=%f bitoffset=%d\n", bps, bps1, high_Freq, s.Byte_Offset_Bits);
			Log.FF_DLog(s.AvCtx, "use_noise_coding=%d use_exp_vlc=%d nb_block_sizes=%d\n", s.Use_Noise_Coding, s.Use_Exp_Vlc, s.Nb_Block_Sizes);

			// Compute the scale factor band sizes for each MDCT block size
			{
				c_int a, b, pos, lPos, i, j, n;

				if (s.Version == 1)
					s.Coefs_Start = 3;
				else
					s.Coefs_Start = 0;

				for (c_int k = 0; k < s.Nb_Block_Sizes; k++)
				{
					c_int block_Len = s.Frame_Len >> k;

					if (s.Version == 1)
					{
						lPos = 0;

						for (i = 0; i < 25; i++)
						{
							a = Wma_Freqs.ff_Wma_Critical_Freqs[i];
							b = avCtx.Sample_Rate;
							pos = ((block_Len * 2 * a) + (b >> 1)) / b;

							if (pos > block_Len)
								pos = block_Len;

							s.Exponent_Bands[0][i] = (uint16_t)(pos - lPos);

							if (pos >= block_Len)
							{
								i++;

								break;
							}

							lPos = pos;
						}

						s.Exponent_Sizes[0] = i;
					}
					else
					{
						// Hardcoded tables
						CPointer<uint8_t> table = null;

						a = s.Frame_Len_Bits - WmaConstants.Block_Min_Bits - k;

						if (a < 3)
						{
							if (avCtx.Sample_Rate >= 44100)
								table = WmaData.exponent_Band_44100[a];
							else if (avCtx.Sample_Rate >= 32000)
								table = WmaData.exponent_Band_32000[a];
							else if (avCtx.Sample_Rate >= 22050)
								table = WmaData.exponent_Band_22050[a];
						}

						if (table.IsNotNull)
						{
							n = table[0, 1];

							for (i = 0; i < n; i++)
								s.Exponent_Bands[k][i] = table[i];

							s.Exponent_Sizes[k] = n;
						}
						else
						{
							j = 0;
							lPos = 0;

							for (i = 0; i < 25; i++)
							{
								a = Wma_Freqs.ff_Wma_Critical_Freqs[i];
								b = avCtx.Sample_Rate;
								pos = ((block_Len * 2 * a) + (b << 1)) / (4 * b);
								pos <<= 2;

								if (pos > block_Len)
									pos = block_Len;

								if (pos > lPos)
									s.Exponent_Bands[k][j++] = (uint16_t)(pos - lPos);

								if (pos >= block_Len)
									break;

								lPos = pos;
							}

							s.Exponent_Sizes[k] = j;
						}
					}

					// Max number of coefs
					s.Coefs_End[k] = (s.Frame_Len - ((s.Frame_Len * 9) / 100)) >> k;

					// High freq computation
					s.High_Band_Start[k] = (c_int)((block_Len * 2 * high_Freq) / avCtx.Sample_Rate + 0.5);

					n = s.Exponent_Sizes[k];
					j = 0;
					pos = 0;

					for (i = 0; i < n; i++)
					{
						c_int start = pos;
						pos += s.Exponent_Bands[k][i];
						c_int end = pos;

						if (start < s.High_Band_Start[k])
							start = s.High_Band_Start[k];

						if (end > s.Coefs_End[k])
							end = s.Coefs_End[k];

						if (end > start)
							s.Exponent_High_Bands[k][j++] = end - start;
					}

					s.Exponent_High_Sizes[k] = j;
				}
			}

			// Init MDCT windows : simple sine window
			for (c_int i = 0; i < s.Nb_Block_Sizes; i++)
			{
				SineWin_TableGen.FF_Init_FF_Sine_Windows(s.Frame_Len_Bits - i);

				s.Windows[i] = SineWin_TableGen.ff_Sine_Windows[s.Frame_Len_Bits - i];
			}

			s.Reset_Block_Lengths = 1;

			if (s.Use_Noise_Coding != 0)
			{
				// Init the noise generator
				if (s.Use_Exp_Vlc != 0)
					s.Noise_Mult = 0.02f;
				else
					s.Noise_Mult = 0.04f;

				c_uint seed = 1;
				c_float norm = (c_float)((1.0 / (1L << 31)) * CMath.sqrt(3) * s.Noise_Mult);

				for (c_int i = 0; i < WmaConstants.Noise_Tab_Size; i++)
				{
					seed = seed * 314159 + 1;
					s.Noise_Table[i] = ((c_int)seed) * norm;
				}
			}

			s.fDsp = Float_Dsp.AvPriv_Float_Dsp_Alloc((c_int)(avCtx.Flags & AvCodecFlag.BitExact));

			if (s.fDsp == null)
				return Error.ENOMEM;

			// Choose the VLC tables for the coefficients
			c_int coef_Vlc_Table = 2;

			if (avCtx.Sample_Rate >= 32000)
			{
				if (bps1 < 0.72)
					coef_Vlc_Table = 0;
				else if (bps1 < 1.16)
					coef_Vlc_Table = 1;
			}

			s.Coef_Vlcs[0] = WmaData.coef_Vlcs[coef_Vlc_Table * 2];
			s.Coef_Vlcs[1] = WmaData.coef_Vlcs[(coef_Vlc_Table * 2) + 1];

			c_int ret = Init_Coef_Vlc(s.Coef_Vlc[0], out s.Run_Table[0], out s.Level_Table[0], out s.Int_Table[0], s.Coef_Vlcs[0]);

			if (ret < 0)
				return ret;

			return Init_Coef_Vlc(s.Coef_Vlc[1], out s.Run_Table[1], out s.Level_Table[1], out s.Int_Table[1], s.Coef_Vlcs[1]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Wma_Total_Gain_To_Bits(c_int total_Gain)//XX 353
		{
			if (total_Gain < 15)
				return 13;
			else if (total_Gain < 32)
				return 12;
			else if (total_Gain < 40)
				return 11;
			else if (total_Gain < 45)
				return 10;
			else
				return 9;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Wma_End(AvCodecContext avCtx)//XX 367
		{
			WmaCodecContext s = (WmaCodecContext)avCtx.Priv_Data;

			for (c_int i = 0; i < s.Nb_Block_Sizes; i++)
				Tx.Av_Tx_Uninit(ref s.Mdct_Ctx[i]);

			if (s.Use_Exp_Vlc != 0)
				Vlc_.FF_Vlc_Free(s.Exp_Vlc);

			if (s.Use_Noise_Coding != 0)
				Vlc_.FF_Vlc_Free(s.Hgain_Vlc);

			for (c_int i = 0; i < 2; i++)
			{
				Vlc_.FF_Vlc_Free(s.Coef_Vlc[i]);

				Mem.Av_FreeP(ref s.Run_Table[i]);
				Mem.Av_FreeP(ref s.Level_Table[i]);
				Mem.Av_FreeP(ref s.Int_Table[i]);
			}

			Mem.Av_FreeP(ref s.fDsp);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Decode an uncompressed coefficient
		/// </summary>
		/********************************************************************/
		public static c_int FF_Wma_Get_Large_Val(GetBitContext gb)//XX 395
		{
			// Consumes up to 34 bits
			c_int n_Bits = 8;

			// Decode length
			if (Get_Bits.Get_Bits1(gb) != 0)
			{
				n_Bits += 8;

				if (Get_Bits.Get_Bits1(gb) != 0)
				{
					n_Bits += 8;

					if (Get_Bits.Get_Bits1(gb) != 0)
						n_Bits += 7;
				}
			}

			return (c_int)Get_Bits.Get_Bits_Long(gb, n_Bits);
		}



		/********************************************************************/
		/// <summary>
		/// Decode run level compressed coefficients
		/// </summary>
		/********************************************************************/
		public static c_int FF_Wma_Run_Level_Decode(AvCodecContext avCtx, GetBitContext gb, CPointer<VlcElem> vlc, CPointer<c_float> level_Table, CPointer<uint16_t> run_Table, c_int version, CPointer<WmaCoef> ptr, c_int offset, c_int num_Coefs, c_int block_Len, c_int frame_Len_Bits, c_int coef_Nb_Bits)//XX 427
		{
			c_uint coef_Mask = (c_uint)block_Len - 1;

			for (; offset < num_Coefs; offset++)
			{
				c_int sign, level;

				c_int code = Get_Bits.Get_Vlc2(gb, vlc, WmaConstants.VlcBits, WmaConstants.VlcMax);

				if (code > 1)
				{
					// Normal code
					offset += run_Table[code];
					sign = (c_int)Get_Bits.Get_Bits1(gb) - 1;
					ptr[offset & coef_Mask] = sign != 0 ? -level_Table[code] : level_Table[code];
				}
				else if (code == 1)
				{
					// EOB
					break;
				}
				else
				{
					// Escape
					if (version == 0)
					{
						level = (c_int)Get_Bits._Get_Bits(gb, coef_Nb_Bits);

						// NOTE: this is rather suboptimal. Reading
						// block_len_bits would be better
						offset += (c_int)Get_Bits._Get_Bits(gb, frame_Len_Bits);
					}
					else
					{
						level = FF_Wma_Get_Large_Val(gb);

						// Escape decode
						if (Get_Bits.Get_Bits1(gb) != 0)
						{
							if (Get_Bits.Get_Bits1(gb) != 0)
							{
								if (Get_Bits.Get_Bits1(gb) != 0)
								{
									Log.Av_Log(avCtx, Log.Av_Log_Error, "broken escape sequence\n");

									return Error.InvalidData;
								}
								else
									offset += (c_int)Get_Bits._Get_Bits(gb, frame_Len_Bits) + 4;
							}
							else
								offset += (c_int)Get_Bits._Get_Bits(gb, 2) + 1;
						}
					}

					sign = (c_int)Get_Bits.Get_Bits1(gb) - 1;
					ptr[offset & coef_Mask] = (level ^ sign) - sign;
				}
			}

			// NOTE: EOB can be omitted
			if (offset > num_Coefs)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "overflow (%d > %d) in spectral RLE, ignoring\n", offset, num_Coefs);

				return Error.InvalidData;
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Init_Coef_Vlc(Vlc vlc, out CPointer<uint16_t> pRun_Table, out CPointer<c_float> pLevel_Table, out CPointer<uint16_t> pInt_Table, CoefVlcTable vlc_Table)//XX 34
		{
			pRun_Table = null;
			pLevel_Table = null;
			pInt_Table = null;

			c_int n = vlc_Table.N;
			CPointer<uint8_t> table_Bits = vlc_Table.HuffBits;
			CPointer<uint32_t> table_Codes = vlc_Table.HuffCodes;
			CPointer<uint16_t> levels_Table = vlc_Table.Levels;

			c_int ret = Vlc_.Vlc_Init(vlc, WmaConstants.VlcBits, n, table_Bits, table_Codes, VlcInit.None);

			if (ret < 0)
				return ret;

			CPointer<uint16_t> run_Table = Mem.Av_MAlloc_Array<uint16_t>((size_t)n);
			CPointer<c_float> fLevel_Table = Mem.Av_MAlloc_Array<c_float>((size_t)n);
			CPointer<uint16_t> int_Table = Mem.Av_MAlloc_Array<uint16_t>((size_t)n);

			if (run_Table.IsNull || fLevel_Table.IsNull || int_Table.IsNull)
			{
				Mem.Av_FreeP(ref run_Table);
				Mem.Av_FreeP(ref fLevel_Table);
				Mem.Av_FreeP(ref int_Table);

				return Error.ENOMEM;
			}

			c_int i = 2;
			c_int level = 2;
			c_int k = 0;

			while (i < n)
			{
				int_Table[k] = (uint16_t)i;
				c_int l = levels_Table[k++];

				for (c_int j = 0; j < l; j++)
				{
					run_Table[i] = (uint16_t)j;
					fLevel_Table[i] = level;
					i++;
				}

				level++;
			}

			pRun_Table = run_Table;
			pLevel_Table = fLevel_Table;
			pInt_Table = int_Table;

			return 0;
		}
		#endregion
	}
}
