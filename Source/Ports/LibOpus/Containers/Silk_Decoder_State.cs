/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Decoder state
	/// </summary>
	internal class Silk_Decoder_State : IDeepCloneable<Silk_Decoder_State>
	{
		public opus_int32 prev_gain_Q16;
		public CPointer<opus_int32> exc_Q14 = new CPointer<opus_int32>(Constants.Max_Frame_Length);
		public readonly opus_int32[] sLpc_Q14_buf = new opus_int32[Constants.Max_Lpc_Order];

		/// <summary>
		/// Buffer for output signal
		/// </summary>
		public CPointer<opus_int16> outBuf = new CPointer<opus_int16>(Constants.Max_Frame_Length + 2 * Constants.Max_Sub_Frame_Length);

		/// <summary>
		/// Previous lag
		/// </summary>
		public opus_int lagPrev;

		/// <summary>
		/// Previous gain index
		/// </summary>
		public opus_int8 LastGainIndex;

		/// <summary>
		/// Sampling frequency in kHz
		/// </summary>
		public opus_int fs_kHz;

		/// <summary>
		/// API sample frequency (Hz)
		/// </summary>
		public opus_int32 fs_API_hz;

		/// <summary>
		/// Number of 5 ms subframes in a frame
		/// </summary>
		public opus_int nb_subfr;

		/// <summary>
		/// Frame length (samples)
		/// </summary>
		public opus_int frame_length;

		/// <summary>
		/// Subframe length (samples)
		/// </summary>
		public opus_int subfr_length;

		/// <summary>
		/// Length of LTP memory
		/// </summary>
		public opus_int ltp_mem_length;

		/// <summary>
		/// LPC order
		/// </summary>
		public opus_int LPC_Order;

		/// <summary>
		/// Used to interpolate LSFs
		/// </summary>
		public readonly opus_int16[] prevNLSF_Q15 = new opus_int16[Constants.Max_Lpc_Order];

		/// <summary>
		/// Flag for deactivating NLSF interpolation
		/// </summary>
		public bool first_frame_after_reset;

		/// <summary>
		/// Pointer to iCDF table for low bits of pitch lag index
		/// </summary>
		public CPointer<opus_uint8> pitch_lag_low_bits_iCDF;

		/// <summary>
		/// Pointer to iCDF table for pitch contour index
		/// </summary>
		public CPointer<opus_uint8> pitch_contour_iCDF;

		// For buffering payload in case of more frames per packet
		public opus_int nFramesDecoded;
		public opus_int nFramesPerPacket;

		// Specifically for entropy coding
		public SignalType ec_prevSignalType;
		public opus_int16 ec_prevLagIndex;

		public readonly bool[] VAD_flags = new bool[Constants.Max_Frames_Per_Packet];
		public bool LBRR_flag;
		public readonly bool[] LBRR_flags = new bool[Constants.Max_Frames_Per_Packet];

		public Silk_Resampler_State_Struct resampler_state = new Silk_Resampler_State_Struct();

		/// <summary>
		/// Pointer to NLSF codebook
		/// </summary>
		public Silk_NLSF_CB_Struct psNLSF_CB;

		/// <summary>
		/// Quantization indices
		/// </summary>
		public SideInfoIndices indices = new SideInfoIndices();

		/// <summary>
		/// CNG state
		/// </summary>
		public Silk_CNG_Struct sCNG = new Silk_CNG_Struct();

		// Stuff used for PLC
		public opus_int lossCnt;
		public SignalType prevSignalType;
		public c_int arch;

		public Silk_PLC_Struct sPLC = new Silk_PLC_Struct();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			prev_gain_Q16 = 0;

			exc_Q14.Clear();
			Array.Clear(sLpc_Q14_buf);
			outBuf.Clear();

			lagPrev = 0;
			LastGainIndex = 0;
			fs_kHz = 0;
			fs_API_hz = 0;
			nb_subfr = 0;
			frame_length = 0;
			subfr_length = 0;
			ltp_mem_length = 0;
			LPC_Order = 0;

			Array.Clear(prevNLSF_Q15);

			first_frame_after_reset = false;
			pitch_lag_low_bits_iCDF.SetToNull();
			pitch_contour_iCDF.SetToNull();

			nFramesDecoded = 0;
			nFramesPerPacket = 0;
			ec_prevSignalType = 0;
			ec_prevLagIndex = 0;

			Array.Clear(VAD_flags);

			LBRR_flag = false;

			Array.Clear(LBRR_flags);

			resampler_state.Clear();

			psNLSF_CB = null;

			indices.Clear();
			sCNG.Clear();

			lossCnt = 0;
			prevSignalType = 0;
			arch = 0;

			sPLC.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Silk_Decoder_State MakeDeepClone()
		{
			Silk_Decoder_State clone = new Silk_Decoder_State
			{
				prev_gain_Q16 = prev_gain_Q16,
				exc_Q14 = exc_Q14.MakeDeepClone(),
				outBuf = outBuf.MakeDeepClone(),
				lagPrev = lagPrev,
				LastGainIndex = LastGainIndex,
				fs_kHz = fs_kHz,
				fs_API_hz = fs_API_hz,
				nb_subfr = nb_subfr,
				frame_length = frame_length,
				subfr_length = subfr_length,
				ltp_mem_length = ltp_mem_length,
				LPC_Order = LPC_Order,
				first_frame_after_reset = first_frame_after_reset,
				nFramesDecoded = nFramesDecoded,
				nFramesPerPacket = nFramesPerPacket,
				pitch_lag_low_bits_iCDF = pitch_lag_low_bits_iCDF.MakeDeepClone(),
				pitch_contour_iCDF = pitch_contour_iCDF.MakeDeepClone(),
				ec_prevSignalType = ec_prevSignalType,
				ec_prevLagIndex = ec_prevLagIndex,
				LBRR_flag = LBRR_flag,
				resampler_state = resampler_state.MakeDeepClone(),
				psNLSF_CB = psNLSF_CB?.MakeDeepClone(),
				indices = indices.MakeDeepClone(),
				sCNG = sCNG.MakeDeepClone(),
				lossCnt = lossCnt,
				prevSignalType = prevSignalType,
				arch = arch,
				sPLC = sPLC.MakeDeepClone()
			};

			Array.Copy(sLpc_Q14_buf, clone.sLpc_Q14_buf, sLpc_Q14_buf.Length);
			Array.Copy(prevNLSF_Q15, clone.prevNLSF_Q15, prevNLSF_Q15.Length);
			Array.Copy(VAD_flags, clone.VAD_flags, VAD_flags.Length);
			Array.Copy(LBRR_flags, clone.LBRR_flags, LBRR_flags.Length);

			return clone;
		}
	}
}
