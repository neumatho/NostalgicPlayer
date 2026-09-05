/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundDsp.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundDsp
{
	/// <summary>
	/// Mixing code for reverb
	/// </summary>
	internal class CReverb
	{
		// Length-1 (in samples) of the reflections delay buffer: 32K, 371ms@22kHz
		public const c_int SndMix_Reflections_Delay_Mask = 0x1fff;
		public const c_int SndMix_PreDiffusion_Delay_Mask = 0x7f;		// 128 samples
		public const c_int SndMix_Reverb_Delay_Mask = 0xfff;			// 4K samples (92ms @ 44kHz)

		// Late reverberation
		// Tank diffusers lengths
		public const c_int RvbDif1L_Len = 149 * 2;		// 6.8ms
		public const c_int RvbDif1R_Len = 223 * 2;		// 10.1ms
		public const c_int RvbDif2L_Len = 421 * 2;		// 19.1ms
		public const c_int RvbDif2R_Len = 647 * 2;		// 29.3ms

		// Tank delay lines lengths
		public const c_int RvbDly1L_Len = 683 * 2;		// 30.9ms
		public const c_int RvbDly1R_Len = 811 * 2;		// 36.7ms
		public const c_int RvbDly2L_Len = 773 * 2;		// 35.1ms
		public const c_int RvbDly2R_Len = 1013 * 2;		// 45.9ms

		// Tank delay lines mask
		public const c_int RvbDly_Mask = 2047;

		// Min/Max reflections delay
		public const c_int RvbMinRefDelay = 96;		// 96 samples
		public const c_int RvbMaxRefDelay = 7500;	// 7500 samples

		// Min/Max reverb delay
		public const c_int RvbMinRvbDelay = 128;	// 256 samples (11.6ms @ 22kHz)
		public const c_int RvbMaxRvbDelay = 3800;	// 1900 samples (86ms @ 24kHz)

		public const c_int Num_ReverbTypes = 29;
		public const c_int Environment_NumReflections = 8;

		private const c_int Dcr_Amount = 9;

		public readonly CReverbSettings m_Settings = new CReverbSettings();
		private SndMixReverbProperties m_CurrentPreset = null;

		private bool gnReverbSend = false;

		private uint32 gnReverbSamples = 0;
		private uint32 gnReverbDecaySamples = 0;

		// Internal reverb state
//XX		private bool g_bLastInPresent = false;
//XX		private bool g_bLastOutPresent = false;
//XX		private c_int g_nLastRvbIn_xl = 0;
//XX		private c_int g_nLastRvbIn_xr = 0;
		private c_int g_nLastRvbIn_yl = 0;
		private c_int g_nLastRvbIn_yr = 0;
//XX		private c_int g_nLastRvbOut_xl = 0;
//XX		private c_int g_nLastRvbOut_xr = 0;
		private readonly int32[] gnDCRRvb_Y1 = new int32[2];
		private readonly int32[] gnDCRRvb_X1 = new int32[2];

		// Reverb mix buffers
		private readonly SwRvbRefDelay g_RefDelay;
		private readonly SwLateReverb g_LateReverb;

		private static readonly pair<SndMixReverbProperties, string>[] ReverbPresets =
		[
			// Examples simulating General MIDI 2'musical' reverb presets
			// Name  (Decay time)  Description
			// Plate       (1.3s)  A plate reverb simulation.
			pair.make_pair(new SndMixReverbProperties(-1000, -200, 1.30f, 0.90f, 0, 0.002f, 0, 0.010f, 100.0f, 75.0f), "GM Plate"),

			// Small Room  (1.1s)  A small size room with a length of 5m or so.
			pair.make_pair(new SndMixReverbProperties(-1000, -600, 1.10f, 0.83f, -400, 0.005f, 500, 0.010f, 100.0f, 100.0f), "GM Small Room"),

			// Medium Room (1.3s)  A medium size room with a length of 10m or so.
			pair.make_pair(new SndMixReverbProperties(-1000, -600, 1.30f, 0.83f, -1000, 0.010f, -200, 0.020f, 100.0f, 100.0f), "GM Medium Room"),
			
			// Large Room  (1.5s)  A large size room suitable for live performances.
			pair.make_pair(new SndMixReverbProperties(-1000, -600, 1.50f, 0.83f, -1600, 0.020f, -1000, 0.040f, 100.0f, 100.0f), "GM Large Room"),

			// Medium Hall (1.8s)  A medium size concert hall.
			pair.make_pair(new SndMixReverbProperties(-1000, -600, 1.80f, 0.70f, -1300, 0.015f, -800, 0.030f, 100.0f, 100.0f), "GM Medium Hall"),

			// Large Hall  (1.8s)  A large size concert hall suitable for a full orchestra.
			pair.make_pair(new SndMixReverbProperties(-1000, -600, 1.80f, 0.70f, -2000, 0.030f, -1400, 0.060f, 100.0f, 100.0f), "GM Large Hall"),

			pair.make_pair(new SndMixReverbProperties(-1000, -100, 1.49f, 0.83f, -2602, 0.007f, 200, 0.011f, 100.0f, 100.0f), "Generic"),
			pair.make_pair(new SndMixReverbProperties(-1000, -6000, 0.17f, 0.10f, -1204, 0.001f, 207, 0.002f, 100.0f, 100.0f), "Padded Cell"),
			pair.make_pair(new SndMixReverbProperties(-1000, -454, 0.40f, 0.83f, -1646, 0.002f, 53, 0.003f, 100.0f, 100.0f), "Room"),
			pair.make_pair(new SndMixReverbProperties(-1000, -1200, 1.49f, 0.54f, -370, 0.007f, 1030, 0.011f, 100.0f, 60.0f), "Bathroom"),
			pair.make_pair(new SndMixReverbProperties(-1000, -6000, 0.50f, 0.10f, -1376, 0.003f, -1104, 0.004f, 100.0f, 100.0f), "Living Room"),
			pair.make_pair(new SndMixReverbProperties(-1000, -300, 2.31f, 0.64f, -711, 0.012f, 83, 0.017f, 100.0f, 100.0f), "Stone Room"),
			pair.make_pair(new SndMixReverbProperties(-1000, -476, 4.32f, 0.59f, -789, 0.020f, -289, 0.030f, 100.0f, 100.0f), "Auditorium"),
			pair.make_pair(new SndMixReverbProperties(-1000, -500, 3.92f, 0.70f, -1230, 0.020f, -2, 0.029f, 100.0f, 100.0f), "Concert Hall"),
			pair.make_pair(new SndMixReverbProperties(-1000, 0, 2.91f, 1.30f, -602, 0.015f, -302, 0.022f, 100.0f, 100.0f), "Cave"),
			pair.make_pair(new SndMixReverbProperties(-1000, -698, 7.24f, 0.33f, -1166, 0.020f, 16, 0.030f, 100.0f, 100.0f), "Arena"),
			pair.make_pair(new SndMixReverbProperties(-1000, -1000, 10.05f, 0.23f, -602, 0.020f, 198, 0.030f, 100.0f, 100.0f), "Hangar"),
			pair.make_pair(new SndMixReverbProperties(-1000, -4000, 0.30f, 0.10f, -1831, 0.002f, -1630, 0.030f, 100.0f, 100.0f), "Carpeted Hallway"),
			pair.make_pair(new SndMixReverbProperties(-1000, -300, 1.49f, 0.59f, -1219, 0.007f, 441, 0.011f, 100.0f, 100.0f), "Hallway"),
			pair.make_pair(new SndMixReverbProperties(-1000, -237, 2.70f, 0.79f, -1214, 0.013f, 395, 0.020f, 100.0f, 100.0f), "Stone Corridor"),
			pair.make_pair(new SndMixReverbProperties(-1000, -270, 1.49f, 0.86f, -1204, 0.007f, -4, 0.011f, 100.0f, 100.0f), "Alley"),
			pair.make_pair(new SndMixReverbProperties(-1000, -3300, 1.49f, 0.54f, -2560, 0.162f, -613, 0.088f, 79.0f, 100.0f), "Forest"),
			pair.make_pair(new SndMixReverbProperties(-1000, -800, 1.49f, 0.67f, -2273, 0.007f, -2217, 0.011f, 50.0f, 100.0f), "City"),
			pair.make_pair(new SndMixReverbProperties(-1000, -2500, 1.49f, 0.21f, -2780, 0.300f, -2014, 0.100f, 27.0f, 100.0f), "Mountains"),
			pair.make_pair(new SndMixReverbProperties(-1000, -1000, 1.49f, 0.83f, -10000, 0.061f, 500, 0.025f, 100.0f, 100.0f), "Quarry"),
			pair.make_pair(new SndMixReverbProperties(-1000, -2000, 1.49f, 0.50f, -2466, 0.179f, -2514, 0.100f, 21.0f, 100.0f), "Plain"),
			pair.make_pair(new SndMixReverbProperties(-1000, 0, 1.65f, 1.50f, -1363, 0.008f, -1153, 0.012f, 100.0f, 100.0f), "Parking Lot"),
			pair.make_pair(new SndMixReverbProperties(-1000, -1000, 2.81f, 0.14f, 429, 0.014f, 648, 0.021f, 80.0f, 60.0f), "Sewer Pipe"),
			pair.make_pair(new SndMixReverbProperties(-1000, -4000, 1.49f, 0.10f, -449, 0.007f, 1700, 0.011f, 100.0f, 100.0f), "Underwater")
		];

		private class ReflectionPreset
		{
			public ReflectionPreset(int32 lDelayFactor, int16 sGainLL, int16 sGainRR, int16 sGainLR, int16 sGainRL)
			{
				this.lDelayFactor = lDelayFactor;
				this.sGainLL = sGainLL;
				this.sGainRR = sGainRR;
				this.sGainLR = sGainLR;
				this.sGainRL = sGainRL;
			}

			public int32 lDelayFactor { get; }
			public int16 sGainLL { get; }
			public int16 sGainRR { get; }
			public int16 sGainLR { get; }
			public int16 sGainRL { get; }
		}

		private static readonly ReflectionPreset[] gReflectionsPreset =
		[
			new ReflectionPreset(0, 9830, 6554, 0, 0),
			new ReflectionPreset(10, 6554, 13107, 0, 0),
			new ReflectionPreset(24, -9830, 13107, 0, 0),
			new ReflectionPreset(36, 13107, -6554, 0, 0),
			new ReflectionPreset(54, 16384, 16384, -1638, -1638),
			new ReflectionPreset(61, -13107, 8192, -328, -328),
			new ReflectionPreset(73, -11468, -11468, -3277, 3277),
			new ReflectionPreset(87, 13107, -9830, 4916, -4916)
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CReverb()
		{
			// Reverb mix buffers
			g_RefDelay = new SwRvbRefDelay();
			g_LateReverb = new SwLateReverb();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Initialize(bool bReset, ref MixSampleInt gnRvbROfsVol, ref MixSampleInt gnRvbLOfsVol, uint32 mixingFreq)//XX 279
		{
			if (m_Settings.m_nReverbType >= Num_ReverbTypes)
				m_Settings.m_nReverbType = 0;

			SndMixReverbProperties rvbPreset = ReverbPresets[m_Settings.m_nReverbType].first;

			if (!ReferenceEquals(rvbPreset, m_CurrentPreset) || bReset)
			{
				// Reverb output frequency is half of the dry output rate
				c_float flOutputFrequency = mixingFreq;

				// Reset reverb parameters
				m_CurrentPreset = rvbPreset;

				I3dl2_To_Generic(rvbPreset, out EnvironmentReverb rvb, flOutputFrequency, RvbMinRefDelay, RvbMaxRefDelay, RvbMinRvbDelay, RvbMaxRvbDelay, (RvbDif1L_Len + RvbDif1R_Len + RvbDif2L_Len + RvbDif2R_Len + RvbDly1L_Len + RvbDly1R_Len + RvbDly2L_Len + RvbDly2R_Len) / 2);

				// Store reverb decay time (in samples) for reverb auto-shutdown
				gnReverbDecaySamples = (uint32)rvb.ReverbDecaySamples;

				// Room attenuation at high frequencies
				int32 nRoomLP = OnePoleLowPassCoef(32768, mBToLinear(rvb.RoomHF), 5000, flOutputFrequency);
				g_RefDelay.nCoeffs.C.L = (int16)nRoomLP;
				g_RefDelay.nCoeffs.C.R = (int16)nRoomLP;

				// Pre-Diffusion factor (for both reflections and late reverb)
				g_RefDelay.nPreDifCoeffs.C.L = (int16)(rvb.PreDiffusion * 2);
				g_RefDelay.nPreDifCoeffs.C.R = (int16)(rvb.PreDiffusion * 2);

				// Setup individual reflections delay and gains
				for (uint32 iRef = 0; iRef < 8; iRef++)
				{
					SwRvbReflection @ref = g_RefDelay.Reflections[iRef];

					@ref.DelayDest = rvb.Reflections[iRef].Delay;
					@ref.Delay = @ref.DelayDest;

					@ref.Gains[0].C.L = rvb.Reflections[iRef].GainLL;
					@ref.Gains[0].C.R = rvb.Reflections[iRef].GainRL;
					@ref.Gains[1].C.L = rvb.Reflections[iRef].GainLR;
					@ref.Gains[1].C.R = rvb.Reflections[iRef].GainRR;
				}

				g_LateReverb.nReverbDelay = rvb.ReverbDelay;

				// Reflections Master Gain
				uint32 lReflectionsGain = 0;

				if (rvb.ReflectionsLevel > -9000)
					lReflectionsGain = (uint32)mBToLinear(32768, rvb.ReflectionsLevel);

				g_RefDelay.lMasterGain = (int32)lReflectionsGain;

				// Late reverb master gain
				uint32 lReverbGain = 0;

				if (rvb.ReverbLevel > -9000)
					lReverbGain = (uint32)mBToLinear(32768, rvb.ReverbLevel);

				g_LateReverb.lMasterGain = (int32)lReverbGain;

				// Late reverb diffusion
				uint32 nTailDiffusion = (uint32)rvb.TankDiffusion;

				if (nTailDiffusion > 0x7f00)
					nTailDiffusion = 0x7f00;

				g_LateReverb.nDifCoeffs[0].C.L = (int16)nTailDiffusion;
				g_LateReverb.nDifCoeffs[0].C.R = (int16)nTailDiffusion;
				g_LateReverb.nDifCoeffs[1].C.L = (int16)nTailDiffusion;
				g_LateReverb.nDifCoeffs[1].C.R = (int16)nTailDiffusion;
				g_LateReverb.Dif2InGains[0].C.L = 0x7000;
				g_LateReverb.Dif2InGains[0].C.R = 0x1000;
				g_LateReverb.Dif2InGains[1].C.L = 0x1000;
				g_LateReverb.Dif2InGains[1].C.R = 0x7000;

				// Late reverb decay time
				int32 nReverbDecay = (int32_t)rvb.ReverbDecay;
				OpenMpt.Limit(ref nReverbDecay, 0, 0x7ff0);

				g_LateReverb.nDecayDC[0].C.L = (int16)nReverbDecay;
				g_LateReverb.nDecayDC[0].C.R = 0;
				g_LateReverb.nDecayDC[1].C.L = 0;
				g_LateReverb.nDecayDC[1].C.R = (int16)nReverbDecay;

				// Late Reverb Decay HF
				c_float fReverbDamping = rvb.flReverbDamping * rvb.flReverbDamping;
				int32 nDampingLowPass = OnePoleLowPassCoef(32768, fReverbDamping, 5000, flOutputFrequency);
				OpenMpt.Limit(ref nDampingLowPass, 0x100, 0x7f00);

				g_LateReverb.nDecayLP[0].C.L = (int16)nDampingLowPass;
				g_LateReverb.nDecayLP[0].C.R = 0;
				g_LateReverb.nDecayLP[1].C.L = 0;
				g_LateReverb.nDecayLP[1].C.R = (int16)nDampingLowPass;
			}

			if (bReset)
			{
				gnReverbSamples = 0;
				Shutdown(ref gnRvbROfsVol, ref gnRvbLOfsVol);
			}

			// Wait at least 5 seconds before shutting down the reverb
			if (gnReverbDecaySamples < (mixingFreq * 5))
				gnReverbDecaySamples = mixingFreq * 5;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void TouchReverbSendBuffer(CPointer<MixSampleInt> mixReverbBuffer, ref MixSampleInt gnRvbROfsVol, ref MixSampleInt gnRvbLOfsVol, uint32 nSamples)//XX 387
		{
			if (!gnReverbSend)
			{	// And we did not clear the buffer yet, do it now because we will get new data
				MixerLoops.StereoFill(mixReverbBuffer, nSamples, ref gnRvbROfsVol, ref gnRvbLOfsVol);
			}

			gnReverbSend = true;	// We will have to process reverb
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Process(CPointer<MixSampleInt> mixSoundBuffer, CPointer<MixSampleInt> mixReverbBuffer, ref MixSampleInt gnRvbROfsVol, ref MixSampleInt gnRvbLOfsVol, uint32 nSamples)//XX 398
		{
			if (!gnReverbSend && (gnReverbSamples == 0))
			{
				// No data is sent to reverb and reverb decayed completely
				return;
			}

			if (!gnReverbSend)
			{
				// No input data in MixReverbBuffer, so the buffer got not cleared in TouchReverbSendBuffer(), do it now for decay
				MixerLoops.StereoFill(mixReverbBuffer, nSamples, ref gnRvbROfsVol, ref gnRvbLOfsVol);
			}

			// Dynamically adjust reverb master gains
			int32 lMasterGain = (int32)((g_RefDelay.lMasterGain * m_Settings.m_nReverbDepth) >> 4);

			if (lMasterGain > 0x7fff)
				lMasterGain = 0x7fff;

			g_RefDelay.ReflectionsGain.C.L = (int16)lMasterGain;
			g_RefDelay.ReflectionsGain.C.R = (int16)lMasterGain;

			lMasterGain = (int32)((g_LateReverb.lMasterGain * m_Settings.m_nReverbDepth) >> 4);

			if (lMasterGain > 0x10000)
				lMasterGain = 0x10000;

			g_LateReverb.RvbOutGains[0].C.L = (int16)((lMasterGain + 0x7f) >> 3);	// l->l
			g_LateReverb.RvbOutGains[0].C.R = (int16)((lMasterGain + 0xff) >> 4);	// r->l
			g_LateReverb.RvbOutGains[1].C.L = (int16)((lMasterGain + 0xff) >> 4);	// l->r
			g_LateReverb.RvbOutGains[1].C.R = (int16)((lMasterGain + 0x7f) >> 3);	// r->r

			// Process Dry/Wet Mix
			int32 lMaxRvbGain = (g_RefDelay.lMasterGain > g_LateReverb.lMasterGain) ? g_RefDelay.lMasterGain : g_LateReverb.lMasterGain;

			if (lMaxRvbGain > 32768)
				lMaxRvbGain = 32768;

			int32 lDryVol = (int32)((36 - m_Settings.m_nReverbDepth) >> 1);

			if (lDryVol < 8)
				lDryVol = 8;

			if (lDryVol > 16)
				lDryVol = 16;

			lDryVol = 16 - (((16 - lDryVol) * lMaxRvbGain) >> 15);

			ReverbDryMix(mixSoundBuffer, mixReverbBuffer, lDryVol, nSamples);

			// Downsample 2x + 1st stage of lowpass filter
			uint32 nIn = ReverbProcessPreFiltering1x(mixReverbBuffer, nSamples);
			uint32 nOut = nIn;

			// Main reverb processing: split into small chunks (needed for short reverb delays)
			// Reverb Input + Low-Pass stage #2 + Pre-diffusion
			if (nIn > 0)
				ProcessPreDelay(g_RefDelay, mixReverbBuffer, nIn);

			// Process Reverb Reflections and Late Reverberation
			CPointer<int32> pRvbOut = mixReverbBuffer;
			uint32 nRvbSamples = nOut;

			while (nRvbSamples > 0)
			{
				uint32 nPosRef = g_RefDelay.nRefOutPos & SndMix_Reverb_Delay_Mask;
				uint32 nPosRvb = (nPosRef - g_LateReverb.nReverbDelay) & SndMix_Reverb_Delay_Mask;
				uint32 nMax1 = (SndMix_Reverb_Delay_Mask + 1) - nPosRef;
				uint32 nMax2 = (SndMix_Reverb_Delay_Mask + 1) - nPosRvb;
				nMax1 = nMax1 < nMax2 ? nMax1 : nMax2;
				uint32 n = nRvbSamples;

				if (n > nMax1)
					n = nMax1;

				if (n > 64)
					n = 64;

				// Reflections output + late reverb delay
				ProcessReflections(g_RefDelay, new CPointer<LR16>(g_RefDelay.RefOut, (int)nPosRef), pRvbOut, n);

				// Late Reverberation
				ProcessLateReverb(g_LateReverb, new CPointer<LR16>(g_RefDelay.RefOut, (int)nPosRvb), pRvbOut, n);

				// Update delay positions
				g_RefDelay.nRefOutPos = (g_RefDelay.nRefOutPos + n) & SndMix_Reverb_Delay_Mask;
				g_RefDelay.nDelayPos = (g_RefDelay.nDelayPos + n) & SndMix_Reflections_Delay_Mask;

				pRvbOut += n * 2;
				nRvbSamples -= n;
			}

			// Adjust nDelayPos, in case nIn != nOut
			g_RefDelay.nDelayPos = (g_RefDelay.nDelayPos - nOut + nIn) & SndMix_Reflections_Delay_Mask;

			// Upsample 2x
			ReverbProcessPostFiltering1x(mixReverbBuffer, mixSoundBuffer, nSamples);

			// Automatically shut down if needed
			if (gnReverbSend)
				gnReverbSamples = gnReverbDecaySamples;	// Reset decay counter
			else if (gnReverbSamples > nSamples)
				gnReverbSamples -= nSamples;		// Decay
			else
			{
				Shutdown(ref gnRvbROfsVol, ref gnRvbLOfsVol);
				gnReverbSamples = 0;
			}

			gnReverbSend = false;	// No input data in MixReverbBuffer
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int32 OnePoleLowPassCoef(int32 scale, c_double g, c_double f_c, c_double f_s)//XX 57
		{
			if (g > 0.999999)
				return 0;

			g *= g;
			c_double scale_Over_1mg = scale / (1.0 - g);
			c_double cosw = CMath.cos((2.0 * Math.PI) * f_c / f_s);

			return SaturateRound.Saturate_Round<int32, c_double>((1.0 - (CMath.sqrt(((g + g) * (1.0 - cosw)) - (g * g * (1.0 - (cosw * cosw)))) + (g * cosw))) * scale_Over_1mg);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_double mBToLinear(int32 value_mB)//XX 67
		{
			if (value_mB == 0)
				return 1;

			if (value_mB <= -100000)
				return 0;

			c_double val = value_mB * 3.321928094887362304 / (100.0 * 20.0);	// log2(10)/(100*20)

			return CMath.pow(2.0, val - (int32)(0.5 + val));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int32 mBToLinear(int32 scale, int32 value_mB)//XX 76
		{
			return SaturateRound.Saturate_Round<int32, c_double>(mBToLinear(value_mB) * scale);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int32 ftol(c_float f)//XX 162
		{
			return (int32)f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void I3dl2_To_Generic(SndMixReverbProperties pReverb, out EnvironmentReverb pRvb, c_float flOutputFreq, int32 lMinRefDelay, int32 lMaxRefDelay, int32 lMinRvbDelay, int32 lMaxRvbDelay, int32 lTankLength)//XX 164
		{
			pRvb = new EnvironmentReverb();

			// Common parameters
			pRvb.ReverbLevel = pReverb.lReverb;
			pRvb.ReflectionsLevel = pReverb.lReflections;
			pRvb.RoomHF = pReverb.lRoomHF;

			// HACK: Somewhat normalize the reverb output level
			int32 lMaxLevel = (pRvb.ReverbLevel > pRvb.ReflectionsLevel) ? pRvb.ReverbLevel : pRvb.ReflectionsLevel;

			if (lMaxLevel < -600)
			{
				lMaxLevel += 600;
				pRvb.ReverbLevel -= lMaxLevel;
				pRvb.ReflectionsLevel -= lMaxLevel;
			}

			// Pre-Diffusion factor (for both reflections and late reverb)
			int32 lDensity = 8192 + ftol(79.31f * pReverb.flDensity);
			pRvb.PreDiffusion = lDensity;

			// Late reverb diffusion
			int32 lTailDiffusion = ftol((0.15f + (pReverb.flDiffusion * (0.36f * 0.01f))) * 32767.0f);

			if (lTailDiffusion > 0x7f00)
				lTailDiffusion = 0x7f00;

			pRvb.TankDiffusion = lTailDiffusion;

			// Verify reflections and reverb delay parameters
			c_float flRefDelay = pReverb.flReflectionsDelay;

			if (flRefDelay > 0.100f)
				flRefDelay = 0.100f;

			int32 lReverbDelay = ftol(pReverb.flReverbDelay * flOutputFreq);
			int32 lReflectionsDelay = ftol(flRefDelay * flOutputFreq);
			int32 lReverbDecayTime = ftol(pReverb.flDecayTime * flOutputFreq);

			if (lReflectionsDelay < lMinRefDelay)
			{
				lReverbDelay -= (lMinRefDelay - lReflectionsDelay);
				lReflectionsDelay = lMinRefDelay;
			}

			if (lReflectionsDelay > lMaxRefDelay)
			{
				lReverbDelay += (lReflectionsDelay - lMaxRefDelay);
				lReflectionsDelay = lMaxRefDelay;
			}

			// Adjust decay time when adjusting reverb delay
			if (lReverbDelay < lMinRvbDelay)
			{
				lReverbDecayTime -= (lMinRvbDelay - lReverbDelay);
				lReverbDelay = lMinRvbDelay;
			}

			if (lReverbDelay > lMaxRvbDelay)
			{
				lReverbDecayTime += (lReverbDelay - lMaxRvbDelay);
				lReverbDelay = lMaxRvbDelay;
			}

			pRvb.ReverbDelay = (uint32)lReverbDelay;
			pRvb.ReverbDecaySamples = lReverbDecayTime;

			// Setup individual reflections delay and gains
			for (uint32 iRef = 0; iRef < Environment_NumReflections; iRef++)
			{
				EnvironmentReflection @ref = pRvb.Reflections[iRef];

				@ref.Delay = (uint32)(lReflectionsDelay + (((gReflectionsPreset[iRef].lDelayFactor * lReverbDelay) + 50) / 100));
				@ref.GainLL = gReflectionsPreset[iRef].sGainLL;
				@ref.GainRL = gReflectionsPreset[iRef].sGainRL;
				@ref.GainLR = gReflectionsPreset[iRef].sGainLR;
				@ref.GainRR = gReflectionsPreset[iRef].sGainRR;
			}

			// Late reverb decay time
			if (lTankLength < 10)
				lTankLength = 10;

			c_float flDelayFactor = (lReverbDecayTime <= lTankLength) ? 1.0f : ((c_float)lTankLength / lReverbDecayTime);
			pRvb.ReverbDecay = (uint32)ftol(CMath.pow(0.001f, flDelayFactor) * 32768.0f);

			// Late Reverb Decay HF
			c_float flDecayTimeHF = lReverbDecayTime * pReverb.flDecayHFRatio;
			c_float flDelayFactorHF = (flDecayTimeHF <= lTankLength) ? 1.0f : lTankLength / flDecayTimeHF;
			pRvb.flReverbDamping = CMath.pow(0.001f, flDelayFactorHF);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Shutdown(ref MixSampleInt gnRvbROfsVol, ref MixSampleInt gnRvbLOfsVol)//XX 252
		{
			gnReverbSend = false;

			gnRvbLOfsVol = 0;
			gnRvbROfsVol = 0;

			// Clear out all reverb state
//XX			g_bLastInPresent = false;
//XX			g_bLastOutPresent = false;
//XX			g_nLastRvbIn_xl = g_nLastRvbIn_xr = 0;
			g_nLastRvbIn_yl = g_nLastRvbIn_yr = 0;
//XX			g_nLastRvbOut_xl = g_nLastRvbOut_xr = 0;
			OpenMpt.MemsetZero(gnDCRRvb_X1);
			OpenMpt.MemsetZero(gnDCRRvb_Y1);

			// Zero internal buffers
			OpenMpt.MemsetZero(g_LateReverb.Diffusion1);
			OpenMpt.MemsetZero(g_LateReverb.Diffusion2);
			OpenMpt.MemsetZero(g_LateReverb.Delay1);
			OpenMpt.MemsetZero(g_LateReverb.Delay2);
			OpenMpt.MemsetZero(g_RefDelay.RefDelayBuffer);
			OpenMpt.MemsetZero(g_RefDelay.PreDifBuffer);
			OpenMpt.MemsetZero(g_RefDelay.RefOut);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ReverbDryMix(CPointer<int32> pDry, CPointer<int32> pWet, c_int lDryVol, uint32 nSamples)//XX 475
		{
			for (uint32 i = 0; i < nSamples; i++)
			{
				pDry[i * 2] += (pWet[i * 2] >> 4) * lDryVol;
				pDry[(i * 2) + 1] += (pWet[(i * 2) + 1] >> 4) * lDryVol;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32 ReverbProcessPreFiltering1x(CPointer<int32> pWet, uint32 nSamples)//XX 534
		{
			c_int lowpass = g_RefDelay.nCoeffs.C.L;
			c_int y1_l = g_nLastRvbIn_yl, y1_r = g_nLastRvbIn_yr;

			for (uint32 i = 0; i < nSamples; i++)
			{
				c_int x_l = pWet[i * 2] >> 12;
				c_int x_r = pWet[(i * 2) + 1] >> 12;

				y1_l = x_l + (((x_l - y1_l) * lowpass) >> 15);
				y1_r = x_r + (((x_r - y1_r) * lowpass) >> 15);

				pWet[i * 2] = y1_l;
				pWet[(i * 2) + 1] = y1_r;
			}

			g_nLastRvbIn_yl = y1_l;
			g_nLastRvbIn_yr = y1_r;

			return nSamples;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ReverbProcessPostFiltering1x(CPointer<int32> pRvb, CPointer<int32> pDry, uint32 nSamples)//XX 595
		{
			int32 x1L = gnDCRRvb_X1[0], x1R = gnDCRRvb_X1[1];
			int32 y1L = gnDCRRvb_Y1[0], y1R = gnDCRRvb_Y1[1];
			int32 inL = 0, inR = 0;

			while (nSamples-- != 0)
			{
				inL = pRvb[0];
				inR = pRvb[1];
				pRvb += 2;
				int32 outL = pDry[0], outR = pDry[1];

				// x(n-1) - x(n)
				x1L -= inL;
				x1R -= inR;
				x1L = x1L / (1 << (Dcr_Amount + 1)) - x1L;
				x1R = x1R / (1 << (Dcr_Amount + 1)) - x1R;
				y1L += x1L;
				y1R += x1R;

				// Add to dry mix
				outL += y1L;
				outR += y1R;
				y1L -= y1L / (1 << Dcr_Amount);
				y1R -= y1R / (1 << Dcr_Amount);
				x1L = inL;
				x1R = inR;

				pDry[0] = outL;
				pDry[1] = outR;
				pDry += 2;
			}

			gnDCRRvb_Y1[0] = y1L;
			gnDCRRvb_Y1[1] = y1R;
			gnDCRRvb_X1[0] = inL;
			gnDCRRvb_X1[1] = inR;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int32 Clamp16(int32 x)//XX 720
		{
			return OpenMpt.Clamp<int32, int32>(x, int16.MinValue, int16.MaxValue);
		}



		/********************************************************************/
		/// <summary>
		/// Pre-Delay:
		///
		/// 1. Saturate and low-pass the reverb input (stage 2 of roomHF)
		/// 2. Process pre-diffusion
		/// 3. Insert the result in the reflections delay buffer
		/// </summary>
		/********************************************************************/
		private void ProcessPreDelay(SwRvbRefDelay pPreDelay, CPointer<int32> pIn, uint32 nSamples)//XX 722
		{
			uint32 preDifPos = pPreDelay.nPreDifPos;
			uint32 delayPos = pPreDelay.nDelayPos - 1;
			int32 coeffsL = pPreDelay.nCoeffs.C.L, coeffsR = pPreDelay.nCoeffs.C.R;
			int32 preDifCoeffsL = pPreDelay.nPreDifCoeffs.C.L, preDifCoeffsR = pPreDelay.nPreDifCoeffs.C.R;
			int16 historyL = pPreDelay.History.C.L, historyR = pPreDelay.History.C.R;

			while (nSamples-- != 0)
			{
				int32 inL = Clamp16(pIn[0]);
				int32 inR = Clamp16(pIn[1]);
				pIn += 2;

				// Low-pass
				int32 lpL = (Clamp16(historyL - inL) * coeffsL) / 65536;
				int32 lpR = (Clamp16(historyR - inR) * coeffsR) / 65536;
				historyL = int16.CreateSaturating(Clamp16(lpL + lpL) + inL);
				historyR = int16.CreateSaturating(Clamp16(lpR + lpR) + inR);

				// Pre-Diffusion
				int32 preDifL = pPreDelay.PreDifBuffer[preDifPos].C.L;
				int32 preDifR = pPreDelay.PreDifBuffer[preDifPos].C.R;
				preDifPos = (preDifPos + 1) & SndMix_PreDiffusion_Delay_Mask;
				delayPos = (delayPos + 1) & SndMix_Reflections_Delay_Mask;
				int16 preDif2L = int16.CreateSaturating(historyL - (preDifL * preDifCoeffsL / 65536));
				int16 preDif2R = int16.CreateSaturating(historyR - (preDifR * preDifCoeffsR / 65536));
				pPreDelay.PreDifBuffer[preDifPos].C.L = preDif2L;
				pPreDelay.PreDifBuffer[preDifPos].C.R = preDif2R;
				pPreDelay.RefDelayBuffer[delayPos].C.L = int16.CreateSaturating((preDifCoeffsL * preDif2L / 65536) + preDifL);
				pPreDelay.RefDelayBuffer[delayPos].C.R = int16.CreateSaturating((preDifCoeffsR * preDif2R / 65536) + preDifR);
			}

			pPreDelay.nPreDifPos = preDifPos;
			pPreDelay.History.C.L = historyL;
			pPreDelay.History.C.R = historyR;
		}



		/********************************************************************/
		/// <summary>
		/// ProcessReflections:
		/// First stage:
		///  - process 4 reflections, output to pRefOut
		///  - output results to pRefOut
		/// Second stage:
		///  - process another 3 reflections
		///  - sum with pRefOut
		///  - apply reflections master gain and accumulate in the given
		///    output
		/// </summary>
		/********************************************************************/
		private void ProcessReflections(SwRvbRefDelay pPreDelay, CPointer<LR16> pRefOut, CPointer<int32> pOut, uint32 nSamples)//XX 797
		{
			c_int[] pos = new c_int[7];

			for (c_int i = 0; i < 7; i++)
				pos[i] = (c_int)(pPreDelay.nDelayPos - pPreDelay.Reflections[i].Delay - 1);

			// For 28-bit final output: 16+15-3 = 28
			int16 refGain = (int16)(pPreDelay.ReflectionsGain.C.L / (1 << 3));

			while (nSamples-- != 0)
			{
				// First stage
				int32 refOutL = 0, refOutR = 0;

				for (c_int i = 0; i < 4; i++)
				{
					pos[i] = (pos[i] + 1) & SndMix_Reflections_Delay_Mask;
					int16 refL = pPreDelay.RefDelayBuffer[pos[i]].C.L, refR = pPreDelay.RefDelayBuffer[pos[i]].C.R;
					refOutL += (refL * pPreDelay.Reflections[i].Gains[0].C.L) + (refR * pPreDelay.Reflections[i].Gains[0].C.R);
					refOutR += (refL * pPreDelay.Reflections[i].Gains[1].C.L) + (refR * pPreDelay.Reflections[i].Gains[1].C.R);
				}

				int16 stage1L = int16.CreateSaturating(refOutL / (1 << 15));
				int16 stage1R = int16.CreateSaturating(refOutR / (1 << 15));

				// Second stage
				refOutL = 0;
				refOutR = 0;

				for (c_int i = 4; i < 7; i++)
				{
					pos[i] = (pos[i] + 1) & SndMix_Reflections_Delay_Mask;
					int16 refL = pPreDelay.RefDelayBuffer[pos[i]].C.L, refR = pPreDelay.RefDelayBuffer[pos[i]].C.R;
					refOutL += (refL * pPreDelay.Reflections[i].Gains[0].C.L) + (refR * pPreDelay.Reflections[i].Gains[0].C.R);
					refOutR += (refL * pPreDelay.Reflections[i].Gains[1].C.L) + (refR * pPreDelay.Reflections[i].Gains[1].C.R);
				}

				pOut[0] = (pRefOut[0].C.L = int16.CreateSaturating(stage1L + (refOutL / (1 << 15)))) * refGain;
				pOut[1] = (pRefOut[0].C.R = int16.CreateSaturating(stage1R + (refOutR / (1 << 15)))) * refGain;
				pRefOut++;
				pOut += 2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Late reverberation (with SW reflections)
		/// </summary>
		/********************************************************************/
		private void ProcessLateReverb(SwLateReverb pReverb, CPointer<LR16> pRefOut, CPointer<int32> pMixOut, uint32 nSamples)//XX 890
		{
			c_int delayPos;

			// Calculate delay line offset from current delay position
			c_int Delay_Offset(c_int x)
			{
				return (delayPos - x) & RvbDly_Mask;
			}

			delayPos = (c_int)(pReverb.nDelayPos & RvbDly_Mask);

			while (nSamples-- != 0)
			{
				int16 refInL = pRefOut[0].C.L, refInR = pRefOut[0].C.R;
				pRefOut++;

				int32 delay2LL = pReverb.Delay2[Delay_Offset(RvbDly2L_Len)].C.L, delay2LR = pReverb.Delay2[Delay_Offset(RvbDly2L_Len)].C.R;
				int32 delay2RL = pReverb.Delay2[Delay_Offset(RvbDly2R_Len)].C.L, delay2RR = pReverb.Delay2[Delay_Offset(RvbDly2R_Len)].C.R;

				int32 diff1L = pReverb.Diffusion1[Delay_Offset(RvbDif1L_Len)].C.L;
				int32 diff1R = pReverb.Diffusion1[Delay_Offset(RvbDif1R_Len)].C.R;

				int32 diff2L = pReverb.Diffusion2[Delay_Offset(RvbDif2L_Len)].C.L;
				int32 diff2R = pReverb.Diffusion2[Delay_Offset(RvbDif2R_Len)].C.R;

				int32 lpDecayLL = Clamp16(pReverb.LPHistory[0].C.L - delay2LL) * pReverb.nDecayLP[0].C.L / 65536;
				int32 lpDecayLR = Clamp16(pReverb.LPHistory[0].C.R - delay2LR) * pReverb.nDecayLP[0].C.R / 65536;
				int32 lpDecayRL = Clamp16(pReverb.LPHistory[1].C.L - delay2RL) * pReverb.nDecayLP[1].C.L / 65536;
				int32 lpDecayRR = Clamp16(pReverb.LPHistory[1].C.R - delay2RR) * pReverb.nDecayLP[1].C.R / 65536;

				// Low-passed decay
				pReverb.LPHistory[0].C.L = int16.CreateSaturating(Clamp16(lpDecayLL + lpDecayLL) + delay2LL);
				pReverb.LPHistory[0].C.R = int16.CreateSaturating(Clamp16(lpDecayLR + lpDecayLR) + delay2LR);
				pReverb.LPHistory[1].C.L = int16.CreateSaturating(Clamp16(lpDecayRL + lpDecayRL) + delay2RL);
				pReverb.LPHistory[1].C.R = int16.CreateSaturating(Clamp16(lpDecayRR + lpDecayRR) + delay2RR);

				// Apply decay gain
				int32 histDecayL = Clamp16(pReverb.nDecayDC[0].C.L * pReverb.LPHistory[0].C.L / (1 << 15));
				int32 histDecayR = Clamp16(pReverb.nDecayDC[1].C.R * pReverb.LPHistory[1].C.R / (1 << 15));
				int32 histDecayInL = Clamp16(histDecayL + (refInL / 4));
				int32 histDecayInR = Clamp16(histDecayR + (refInR / 4));
				int32 histDecayInDiffL = Clamp16(histDecayInL - (diff1L * pReverb.nDifCoeffs[0].C.L / 65536));
				int32 histDecayInDiffR = Clamp16(histDecayInR - (diff1R * pReverb.nDifCoeffs[0].C.R / 65536));
				pReverb.Diffusion1[delayPos].C.L = (int16)histDecayInDiffL;
				pReverb.Diffusion1[delayPos].C.R = (int16)histDecayInDiffR;

				int32 delay1L = Clamp16((pReverb.nDifCoeffs[0].C.L * histDecayInDiffL / 65536) + diff1L);
				int32 delay1R = Clamp16((pReverb.nDifCoeffs[0].C.R * histDecayInDiffR / 65536) + diff1R);

				// Insert the diffusion output in the reverb delay line
				pReverb.Delay1[delayPos].C.L = (int16)delay1L;
				pReverb.Delay1[delayPos].C.R = (int16)delay1R;
				int32 histDecayInDelayL = Clamp16(histDecayInL + delay1L);
				int32 histDecayInDelayR = Clamp16(histDecayInR + delay1R);

				// Input to second diffuser
				int32 delay1LL = pReverb.Delay1[Delay_Offset(RvbDly1L_Len)].C.L, delay1LR = pReverb.Delay1[Delay_Offset(RvbDly1L_Len)].C.R;
				int32 delay1RL = pReverb.Delay1[Delay_Offset(RvbDly1R_Len)].C.L, delay1RR = pReverb.Delay1[Delay_Offset(RvbDly1R_Len)].C.R;

				int32 delay1GainsL = Clamp16(((delay1LL * pReverb.Dif2InGains[0].C.L) + (delay1LR * pReverb.Dif2InGains[0].C.R)) / (1 << 15));
				int32 delay1GainsR = Clamp16(((delay1RL * pReverb.Dif2InGains[1].C.L) + (delay1RR * pReverb.Dif2InGains[1].C.R)) / (1 << 15));

				// Accumulate with reverb output
				int32 histDelay1LL = Clamp16(Clamp16(histDecayInDelayL + delay1LL) - delay1GainsL);
				int32 histDelay1LR = Clamp16(Clamp16(histDecayInDelayR + delay1LR) - delay1GainsR);
				int32 histDelay1RL = Clamp16(Clamp16(histDecayInDelayL + delay1RL) - delay1GainsL);
				int32 histDelay1RR = Clamp16(Clamp16(histDecayInDelayR + delay1RR) - delay1GainsR);
				int32 diff2OutL = Clamp16(delay1GainsL - (diff2L * pReverb.nDifCoeffs[0].C.L / 65536));
				int32 diff2OutR = Clamp16(delay1GainsR - (diff2R * pReverb.nDifCoeffs[0].C.R / 65536));
				int32 diff2OutCoeffsL = pReverb.nDifCoeffs[0].C.L * diff2OutL / 65536;
				int32 diff2OutCoeffsR = pReverb.nDifCoeffs[0].C.R * diff2OutR / 65536;
				pReverb.Diffusion2[delayPos].C.L = (int16)diff2OutL;
				pReverb.Diffusion2[delayPos].C.R = (int16)diff2OutR;

				int32 delay2OutL = Clamp16(diff2OutCoeffsL + diff2L);
				int32 delay2OutR = Clamp16(diff2OutCoeffsR + diff2R);
				pReverb.Delay2[delayPos].C.L = (int16)delay2OutL;
				pReverb.Delay2[delayPos].C.R = (int16)delay2OutR;
				delayPos = (delayPos + 1) & RvbDly_Mask;

				// Accumulate with reverb output
				pMixOut[0] += (Clamp16(histDelay1LL + delay2OutL) * pReverb.RvbOutGains[0].C.L) + (Clamp16(histDelay1LR + delay2OutR) * pReverb.RvbOutGains[0].C.R);
				pMixOut[1] += (Clamp16(histDelay1RL + Clamp16(diff2OutCoeffsL)) * pReverb.RvbOutGains[1].C.L) + (Clamp16(histDelay1RR + Clamp16(diff2OutCoeffsR)) * pReverb.RvbOutGains[1].C.R);
				pMixOut += 2;
			}

			pReverb.nDelayPos = (uint32)delayPos;
		}
		#endregion
	}
}
