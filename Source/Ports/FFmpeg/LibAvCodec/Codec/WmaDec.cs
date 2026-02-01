/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec
{
	/// <summary>
	/// WMA compatible decoder.
	/// 
	/// This decoder handles Microsoft Windows Media Audio data, versions 1 and 2.
	/// WMA v1 is identified by audio format 0x160 in Microsoft media files
	/// (ASF/AVI/WAV). WMA v2 is identified by audio format 0x161.
	///
	/// To use this decoder, a calling application must supply the extra data
	/// bytes provided with the WMA data. These are the extra, codec-specific
	/// bytes at the end of a WAVEFORMATEX data structure. Transmit these bytes
	/// to the decoder using the extradata[_size] fields in AVCodecContext. There
	/// should be 4 extra bytes for v1 data and 6 extra bytes for v2 data
	/// </summary>
	internal class WmaDec
	{
		public static readonly FFCodec FF_Wma2_Decoder;

		// pow(10, i / 16.0) for i in -60..95
		private static readonly c_float[] pow_Tab =
		[
			1.7782794100389e-04f, 2.0535250264571e-04f,
			2.3713737056617e-04f, 2.7384196342644e-04f,
			3.1622776601684e-04f, 3.6517412725484e-04f,
			4.2169650342858e-04f, 4.8696752516586e-04f,
			5.6234132519035e-04f, 6.4938163157621e-04f,
			7.4989420933246e-04f, 8.6596432336006e-04f,
			1.0000000000000e-03f, 1.1547819846895e-03f,
			1.3335214321633e-03f, 1.5399265260595e-03f,
			1.7782794100389e-03f, 2.0535250264571e-03f,
			2.3713737056617e-03f, 2.7384196342644e-03f,
			3.1622776601684e-03f, 3.6517412725484e-03f,
			4.2169650342858e-03f, 4.8696752516586e-03f,
			5.6234132519035e-03f, 6.4938163157621e-03f,
			7.4989420933246e-03f, 8.6596432336006e-03f,
			1.0000000000000e-02f, 1.1547819846895e-02f,
			1.3335214321633e-02f, 1.5399265260595e-02f,
			1.7782794100389e-02f, 2.0535250264571e-02f,
			2.3713737056617e-02f, 2.7384196342644e-02f,
			3.1622776601684e-02f, 3.6517412725484e-02f,
			4.2169650342858e-02f, 4.8696752516586e-02f,
			5.6234132519035e-02f, 6.4938163157621e-02f,
			7.4989420933246e-02f, 8.6596432336007e-02f,
			1.0000000000000e-01f, 1.1547819846895e-01f,
			1.3335214321633e-01f, 1.5399265260595e-01f,
			1.7782794100389e-01f, 2.0535250264571e-01f,
			2.3713737056617e-01f, 2.7384196342644e-01f,
			3.1622776601684e-01f, 3.6517412725484e-01f,
			4.2169650342858e-01f, 4.8696752516586e-01f,
			5.6234132519035e-01f, 6.4938163157621e-01f,
			7.4989420933246e-01f, 8.6596432336007e-01f,
			1.0000000000000e+00f, 1.1547819846895e+00f,
			1.3335214321633e+00f, 1.5399265260595e+00f,
			1.7782794100389e+00f, 2.0535250264571e+00f,
			2.3713737056617e+00f, 2.7384196342644e+00f,
			3.1622776601684e+00f, 3.6517412725484e+00f,
			4.2169650342858e+00f, 4.8696752516586e+00f,
			5.6234132519035e+00f, 6.4938163157621e+00f,
			7.4989420933246e+00f, 8.6596432336007e+00f,
			1.0000000000000e+01f, 1.1547819846895e+01f,
			1.3335214321633e+01f, 1.5399265260595e+01f,
			1.7782794100389e+01f, 2.0535250264571e+01f,
			2.3713737056617e+01f, 2.7384196342644e+01f,
			3.1622776601684e+01f, 3.6517412725484e+01f,
			4.2169650342858e+01f, 4.8696752516586e+01f,
			5.6234132519035e+01f, 6.4938163157621e+01f,
			7.4989420933246e+01f, 8.6596432336007e+01f,
			1.0000000000000e+02f, 1.1547819846895e+02f,
			1.3335214321633e+02f, 1.5399265260595e+02f,
			1.7782794100389e+02f, 2.0535250264571e+02f,
			2.3713737056617e+02f, 2.7384196342644e+02f,
			3.1622776601684e+02f, 3.6517412725484e+02f,
			4.2169650342858e+02f, 4.8696752516586e+02f,
			5.6234132519035e+02f, 6.4938163157621e+02f,
			7.4989420933246e+02f, 8.6596432336007e+02f,
			1.0000000000000e+03f, 1.1547819846895e+03f,
			1.3335214321633e+03f, 1.5399265260595e+03f,
			1.7782794100389e+03f, 2.0535250264571e+03f,
			2.3713737056617e+03f, 2.7384196342644e+03f,
			3.1622776601684e+03f, 3.6517412725484e+03f,
			4.2169650342858e+03f, 4.8696752516586e+03f,
			5.6234132519035e+03f, 6.4938163157621e+03f,
			7.4989420933246e+03f, 8.6596432336007e+03f,
			1.0000000000000e+04f, 1.1547819846895e+04f,
			1.3335214321633e+04f, 1.5399265260595e+04f,
			1.7782794100389e+04f, 2.0535250264571e+04f,
			2.3713737056617e+04f, 2.7384196342644e+04f,
			3.1622776601684e+04f, 3.6517412725484e+04f,
			4.2169650342858e+04f, 4.8696752516586e+04f,
			5.6234132519035e+04f, 6.4938163157621e+04f,
			7.4989420933246e+04f, 8.6596432336007e+04f,
			1.0000000000000e+05f, 1.1547819846895e+05f,
			1.3335214321633e+05f, 1.5399265260595e+05f,
			1.7782794100389e+05f, 2.0535250264571e+05f,
			2.3713737056617e+05f, 2.7384196342644e+05f,
			3.1622776601684e+05f, 3.6517412725484e+05f,
			4.2169650342858e+05f, 4.8696752516586e+05f,
			5.6234132519035e+05f, 6.4938163157621e+05f,
			7.4989420933246e+05f, 8.6596432336007e+05f
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		static WmaDec()
		{
			FF_Wma2_Decoder = new FFCodec
			{
				Name = "wma2".ToCharPointer(),
				Long_Name = "Windows Media Audio 2".ToCharPointer(),
				Type = AvMediaType.Audio,
				Id = AvCodecId.WmaV2,
				Priv_Data_Alloc = Alloc_Priv_Data,
				Init = Wma_Decode_Init,
				Close = Wma.FF_Wma_End,
				Is_Decoder = true,
				Cb_Type = FFCodecType.Decode,
				Flush = Flush,
				Capabilities = AvCodecCap.Dr1 | AvCodecCap.Delay,
				Sample_Fmts = new CPointer<AvSampleFormat>([ AvSampleFormat.FltP, AvSampleFormat.None ]),//XX skal none være der?
				Caps_Internal = FFCodecCap.Init_Cleanup
			};

			FF_Wma2_Decoder.Cb.Decode = Wma_Decode_SuperFrame;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IPrivateData Alloc_Priv_Data()
		{
			return new WmaCodecContext();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Wma_Decode_Init(AvCodecContext avCtx)//XX 74
		{
			WmaCodecContext s = (WmaCodecContext)avCtx.Priv_Data;

			if (avCtx.Block_Align == 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "block_align is not set\n");

				return Error.EINVAL;
			}

			s.AvCtx = avCtx;

			// Extract flag info
			c_int flags2 = 0;
			DataBufferContext extraData = (DataBufferContext)avCtx.ExtraData;

			if ((avCtx.Codec_Id == AvCodecId.WmaV1) && (extraData.Size >= 4))
				flags2 = (c_int)IntReadWrite.Av_RL16(extraData.Data + 2);
			else if ((avCtx.Codec_Id == AvCodecId.WmaV2) && (extraData.Size >= 6))
				flags2 = (c_int)IntReadWrite.Av_RL16(extraData.Data + 4);

			s.Use_Exp_Vlc = flags2 & 0x0001;
			s.Use_Bit_Reservoir = flags2 & 0x0002;
			s.Use_Variable_Block_Len = flags2 & 0x0004;

			if ((avCtx.Codec_Id == AvCodecId.WmaV2) && (extraData.Size >= 8))
			{
				if ((IntReadWrite.Av_RL16(extraData.Data + 4) == 0xd) && (s.Use_Variable_Block_Len != 0))
				{
					Log.Av_Log(avCtx, Log.Av_Log_Warning, "Disabling use_variable_block_len, if this fails contact the ffmpeg developers and send us the file\n");

					s.Use_Variable_Block_Len = 0;
				}
			}

			for (c_int i = 0; i < WmaConstants.Max_Channels; i++)
				s.Max_Exponent[i] = 1.0f;

			c_int ret = Wma.FF_Wma_Init(avCtx, flags2);

			if (ret < 0)
				return ret;

			// Init MDCT
			for (c_int i = 0; i < s.Nb_Block_Sizes; i++)
			{
				c_float scale = 1.0f / 32768.0f;

				ret = Tx.Av_Tx_Init(out s.Mdct_Ctx[i], out s.Mdct_Fn[i], AvTxType.Float_Mdct, 1, 1 << (s.Frame_Len_Bits - i), ref scale, AvTxFlags.Full_Imdct);

				if (ret < 0)
					return ret;
			}

			if (s.Use_Noise_Coding != 0)
			{
				ret = Vlc_.FF_Vlc_Init_From_Lengths<uint8_t, uint8_t>(s.Hgain_Vlc, WmaConstants.HGainVlcBits, (c_int)Macros.FF_Array_Elems(WmaData.ff_Wma_HGain_HuffTab1), WmaData.ff_Wma_HGain_HuffTab2, WmaData.ff_Wma_HGain_HuffTab1, -18, VlcInit.None, avCtx);

				if (ret < 0)
					return ret;
			}

			if (s.Use_Exp_Vlc != 0)
			{
				// FIXME move out of context
				ret = Vlc_.Vlc_Init<uint8_t, uint32_t>(s.Exp_Vlc, WmaConstants.ExpVlcBits, AacTab.ff_Aac_ScaleFactor_Bits.Length, AacTab.ff_Aac_ScaleFactor_Bits, AacTab.ff_Aac_ScaleFactor_Code, VlcInit.None);

				if (ret < 0)
					return ret;
			}
			else
				Wma_Lsp_To_Curve_Init(s, s.Frame_Len);

			avCtx.Sample_Fmt = AvSampleFormat.FltP;

			avCtx.Internal.Skip_Samples = s.Frame_Len * 2;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compute x^-0.25 with an exponent and mantissa table. We use
		/// linear interpolation to reduce the mantissa table size at a small
		/// speed expense (linear interpolation approximately doubles the
		/// number of bits of precision)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_float Pow_M1_4(WmaCodecContext s, c_float x)//XX 154
		{
			c_uint u = BitConverter.SingleToUInt32Bits(x);
			c_uint e = u >> 23;
			c_uint m = (u >> (23 - WmaConstants.Lsp_Pow_Bits)) & ((1 << WmaConstants.Lsp_Pow_Bits) - 1);

			// Build interpolation scale: 1 <= t < 2
			c_uint t = ((u << WmaConstants.Lsp_Pow_Bits) & ((1 << 23) - 1)) | (127 << 23);
			c_float a = s.Lsp_Pow_M_Table1[m];
			c_float b = s.Lsp_Pow_M_Table2[m];

			return s.Lsp_Pow_E_Table[e] * (a + b * BitConverter.UInt32BitsToSingle(t));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Wma_Lsp_To_Curve_Init(WmaCodecContext s, c_int frame_Len)//XX 173
		{
			c_float wDel = (c_float)Math.PI / frame_Len;

			for (c_int i = 0; i < frame_Len; i++)
				s.Lsp_Cos_Table[i] = (c_float)(2.0f * CMath.cos(wDel * i));

			// Tables for x^-0.25 computation
			for (c_int i = 0; i < 256; i++)
			{
				c_int e = i - 126;
				s.Lsp_Pow_E_Table[i] = (c_float)CMath.exp2(e * -0.25);
			}

			// NOTE: these two tables are needed to avoid two operations in
			// pow_m1_4
			c_float b = 1.0f;

			for (c_int i = (1 << WmaConstants.Lsp_Pow_Bits) - 1; i >= 0; i--)
			{
				c_int m = (1 << WmaConstants.Lsp_Pow_Bits) + i;
				c_float a = m * (0.5f / (1 << WmaConstants.Lsp_Pow_Bits));
				a = (c_float)(1 / CMath.sqrt(CMath.sqrt(a)));

				s.Lsp_Pow_M_Table1[i] = (2 * a) - b;
				s.Lsp_Pow_M_Table2[i] = b - a;

				b = a;
			}
		}



		/********************************************************************/
		/// <summary>
		/// NOTE: We use the same code as Vorbis here
		/// </summary>
		/********************************************************************/
		private static void Wma_Lsp_To_Curve(WmaCodecContext s, CPointer<c_float> @out, out c_float val_Max_Ptr, c_int n, CPointer<c_float> lsp)//XX 205
		{
			c_float val_Max = 0;

			for (c_int i = 0; i < n; i++)
			{
				c_float p = 0.5f;
				c_float q = 0.5f;
				c_float w = s.Lsp_Cos_Table[i];

				for (c_int j = 1; j < WmaConstants.Nb_Lsp_Coefs; j += 2)
				{
					q *= w - lsp[j - 1];
					p *= w - lsp[j];
				}

				p *= p * (2.0f - w);
				q *= q * (2.0f + w);

				c_float v = p + q;
				v = Pow_M1_4(s, v);

				if (v > val_Max)
					val_Max = v;

				@out[i] = v;
			}

			val_Max_Ptr = val_Max;
		}



		/********************************************************************/
		/// <summary>
		/// Decode exponents coded with LSP coefficients (same idea as
		/// Vorbis)
		/// </summary>
		/********************************************************************/
		private static void Decode_Exp_Lsp(WmaCodecContext s, c_int ch)//XX 234
		{
			c_int val;

			c_float[] lsp_Coefs = new c_float[WmaConstants.Nb_Lsp_Coefs];

			for (c_int i = 0; i < WmaConstants.Nb_Lsp_Coefs; i++)
			{
				if ((i == 0) || (i >= 8))
					val = (c_int)Get_Bits._Get_Bits(s.Gb, 3);
				else
					val = (c_int)Get_Bits._Get_Bits(s.Gb, 4);

				lsp_Coefs[i] = WmaData.ff_Wma_Lsp_CodeBook[i][val];
			}

			Wma_Lsp_To_Curve(s, s.Exponents[ch], out s.Max_Exponent[ch], s.Block_Len, lsp_Coefs);
		}



		/********************************************************************/
		/// <summary>
		/// Decode exponents coded with VLC codes
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Exp_Vlc(WmaCodecContext s, c_int ch)//XX 336
		{
			c_int last_Exp, n;
			c_float v;

			CPointer<c_float> pTab = pow_Tab.ToPointer() + 60;

			CPointer<uint16_t> ptr = s.Exponent_Bands[s.Frame_Len_Bits - s.Block_Len_Bits];
			CPointer<c_float> q = s.Exponents[ch];
			CPointer<c_float> q_End = q + s.Block_Len;
			c_float max_Scale = 0;

			if (s.Version == 1)
			{
				last_Exp = (c_int)Get_Bits._Get_Bits(s.Gb, 5) + 10;

				v = pTab[last_Exp];

				max_Scale = v;
				n = ptr[0, 1];

				for (; n > 0; n--)
					q[0, 1] = v;
			}
			else
				last_Exp = 36;

			while (q < q_End)
			{
				c_int code = Get_Bits.Get_Vlc2(s.Gb, s.Exp_Vlc.Table, WmaConstants.ExpVlcBits, WmaConstants.ExpMax);

				// NOTE: this offset is the same as MPEG-4 AAC!
				last_Exp += code - 60;

				if (((c_uint)last_Exp + 60) >= Macros.FF_Array_Elems(pow_Tab))
				{
					Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "Exponent out of range: %d\n", last_Exp);

					return Error.InvalidData;
				}

				v = pTab[last_Exp];

				if (v > max_Scale)
					max_Scale = v;

				n = ptr[0, 1];

				for (; n > 0; n--)
					q[0, 1] = v;
			}

			s.Max_Exponent[ch] = max_Scale;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Apply MDCT window and add into output.
		///
		/// We ensure that when the windows overlap their squared sum is
		/// always 1 (MDCT reconstruction rule)
		/// </summary>
		/********************************************************************/
		private static void Wma_Window(WmaCodecContext s, CPointer<c_float> @out)//XX 395
		{
			CPointer<c_float> @in = s.Output;
			c_int block_Len, bSize, n;

			// Left part
			if (s.Block_Len_Bits <= s.Prev_Block_Len_Bits)
			{
				block_Len = s.Block_Len;
				bSize = s.Frame_Len_Bits - s.Block_Len_Bits;

				s.fDsp.Vector_FMul_Add(@out, @in, s.Windows[bSize], @out, block_Len);
			}
			else
			{
				block_Len = 1 << s.Prev_Block_Len_Bits;
				n = (s.Block_Len - block_Len) / 2;
				bSize = s.Frame_Len_Bits - s.Prev_Block_Len_Bits;

				s.fDsp.Vector_FMul_Add(@out + n, @in + n, s.Windows[bSize], @out + n, block_Len);

				CMemory.memcpy(@out + n + block_Len, @in + n + block_Len, (size_t)n);
			}

			@out += s.Block_Len;
			@in += s.Block_Len;

			// Right part
			if (s.Block_Len_Bits <= s.Next_Block_Len_Bits)
			{
				block_Len = s.Block_Len;
				bSize = s.Frame_Len_Bits - s.Block_Len_Bits;

				s.fDsp.Vector_FMul_Reverse(@out, @in, s.Windows[bSize], block_Len);
			}
			else
			{
				block_Len = 1 << s.Next_Block_Len_Bits;
				n = (s.Block_Len - block_Len) / 2;
				bSize = s.Frame_Len_Bits - s.Next_Block_Len_Bits;

				CMemory.memcpy(@out, @in, (size_t)n);

				s.fDsp.Vector_FMul_Reverse(@out + n, @in + n, s.Windows[bSize], block_Len);

				CMemory.memset(@out + n + block_Len, 0, (size_t)n);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Wma_Decode_Block(WmaCodecContext s)//XX 447
		{
			c_int n, v;

			c_int channels = s.AvCtx.Ch_Layout.Nb_Channels;
			c_int[] nb_Coefs = new c_int[WmaConstants.Max_Channels];

			// Compute current block length
			if (s.Use_Variable_Block_Len != 0)
			{
				n = IntMath.Av_Log2((c_uint)s.Nb_Block_Sizes - 1) + 1;

				if (s.Reset_Block_Lengths != 0)
				{
					s.Reset_Block_Lengths = 0;

					v = (c_int)Get_Bits._Get_Bits(s.Gb, n);

					if (v >= s.Nb_Block_Sizes)
					{
						Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "prev_block_len_bits %d out of range\n", s.Frame_Len_Bits - v);

						return Error.InvalidData;
					}

					s.Prev_Block_Len_Bits = s.Frame_Len_Bits - v;

					v = (c_int)Get_Bits._Get_Bits(s.Gb, n);

					if (v >= s.Nb_Block_Sizes)
					{
						Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "block_len_bits %d out of range\n", s.Frame_Len_Bits - v);

						return Error.InvalidData;
					}

					s.Block_Len_Bits = s.Frame_Len_Bits - v;
				}
				else
				{
					// Update block lengths
					s.Prev_Block_Len_Bits = s.Block_Len_Bits;
					s.Block_Len_Bits = s.Next_Block_Len_Bits;
				}

				v = (c_int)Get_Bits._Get_Bits(s.Gb, n);

				if (v >= s.Nb_Block_Sizes)
				{
					Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "next_block_len_bits %d out of range\n", s.Frame_Len_Bits - v);

					return Error.InvalidData;
				}

				s.Next_Block_Len_Bits = s.Frame_Len_Bits - v;
			}
			else
			{
				// Fixed block len
				s.Next_Block_Len_Bits = s.Frame_Len_Bits;
				s.Prev_Block_Len_Bits = s.Frame_Len_Bits;
				s.Block_Len_Bits = s.Frame_Len_Bits;
			}

			if ((s.Frame_Len_Bits - s.Block_Len_Bits) >= s.Nb_Block_Sizes)
			{
				Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "block_len_bits not initialized to a valid value\n");

				return Error.InvalidData;
			}

			// Now check if the block length is coherent with the frame length
			s.Block_Len = 1 << s.Block_Len_Bits;

			if ((s.Block_Pos + s.Block_Len) > s.Frame_Len)
			{
				Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "frame_len overflow\n");

				return Error.InvalidData;
			}

			if (channels == 2)
				s.Ms_Stereo = (uint8_t)Get_Bits.Get_Bits1(s.Gb);

			v = 0;

			for (c_int ch = 0; ch < channels; ch++)
			{
				c_int a = (c_int)Get_Bits.Get_Bits1(s.Gb);
				s.Channel_Coded[ch] = (uint8_t)a;
				v |= a;
			}

			c_int bSize = s.Frame_Len_Bits - s.Block_Len_Bits;

			// If no channel coded, no need to go further
			// XXX: fix potential framing problems
			if (v == 0)
				goto Next;

			// Read total gain and extract corresponding number of bits for
			// coef escape coding
			c_int total_Gain = 1;

			for (;;)
			{
				if (Get_Bits.Get_Bits_Left(s.Gb) < 7)
				{
					Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "total_gain overread\n");

					return Error.InvalidData;
				}

				c_int a = (c_int)Get_Bits._Get_Bits(s.Gb, 7);
				total_Gain += a;

				if (a != 127)
					break;
			}

			c_int coef_Nb_Bits = Wma.FF_Wma_Total_Gain_To_Bits(total_Gain);

			// Compute number of coefficients
			n = s.Coefs_End[bSize] - s.Coefs_Start;

			for (c_int ch = 0; ch < channels; ch++)
				nb_Coefs[ch] = n;

			// Complex coding
			if (s.Use_Noise_Coding != 0)
			{
				for (c_int ch = 0; ch < channels; ch++)
				{
					if (s.Channel_Coded[ch] != 0)
					{
						c_int n_ = s.Exponent_High_Sizes[bSize];

						for (c_int i = 0; i < n_; i++)
						{
							c_int a = (c_int)Get_Bits.Get_Bits1(s.Gb);
							s.High_Band_Coded[ch][i] = a;

							// If noise coding, the coefficients are not transmitted
							if (a != 0)
								nb_Coefs[ch] -= s.Exponent_High_Bands[bSize][i];
						}
					}
				}

				for (c_int ch = 0; ch < channels; ch++)
				{
					if (s.Channel_Coded[ch] != 0)
					{
						c_int n_ = s.Exponent_High_Sizes[bSize];
						c_int val = unchecked((c_int)0x80000000);

						for (c_int i = 0; i < n_; i++)
						{
							if (s.High_Band_Coded[ch][i] != 0)
							{
								if (val == unchecked((c_int)0x80000000))
									val = (c_int)Get_Bits._Get_Bits(s.Gb, 7) - 19;
								else
									val += Get_Bits.Get_Vlc2(s.Gb, s.Hgain_Vlc.Table, WmaConstants.HGainVlcBits, WmaConstants.HGainMax);

								s.High_Band_Values[ch][i] = val;
							}
						}
					}
				}
			}

			// Exponents can be reused in short blocks
			if ((s.Block_Len_Bits == s.Frame_Len_Bits) || (Get_Bits.Get_Bits1(s.Gb) != 0))
			{
				for (c_int ch = 0; ch < channels; ch++)
				{
					if (s.Channel_Coded[ch] != 0)
					{
						if (s.Use_Exp_Vlc != 0)
						{
							if (Decode_Exp_Vlc(s, ch) < 0)
								return Error.InvalidData;
						}
						else
							Decode_Exp_Lsp(s, ch);

						s.Exponents_BSize[ch] = bSize;
						s.Exponents_Initialized[ch] = 1;
					}
				}
			}

			for (c_int ch = 0; ch < channels; ch++)
			{
				if ((s.Channel_Coded[ch] != 0) && (s.Exponents_Initialized[ch] == 0))
					return Error.InvalidData;
			}

			// Parse spectral coefficients : just RLE encoding
			for (c_int ch = 0; ch < channels; ch++)
			{
				if (s.Channel_Coded[ch] != 0)
				{
					CPointer<WmaCoef> ptr = s.Coefs1[ch];

					// Special VLC tables are used for ms stereo because
					// there is potentially less energy there
					c_int tIndex = (ch == 1) && (s.Ms_Stereo != 0) ? 1 : 0;

					CMemory.memset(ptr, 0, (size_t)s.Block_Len);

					c_int ret = Wma.FF_Wma_Run_Level_Decode(s.AvCtx, s.Gb, s.Coef_Vlc[tIndex].Table, s.Level_Table[tIndex], s.Run_Table[tIndex], 0, ptr, 0, nb_Coefs[ch], s.Block_Len, s.Frame_Len_Bits, coef_Nb_Bits);

					if (ret < 0)
						return ret;
				}

				if ((s.Version == 1) && (channels >= 2))
					Get_Bits.Align_Get_Bits(s.Gb);
			}

			// Normalize
			c_float mdct_Norm;

			{
				c_int n4 = s.Block_Len / 2;
				mdct_Norm = 1.0f / n4;

				if (s.Version == 1)
					mdct_Norm *= (c_float)CMath.sqrt(n4);
			}

			// Finally compute the MDCT coefficients
			for (c_int ch = 0; ch < channels; ch++)
			{
				if (s.Channel_Coded[ch] != 0)
				{
					c_float[] exp_Power = new c_float[WmaConstants.High_Band_Max_Size];

					CPointer<WmaCoef> coefs1 = s.Coefs1[ch];
					CPointer<c_float> exponents = s.Exponents[ch];
					c_int eSize = s.Exponents_BSize[ch];
					c_float mult = (c_float)(FFMath.FF_Exp10(total_Gain * 0.05) / s.Max_Exponent[ch]);
					mult *= mdct_Norm;
					CPointer<c_float> coefs = s.Coefs[ch];

					if (s.Use_Noise_Coding != 0)
					{
						c_float mult1 = mult;

						// Very low freqs : noise
						for (c_int i = 0; i < s.Coefs_Start; i++)
						{
							coefs[0, 1] = s.Noise_Table[s.Noise_Index] * exponents[i << bSize >> eSize] * mult1;
							s.Noise_Index = (s.Noise_Index + 1) & (WmaConstants.Noise_Tab_Size - 1);
						}

						c_int n1 = s.Exponent_High_Sizes[bSize];

						// Compute power of high bands
						exponents = s.Exponents[ch].ToPointer() + (s.High_Band_Start[bSize] << bSize >> eSize);
						c_int last_High_Band = 0;	// Avoid warning

						for (c_int j = 0; j < n1; j++)
						{
							n = s.Exponent_High_Bands[s.Frame_Len_Bits - s.Block_Len_Bits][j];

							if (s.High_Band_Coded[ch][j] != 0)
							{
								c_float e2 = 0;

								for (c_int i = 0; i < n; i++)
								{
									c_float v_ = exponents[i << bSize >> eSize];
									e2 += v_ * v_;
								}

								exp_Power[j] = e2 / n;
								last_High_Band = j;

								Log.FF_TLog(s.AvCtx, "%d: power=%f (%d)\n", j, exp_Power[j], n);
							}

							exponents += n << bSize >> eSize;
						}

						// Main freqs and high freqs
						exponents = s.Exponents[ch].ToPointer() + (s.Coefs_Start << bSize >> eSize);

						for (c_int j = -1; j < n1; j++)
						{
							if (j < 0)
								n = s.High_Band_Start[bSize] - s.Coefs_Start;
							else
								n = s.Exponent_High_Bands[s.Frame_Len_Bits - s.Block_Len_Bits][j];

							if ((j >= 0) && (s.High_Band_Coded[ch][j] != 0))
							{
								// Use noise with specified power
								mult1 = (c_float)CMath.sqrt(exp_Power[j] / exp_Power[last_High_Band]);
								mult1 = (c_float)(mult1 * FFMath.FF_Exp10(s.High_Band_Values[ch][j] * 0.05));
								mult1 = mult1 / (s.Max_Exponent[ch] * s.Noise_Mult);
								mult1 *= mdct_Norm;

								for (c_int i = 0; i < n; i++)
								{
									c_float noise = s.Noise_Table[s.Noise_Index];
									s.Noise_Index = (s.Noise_Index + 1) & (WmaConstants.Noise_Tab_Size - 1);
									coefs[0, 1] = noise * exponents[i << bSize >> eSize] * mult1;
								}

								exponents += n << bSize >> eSize;
							}
							else
							{
								// Coded values + small noise
								for (c_int i = 0; i < n; i++)
								{
									c_float noise = s.Noise_Table[s.Noise_Index];
									s.Noise_Index = (s.Noise_Index + 1) & (WmaConstants.Noise_Tab_Size - 1);
									coefs[0, 1] = (coefs1[0, 1] + noise) * exponents[i << bSize >> eSize] * mult;
								}

								exponents += n << bSize >> eSize;
							}
						}

						// Very high freqs : noise
						n = s.Block_Len - s.Coefs_End[bSize];
						mult1 = mult * exponents[(-(1 << bSize)) >> eSize];

						for (c_int i = 0; i < n; i++)
						{
							coefs[0, 1] = s.Noise_Table[s.Noise_Index] * mult1;
							s.Noise_Index = (s.Noise_Index + 1) & (WmaConstants.Noise_Tab_Size - 1);
						}
					}
					else
					{
						for (c_int i = 0; i < s.Coefs_Start; i++)
							coefs[0, 1] = 0.0f;

						n = nb_Coefs[ch];

						for (c_int i = 0; i < n; i++)
							coefs[0, 1] = coefs1[i] * exponents[i << bSize >> eSize] * mult;

						n = s.Block_Len - s.Coefs_End[bSize];

						for (c_int i = 0; i < n; i++)
							coefs[0, 1] = 0.0f;
					}
				}
			}

			if ((s.Ms_Stereo != 0) && (s.Channel_Coded[1] != 0))
			{
				// Nominal case for ms stereo: we do it before mdct
				// no need to optimize this case because it should almost
				// never happen
				if (s.Channel_Coded[0] == 0)
				{
					Log.FF_TLog(s.AvCtx, "rare ms-stereo case happend\n");

					CMemory.memset<c_float>(s.Coefs[0], 0, (size_t)s.Block_Len);
					s.Channel_Coded[0] = 1;
				}

				s.fDsp.Butterflies_Float(s.Coefs[0], s.Coefs[1], s.Block_Len);
			}

			Next:
			AvTxContext mdct = s.Mdct_Ctx[bSize];
			UtilFunc.Av_Tx_Fn mdct_Fn = s.Mdct_Fn[bSize];

			for (c_int ch = 0; ch < channels; ch++)
			{
				c_int n4 = s.Block_Len / 2;

				if (s.Channel_Coded[ch] != 0)
					mdct_Fn(mdct, new CPointer<c_float>(s.Output), new CPointer<c_float>(s.Coefs[ch]), sizeof(c_float));
				else if (!((s.Ms_Stereo != 0) && (ch == 1)))
					CMemory.memset<c_float>(s.Output, 0, (size_t)s.Output.Length);

				// Multiply by the window and add in the frame
				c_int index = (s.Frame_Len / 2) + s.Block_Pos - n4;
				Wma_Window(s, s.Frame_Out[ch].ToPointer() + index);
			}

			// Update block number
			s.Block_Num++;
			s.Block_Pos += s.Block_Len;

			if (s.Block_Pos >= s.Frame_Len)
				return 1;
			else
				return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Decode a frame of frame_len samples
		/// </summary>
		/********************************************************************/
		private static c_int Wma_Decode_Frame(WmaCodecContext s, CPointer<CPointer<c_float>> samples, c_int samples_Offset)//XX 791
		{
			// Read each block
			s.Block_Num = 0;
			s.Block_Pos = 0;

			for (;;)
			{
				c_int ret = Wma_Decode_Block(s);

				if (ret < 0)
					return ret;

				if (ret != 0)
					break;
			}

			for (c_int ch = 0; ch < s.AvCtx.Ch_Layout.Nb_Channels; ch++)
			{
				// Copy current block to output
				CMemory.memcpy(samples[ch] + samples_Offset, s.Frame_Out[ch], (size_t)s.Frame_Len);

				// Prepare for next block
				CMemory.memmove(s.Frame_Out[ch], s.Frame_Out[ch].ToPointer() + s.Frame_Len, (size_t)s.Frame_Len);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Wma_Decode_SuperFrame(AvCodecContext avCtx, AvFrame frame, out c_int got_Frame_Ptr, AvPacket avPkt)//XX 829
		{
			got_Frame_Ptr = 0;

			CPointer<uint8_t> buf = avPkt.Data;
			c_int buf_Size = avPkt.Size;
			WmaCodecContext s = (WmaCodecContext)avCtx.Priv_Data;
			CPointer<uint8_t> q;
			c_int nb_Frames, len, ret;

			Log.FF_TLog(avCtx, "***decode_superframe:\n");

			if (buf_Size == 0)
			{
				if (s.Eof_Done != 0)
					return 0;

				frame.Nb_Samples = s.Frame_Len;

				ret = Decode.FF_Get_Buffer(avCtx, frame, 0);

				if (ret < 0)
					return ret;

				frame.Pts = UtilConstants.Av_NoPts_Value;

				for (c_int i = 0; i < s.AvCtx.Ch_Layout.Nb_Channels; i++)
					CMemory.memcpy(frame.Extended_Data[i].Cast<uint8_t, c_float>(), s.Frame_Out[i], (size_t)frame.Nb_Samples);

				s.Last_SuperFrame_Len = 0;
				s.Eof_Done = 1;
				got_Frame_Ptr = 1;

				return 0;
			}

			if (buf_Size < avCtx.Block_Align)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Input packet size too small (%d < %d)\n", buf_Size, avCtx.Block_Align);

				return Error.InvalidData;
			}

			if (avCtx.Block_Align != 0)
				buf_Size = avCtx.Block_Align;

			Get_Bits.Init_Get_Bits(s.Gb, buf, buf_Size * 8);

			if (s.Use_Bit_Reservoir != 0)
			{
				// Read super frame header
				Get_Bits.Skip_Bits(s.Gb, 4);		// Super frame index

				nb_Frames = (c_int)Get_Bits._Get_Bits(s.Gb, 4) - ((s.Last_SuperFrame_Len <= 0) ? 1 : 0);

				if (nb_Frames <= 0)
				{
					c_int is_Error = (nb_Frames < 0) || (Get_Bits.Get_Bits_Left(s.Gb) <= 8) ? 1 : 0;

					Log.Av_Log(avCtx, is_Error != 0 ? Log.Av_Log_Error : Log.Av_Log_Warning, "nb_frames is %d bits left %d\n", nb_Frames, Get_Bits.Get_Bits_Left(s.Gb));

					if (is_Error != 0)
						return Error.InvalidData;

					if ((s.Last_SuperFrame_Len + buf_Size - 1) > WmaConstants.Max_Coded_SuperFrame_Size)
					{
						ret = Error.InvalidData;

						goto Fail;
					}

					q = s.Last_SuperFrame + s.Last_SuperFrame_Len;
					len = buf_Size - 1;

					while (len > 0)
					{
						q[0, 1] = (uint8_t)Get_Bits._Get_Bits(s.Gb, 8);
						len--;
					}

					CMemory.memset<uint8_t>(q, 0, Defs.Av_Input_Buffer_Padding_Size);

					s.Last_SuperFrame_Len += (8 * buf_Size) - 8;
					got_Frame_Ptr = 0;

					return buf_Size;
				}
			}
			else
				nb_Frames = 1;

			// Get output buffer
			frame.Nb_Samples = nb_Frames * s.Frame_Len;

			ret = Decode.FF_Get_Buffer(avCtx, frame, 0);

			if (ret < 0)
				return ret;

			CPointer<CPointer<c_float>> samples = new CPointer<CPointer<c_float>>(frame.Extended_Data.Length);

			for (c_int i = 0; i < samples.Length; i++)
				samples[i] = frame.Extended_Data[i].Cast<uint8_t, c_float>();

			c_int samples_Offset = 0;

			if (s.Use_Bit_Reservoir != 0)
			{
				c_int bit_Offset = (c_int)Get_Bits._Get_Bits(s.Gb, s.Byte_Offset_Bits + 3);

				if (bit_Offset > Get_Bits.Get_Bits_Left(s.Gb))
				{
					Log.Av_Log(avCtx, Log.Av_Log_Error, "Invalid last frame bit offset %d > buf size %d (%d)\n", bit_Offset, Get_Bits.Get_Bits_Left(s.Gb), buf_Size);

					ret = Error.InvalidData;

					goto Fail;
				}

				if (s.Last_SuperFrame_Len > 0)
				{
					// Add bit_offset bits to last frame
					if ((s.Last_SuperFrame_Len + ((bit_Offset + 7) >> 3)) > WmaConstants.Max_Coded_SuperFrame_Size)
					{
						ret = Error.InvalidData;

						goto Fail;
					}

					q = s.Last_SuperFrame + s.Last_SuperFrame_Len;
					len = bit_Offset;

					while (len > 7)
					{
						q[0, 1] = (uint8_t)Get_Bits._Get_Bits(s.Gb, 8);
						len -= 8;
					}

					if (len > 0)
						q[0, 1] = (uint8_t)(Get_Bits._Get_Bits(s.Gb, len) << (8 - len));

					CMemory.memset<uint8_t>(q, 0, Defs.Av_Input_Buffer_Padding_Size);

					// XXX: bit_offset bits into last frame
					Get_Bits.Init_Get_Bits(s.Gb, s.Last_SuperFrame, s.Last_SuperFrame_Len * 8 + bit_Offset);

					// Skip unused bits
					if (s.Last_BitOffset > 0)
						Get_Bits.Skip_Bits(s.Gb, s.Last_BitOffset);

					// This frame is stored in the last superframe and in the
					// current one
					ret = Wma_Decode_Frame(s, samples, samples_Offset);

					if (ret < 0)
						goto Fail;

					samples_Offset += s.Frame_Len;
					nb_Frames--;
				}

				// Read each frame starting from bit_offset
				c_int pos = bit_Offset + 4 + 4 + s.Byte_Offset_Bits + 3;

				if ((pos >= (WmaConstants.Max_Coded_SuperFrame_Size * 8)) || (pos > (buf_Size * 8)))
					return Error.InvalidData;

				Get_Bits.Init_Get_Bits(s.Gb, buf + (pos >> 3), (buf_Size - (pos >> 3)) * 8);

				len = pos & 7;

				if (len > 0)
					Get_Bits.Skip_Bits(s.Gb, len);

				s.Reset_Block_Lengths = 1;

				for (c_int i = 0; i < nb_Frames; i++)
				{
					ret = Wma_Decode_Frame(s, samples, samples_Offset);

					if (ret < 0)
						goto Fail;

					samples_Offset += s.Frame_Len;
				}

				// We copy the end of the frame in the last frame buffer
				pos = Get_Bits.Get_Bits_Count(s.Gb) + ((bit_Offset + 4 + 4 + s.Byte_Offset_Bits + 3) & ~7);

				s.Last_BitOffset = pos & 7;
				pos >>= 3;
				len = buf_Size - pos;

				if ((len > WmaConstants.Max_Coded_SuperFrame_Size) || (len < 0))
				{
					Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "len %d invalid\n", len);

					ret = Error.InvalidData;

					goto Fail;
				}

				s.Last_SuperFrame_Len = len;

				CMemory.memcpy(s.Last_SuperFrame, buf + pos, (size_t)len);
			}
			else
			{
				// Single frame decode
				ret = Wma_Decode_Frame(s, samples, samples_Offset);

				if (ret < 0)
					goto Fail;

				samples_Offset += s.Frame_Len;
			}

			Log.FF_DLog(s.AvCtx, "%d %d %d %d eaten:%d\n", s.Frame_Len_Bits, s.Block_Len_Bits, s.Frame_Len, s.Block_Len, avCtx.Block_Align);

			got_Frame_Ptr = 1;

			return buf_Size;

			Fail:
			// When error, we reset the bit reservoir
			s.Last_SuperFrame_Len = 0;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Flush(AvCodecContext avCtx)//XX 1003
		{
			WmaCodecContext s = (WmaCodecContext)avCtx.Priv_Data;

			s.Last_BitOffset = s.Last_SuperFrame_Len = 0;

			s.Eof_Done = 0;
			avCtx.Internal.Skip_Samples = s.Frame_Len * 2;
		}
		#endregion
	}
}
