/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.ReSidFp
{
	/// <summary>
	/// SID filter base class
	/// </summary>
	internal abstract class Filter
	{
		private readonly ushort[][] mixer;
		private readonly ushort[][] summer;
		private readonly ushort[][] resonance;
		private readonly ushort[][] volume;

		/// <summary>
		/// 
		/// </summary>
		protected readonly FilterModelConfig fmc;

		/// <summary>
		/// Current filter/voice mixer setting
		/// </summary>
		protected ushort[] currentMixer = null;

		/// <summary>
		/// Filter input summer setting
		/// </summary>
		protected ushort[] currentSummer = null;

		/// <summary>
		/// Filter resonance value
		/// </summary>
		protected ushort[] currentResonance = null;

		/// <summary>
		/// Current volume amplifier setting
		/// </summary>
		protected ushort[] currentVolume = null;

		/// <summary>
		/// Filter highpass state
		/// </summary>
		protected int vhp = 0;

		/// <summary>
		/// Filter bandpass state
		/// </summary>
		protected int vbp = 0;

		/// <summary>
		/// Filter lowpass state
		/// </summary>
		protected int vlp = 0;

		/// <summary>
		/// Filter external input
		/// </summary>
		protected int ve = 0;

		/// <summary>
		/// Filter cutoff frequency
		/// </summary>
		private uint fc = 0;

		/// <summary>
		/// Routing to filter or outside filter
		/// </summary>
		protected bool filt1 = false;
		protected bool filt2 = false;
		protected bool filt3 = false;
		protected bool filtE = false;

		/// <summary>
		/// Switch voice 3 off
		/// </summary>
		protected bool voice3Off = false;

		/// <summary>
		/// Highpass, bandpass, and lowpass filter modes
		/// </summary>
		protected bool hp = false;
		protected bool bp = false;
		protected bool lp = false;

		/// <summary>
		/// Current volume
		/// </summary>
		private byte vol = 0;

		/// <summary>
		/// Filter enabled
		/// </summary>
		private bool enabled = true;

		/// <summary>
		/// Selects which inputs to route through filter
		/// </summary>
		private byte filt = 0;

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
		public abstract ushort Clock(float voice1, float voice2, float voice3);



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
