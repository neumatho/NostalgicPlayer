/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Calculation of resonant filter coefficients.
	/// Notes  : Extended filter range was introduced in MPT 1.12 and went up to 8652 Hz.
	///          MPT 1.16 upped this to the current 10670 Hz.
	///          We have no way of telling whether a file was made with MPT 1.12 or 1.16 though.
	/// </summary>
	internal partial class CSoundFile
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_float CutOffToFrequency(uint32 nCutOff, c_int envModifier)//XX 40
		{
			c_float computedCutOff = nCutOff * (uint32)(envModifier + 256);	// 0...127*512
			c_float frequency;

			if (GetType_() != ModType.Imf)
				frequency = 110.0f * CMath.pow(2.0f, 0.25f + (computedCutOff / (m_SongFlags.Test(SongFlags.ExFilterRange) ? 20.0f * 512.0f : 24.0f * 512.0f)));
			else
			{
				// EMU8000: Documentation says the cutoff is in quarter semitones, with 0x00 being 125 Hz and 0xFF being 8 kHz
				// The first half of the sentence contradicts the second, though
				frequency = 125.0f * CMath.pow(2.0f, computedCutOff * 6.0f / (127.0f * 512.0f));
			}

			OpenMpt.Limit(ref frequency, 120.0f, 20000.0f);
			OpenMpt.LimitMax(ref frequency, m_MixerSettings.gdwMixingFreq * 0.5f);

			return frequency;
		}



		/********************************************************************/
		/// <summary>
		/// Simple 2-poles resonant filter. Returns computed cutoff in range
		/// [0, 254] or -1 if filter is not applied
		/// </summary>
		/********************************************************************/
		public c_int SetupChannelFilter(ModChannel chn, bool bReset, c_int envModifier = 256)//XX 92
		{
			c_int cutOff = chn.nCutOff + chn.nCutSwing;
			c_int resonance = (chn.nResonance & 0x7f) + chn.nResSwing;

			OpenMpt.Limit(ref cutOff, 0, 127);
			OpenMpt.Limit(ref resonance, 0, 127);

			if (!m_PlayBehaviour[PlayBehaviour.MptOldSwingBehaviour])
			{
				chn.nCutOff = (uint8)cutOff;
				chn.nCutSwing = 0;
				chn.nResonance = (uint8)resonance;
				chn.nResSwing = 0;
			}

			// envModifier is in [-256, 256], so cutoff is in [0, 127 * 2] after this calculation
			c_int computedCutOff = cutOff * (envModifier + 256) / 256;

			// Filtering is only ever done in IT if either cutoff is not full or if resonance is set
			if (m_PlayBehaviour[PlayBehaviour.ItFilterBehaviour] && (resonance == 0) && (computedCutOff >= 254))
			{
				if (chn.TriggerNote)
				{
					// Z7F next to a note disables the filter, however in other cases this should not happen.
					// Test cases: filter-reset.it, filter-reset-carry.it, filter-reset-envelope.it, filter-nna.it, FilterResetPatDelay.it, FilterPortaSmpChange.it, FilterPortaSmpChange-InsMode.it
					chn.dwFlags.Reset(ChannelFlags.Chn_Filter);
				}

				return -1;
			}

			chn.dwFlags.Set(ChannelFlags.Chn_Filter);

			// 2 * damping factor
			c_float dmpFac = CMath.pow(10.0f, -resonance * ((24.0f / 128.0f) / 20.0f));
			c_float fc = CutOffToFrequency((uint32)cutOff, envModifier) * (2.0f * MathF.PI);
			c_float d, e;

			if (m_PlayBehaviour[PlayBehaviour.ItFilterBehaviour] && !m_SongFlags.Test(SongFlags.ExFilterRange))
			{
				c_float r = m_MixerSettings.gdwMixingFreq / fc;

				d = (dmpFac * r) + dmpFac - 1.0f;
				e = r * r;
			}
			else
			{
				c_float r = fc / m_MixerSettings.gdwMixingFreq;

				d = (1.0f - (2.0f * dmpFac)) * r;
				OpenMpt.LimitMax(ref d, 2.0f);

				d = ((2.0f * dmpFac) - d) / r;
				e = 1.0f / (r * r);
			}

			c_float fg = 1.0f / (1.0f + d + e);
			c_float fb0 = (d + e + e) / (1 + d + e);
			c_float fb1 = -e / (1.0f + d + e);

			mixsample_t Mpt_Filter_Convert(c_float x)
			{
				return SaturateRound.Saturate_Round<mixsample_t, c_float>(x * (1 << Mixer.Mixing_Filter_Precision));
			}

			switch (chn.nFilterMode)
			{
				case FilterMode.HighPass:
				{
					chn.nFilter_A0 = Mpt_Filter_Convert(1.0f - fg);
					chn.nFilter_B0 = Mpt_Filter_Convert(fb0);
					chn.nFilter_B1 = Mpt_Filter_Convert(fb1);
					chn.nFilter_HP = -1;
					break;
				}

				default:
				{
					chn.nFilter_A0 = Mpt_Filter_Convert(fg);
					chn.nFilter_B0 = Mpt_Filter_Convert(fb0);
					chn.nFilter_B1 = Mpt_Filter_Convert(fb1);

					if (chn.nFilter_A0 == 0)
						chn.nFilter_A0 = 1;	// Prevent silence at low filter cutoff and very high sampling rate

					chn.nFilter_HP = 0;
					break;
				}
			}

			if (bReset)
			{
				chn.nFilter_Y[0][0] = chn.nFilter_Y[0][1] = 0;
				chn.nFilter_Y[1][0] = chn.nFilter_Y[1][1] = 0;
			}

			return computedCutOff;
		}
	}
}
