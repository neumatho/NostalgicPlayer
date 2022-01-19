/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Exceptions;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Resample;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp
{
	/// <summary>
	/// MOS6581/MOS8580 emulation
	/// </summary>
	internal class Sid
	{
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
		/// External filter that provides high-pass and low-pass filtering
		/// to adjust sound tone slightly
		/// </summary>
		private readonly ExternalFilter externalFilter;

		/// <summary>
		/// Resampler used by audio generation code
		/// </summary>
		private Resampler resampler;

		/// <summary>
		/// Paddle X register support
		/// </summary>
		private readonly Potentiometer potX;

		/// <summary>
		/// Paddle Y register support
		/// </summary>
		private readonly Potentiometer potY;

		/// <summary>
		/// SID voices
		/// </summary>
		private readonly Voice[] voice =  new Voice[3];

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
		/// Last written value
		/// </summary>
		private byte busValue;

		/// <summary>
		/// Flags for muted channels
		/// </summary>
		private readonly bool[] muted = new bool[3];

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
			externalFilter = new ExternalFilter();
			resampler = null;
			potX = new Potentiometer();
			potY = new Potentiometer();

			voice[0] = new Voice();
			voice[1] = new Voice();
			voice[2] = new Voice();

			muted[0] = muted[1] = muted[2] = false;

			Reset();
			SetChipModel(ChipModel.MOS8580);
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter emulation
		/// </summary>
		/********************************************************************/
		public void EnableFilter(bool enable)
		{
			filter6581.EnableFilter(enable);
			filter8580.EnableFilter(enable);
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
					modelTtl = BUS_TTL_6581;
					break;
				}

				case ChipModel.MOS8580:
				{
					filter = filter8580;
					modelTtl = BUS_TTL_8580;
					break;
				}

				default:
					throw new SidErrorException(Resources.IDS_SID_ERR_UNKNOWN_CHIP);
			}

			this.model = model;

			// Calculate waveform-related tables
			matrix_t tables = WaveformCalculator.GetInstance().BuildTable(model);

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

				double offset = dacBuilder.GetOutput(is6581 ? (uint)0x380 : 0x9c0);

				for (uint i = 0; i < (1 << (int)OSC_DAC_BITS); i++)
				{
					double dacValue = dacBuilder.GetOutput(i);
					oscDac[i] = (float)(dacValue - offset);
				}
			}

			// Set voice tables
			for (int i = 0; i < 3; i++)
			{
				voice[i].Envelope().SetDac(envDac);
				voice[i].Wave().SetDac(oscDac);
				voice[i].Wave().SetModel(is6581);
				voice[i].Wave().SetWaveformModels(tables);
			}
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
			filter6581.Input(value);
			filter8580.Input(value);
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
					voice[0].WriteControl_Reg(muted[0] ? (byte)0 : value);
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
					voice[1].WriteControl_Reg(muted[1] ? (byte)0 : value);
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
					voice[2].WriteControl_Reg(muted[2] ? (byte)0 : value);
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

						if (resampler.Input(Output()))
							buf[s++] = resampler.GetOutput();
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
		public void SetSamplingParameters(double clockFrequency, SamplingMethod method, double samplingFrequency, double highestAccurateFrequency)
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
					resampler = TwoPassSincResampler.Create(clockFrequency, samplingFrequency, highestAccurateFrequency);
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
					voice[i].Wave().Synchronize(voice[(i + 1) % 3].Wave(), voice[(i + 2) % 3].Wave());
			}

			// Calculate the time to next voice sync
			nextVoiceSync = int.MaxValue;

			for (int i = 0; i < 3; i++)
			{
				WaveformGenerator wave = voice[i].Wave();
				uint freq = wave.ReadFreq();

				if (wave.ReadTest() || (freq == 0) || !voice[(i + 1) % 3].Wave().ReadSync())
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



		/********************************************************************/
		/// <summary>
		/// Get output sample
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Output()
		{
			int v1 = voice[0].Output(voice[2].Wave());
			int v2 = voice[1].Output(voice[0].Wave());
			int v3 = voice[2].Output(voice[1].Wave());

			return externalFilter.Clock(filter.Clock(v1, v2, v3));
		}
		#endregion
	}
}
