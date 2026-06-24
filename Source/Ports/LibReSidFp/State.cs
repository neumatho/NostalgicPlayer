/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Containers;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Resample;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class State : IState
	{
		// SID
		private int bus_Value_Ttl;
		private uint nextVoiceSync;
		private ChipModel model;
		private CombinedWaveforms cws;
		private uint8_t bus_Value;
		private uint8_t paddle_X;
		private uint8_t paddle_Y;

		// Waveform
		private readonly uint32_t[] pw = new uint32_t[3];
		private readonly uint32_t[] shift_Register = new uint32_t[3];
		private readonly uint32_t[] shift_Latch = new uint32_t[3];
		private readonly uint32_t[] ring_Msb_Mask = new uint32_t[3];
		private readonly uint32_t[] no_Noise = new uint32_t[3];
		private readonly uint32_t[] noise_Output = new uint32_t[3];
		private readonly uint32_t[] no_Noise_Or_Noise_Output = new uint32_t[3];
		private readonly uint32_t[] no_Pulse = new uint32_t[3];
		private readonly uint32_t[] pulse_Output = new uint32_t[3];
		private readonly uint32_t[] waveform_Output = new uint32_t[3];
		private readonly uint32_t[] accumulator = new uint32_t[3];
		private readonly uint32_t[] freq = new uint32_t[3];
		private readonly uint32_t[] tri_Saw_Pipeline = new uint32_t[3];
		private readonly uint32_t[] osc3 = new uint32_t[3];
		private readonly int[] shift_Pipeline = new int[3];
		private readonly uint[] shift_Register_Reset = new uint[3];
		private readonly uint[] floating_Output_Ttl = new uint[3];
		private readonly bool[] test = new bool[3];
		private readonly bool[] sync = new bool[3];
		private readonly bool[] test_Or_Reset = new bool[3];
		private readonly bool[] msb_Rising = new bool[3];
		private readonly uint8_t[] waveform = new uint8_t[3];

		// Envelope
		private readonly uint16_t[] lfsr = new uint16_t[3];
		private readonly uint16_t[] rate = new uint16_t[3];
		private readonly uint[] exponential_Counter = new uint[3];
		private readonly uint[] exponential_Counter_Period = new uint[3];
		private readonly uint[] new_Exponential_Counter_Period = new uint[3];
		private readonly uint[] state_Pipeline = new uint[3];
		private readonly uint[] envelope_Pipeline = new uint[3];
		private readonly uint[] exponential_Pipeline = new uint[3];
		private readonly EnvelopeGenerator.State[] env_State = new EnvelopeGenerator.State[3];
		private readonly EnvelopeGenerator.State[] next_State = new EnvelopeGenerator.State[3];
		private readonly bool[] counter_Enabled = new bool[3];
		private readonly bool[] gate = new bool[3];
		private readonly bool[] resetLfsr = new bool[3];
		private readonly uint8_t[] envelope_Counter = new uint8_t[3];
		private readonly uint8_t[] attack = new uint8_t[3];
		private readonly uint8_t[] decay = new uint8_t[3];
		private readonly uint8_t[] sustain = new uint8_t[3];
		private readonly uint8_t[] release = new uint8_t[3];
		private readonly uint8_t[] env3 = new uint8_t[3];

		// Filter
		private readonly int32_t[] Vhp = new int32_t[2];
		private readonly int32_t[] Vbp = new int32_t[2];
		private readonly int32_t[] Vlp = new int32_t[2];
		private readonly int32_t[] Ve = new int32_t[2];
		private readonly uint8_t[] fc = new uint8_t[2];
		private readonly uint8_t[] vol = new uint8_t[2];
		private readonly uint8_t[] filt = new uint8_t[2];
		private readonly bool[] filt1 = new bool[2];
		private readonly bool[] filt2 = new bool[2];
		private readonly bool[] filt3 = new bool[2];
		private readonly bool[] filtE = new bool[2];
		private readonly bool[] voice3Off = new bool[2];
		private readonly bool[] hp = new bool[2];
		private readonly bool[] bp = new bool[2];
		private readonly bool[] lp = new bool[2];
		private readonly bool[] enabled = new bool[2];

		private double filterCurve6581;
		private double filterRange6581;
		private double filterCurve8580;
		private bool Old6581Caps;

		// Integrators
		private readonly int32_t[][] vx = ArrayHelper.Initialize2Arrays<int32_t>(2, 2);
		private readonly int32_t[][] vc = ArrayHelper.Initialize2Arrays<int32_t>(2, 2);
		private readonly uint32_t[] nVddt_Vw_2 = new uint32_t[2];
		private readonly uint16_t[] nVgt = new uint16_t[2];
		private readonly uint16_t[] n_Dac = new uint16_t[2];

		// External filter
		private int32_t exVlp;
		private int32_t exVhp;

		// Resampler
		private double clockFrequency;
		private double samplingFrequency;
		private SamplingMethod method;

		// ZeroOrder
		private int32_t zor_CachedSample;
		private int32_t zor_OutputValue;
		private int zor_SampleOffset;

		// PassThrough
		private int pt_OutputValue;

		// TwoPassSinc
		private readonly int[] tp_SampleIndex = new int[2];
		private readonly int[] tp_SampleOffset = new int[2];
		private readonly int32_t[] tp_OutputValue = new int32_t[2];
		private readonly int[][] tp_Sample = new int[2][];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static IState SaveState(Sid s)
		{
			State state = new State();

			for (int i = 0; i < 3; i++)
			{
				WaveformGenerator wave = s.voice[i].Wave();

				state.pw[i] = wave.pw;
				state.shift_Register[i] = wave.shift_register;
				state.shift_Latch[i] = wave.shift_latch;
				state.ring_Msb_Mask[i] = wave.ring_msb_mask;
				state.no_Noise[i] = wave.no_noise;
				state.noise_Output[i] = wave.noise_output;
				state.no_Noise_Or_Noise_Output[i] = wave.no_noise_or_noise_output;
				state.no_Pulse[i] = wave.no_pulse;
				state.pulse_Output[i] = wave.pulse_output;
				state.waveform_Output[i] = wave.waveform_output;
				state.accumulator[i] = wave.accumulator;
				state.freq[i] = wave.freq;
				state.tri_Saw_Pipeline[i] = wave.tri_saw_pipeline;
				state.osc3[i] = wave.osc3;
				state.shift_Pipeline[i] = wave.shift_pipeline;
				state.shift_Register_Reset[i] = wave.shift_register_reset;
				state.floating_Output_Ttl[i] = wave.floating_output_ttl;
				state.waveform[i] = wave.waveform;
				state.test[i] = wave.test;
				state.sync[i] = wave.sync;
				state.test_Or_Reset[i] = wave.test_or_reset;
				state.msb_Rising[i] = wave.msb_rising;

				EnvelopeGenerator envelope = s.voice[i].Envelope();

				state.lfsr[i] = envelope.lfsr;
				state.rate[i] = envelope.rate;
				state.exponential_Counter[i] = envelope.exponential_counter;
				state.exponential_Counter_Period[i] = envelope.exponential_counter_period;
				state.new_Exponential_Counter_Period[i] = envelope.new_exponential_counter_period;
				state.state_Pipeline[i] = envelope.state_pipeline;
				state.envelope_Pipeline[i] = envelope.envelope_pipeline;
				state.exponential_Pipeline[i] = envelope.exponential_pipeline;
				state.env_State[i] = envelope.state;
				state.next_State[i] = envelope.next_state;
				state.counter_Enabled[i] = envelope.counter_enabled;
				state.gate[i] = envelope.gate;
				state.resetLfsr[i] = envelope.resetLfsr;
				state.envelope_Counter[i] = envelope.envelope_counter;
				state.attack[i] = envelope.attack;
				state.decay[i] = envelope.decay;
				state.sustain[i] = envelope.sustain;
				state.release[i] = envelope.release;
				state.env3[i] = envelope.env3;
			}

			state.bus_Value = s.busValue;
			state.bus_Value_Ttl = s.busValueTtl;
			state.nextVoiceSync = s.nextVoiceSync;
			state.paddle_X = s.paddleX;
			state.paddle_Y = s.paddleY;
			state.model = s.model;
			state.cws = s.cws;

			for (int i = 0; i < 2; i++)
			{
				Filter f = (i == 0) ? s.filter6581 : s.filter8580;

				state.Vhp[i] = f.vhp;
				state.Vbp[i] = f.vbp;
				state.Vlp[i] = f.vlp;
				state.Ve[i] = f.ve;
				state.fc[i] = (uint8_t)f.fc;
				state.filt1[i] = f.filt1;
				state.filt2[i] = f.filt2;
				state.filt3[i] = f.filt3;
				state.filtE[i] = f.filtE;
				state.voice3Off[i] = f.voice3Off;
				state.hp[i] = f.hp;
				state.bp[i] = f.bp;
				state.lp[i] = f.lp;
				state.vol[i] = f.vol;
				state.enabled[i] = f.enabled;
				state.filt[i] = f.filt;
			}

			state.filterCurve6581 = s.p.FilterCurve6581;
			state.filterRange6581 = s.p.FilterRange6581;
			state.filterCurve8580 = s.p.FilterCurve8580;
			state.Old6581Caps = s.p.Old6581Caps;

			state.vx[0][0] = s.filter6581.hpIntegrator.vx;
			state.vx[0][1] = s.filter6581.bpIntegrator.vx;
			state.vx[1][0] = s.filter8580.hpIntegrator.vx;
			state.vx[1][1] = s.filter8580.bpIntegrator.vx;
			state.vc[0][0] = s.filter6581.hpIntegrator.vc;
			state.vc[0][1] = s.filter6581.bpIntegrator.vc;
			state.vc[1][0] = s.filter8580.hpIntegrator.vc;
			state.vc[1][1] = s.filter8580.bpIntegrator.vc;
			state.nVddt_Vw_2[0] = s.filter6581.hpIntegrator.nVddt_vw_2;
			state.nVddt_Vw_2[1] = s.filter6581.bpIntegrator.nVddt_vw_2;
			state.nVgt[0] = s.filter8580.hpIntegrator.nVgt;
			state.nVgt[1] = s.filter8580.bpIntegrator.nVgt;
			state.n_Dac[0] = s.filter8580.hpIntegrator.n_dac;
			state.n_Dac[1] = s.filter8580.bpIntegrator.n_dac;

			state.exVlp = s.externalFilter.vlp;
			state.exVhp = s.externalFilter.vhp;

			state.method = s.p.Method;
			state.clockFrequency = s.p.ClockFrequency;
			state.samplingFrequency = s.p.SamplingFrequency;

			switch (s.p.Method)
			{
				case SamplingMethod.DECIMATE:
				{
					ZeroOrderResampler zor = (ZeroOrderResampler)s.resampler;

					state.zor_CachedSample = zor.cachedSample;
					state.zor_SampleOffset = zor.sampleOffset;
					state.zor_OutputValue = zor.outputValue;
					break;
				}

				case SamplingMethod.RESAMPLE:
				{
					TwoPassSincResampler tp = (TwoPassSincResampler)s.resampler;

					for (int i = 0; i < 2; i++)
					{
						SincResampler sr = (i == 0) ? tp.s1 : tp.s2;

						state.tp_SampleIndex[i] = sr.sampleIndex;
						state.tp_SampleOffset[i] = sr.sampleOffset;
						state.tp_OutputValue[i] = sr.outputValue;
					}

					break;
				}

				case SamplingMethod.NONE:
				{
					PassThrough pt = (PassThrough)s.resampler;

					state.pt_OutputValue = pt.outputValue;
					break;
				}
			}

			if (state.method == SamplingMethod.RESAMPLE)
			{
				TwoPassSincResampler tp = (TwoPassSincResampler)s.resampler;

				for (int i = 0; i < 2; i++)
				{
					SincResampler sr = (i == 0) ? tp.s1 : tp.s2;

					state.tp_Sample[i] = new int[sr.sample.Length];
					System.Array.Copy(sr.sample, state.tp_Sample[i], sr.sample.Length);
				}
			}

			return state;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void RestoreState(Sid s, IState buffer)
		{
			State state = (State)buffer;

			s.busValue = state.bus_Value;
			s.busValueTtl = state.bus_Value_Ttl;
			s.nextVoiceSync = state.nextVoiceSync;
			s.paddleX = state.paddle_X;
			s.paddleY = state.paddle_Y;
			s.model = state.model;
			s.SetChipModel(s.model);
			s.cws = state.cws;
			s.SetCombinedWaveforms(s.cws);

			for (int i = 0; i < 2; i++)
			{
				Filter f = (i == 0) ? s.filter6581 : s.filter8580;

				f.vhp = state.Vhp[i];
				f.vbp = state.Vbp[i];
				f.vlp = state.Vlp[i];
				f.ve = state.Ve[i];
				f.fc = state.fc[i];
				f.filt1 = state.filt1[i];
				f.filt2 = state.filt2[i];
				f.filt3 = state.filt3[i];
				f.filtE = state.filtE[i];
				f.voice3Off = state.voice3Off[i];
				f.hp = state.hp[i];
				f.bp = state.bp[i];
				f.lp = state.lp[i];
				f.vol = state.vol[i];
				f.enabled = state.enabled[i];
				f.filt = state.filt[i];
			}

			s.SetFilter6581Curve(state.filterCurve6581);
			s.SetFilter6581Range(state.filterRange6581);
			s.SetFilter8580Curve(state.filterCurve8580);
			s.EnableOld6581Caps(state.Old6581Caps);

			s.filter6581.hpIntegrator.vx = state.vx[0][0];
			s.filter6581.bpIntegrator.vx = state.vx[0][1];
			s.filter8580.hpIntegrator.vx = state.vx[1][0];
			s.filter8580.bpIntegrator.vx = state.vx[1][1];
			s.filter6581.hpIntegrator.vc = state.vc[0][0];
			s.filter6581.bpIntegrator.vc = state.vc[0][1];
			s.filter8580.hpIntegrator.vc = state.vc[1][0];
			s.filter8580.bpIntegrator.vc = state.vc[1][1];
			s.filter6581.hpIntegrator.nVddt_vw_2 = state.nVddt_Vw_2[0];
			s.filter6581.bpIntegrator.nVddt_vw_2 = state.nVddt_Vw_2[1];
			s.filter8580.hpIntegrator.nVgt = state.nVgt[0];
			s.filter8580.bpIntegrator.nVgt = state.nVgt[1];
			s.filter8580.hpIntegrator.n_dac = state.n_Dac[0];
			s.filter8580.bpIntegrator.n_dac = state.n_Dac[1];

			s.externalFilter.vlp = state.exVlp;
			s.externalFilter.vhp = state.exVhp;

			s.SetSamplingParameters(state.clockFrequency, state.method, state.samplingFrequency);

			for (int i = 0; i < 3; i++)
			{
				WaveformGenerator wave = s.voice[i].Wave();

				wave.pw = state.pw[i];
				wave.shift_register = state.shift_Register[i];
				wave.shift_latch = state.shift_Latch[i];
				wave.ring_msb_mask = state.ring_Msb_Mask[i];
				wave.no_noise = state.no_Noise[i];
				wave.noise_output = state.noise_Output[i];
				wave.no_noise_or_noise_output = state.no_Noise_Or_Noise_Output[i];
				wave.no_pulse = state.no_Pulse[i];
				wave.pulse_output = state.pulse_Output[i];
				wave.waveform_output = state.waveform_Output[i];
				wave.accumulator = state.accumulator[i];
				wave.freq = state.freq[i];
				wave.tri_saw_pipeline = state.tri_Saw_Pipeline[i];
				wave.osc3 = state.osc3[i];
				wave.shift_pipeline = state.shift_Pipeline[i];
				wave.shift_register_reset = state.shift_Register_Reset[i];
				wave.floating_output_ttl = state.floating_Output_Ttl[i];
				wave.waveform = state.waveform[i];
				wave.test = state.test[i];
				wave.sync = state.sync[i];
				wave.test_or_reset = state.test_Or_Reset[i];
				wave.msb_rising = state.msb_Rising[i];

				wave.SetWave();
				wave.SetPulldown();

				EnvelopeGenerator envelope = s.voice[i].Envelope();

				envelope.lfsr = state.lfsr[i];
				envelope.rate = state.rate[i];
				envelope.exponential_counter = state.exponential_Counter[i];
				envelope.exponential_counter_period = state.exponential_Counter_Period[i];
				envelope.new_exponential_counter_period = state.new_Exponential_Counter_Period[i];
				envelope.state_pipeline = state.state_Pipeline[i];
				envelope.envelope_pipeline = state.envelope_Pipeline[i];
				envelope.exponential_pipeline = state.exponential_Pipeline[i];
				envelope.state = state.env_State[i];
				envelope.next_state = state.next_State[i];
				envelope.counter_enabled = state.counter_Enabled[i];
				envelope.gate = state.gate[i];
				envelope.resetLfsr = state.resetLfsr[i];
				envelope.envelope_counter = state.envelope_Counter[i];
				envelope.attack = state.attack[i];
				envelope.decay = state.decay[i];
				envelope.sustain = state.sustain[i];
				envelope.release = state.release[i];
				envelope.env3 = state.env3[i];
			}

			switch (s.p.Method)
			{
				case SamplingMethod.DECIMATE:
				{
					ZeroOrderResampler zor = (ZeroOrderResampler)s.resampler;

					zor.cachedSample = state.zor_CachedSample;
					zor.sampleOffset = state.zor_SampleOffset;
					zor.outputValue = state.zor_OutputValue;
					break;
				}

				case SamplingMethod.RESAMPLE:
				{
					TwoPassSincResampler tp = (TwoPassSincResampler)s.resampler;

					for (int i = 0; i < 2; i++)
					{
						SincResampler sr = (i == 0) ? tp.s1 : tp.s2;

						sr.sampleIndex = state.tp_SampleIndex[i];
						sr.sampleOffset = state.tp_SampleOffset[i];
						sr.outputValue = state.tp_OutputValue[i];
					}

					break;
				}

				case SamplingMethod.NONE:
				{
					PassThrough pt = (PassThrough)s.resampler;

					pt.outputValue = state.pt_OutputValue;
					break;
				}
			}

			if (state.method == SamplingMethod.RESAMPLE)
			{
				TwoPassSincResampler tp = (TwoPassSincResampler)s.resampler;

				for (int i = 0; i < 2; i++)
				{
					SincResampler sr = (i == 0) ? tp.s1 : tp.s2;

					System.Array.Copy(state.tp_Sample[i], sr.sample,  sr.sample.Length);
				}
			}
		}
	}
}
