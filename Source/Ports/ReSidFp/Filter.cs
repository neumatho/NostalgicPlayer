/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
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

		private readonly CPointer<ushort> mixer;
		private readonly CPointer<ushort> summer;
		private readonly CPointer<ushort> resonance;
		private readonly CPointer<ushort> volume;

		/// <summary>
		/// 
		/// </summary>
		private readonly FilterModelConfig fmc;

		/// <summary>
		/// Current filter/voice mixer setting
		/// </summary>
		private CPointer<ushort> currentMixer = null;

		/// <summary>
		/// Filter input summer setting
		/// </summary>
		private CPointer<ushort> currentSummer = null;

		/// <summary>
		/// Filter resonance value
		/// </summary>
		private CPointer<ushort> currentResonance = null;

		/// <summary>
		/// Current volume amplifier setting
		/// </summary>
		private CPointer<ushort> currentVolume = null;

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
		private int ve = 0;

		/// <summary>
		/// Filter cutoff frequency
		/// </summary>
		private uint fc = 0;

		/// <summary>
		/// Routing to filter or outside filter
		/// </summary>
		private bool filt1 = false;
		private bool filt2 = false;
		private bool filt3 = false;
		private bool filtE = false;

		/// <summary>
		/// Switch voice 3 off
		/// </summary>
		private bool voice3Off = false;

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
			mixer = fmc.GetMixer();
			summer = fmc.GetSummer();
			resonance = fmc.GetResonance();
			volume = fmc.GetVolume();
			this.fmc = fmc;

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
		/// SID clocking - 1 cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort Clock(Voice voice1, Voice voice2, Voice voice3)
		{
			int V1 = GetNormalizedVoice(voice1);
			int V2 = GetNormalizedVoice(voice2);

			// Voice 3 is silenced by voice3off if it is not routed through the filter
			int V3 = (filt3 || !voice3Off) ? GetNormalizedVoice(voice3) : 0;

			int Vsum = 0;
			int Vmix = 0;

			if (filt1)
				Vsum += V1;
			else
				Vmix += V1;

			if (filt2)
				Vsum += V2;
			else
				Vmix += V2;

			if (filt3)
				Vsum += V3;
			else
				Vmix += V3;

			if (filtE)
				Vsum += ve;
			else
				Vmix += ve;

			vhp = currentSummer[currentResonance[vbp] + vlp + Vsum];

			Vmix += SolveIntegrators();

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

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Update filter cutoff frequency
		/// </summary>
		/********************************************************************/
		protected abstract void UpdateCenterFrequency();



		/********************************************************************/
		/// <summary>
		/// Update filter cutoff frequency
		/// </summary>
		/********************************************************************/
		protected abstract int SolveIntegrators();



		/********************************************************************/
		/// <summary>
		/// Apply a signal to EXT-IN
		/// </summary>
		/********************************************************************/
		public void Input(short input)
		{
			ve = fmc.GetNormalizedVoice(input / 32768.0f, 0);
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Get the filter cutoff register value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected int GetFc()
		{
			return (int)fc;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int GetNormalizedVoice(Voice v)
		{
			return fmc.GetNormalizedVoice(v.Output(), v.Envelope().Output());
		}



		/********************************************************************/
		/// <summary>
		/// Update filter resonance
		/// </summary>
		/********************************************************************/
		private void UpdateResonance(byte res)
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
