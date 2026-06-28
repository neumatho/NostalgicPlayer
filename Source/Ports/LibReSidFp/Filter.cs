/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// SID filter base class
	/// </summary>
	internal abstract class Filter
	{
		private static readonly int[] summerIdx =
		[
			FilterModelConfig.Summer_Offset.Value(0),
			FilterModelConfig.Summer_Offset.Value(1),
			FilterModelConfig.Summer_Offset.Value(2),
			FilterModelConfig.Summer_Offset.Value(3),
			FilterModelConfig.Summer_Offset.Value(4)
		];

		private static readonly int[] mixerIdx =
		[
			FilterModelConfig.Mixer_Offset.Value(0),
			FilterModelConfig.Mixer_Offset.Value(1),
			FilterModelConfig.Mixer_Offset.Value(2),
			FilterModelConfig.Mixer_Offset.Value(3),
			FilterModelConfig.Mixer_Offset.Value(4),
			FilterModelConfig.Mixer_Offset.Value(5),
			FilterModelConfig.Mixer_Offset.Value(6),
			FilterModelConfig.Mixer_Offset.Value(7)
		];

		private readonly CPointer<uint16_t> mixer;
		private readonly CPointer<uint16_t> summer;
		private readonly CPointer<uint16_t> resonance;
		private readonly CPointer<uint16_t> volume;

		/// <summary>
		/// 
		/// </summary>
		private readonly FilterModelConfig m_fmc;

		/// <summary>
		/// 
		/// </summary>
		private readonly Integrator m_hpIntegrator;

		/// <summary>
		/// 
		/// </summary>
		private readonly Integrator m_bpIntegrator;

		/// <summary>
		/// Current filter/voice mixer setting
		/// </summary>
		private CPointer<uint16_t> currentMixer = null;

		/// <summary>
		/// Filter input summer setting
		/// </summary>
		private CPointer<uint16_t> currentSummer = null;

		/// <summary>
		/// Filter resonance value
		/// </summary>
		private CPointer<uint16_t> currentResonance = null;

		/// <summary>
		/// Current volume amplifier setting
		/// </summary>
		private CPointer<uint16_t> currentVolume = null;

		/// <summary>
		/// Filter highpass state
		/// </summary>
		internal int32_t vhp = 0;

		/// <summary>
		/// Filter bandpass state
		/// </summary>
		internal int32_t vbp = 0;

		/// <summary>
		/// Filter lowpass state
		/// </summary>
		internal int32_t vlp = 0;

		/// <summary>
		/// Filter external input
		/// </summary>
		internal float extIn = 0;

		/// <summary>
		/// Filter cutoff frequency
		/// </summary>
		internal uint16_t fc = 0;

		/// <summary>
		/// Routing to filter or outside filter
		/// </summary>
		internal bool filt1 = false;
		internal bool filt2 = false;
		internal bool filt3 = false;
		internal bool filtE = false;

		/// <summary>
		/// Switch voice 3 off
		/// </summary>
		internal bool voice3Off = false;

		/// <summary>
		/// Highpass, bandpass, and lowpass filter modes
		/// </summary>
		internal bool hp = false;
		internal bool bp = false;
		internal bool lp = false;

		/// <summary>
		/// Current volume
		/// </summary>
		internal uint8_t vol = 0;

		/// <summary>
		/// Filter enabled
		/// </summary>
		internal bool enabled = true;

		/// <summary>
		/// Selects which inputs to route through filter
		/// </summary>
		internal uint8_t filt = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Filter(FilterModelConfig fmc, Integrator hpIntegrator, Integrator bpIntegrator)
		{
			mixer = fmc.GetMixer();
			summer = fmc.GetSummer();
			resonance = fmc.GetResonance();
			volume = fmc.GetVolume();
			m_fmc = fmc;
			m_hpIntegrator = hpIntegrator;
			m_bpIntegrator = bpIntegrator;

			Input(0);
		}



		/********************************************************************/
		/// <summary>
		/// Write frequency cutoff low register
		/// </summary>
		/********************************************************************/
		public void WriteFc_Lo(uint8_t fc_lo)
		{
			fc = (uint16_t)((fc & 0x7f8) | (fc_lo & 0x007));
			UpdateCenterFrequency();
		}



		/********************************************************************/
		/// <summary>
		/// Write frequency cutoff high register
		/// </summary>
		/********************************************************************/
		public void WriteFc_Hi(uint8_t fc_hi)
		{
			fc = (uint16_t)(((fc_hi << 3) & 0x7f8) | (fc & 0x007));
			UpdateCenterFrequency();
		}



		/********************************************************************/
		/// <summary>
		/// Write resonance/filter register
		/// </summary>
		/********************************************************************/
		public void WriteRes_Filt(uint8_t res_filt)
		{
			filt = res_filt;

			UpdateResonance((uint8_t)((res_filt >> 4) & 0x0f));

			if (enabled)
			{
				filt1 = (filt & 0x01) != 0;
				filt2 = (filt & 0x02) != 0;
				filt3 = (filt & 0x04) != 0;
				filtE = (filt & 0x08) != 0;
			}

			UpdateMixing();
		}



		/********************************************************************/
		/// <summary>
		/// Write filter mode/volume register
		/// </summary>
		/********************************************************************/
		public void WriteMode_Vol(uint8_t mode_vol)
		{
			vol = (uint8_t)(mode_vol & 0x0f);
			lp = (mode_vol & 0x10) != 0;
			bp = (mode_vol & 0x20) != 0;
			hp = (mode_vol & 0x40) != 0;
			voice3Off = (mode_vol & 0x80) != 0;

			UpdateMixing();
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - 1 cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint16_t Clock(Voice voice1, Voice voice2, Voice voice3)
		{
			// Waveform outputs
			float wav1 = voice1.Output();
			float wav2 = voice2.Output();
			float wav3 = voice3.Output();

			// Envelope outputs
			uint8_t env1 = voice1.Envelope().Output();
			uint8_t env2 = voice2.Envelope().Output();
			uint8_t env3 = voice3.Envelope().Output();

			// Voltage summer for filter input
			int32_t Vsum = 0;
			Vsum += filt1 ? GetNormalizedVoice(wav1, env1) : 0;
			Vsum += filt2 ? GetNormalizedVoice(wav2, env2) : 0;
			Vsum += filt3 ? GetNormalizedVoice(wav3, env3) : 0;
			Vsum += filtE ? GetNormalizedVoice(extIn, 0) : 0;
			Vsum += vlp;
			Vsum += currentResonance[vbp];

			// Filter
			vhp = currentSummer[Vsum];
			vbp = m_hpIntegrator.Solve(vhp);
			vlp = m_bpIntegrator.Solve(vbp);

			int32_t Vfilt = 0;

			if (lp)
				Vfilt += vlp;

			if (bp)
				Vfilt += vbp;

			if (hp)
				Vfilt += vhp;

			// Voltage summer for mixer input
			int32_t Vmix = 0;
			Vmix += filt1 ? 0 : GetNormalizedMixerVoice(wav1, env1);
			Vmix += filt2 ? 0 : GetNormalizedMixerVoice(wav2, env2);

			// Voice 3 is silenced by voice3off if it is not routed through the filter
			Vmix += filt3 || voice3Off ? 0 : GetNormalizedMixerVoice(wav3, env3);
			Vmix += filtE ? 0 : GetNormalizedMixerVoice(extIn, 0);
			Vmix += Vfilt;

			return currentVolume[currentMixer[Vmix]];
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter
		/// </summary>
		/********************************************************************/
		public void Enable(bool enable)
		{
			enabled = enable;

			if (enabled)
				WriteRes_Filt(filt);
			else
				filt1 = filt2 = filt3 = filtE = false;
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			WriteFc_Lo(0);
			WriteFc_Hi(0);
			WriteMode_Vol(0);
			WriteRes_Filt(0);
		}



		/********************************************************************/
		/// <summary>
		/// Apply a signal to EXT-IN
		/// </summary>
		/********************************************************************/
		public void Input(int16_t input)
		{
			extIn = input / 65535.0f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Restart()
		{
			RestartIntegrators();

			vhp = 0;
			vlp = 0;
			vbp = 0;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Update filter cutoff frequency
		/// </summary>
		/********************************************************************/
		protected abstract void UpdateCenterFrequency();



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract void RestartIntegrators();



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract int32_t GetNormalizedMixerVoice(float v, uint8_t env);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Get the filter cutoff register value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected uint GetFc()
		{
			return fc;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected int32_t GetNormalizedVoice(float v, uint8_t env)
		{
			return m_fmc.GetNormalizedVoice(v, env);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Update filter resonance
		/// </summary>
		/********************************************************************/
		private void UpdateResonance(uint8_t res)
		{
			currentResonance = resonance + (res * (1 << 16));
		}



		/********************************************************************/
		/// <summary>
		/// Mixing configuration modified (offsets change)
		/// </summary>
		/********************************************************************/
		private void UpdateMixing()
		{
			currentVolume = volume + (vol * (1 << 16));

			int nSum = 0;
			int nMix = 0;

			if (filt1)
				nSum++;
			else
				nMix++;

			if (filt2)
				nSum++;
			else
				nMix++;

			if (filt3)
				nSum++;
			else if (!voice3Off)
				nMix++;

			if (filtE)
				nSum++;
			else
				nMix++;

			currentSummer = summer + summerIdx[nSum];

			if (lp)
				nMix++;

			if (bp)
				nMix++;

			if (hp)
				nMix++;

			currentMixer = mixer + mixerIdx[nMix];
		}
		#endregion
	}
}
