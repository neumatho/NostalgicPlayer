/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// This class implements the mixer
	/// </summary>
	internal class Mixer
	{
		/// <summary>
		/// Random number generator for dithering
		/// </summary>
		private class RandomLcg
		{
			private int MAX_VAL;
			private uint32_t rand_seed;

			public RandomLcg(int maxVal, uint32_t seed)
			{
				MAX_VAL = maxVal;
				rand_seed = seed;
			}

			public int Get()
			{
				rand_seed = (214013 * rand_seed + 2531011);
				return (int)((rand_seed >> 16) & (MAX_VAL - 1));
			}
		}

		/// <summary>
		/// Maximum number of supported SIDs
		/// </summary>
		public const uint MAX_SIDS = 3;

		private const int_least32_t SCALE_FACTOR = 1 << 16;
		private const double SQRT_2 = 1.41421356237;
		private const double SQRT_3 = 1.73205080757;

		private static readonly int_least32_t[] SCALE =
		[
			SCALE_FACTOR,									// 1 chip, no scale
			(int_least32_t)((1.0 / SQRT_2) * SCALE_FACTOR),	// 2 chips, scale by sqrt(2)
			(int_least32_t)((1.0 / SQRT_3) * SCALE_FACTOR)	// 3 chips, scale by sqrt(3)
		];

		/// <summary>
		/// Maximum allowed volume, must be a power of 2
		/// </summary>
		public const int_least32_t VOLUME_MAX = 1024;

		private delegate int_least32_t mixer_func_t();
		private delegate int scale_func_t(uint ch);

		private readonly List<SidEmu> chips = new List<SidEmu>();

		private int_least32_t[] iSamples;
		private int_least32_t[] volume;

		private mixer_func_t[] mix;
		private scale_func_t[] scale;

		private int oldRandomValue = 0;
		private int fastForwardFactor = 1;

		// Mixer settings
		private readonly short[][] sampleBuffers = new short[2][];
		private uint_least32_t sampleCount = 0;
		private uint_least32_t sampleIndex = 0;

		private bool stereo = false;

		private bool wait = false;

		private readonly RandomLcg rand;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mixer()
		{
			rand = new RandomLcg(VOLUME_MAX, 257254);

			mix = new mixer_func_t[] { Mono1 };
		}



		/********************************************************************/
		/// <summary>
		/// This clocks the SID chips to the present moment, if they aren't
		/// already
		/// </summary>
		/********************************************************************/
		public void ClockChips()
		{
			foreach (SidEmu s in chips)
				s.Clock();
		}



		/********************************************************************/
		/// <summary>
		/// Reset sidemu buffer position discarding produced samples
		/// </summary>
		/********************************************************************/
		public void ResetBufs()
		{
			foreach (SidEmu s in chips)
				s.BufferPos(0);
		}



		/********************************************************************/
		/// <summary>
		/// Do the mixing
		/// </summary>
		/********************************************************************/
		public void DoMix()
		{
			// Extract buffer info now that the SID is updated.
			// Clock() may update bufferPos
			// NB: If more than one chip exists, their bufferPos is identical to first chip's
			int sampCount = chips[0].BufferPos();

			int i = 0;
			while (
				(i < sampCount) &&
				(sampleIndex < sampleCount) &&			// Handle whatever output the SID has generated so far
				(i + fastForwardFactor < sampCount)		// Are there enough samples to generate the next one?
			)
			{
				// This is a crude boxcar low-pass filter to
				// reduce aliasing during fast forward
				for (int k = 0; k < chips.Count; k++)
				{
					int_least32_t sample = 0;
					short[] buffer = chips[k].Buffer();

					for (int j = 0; j < fastForwardFactor; j++)
						sample += buffer[i + j];

					iSamples[k] = sample / fastForwardFactor;
				}

				// Increment i to mark we ate some samples, finish the boxcar thing
				i += fastForwardFactor;

				int channels = stereo ? 2 : 1;
				for (uint ch = 0; ch < channels; ch++)
				{
					int_least32_t tmp = scale[ch](ch);
					sampleBuffers[ch][sampleIndex] = (short)tmp;
				}

				sampleIndex++;
			}

			// Move the unhandled data to start of buffer, if any
			int samplesLeft = sampCount - i;

			foreach (SidEmu chip in chips)
			{
				short[] buffer = chip.Buffer();
				Array.Copy(buffer, i, buffer, 0, samplesLeft);

				chip.BufferPos(samplesLeft);
			}

			wait = (uint_least32_t)samplesLeft > sampleCount;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare for mixing cycle
		/// </summary>
		/********************************************************************/
		public void Begin(short[] leftBuffer, short[] rightBuffer, uint32_t count)
		{
			// Sanity check
			//
			// Don't allow odd counts for stereo playback
			if (stereo && ((count & 1) != 0))
				throw new BadBufferSize();

			sampleIndex = 0;
			sampleCount = count;
			sampleBuffers[0] = leftBuffer;
			sampleBuffers[1] = rightBuffer;

			wait = false;
		}



		/********************************************************************/
		/// <summary>
		/// Remove all SIDs from the mixer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearSids()
		{
			chips.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Add a SID to the mixer
		/// </summary>
		/********************************************************************/
		public void AddSid(SidEmu chip)
		{
			if (chip != null)
			{
				chips.Add(chip);

				iSamples = new int_least32_t[chips.Count];

				if (mix.Length > 0)
					UpdateParams();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set mixing mode
		/// </summary>
		/********************************************************************/
		public void SetStereo(bool stereo)
		{
			if (this.stereo != stereo)
			{
				this.stereo = stereo;

				mix = new mixer_func_t[stereo ? 2 : 1];
				UpdateParams();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set mixing volumes, from 0 to VOLUME_MAX
		/// </summary>
		/********************************************************************/
		public void SetVolume(int_least32_t left, int_least32_t right)
		{
			volume = new int[2];
			volume[0] = left;
			volume[1] = right;

			scale = new scale_func_t[2];
			scale[0] = left == VOLUME_MAX ? NoScale : Scale;
			scale[1] = right == VOLUME_MAX ? NoScale : Scale;
		}



		/********************************************************************/
		/// <summary>
		/// Get a SID from the mixer
		/// </summary>
		/********************************************************************/
		public SidEmu GetSid(uint i)
		{
			return i < chips.Count ? chips[(int)i] : null;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the buffer have been filled
		/// </summary>
		/********************************************************************/
		public bool NotFinished()
		{
			return sampleIndex < sampleCount;
		}



		/********************************************************************/
		/// <summary>
		/// Get the number of samples generated up to now
		/// </summary>
		/********************************************************************/
		public uint_least32_t SamplesGenerated()
		{
			return sampleIndex;
		}



		/********************************************************************/
		/// <summary>
		/// Wait till we consume the buffer
		/// </summary>
		/********************************************************************/
		public bool Wait()
		{
			return wait;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int TriangularDithering()
		{
			int prevValue = oldRandomValue;
			oldRandomValue = rand.Get();

			return oldRandomValue - prevValue;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int Scale(uint ch)
		{
			int_least32_t sample = mix[ch]();

			return (sample * volume[ch] + TriangularDithering()) / VOLUME_MAX;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int NoScale(uint ch)
		{
			return mix[ch]();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void UpdateParams()
		{
			switch (chips.Count)
			{
				case 1:
				{
					mix[0] = stereo ? Stereo_OneChip : Mono1;
					if (stereo)
						mix[1] = Stereo_OneChip;

					break;
				}

				case 2:
				{
					mix[0] = stereo ? Stereo_Ch1_TwoChips : Mono2;
					if (stereo)
						mix[1] = Stereo_Ch2_TwoChips;

					break;
				}

				case 3:
				{
					mix[0] = stereo ? Stereo_Ch1_ThreeChips : Mono3;
					if (stereo)
						mix[1] = Stereo_Ch2_ThreeChips;

					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Mono1()
		{
			return iSamples[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Mono2()
		{
			return (iSamples[0] + iSamples[1]) / 2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Mono3()
		{
			return (iSamples[0] + iSamples[1] + iSamples[2]) / 3;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_OneChip()
		{
			return iSamples[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_Ch1_TwoChips()
		{
			return (int_least32_t)(iSamples[0] + 0.5 * iSamples[1]) * SCALE[1] / SCALE_FACTOR;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_Ch2_TwoChips()
		{
			return (int_least32_t)(0.5 * iSamples[0] + iSamples[1]) * SCALE[1] / SCALE_FACTOR;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_Ch1_ThreeChips()
		{
			return (int_least32_t)(iSamples[0] + iSamples[1] + 0.5 * iSamples[2]) * SCALE[2] / SCALE_FACTOR;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_Ch2_ThreeChips()
		{
			return (int_least32_t)(0.5 * iSamples[0] + iSamples[1] + iSamples[2]) * SCALE[2] / SCALE_FACTOR;
		}
		#endregion
	}
}
