/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Opus_Decoder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_Decoder_Init(OpusDecoderInternal st, opus_int32 Fs, c_int channels)
		{
			if (((Fs != 48000) && (Fs != 24000) && (Fs != 16000) && (Fs != 12000) && (Fs != 8000)) || ((channels != 1) && (channels != 2)))
				return OpusError.Bad_Arg;

			st.Clear();

			// Initialize SILK decoder
			st.silk_dec = new Silk_Decoder();
			st.stream_channels = st.channels = channels;
			st.complexity = 0;

			st.Fs = Fs;
			st.DecControl.API_sampleRate = st.Fs;
			st.DecControl.nChannelsAPI = st.channels;

			// Reset decoder
			SilkError ret = Dec_Api.Silk_InitDecoder(st.silk_dec);
			if (ret != SilkError.No_Error)
				return OpusError.Internal_Error;

			// Initialize CELT decoder
			st.celt_dec = new CeltDecoder(channels);

			OpusError ret1 = Celt_Decoder.Celt_Decoder_Init(st.celt_dec, Fs, channels);
			if (ret1 != OpusError.Ok)
				return OpusError.Internal_Error;

			Celt_Decoder.Celt_Decoder_Ctl_Set(st.celt_dec, OpusControlSetRequest.Celt_Set_Signalling, 0);

			st.prev_mode = 0;
			st.frame_size = Fs / 400;
			st.arch = Cpu_Support.Opus_Select_Arch();

			return OpusError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusDecoderInternal Opus_Decoder_Create(opus_int32 Fs, c_int channels, out OpusError error)
		{
			if (((Fs != 48000) && (Fs != 24000) && (Fs != 16000) && (Fs != 12000) && (Fs != 8000)) || ((channels != 1) && (channels != 2)))
			{
				error = OpusError.Bad_Arg;
				return null;
			}

			OpusDecoderInternal st = Memory.Opus_Alloc<OpusDecoderInternal>();
			if (st == null)
			{
				error = OpusError.Alloc_Fail;
				return null;
			}

			error = Opus_Decoder_Init(st, Fs, channels);
			if (error != OpusError.Ok)
			{
				Memory.Opus_Free(st);
				st = null;
			}

			return st;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Smooth_Fade(CPointer<opus_res> in1, CPointer<opus_res> in2, CPointer<opus_res> _out, c_int overlap, c_int channels, CPointer<celt_coef> window, opus_int32 Fs)
		{
			c_int inc = 48000 / Fs;

			for (c_int c = 0; c < channels; c++)
			{
				for (c_int i = 0; i < overlap; i++)
				{
					opus_val16 w = Arch.COEF2VAL16(window[i * inc]);
					w = Arch.MULT16_16_Q15(w, w);
					_out[(i * channels) + c] = Arch.SHR32(Arch.MAC16_16(Arch.MULT16_16(w, in2[(i * channels) + c]), Constants.Q15One - w, in1[(i * channels) + c]), 15);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static PacketMode Opus_Packet_Get_Mode(CPointer<byte> data)
		{
			PacketMode mode;

			if ((data[0] & 0x80) != 0)
				mode = PacketMode.Celt_Only;
			else if ((data[0] & 0x60) == 0x60)
				mode = PacketMode.Hybrid;
			else
				mode = PacketMode.Silk_Only;

			return mode;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opus_Decode_Frame(OpusDecoderInternal st, CPointer<byte> data, opus_int32 len, CPointer<opus_res> pcm, c_int frame_size, bool decode_fec)
		{
			SilkError silk_ret = SilkError.No_Error;
			c_int celt_ret = 0;
			Ec_Dec dec = new Ec_Dec();
			CPointer<opus_res> pcm_transition = null;

			c_int audiosize;
			PacketMode mode;
			Bandwidth bandwidth;
			bool transition = false;
			bool redundancy = false;
			c_int redundancy_bytes = 0;
			bool celt_to_silk = false;
			opus_uint32 redundant_rng = 0;

			Silk_Decoder silk_dec = st.silk_dec;
			CeltDecoder celt_dec = st.celt_dec;
			c_int F20 = st.Fs / 50;
			c_int F10 = F20 >> 1;
			c_int F5 = F10 >> 1;
			c_int F2_5 = F5 >> 1;

			if (frame_size < F2_5)
				return (c_int)OpusError.Buffer_Too_Small;

			// Limit frameSize to avoid excessive stack allocations
			frame_size = Arch.IMIN(frame_size, st.Fs / 25 * 3);

			// Payloads of 1 (2 including ToC) or 0 trigger the PLC/DTX
			if (len <= 1)
			{
				data.SetToNull();

				// In that case, don't conceal more than what the ToC says
				frame_size = Arch.IMIN(frame_size, st.frame_size);
			}

			if (data.IsNotNull)
			{
				audiosize = st.frame_size;
				mode = st.mode;
				bandwidth = st.bandwidth;

				EntDec.Ec_Dec_Init(out dec, data, (opus_uint32)len);
			}
			else
			{
				audiosize = frame_size;

				// Run PLC using last used mode (CELT if we ended with CELT redundancy)
				mode = st.prev_redundancy ? PacketMode.Celt_Only : st.prev_mode;
				bandwidth = Bandwidth.None;

				if (mode == PacketMode.None)
				{
					// If we haven't got any packet yet, all we can do is return zeros
					for (c_int i = 0; i < (audiosize * st.channels); i++)
						pcm[i] = 0;

					return audiosize;
				}

				// Avoids trying to run the PLC on sizes other than 2.5 (CELT), 5 (CELT), 10, or 20 (e.g. 12.5 or 30 ms)
				if (audiosize > F20)
				{
					do
					{
						c_int ret = Opus_Decode_Frame(st, null, 0, pcm, Arch.IMIN(audiosize, F20), false);
						if (ret < 0)
							return ret;

						pcm += ret * st.channels;
						audiosize -= ret;
					}
					while (audiosize > 0);

					return frame_size;
				}
				else if (audiosize < F20)
				{
					if (audiosize > F10)
						audiosize = F10;
					else if ((mode != PacketMode.Silk_Only) && (audiosize > F5) && (audiosize < F10))
						audiosize = F5;
				}
			}

			bool celt_accum = mode != PacketMode.Celt_Only;

			c_int pcm_transition_silk_size = Constants.Alloc_None;
			c_int pcm_transition_celt_size = Constants.Alloc_None;

			if (data.IsNotNull && (st.prev_mode != PacketMode.None) && (((mode == PacketMode.Celt_Only) && (st.prev_mode != PacketMode.Celt_Only) && !st.prev_redundancy) || ((mode != PacketMode.Celt_Only) && (st.prev_mode == PacketMode.Celt_Only))))
			{
				transition = true;

				// Decide where to allocate the stack memory for pcmTransition
				if (mode == PacketMode.Celt_Only)
					pcm_transition_celt_size = F5 * st.channels;
				else
					pcm_transition_silk_size = F5 * st.channels;
			}

			opus_res[] pcm_transition_celt = new opus_res[pcm_transition_celt_size];

			if (transition && (mode == PacketMode.Celt_Only))
			{
				pcm_transition = pcm_transition_celt;
				Opus_Decode_Frame(st, null, 0, pcm_transition, Arch.IMIN(F5, audiosize), false);
			}

			if (audiosize > frame_size)
				return (c_int)OpusError.Bad_Arg;
			else
				frame_size = audiosize;

			// SILK processing
			if (mode != PacketMode.Celt_Only)
			{
				CPointer<opus_res> pcm_ptr;
				c_int pcm_silk_size = Constants.Alloc_None;
				bool pcm_too_small = frame_size < F10;

				if (pcm_too_small)
					pcm_silk_size = F10 * st.channels;

				opus_res[] pcm_silk = new opus_res[pcm_silk_size];

				if (pcm_too_small)
					pcm_ptr = pcm_silk;
				else
					pcm_ptr = pcm;

				if (st.prev_mode == PacketMode.Celt_Only)
					Dec_Api.Silk_ResetDecoder(silk_dec);

				// The SILK PLC cannot produce frames of less than 10 ms
				st.DecControl.payloadSize_ms = Arch.IMAX(10, 1000 * audiosize / st.Fs);

				if (data.IsNotNull)
				{
					st.DecControl.nChannelsInternal = st.stream_channels;

					if (mode == PacketMode.Silk_Only)
					{
						if (bandwidth == Bandwidth.Narrowband)
							st.DecControl.internalSampleRate = 8000;
						else if (bandwidth == Bandwidth.Mediumband)
							st.DecControl.internalSampleRate = 12000;
						else if (bandwidth == Bandwidth.Wideband)
							st.DecControl.internalSampleRate = 16000;
						else
							st.DecControl.internalSampleRate = 16000;
					}
					else
					{
						// Hybrid mode
						st.DecControl.internalSampleRate = 16000;
					}
				}

				st.DecControl.enable_deep_plc = st.complexity >= 5;

				LostFlag lost_flag = data.IsNull ? LostFlag.Packet_Lost : (LostFlag)(2 * (decode_fec ? 1 : 0));
				c_int decoded_samples = 0;

				do
				{
					// Call SILK decoder
					bool first_frame = decoded_samples == 0;

					silk_ret = Dec_Api.Silk_Decode(silk_dec, st.DecControl, lost_flag, first_frame, dec, pcm_ptr, out opus_int32 silk_frame_size, st.arch);
					if (silk_ret != SilkError.No_Error)
					{
						if (lost_flag != 0)
						{
							// PLC failure should not be fatal
							silk_frame_size = frame_size;

							for (c_int i = 0; i < (frame_size * st.channels); i++)
								pcm_ptr[i] = 0;
						}
						else
							return (c_int)OpusError.Internal_Error;
					}

					pcm_ptr += silk_frame_size * st.channels;
					decoded_samples += silk_frame_size;
				}
				while (decoded_samples < frame_size);

				if (pcm_too_small)
					Memory.Opus_Copy(pcm, pcm_silk, frame_size * st.channels);
			}

			c_int start_band = 0;

			if (!decode_fec && (mode != PacketMode.Celt_Only) && data.IsNotNull && ((EntCode.Ec_Tell(dec) + 17 + (20 * (mode == PacketMode.Hybrid ? 1 : 0))) <= (8 * len)))
			{
				// Check if we have a redundant 0-8 kHz band
				if (mode == PacketMode.Hybrid)
					redundancy = EntDec.Ec_Dec_Bit_Logp(dec, 12);
				else
					redundancy = true;

				if (redundancy)
				{
					celt_to_silk = EntDec.Ec_Dec_Bit_Logp(dec, 1);

					// redundacy_bytes will be at least two, in the non-hybrid
					// case due to the Ec_Tell() check above
					redundancy_bytes = mode == PacketMode.Hybrid ? (opus_int32)EntDec.Ec_Dec_UInt(dec, 256) + 2 : len - ((EntCode.Ec_Tell(dec) + 7) >> 3);
					len -= redundancy_bytes;

					// This is a sanity check. It should never happen for a valid
					// packet, so the exact behaviour is not normative
					if ((len * 8) < EntCode.Ec_Tell(dec))
					{
						len = 0;
						redundancy_bytes = 0;
						redundancy = false;
					}

					// Shrink decoder because of raw bits
					dec.storage -= (opus_uint32)redundancy_bytes;
				}
			}

			if (mode != PacketMode.Celt_Only)
				start_band = 17;

			if (redundancy)
			{
				transition = false;
				pcm_transition_silk_size = Constants.Alloc_None;
			}

			opus_res[] pcm_transition_silk = new opus_res[pcm_transition_silk_size];

			if (transition && (mode != PacketMode.Celt_Only))
			{
				pcm_transition = pcm_transition_silk;
				Opus_Decode_Frame(st, null, 0, pcm_transition, Arch.IMIN(F5, audiosize), false);
			}

			if (bandwidth != Bandwidth.None)
			{
				c_int endband = 21;

				switch (bandwidth)
				{
					case Bandwidth.Narrowband:
					{
						endband = 13;
						break;
					}

					case Bandwidth.Mediumband:
					case Bandwidth.Wideband:
					{
						endband = 17;
						break;
					}

					case Bandwidth.Superwideband:
					{
						endband = 19;
						break;
					}

					case Bandwidth.Fullband:
					{
						endband = 21;
						break;
					}
				}

				if (Celt_Decoder.Celt_Decoder_Ctl_Set(celt_dec, OpusControlSetRequest.Celt_Set_End_Band, endband) != OpusError.Ok)
					return (c_int)OpusError.Internal_Error;
			}

			if (Celt_Decoder.Celt_Decoder_Ctl_Set(celt_dec, OpusControlSetRequest.Celt_Set_Channels, st.stream_channels) != OpusError.Ok)
				return (c_int)OpusError.Internal_Error;

			// Only allocation memory for redundancy if/when needed
			c_int redundant_audio_size = redundancy ? F5 * st.channels : Constants.Alloc_None;
			CPointer<opus_res> redundant_audio = new opus_res[redundant_audio_size];

			// 5 ms redundant frame for CELT->SILK
			if (redundancy && celt_to_silk)
			{
				// If the previous frame did not use CELT (the first redundancy frame in
				// a transition from SILK may have been lost) then the CELT decoder is
				// stale at this point and the redundancy audio is not useful, however
				// the final range is still needed (for testing), so the redundancy is
				// always decoded but the decoded audio may not be used
				if (Celt_Decoder.Celt_Decoder_Ctl_Set(celt_dec, OpusControlSetRequest.Celt_Set_Start_Band, 0) != OpusError.Ok)
					return (c_int)OpusError.Internal_Error;

				Celt_Decoder.Celt_Decode_With_Ec(celt_dec, data + len, redundancy_bytes, redundant_audio, F5, null, false);

				if (Celt_Decoder.Celt_Decoder_Ctl_Get(celt_dec, OpusControlGetRequest.Opus_Get_Final_Range, out redundant_rng) != OpusError.Ok)
					return (c_int)OpusError.Internal_Error;
			}

			// MUST be after PLC
			if (Celt_Decoder.Celt_Decoder_Ctl_Set(celt_dec, OpusControlSetRequest.Celt_Set_Start_Band, start_band) != OpusError.Ok)
				return (c_int)OpusError.Internal_Error;

			if (mode != PacketMode.Silk_Only)
			{
				c_int celt_frame_size = Arch.IMIN(F20, frame_size);

				// Make sure to discard any previous CELT state
				if ((mode != st.prev_mode) && (st.prev_mode != PacketMode.None) && !st.prev_redundancy)
				{
					if (Celt_Decoder.Celt_Decoder_Ctl_Set(celt_dec, OpusControlSetRequest.Opus_Reset_State) != OpusError.Ok)
						return (c_int)OpusError.Internal_Error;
				}

				// Decode CELT
				celt_ret = Celt_Decoder.Celt_Decode_With_Ec_Dred(celt_dec, decode_fec ? null : data, len, pcm, celt_frame_size, dec, celt_accum);
				Celt_Decoder.Celt_Decoder_Ctl_Get(celt_dec, OpusControlGetRequest.Opus_Get_Final_Range, out st.rangeFinal);
			}
			else
			{
				byte[] silence = [ 0xff, 0xff];

				if (!celt_accum)
				{
					for (c_int i = 0; i < (frame_size * st.channels); i++)
						pcm[i] = 0;
				}

				// For hybrid -> SILK transitions, we let the CELT MDCT
				// do a face-out by decoding a silence frame
				if ((st.prev_mode == PacketMode.Hybrid) && !(redundancy && celt_to_silk && st.prev_redundancy))
				{
					if (Celt_Decoder.Celt_Decoder_Ctl_Set(celt_dec, OpusControlSetRequest.Celt_Set_Start_Band, 0) != OpusError.Ok)
						return (c_int)OpusError.Internal_Error;

					Celt_Decoder.Celt_Decode_With_Ec(celt_dec, silence, 2, pcm, F2_5, null, celt_accum);
				}

				st.rangeFinal = dec.rng;
			}

			CPointer<celt_coef> window;
			{
				if (Celt_Decoder.Celt_Decoder_Ctl_Get(celt_dec, OpusControlGetRequest.Celt_Get_Mode, out CeltMode celtMode) != OpusError.Ok)
					return (c_int)OpusError.Internal_Error;

				window = celtMode.window;
			}

			// 5 ms redundant frame for SILK->CELT
			if (redundancy && !celt_to_silk)
			{
				if (Celt_Decoder.Celt_Decoder_Ctl_Set(celt_dec, OpusControlSetRequest.Opus_Reset_State) != OpusError.Ok)
					return (c_int)OpusError.Internal_Error;

				if (Celt_Decoder.Celt_Decoder_Ctl_Set(celt_dec, OpusControlSetRequest.Celt_Set_Start_Band, 0) != OpusError.Ok)
					return (c_int)OpusError.Internal_Error;

				Celt_Decoder.Celt_Decode_With_Ec(celt_dec, data + len, redundancy_bytes, redundant_audio, F5, null, false);

				if (Celt_Decoder.Celt_Decoder_Ctl_Get(celt_dec, OpusControlGetRequest.Opus_Get_Final_Range, out redundant_rng) != OpusError.Ok)
					return (c_int)OpusError.Internal_Error;

				Smooth_Fade(pcm + st.channels * (frame_size - F2_5), redundant_audio + st.channels * F2_5,
							pcm + st.channels * (frame_size - F2_5), F2_5, st.channels, window, st.Fs);
			}

			// 5ms redundant frame for CELT->SILK; ignore if the previous frame did not
			// use CELT (the first redundancy frame in a transition from SILK may have
			// been lost)
			if (redundancy && celt_to_silk && ((st.prev_mode != PacketMode.Silk_Only) || st.prev_redundancy))
			{
				for (c_int c = 0; c < st.channels; c++)
				{
					for (c_int i = 0; i < F2_5; i++)
						pcm[st.channels * i + c] = redundant_audio[st.channels * i + c];
				}

				Smooth_Fade(redundant_audio + st.channels * F2_5, pcm + st.channels * F2_5,
							pcm + st.channels * F2_5, F2_5, st.channels, window, st.Fs);
			}

			if (transition)
			{
				if (audiosize >= F5)
				{
					for (c_int i = 0; i < (st.channels * F2_5); i++)
						pcm[i] = pcm_transition[i];

					Smooth_Fade(pcm_transition + st.channels * F2_5, pcm + st.channels * F2_5,
								pcm + st.channels * F2_5, F2_5, st.channels, window, st.Fs);
				}
				else
				{
					// Not enough time to do a clean transition, but we do it anyway
					// This will not preserve amplitude perfectly and may introduce
					// a bit of temporal aliasing, but it shouldn't be too bad and
					// that's pretty much the best we can do. In any case, generating this
					// transition it pretty silly in the first place
					Smooth_Fade(pcm_transition, pcm, pcm, F2_5, st.channels, window, st.Fs);
				}
			}

			if (st.decode_gain != 0)
			{
				opus_val32 gain = MathOps.Celt_Exp2(Arch.MULT16_16_P15(Arch.QCONST16(6.48814081e-4f, 25), st.decode_gain));

				for (c_int i = 0; i < (frame_size * st.channels); i++)
				{
					opus_val32 x = Arch.MULT16_32_P16(pcm[i], gain);
					pcm[i] = Arch.SATURATE(x, 32767);
				}
			}

			if (len <= 1)
				st.rangeFinal = 0;
			else
				st.rangeFinal ^= redundant_rng;

			st.prev_mode = mode;
			st.prev_redundancy = redundancy && !celt_to_silk;

			return celt_ret < 0 ? celt_ret : audiosize;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Decode_Native(OpusDecoderInternal st, CPointer<byte> data, opus_int32 len, CPointer<opus_res> pcm, c_int frame_size, bool decode_fec, bool self_delimited, out opus_int32 packet_offset, bool soft_clip, OpusDRED dred, opus_int32 dred_offset)
		{
			packet_offset = 0;

			// 48 x 2.5 ms = 120 ms
			opus_int16[] size = new opus_int16[48];

			// For FEC/PLC, frame size has to be to have a multiple of 2.5 ms
			if ((decode_fec || (len == 0) || data.IsNull) && (frame_size % (st.Fs / 400) != 0))
				return (c_int)OpusError.Bad_Arg;

			if ((len == 0) || data.IsNull)
			{
				c_int pcm_count = 0;

				do
				{
					c_int ret = Opus_Decode_Frame(st, null, 0, pcm + pcm_count * st.channels, frame_size - pcm_count, false);
					if (ret < 0)
						return ret;

					pcm_count += ret;
				}
				while (pcm_count < frame_size);

				st.last_packet_duration = pcm_count;

				return pcm_count;
			}
			else if (len < 0)
				return (c_int)OpusError.Bad_Arg;

			PacketMode packet_mode = Opus_Packet_Get_Mode(data);
			Bandwidth packet_bandwidth = Opus_Packet_Get_Bandwidth(data);
			c_int packet_frame_size = Opus.Opus_Packet_Get_Samples_Per_Frame(data, st.Fs);
			c_int packet_stream_channels = Opus_Packet_Get_Nb_Channels(data);

			c_int count = Opus.Opus_Packet_Parse_Impl(data, len, self_delimited, out byte toc, null, size, out c_int offset, out packet_offset, out CPointer<byte> padding, out opus_int32 padding_len);

			if (st.ignore_extensions != 0)
			{
				padding.SetToNull();
				padding_len = 0;
			}

			if (count < 0)
				return count;

			data += offset;

			if (decode_fec)
			{
				// If no FEC can be present, run the PLC (recursive call)
				if ((frame_size < packet_frame_size) || (packet_mode == PacketMode.Celt_Only) || (st.mode == PacketMode.Celt_Only))
					return Opus_Decode_Native(st, null, 0, pcm, frame_size, false, false, out _, soft_clip, null, 0);

				// Otherwise, run the PLC on everything except the size for which we might have FEC
				c_int ret;
				c_int duration_copy = st.last_packet_duration;

				if ((frame_size - packet_frame_size) != 0)
				{
					ret = Opus_Decode_Native(st, null, 0, pcm, frame_size - packet_frame_size, false, false, out _, soft_clip, null, 0);
					if (ret < 0)
					{
						st.last_packet_duration = duration_copy;
						return ret;
					}
				}

				// Complete with FEC
				st.mode = packet_mode;
				st.bandwidth = packet_bandwidth;
				st.frame_size = packet_frame_size;
				st.stream_channels = packet_stream_channels;

				ret = Opus_Decode_Frame(st, data, size[0], pcm + (st.channels * (frame_size - packet_frame_size)), packet_frame_size, true);
				if (ret < 0)
					return ret;

				st.last_packet_duration = frame_size;

				return frame_size;
			}

			if ((count * packet_frame_size) > frame_size)
				return (c_int)OpusError.Buffer_Too_Small;

			// Update the state as the last step to avoid updating it on an invalid packet
			st.mode = packet_mode;
			st.bandwidth = packet_bandwidth;
			st.frame_size = packet_frame_size;
			st.stream_channels = packet_stream_channels;

			c_int nb_samples = 0;

			for (c_int i = 0; i < count; i++)
			{
				c_int ret = Opus_Decode_Frame(st, data, size[i], pcm + (nb_samples * st.channels), frame_size - nb_samples, false);
				if (ret < 0)
					return ret;

				data += size[i];
				nb_samples += ret;
			}

			st.last_packet_duration = nb_samples;

			if (soft_clip)
				Opus.Opus_Pcm_Soft_Clip_Impl(pcm, nb_samples, st.channels, st.softclip_mem, st.arch);
			else
				st.softclip_mem[0] = st.softclip_mem[1] = 0;

			return nb_samples;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Decode(OpusDecoderInternal st, CPointer<byte> data, opus_int32 len, CPointer<opus_int16> pcm, c_int frame_size, bool decode_fec)
		{
			if (frame_size <= 0)
				return (c_int)OpusError.Bad_Arg;

			if (data.IsNotNull && (len > 0) && !decode_fec)
			{
				c_int nb_samples = Opus_Decoder_Get_Nb_Samples(st, data, len);
				if (nb_samples > 0)
					frame_size = Arch.IMIN(frame_size, nb_samples);
				else
					return (c_int)OpusError.Invalid_Packet;
			}

			opus_res[] _out = new opus_res[frame_size * st.channels];

			c_int ret = Opus_Decode_Native(st, data, len, _out, frame_size, decode_fec, false, out _, true, null, 0);
			if (ret > 0)
				MathOps.Celt_Float2Int16(_out, pcm, ret * st.channels, st.arch);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Decode_Float(OpusDecoderInternal st, CPointer<byte> data, opus_int32 len, CPointer<opus_val16> pcm, c_int frame_size, bool decode_fec)
		{
			if (frame_size <= 0)
				return (c_int)OpusError.Bad_Arg;

			return Opus_Decode_Native(st, data, len, pcm, frame_size, decode_fec, false, out _, false, null, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_Decoder_Ctl_Get<T>(OpusDecoderInternal st, OpusControlGetRequest request, out T _out) where T : INumber<T>
		{
			// I know, the casting below in the case statements are not pretty
			if (typeof(T) == typeof(c_int))
			{
				switch (request)
				{
					case OpusControlGetRequest.Opus_Get_Bandwidth:
					{
						_out = (T)(object)st.bandwidth;
						return OpusError.Ok;
					}

					case OpusControlGetRequest.Opus_Get_Sample_Rate:
					{
						_out = (T)(object)st.Fs;
						return OpusError.Ok;
					}

					case OpusControlGetRequest.Opus_Get_Pitch:
					{
						if (st.prev_mode == PacketMode.Celt_Only)
							return Celt_Decoder.Celt_Decoder_Ctl_Get(st.celt_dec, OpusControlGetRequest.Opus_Get_Pitch, out _out);

						_out = (T)(object)st.DecControl.prevPitchLag;
						return OpusError.Ok;
					}

					case OpusControlGetRequest.Opus_Get_Gain:
					{
						_out = (T)(object)st.decode_gain;
						return OpusError.Ok;
					}

					case OpusControlGetRequest.Opus_Get_Last_Packet_Duration:
					{
						_out = (T)(object)st.last_packet_duration;
						return OpusError.Ok;
					}
				}
			}
			else if (typeof(T) == typeof(opus_uint32))
			{
				switch (request)
				{
					case OpusControlGetRequest.Opus_Get_Final_Range:
					{
						_out = (T)(object)st.rangeFinal;
						return OpusError.Ok;
					}
				}
			}

			_out = default;
			return OpusError.Unimplemented;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_Decoder_Ctl_Set(OpusDecoderInternal st, OpusControlSetRequest request, params object[] args)
		{
			switch (request)
			{
				case OpusControlSetRequest.Opus_Reset_State:
				{
					st.Reset();

					Celt_Decoder.Celt_Decoder_Ctl_Set(st.celt_dec, OpusControlSetRequest.Opus_Reset_State);
					Dec_Api.Silk_ResetDecoder(st.silk_dec);

					st.stream_channels = st.channels;
					st.frame_size = st.Fs / 400;
					break;
				}

				case OpusControlSetRequest.Opus_Set_Gain:
				{
					if ((args.Length == 0) || (args[0].GetType() != typeof(opus_int32)))
						return OpusError.Bad_Arg;

					opus_int32 value = (opus_int32)args[0];
					if ((value < -32768) || (value > 32767))
						return OpusError.Bad_Arg;

					st.decode_gain = value;
					break;
				}

				default:
					return OpusError.Unimplemented;
			}

			return OpusError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Opus_Decoder_Destroy(OpusDecoderInternal st)
		{
			Memory.Opus_Free(st);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Bandwidth Opus_Packet_Get_Bandwidth(CPointer<byte> data)
		{
			Bandwidth bandwidth;

			if ((data[0] & 0x80) != 0)
			{
				bandwidth = Bandwidth.Mediumband + ((data[0] >> 5) & 0x3);
				if (bandwidth == Bandwidth.Mediumband)
					bandwidth = Bandwidth.Narrowband;
			}
			else if ((data[0] & 0x60) == 0x60)
				bandwidth = (data[0] & 0x10) != 0 ? Bandwidth.Fullband : Bandwidth.Superwideband;
			else
				bandwidth = Bandwidth.Narrowband + ((data[0] >> 5) & 0x3);

			return bandwidth;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Packet_Get_Nb_Channels(CPointer<byte> data)
		{
			return (data[0] & 0x4) != 0 ? 2 : 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Packet_Get_Nb_Frames(CPointer<byte> packet, opus_int32 len)
		{
			if (len < 1)
				return (c_int)OpusError.Bad_Arg;

			c_int count = packet[0] & 0x3;
			if (count == 0)
				return 1;

			if (count != 3)
				return 2;

			if (len < 2)
				return (c_int)OpusError.Invalid_Packet;

			return packet[1] & 0x3f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Packet_Get_Nb_Samples(CPointer<byte> packet, opus_int32 len, opus_int32 Fs)
		{
			c_int count = Opus_Packet_Get_Nb_Frames(packet, len);

			if (count < 0)
				return count;

			c_int samples = count * Opus.Opus_Packet_Get_Samples_Per_Frame(packet, Fs);

			// Can't have more than 120 ms
			if ((samples * 25) > (Fs * 3))
				return (c_int)OpusError.Invalid_Packet;

			return samples;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Decoder_Get_Nb_Samples(OpusDecoderInternal dec, CPointer<byte> packet, opus_int32 len)
		{
			return Opus_Packet_Get_Nb_Samples(packet, len, dec.Fs);
		}
	}
}
