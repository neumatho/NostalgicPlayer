/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Containers;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Exceptions;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Resample;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
{
	/// <summary>
	/// MOS6581/MOS8580 emulation
	/// </summary>
	public class Sid
	{
		// The waveform D/A converter introduces a DC offset in the signal
		// to the envelope multiplying D/A converter. The "zero" level of
		// the waveform D/A converter can be found as follows:
		//
		// Measure the "zero" voltage of voice 3 on the SID audio output
		// pin, routing only voice 3 to the mixer ($d417 = $0b, $d418 =
		// $0f, all other registers zeroed).
		//
		// Then set the sustain level for voice 3 to maximum and search for
		// the waveform output value yielding the same voltage as found
		// above. This is done by trying out different waveform output
		// values until the correct value is found, e.g. with the following
		// program:
		//
		//       lda #$08
		//       sta $d412
		//       lda #$0b
		//       sta $d417
		//       lda #$0f
		//       sta $d418
		//       lda #$f0
		//       sta $d414
		//       lda #$21
		//       sta $d412
		//       lda #$01
		//       sta $d40e
		//
		//       ldx #$00
		//       lda #$38        ; Tweak this to find the "zero" level
		// l     cmp $d41b
		//       bne l
		//       stx $d40e       ; Stop frequency counter - freeze waveform output
		//       brk
		//
		// The waveform output range is 0x000 to 0xfff, so the "zero"
		// level should ideally have been 0x800. In the measured chip, the
		// waveform output "zero" level was found to be 0x380 (i.e. $d41b
		// = 0x38) at an audio output voltage of 5.94V.
		//
		// With knowledge of the mixer op-amp characteristics, further estimates
		// of waveform voltages can be obtained by sampling the EXT IN pin.
		// From EXT IN samples, the corresponding waveform output can be found by
		// using the model for the mixer.
		//
		// Such measurements have been done on a chip marked MOS 6581R4AR
		// 0687 14, and the following results have been obtained:
		// * The full range of one voice is approximately 1.5V.
		// * The "zero" level rides at approximately 5.0V.
		//
		//
		// zero-x did the measuring on the 8580 (https://sourceforge.net/p/vice-emu/bugs/1036/#c5b3):
		// When it sits on basic from powerup it's at 4.72
		// Run 1.prg and check the output pin level.
		// Then run 2.prg and adjust it until the output level is the same...
		// 0x94-0xA8 gives me the same 4.72 1.prg shows.
		// On another 8580 it's 0x90-0x9C
		// Third chip 0x94-0xA8
		// Fourth chip 0x90-0xA4
		// On the 8580 that plays digis the output is 4.66 and 0x93 is the only value to reach that.
		// To me that seems as regular 8580s have somewhat wide 0-level range,
		// whereas that digi-compatible 8580 has it very narrow.
		// On my 6581R4AR has 0x3A as the only value giving the same output level as 1.prg
		private const uint OFFSET_6581 = 0x380;
		private const uint OFFSET_8580 = 0x9c0;

		private const uint ENV_DAC_BITS = 8;
		private const uint OSC_DAC_BITS = 12;

		// Bus value stays alive for some time after each operation.
		// Values differs between chip models, the timings used here
		// are taken from VICE [1].
		// See also the discussion "How do I reliably detect 6581/8580 sid?" on CSDb [2].
		//
		//   Results from real C64 (testprogs/SID/bitfade/delayfrq0.prg):
		//
		//   (new SID) (250469/8580R5) (250469/8580R5)
		//   delayfrq0    ~7a000        ~108000
		//
		//   (old SID) (250407/6581)
		//   delayfrq0    ~01d00
		//
		// [1]: http://sourceforge.net/p/vice-emu/patches/99/
		// [2]: http://noname.c64.org/csdb/forums/?roomid=11&topicid=29025&showallposts=1
		private const int BUS_TTL_6581 = 0x01d00;
		private const int BUS_TTL_8580 = 0xa2000;

		/// <summary>
		/// Currently active filter
		/// </summary>
		private Filter filter;

		/// <summary>
		/// Filter used, if model is set to 6581
		/// </summary>
		private readonly Filter6581 filter6581;

		/// <summary>
		/// Filter used, if model is set to 8580
		/// </summary>
		private readonly Filter8580 filter8580;

		/// <summary>
		/// Resampler used by audio generation code
		/// </summary>
		private Resampler resampler;

		/// <summary>
		/// External filter that provides high-pass and low-pass filtering
		/// to adjust sound tone slightly
		/// </summary>
		private readonly ExternalFilter externalFilter = new ExternalFilter();

		/// <summary>
		/// Paddle X register support
		/// </summary>
		private readonly Potentiometer potX = new Potentiometer();

		/// <summary>
		/// Paddle Y register support
		/// </summary>
		private readonly Potentiometer potY = new Potentiometer();

		/// <summary>
		/// SID voices
		/// </summary>
		private readonly Voice[] voice =  ArrayHelper.InitializeArray<Voice>(3);

		/// <summary>
		/// Used to amplify the output by x/2 to get an adequate playback volume
		/// </summary>
		private int scaleFactor;

		/// <summary>
		/// Time to live for the last written value
		/// </summary>
		private int busValueTtl;

		/// <summary>
		/// Current chip model's bus value TTL
		/// </summary>
		private int modelTtl;

		/// <summary>
		/// Time until VoiceSync() must run
		/// </summary>
		private uint nextVoiceSync;

		/// <summary>
		/// Currently active chip model
		/// </summary>
		private ChipModel model;

		/// <summary>
		/// Currently selected combined waveforms strength
		/// </summary>
		private CombinedWaveforms cws;

		/// <summary>
		/// Last written value
		/// </summary>
		private byte busValue;

		/// <summary>
		/// Emulated non-linearity of the envelope DAC
		/// </summary>
		private readonly float[] envDac = new float[256];

		/// <summary>
		/// Emulated non-linearity of the oscillator DAC
		/// </summary>
		private readonly float[] oscDac = new float[4096];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Sid()
		{
			filter6581 = new Filter6581();
			filter8580 = new Filter8580();
			resampler = null;
			cws = CombinedWaveforms.AVERAGE;

			voice[0].SetOtherVoices(voice[2], voice[1]);
			voice[1].SetOtherVoices(voice[0], voice[2]);
			voice[2].SetOtherVoices(voice[1], voice[0]);

			SetChipModel(ChipModel.MOS8580);
			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Set filter curve parameter for 6581 model
		/// </summary>
		/********************************************************************/
		public void SetFilter6581Curve(double filterCurve)
		{
			filter6581.SetFilterCurve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// Set filter curve parameter for 6581 model
		/// </summary>
		/********************************************************************/
		public void SetFilter6581Range(double adjustment)
		{
			filter6581.SetFilterRange(adjustment);
		}



		/********************************************************************/
		/// <summary>
		/// Set filter curve parameter for 8580 model
		/// </summary>
		/********************************************************************/
		public void SetFilter8580Curve(double filterCurve)
		{
			filter8580.SetFilterCurve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter emulation
		/// </summary>
		/********************************************************************/
		public void EnableFilter(bool enable)
		{
			filter6581.Enable(enable);
			filter8580.Enable(enable);
		}



		/********************************************************************/
		/// <summary>
		/// Set chip model
		/// </summary>
		/********************************************************************/
		public void SetChipModel(ChipModel model)
		{
			switch (model)
			{
				case ChipModel.MOS6581:
				{
					filter = filter6581;
					scaleFactor = 3;
					modelTtl = BUS_TTL_6581;
					break;
				}

				case ChipModel.MOS8580:
				{
					filter = filter8580;
					scaleFactor = 5;
					modelTtl = BUS_TTL_8580;
					break;
				}

				default:
					throw new SidErrorException(Resources.IDS_SID_ERR_UNKNOWN_CHIP);
			}

			this.model = model;

			// Calculate waveform-related tables
			matrix_t waveTables = WaveformCalculator.GetInstance().GetWaveTable();
			matrix_t pulldownTables = WaveformCalculator.GetInstance().BuildPulldownTable(model, cws);

			// Calculate envelope DAC table
			{
				Dac dacBuilder = new Dac(ENV_DAC_BITS);
				dacBuilder.KinkedDac(model);

				for (uint i = 0; i < (1 << (int)ENV_DAC_BITS); i++)
					envDac[i] = (float)dacBuilder.GetOutput(i);
			}

			// Calculate oscillator DAC table
			bool is6581 = model == ChipModel.MOS6581;

			{
				Dac dacBuilder = new Dac(OSC_DAC_BITS);
				dacBuilder.KinkedDac(model);

				//double offset = dacBuilder.GetOutput(is6581 ? OFFSET_6581 : OFFSET_8580);
				double offset = dacBuilder.GetOutput(0x7ff, is6581);

				for (uint i = 0; i < (1 << (int)OSC_DAC_BITS); i++)
				{
					double dacValue = dacBuilder.GetOutput(i, is6581);
					oscDac[i] = (float)(dacValue - offset);
				}
			}

			// Set voice tables
			for (int i = 0; i < 3; i++)
			{
				voice[i].SetEnvDac(envDac);
				voice[i].SetWavDac(oscDac);
				voice[i].Wave().SetModel(is6581);
				voice[i].Wave().SetWaveformModels(waveTables);
				voice[i].Wave().SetPulldownModels(pulldownTables);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set combined waveforms strength
		/// </summary>
		/********************************************************************/
		public void SetCombinedWaveforms(CombinedWaveforms cws)
		{
			switch (cws)
			{
				case CombinedWaveforms.AVERAGE:
				case CombinedWaveforms.STRONG:
				case CombinedWaveforms.WEAK:
					break;

				default:
					throw new SidErrorException(Resources.IDS_SID_ERR_UNKNOWN_COMBINED_WAVEFORMS);
			}

			this.cws = cws;

			// Rebuild waveform related tables
			matrix_t pulldownTables = WaveformCalculator.GetInstance().BuildPulldownTable(model, cws);

			for (int i = 0; i < 3; i++)
				voice[i].Wave().SetPulldownModels(pulldownTables);
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			for (int i = 0; i < 3; i++)
				voice[i].Reset();

			filter6581.Reset();
			filter8580.Reset();
			externalFilter.Reset();

			if (resampler != null)
				resampler.Reset();

			busValue = 0;
			busValueTtl = 0;
			VoiceSync(false);
		}



		/********************************************************************/
		/// <summary>
		/// 16-bit input (EXT IN). Write 16-bit sample to audio input.
		/// NB! The caller is responsible for keeping the value within 16
		/// bits. Note that to mix in an external audio signal, the signal
		/// should be resampled to 1MHz first to avoid sampling noise
		/// </summary>
		/********************************************************************/
		public void Input(int value)
		{
			filter6581.Input((short)value);
			filter8580.Input((short)value);
		}



		/********************************************************************/
		/// <summary>
		/// Read registers.
		///
		/// Reading a write only register returns the last char written to
		/// any SID register. The individual bits in this value start to fade
		/// down towards zero after a few cycles. All bits reach zero within
		/// approximately $2000 - $4000 cycles. It has been claimed that this
		/// fading happens in an orderly fashion, however sampling of write
		/// only registers reveals that this is not the case.
		/// NOTE: This is not correctly modeled.
		/// The actual use of write only registers has largely been made in
		/// the belief that all SID registers are readable. To support this
		/// belief the read would have to be done immediately after a write
		/// to the same register (remember that an intermediate write to
		/// another register would yield that value instead). With this in
		/// mind we return the last value written to any SID register for
		/// $2000 cycles without modeling the bit fading
		/// </summary>
		/********************************************************************/
		public byte Read(int offset)
		{
			switch (offset)
			{
				// X value of paddle
				case 0x19:
				{
					busValue = potX.ReadPot();
					busValueTtl = modelTtl;
					break;
				}

				// Y value of paddle
				case 0x1a:
				{
					busValue = potY.ReadPot();
					busValueTtl = modelTtl;
					break;
				}

				// Voice #3 waveform output
				case 0x1b:
				{
					busValue = voice[2].Wave().ReadOsc();
					busValueTtl = modelTtl;
					break;
				}

				// Voice #3 ADSR output
				case 0x1c:
				{
					busValue = voice[2].Envelope().ReadEnv();
					busValueTtl = modelTtl;
					break;
				}

				default:
				{
					// Reading from a write-only or non-existing register
					// makes the bus discharge faster.
					// Emulate this by halving the residual TTL
					busValueTtl /= 2;
					break;
				}
			}

			return busValue;
		}



		/********************************************************************/
		/// <summary>
		/// Write registers
		/// </summary>
		/********************************************************************/
		public void Write(int offset, byte value)
		{
			busValue = value;
			busValueTtl = modelTtl;

			switch (offset)
			{
				// Voice #1 frequency (low-byte)
				case 0x00:
				{
					voice[0].Wave().WriteFreq_Lo(value);
					break;
				}

				// Voice #1 frequency (high-byte)
				case 0x01:
				{
					voice[0].Wave().WriteFreq_Hi(value);
					break;
				}

				// Voice #1 pulse width (low-byte)
				case 0x02:
				{
					voice[0].Wave().WritePw_Lo(value);
					break;
				}

				// Voice #1 pulse width (bits #8-#15)
				case 0x03:
				{
					voice[0].Wave().WritePw_Hi(value);
					break;
				}

				// Voice #1 control register
				case 0x04:
				{
					voice[0].WriteControl_Reg(value);
					break;
				}

				// Voice #1 attack and decay length
				case 0x05:
				{
					voice[0].Envelope().WriteAttack_Decay(value);
					break;
				}

				// Voice #1 sustain volume and release length
				case 0x06:
				{
					voice[0].Envelope().WriteSustain_Release(value);
					break;
				}

				// Voice #2 frequency (low-byte)
				case 0x07:
				{
					voice[1].Wave().WriteFreq_Lo(value);
					break;
				}

				// Voice #2 frequency (high-byte)
				case 0x08:
				{
					voice[1].Wave().WriteFreq_Hi(value);
					break;
				}

				// Voice #2 pulse width (low-byte)
				case 0x09:
				{
					voice[1].Wave().WritePw_Lo(value);
					break;
				}

				// Voice #2 pulse width (bits #8-#15)
				case 0x0a:
				{
					voice[1].Wave().WritePw_Hi(value);
					break;
				}

				// Voice #2 control register
				case 0x0b:
				{
					voice[1].WriteControl_Reg(value);
					break;
				}

				// Voice #2 attack and decay length
				case 0x0c:
				{
					voice[1].Envelope().WriteAttack_Decay(value);
					break;
				}

				// Voice #2 sustain volume and release length
				case 0x0d:
				{
					voice[1].Envelope().WriteSustain_Release(value);
					break;
				}

				// Voice #3 frequency (low-byte)
				case 0x0e:
				{
					voice[2].Wave().WriteFreq_Lo(value);
					break;
				}

				// Voice #3 frequency (high-byte)
				case 0x0f:
				{
					voice[2].Wave().WriteFreq_Hi(value);
					break;
				}

				// Voice #3 pulse width (low-byte)
				case 0x10:
				{
					voice[2].Wave().WritePw_Lo(value);
					break;
				}

				// Voice #3 pulse width (bits #8-#15)
				case 0x11:
				{
					voice[2].Wave().WritePw_Hi(value);
					break;
				}

				// Voice #3 control register
				case 0x12:
				{
					voice[2].WriteControl_Reg(value);
					break;
				}

				// Voice #3 attack and decay length
				case 0x13:
				{
					voice[2].Envelope().WriteAttack_Decay(value);
					break;
				}

				// Voice #3 sustain volume and release length
				case 0x14:
				{
					voice[2].Envelope().WriteSustain_Release(value);
					break;
				}

				// Filter cut off frequency (bits #0-#2)
				case 0x15:
				{
					filter6581.WriteFc_Lo(value);
					filter8580.WriteFc_Lo(value);
					break;
				}

				// Filter cut off frequency (bits #3-#10)
				case 0x16:
				{
					filter6581.WriteFc_Hi(value);
					filter8580.WriteFc_Hi(value);
					break;
				}

				// Filter control
				case 0x17:
				{
					filter6581.WriteRes_Filt(value);
					filter8580.WriteRes_Filt(value);
					break;
				}

				// Volume and filter modes
				case 0x18:
				{
					filter6581.WriteMode_Vol(value);
					filter8580.WriteMode_Vol(value);
					break;
				}
			}

			// Update voice sync just in case
			VoiceSync(false);
		}



		/********************************************************************/
		/// <summary>
		/// Clock SID forward using chosen output sampling algorithm
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Clock(uint cycles, short[] buf, int bufPos)
		{
			AgeBusValue(cycles);
			int s = bufPos;

			while (cycles != 0)
			{
				uint delta_t = Math.Min(nextVoiceSync, cycles);

				if (delta_t > 0)
				{
					for (uint i = 0; i < delta_t; i++)
					{
						// Clock waveform generators
						voice[0].Wave().Clock();
						voice[1].Wave().Clock();
						voice[2].Wave().Clock();

						// Clock envelope generators
						voice[0].Envelope().Clock();
						voice[1].Envelope().Clock();
						voice[2].Envelope().Clock();

						int sidOutput = filter.Clock(voice[0], voice[1], voice[2]);
						int c64Output = externalFilter.Clock(sidOutput - (1 << 15));

						if (resampler.Input(c64Output))
							buf[s++] = resampler.GetOutput(scaleFactor);
					}

					cycles -= delta_t;
					nextVoiceSync -= delta_t;
				}

				if (nextVoiceSync == 0)
					VoiceSync(true);
			}

			return s - bufPos;
		}



		/********************************************************************/
		/// <summary>
		/// Setting of SID sampling parameters.
		///
		/// Use a clock freqency of 985248Hz for PAL C64, 1022730Hz for
		/// NTSC C64. The default end of passband frequency is
		/// pass_freq = 0.9*sample_freq/2 for sample frequencies up to
		/// ~ 44.1kHz, and 20kHz for higher sample frequencies.
		///
		/// For resampling, the ratio between the clock frequency and the
		/// sample frequency is limited as follows:
		/// 125*clock_freq/sample_freq &lt; 16384
		/// E.g. provided a clock frequency of ~ 1MHz, the sample frequency
		/// can not be set lower than ~ 8kHz. A lower sample frequency would
		/// make the resampling code overfill its 16k sample ring buffer.
		///
		/// The end of passband frequency is also limited:
		/// pass_freq &lt;= 0.9*sample_freq/2
		///
		/// E.g. for a 44.1kHz sampling rate the end of passband frequency
		/// is limited to slightly below 20kHz.
		/// This constraint ensures that the FIR table is not overfilled
		/// </summary>
		/********************************************************************/
		public void SetSamplingParameters(double clockFrequency, SamplingMethod method, double samplingFrequency)
		{
			externalFilter.SetClockFrequency(clockFrequency);

			switch (method)
			{
				case SamplingMethod.DECIMATE:
				{
					resampler = new ZeroOrderResampler(clockFrequency, samplingFrequency);
					break;
				}

				case SamplingMethod.RESAMPLE:
				{
					resampler = TwoPassSincResampler.Create(clockFrequency, samplingFrequency);
					break;
				}

				default:
					throw new SidErrorException(Resources.IDS_SID_ERR_UNKNOWN_SAMPLING_METHOD);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate the number of cycles according to current parameters
		/// that it takes to reach sync
		/// </summary>
		/********************************************************************/
		private void VoiceSync(bool sync)
		{
			if (sync)
			{
				// Synchronize the 3 waveform generators
				for (int i = 0; i < 3; i++)
					voice[i].Wave().Synchronize();
			}

			// Calculate the time to next voice sync
			nextVoiceSync = int.MaxValue;

			for (int i = 0; i < 3; i++)
			{
				WaveformGenerator wave = voice[i].Wave();
				uint freq = wave.ReadFreq();

				if (wave.ReadTest() || (freq == 0) || !voice[i].Wave().ReadFollowingVoiceSync())
					continue;

				uint accumulator = wave.ReadAccumulator();
				uint thisVoiceSync = ((0x7fffff - accumulator) & 0xffffff) / freq + 1;

				if (thisVoiceSync < nextVoiceSync)
					nextVoiceSync = thisVoiceSync;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Age the bus value and zero it if it's TTL has expired
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AgeBusValue(uint n)
		{
			if (busValueTtl != 0)
			{
				busValueTtl -= (int)n;

				if (busValueTtl <= 0)
				{
					busValue = 0;
					busValueTtl = 0;
				}
			}
		}
		#endregion
	}
}
