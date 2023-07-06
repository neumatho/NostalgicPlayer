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
		/// <summary>
		/// Current volume amplifier setting
		/// </summary>
		protected ushort[] currentGain;

		/// <summary>
		/// Current filter/voice mixer setting
		/// </summary>
		protected ushort[] currentMixer;

		/// <summary>
		/// Filter input summer setting
		/// </summary>
		protected ushort[] currentSummer;

		/// <summary>
		/// Filter resonance value
		/// </summary>
		protected ushort[] currentResonance;

		/// <summary>
		/// Filter highpass state
		/// </summary>
		protected int vhp;

		/// <summary>
		/// Filter bandpass state
		/// </summary>
		protected int vbp;

		/// <summary>
		/// Filter lowpass state
		/// </summary>
		protected int vlp;

		/// <summary>
		/// Filter external input
		/// </summary>
		protected int ve;

		/// <summary>
		/// Filter cutoff frequency
		/// </summary>
		protected uint fc;

		/// <summary>
		/// Routing to filter or outside filter
		/// </summary>
		protected bool filt1, filt2, filt3, filtE;

		/// <summary>
		/// Switch voice 3 off
		/// </summary>
		protected bool voice3Off;

		/// <summary>
		/// Highpass, bandpass, and lowpass filter modes
		/// </summary>
		protected bool hp, bp, lp;

		/// <summary>
		/// Current volume
		/// </summary>
		protected byte vol;

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
		protected Filter()
		{
			currentGain = null;
			currentMixer = null;
			currentSummer = null;
			currentResonance = null;
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
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter
		/// </summary>
		/********************************************************************/
		public void EnableFilter(bool enable)
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
		/// Write frequency cutoff low register
		/// </summary>
		/********************************************************************/
		public void WriteFc_Lo(byte fc_lo)
		{
			fc = (fc & 0x7f8) | ((uint)fc_lo & 0x007);
			UpdatedCenterFrequency();
		}



		/********************************************************************/
		/// <summary>
		/// Write frequency cutoff high register
		/// </summary>
		/********************************************************************/
		public void WriteFc_Hi(byte fc_hi)
		{
			fc = ((uint)fc_hi << 3 & 0x7f8) | (fc & 0x007);
			UpdatedCenterFrequency();
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

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Set filter cutoff frequency
		/// </summary>
		/********************************************************************/
		protected abstract void UpdatedCenterFrequency();



		/********************************************************************/
		/// <summary>
		/// Set filter resonance
		/// </summary>
		/********************************************************************/
		protected abstract void UpdateResonance(byte res);



		/********************************************************************/
		/// <summary>
		/// Mixing configuration modified (offsets change)
		/// </summary>
		/********************************************************************/
		protected abstract void UpdateMixing();



		/********************************************************************/
		/// <summary>
		/// SID clocking - 1 cycle
		/// </summary>
		/********************************************************************/
		public abstract ushort Clock(int v1, int v2, int v3);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void Input(int input);
		#endregion
	}
}
