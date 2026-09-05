/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Configuration of sound levels, pan laws, etc... for various mix configurations
	/// </summary>
	internal class CSoundFilePlayConfig
	{
		protected c_float m_IntToFloat;
		protected c_float m_FloatToInt;
		protected c_float m_VstiAttenuation;
		protected c_float m_VstiVolume;

		protected c_float m_NormalSamplePreAmp;
		protected c_float m_NormalVstiVol;
		protected c_float m_NormalGlobalVol;

		protected c_int m_ExtraAttenuation;
		protected PanningMode m_ForceSoftPanning;
		protected bool m_GlobalVolumeAppliesToMaster;
		protected bool m_IgnorePreAmp;
		protected bool m_DisplayDbValues;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CSoundFilePlayConfig()
		{
			SetVstiVolume(1.0f);
			SetMixLevels(MixLevels.Compatible);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetGlobalVolumeAppliesToMaster()
		{
			return m_GlobalVolumeAppliesToMaster;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetGlobalVolumeAppliesToMaster(bool inGlobalVolumeAppliesToMaster)
		{
			m_GlobalVolumeAppliesToMaster = inGlobalVolumeAppliesToMaster;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetVstiVolume(c_float inVstiVolume)
		{
			m_VstiVolume = inVstiVolume;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetVstiAttenuation(c_float inVstiAttenuation)
		{
			m_VstiAttenuation = inVstiAttenuation;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetIntToFloat(c_float inIntToFloat)
		{
			m_IntToFloat = inIntToFloat;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetFloatToInt(c_float inFloatToInt)
		{
			m_FloatToInt = inFloatToInt;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetUseGlobalPreAmp()
		{
			return m_IgnorePreAmp;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUseGlobalPreAmp(bool inUseGlobalPreAmp)
		{
			m_IgnorePreAmp = inUseGlobalPreAmp;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PanningMode GetPanningMode()
		{
			return m_ForceSoftPanning;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetPanningMode(PanningMode inForceSoftPanning)
		{
			m_ForceSoftPanning = inForceSoftPanning;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDisplayDbValues(bool @in)
		{
			m_DisplayDbValues = @in;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetNormalSamplePreAmp(c_float @in)
		{
			m_NormalSamplePreAmp = @in;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetNormalVstiVol(c_float @in)
		{
			m_NormalVstiVol = @in;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetNormalGlobalVol(c_float @in)
		{
			m_NormalGlobalVol = @in;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_int GetExtraSampleAttenuation()
		{
			return m_ExtraAttenuation;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetExtraSampleAttenuation(c_int attn)
		{
			m_ExtraAttenuation = attn;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetMixLevels(MixLevels mixLevelType)
		{
			switch (mixLevelType)
			{
				// Olivier's version gives us floats in [-0.5; 0.5] and slightly saturates VSTis
				case MixLevels.Original:
				{
					SetVstiAttenuation(1.0f);	// No Attenuation
					SetIntToFloat(1.0f / (1 << 28));
					SetFloatToInt((c_float)(1 << 28));
					SetGlobalVolumeAppliesToMaster(false);
					SetUseGlobalPreAmp(true);
					SetPanningMode(PanningMode.Undetermined);
					SetDisplayDbValues(false);
					SetNormalSamplePreAmp(256.0f);
					SetNormalVstiVol(100.0f);
					SetNormalGlobalVol(128.0f);
					SetExtraSampleAttenuation(Mixer.Mixing_Attentuation);
					break;
				}

				// Ericus' version gives us floats in [-0.06;0.06] and requires attenuation to
				// avoid massive VSTi saturation.
				case MixLevels.v1_17RC1:
				{
					SetVstiAttenuation(32.0f);
					SetIntToFloat(1.0f / 0x07fffffff);
					SetFloatToInt(0x07fffffff);
					SetGlobalVolumeAppliesToMaster(false);
					SetUseGlobalPreAmp(true);
					SetPanningMode(PanningMode.Undetermined);
					SetDisplayDbValues(false);
					SetNormalSamplePreAmp(256.0f);
					SetNormalVstiVol(100.0f);
					SetNormalGlobalVol(128.0f);
					SetExtraSampleAttenuation(Mixer.Mixing_Attentuation);
					break;
				}

				// 1.17RC2 gives us floats in [-1.0; 1.0] and hopefully plays VSTis at
				// the right volume... but we attenuate by 2x to approx. match sample volume
				case MixLevels.v1_17RC2:
				{
					SetVstiAttenuation(2.0f);
					SetIntToFloat(1.0f / Mixer.Mixing_ScaleF);
					SetFloatToInt(Mixer.Mixing_ScaleF);
					SetGlobalVolumeAppliesToMaster(true);
					SetUseGlobalPreAmp(true);
					SetPanningMode(PanningMode.Undetermined);
					SetDisplayDbValues(false);
					SetNormalSamplePreAmp(256.0f);
					SetNormalVstiVol(100.0f);
					SetNormalGlobalVol(128.0f);
					SetExtraSampleAttenuation(Mixer.Mixing_Attentuation);
					break;
				}

				// 1.17RC3 ignores the horrible global, system-specific pre-amp,
				// treats panning as balance to avoid saturation on loud sample (and because I think it's better :),
				// and allows display of attenuation in decibels
				default:
				case MixLevels.v1_17RC3:
				{
					SetVstiAttenuation(1.0f);
					SetIntToFloat(1.0f / Mixer.Mixing_ScaleF);
					SetFloatToInt(Mixer.Mixing_ScaleF);
					SetGlobalVolumeAppliesToMaster(true);
					SetUseGlobalPreAmp(false);
					SetPanningMode(PanningMode.SoftPanning);
					SetDisplayDbValues(true);
					SetNormalSamplePreAmp(128.0f);
					SetNormalVstiVol(128.0f);
					SetNormalGlobalVol(256.0f);
					SetExtraSampleAttenuation(0);
					break;
				}

				// A mixmode that is intended to be compatible to legacy trackers (IT/FT2/etc).
				// This is basically derived from mixmode 1.17 RC3, with panning mode and volume levels changed.
				// Sample attenuation is the same as in Schism Tracker (more attenuation than with RC3, thus VSTi attenuation is also higher)
				case MixLevels.Compatible:
				case MixLevels.CompatibleFT2:
				{
					SetVstiAttenuation(0.75f);
					SetIntToFloat(1.0f / Mixer.Mixing_ScaleF);
					SetFloatToInt(Mixer.Mixing_ScaleF);
					SetGlobalVolumeAppliesToMaster(true);
					SetUseGlobalPreAmp(false);
					SetPanningMode(mixLevelType == MixLevels.Compatible ? PanningMode.NoSoftPanning : PanningMode.Ft2Panning);
					SetDisplayDbValues(true);
					SetNormalSamplePreAmp(mixLevelType == MixLevels.Compatible ? 256.0f : 192.0f);
					SetNormalVstiVol(mixLevelType == MixLevels.Compatible ? 256.0f : 192.0f);
					SetNormalGlobalVol(256.0f);
					SetExtraSampleAttenuation(1);
					break;
				}
			}
		}
	}
}
