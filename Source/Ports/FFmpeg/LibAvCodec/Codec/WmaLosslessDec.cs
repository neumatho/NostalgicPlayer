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
	/// 
	/// </summary>
	internal static class WmaLosslessDec
	{
		public static readonly FFCodec FF_WmaLossless_Decoder;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		static WmaLosslessDec()
		{
			FF_WmaLossless_Decoder = new FFCodec
			{
				Name = "wmalossless".ToCharPointer(),
				Long_Name = "Windows Media Audio Lossless".ToCharPointer(),
				Type = AvMediaType.Audio,
				Id = AvCodecId.WmaLossless,
				Priv_Data_Alloc = Alloc_Priv_Data,
				Init = Decode_Init,
				Close = Decode_Close,
				Is_Decoder = true,
				Cb_Type = FFCodecType.Decode,
				Flush = Flush,
				Capabilities = AvCodecCap.Dr1 | AvCodecCap.Delay,
				Sample_Fmts = new CPointer<AvSampleFormat>([ AvSampleFormat.S16P, AvSampleFormat.S32P ]),
				Caps_Internal = FFCodecCap.Init_Cleanup
			};

			FF_WmaLossless_Decoder.Cb.Decode = Decode_Packet;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IPrivateData Alloc_Priv_Data()
		{
			WmaLLDecodeCtx ctx = new WmaLLDecodeCtx();

			for (c_int i = 0; i < ctx.Cdlms.Length; i++)
			{
				for (c_int j = 0; j < ctx.Cdlms[i].Length; j++)
				{
					ctx.Cdlms[i][j].Coefs = new CPointer<int16_t>(WmaConstants.Max_Order + (WmaConstants.WmaLL_Coeff_Pad_Size / sizeof(int16_t)));
					ctx.Cdlms[i][j].Lms_PrevValues = new CPointer<int32_t>((WmaConstants.Max_Order * 2) + (WmaConstants.WmaLL_Coeff_Pad_Size / sizeof(int16_t)));
					ctx.Cdlms[i][j].Lms_Updates = new CPointer<int16_t>((WmaConstants.Max_Order * 2) + (WmaConstants.WmaLL_Coeff_Pad_Size / sizeof(int16_t)));
				}
			}

			return ctx;
		}



		/********************************************************************/
		/// <summary>
		/// Get sign of integer (1 for positive, -1 for negative and 0 for
		/// zero)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int WmaSign(c_int x)
		{
			return (x > 0 ? 1 : 0) - (x < 0 ? 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Init(AvCodecContext avCtx)
		{
			WmaLLDecodeCtx s = (WmaLLDecodeCtx)avCtx.Priv_Data;
			DataBufferContext eData_Ptr = (DataBufferContext)avCtx.ExtraData;
			c_uint channel_Mask;

			if ((avCtx.Block_Align <= 0) || (avCtx.Block_Align > (1 << 21)))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "block_align is not set or invalid\n");
				return Error.EINVAL;
			}

			if (eData_Ptr.Size >= 18)
			{
				s.Decode_Flags = IntReadWrite.Av_RL16(eData_Ptr.Data + 14);
				channel_Mask = IntReadWrite.Av_RL32(eData_Ptr.Data + 2);
				s.Bits_Per_Sample = (uint8_t)IntReadWrite.Av_RL16(eData_Ptr.Data);

				if (s.Bits_Per_Sample == 16)
					avCtx.Sample_Fmt = AvSampleFormat.S16P;
				else if (s.Bits_Per_Sample == 24)
				{
					avCtx.Sample_Fmt = AvSampleFormat.S32P;
					avCtx.Bits_Per_Raw_Sample = 24;
				}
				else
				{
					Log.Av_Log(avCtx, Log.Av_Log_Error, "Unknown bit-depth %u\n", s.Bits_Per_Sample);
					return Error.InvalidData;
				}
			}
			else
			{
				Log.AvPriv_Request_Sample(avCtx, "Unsupported extradata size");
				return Error.PatchWelcome;
			}

			if (channel_Mask != 0)
			{
				Channel_Layout.Av_Channel_Layout_Uninit(avCtx.Ch_Layout);
				Channel_Layout.Av_Channel_Layout_From_Mask(avCtx.Ch_Layout, (AvChannelMask)channel_Mask);
			}

			if (avCtx.Ch_Layout.Nb_Channels > WmaConstants.WmaLL_Max_Channels)
			{
				Log.AvPriv_Request_Sample(avCtx, $"More than {WmaConstants.WmaLL_Max_Channels} channels");
				return Error.PatchWelcome;
			}

			s.Num_Channels = (int8_t)avCtx.Ch_Layout.Nb_Channels;

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

			s.Max_Frame_Size = WmaConstants.Max_FrameSize * avCtx.Ch_Layout.Nb_Channels;
			s.Frame_Data = Mem.Av_MAllocz<uint8_t>((size_t)(s.Max_Frame_Size + Defs.Av_Input_Buffer_Padding_Size));
			if (s.Frame_Data.IsNull)
				return Error.ENOMEM;

			s.AvCtx = avCtx;

			Lossless_AudioDsp.FF_LLAudDsp_Init(s.Dsp);
			Put_Bits.Init_Put_Bits(s.Pb, s.Frame_Data, s.Max_Frame_Size);

			// Generic init
			s.Log2_Frame_Size = (uint16_t)(IntMath.Av_Log2((c_uint)avCtx.Block_Align) + 4);

			// Frame info
			s.Skip_Frame = 1;	// Skip first frame
			s.Packet_Loss = 1;
			s.Len_Prefix = (c_int)(s.Decode_Flags & 0x40);

			// Get frame len
			s.Samples_Per_Frame = (uint16_t)(1 << Wma_Common.FF_Wma_Get_Frame_Len_Bits(avCtx.Sample_Rate, 3, s.Decode_Flags));

			// Init previous block len
			for (c_int i = 0; i < avCtx.Ch_Layout.Nb_Channels; i++)
				s.Channel[i].Prev_Block_Len = (int16_t)s.Samples_Per_Frame;

			// Subframe info
			c_int log2_Max_Num_Subframes = (c_int)((s.Decode_Flags & 0x38) >> 3);
			s.Max_Num_Subframes = (uint8_t)(1 << log2_Max_Num_Subframes);
			s.Max_Subframe_Len_Bit = 0;
			s.Subframe_Len_Bits = (uint8_t)(IntMath.Av_Log2((c_uint)log2_Max_Num_Subframes) + 1);

			s.Min_Samples_Per_Subframe = (uint16_t)(s.Samples_Per_Frame / s.Max_Num_Subframes);
			s.Dynamic_Range_Compression = (c_int)(s.Decode_Flags & 0x80);
			s.BV3Rtm = (c_int)(s.Decode_Flags & 0x100);

			if (s.Max_Num_Subframes > WmaConstants.Max_Subframes)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "invalid number of subframes %u\n", s.Max_Num_Subframes);
				return Error.InvalidData;
			}

			s.Frame = Frame.Av_Frame_Alloc();
			if (s.Frame == null)
				return Error.ENOMEM;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Decode the subframe length
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Subframe_Length(WmaLLDecodeCtx s, c_int offset)
		{
			// No need to read from the bitstream when only one length is possible
			if (offset == (s.Samples_Per_Frame - s.Min_Samples_Per_Subframe))
				return s.Min_Samples_Per_Subframe;

			c_int len = IntMath.Av_Log2((c_uint)(s.Max_Num_Subframes - 1)) + 1;
			c_int frame_Len_Ratio = (c_int)Get_Bits._Get_Bits(s.Gb, len);
			c_int subframe_Len = s.Min_Samples_Per_Subframe * (frame_Len_Ratio + 1);

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
		private static c_int Decode_TileHdr(WmaLLDecodeCtx s)
		{
			uint16_t[] num_Samples = new uint16_t[WmaConstants.WmaLL_Max_Channels];			// Sum of samples for all currently known subframes of a channel
			uint8_t[] contains_Subframe = new uint8_t[WmaConstants.WmaLL_Max_Channels];		// Flag indicating if a channel contains the current subframe
			c_int channels_For_Cur_Subframe = s.Num_Channels;								// Number of channels that contain the current subframe
			c_int fixed_Channel_Layout = 0;													// Flag indicating that all channels use the same subframe offsets and sizes
			c_int min_Channel_Len = 0;														// Smallest sum of samples (channels with this length will be processed first)

			// Reset tiling information
			for (c_int c = 0; c < s.Num_Channels; c++)
				s.Channel[c].Num_Subframes = 0;

			c_int tile_Aligned = (c_int)Get_Bits.Get_Bits1(s.Gb);

			if ((s.Max_Num_Subframes == 1) || (tile_Aligned != 0))
				fixed_Channel_Layout = 1;

			// Loop until the frame data is split between the subframes
			do
			{
				c_int in_Use = 0;

				// Check which channels contain the subframe
				for (c_int c = 0; c < s.Num_Channels; c++)
				{
					if (num_Samples[c] == min_Channel_Len)
					{
						if ((fixed_Channel_Layout != 0) || (channels_For_Cur_Subframe == 1) || (min_Channel_Len == (s.Samples_Per_Frame - s.Min_Samples_Per_Subframe)))
							contains_Subframe[c] = 1;
						else
							contains_Subframe[c] = (uint8_t)Get_Bits.Get_Bits1(s.Gb);

						in_Use |= contains_Subframe[c];
					}
					else
						contains_Subframe[c] = 0;
				}

				if (in_Use == 0)
				{
					Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "Found empty subframe\n");
					return Error.InvalidData;
				}

				// Get subframe length, subframe_len == 0 is not allowed
				c_int subframe_Len = Decode_Subframe_Length(s, min_Channel_Len);

				if (subframe_Len <= 0)
					return Error.InvalidData;

				// Add subframes to the individual channels and find new min_channel_len
				min_Channel_Len += subframe_Len;

				for (c_int c = 0; c < s.Num_Channels; c++)
				{
					WmaLLChannelCtx chan = s.Channel[c];

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
							Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "broken frame: channel len(%d) > samples_per_frame(%d)\n", num_Samples[c], s.Samples_Per_Frame);
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

			for (c_int c = 0; c < s.Num_Channels; c++)
			{
				c_int offset = 0;

				for (c_int i = 0; i < s.Channel[c].Num_Subframes; i++)
				{
					s.Channel[c].Subframe_Offsets[i] = (uint16_t)offset;
					offset += s.Channel[c].Subframe_Len[i];
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Decode_Ac_Filter(WmaLLDecodeCtx s)
		{
			s.AcFilter_Order = (int8_t)(Get_Bits._Get_Bits(s.Gb, 4) + 1);
			s.AcFilter_Scaling = (int8_t)Get_Bits._Get_Bits(s.Gb, 4);

			for (c_int i = 0; i < s.AcFilter_Order; i++)
				s.AcFilter_Coeffs[i] = (int16_t)(Get_Bits.Get_Bitsz(s.Gb, s.AcFilter_Scaling) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Decode_Mclms(WmaLLDecodeCtx s)
		{
			s.Mclms_Order = (int8_t)((Get_Bits._Get_Bits(s.Gb, 4) + 1) * 2);
			s.Mclms_Scaling = (int8_t)Get_Bits._Get_Bits(s.Gb, 4);

			if (Get_Bits.Get_Bits1(s.Gb) != 0)
			{
				c_int cBits = IntMath.Av_Log2((c_uint)(s.Mclms_Scaling + 1));

				if ((1 << cBits) < (s.Mclms_Scaling + 1))
					cBits++;

				c_int send_Coef_Bits = Get_Bits.Get_Bitsz(s.Gb, cBits) + 2;

				for (c_int i = 0; i < (s.Mclms_Order * s.Num_Channels * s.Num_Channels); i++)
					s.Mclms_Coeffs[i] = (int16_t)Get_Bits._Get_Bits(s.Gb, send_Coef_Bits);

				for (c_int i = 0; i < s.Num_Channels; i++)
				{
					for (c_int c = 0; c < i; c++)
						s.Mclms_Coeffs_Cur[(i * s.Num_Channels) + c] = (int16_t)Get_Bits._Get_Bits(s.Gb, send_Coef_Bits);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Cdlms(WmaLLDecodeCtx s)
		{
			c_int cdlms_Send_Coef = (c_int)Get_Bits.Get_Bits1(s.Gb);

			for (c_int c = 0; c < s.Num_Channels; c++)
			{
				s.Cdlms_Ttl[c] = (c_int)(Get_Bits._Get_Bits(s.Gb, 3) + 1);

				for (c_int i = 0; i < s.Cdlms_Ttl[c]; i++)
				{
					s.Cdlms[c][i].Order = (c_int)((Get_Bits._Get_Bits(s.Gb, 7) + 1) * 8);

					if (s.Cdlms[c][i].Order > WmaConstants.Max_Order)
					{
						Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "Order[%d][%d] %d > max (%d), not supported\n", c, i, s.Cdlms[c][i].Order, WmaConstants.Max_Order);

						s.Cdlms[0][0].Order = 0;

						return Error.InvalidData;
					}

					if (((s.Cdlms[c][i].Order & 8) != 0) && (s.Bits_Per_Sample == 16))
					{
						if (warned == 0)
							Log.AvPriv_Request_Sample(s.AvCtx, "CDLMS of order %d", s.Cdlms[c][i].Order);

						warned = 1;
					}
				}

				for (c_int i = 0; i < s.Cdlms_Ttl[c]; i++)
					s.Cdlms[c][i].Scaling = (c_int)Get_Bits._Get_Bits(s.Gb, 4);

				if (cdlms_Send_Coef != 0)
				{
					for (c_int i = 0; i < s.Cdlms_Ttl[c]; i++)
					{
						c_int cBits = IntMath.Av_Log2((c_uint)s.Cdlms[c][i].Order);

						if ((1 << cBits) < s.Cdlms[c][i].Order)
							cBits++;

						s.Cdlms[c][i].CoefsEnd = (c_int)(Get_Bits._Get_Bits(s.Gb, cBits) + 1);

						cBits = IntMath.Av_Log2((c_uint)(s.Cdlms[c][i].Scaling + 1));

						if ((1 << cBits) < (s.Cdlms[c][i].Scaling + 1))
							cBits++;

						s.Cdlms[c][i].BitsEnd = Get_Bits.Get_Bitsz(s.Gb, cBits) + 2;
						c_int shift_L = 32 - s.Cdlms[c][i].BitsEnd;
						c_int shift_R = 32 - s.Cdlms[c][i].Scaling - 2;

						for (c_int j = 0; j < s.Cdlms[c][i].CoefsEnd; j++)
							s.Cdlms[c][i].Coefs[j] = (int16_t)((Get_Bits._Get_Bits(s.Gb, s.Cdlms[c][i].BitsEnd) << shift_L) >> shift_R);
					}
				}

				for (c_int i = 0; i < s.Cdlms_Ttl[c]; i++)
					CMemory.memset<int16_t>(s.Cdlms[c][i].Coefs + s.Cdlms[c][i].Order, 0, WmaConstants.WmaLL_Coeff_Pad_Size / sizeof(int16_t));
			}

			return 0;
		}
		private static c_int warned;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Channel_Residues(WmaLLDecodeCtx s, c_int ch, c_int tile_Size)
		{
			c_int i = 0;
			c_uint ave_Mean;

			s.Transient[ch] = (c_int)Get_Bits.Get_Bits1(s.Gb);

			if (s.Transient[ch] != 0)
			{
				s.Transient_Pos[ch] = (c_int)Get_Bits._Get_Bits(s.Gb, IntMath.Av_Log2((c_uint)tile_Size));

				if (s.Transient_Pos[ch] != 0)
					s.Transient[ch] = 0;

				s.Channel[ch].Transient_Counter = Macros.FFMax(s.Channel[ch].Transient_Counter, s.Samples_Per_Frame / 2);
			}
			else if (s.Channel[ch].Transient_Counter != 0)
				s.Transient[ch] = 1;

			if (s.Seekable_Tile != 0)
			{
				ave_Mean = Get_Bits._Get_Bits(s.Gb, s.Bits_Per_Sample);
				s.Ave_Sum[ch] = ave_Mean << (s.Movave_Scaling + 1);
			}

			if (s.Seekable_Tile != 0)
			{
				if (s.Do_Inter_Ch_Decorr != 0)
					s.Channel_Residues[ch][0] = Get_Bits.Get_SBits_Long(s.Gb, s.Bits_Per_Sample + 1);
				else
					s.Channel_Residues[ch][0] = Get_Bits.Get_SBits_Long(s.Gb, s.Bits_Per_Sample);

				i++;
			}

			for (; i < tile_Size; i++)
			{
				c_uint quo = 0, residue;

				while (Get_Bits.Get_Bits1(s.Gb) != 0)
				{
					quo++;

					if (Get_Bits.Get_Bits_Left(s.Gb) <= 0)
						return -1;
				}

				if (quo >= 32)
					quo += Get_Bits.Get_Bits_Long(s.Gb, (c_int)(Get_Bits._Get_Bits(s.Gb, 5) + 1));

				ave_Mean = (s.Ave_Sum[ch] + (1U << s.Movave_Scaling)) >> (s.Movave_Scaling + 1);

				if (ave_Mean <= 1)
					residue = quo;
				else
				{
					c_int rem_Bits = Common.Av_Ceil_Log2((c_int)ave_Mean);
					c_int rem = (c_int)Get_Bits.Get_Bits_Long(s.Gb, rem_Bits);
					residue = (c_uint)((quo << rem_Bits) + rem);
				}

				s.Ave_Sum[ch] = residue + s.Ave_Sum[ch] - (s.Ave_Sum[ch] >> s.Movave_Scaling);

				residue = (c_uint)((residue >> 1) ^ -(residue & 1));
				s.Channel_Residues[ch][i] = (c_int)residue;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Decode_Lpc(WmaLLDecodeCtx s)
		{
			s.Lpc_Order = (c_int)(Get_Bits._Get_Bits(s.Gb, 5) + 1);
			s.Lpc_Scaling = (c_int)Get_Bits._Get_Bits(s.Gb, 4);
			s.Lpc_IntBits = (c_int)(Get_Bits._Get_Bits(s.Gb, 3) + 1);

			c_int cBits = s.Lpc_Scaling + s.Lpc_IntBits;

			for (c_int ch = 0; ch < s.Num_Channels; ch++)
			{
				for (c_int i = 0; i < s.Lpc_Order; i++)
					s.Lpc_Coefs[ch][i] = Get_Bits.Get_SBits(s.Gb, cBits);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Clear_Codec_Buffers(WmaLLDecodeCtx s)
		{
			Array.Clear(s.AcFilter_Coeffs);

			foreach (c_int[] row in s.AcFilter_PrevValues)
				Array.Clear(row);

			foreach (c_int[] row in s.Lpc_Coefs)
				Array.Clear(row);

			Array.Clear(s.Mclms_Coeffs);
			Array.Clear(s.Mclms_Coeffs_Cur);
			s.Mclms_PrevValues.Clear();
			s.Mclms_Updates.Clear();

			for (c_int ich = 0; ich < s.Num_Channels; ich++)
			{
				for (c_int ilms = 0; ilms < s.Cdlms_Ttl[ich]; ilms++)
				{
					CMemory.memset<int16_t>(s.Cdlms[ich][ilms].Coefs, 0, (size_t)s.Cdlms[ich][ilms].Coefs.Length);
					CMemory.memset(s.Cdlms[ich][ilms].Lms_PrevValues, 0, (size_t)s.Cdlms[ich][ilms].Lms_PrevValues.Length);
					CMemory.memset<int16_t>(s.Cdlms[ich][ilms].Lms_Updates, 0, (size_t)s.Cdlms[ich][ilms].Lms_Updates.Length);
				}

				s.Ave_Sum[ich] = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Reset filter parameters and transient area at new seekable tile
		/// </summary>
		/********************************************************************/
		private static void Reset_Codec(WmaLLDecodeCtx s)
		{
			s.Mclms_Recent = s.Mclms_Order * s.Num_Channels;

			for (c_int ich = 0; ich < s.Num_Channels; ich++)
			{
				for (c_int ilms = 0; ilms < s.Cdlms_Ttl[ich]; ilms++)
					s.Cdlms[ich][ilms].Recent = s.Cdlms[ich][ilms].Order;

				// First sample of a seekable subframe is considered as the starting of
				// a transient area which is samples_per_frame samples long
				s.Channel[ich].Transient_Counter = s.Samples_Per_Frame;
				s.Transient[ich] = 1;
				s.Transient_Pos[ich] = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Mclms_Update(WmaLLDecodeCtx s, c_int icoef, CPointer<c_int> pred)
		{
			c_int order = s.Mclms_Order;
			c_int num_Channels = s.Num_Channels;
			c_int range = 1 << (s.Bits_Per_Sample - 1);

			for (c_int ich = 0; ich < num_Channels; ich++)
			{
				c_int pred_Error = s.Channel_Residues[ich][icoef] - pred[ich];

				if (pred_Error > 0)
				{
					for (c_int i = 0; i < (order * num_Channels); i++)
						s.Mclms_Coeffs[i + (ich * order * num_Channels)] += (int16_t)s.Mclms_Updates[s.Mclms_Recent + i];

					for (c_int j = 0; j < ich; j++)
						s.Mclms_Coeffs_Cur[(ich * num_Channels) + j] += (int16_t)WmaSign(s.Channel_Residues[j][icoef]);
				}
				else if (pred_Error < 0)
				{
					for (c_int i = 0; i < (order * num_Channels); i++)
						s.Mclms_Coeffs[i + (ich * order * num_Channels)] -= (int16_t)s.Mclms_Updates[s.Mclms_Recent + i];

					for (c_int j = 0; j < ich; j++)
						s.Mclms_Coeffs_Cur[(ich * num_Channels) + j] -= (int16_t)WmaSign(s.Channel_Residues[j][icoef]);
				}
			}

			for (c_int ich = num_Channels - 1; ich >= 0; ich--)
			{
				s.Mclms_Recent--;
				s.Mclms_PrevValues[s.Mclms_Recent] = Common.Av_Clip(s.Channel_Residues[ich][icoef], -range, range - 1);
				s.Mclms_Updates[s.Mclms_Recent] = WmaSign(s.Channel_Residues[ich][icoef]);
			}

			if (s.Mclms_Recent == 0)
			{
				CMemory.memcpy(s.Mclms_PrevValues + (order * num_Channels), s.Mclms_PrevValues, (size_t)(order * num_Channels));
				CMemory.memcpy(s.Mclms_Updates + (order * num_Channels), s.Mclms_Updates, (size_t)(order * num_Channels));

				s.Mclms_Recent = num_Channels * order;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Mclms_Predict(WmaLLDecodeCtx s, c_int icoef, CPointer<c_int> pred)
		{
			c_int order = s.Mclms_Order;
			c_int num_Channels = s.Num_Channels;

			for (c_int ich = 0; ich < num_Channels; ich++)
			{
				pred[ich] = 0;

				if (s.Is_Channel_Coded[ich] == 0)
					continue;

				for (c_int i = 0; i < (order * num_Channels); i++)
					pred[ich] += s.Mclms_PrevValues[i + s.Mclms_Recent] * s.Mclms_Coeffs[i + (order * num_Channels * ich)];

				for (c_int i = 0; i < ich; i++)
					pred[ich] += s.Channel_Residues[i][icoef] * s.Mclms_Coeffs_Cur[i + (num_Channels * ich)];

				pred[ich] += (1 << s.Mclms_Scaling) >> 1;
				pred[ich] >>= s.Mclms_Scaling;

				s.Channel_Residues[ich][icoef] += pred[ich];
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Revert_Mclms(WmaLLDecodeCtx s, c_int tile_Size)
		{
			CPointer<c_int> pred = new CPointer<c_int>(WmaConstants.WmaLL_Max_Channels);

			for (c_int icoef = 0; icoef < tile_Size; icoef++)
			{
				Mclms_Predict(s, icoef, pred);
				Mclms_Update(s, icoef, pred);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Use_High_Update_Speed(WmaLLDecodeCtx s, c_int ich)
		{
			for (c_int ilms = s.Cdlms_Ttl[ich] - 1; ilms >= 0; ilms--)
			{
				c_int recent = s.Cdlms[ich][ilms].Recent;

				if (s.Update_Speed[ich] == 16)
					continue;

				if (s.BV3Rtm != 0)
				{
					for (c_int icoef = 0; icoef < s.Cdlms[ich][ilms].Order; icoef++)
						s.Cdlms[ich][ilms].Lms_Updates[icoef + recent] *= 2;
				}
				else
				{
					for (c_int icoef = 0; icoef < s.Cdlms[ich][ilms].Order; icoef++)
						s.Cdlms[ich][ilms].Lms_Updates[icoef] *= 2;
				}
			}

			s.Update_Speed[ich] = 16;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Use_Normal_Update_Speed(WmaLLDecodeCtx s, c_int ich)
		{
			for (c_int ilms = s.Cdlms_Ttl[ich] - 1; ilms >= 0; ilms--)
			{
				c_int recent = s.Cdlms[ich][ilms].Recent;

				if (s.Update_Speed[ich] == 8)
					continue;

				if (s.BV3Rtm != 0)
				{
					for (c_int icoef = 0; icoef < s.Cdlms[ich][ilms].Order; icoef++)
						s.Cdlms[ich][ilms].Lms_Updates[icoef + recent] /= 2;
				}
				else
				{
					for (c_int icoef = 0; icoef < s.Cdlms[ich][ilms].Order; icoef++)
						s.Cdlms[ich][ilms].Lms_Updates[icoef] /= 2;
				}
			}

			s.Update_Speed[ich] = 8;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Lms_Update16(WmaLLDecodeCtx s, c_int ich, c_int ilms, c_int input)
		{
			c_int recent = s.Cdlms[ich][ilms].Recent;
			c_int range = 1 << (s.Bits_Per_Sample - 1);
			c_int order = s.Cdlms[ich][ilms].Order;
			CPointer<int16_t> prev = s.Cdlms[ich][ilms].Lms_PrevValues.Cast<c_int, int16_t>();

			if (recent != 0)
				recent--;
			else
			{
				CMemory.memcpy(prev + order, prev, (size_t)order);
				CMemory.memcpy(s.Cdlms[ich][ilms].Lms_Updates + order, s.Cdlms[ich][ilms].Lms_Updates, (size_t)order);

				recent = order - 1;
			}

			prev[recent] = (int16_t)Common.Av_Clip(input, -range, range - 1);
			s.Cdlms[ich][ilms].Lms_Updates[recent] = (int16_t)(WmaSign(input) * s.Update_Speed[ich]);

			s.Cdlms[ich][ilms].Lms_Updates[recent + (order >> 4)] >>= 2;
			s.Cdlms[ich][ilms].Lms_Updates[recent + (order >> 3)] >>= 1;
			s.Cdlms[ich][ilms].Recent = recent;

			CMemory.memset<int16_t>(s.Cdlms[ich][ilms].Lms_Updates + recent + order, 0, (size_t)(s.Cdlms[ich][ilms].Lms_Updates.Length - (recent + order)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Revert_Cdlms16(WmaLLDecodeCtx s, c_int ch, c_int coef_Begin, c_int coef_End)
		{
			c_int num_Lms = s.Cdlms_Ttl[ch];

			for (c_int ilms = num_Lms - 1; ilms >= 0; ilms--)
			{
				for (c_int icoef = coef_Begin; icoef < coef_End; icoef++)
				{
					CPointer<int16_t> prevValues = s.Cdlms[ch][ilms].Lms_PrevValues.Cast<int32_t, int16_t>();
					c_uint pred = (1U << s.Cdlms[ch][ilms].Scaling) >> 1;
					c_int residue = s.Channel_Residues[ch][icoef];
					pred += (c_uint)s.Dsp.ScalarProduct_And_MAdd_Int16(
						s.Cdlms[ch][ilms].Coefs,
						prevValues + s.Cdlms[ch][ilms].Recent,
						s.Cdlms[ch][ilms].Lms_Updates + s.Cdlms[ch][ilms].Recent,
						Macros.FFAlign(s.Cdlms[ch][ilms].Order, WmaConstants.WmaLL_Coeff_Pad_Size),
						WmaSign(residue));
					c_int input = (c_int)(residue + (c_uint)((c_int)pred >> s.Cdlms[ch][ilms].Scaling));
					Lms_Update16(s, ch, ilms, input);
					s.Channel_Residues[ch][icoef] = input;
				}
			}
		}






		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Lms_Update32(WmaLLDecodeCtx s, c_int ich, c_int ilms, c_int input)
		{
			c_int recent = s.Cdlms[ich][ilms].Recent;
			c_int range = 1 << (s.Bits_Per_Sample - 1);
			c_int order = s.Cdlms[ich][ilms].Order;
			CPointer<int32_t> prev = s.Cdlms[ich][ilms].Lms_PrevValues;

			if (recent != 0)
				recent--;
			else
			{
				CMemory.memcpy(prev + order, prev, (size_t)order);
				CMemory.memcpy(s.Cdlms[ich][ilms].Lms_Updates + order, s.Cdlms[ich][ilms].Lms_Updates, (size_t)order);

				recent = order - 1;
			}

			prev[recent] = Common.Av_Clip(input, -range, range - 1);
			s.Cdlms[ich][ilms].Lms_Updates[recent] = (int16_t)(WmaSign(input) * s.Update_Speed[ich]);

			s.Cdlms[ich][ilms].Lms_Updates[recent + (order >> 4)] >>= 2;
			s.Cdlms[ich][ilms].Lms_Updates[recent + (order >> 3)] >>= 1;
			s.Cdlms[ich][ilms].Recent = recent;

			CMemory.memset<int16_t>(s.Cdlms[ich][ilms].Lms_Updates + recent + order, 0, (size_t)(s.Cdlms[ich][ilms].Lms_Updates.Length - (recent + order)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Revert_Cdlms32(WmaLLDecodeCtx s, c_int ch, c_int coef_Begin, c_int coef_End)
		{
			c_int num_Lms = s.Cdlms_Ttl[ch];

			for (c_int ilms = num_Lms - 1; ilms >= 0; ilms--)
			{
				for (c_int icoef = coef_Begin; icoef < coef_End; icoef++)
				{
					CPointer<int32_t> prevValues = s.Cdlms[ch][ilms].Lms_PrevValues;
					c_uint pred = (1U << s.Cdlms[ch][ilms].Scaling) >> 1;
					c_int residue = s.Channel_Residues[ch][icoef];
					pred += (c_uint)s.Dsp.ScalarProduct_And_MAdd_Int32(
						s.Cdlms[ch][ilms].Coefs,
						prevValues + s.Cdlms[ch][ilms].Recent,
						s.Cdlms[ch][ilms].Lms_Updates + s.Cdlms[ch][ilms].Recent,
						Macros.FFAlign(s.Cdlms[ch][ilms].Order, 8),
						WmaSign(residue));
					c_int input = (c_int)(residue + (c_uint)((c_int)pred >> s.Cdlms[ch][ilms].Scaling));
					Lms_Update32(s, ch, ilms, input);
					s.Channel_Residues[ch][icoef] = input;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Revert_Inter_Ch_Decorr(WmaLLDecodeCtx s, c_int tile_Size)
		{
			if (s.Num_Channels != 2)
				return;
			else if ((s.Is_Channel_Coded[0] != 0) || (s.Is_Channel_Coded[1] != 0))
			{
				for (c_int icoef = 0; icoef < tile_Size; icoef++)
				{
					s.Channel_Residues[0][icoef] -= s.Channel_Residues[1][icoef] >> 1;
					s.Channel_Residues[1][icoef] += s.Channel_Residues[0][icoef];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Revert_AcFilter(WmaLLDecodeCtx s, c_int tile_Size)
		{
			c_int pred;
			CPointer<int16_t> filter_Coeffs = s.AcFilter_Coeffs;
			c_int scaling = s.AcFilter_Scaling;
			c_int order = s.AcFilter_Order;

			for (c_int ich = 0; ich < s.Num_Channels; ich++)
			{
				CPointer<c_int> prevValues = s.AcFilter_PrevValues[ich];

				for (c_int i = 0; i < order; i++)
				{
					pred = 0;

					for (c_int j = 0; j < order; j++)
					{
						if (i <= j)
							pred += filter_Coeffs[j] * prevValues[j - i];
						else
							pred += s.Channel_Residues[ich][i - j - 1] * filter_Coeffs[j];
					}

					pred >>= scaling;
					s.Channel_Residues[ich][i] += pred;
				}

				for (c_int i = order; i < tile_Size; i++)
				{
					pred = 0;

					for (c_int j = 0; j < order; j++)
						pred += s.Channel_Residues[ich][i - j - 1] * filter_Coeffs[j];

					pred >>= scaling;
					s.Channel_Residues[ich][i] += pred;
				}

				for (c_int j = order - 1; j >= 0; j--)
				{
					if (tile_Size <= j)
						prevValues[j] = prevValues[j - tile_Size];
					else
						prevValues[j] = s.Channel_Residues[ich][tile_Size - j - 1];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Subframe(WmaLLDecodeCtx s)
		{
			c_int offset = s.Samples_Per_Frame;
			c_int subframe_Len = s.Samples_Per_Frame;
			c_int total_Samples = s.Samples_Per_Frame * s.Num_Channels;
			c_int padding_Zeroes;

			s.Subframe_Offset = Get_Bits.Get_Bits_Count(s.Gb);

			// Reset channel context and find the next block offset and size
			// == the next block of the channel with the smallest number of
			// decoded samples
			for (c_int i = 0; i < s.Num_Channels; i++)
			{
				if (offset > s.Channel[i].Decoded_Samples)
				{
					offset = s.Channel[i].Decoded_Samples;
					subframe_Len = s.Channel[i].Subframe_Len[s.Channel[i].Cur_Subframe];
				}
			}

			// Get a list of all channels that contain the estimated block
			s.Channels_For_Cur_Subframe = 0;

			for (c_int i = 0; i < s.Num_Channels; i++)
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

			s.Seekable_Tile = (c_int)Get_Bits.Get_Bits1(s.Gb);

			if (s.Seekable_Tile != 0)
			{
				Clear_Codec_Buffers(s);

				s.Do_Arith_Coding = (uint8_t)Get_Bits.Get_Bits1(s.Gb);

				if (s.Do_Arith_Coding != 0)
				{
					Log.AvPriv_Request_Sample(s.AvCtx, "Arithmetic coding");
					return Error.PatchWelcome;
				}

				s.Do_Ac_Filter = (uint8_t)Get_Bits.Get_Bits1(s.Gb);
				s.Do_Inter_Ch_Decorr = (uint8_t)Get_Bits.Get_Bits1(s.Gb);
				s.Do_Mclms = (uint8_t)Get_Bits.Get_Bits1(s.Gb);

				if (s.Do_Ac_Filter != 0)
					Decode_Ac_Filter(s);

				if (s.Do_Mclms != 0)
					Decode_Mclms(s);

				c_int res = Decode_Cdlms(s);

				if (res < 0)
					return res;

				s.Movave_Scaling = (c_int)Get_Bits._Get_Bits(s.Gb, 3);
				s.Quant_StepSize = (c_int)(Get_Bits._Get_Bits(s.Gb, 8) + 1);

				Reset_Codec(s);
			}

			c_int rawPcm_Tile = (c_int)Get_Bits.Get_Bits1(s.Gb);

			if ((rawPcm_Tile == 0) && (s.Cdlms[0][0].Order == 0))
			{
				Log.Av_Log(s.AvCtx, Log.Av_Log_Debug, "Waiting for seekable tile\n");

				Frame.Av_Frame_Unref(s.Frame);

				return -1;
			}

			for (c_int i = 0; i < s.Num_Channels; i++)
				s.Is_Channel_Coded[i] = 1;

			if (rawPcm_Tile == 0)
			{
				for (c_int i = 0; i < s.Num_Channels; i++)
					s.Is_Channel_Coded[i] = (c_int)Get_Bits.Get_Bits1(s.Gb);

				if (s.BV3Rtm != 0)
				{
					// LPC
					s.Do_Lpc = (uint8_t)Get_Bits.Get_Bits1(s.Gb);

					if (s.Do_Lpc != 0)
					{
						Decode_Lpc(s);

						Log.AvPriv_Request_Sample(s.AvCtx, "Expect wrong output since inverse LPC filter");
					}
				}
				else
					s.Do_Lpc = 0;
			}

			if (Get_Bits.Get_Bits_Left(s.Gb) < 1)
				return Error.InvalidData;

			if (Get_Bits.Get_Bits1(s.Gb) != 0)
				padding_Zeroes = (c_int)Get_Bits._Get_Bits(s.Gb, 5);
			else
				padding_Zeroes = 0;

			if (rawPcm_Tile != 0)
			{
				c_int bits = s.Bits_Per_Sample - padding_Zeroes;

				if (bits <= 0)
				{
					Log.Av_Log(s.AvCtx, Log.Av_Log_Error, "Invalid number of padding bits in raw PCM tile\n");

					return Error.InvalidData;
				}

				Log.FF_DLog(s.AvCtx, "RAWPCM %d bits per sample. total %d bits, remain=%d\n", bits, bits * s.Num_Channels * subframe_Len, Get_Bits.Get_Bits_Count(s.Gb));

				for (c_int i = 0; i < s.Num_Channels; i++)
				{
					for (c_int j = 0; j < subframe_Len; j++)
						s.Channel_Residues[i][j] = Get_Bits.Get_SBits_Long(s.Gb, bits);
				}
			}
			else
			{
				if (s.Bits_Per_Sample < padding_Zeroes)
					return Error.InvalidData;

				for (c_int i = 0; i < s.Num_Channels; i++)
				{
					if (s.Is_Channel_Coded[i] != 0)
					{
						Decode_Channel_Residues(s, i, subframe_Len);

						if (s.Seekable_Tile != 0)
							Use_High_Update_Speed(s, i);
						else
							Use_Normal_Update_Speed(s, i);

						if (s.Bits_Per_Sample > 16)
							Revert_Cdlms32(s, i, 0, subframe_Len);
						else
							Revert_Cdlms16(s, i, 0, subframe_Len);
					}
					else
						CMemory.memset(s.Channel_Residues[i], 0, (size_t)subframe_Len);
				}

				if (s.Do_Mclms != 0)
					Revert_Mclms(s, subframe_Len);

				if (s.Do_Inter_Ch_Decorr != 0)
					Revert_Inter_Ch_Decorr(s, subframe_Len);

				if (s.Do_Ac_Filter != 0)
					Revert_AcFilter(s, subframe_Len);

				// Dequantize
				if (s.Quant_StepSize != 1)
				{
					for (c_int i = 0; i < s.Num_Channels; i++)
					{
						for (c_int j = 0; j < subframe_Len; j++)
							s.Channel_Residues[i][j] *= s.Quant_StepSize;
					}
				}
			}

			// Write to proper output buffer depending on bit-depth
			for (c_int i = 0; i < s.Channels_For_Cur_Subframe; i++)
			{
				c_int c = s.Channel_Indexes_For_Cur_Subframe[i];
				c_int subframe_Len_ = s.Channel[c].Subframe_Len[s.Channel[c].Cur_Subframe];

				for (c_int j = 0; j < subframe_Len_; j++)
				{
					if (s.Bits_Per_Sample == 16)
						s.Samples_16[c][0, 1] = (int16_t)(s.Channel_Residues[c][j] * (1 << padding_Zeroes));
					else
						s.Samples_32[c][0, 1] = s.Channel_Residues[c][j] * (256 << padding_Zeroes);
				}
			}

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
		private static c_int Decode_Frame(WmaLLDecodeCtx s)
		{
			GetBitContext gb = s.Gb;
			c_int more_Frames = 0;
			c_int len = 0;

			s.Frame.Nb_Samples = s.Samples_Per_Frame;

			c_int ret = Decode.FF_Get_Buffer(s.AvCtx, s.Frame, 0);

			if (ret < 0)
			{
				// Return an error if no frame could be decoded at all
				s.Packet_Loss = 1;
				s.Frame.Nb_Samples = 0;

				return ret;
			}

			for (c_int i = 0; i < s.Num_Channels; i++)
			{
				s.Samples_16[i] = s.Frame.Extended_Data[i].Cast<uint8_t, int16_t>();
				s.Samples_32[i] = s.Frame.Extended_Data[i].Cast<uint8_t, int32_t>();
			}

			// Get frame length
			if (s.Len_Prefix != 0)
				len = (c_int)Get_Bits._Get_Bits(gb, s.Log2_Frame_Size);

			// Decode tile information
			ret = Decode_TileHdr(s);

			if (ret != 0)
			{
				s.Packet_Loss = 1;
				Frame.Av_Frame_Unref(s.Frame);

				return ret;
			}

			// Read drc info
			if (s.Dynamic_Range_Compression != 0)
				s.Drc_Gain = (uint8_t)Get_Bits._Get_Bits(gb, 8);

			// No idea what these are for, might be the number of samples
			// that need to be skipped at the beginning or end of a stream
			if (Get_Bits.Get_Bits1(gb) != 0)
			{
				c_int skip;

				// Usually true for the first frame
				if (Get_Bits.Get_Bits1(gb) != 0)
				{
					skip = (c_int)Get_Bits._Get_Bits(gb, IntMath.Av_Log2(s.Samples_Per_Frame * 2U));
					Log.FF_DLog(s.AvCtx, "start skip: %i\n", skip);
				}

				// Sometimes true for the last frame
				if (Get_Bits.Get_Bits1(gb) != 0)
				{
					skip = (c_int)Get_Bits._Get_Bits(gb, IntMath.Av_Log2(s.Samples_Per_Frame * 2U));
					Log.FF_DLog(s.AvCtx, "end skip: %i\n", skip);

					s.Frame.Nb_Samples -= skip;

					if (s.Frame.Nb_Samples <= 0)
						return Error.InvalidData;
				}
			}

			// Reset subframe states
			s.Parsed_All_Subframes = 0;

			for (c_int i = 0; i < s.Num_Channels; i++)
			{
				s.Channel[i].Decoded_Samples = 0;
				s.Channel[i].Cur_Subframe = 0;
			}

			// Decode all subframes
			while (s.Parsed_All_Subframes == 0)
			{
				c_int decoded_Samples = s.Channel[0].Decoded_Samples;

				if (Decode_Subframe(s) < 0)
				{
					s.Packet_Loss = 1;

					if (s.Frame.Nb_Samples != 0)
						s.Frame.Nb_Samples = decoded_Samples;

					return 0;
				}
			}

			Log.FF_DLog(s.AvCtx, "Frame done\n");

			s.Skip_Frame = 0;

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
		private static c_int Remaining_Bits(WmaLLDecodeCtx s, GetBitContext gb)
		{
			return s.Buf_Bit_Size - Get_Bits.Get_Bits_Count(gb);
		}



		/********************************************************************/
		/// <summary>
		/// Fill the bit reservoir with a (partial) frame
		/// </summary>
		/********************************************************************/
		private static void Save_Bits(WmaLLDecodeCtx s, GetBitContext gb, c_int len, c_int append)
		{
			// When the frame data does not need to be concatenated, the input buffer
			// is reset and additional bits from the previous frame are copied
			// and skipped later so that a fast byte copy is possible
			if (append == 0)
			{
				s.Frame_Offset = Get_Bits.Get_Bits_Count(gb) & 7;
				s.Num_Saved_Bits = s.Frame_Offset;
				Put_Bits.Init_Put_Bits(s.Pb, s.Frame_Data, s.Max_Frame_Size);
			}

			c_int bufLen = (s.Num_Saved_Bits + len + 8) >> 3;

			if ((len <= 0) || (bufLen > s.Max_Frame_Size))
			{
				Log.AvPriv_Request_Sample(s.AvCtx, "Too small input buffer");
				s.Packet_Loss = 1;
				s.Num_Saved_Bits = 0;
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

			PutBitContext tmp = s.Pb.MakeDeepClone();
			Put_Bits.Flush_Put_Bits(tmp);

			Get_Bits.Init_Get_Bits(s.Gb, s.Frame_Data, s.Num_Saved_Bits);
			Get_Bits.Skip_Bits(s.Gb, s.Frame_Offset);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Packet(AvCodecContext avCtx, AvFrame rFrame, out c_int got_Frame_Ptr, AvPacket avPkt)
		{
			got_Frame_Ptr = 0;

			WmaLLDecodeCtx s = (WmaLLDecodeCtx)avCtx.Priv_Data;
			GetBitContext gb = s.Pgb;
			CPointer<uint8_t> buf = avPkt.Data;
			c_int buf_Size = avPkt.Size;

			s.Frame.Nb_Samples = 0;

			if (buf_Size == 0)
			{
				s.Packet_Done = 0;

				if (s.Num_Saved_Bits <= Get_Bits.Get_Bits_Count(s.Gb))
					return 0;

				if (Decode_Frame(s) == 0)
					s.Num_Saved_Bits = 0;
			}
			else if ((s.Packet_Done != 0) || (s.Packet_Loss != 0))
			{
				s.Packet_Done = 0;
				s.Next_Packet_Start = buf_Size - Macros.FFMin(avCtx.Block_Align, buf_Size);
				buf_Size = Macros.FFMin(avCtx.Block_Align, buf_Size);
				s.Buf_Bit_Size = buf_Size << 3;

				// Parse packet header
				Get_Bits.Init_Get_Bits(gb, buf, s.Buf_Bit_Size);
				c_int packet_Sequence_Number = (c_int)Get_Bits._Get_Bits(gb, 4);
				Get_Bits.Skip_Bits(gb, 1);		// Skip seekable_frame_in_packet, current unused
				c_int spliced_Packet = (c_int)Get_Bits.Get_Bits1(gb);

				if (spliced_Packet != 0)
					Log.AvPriv_Request_Sample(avCtx, "Bitstream splicing");

				// Get number of bits that need to be added to the previous frame
				c_int num_Bits_Prev_Frame = (c_int)Get_Bits._Get_Bits(gb, s.Log2_Frame_Size);

				// Check for packet loss
				if ((s.Packet_Loss == 0) && (((s.Packet_Sequence_Number + 1) & 0xf) != packet_Sequence_Number))
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

					// Decode the cross packet frame if it is valid
					if ((num_Bits_Prev_Frame < remaining_Packet_Bits) && (s.Packet_Loss == 0))
						Decode_Frame(s);
				}
				else if ((s.Num_Saved_Bits - s.Frame_Offset) != 0)
					Log.FF_DLog(avCtx, "ignoring %x previously saved bits\n", s.Num_Saved_Bits - s.Frame_Offset);

				if (s.Packet_Loss != 0)
				{
					// Reset number of saved bits so that the decoder does not start
					// to decode incomplete frames in the s->len_prefix == 0 case
					s.Num_Saved_Bits = 0;
					s.Packet_Loss = 0;
					Put_Bits.Init_Put_Bits(s.Pb, s.Frame_Data, s.Max_Frame_Size);
				}
			}
			else
			{
				c_int frame_Size;

				s.Buf_Bit_Size = (avPkt.Size - s.Next_Packet_Start) << 3;
				Get_Bits.Init_Get_Bits(gb, avPkt.Data, s.Buf_Bit_Size);
				Get_Bits.Skip_Bits(gb, s.Packet_Offset);

				if ((s.Len_Prefix != 0) && (Remaining_Bits(s, gb) > s.Log2_Frame_Size) && ((frame_Size = (c_int)Get_Bits.Show_Bits(gb, s.Log2_Frame_Size)) != 0) && (frame_Size <= Remaining_Bits(s, gb)))
				{
					Save_Bits(s, gb, frame_Size, 0);

					if (s.Packet_Loss == 0)
						s.Packet_Done = (uint8_t)(Decode_Frame(s) == 0 ? 1 : 0);
				}
				else if ((s.Len_Prefix == 0) && (s.Num_Saved_Bits > Get_Bits.Get_Bits_Count(s.Gb)))
				{
					// When the frames do not have a length prefix, we don't know the
					// compressed length of the individual frames however, we know what
					// part of a new packet belongs to the previous frame therefore we
					// save the incoming packet first, then we append the "previous
					// frame" data from the next packet so that we get a buffer that
					// only contains full frames
					s.Packet_Done = (uint8_t)(Decode_Frame(s) == 0 ? 1 : 0);
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

			got_Frame_Ptr = s.Frame.Nb_Samples > 0 ? 1 : 0;
			Frame.Av_Frame_Move_Ref(rFrame, s.Frame);

			s.Packet_Offset = (uint8_t)(Get_Bits.Get_Bits_Count(gb) & 7);

			return s.Packet_Loss != 0 ? Error.InvalidData : Get_Bits.Get_Bits_Count(gb) >> 3;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Flush(AvCodecContext avCtx)
		{
			WmaLLDecodeCtx s = (WmaLLDecodeCtx)avCtx.Priv_Data;

			s.Packet_Loss = 1;
			s.Packet_Done = 0;
			s.Num_Saved_Bits = 0;
			s.Frame_Offset = 0;
			s.Next_Packet_Start = 0;
			s.Cdlms[0][0].Order = 0;
			s.Frame.Nb_Samples = 0;

			Put_Bits.Init_Put_Bits(s.Pb, s.Frame_Data, s.Max_Frame_Size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Close(AvCodecContext avCtx)
		{
			WmaLLDecodeCtx s = (WmaLLDecodeCtx)avCtx.Priv_Data;

			Frame.Av_Frame_Free(ref s.Frame);
			Mem.Av_FreeP(ref s.Frame_Data);

			return 0;
		}
		#endregion
	}
}
