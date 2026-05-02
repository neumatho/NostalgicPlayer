/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec
{
	/// <summary>
	/// Wmapro is an MDCT based codec comparable to wma standard or AAC.
	/// The decoding therefore consists of the following steps:
	/// - bitstream decoding
	/// - reconstruction of per-channel data
	/// - rescaling and inverse quantization
	/// - IMDCT
	/// - windowing and overlapp-add
	///
	/// The compressed wmapro bitstream is split into individual packets.
	/// Every such packet contains one or more wma frames.
	/// The compressed frames may have a variable length and frames may
	/// cross packet boundaries.
	/// Common to all wmapro frames is the number of samples that are stored in
	/// a frame.
	/// The number of samples and a few other decode flags are stored
	/// as extradata that has to be passed to the decoder.
	///
	/// The wmapro frames themselves are again split into a variable number of
	/// subframes. Every subframe contains the data for 2^N time domain samples
	/// where N varies between 7 and 12.
	///
	/// Example wmapro bitstream (in samples):
	///
	/// ||   packet 0           || packet 1 || packet 2      packets
	/// ---------------------------------------------------
	/// || frame 0      || frame 1       || frame 2    ||    frames
	/// ---------------------------------------------------
	/// ||   |      |   ||   |   |   |   ||            ||    subframes of channel 0
	/// ---------------------------------------------------
	/// ||      |   |   ||   |   |   |   ||            ||    subframes of channel 1
	/// ---------------------------------------------------
	///
	/// The frame layouts for the individual channels of a wma frame does not need
	/// to be the same.
	///
	/// However, if the offsets and lengths of several subframes of a frame are the
	/// same, the subframes of the channels can be grouped.
	/// Every group may then use special coding techniques like M/S stereo coding
	/// to improve the compression ratio. These channel transformations do not
	/// need to be applied to a whole subframe. Instead, they can also work on
	/// individual scale factor bands (see below).
	/// The coefficients that carry the audio signal in the frequency domain
	/// are transmitted as huffman-coded vectors with 4, 2 and 1 elements.
	/// In addition to that, the encoder can switch to a runlevel coding scheme
	/// by transmitting subframe_length / 128 zero coefficients.
	///
	/// Before the audio signal can be converted to the time domain, the
	/// coefficients have to be rescaled and inverse quantized.
	/// A subframe is therefore split into several scale factor bands that get
	/// scaled individually.
	/// Scale factors are submitted for every frame but they might be shared
	/// between the subframes of a channel. Scale factors are initially DPCM-coded.
	/// Once scale factors are shared, the differences are transmitted as runlevel
	/// codes.
	/// Every subframe length and offset combination in the frame layout shares a
	/// common quantization factor that can be adjusted for every channel by a
	/// modifier.
	/// After the inverse quantization, the coefficients get processed by an IMDCT.
	/// The resulting values are then windowed with a sine window and the first half
	/// of the values are added to the second half of the output from the previous
	/// subframe in order to reconstruct the output samples.
	/// </summary>
	internal static class WmaProDec
	{
		public static readonly FFCodec FF_WmaPro_Decoder;

		private static readonly pthread_once_t init_Static_Once = CThread.pthread_once_init();

		private static readonly VlcElem[] sf_Vlc = ArrayHelper.InitializeArray<VlcElem>(616);			// Scale factor DPCM vlc
		private static readonly VlcElem[] sf_Rl_Vlc = ArrayHelper.InitializeArray<VlcElem>(1406);		// Scale factor run length vlc
		private static readonly VlcElem[] vec4_Vlc = ArrayHelper.InitializeArray<VlcElem>(604);		// 4 coefficients per symbol
		private static readonly VlcElem[] vec2_Vlc = ArrayHelper.InitializeArray<VlcElem>(562);		// 2 coefficients per symbol
		private static readonly VlcElem[] vec1_Vlc = ArrayHelper.InitializeArray<VlcElem>(562);		// 1 coefficient per symbol
		private static readonly CPointer<VlcElem>[] coef_Vlc = new CPointer<VlcElem>[2];					// Coefficient run length vlc codes
		private static readonly c_float[] sin64 = new c_float[33];											// Sine table for decorrelation

		/// <summary>
		/// Integers 0..15 as single-precision floats. The table saves a
		/// costly int to float conversion, and storing the values as
		/// integers allows fast sign-flipping
		/// </summary>
		private static readonly uint32_t[] fVal_Tab =
		[
			0x00000000, 0x3f800000, 0x40000000, 0x40400000,
			0x40800000, 0x40a00000, 0x40c00000, 0x40e00000,
			0x41000000, 0x41100000, 0x41200000, 0x41300000,
			0x41400000, 0x41500000, 0x41600000, 0x41700000
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		static WmaProDec()
		{
			FF_WmaPro_Decoder = new FFCodec
			{
				Name = "wmapro".ToCharPointer(),
				Long_Name = "Windows Media Audio 9 Professional".ToCharPointer(),
				Type = AvMediaType.Audio,
				Id = AvCodecId.WmaPro,
				Priv_Data_Alloc = Alloc_Priv_Data,
				Init = WmaPro_Decode_Init,
				Close = WmaPro_Decode_End,
				Is_Decoder = true,
				Cb_Type = FFCodecType.Decode,
				Flush = WmaPro_Flush,
				Capabilities = AvCodecCap.Dr1,
				Sample_Fmts = new CPointer<AvSampleFormat>([ AvSampleFormat.FltP ]),
				Caps_Internal = FFCodecCap.Init_Cleanup
			};

			FF_WmaPro_Decoder.Cb.Decode = WmaPro_Decode_Packet;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IPrivateData Alloc_Priv_Data()
		{
			return new WmaProDecodeCtx();
		}



		/********************************************************************/
		/// <summary>
		/// Uninitialize the decoder and free all resources
		/// </summary>
		/********************************************************************/
		private static c_int Decode_End(WmaProDecodeCtx s)
		{
			Mem.Av_FreeP(ref s.fDsp);

			for (c_int i = 0; i < WmaConstants.WmaPro_Block_Sizes; i++)
				Tx.Av_Tx_Uninit(ref s.Tx[i]);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int WmaPro_Decode_End(AvCodecContext avCtx)
		{
			WmaProDecodeCtx s = (WmaProDecodeCtx)avCtx.Priv_Data;

			Decode_End(s);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Rate(AvCodecContext avCtx)
		{
			if (avCtx.Codec_Id != AvCodecId.WmaPro)	// XXX: is this really only for XMA?
			{
				if (avCtx.Sample_Rate > 44100)
					return 48000;
				else if (avCtx.Sample_Rate > 32000)
					return 44100;
				else if (avCtx.Sample_Rate > 24000)
					return 32000;

				return 24000;
			}

			return avCtx.Sample_Rate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Decode_Init_Static()
		{
			VlcElem[] vlc_Buf = ArrayHelper.InitializeArray<VlcElem>(2108 + 3912);
			VlcInitState state = Vlc_.Vlc_Init_State(vlc_Buf);

			Vlc_.Vlc_Init_Static_Table_From_Lengths<uint8_t, uint8_t>(sf_Vlc, WmaConstants.ScaleVlcBits, WmaConstants.Huff_Scale_Size, WmaProData.scale_Table2, WmaProData.scale_Table1, -60, VlcInit.None);
			Vlc_.Vlc_Init_Static_Table_From_Lengths<uint8_t, uint8_t>(sf_Rl_Vlc, WmaConstants.VlcBits, WmaConstants.Huff_Scale_Rl_Size, WmaProData.scale_Rl_Table2, WmaProData.scale_Rl_Table1, 0, VlcInit.None);

			coef_Vlc[0] = Vlc_.FF_Vlc_Init_Tables_From_Lengths<uint8_t, uint16_t>(state, WmaConstants.VlcBits, WmaConstants.Huff_Coef0_Size, WmaProData.coef0_Lens, WmaProData.coef0_Syms, 0, VlcInit.None);
			coef_Vlc[1] = Vlc_.FF_Vlc_Init_Tables_From_Lengths<uint8_t, uint8_t>(state, WmaConstants.VlcBits, WmaConstants.Huff_Coef1_Size, WmaProData.coef1_Table2, WmaProData.coef1_Table1, 0, VlcInit.None);

			Vlc_.Vlc_Init_Static_Table_From_Lengths<uint8_t, uint16_t>(vec4_Vlc, WmaConstants.VlcBits, WmaConstants.Huff_Vec4_Size, WmaProData.vec4_Lens, WmaProData.vec4_Syms, -1, VlcInit.None);
			Vlc_.Vlc_Init_Static_Table_From_Lengths<uint8_t, uint8_t>(vec2_Vlc, WmaConstants.VlcBits, WmaConstants.Huff_Vec2_Size, WmaProData.vec2_Table2, WmaProData.vec2_Table1, -1, VlcInit.None);
			Vlc_.Vlc_Init_Static_Table_From_Lengths<uint8_t, uint8_t>(vec1_Vlc, WmaConstants.VlcBits, WmaConstants.Huff_Vec1_Size, WmaProData.vec1_Table2, WmaProData.vec1_Table1, 0, VlcInit.None);

			// Calculate sine values for the decorrelation matrix
			for (c_int i = 0; i < 33; i++)
				sin64[i] = (c_float)CMath.sin(i * Math.PI / 64.0);

			for (c_int i = WmaConstants.WmaPro_Block_Min_Bits; i <= WmaConstants.WmaPro_Block_Max_Bits; i++)
				SineWin_TableGen.FF_Init_FF_Sine_Windows(i);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the decoder
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Init(WmaProDecodeCtx s, AvCodecContext avCtx, c_int num_Stream)
		{
			DataBufferContext eData_Ptr = (DataBufferContext)avCtx.ExtraData;
			c_uint channel_Mask;

			s.AvCtx = avCtx;

			Put_Bits.Init_Put_Bits(s.Pb, s.Frame_Data, WmaConstants.Max_FrameSize);

			avCtx.Sample_Fmt = AvSampleFormat.FltP;

			if ((avCtx.Codec_Id == AvCodecId.Xma2) && (eData_Ptr.Size == 34))	// XMA2WAVEFORMATEX
			{
				s.Decode_Flags = 0x10d6;
				s.Bits_Per_Sample = 16;
				channel_Mask = 0;			// Would need to aggregate from all streams

				if (((num_Stream + 1) * WmaConstants.Xma_Max_Channels_Stream) > avCtx.Ch_Layout.Nb_Channels)	// Stream config is 2ch + 2ch + ... + 1/2 ch
					s.Nb_Channels = 1;
				else
					s.Nb_Channels = 2;
			}
			else if (avCtx.Codec_Id == AvCodecId.Xma2)								// XMA2WAVEFORMAT
			{
				s.Decode_Flags = 0x10d6;
				s.Bits_Per_Sample = 16;
				channel_Mask = 0;			// Would need to aggregate from all streams
				s.Nb_Channels = (int8_t)eData_Ptr.Data[32 + ((eData_Ptr.Data[0] == 3) ? 0 : 8) + (4 * num_Stream) + 0];	// nth stream config
			}
			else if (avCtx.Codec_Id == AvCodecId.Xma1)
			{
				s.Decode_Flags = 0x10d6;
				s.Bits_Per_Sample = 16;
				channel_Mask = 0;			// Would need to aggregate from all streams
				s.Nb_Channels = (int8_t)eData_Ptr.Data[8 + (20 * num_Stream) + 17];	// nth stream config
			}
			else if ((avCtx.Codec_Id == AvCodecId.WmaPro) && (eData_Ptr.Size >= 18))
			{
				s.Decode_Flags = IntReadWrite.Av_RL16(eData_Ptr.Data + 14);
				channel_Mask = IntReadWrite.Av_RL32(eData_Ptr.Data + 2);
				s.Bits_Per_Sample = (uint8_t)IntReadWrite.Av_RL16(eData_Ptr.Data);
				s.Nb_Channels = (int8_t)(channel_Mask != 0 ? Common.Av_PopCount(channel_Mask) : avCtx.Ch_Layout.Nb_Channels);

				if ((s.Bits_Per_Sample > 32) || (s.Bits_Per_Sample < 1))
				{
					Log.AvPriv_Request_Sample(avCtx, "bits per sample is %d", s.Bits_Per_Sample);
					return Error.PatchWelcome;
				}
			}
			else
			{
				Log.AvPriv_Request_Sample(avCtx, "Unknown extradata size");
				return Error.PatchWelcome;
			}

			// Generic init
			s.Log2_Frame_Size = (uint16_t)(IntMath.Av_Log2((c_uint)avCtx.Block_Align) + 4);

			if (s.Log2_Frame_Size > 25)
			{
				Log.AvPriv_Request_Sample(avCtx, "Large block align");
				return Error.PatchWelcome;
			}

			// Frame info
			s.Skip_Frame = 1;	// Skip first frame

			s.Packet_Loss = 1;
			s.Len_Prefix = (uint8_t)(s.Decode_Flags & 0x40);

			// Get frame len
			if (avCtx.Codec_Id == AvCodecId.WmaPro)
			{
				c_int bits = Wma_Common.FF_Wma_Get_Frame_Len_Bits(avCtx.Sample_Rate, 3, s.Decode_Flags);

				if (bits > WmaConstants.WmaPro_Block_Max_Bits)
				{
					Log.AvPriv_Request_Sample(avCtx, "14-bit block sizes");
					return Error.PatchWelcome;
				}

				s.Samples_Per_Frame = (uint16_t)(1 << bits);
			}
			else
				s.Samples_Per_Frame = 512;

			// Subframe info
			c_int log2_Max_Num_Subframes = (c_int)((s.Decode_Flags & 0x38) >> 3);
			s.Max_Num_Subframes = (uint8_t)(1 << log2_Max_Num_Subframes);

			if ((s.Max_Num_Subframes == 16) || (s.Max_Num_Subframes == 4))
				s.Max_Subframe_Len_Bit = 1;

			s.Subframe_Len_Bits = (uint8_t)(IntMath.Av_Log2((c_uint)log2_Max_Num_Subframes) + 1);

			c_int num_Possible_Block_Sizes = log2_Max_Num_Subframes + 1;
			s.Min_Samples_Per_Subframe = (uint16_t)(s.Samples_Per_Frame / s.Max_Num_Subframes);
			s.Dynamic_Range_Compression = (uint8_t)(s.Decode_Flags & 0x80);

			if (s.Max_Num_Subframes > WmaConstants.Max_Subframes)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "invalid number of subframes %d\n", s.Max_Num_Subframes);
				return Error.InvalidData;
			}

			if (s.Min_Samples_Per_Subframe < WmaConstants.WmaPro_Block_Min_Size)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "min_samples_per_subframe of %d too small\n", s.Min_Samples_Per_Subframe);
				return Error.InvalidData;
			}

			if (s.Nb_Channels <= 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "invalid number of channels %d\n", s.Nb_Channels);
				return Error.InvalidData;
			}
			else if ((avCtx.Codec_Id != AvCodecId.WmaPro) && (s.Nb_Channels > WmaConstants.Xma_Max_Channels_Stream))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "invalid number of channels per XMA stream %d\n", s.Nb_Channels);
				return Error.InvalidData;
			}
			else if ((s.Nb_Channels > WmaConstants.WmaPro_Max_Channels) || (s.Nb_Channels > avCtx.Ch_Layout.Nb_Channels))
			{
				Log.AvPriv_Request_Sample(avCtx, "More than %d channels", WmaConstants.WmaPro_Max_Channels);
				return Error.PatchWelcome;
			}

			// Init previous block len
			for (c_int i = 0; i < s.Nb_Channels; i++)
				s.Channel[i].Prev_Block_Len = (int16_t)s.Samples_Per_Frame;

			// Extract lfe channel position
			s.Lfe_Channel = -1;

			if ((channel_Mask & 8) != 0)
			{
				for (c_uint mask = 1; mask < 16; mask <<= 1)
				{
					if ((channel_Mask & mask) != 0)
						++s.Lfe_Channel;
				}
			}

			// Calculate number of scale factor bands and their offsets
			// for every possible block size
			for (c_int i = 0; i < num_Possible_Block_Sizes; i++)
			{
				c_int subframe_Len = s.Samples_Per_Frame >> i;
				c_int band = 1;
				c_int rate = Get_Rate(avCtx);

				s.Sfb_Offsets[i][0] = 0;

				for (c_int x = 0; (x < WmaConstants.Max_Bands - 1) && (s.Sfb_Offsets[i][band - 1] < subframe_Len); x++)
				{
					c_int offset = ((subframe_Len * 2 * WmaProData.critical_Freq[x]) / rate) + 2;
					offset &= ~3;

					if (offset > s.Sfb_Offsets[i][band - 1])
						s.Sfb_Offsets[i][band++] = (int16_t)offset;

					if (offset >= subframe_Len)
						break;
				}

				s.Sfb_Offsets[i][band - 1] = (int16_t)subframe_Len;
				s.Num_Sfb[i] = (int8_t)(band - 1);

				if (s.Num_Sfb[i] <= 0)
				{
					Log.Av_Log(avCtx, Log.Av_Log_Error, "num_sfb invalid\n");
					return Error.InvalidData;
				}
			}

			// Scale factors can be shared between blocks of different size
			// as every block has a different scale factor band layout.
			// The matrix sf_offsets is needed to find the correct scale factor

			for (c_int i = 0; i < num_Possible_Block_Sizes; i++)
			{
				for (c_int b = 0; b < s.Num_Sfb[i]; b++)
				{
					c_int offset = ((s.Sfb_Offsets[i][b] + s.Sfb_Offsets[i][b + 1] - 1) << i) >> 1;

					for (c_int x = 0; x < num_Possible_Block_Sizes; x++)
					{
						c_int v = 0;

						while ((s.Sfb_Offsets[x][v + 1] << x) < offset)
							v++;

						s.Sf_Offsets[i][x][b] = (int8_t)v;
					}
				}
			}

			s.fDsp = Float_Dsp.AvPriv_Float_Dsp_Alloc((c_int)(avCtx.Flags & AvCodecFlag.BitExact));

			if (s.fDsp == null)
				return Error.ENOMEM;

			// Init MDCT, FIXME: only init needed sizes
			for (c_int i = 0; i < WmaConstants.WmaPro_Block_Sizes; i++)
			{
				c_float scale = 1.0f / (1 << (WmaConstants.WmaPro_Block_Min_Bits + i - 1)) / (1L << (s.Bits_Per_Sample - 1));
				c_int err = Tx.Av_Tx_Init(out s.Tx[i], out s.Tx_Fn[i], AvTxType.Float_Mdct, 1, 1 << (WmaConstants.WmaPro_Block_Min_Bits + i), ref scale, AvTxFlags.None);

				if (err < 0)
					return err;
			}

			// Init MDCT windows: simple sine window
			for (c_int i = 0; i < WmaConstants.WmaPro_Block_Sizes; i++)
			{
				c_int win_Idx = WmaConstants.WmaPro_Block_Max_Bits - i;
				s.Windows[WmaConstants.WmaPro_Block_Sizes - i - 1] = SineWin_TableGen.ff_Sine_Windows[win_Idx];
			}

			// Calculate subwoofer cutoff values
			for (c_int i = 0; i < num_Possible_Block_Sizes; i++)
			{
				c_int block_Size = s.Samples_Per_Frame >> i;
				c_int cutOff = (c_int)(((440 * block_Size) + (3L * (s.AvCtx.Sample_Rate >> 1)) - 1) / s.AvCtx.Sample_Rate);
				s.Subwoofer_CutOffs[i] = (int16_t)Common.Av_Clip(cutOff, 4, block_Size);
			}

			if (avCtx.Codec_Id == AvCodecId.WmaPro)
			{
				if (channel_Mask != 0)
				{
					Channel_Layout.Av_Channel_Layout_Uninit(avCtx.Ch_Layout);
					Channel_Layout.Av_Channel_Layout_From_Mask(avCtx.Ch_Layout, (AvChannelMask)channel_Mask);
				}
				else
					avCtx.Ch_Layout.Order = AvChannelOrder.Unspec;
			}

			CThread.pthread_once(init_Static_Once, Decode_Init_Static);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the decoder
		/// </summary>
		/********************************************************************/
		private static c_int WmaPro_Decode_Init(AvCodecContext avCtx)
		{
			WmaProDecodeCtx s = (WmaProDecodeCtx)avCtx.Priv_Data;

			if (avCtx.Block_Align == 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "block_align is not set\n");

				return Error.EINVAL;
			}

			return Decode_Init(s, avCtx, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Decode the subframe length
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Subframe_Length(WmaProDecodeCtx s, c_int offset)
		{
			c_int frame_Len_Shift = 0;

			// No need to read from the bitstream when only one length is possible
			if (offset == (s.Samples_Per_Frame - s.Min_Samples_Per_Subframe))
				return s.Min_Samples_Per_Subframe;

			if (Get_Bits.Get_Bits_Left(s.Gb) < 1)
				return Error.InvalidData;

			// 1 bit indicates if the subframe is of maximum length
			if (s.Max_Subframe_Len_Bit != 0)
			{
				if (Get_Bits.Get_Bits1(s.Gb) != 0)
					frame_Len_Shift = 1 + (c_int)Get_Bits._Get_Bits(s.Gb, s.Subframe_Len_Bits - 1);
			}
			else
				frame_Len_Shift = (c_int)Get_Bits._Get_Bits(s.Gb, s.Subframe_Len_Bits);

			c_int subframe_Len = s.Samples_Per_Frame >> frame_Len_Shift;

			// Sanity check the length
			if ((subframe_Len < s.Min_Samples_Per_Subframe) || (subframe_Len > s.Samples_Per_Frame))
			{
				Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "broken frame: subframe_len %i\n", subframe_Len);
				return Error.InvalidData;
			}

			return subframe_Len;
		}



		/********************************************************************/
		/// <summary>
		/// Decode how the data in the frame is split into subframes.
		/// Every WMA frame contains the encoded data for a fixed number of
		/// samples per channel. The data for every channel might be split
		/// into several subframes. This function will reconstruct the list
		/// of subframes for every channel.
		///
		/// If the subframes are not evenly split, the algorithm estimates
		/// the channels with the lowest number of total samples.
		/// Afterwards, for each of these channels a bit is read from the
		/// bitstream that indicates if the channel contains a subframe with
		/// the next subframe size that is going to be read from the
		/// bitstream or not.
		/// If a channel contains such a subframe, the subframe size gets
		/// added to the channel's subframe list.
		/// The algorithm repeats these steps until the frame is properly
		/// divided between the individual channels
		/// </summary>
		/********************************************************************/
		private static c_int Decode_TileHdr(WmaProDecodeCtx s)
		{
			uint16_t[] num_Samples = new uint16_t[WmaConstants.WmaPro_Max_Channels];		// Sum of samples for all currently known subframes of a channel
			uint8_t[] contains_Subframe = new uint8_t[WmaConstants.WmaPro_Max_Channels];	// Flag indicating if a channel contains the current subframe
			c_int channels_For_Cur_Subframe = s.Nb_Channels;								// Number of channels that contain the current subframe
			c_int fixed_Channel_Layout = 0;													// Flag indicating that all channels use the same subframe offsets and sizes
			c_int min_Channel_Len = 0;														// Smallest sum of samples (channels with this length will be processed first)

			// Should never consume more than 3073 bits (256 iterations for the
			// while loop when always the minimum amount of 128 samples is subtracted
			// from missing samples in the 8 channel case).
			// 1 + BLOCK_MAX_SIZE * MAX_CHANNELS / BLOCK_MIN_SIZE * (MAX_CHANNELS  + 4)

			// Reset tiling information
			for (c_int c = 0; c < s.Nb_Channels; c++)
				s.Channel[c].Num_Subframes = 0;

			if ((s.Max_Num_Subframes == 1) || (Get_Bits.Get_Bits1(s.Gb) != 0))
				fixed_Channel_Layout = 1;

			// Loop until the frame data is split between the subframes
			do
			{
				// Check which channels contain the subframe
				for (c_int c = 0; c < s.Nb_Channels; c++)
				{
					if (num_Samples[c] == min_Channel_Len)
					{
						if ((fixed_Channel_Layout != 0) || (channels_For_Cur_Subframe == 1) || (min_Channel_Len == (s.Samples_Per_Frame - s.Min_Samples_Per_Subframe)))
							contains_Subframe[c] = 1;
						else
							contains_Subframe[c] = (uint8_t)Get_Bits.Get_Bits1(s.Gb);
					}
					else
						contains_Subframe[c] = 0;
				}

				// Get subframe length, subframe_len == 0 is not allowed
				c_int subframe_Len = Decode_Subframe_Length(s, min_Channel_Len);

				if (subframe_Len <= 0)
					return Error.InvalidData;

				// Add subframes to the individual channels and find new min_channel_len
				min_Channel_Len += subframe_Len;

				for (c_int c = 0; c < s.Nb_Channels; c++)
				{
					WmaProChannelCtx chan = s.Channel[c];

					if (contains_Subframe[c] != 0)
					{
						if (chan.Num_Subframes >= WmaConstants.Max_Subframes)
						{
							Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "broken frame: num subframes > 31\n");
							return Error.InvalidData;
						}

						chan.Subframe_Len[chan.Num_Subframes] = (uint16_t)subframe_Len;
						num_Samples[c] += (uint16_t)subframe_Len;
						++chan.Num_Subframes;

						if (num_Samples[c] > s.Samples_Per_Frame)
						{
							Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "broken frame: channel len > samples_per_frame\n");
							return Error.InvalidData;
						}
					}
					else if (num_Samples[c] <= min_Channel_Len)
					{
						if (num_Samples[c] < min_Channel_Len)
						{
							channels_For_Cur_Subframe = 0;
							min_Channel_Len = num_Samples[c];
						}

						++channels_For_Cur_Subframe;
					}
				}
			}
			while (min_Channel_Len < s.Samples_Per_Frame);

			for (c_int c = 0; c < s.Nb_Channels; c++)
			{
				c_int offset = 0;

				for (c_int i = 0; i < s.Channel[c].Num_Subframes; i++)
				{
					Log.FF_DLog(s.AvCtx, "frame[%u] channel[%i] subframe[%i] len %i\n", s.Frame_Num, c, i, s.Channel[c].Subframe_Len[i]);

					s.Channel[c].Subframe_Offset[i] = (uint16_t)offset;
					offset += s.Channel[c].Subframe_Len[i];
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate a decorrelation matrix from the bitstream parameters
		/// </summary>
		/********************************************************************/
		private static void Decode_Decorrelation_Matrix(WmaProDecodeCtx s, WmaProChannelGrp chGroup)
		{
			c_int offset = 0;
			int8_t[] rotation_Offset = new int8_t[WmaConstants.WmaPro_Max_Channels * WmaConstants.WmaPro_Max_Channels];

			CMemory.memset(chGroup.Decorrelation_Matrix, 0.0f, (size_t)(s.Nb_Channels * s.Nb_Channels));

			for (c_int i = 0; i < ((chGroup.Num_Channels * (chGroup.Num_Channels - 1)) >> 1); i++)
				rotation_Offset[i] = (int8_t)Get_Bits._Get_Bits(s.Gb, 6);

			for (c_int i = 0; i < chGroup.Num_Channels; i++)
				chGroup.Decorrelation_Matrix[(chGroup.Num_Channels * i) + i] = Get_Bits.Get_Bits1(s.Gb) != 0 ? 1.0f : -1.0f;

			for (c_int i = 1; i < chGroup.Num_Channels; i++)
			{
				for (c_int x = 0; x < i; x++)
				{
					for (c_int y = 0; y < (i + 1); y++)
					{
						c_float v1 = chGroup.Decorrelation_Matrix[(x * chGroup.Num_Channels) + y];
						c_float v2 = chGroup.Decorrelation_Matrix[(i * chGroup.Num_Channels) + y];
						c_int n = rotation_Offset[offset + x];
						c_float sinV;
						c_float cosV;

						if (n < 32)
						{
							sinV = sin64[n];
							cosV = sin64[32 - n];
						}
						else
						{
							sinV = sin64[64 - n];
							cosV = -sin64[n - 32];
						}

						chGroup.Decorrelation_Matrix[y + (x * chGroup.Num_Channels)] = (v1 * sinV) - (v2 * cosV);
						chGroup.Decorrelation_Matrix[y + (i * chGroup.Num_Channels)] = (v1 * cosV) + (v2 * sinV);
					}
				}

				offset += i;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Decode channel transformation parameters
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Channel_Transform(WmaProDecodeCtx s)
		{
			// Should never consume more than 1921 bits for the 8 channel case
			// 1 + MAX_CHANNELS * (MAX_CHANNELS + 2 + 3 * MAX_CHANNELS * MAX_CHANNELS
			// + MAX_CHANNELS + MAX_BANDS + 1)

			// In the one channel case channel transforms are pointless
			s.Num_ChGroups = 0;

			if (s.Nb_Channels > 1)
			{
				c_int remaining_Channels = s.Channels_For_Cur_Subframe;

				if (Get_Bits.Get_Bits1(s.Gb) != 0)
				{
					Log.AvPriv_Request_Sample(s.AvCtx, "Channel transform bit");
					return Error.PatchWelcome;
				}

				for (s.Num_ChGroups = 0; (remaining_Channels != 0) && (s.Num_ChGroups < s.Channels_For_Cur_Subframe); s.Num_ChGroups++)
				{
					WmaProChannelGrp chGroup = s.ChGroup[s.Num_ChGroups];
					CPointer<CPointer<c_float>> channel_Data = chGroup.Channel_Data;
					chGroup.Num_Channels = 0;
					chGroup.Transform = 0;

					// Decode channel mask
					if (remaining_Channels > 2)
					{
						for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
						{
							c_int channel_Idx = s.Channel_Indexes_For_Cur_Subframe[i];

							if ((s.Channel[channel_Idx].Grouped == 0) && (Get_Bits.Get_Bits1(s.Gb) != 0))
							{
								++chGroup.Num_Channels;
								s.Channel[channel_Idx].Grouped = 1;
								channel_Data[0, 1] = s.Channel[channel_Idx].Coeffs;
							}
						}
					}
					else
					{
						chGroup.Num_Channels = (uint8_t)remaining_Channels;

						for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
						{
							c_int channel_Idx = s.Channel_Indexes_For_Cur_Subframe[i];

							if (s.Channel[channel_Idx].Grouped == 0)
								channel_Data[0, 1] = s.Channel[channel_Idx].Coeffs;

							s.Channel[channel_Idx].Grouped = 1;
						}
					}

					// Decode transform type
					if (chGroup.Num_Channels == 2)
					{
						if (Get_Bits.Get_Bits1(s.Gb) != 0)
						{
							if (Get_Bits.Get_Bits1(s.Gb) != 0)
							{
								Log.AvPriv_Request_Sample(s.AvCtx, "Unknown channel transform type");
								return Error.PatchWelcome;
							}
						}
						else
						{
							chGroup.Transform = 1;

							if (s.Nb_Channels == 2)
							{
								chGroup.Decorrelation_Matrix[0] = 1.0f;
								chGroup.Decorrelation_Matrix[1] = -1.0f;
								chGroup.Decorrelation_Matrix[2] = 1.0f;
								chGroup.Decorrelation_Matrix[3] = 1.0f;
							}
							else
							{
								// cos(pi/4)
								chGroup.Decorrelation_Matrix[0] = 0.70703125f;
								chGroup.Decorrelation_Matrix[1] = -0.70703125f;
								chGroup.Decorrelation_Matrix[2] = 0.70703125f;
								chGroup.Decorrelation_Matrix[3] = 0.70703125f;
							}
						}
					}
					else if (chGroup.Num_Channels > 2)
					{
						if (Get_Bits.Get_Bits1(s.Gb) != 0)
						{
							chGroup.Transform = 1;

							if (Get_Bits.Get_Bits1(s.Gb) != 0)
								Decode_Decorrelation_Matrix(s, chGroup);
							else
							{
								// FIXME: more than 6 coupled channels not supported
								if (chGroup.Num_Channels > 6)
									Log.AvPriv_Request_Sample(s.AvCtx, "Coupled channels > 6");
								else
									CMemory.memcpy(chGroup.Decorrelation_Matrix, WmaProData.default_Decorrelation[chGroup.Num_Channels], (size_t)(chGroup.Num_Channels * chGroup.Num_Channels));
							}
						}
					}

					// Decode transform on / off
					if (chGroup.Transform != 0)
					{
						if (Get_Bits.Get_Bits1(s.Gb) == 0)
						{
							// Transform can be enabled for individual bands
							for (c_int i = 0; i < s.Num_Bands; i++)
								chGroup.Transform_Band[i] = (int8_t)Get_Bits.Get_Bits1(s.Gb);
						}
						else
							CMemory.memset<int8_t>(chGroup.Transform_Band, 1, (size_t)s.Num_Bands);
					}

					remaining_Channels -= chGroup.Num_Channels;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Extract the coefficients from the bitstream
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Coeffs(WmaProDecodeCtx s, c_int c)
		{
			WmaProChannelCtx ci = s.Channel[c];
			c_int rl_Mode = 0;
			c_int cur_Coeff = 0;
			c_int num_Zeros = 0;
			CPointer<uint16_t> run;
			CPointer<c_float> level;

			Log.FF_DLog(s.AvCtx, "decode coefficients for channel %i\n", c);

			c_int vlcTable = (c_int)Get_Bits.Get_Bits1(s.Gb);
			CPointer<VlcElem> vlc = coef_Vlc[vlcTable];

			if (vlcTable != 0)
			{
				run = WmaProData.coef1_Run;
				level = WmaProData.coef1_Level;
			}
			else
			{
				run = WmaProData.coef0_Run;
				level = WmaProData.coef0_Level;
			}

			// Decode vector coefficients (consumes up to 167 bits per iteration for
			// 4 vector coded large values)
			while (((s.Transmit_Num_Vec_Coeffs != 0) || (rl_Mode == 0)) && ((cur_Coeff + 3) < ci.Num_Vec_Coeffs))
			{
				uint32_t[] vals = new uint32_t[4];

				c_uint idx = (c_uint)Get_Bits.Get_Vlc2(s.Gb, vec4_Vlc, WmaConstants.VlcBits, WmaConstants.Vec4MaxDepth);

				if ((c_int)idx < 0)
				{
					for (c_int i = 0; i < 4; i += 2)
					{
						idx = (c_uint)Get_Bits.Get_Vlc2(s.Gb, vec2_Vlc, WmaConstants.VlcBits, WmaConstants.Vec2MaxDepth);

						if ((c_int)idx < 0)
						{
							uint32_t v0 = (c_uint)Get_Bits.Get_Vlc2(s.Gb, vec1_Vlc, WmaConstants.VlcBits, WmaConstants.Vec1MaxDepth);

							if (v0 == (WmaConstants.Huff_Vec1_Size - 1))
								v0 += (uint32_t)Wma.FF_Wma_Get_Large_Val(s.Gb);

							uint32_t v1 = (c_uint)Get_Bits.Get_Vlc2(s.Gb, vec1_Vlc, WmaConstants.VlcBits, WmaConstants.Vec1MaxDepth);

							if (v1 == (WmaConstants.Huff_Vec1_Size - 1))
								v1 += (uint32_t)Wma.FF_Wma_Get_Large_Val(s.Gb);

							vals[i] = IntFloat.Av_Float2Int(v0);
							vals[i + 1] = IntFloat.Av_Float2Int(v1);
						}
						else
						{
							vals[i] = fVal_Tab[idx >> 4];
							vals[i + 1] = fVal_Tab[idx & 0xf];
						}
					}
				}
				else
				{
					vals[0] = fVal_Tab[idx >> 12];
					vals[1] = fVal_Tab[(idx >> 8) & 0xf];
					vals[2] = fVal_Tab[(idx >> 4) & 0xf];
					vals[3] = fVal_Tab[idx & 0xf];
				}

				// Decode sign
				for (c_int i = 0; i < 4; i++)
				{
					if (vals[i] != 0)
					{
						uint32_t sign = Get_Bits.Get_Bits1(s.Gb) - 1;
						IntReadWrite.Av_WN32A(ci.Coeffs + cur_Coeff, vals[i] ^ (sign << 31));
						num_Zeros = 0;
					}
					else
					{
						ci.Coeffs[cur_Coeff] = 0;

						// Wwitch to run level mode when subframe_len / 128 zeros
						// were found in a row
						rl_Mode |= (++num_Zeros > (s.Subframe_Len >> 8)) ? 1 : 0;
					}

					++cur_Coeff;
				}
			}

			// Decode run level coded coefficients
			if (cur_Coeff < s.Subframe_Len)
			{
				CMemory.memset(ci.Coeffs + cur_Coeff, 0.0f, (size_t)(s.Subframe_Len - cur_Coeff));

				c_int ret = Wma.FF_Wma_Run_Level_Decode(s.AvCtx, s.Gb, vlc, level, run, 1, ci.Coeffs, cur_Coeff, s.Subframe_Len, s.Subframe_Len, s.Esc_Len, 0);

				if (ret < 0)
					return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Extract scale factors from the bitstream
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Scale_Factors(WmaProDecodeCtx s)
		{
			// should never consume more than 5344 bits
			// MAX_CHANNELS * (1 +  MAX_BANDS * 23)
			for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
			{
				c_int c = s.Channel_Indexes_For_Cur_Subframe[i];

				s.Channel[c].Scale_Factors = s.Channel[c].Saved_Scale_Factors[s.Channel[c].Scale_Factor_Idx == 0 ? 1 : 0];
				CPointer<c_int> sf_End = s.Channel[c].Scale_Factors + s.Num_Bands;

				// Resample scale factors for the new block size
				// as the scale factors might need to be resampled several times
				// before some new values are transmitted, a backup of the last
				// transmitted scale factors is kept in saved_scale_factors
				if (s.Channel[c].Reuse_Sf != 0)
				{
					CPointer<int8_t> sf_Offsets = s.Sf_Offsets[s.Table_Idx][s.Channel[c].Table_Idx];

					for (c_int b = 0; b < s.Num_Bands; b++)
						s.Channel[c].Scale_Factors[b] = s.Channel[c].Saved_Scale_Factors[s.Channel[c].Scale_Factor_Idx][sf_Offsets[0, 1]];
				}

				if ((s.Channel[c].Cur_Subframe == 0) || (Get_Bits.Get_Bits1(s.Gb) != 0))
				{
					if (s.Channel[c].Reuse_Sf == 0)
					{
						// Decode DPCM coded scale factors
						s.Channel[c].Scale_Factor_Step = (int8_t)(Get_Bits._Get_Bits(s.Gb, 2) + 1);
						c_int val = 45 / s.Channel[c].Scale_Factor_Step;

						for (CPointer<c_int> sf = s.Channel[c].Scale_Factors; sf < sf_End; sf++)
						{
							val += Get_Bits.Get_Vlc2(s.Gb, sf_Vlc, WmaConstants.ScaleVlcBits, WmaConstants.ScaleMaxDepth);
							sf[0] = val;
						}
					}
					else
					{
						// Run level decode differences to the resampled factors
						for (c_int i_ = 0; i_ < s.Num_Bands; i_++)
						{
							c_int skip;
							c_int val;
							c_int sign;

							c_int idx = Get_Bits.Get_Vlc2(s.Gb, sf_Rl_Vlc, WmaConstants.VlcBits, WmaConstants.ScaleRlMaxDepth);

							if (idx == 0)
							{
								uint32_t code = Get_Bits._Get_Bits(s.Gb, 14);
								val = (c_int)(code >> 6);
								sign = (c_int)((code & 1) - 1);
								skip = (c_int)((code & 0x3f) >> 1);
							}
							else if (idx == 1)
								break;
							else
							{
								skip = WmaProData.scale_Rl_Run[idx];
								val = WmaProData.scale_Rl_Level[idx];
								sign = (c_int)(Get_Bits.Get_Bits1(s.Gb) - 1);
							}

							i_ += skip;

							if (i_ >= s.Num_Bands)
							{
								Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "invalid scale factor coding\n");
								return Error.InvalidData;
							}

							s.Channel[c].Scale_Factors[i_] += (val ^ sign) - sign;
						}
					}

					// Swap buffers
					s.Channel[c].Scale_Factor_Idx = (int8_t)(s.Channel[c].Scale_Factor_Idx == 0 ? 1 : 0);
					s.Channel[c].Table_Idx = s.Table_Idx;
					s.Channel[c].Reuse_Sf = 1;
				}

				// Calculate new scale factor maximum
				s.Channel[c].Max_Scale_Factor = s.Channel[c].Scale_Factors[0];

				for (CPointer<c_int> sf = s.Channel[c].Scale_Factors + 1; sf < sf_End; sf++)
					s.Channel[c].Max_Scale_Factor = Macros.FFMax(s.Channel[c].Max_Scale_Factor, sf[0]);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reconstruct the individual channel data
		/// </summary>
		/********************************************************************/
		private static void Inverse_Channel_Transform(WmaProDecodeCtx s)
		{
			for (c_int i = 0; i < s.Num_ChGroups; i++)
			{
				if (s.ChGroup[i].Transform != 0)
				{
					CPointer<c_float> data = new CPointer<c_float>(WmaConstants.WmaPro_Max_Channels);

					c_int num_Channels = s.ChGroup[i].Num_Channels;
					CPointer<CPointer<c_float>> ch_Data = s.ChGroup[i].Channel_Data;
					CPointer<CPointer<c_float>> ch_End = ch_Data + num_Channels;
					CPointer<int8_t> tb = s.ChGroup[i].Transform_Band;

					// Multichannel decorrelation
					for (CPointer<int16_t> sfb = s.Cur_Sfb_Offsets; sfb < s.Cur_Sfb_Offsets + s.Num_Bands; sfb++)
					{
						if (tb[0, 1] == 1)
						{
							// Multiply values with the decorrelation_matrix
							for (c_int y = sfb[0]; y < Macros.FFMin(sfb[1], s.Subframe_Len); y++)
							{
								CPointer<c_float> mat = s.ChGroup[i].Decorrelation_Matrix;
								CPointer<c_float> data_End = data + num_Channels;
								CPointer<c_float> data_Ptr = data;

								for (CPointer<CPointer<c_float>> ch = ch_Data; ch < ch_End; ch++)
									data_Ptr[0, 1] = ch[0][y];

								for (CPointer<CPointer<c_float>> ch = ch_Data; ch < ch_End; ch++)
								{
									c_float sum = 0;
									data_Ptr = data;

									while (data_Ptr < data_End)
										sum += data_Ptr[0, 1] * mat[0, 1];

									ch[0][y] = sum;
								}
							}
						}
						else if (s.Nb_Channels == 2)
						{
							c_int len = Macros.FFMin(sfb[1], s.Subframe_Len) - sfb[0];

							s.fDsp.Vector_FMul_Scalar(ch_Data[0] + sfb[0], ch_Data[0] + sfb[0], 181.0f / 128, len);
							s.fDsp.Vector_FMul_Scalar(ch_Data[1] + sfb[0], ch_Data[1] + sfb[0], 181.0f / 128, len);
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Apply sine window and reconstruct the output buffer
		/// </summary>
		/********************************************************************/
		private static void WmaPro_Window(WmaProDecodeCtx s)
		{
			for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
			{
				c_int c = s.Channel_Indexes_For_Cur_Subframe[i];
				c_int winLen = s.Channel[c].Prev_Block_Len;
				CPointer<c_float> start = s.Channel[c].Coeffs - (winLen >> 1);

				if (s.Subframe_Len < winLen)
				{
					start += (winLen - s.Subframe_Len) >> 1;
					winLen = s.Subframe_Len;
				}

				CPointer<c_float> window = s.Windows[IntMath.Av_Log2((c_uint)winLen) - WmaConstants.WmaPro_Block_Min_Bits];

				winLen >>= 1;

				s.fDsp.Vector_FMul_Window(start, start, start + winLen, window, winLen);

				s.Channel[c].Prev_Block_Len = s.Subframe_Len;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Decode a single subframe (block)
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Subframe(WmaProDecodeCtx s)
		{
			c_int offset = s.Samples_Per_Frame;
			c_int subframe_Len = s.Samples_Per_Frame;
			c_int total_Samples = s.Samples_Per_Frame * s.Nb_Channels;
			c_int transmit_Coeefs = 0;

			s.Subframe_Offset = Get_Bits.Get_Bits_Count(s.Gb);

			// Reset channel context and find the next block offset and size
			// == the next block of the channel with the smallest number of
			// decoded samples
			for (c_int i = 0; i < s.Nb_Channels; i++)
			{
				s.Channel[i].Grouped = 0;

				if (offset > s.Channel[i].Decoded_Samples)
				{
					offset = s.Channel[i].Decoded_Samples;
					subframe_Len = s.Channel[i].Subframe_Len[s.Channel[i].Cur_Subframe];
				}
			}

			Log.FF_DLog(s.AvCtx, "processing subframe with offset %i len %i\n", offset, subframe_Len);

			// Get a list of all channels that contain the estimated block
			s.Channels_For_Cur_Subframe = 0;

			for (c_int i = 0; i < s.Nb_Channels; i++)
			{
				c_int cur_Subframe = s.Channel[i].Cur_Subframe;

				// Subtract already processed samples
				total_Samples -= s.Channel[i].Decoded_Samples;

				// And count if there are multiple subframes that match our profile
				if ((offset == s.Channel[i].Decoded_Samples) && (subframe_Len == s.Channel[i].Subframe_Len[cur_Subframe]))
				{
					total_Samples -= s.Channel[i].Subframe_Len[cur_Subframe];
					s.Channel[i].Decoded_Samples += s.Channel[i].Subframe_Len[cur_Subframe];
					s.Channel_Indexes_For_Cur_Subframe[s.Channels_For_Cur_Subframe] = (int8_t)i;
					++s.Channels_For_Cur_Subframe;
				}
			}

			// Check if the frame will be complete after processing the
			// estimated block
			if (total_Samples == 0)
				s.Parsed_All_Subframes = 1;

			Log.FF_DLog(s.AvCtx, "subframe is part of %i channels\n", s.Channels_For_Cur_Subframe);

			// Calculate number of scale factor bands and their offsets
			s.Table_Idx = (uint8_t)IntMath.Av_Log2((c_uint)(s.Samples_Per_Frame / subframe_Len));
			s.Num_Bands = s.Num_Sfb[s.Table_Idx];
			s.Cur_Sfb_Offsets = s.Sfb_Offsets[s.Table_Idx];
			c_int cur_Subwoofer_CutOff = s.Subwoofer_CutOffs[s.Table_Idx];

			// Configure the decoder for the current subframe
			offset += s.Samples_Per_Frame >> 1;

			for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
			{
				c_int c = s.Channel_Indexes_For_Cur_Subframe[i];

				s.Channel[c].Coeffs = s.Channel[c].Out + offset;
			}

			s.Subframe_Len = (int16_t)subframe_Len;
			s.Esc_Len = (int8_t)(IntMath.Av_Log2((c_uint)(s.Subframe_Len - 1)) + 1);

			// Skip extended header if any
			if (Get_Bits.Get_Bits1(s.Gb) != 0)
			{
				c_int num_Fill_Bits = (c_int)Get_Bits._Get_Bits(s.Gb, 2);

				if (num_Fill_Bits == 0)
				{
					c_int len = (c_int)Get_Bits._Get_Bits(s.Gb, 4);
					num_Fill_Bits = Get_Bits.Get_Bitsz(s.Gb, len) + 1;
				}

				if (num_Fill_Bits >= 0)
				{
					if ((Get_Bits.Get_Bits_Count(s.Gb) + num_Fill_Bits) > s.Num_Saved_Bits)
					{
						Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "invalid number of fill bits\n");
						return Error.InvalidData;
					}

					Get_Bits.Skip_Bits_Long(s.Gb, num_Fill_Bits);
				}
			}

			// No idea for what the following bit is used
			if (Get_Bits.Get_Bits1(s.Gb) != 0)
			{
				Log.AvPriv_Request_Sample(s.AvCtx, "Reserved bit");
				return Error.PatchWelcome;
			}

			if (Decode_Channel_Transform(s) < 0)
				return Error.InvalidData;

			for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
			{
				c_int c = s.Channel_Indexes_For_Cur_Subframe[i];
				s.Channel[c].Transmit_Coefs = (uint8_t)Get_Bits.Get_Bits1(s.Gb);

				if (s.Channel[c].Transmit_Coefs != 0)
					transmit_Coeefs = 1;
			}

			if (transmit_Coeefs != 0)
			{
				c_int quant_Step = (90 * s.Bits_Per_Sample) >> 4;

				// Decode number of vector coded coefficients
				s.Transmit_Num_Vec_Coeffs = (int8_t)Get_Bits.Get_Bits1(s.Gb);

				if (s.Transmit_Num_Vec_Coeffs != 0)
				{
					c_int num_Bits = IntMath.Av_Log2((c_uint)((s.Subframe_Len + 3) / 4)) + 1;

					for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
					{
						c_int c = s.Channel_Indexes_For_Cur_Subframe[i];
						c_int num_Vec_Coeffs = (c_int)Get_Bits._Get_Bits(s.Gb, num_Bits) << 2;

						if (num_Vec_Coeffs > s.Subframe_Len)
						{
							Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "num_vec_coeffs %d is too large\n", num_Vec_Coeffs);
							return Error.InvalidData;
						}

						s.Channel[c].Num_Vec_Coeffs = (uint16_t)num_Vec_Coeffs;
					}
				}
				else
				{
					for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
					{
						c_int c = s.Channel_Indexes_For_Cur_Subframe[i];
						s.Channel[c].Num_Vec_Coeffs = (uint16_t)s.Subframe_Len;
					}
				}

				// Decode quantization step
				c_int step = Get_Bits.Get_SBits(s.Gb, 6);
				quant_Step += step;

				if ((step == -32) || (step == 31))
				{
					c_int sign = (step == 31 ? 1 : 0) - 1;
					c_int quant = 0;

					while (((Get_Bits.Get_Bits_Count(s.Gb) + 5) < s.Num_Saved_Bits) && ((step = (c_int)Get_Bits._Get_Bits(s.Gb, 5)) == 31))
						quant += 31;

					quant_Step += ((quant + step) ^ sign) - sign;
				}

				if (quant_Step < 0)
					Log.Av_Log(s.AvCtx, Log.Av_Log_Debug, "negative quant step\n");

				// Decode quantization step modifiers for every channel
				if (s.Channels_For_Cur_Subframe == 1)
					s.Channel[s.Channel_Indexes_For_Cur_Subframe[0]].Quant_Step = quant_Step;
				else
				{
					c_int modifier_Len = (c_int)Get_Bits._Get_Bits(s.Gb, 3);

					for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
					{
						c_int c = s.Channel_Indexes_For_Cur_Subframe[i];
						s.Channel[c].Quant_Step = quant_Step;

						if (Get_Bits.Get_Bits1(s.Gb) != 0)
						{
							if (modifier_Len != 0)
								s.Channel[c].Quant_Step += (c_int)Get_Bits._Get_Bits(s.Gb, modifier_Len) + 1;
							else
								++s.Channel[c].Quant_Step;
						}
					}
				}

				// Decode scale factors
				if (Decode_Scale_Factors(s) < 0)
					return Error.InvalidData;
			}

			Log.FF_DLog(s.AvCtx, "BITSTREAM: subframe header length was %i\n", Get_Bits.Get_Bits_Count(s.Gb) - s.Subframe_Offset);

			// Parse coefficients
			for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
			{
				c_int c = s.Channel_Indexes_For_Cur_Subframe[i];

				if ((s.Channel[c].Transmit_Coefs != 0) && (Get_Bits.Get_Bits_Count(s.Gb) < s.Num_Saved_Bits))
					Decode_Coeffs(s, c);
				else
					CMemory.memset(s.Channel[c].Coeffs, 0.0f, (size_t)subframe_Len);
			}

			Log.FF_DLog(s.AvCtx, "BITSTREAM: subframe length was %i\n", Get_Bits.Get_Bits_Count(s.Gb) - s.Subframe_Offset);

			if (transmit_Coeefs != 0)
			{
				AvTxContext tx = s.Tx[IntMath.Av_Log2((c_uint)subframe_Len) - WmaConstants.WmaPro_Block_Min_Bits];
				UtilFunc.Av_Tx_Fn tx_Fn = s.Tx_Fn[IntMath.Av_Log2((c_uint)subframe_Len) - WmaConstants.WmaPro_Block_Min_Bits];

				// Reconstruct the per channel data
				Inverse_Channel_Transform(s);

				for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
				{
					c_int c = s.Channel_Indexes_For_Cur_Subframe[i];
					CPointer<c_int> sf = s.Channel[c].Scale_Factors;

					if (c == s.Lfe_Channel)
						CMemory.memset(s.Tmp + cur_Subwoofer_CutOff, 0, (size_t)(subframe_Len - cur_Subwoofer_CutOff));

					// Inverse quantization and rescaling
					for (c_int b = 0; b < s.Num_Bands; b++)
					{
						c_int end = Macros.FFMin(s.Cur_Sfb_Offsets[b + 1], s.Subframe_Len);
						c_int exp = s.Channel[c].Quant_Step - (s.Channel[c].Max_Scale_Factor - sf[0, 1]) * s.Channel[c].Scale_Factor_Step;
						c_float quant = (c_float)FFMath.FF_Exp10(exp / 20.0);
						c_int start = s.Cur_Sfb_Offsets[b];

						s.fDsp.Vector_FMul_Scalar(s.Tmp + start, s.Channel[c].Coeffs + start, quant, end - start);
					}

					// Apply imdct (imdct_half == DCTIV with reverse)
					tx_Fn(tx, s.Channel[c].Coeffs, s.Tmp, sizeof(float));
				}
			}

			// Window and overlapp-add
			WmaPro_Window(s);

			// Handled one subframe
			for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
			{
				c_int c = s.Channel_Indexes_For_Cur_Subframe[i];

				if (s.Channel[c].Cur_Subframe >= s.Channel[c].Num_Subframes)
				{
					Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "broken subframe\n");
					return Error.InvalidData;
				}

				++s.Channel[c].Cur_Subframe;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Decode one WMA frame
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Frame(WmaProDecodeCtx s, AvFrame frame, out c_int got_Frame_Ptr)
		{
			got_Frame_Ptr = 0;

			GetBitContext gb = s.Gb;
			c_int more_Frames = 0;
			c_int len = 0;

			// Get frame length
			if (s.Len_Prefix != 0)
				len = (c_int)Get_Bits._Get_Bits(gb, s.Log2_Frame_Size);

			Log.FF_DLog(s.AvCtx, "decoding frame with length %x\n", len);

			// Decode tile information
			if (Decode_TileHdr(s) != 0)
			{
				s.Packet_Loss = 1;
				return 0;
			}

			// Read postproc transform
			if ((s.Nb_Channels > 1) && (Get_Bits.Get_Bits1(gb) != 0))
			{
				if (Get_Bits.Get_Bits1(gb) != 0)
				{
					for (c_int i = 0; i < s.Nb_Channels * s.Nb_Channels; i++)
						Get_Bits.Skip_Bits(gb, 4);
				}
			}

			// Read drc info
			if (s.Dynamic_Range_Compression != 0)
			{
				s.Drc_Gain = (uint8_t)Get_Bits._Get_Bits(gb, 8);
				Log.FF_DLog(s.AvCtx, "drc_gain %i\n", s.Drc_Gain);
			}

			if (Get_Bits.Get_Bits1(gb) != 0)
			{
				if (Get_Bits.Get_Bits1(gb) != 0)
					s.Trim_Start = (uint16_t)Get_Bits._Get_Bits(gb, IntMath.Av_Log2(s.Samples_Per_Frame * 2U));

				if (Get_Bits.Get_Bits1(gb) != 0)
					s.Trim_End = (uint16_t)Get_Bits._Get_Bits(gb, IntMath.Av_Log2(s.Samples_Per_Frame * 2U));
			}
			else
				s.Trim_Start = s.Trim_End = 0;

			Log.FF_DLog(s.AvCtx, "BITSTREAM: frame header length was %i\n", Get_Bits.Get_Bits_Count(gb) - s.Frame_Offset);

			// Reset subframe states
			s.Parsed_All_Subframes = 0;

			for (c_int i = 0; i < s.Nb_Channels; i++)
			{
				s.Channel[i].Decoded_Samples = 0;
				s.Channel[i].Cur_Subframe = 0;
				s.Channel[i].Reuse_Sf = 0;
			}

			// Decode all subframes
			while (s.Parsed_All_Subframes == 0)
			{
				if (Decode_Subframe(s) < 0)
				{
					s.Packet_Loss = 1;
					return 0;
				}
			}

			// Copy samples to the output buffer
			for (c_int i = 0; i < s.Nb_Channels; i++)
				CMemory.memcpy(frame.Extended_Data[i].Cast<uint8_t, c_float>(), s.Channel[i].Out, s.Samples_Per_Frame);

			for (c_int i = 0; i < s.Nb_Channels; i++)
			{
				// Reuse second half of the IMDCT output for the next frame
				CMemory.memcpy(s.Channel[i].Out, s.Channel[i].Out + s.Samples_Per_Frame, (size_t)(s.Samples_Per_Frame >> 1));
			}

			if (s.Skip_Frame != 0)
			{
				s.Skip_Frame = 0;
				got_Frame_Ptr = 0;
				Frame.Av_Frame_Unref(frame);
			}
			else
				got_Frame_Ptr = 1;

			if (s.Len_Prefix != 0)
			{
				if (len != ((Get_Bits.Get_Bits_Count(gb) - s.Frame_Offset) + 2))
				{
					// FIXME: not sure if this is always an error
					Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "frame[%lu] would have to skip %i bits\n", s.Frame_Num, len - (Get_Bits.Get_Bits_Count(gb) - s.Frame_Offset) - 1);

					s.Packet_Loss = 1;
					return 0;
				}

				// Skip the rest of the frame data
				Get_Bits.Skip_Bits_Long(gb, len - (Get_Bits.Get_Bits_Count(gb) - s.Frame_Offset) - 1);
			}
			else
			{
				while ((Get_Bits.Get_Bits_Count(gb) < s.Num_Saved_Bits) && (Get_Bits.Get_Bits1(gb) == 0))
				{
				}
			}

			// Decode trailer bit
			more_Frames = (c_int)Get_Bits.Get_Bits1(gb);

			++s.Frame_Num;

			return more_Frames;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate remaining input buffer length
		/// </summary>
		/********************************************************************/
		private static c_int Remaining_Bits(WmaProDecodeCtx s, GetBitContext gb)
		{
			return s.Buf_Bit_Size - Get_Bits.Get_Bits_Count(gb);
		}



		/********************************************************************/
		/// <summary>
		/// Fill the bit reservoir with a (partial) frame
		/// </summary>
		/********************************************************************/
		private static void Save_Bits(WmaProDecodeCtx s, GetBitContext gb, c_int len, c_int append)
		{
			c_int bufLen;

			// When the frame data does not need to be concatenated, the input buffer
			// is reset and additional bits from the previous frame are copied
			// and skipped later so that a fast byte copy is possible
			if (append == 0)
			{
				s.Frame_Offset = Get_Bits.Get_Bits_Count(gb) & 7;
				s.Num_Saved_Bits = s.Frame_Offset;
				Put_Bits.Init_Put_Bits(s.Pb, s.Frame_Data, WmaConstants.Max_FrameSize);
				bufLen = (s.Num_Saved_Bits + len + 7) >> 3;
			}
			else
				bufLen = (Put_Bits.Put_Bits_Count(s.Pb) + len + 7) >> 3;

			if ((len <= 0) || (bufLen > WmaConstants.Max_FrameSize))
			{
				Log.AvPriv_Request_Sample(s.AvCtx, "Too small input buffer");
				s.Packet_Loss = 1;
				return;
			}

			s.Num_Saved_Bits += len;

			if (append == 0)
				BitStream.FF_Copy_Bits(s.Pb, gb.Buffer + (Get_Bits.Get_Bits_Count(gb) >> 3), s.Num_Saved_Bits);
			else
			{
				c_int align = 8 - (Get_Bits.Get_Bits_Count(gb) & 7);
				align = Macros.FFMin(align, len);
				Put_Bits._Put_Bits(s.Pb, align, Get_Bits._Get_Bits(gb, align));
				len -= align;
				BitStream.FF_Copy_Bits(s.Pb, gb.Buffer + (Get_Bits.Get_Bits_Count(gb) >> 3), len);
			}

			Get_Bits.Skip_Bits_Long(gb, len);

			{
				PutBitContext tmp = s.Pb.MakeDeepClone();
				Put_Bits.Flush_Put_Bits(tmp);
			}

			Get_Bits.Init_Get_Bits(s.Gb, s.Frame_Data, s.Num_Saved_Bits);
			Get_Bits.Skip_Bits(s.Gb, s.Frame_Offset);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Packet(AvCodecContext avCtx, WmaProDecodeCtx s, AvFrame frame, out c_int got_Frame_Ptr, AvPacket avPkt)
		{
			GetBitContext gb = s.Pgb;
			CPointer<uint8_t> buf = avPkt.Data;
			c_int buf_Size = avPkt.Size;
			c_int packet_Sequence_Number;
			c_int ret;

			got_Frame_Ptr = 0;

			if (buf_Size == 0)
			{
				// Must output remaining samples after stream end. WMAPRO 5.1 created
				// by XWMA encoder don't though (maybe only 1/2ch streams need it)
				s.Packet_Done = 0;

				if (s.Eof_Done != 0)
					return 0;

				// Clean output buffer and copy last IMDCT samples
				for (c_int i = 0; i < s.Nb_Channels; i++)
				{
					CMemory.memset(frame.Extended_Data[i].Cast<uint8_t, c_float>(), 0.0f, s.Samples_Per_Frame);

					CMemory.memcpy(frame.Extended_Data[i].Cast<uint8_t, c_float>(), s.Channel[i].Out, (size_t)(s.Samples_Per_Frame >> 1));
				}

				s.Eof_Done = 1;
				s.Packet_Done = 1;
				got_Frame_Ptr = 1;

				return 0;
			}
			else if ((s.Packet_Done != 0) || (s.Packet_Loss != 0))
			{
				s.Packet_Done = 0;

				// Sanity check for the buffer length
				if ((avCtx.Codec_Id == AvCodecId.WmaPro) && (buf_Size < avCtx.Block_Align))
				{
					Log.Av_Log(avCtx, Log.Av_Log_Error, "Input packet too small (%d < %d)\n", buf_Size, avCtx.Block_Align);

					s.Packet_Loss = 1;
					return Error.InvalidData;
				}

				if (avCtx.Codec_Id == AvCodecId.WmaPro)
				{
					s.Next_Packet_Start = buf_Size - avCtx.Block_Align;
					buf_Size = avCtx.Block_Align;
				}
				else
				{
					s.Next_Packet_Start = buf_Size - Macros.FFMin(buf_Size, avCtx.Block_Align);
					buf_Size = Macros.FFMin(buf_Size, avCtx.Block_Align);
				}

				s.Buf_Bit_Size = buf_Size << 3;

				// Parse packet header
				ret = Get_Bits.Init_Get_Bits8(gb, buf, buf_Size);

				if (ret < 0)
					return ret;

				if (avCtx.Codec_Id != AvCodecId.Xma2)
				{
					packet_Sequence_Number = (c_int)Get_Bits._Get_Bits(gb, 4);
					Get_Bits.Skip_Bits(gb, 2);
				}
				else
				{
					c_int num_Frames = (c_int)Get_Bits._Get_Bits(gb, 6);
					Log.FF_DLog(avCtx, "packet[%ld]: number of frames %d\n", avCtx.Frame_Num, num_Frames);

					packet_Sequence_Number = 0;
				}

				// Get number of bits that need to be added to the previous frame
				c_int num_Bits_Prev_Frame = (c_int)Get_Bits._Get_Bits(gb, s.Log2_Frame_Size);

				if (avCtx.Codec_Id != AvCodecId.WmaPro)
				{
					Get_Bits.Skip_Bits(gb, 3);
					s.Skip_Packets = (uint8_t)Get_Bits._Get_Bits(gb, 8);

					Log.FF_DLog(avCtx, "packet[%ld]: skip packets %d\n", avCtx.Frame_Num, s.Skip_Packets);
				}

				Log.FF_DLog(avCtx, "packet[%ld]: nbpf %x\n", avCtx.Frame_Num, num_Bits_Prev_Frame);

				// Check for packet loss
				if ((avCtx.Codec_Id == AvCodecId.WmaPro) && (s.Packet_Loss == 0) && (((s.Packet_Sequence_Number + 1) & 0xf) != packet_Sequence_Number))
				{
					s.Packet_Loss = 1;

					Log.Av_Log(avCtx, Log.Av_Log_Error, "Packet loss detected! seq %x vs %x\n", s.Packet_Sequence_Number, packet_Sequence_Number);
				}

				s.Packet_Sequence_Number = (uint8_t)packet_Sequence_Number;

				if (num_Bits_Prev_Frame > 0)
				{
					c_int remaining_Packet_Bits = s.Buf_Bit_Size - Get_Bits.Get_Bits_Count(gb);

					if (num_Bits_Prev_Frame >= remaining_Packet_Bits)
					{
						num_Bits_Prev_Frame = remaining_Packet_Bits;
						s.Packet_Done = 1;
					}

					// Append the previous frame data to the remaining data from the
					// previous packet to create a full frame
					Save_Bits(s, gb, num_Bits_Prev_Frame, 1);
					Log.FF_DLog(avCtx, "accumulated %x bits of frame data\n", s.Num_Saved_Bits - s.Frame_Offset);

					// Decode the cross packet frame if it is valid
					if (s.Packet_Loss == 0)
						Decode_Frame(s, frame, out got_Frame_Ptr);
				}
				else if ((s.Num_Saved_Bits - s.Frame_Offset) != 0)
					Log.FF_DLog(avCtx, "ignoring %x previously saved bits\n", s.Num_Saved_Bits - s.Frame_Offset);

				if (s.Packet_Loss != 0)
				{
					// Reset number of saved bits so that the decoder
					// does not start to decode incomplete frames in the
					// s->len_prefix == 0 case
					s.Num_Saved_Bits = 0;
					s.Packet_Loss = 0;
				}
			}
			else
			{
				c_int frame_Size;

				if (avPkt.Size < s.Next_Packet_Start)
				{
					s.Packet_Loss = 1;
					return Error.InvalidData;
				}

				s.Buf_Bit_Size = (avPkt.Size - s.Next_Packet_Start) << 3;

				ret = Get_Bits.Init_Get_Bits8(gb, avPkt.Data, avPkt.Size - s.Next_Packet_Start);

				if (ret < 0)
					return ret;

				Get_Bits.Skip_Bits(gb, s.Packet_Offset);

				if ((s.Len_Prefix != 0) && (Remaining_Bits(s, gb) > s.Log2_Frame_Size) && ((frame_Size = (c_int)Get_Bits.Show_Bits(gb, s.Log2_Frame_Size)) != 0) && (frame_Size <= Remaining_Bits(s, gb)))
				{
					Save_Bits(s, gb, frame_Size, 0);

					if (s.Packet_Loss == 0)
						s.Packet_Done = (uint8_t)(Decode_Frame(s, frame, out got_Frame_Ptr) == 0 ? 1 : 0);
				}
				else if ((s.Len_Prefix == 0) && (s.Num_Saved_Bits > Get_Bits.Get_Bits_Count(s.Gb)))
				{
					// When the frames do not have a length prefix, we don't know
					// the compressed length of the individual frames
					// however, we know what part of a new packet belongs to the
					// previous frame
					// therefore we save the incoming packet first, then we append
					// the "previous frame" data from the next packet so that
					// we get a buffer that only contains full frames
					s.Packet_Done = (uint8_t)(Decode_Frame(s, frame, out got_Frame_Ptr) == 0 ? 1 : 0);
				}
				else
					s.Packet_Done = 1;
			}

			if (Remaining_Bits(s, gb) < 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Overread %d\n", -Remaining_Bits(s, gb));
				s.Packet_Loss = 1;
			}

			if ((s.Packet_Done != 0) && (s.Packet_Loss == 0) && (Remaining_Bits(s, gb) > 0))
			{
				// Save the rest of the data so that it can be decoded
				// with the next packet
				Save_Bits(s, gb, Remaining_Bits(s, gb), 0);
			}

			s.Packet_Offset = (uint8_t)(Get_Bits.Get_Bits_Count(gb) & 7);

			if (s.Packet_Loss != 0)
				return Error.InvalidData;

			if ((s.Trim_Start != 0) && (avCtx.Codec_Id == AvCodecId.WmaPro))
			{
				if (s.Trim_Start < frame.Nb_Samples)
				{
					for (c_int ch = 0; ch < frame.Ch_Layout.Nb_Channels; ch++)
						frame.Extended_Data[ch] += s.Trim_Start * 4;

					frame.Nb_Samples -= s.Trim_Start;
				}
				else
					got_Frame_Ptr = 0;

				s.Trim_Start = 0;
			}

			if ((s.Trim_End != 0) && (avCtx.Codec_Id == AvCodecId.WmaPro))
			{
				if (s.Trim_End < frame.Nb_Samples)
					frame.Nb_Samples -= s.Trim_End;
				else
					got_Frame_Ptr = 0;

				s.Trim_End = 0;
			}

			return Get_Bits.Get_Bits_Count(gb) >> 3;
		}



		/********************************************************************/
		/// <summary>
		/// Decode a single WMA packet
		/// </summary>
		/********************************************************************/
		private static c_int WmaPro_Decode_Packet(AvCodecContext avCtx, AvFrame frame, out c_int got_Frame_Ptr, AvPacket avPkt)
		{
			got_Frame_Ptr = 0;

			WmaProDecodeCtx s = (WmaProDecodeCtx)avCtx.Priv_Data;

			// Get output buffer
			frame.Nb_Samples = s.Samples_Per_Frame;

			c_int ret = Decode.FF_Get_Buffer(avCtx, frame, 0);

			if (ret < 0)
			{
				s.Packet_Loss = 1;
				return 0;
			}

			return Decode_Packet(avCtx, s, frame, out got_Frame_Ptr, avPkt);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Flush(WmaProDecodeCtx s)
		{
			// Reset output buffer as a part of it is used during the windowing of a
			// new frame
			for (c_int i = 0; i < s.Nb_Channels; i++)
				CMemory.memset(s.Channel[i].Out, 0.0f, s.Samples_Per_Frame);

			s.Packet_Loss = 1;
			s.Skip_Packets = 0;
			s.Eof_Done = 0;
			s.Skip_Frame = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Clear decoder buffers (for seeking)
		/// </summary>
		/********************************************************************/
		private static void WmaPro_Flush(AvCodecContext avCtx)
		{
			WmaProDecodeCtx s = (WmaProDecodeCtx)avCtx.Priv_Data;

			Flush(s);
		}
		#endregion
	}
}
