/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
{
	/// <summary>
	/// SID filter base class
	/// </summary>
	internal abstract class Filter
	{
		private readonly FilterModelConfig fmc;

		private readonly ushort[][] mixer;
		private readonly ushort[][] summer;
		private readonly ushort[][] resonance;
		private readonly ushort[][] volume;

		/// <summary>
		/// VCR + associated capacitor connected to highpass output
		/// </summary>
		protected Integrator hpIntegrator;

		/// <summary>
		/// VCR + associated capacitor connected to bandpass output
		/// </summary>
		protected Integrator bpIntegrator;

		/// <summary>
		/// Current filter/voice mixer setting
		/// </summary>
		private ushort[] currentMixer;

		/// <summary>
		/// Filter input summer setting
		/// </summary>
		private ushort[] currentSummer;

		/// <summary>
		/// Filter resonance value
		/// </summary>
		private ushort[] currentResonance;

		/// <summary>
		/// Current volume amplifier setting
		/// </summary>
		private ushort[] currentVolume;

		/// <summary>
		/// Filter highpass state
		/// </summary>
		private int vhp;

		/// <summary>
		/// Filter bandpass state
		/// </summary>
		private int vbp;

		/// <summary>
		/// Filter lowpass state
		/// </summary>
		private int vlp;

		/// <summary>
		/// Filter external input
		/// </summary>
		private int ve;

		/// <summary>
		/// Filter cutoff frequency
		/// </summary>
		private uint fc;

		/// <summary>
		/// Routing to filter or outside filter
		/// </summary>
		private bool filt1, filt2, filt3, filtE;

		/// <summary>
		/// Switch voice 3 off
		/// </summary>
		private bool voice3Off;

		/// <summary>
		/// Highpass, bandpass, and lowpass filter modes
		/// </summary>
		private bool hp, bp, lp;

		/// <summary>
		/// Current volume
		/// </summary>
		private byte vol;

		/// <summary>
		/// Filter enabled
		/// </summary>
		private bool enabled;

		/// <summary>
		/// Selects which inputs to route through filter
		/// </summary>
		private byte filt;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Filter(FilterModelConfig fmc)
		{
			this.fmc = fmc;
			mixer = fmc.GetMixer();
			summer = fmc.GetSummer();
			resonance = fmc.GetResonance();
			volume = fmc.GetVolume();
			hpIntegrator = fmc.BuildIntegrator();
			bpIntegrator = fmc.BuildIntegrator();
			currentMixer = null;
			currentSummer = null;
			currentResonance = null;
			currentVolume = null;
			vhp = 0;
			vbp = 0;
			vlp = 0;
			ve = 0;
			fc = 0;
			filt1 = false;
			filt2 = false;
			filt3 = false;
			filtE = false;
			voice3Off = false;
			hp = false;
			bp = false;
			lp = false;
			vol = 0;
			enabled = true;
			filt = 0;

			Input(0);
		}



		/********************************************************************/
		/// <summary>
		/// Write frequency cutoff low register
		/// </summary>
		/********************************************************************/
		public void WriteFc_Lo(byte fc_lo)
		{
			fc = (fc & 0x7f8) | ((uint)fc_lo & 0x007);
			UpdateCenterFrequency();
		}



		/********************************************************************/
		/// <summary>
		/// Write frequency cutoff high register
		/// </summary>
		/********************************************************************/
		public void WriteFc_Hi(byte fc_hi)
		{
			fc = ((uint)fc_hi << 3 & 0x7f8) | (fc & 0x007);
			UpdateCenterFrequency();
		}



		/********************************************************************/
		/// <summary>
		/// Write resonance/filter register
		/// </summary>
		/********************************************************************/
		public void WriteRes_Filt(byte res_filt)
		{
			filt = res_filt;

			UpdateResonance((byte)((res_filt >> 4) & 0x0f));

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
		public void WriteMode_Vol(byte mode_vol)
		{
			vol = (byte)(mode_vol & 0x0f);
			lp = (mode_vol & 0x10) != 0;
			bp = (mode_vol & 0x20) != 0;
			hp = (mode_vol & 0x40) != 0;
			voice3Off = (mode_vol & 0x80) != 0;

			UpdateMixing();
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

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Update filter cutoff frequency
		/// </summary>
		/********************************************************************/
		protected abstract void UpdateCenterFrequency();



		/********************************************************************/
		/// <summary>
		/// SID clocking - 1 cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort Clock(float voice1, float voice2, float voice3)
		{
			int v1 = fmc.GetNormalizedVoice(voice1);
			int v2 = fmc.GetNormalizedVoice(voice2);

			// Voice 3 is silenced by voice3Off if it is not routed through the filter
			int v3 = (filt3 || !voice3Off) ? fmc.GetNormalizedVoice(voice3) : 0;

			int vSum = 0;
			int vMix = 0;

			if (filt1)
				vSum += v1;
			else
				vMix += v1;

			if (filt2)
				vSum += v2;
			else
				vMix += v2;

			if (filt3)
				vSum += v3;
			else
				vMix += v3;

			if (filtE)
				vSum += ve;
			else
				vMix += ve;

			vhp = currentSummer[currentResonance[vbp] + vlp + vSum];
			vbp = hpIntegrator.Solve(vhp);
			vlp = bpIntegrator.Solve(vbp);

			if (lp)
				vMix += vlp;

			if (bp)
				vMix += vbp;

			if (hp)
				vMix += vhp;

			return currentVolume[currentMixer[vMix]];
		}



		/********************************************************************/
		/// <summary>
		/// Apply a signal to EXT-IN
		/// </summary>
		/********************************************************************/
		public void Input(int input)
		{
			ve = fmc.GetNormalizedVoice(input / 65536.0f);
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Get the filter cutoff register value
		/// </summary>
		/********************************************************************/
		protected int GetFc()
		{
			return (int)fc;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Update filter resonance
		/// </summary>
		/********************************************************************/
		private void UpdateResonance(byte res)
		{
			currentResonance = resonance[res];
		}



		/********************************************************************/
		/// <summary>
		/// Mixing configuration modified (offsets change)
		/// </summary>
		/********************************************************************/
		private void UpdateMixing()
		{
			currentVolume = volume[vol];

			uint nSum = 0;
			uint nMix = 0;

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

			currentSummer = summer[nSum];

			if (lp)
				nMix++;

			if (bp)
				nMix++;

			if (hp)
				nMix++;

			currentMixer = mixer[nMix];
		}
		#endregion
	}
}
