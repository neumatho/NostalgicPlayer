/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Dec_Api
	{
		/********************************************************************/
		/// <summary>
		/// Reset decoder state
		/// </summary>
		/********************************************************************/
		public static SilkError Silk_ResetDecoder(Silk_Decoder decState)
		{
			SilkError ret = SilkError.No_Error;
			Silk_Decoder_State[] channel_state = decState.channel_state;

			for (opus_int n = 0; n < Constants.Decoder_Num_Channels; n++)
				ret = Init_Decoder.Silk_Reset_Decoder(channel_state[n]);

			decState.sStereo.Clear();

			// Not strictly needed, but it's cleaner that way
			decState.prev_decode_only_middle = false;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static SilkError Silk_InitDecoder(Silk_Decoder decState)
		{
			SilkError ret = SilkError.No_Error;
			Silk_Decoder_State[] channel_state = decState.channel_state;

			for (opus_int n = 0; n < Constants.Decoder_Num_Channels; n++)
				ret = Init_Decoder.Silk_Init_Decoder(channel_state[n]);

			decState.sStereo.Clear();

			// Not strictly needed, but it's cleaner that way
			decState.prev_decode_only_middle = false;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static SilkError Silk_Decode(Silk_Decoder decState, Silk_DecControlStruct decControl, LostFlag lostFlag, bool newPacketFlag, Ec_Dec psRangeDec, CPointer<opus_int16> samplesOut, out opus_int32 nSamplesOut, c_int arch)
		{
			nSamplesOut = 0;

			bool decode_only_middle = false;
			SilkError ret = SilkError.No_Error;
			SilkError resultRet;
			opus_int32 nSamplesOutDec = 0;
			CPointer<opus_int16>[] samplesOut1_tmp = new CPointer<opus_int16>[2];
			opus_int32[] MS_pred_Q13 = new opus_int32[2];
			Silk_Decoder psDec = decState;
			Silk_Decoder_State[] channel_state = psDec.channel_state;

			/**********************************/
			/* Test if first frame in payload */
			/**********************************/
			if (newPacketFlag)
			{
				for (opus_int n = 0; n < decControl.nChannelsInternal; n++)
					channel_state[n].nFramesDecoded = 0;	// Used to count frames in packet
			}

			// If Mono -> Stereo transition in bitstream: init state of second channel
			if (decControl.nChannelsInternal > psDec.nChannelsInternal)
			{
				resultRet = Init_Decoder.Silk_Init_Decoder(channel_state[1]);
				ret = resultRet != SilkError.No_Error ? resultRet : ret;
			}

			bool stereo_to_mono = (decControl.nChannelsInternal == 1) && (psDec.nChannelsInternal == 2) && (decControl.internalSampleRate == (1000 * channel_state[0].fs_kHz));

			if (channel_state[0].nFramesDecoded == 0)
			{
				for (opus_int n = 0; n < decControl.nChannelsInternal; n++)
				{
					if (decControl.payloadSize_ms == 0)
					{
						// Assuming packet loss, use 10 ms
						channel_state[n].nFramesPerPacket = 1;
						channel_state[n].nb_subfr = 2;
					}
					else if (decControl.payloadSize_ms == 10)
					{
						channel_state[n].nFramesPerPacket = 1;
						channel_state[n].nb_subfr = 2;
					}
					else if (decControl.payloadSize_ms == 20)
					{
						channel_state[n].nFramesPerPacket = 1;
						channel_state[n].nb_subfr = 4;
					}
					else if (decControl.payloadSize_ms == 40)
					{
						channel_state[n].nFramesPerPacket = 2;
						channel_state[n].nb_subfr = 4;
					}
					else if (decControl.payloadSize_ms == 60)
					{
						channel_state[n].nFramesPerPacket = 3;
						channel_state[n].nb_subfr = 4;
					}
					else
						return SilkError.Invalid_Frame_Size;

					opus_int fs_kHz_dec = (decControl.internalSampleRate >> 10) + 1;

					if ((fs_kHz_dec != 8) && (fs_kHz_dec != 12) && (fs_kHz_dec != 16))
						return SilkError.Invalid_Sampling_Frequency;

					resultRet = Decoder_Set_Fs.Silk_Decoder_Set_Fs(channel_state[n], fs_kHz_dec, decControl.API_sampleRate);
					ret = resultRet != SilkError.No_Error ? resultRet : ret;
				}
			}

			if ((decControl.nChannelsAPI == 2) && (decControl.nChannelsInternal == 2) && ((psDec.nChannelsAPI == 1) || (psDec.nChannelsInternal == 1)))
			{
				SigProc_Fix.Silk_MemSet(psDec.sStereo.pred_prev_Q13, (opus_int16)0, psDec.sStereo.pred_prev_Q13.Length);
				SigProc_Fix.Silk_MemSet(psDec.sStereo.sSide, (opus_int16)0, psDec.sStereo.sSide.Length);
				channel_state[1].resampler_state = channel_state[0].resampler_state.MakeDeepClone();
			}

			psDec.nChannelsAPI = decControl.nChannelsAPI;
			psDec.nChannelsInternal = decControl.nChannelsInternal;

			if ((decControl.API_sampleRate > (Constants.Max_Api_Fs_KHz * 1000)) || (decControl.API_sampleRate < 8000))
			{
				ret = SilkError.Invalid_Sampling_Frequency;
				return ret;
			}

			if ((lostFlag != LostFlag.Packet_Lost) && (channel_state[0].nFramesDecoded == 0))
			{
				// First decoder call for this payload
				// Decode VAD flags and LBRR flag
				for (opus_int n = 0; n < decControl.nChannelsInternal; n++)
				{
					for (opus_int i = 0; i < channel_state[n].nFramesPerPacket; i++)
						channel_state[n].VAD_flags[i] = EntDec.Ec_Dec_Bit_Logp(psRangeDec, 1);

					channel_state[n].LBRR_flag = EntDec.Ec_Dec_Bit_Logp(psRangeDec, 1);
				}

				// Decode LBRR flags
				for (opus_int n = 0; n < decControl.nChannelsInternal; n++)
				{
					SigProc_Fix.Silk_MemSet(channel_state[n].LBRR_flags, false, channel_state[n].LBRR_flags.Length);

					if (channel_state[n].LBRR_flag)
					{
						if (channel_state[n].nFramesPerPacket == 1)
							channel_state[n].LBRR_flags[0] = true;
						else
						{
							opus_int32 LBRR_symbol = EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_LBRR_Flags_iCDF_Ptr[channel_state[n].nFramesPerPacket - 2], 8) + 1;

							for (opus_int i = 0; i < channel_state[n].nFramesPerPacket; i++)
								channel_state[n].LBRR_flags[i] = (SigProc_Fix.Silk_RSHIFT(LBRR_symbol, i) & 1) != 0;
						}
					}
				}

				if (lostFlag == LostFlag.Decode_Normal)
				{
					// Regular decoding: skip all LBRR data
					for (opus_int i = 0; i < channel_state[0].nFramesPerPacket; i++)
					{
						for (opus_int n = 0; n < decControl.nChannelsInternal; n++)
						{
							if (channel_state[n].LBRR_flags[i])
							{
								opus_int16[] pulses = new opus_int16[Constants.Max_Frame_Length];

								if ((decControl.nChannelsInternal == 2) && (n == 0))
								{
									Stereo_Decode_Pred.Silk_Stereo_Decode_Pred(psRangeDec, MS_pred_Q13);

									if (channel_state[1].LBRR_flags[i] == false)
										Stereo_Decode_Pred.Silk_Stereo_Decode_Mid_Only(psRangeDec, out decode_only_middle);
								}

								// Use conditional coding if previous frame available
								CodeType condCoding;

								if ((i > 0) && channel_state[n].LBRR_flags[i - 1])
									condCoding = CodeType.Conditionally;
								else
									condCoding = CodeType.Independently;

								Decode_Indices.Silk_Decode_Indices(channel_state[n], psRangeDec, i, true, condCoding);
								Decode_Pulses.Silk_Decode_Pulses(psRangeDec, pulses, channel_state[n].indices.signalType, channel_state[n].indices.quantOffsetType, channel_state[n].frame_length);
							}
						}
					}
				}
			}

			// Get MS predictor index
			if (decControl.nChannelsInternal == 2)
			{
				if ((lostFlag == LostFlag.Decode_Normal) || ((lostFlag == LostFlag.Decode_LBRR) && (channel_state[0].LBRR_flags[channel_state[0].nFramesDecoded] == true)))
				{
					Stereo_Decode_Pred.Silk_Stereo_Decode_Pred(psRangeDec, MS_pred_Q13);

					// For LBRR data, decode mid-only flag only if side-channel's LBRR flag is false
					if (((lostFlag == LostFlag.Decode_Normal) && (channel_state[1].VAD_flags[channel_state[0].nFramesDecoded] == false)) ||
					    ((lostFlag == LostFlag.Decode_LBRR) && (channel_state[1].LBRR_flags[channel_state[0].nFramesDecoded] == false)))
					{
						Stereo_Decode_Pred.Silk_Stereo_Decode_Mid_Only(psRangeDec, out decode_only_middle);
					}
					else
						decode_only_middle = false;
				}
				else
				{
					for (opus_int n = 0; n < 2; n++)
						MS_pred_Q13[n] = psDec.sStereo.pred_prev_Q13[n];
				}
			}

			// Reset side channel decoder prediction memory for first frame with side coding
			if ((decControl.nChannelsInternal == 2) && (decode_only_middle == false) && psDec.prev_decode_only_middle)
			{
				psDec.channel_state[1].outBuf.Clear();
				SigProc_Fix.Silk_MemSet(psDec.channel_state[1].sLpc_Q14_buf, 0, psDec.channel_state[1].sLpc_Q14_buf.Length);

				psDec.channel_state[1].lagPrev = 100;
				psDec.channel_state[1].LastGainIndex = 10;
				psDec.channel_state[1].prevSignalType = SignalType.No_Voice_Activity;
				psDec.channel_state[1].first_frame_after_reset = true;
			}

			// Check if the temp buffer fits into the output PCM buffer. If it fits,
			// we can delay allocating the temp buffer until after the SILK peak stack
			// usage. We need to use a < and not a <= because of the two extra samples
			bool delay_stack_alloc = (decControl.internalSampleRate * decControl.nChannelsInternal) < (decControl.API_sampleRate * decControl.nChannelsAPI);

			CPointer<opus_int16> samplesOut1_tmp_storage1 = new CPointer<opus_int16>(delay_stack_alloc ? Constants.Alloc_None : decControl.nChannelsInternal * (channel_state[0].frame_length + 2));

			if (delay_stack_alloc)
			{
				samplesOut1_tmp[0] = samplesOut;
				samplesOut1_tmp[1] = samplesOut + channel_state[0].frame_length + 2;
			}
			else
			{
				samplesOut1_tmp[0] = samplesOut1_tmp_storage1;
				samplesOut1_tmp[1] = samplesOut1_tmp_storage1 + channel_state[0].frame_length + 2;
			}

			bool has_side;

			if (lostFlag == LostFlag.Decode_Normal)
				has_side = !decode_only_middle;
			else
				has_side = !psDec.prev_decode_only_middle || ((decControl.nChannelsInternal == 2) && (lostFlag == LostFlag.Decode_LBRR) && (channel_state[1].LBRR_flags[channel_state[1].nFramesDecoded] == true));

			channel_state[0].sPLC.enable_deep_plc = decControl.enable_deep_plc;

			// Call decoder for one frame
			for (opus_int n = 0; n < decControl.nChannelsInternal; n++)
			{
				if ((n == 0) || has_side)
				{
					opus_int FrameIndex = channel_state[0].nFramesDecoded - n;
					CodeType condCoding;

					// Use independent coding if no previous frame available
					if (FrameIndex <= 0)
						condCoding = CodeType.Independently;
					else if (lostFlag == LostFlag.Decode_LBRR)
						condCoding = channel_state[n].LBRR_flags[FrameIndex - 1] ? CodeType.Conditionally : CodeType.Independently;
					else if ((n > 0) && psDec.prev_decode_only_middle)
					{
						// If we skipped a side frame in this packet, we don't
						// need LTP scaling; the LTP state is well-defined
						condCoding = CodeType.Independently_No_LTP_Scaling;
					}
					else
						condCoding = CodeType.Conditionally;

					resultRet = Decode_Frame.Silk_Decode_Frame(channel_state[n], psRangeDec, samplesOut1_tmp[n] + 2, out nSamplesOutDec, lostFlag, condCoding, arch);
					ret = resultRet != SilkError.No_Error ? resultRet : ret;
				}
				else
					SigProc_Fix.Silk_MemSet<opus_int16>(samplesOut1_tmp[n] + 2, 0, nSamplesOutDec);

				channel_state[n].nFramesDecoded++;
			}

			if ((decControl.nChannelsAPI == 2) && (decControl.nChannelsInternal == 2))
			{
				// Convert Mid/Side to Left/Right
				Stereo_MS_To_LR.Silk_Stereo_MS_To_LR(psDec.sStereo, samplesOut1_tmp[0], samplesOut1_tmp[1], MS_pred_Q13, channel_state[0].fs_kHz, nSamplesOutDec);
			}
			else
			{
				// Buffering
				SigProc_Fix.Silk_MemCpy(samplesOut1_tmp[0], psDec.sStereo.sMid, 2);
				SigProc_Fix.Silk_MemCpy(psDec.sStereo.sMid, samplesOut1_tmp[0] + nSamplesOutDec, 2);
			}

			// Number of output samples
			nSamplesOut = SigProc_Fix.Silk_DIV32(nSamplesOutDec * decControl.API_sampleRate, Macros.Silk_SMULBB(channel_state[0].fs_kHz, 1000));

			// Set up pointers to temp buffers
			opus_int16[] samplesOut2_tmp = new opus_int16[decControl.nChannelsAPI == 2 ? nSamplesOut : Constants.Alloc_None];
			CPointer<opus_int16> resample_out_ptr;

			if (decControl.nChannelsAPI == 2)
				resample_out_ptr = samplesOut2_tmp;
			else
				resample_out_ptr = samplesOut;

			CPointer<opus_int16> samplesOut1_tmp_storage2 = new CPointer<opus_int16>(delay_stack_alloc ? decControl.nChannelsInternal * (channel_state[0].frame_length + 2) : Constants.Alloc_None);

			if (delay_stack_alloc)
			{
				Memory.Opus_Copy(samplesOut1_tmp_storage2, samplesOut, decControl.nChannelsInternal * (channel_state[0].frame_length + 2));

				samplesOut1_tmp[0] = samplesOut1_tmp_storage2;
				samplesOut1_tmp[1] = samplesOut1_tmp_storage2 + channel_state[0].frame_length + 2;
			}

			for (opus_int n = 0; n < SigProc_Fix.Silk_Min(decControl.nChannelsAPI, decControl.nChannelsInternal); n++)
			{
				// Resample decoded signal to API sampleRate
				resultRet = Resampler.Silk_Resampler(channel_state[n].resampler_state, resample_out_ptr, samplesOut1_tmp[n] + 1, nSamplesOutDec);
				ret = resultRet != SilkError.No_Error ? resultRet : ret;

				// Interleave if stereo output and stereo stream
				if (decControl.nChannelsAPI == 2)
				{
					for (opus_int i = 0; i < nSamplesOut; i++)
						samplesOut[n + 2 * i] = resample_out_ptr[i];
				}
			}

			// Create two channel output from mono stream
			if ((decControl.nChannelsAPI == 2) && (decControl.nChannelsInternal == 1))
			{
				if (stereo_to_mono)
				{
					// Resample right channel for newly collapsed stereo just in case
					// we weren't doing collapsing when switching to mono
					resultRet = Resampler.Silk_Resampler(channel_state[1].resampler_state, resample_out_ptr, samplesOut1_tmp[0] + 1, nSamplesOutDec);
					ret = resultRet != SilkError.No_Error ? resultRet : ret;

					for (opus_int i = 0; i < nSamplesOut; i++)
						samplesOut[1 + 2 * i] = resample_out_ptr[i];
				}
				else
				{
					for (opus_int i = 0; i < nSamplesOut; i++)
						samplesOut[1 + 2 * i] = samplesOut[0 + 2 * i];
				}
			}

			// Export pitch lag, measured at 48 kHz sampling rate
			if (channel_state[0].prevSignalType == SignalType.Voiced)
			{
				c_int[] mult_tab = [ 6, 4, 3 ];

				decControl.prevPitchLag = channel_state[0].lagPrev * mult_tab[(channel_state[0].fs_kHz - 8) >> 2];
			}
			else
				decControl.prevPitchLag = 0;

			if (lostFlag == LostFlag.Packet_Lost)
			{
				// On packet lost, remove the gain clamping to prevent having the energy "bounce back"
				// if we lose packets when the energy is going down
				for (opus_int i = 0; i < psDec.nChannelsInternal; i++)
					psDec.channel_state[i].LastGainIndex = 10;
			}
			else
				psDec.prev_decode_only_middle = decode_only_middle;

			return ret;
		}
	}
}
