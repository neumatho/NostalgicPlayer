/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Implementation
{
	/// <summary>
	/// Handles all the effects
	/// </summary>
	internal class EffectMaster : IEffectMaster
	{
		public const int DefaultEffectGroup = int.MaxValue;

		private class EffectGroupInfo
		{
			public EchoArguments Arguments;
			public uint MixerFrequency;
			public bool StereoMode;

			public short[] DelayLine;				// Echo delay line
			public int BufferSize;					// Delay line length in frames
			public int WritePos;					// Current write position in the delay line
			public int DelayTime;					// In frames

			// Echo calculation coefficients
			public int PMix;
			public int NMix;
			public int PCrossPBack;
			public int PCrossNBack;
			public int NCrossPBack;
			public int NCrossNBack;
		}

		private readonly Dictionary<int, int> trackGroups;
		private readonly Dictionary<int, EffectGroupInfo> effectGroups;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EffectMaster(EchoArguments defaultEffect)
		{
			trackGroups = new Dictionary<int, int>();

			effectGroups = new Dictionary<int, EffectGroupInfo>();
			effectGroups[DefaultEffectGroup] = CreateDefaultEffectGroupInfo(defaultEffect);
		}



		/********************************************************************/
		/// <summary>
		/// Return the current echo type for the given track
		/// </summary>
		/********************************************************************/
		public int GetEffectGroup(int track)
		{
			if (trackGroups.TryGetValue(track, out int group))
				return group;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Add the given track to the default effect group
		/// </summary>
		/********************************************************************/
		public void AddToDefaultEffectGroup(int track)
		{
			trackGroups[track] = DefaultEffectGroup;
		}



		/********************************************************************/
		/// <summary>
		/// Add the given track to a specific effect group
		/// </summary>
		/********************************************************************/
		public void AddToEffectGroup(int track, EchoArguments echoArguments)
		{
			effectGroups[track] = CreateDefaultEffectGroupInfo(echoArguments);
			trackGroups[track] = track;
		}



		/********************************************************************/
		/// <summary>
		/// Remove effect from the given track
		/// </summary>
		/********************************************************************/
		public void RemoveFromEffectGroup(int track)
		{
			trackGroups.Remove(track);
			effectGroups.Remove(track);
		}



		/********************************************************************/
		/// <summary>
		/// Change the given effect group
		/// </summary>
		/********************************************************************/
		public void ChangeValuesInEffectGroup(int effectGroup, EchoArguments echoArguments)
		{
			if (effectGroups.TryGetValue(effectGroup, out EffectGroupInfo effectGroupInfo))
			{
				effectGroupInfo.Arguments = echoArguments;
				RecalculateCoefficients(effectGroupInfo);
			}
		}

		#region IEffectMaster implementation
		/********************************************************************/
		/// <summary>
		/// Return a map between a channel and a group number. All channels
		/// in the same group will be mixed together and added the same
		/// effects.
		///
		/// If a channel is not mapped to a group, no extra effects will be
		/// added other than the global effects if any.
		///
		/// Returning null is the same as returning an empty list, so you
		/// can do that, if you do not support channel groups in your player
		/// </summary>
		/********************************************************************/
		public IReadOnlyDictionary<int, int> GetChannelGroups()
		{
			return trackGroups;
		}



		/********************************************************************/
		/// <summary>
		/// Will add effects to a channel group
		/// </summary>
		/********************************************************************/
		public void AddChannelGroupEffects(int group, int[] dest, int todo, uint mixerFrequency, bool stereo)
		{
			if (effectGroups.TryGetValue(group, out EffectGroupInfo effectGroupInfo))
			{
				if ((effectGroupInfo.DelayLine == null) || (mixerFrequency != effectGroupInfo.MixerFrequency) || (stereo != effectGroupInfo.StereoMode))
				{
					effectGroupInfo.MixerFrequency = mixerFrequency;
					effectGroupInfo.StereoMode = stereo;

					InitializeBuffer(effectGroupInfo);
					RecalculateCoefficients(effectGroupInfo);
				}

				if (effectGroupInfo.StereoMode)
					DoEchoStereo(effectGroupInfo, dest, todo);
				else
					DoEchoMono(effectGroupInfo, dest, todo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will add effects to the final mixed output
		/// </summary>
		/********************************************************************/
		public void AddGlobalEffects(int[] dest, int todo, uint mixerFrequency, bool stereo)
		{
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create a default effect group info object
		/// </summary>
		/********************************************************************/
		private EffectGroupInfo CreateDefaultEffectGroupInfo(EchoArguments echoArguments)
		{
			return new EffectGroupInfo
			{
				Arguments = echoArguments,
				MixerFrequency = 0,
				DelayLine = null
			};
		}



		/********************************************************************/
		/// <summary>
		/// Allocate buffer to hold the echo
		/// </summary>
		/********************************************************************/
		private void InitializeBuffer(EffectGroupInfo effectGroupInfo)
		{
			// Maximum echo delay possible is 512 ms, so it needs (0.512 * mixFreq) stereo frames.
			// I round it up to (1/2 + 1/64) * mixFreq. Buffer is rounded up to (and aligned to)
			// 16 bytes (4 stereo frames) for SIMD
			effectGroupInfo.BufferSize = (int)(((effectGroupInfo.MixerFrequency >> 1) + (effectGroupInfo.MixerFrequency >> 6) + 3) & ~4);

			effectGroupInfo.DelayLine = new short[effectGroupInfo.BufferSize * (effectGroupInfo.StereoMode ? 2 : 1)];
			effectGroupInfo.WritePos = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the coefficient values
		/// </summary>
		/********************************************************************/
		private void RecalculateCoefficients(EffectGroupInfo effectGroupInfo)
		{
			effectGroupInfo.DelayTime = (int)((effectGroupInfo.Arguments.EchoDelay * effectGroupInfo.MixerFrequency + 250) / 500);

			effectGroupInfo.PMix = effectGroupInfo.Arguments.EchoMix;
			effectGroupInfo.NMix = 256 - effectGroupInfo.Arguments.EchoMix;
			effectGroupInfo.PCrossPBack = effectGroupInfo.Arguments.EchoCross * effectGroupInfo.Arguments.EchoFeedback;
			effectGroupInfo.PCrossNBack = effectGroupInfo.Arguments.EchoCross * (256 - effectGroupInfo.Arguments.EchoFeedback);
			effectGroupInfo.NCrossPBack = (effectGroupInfo.Arguments.EchoCross - 256) * effectGroupInfo.Arguments.EchoFeedback;
			effectGroupInfo.NCrossNBack = (effectGroupInfo.Arguments.EchoCross - 256) * (effectGroupInfo.Arguments.EchoFeedback - 256);
		}



		/********************************************************************/
		/// <summary>
		/// Will add the echo effect on the given mono buffer
		/// </summary>
		/********************************************************************/
		private void DoEchoMono(EffectGroupInfo effectGroupInfo, int[] dest, int todo)
		{
			int bufferOffset = 0;

			while (todo > 0)
			{
				int readPos = effectGroupInfo.WritePos - effectGroupInfo.DelayTime;
				if (readPos < 0)
					readPos += effectGroupInfo.BufferSize;

				int delayOffset = readPos;

				// Calculation of samples being stored in the delay line
				int sample = dest[bufferOffset] >> 16;
				int sampleDelay = effectGroupInfo.DelayLine[delayOffset];

				int a = sample * effectGroupInfo.NCrossNBack;
				a += sampleDelay * effectGroupInfo.NCrossPBack;

				effectGroupInfo.DelayLine[effectGroupInfo.WritePos] = (short)(a >> 16);

				effectGroupInfo.WritePos++;
				if (effectGroupInfo.WritePos == effectGroupInfo.BufferSize)
					effectGroupInfo.WritePos = 0;

				// Output samples now
				dest[bufferOffset++] = ((sample * effectGroupInfo.NMix + sampleDelay * effectGroupInfo.PMix) >> 8) << 16;

				todo--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will add the echo effect on the given stereo buffer
		/// </summary>
		/********************************************************************/
		private void DoEchoStereo(EffectGroupInfo effectGroupInfo, int[] dest, int todo)
		{
			int bufferOffset = 0;

			while (todo > 0)
			{
				int readPos = effectGroupInfo.WritePos - effectGroupInfo.DelayTime;
				if (readPos < 0)
					readPos += effectGroupInfo.BufferSize;

				int delayOffset = readPos << 1;

				// Calculation of samples being stored in the delay line
				int left = dest[bufferOffset] >> 16;
				int right = dest[bufferOffset + 1] >> 16;
				int leftDelay = effectGroupInfo.DelayLine[delayOffset++];
				int rightDelay = effectGroupInfo.DelayLine[delayOffset];

				int al = left * effectGroupInfo.NCrossNBack;
				al += right * effectGroupInfo.PCrossNBack;
				al += leftDelay * effectGroupInfo.NCrossPBack;
				al += rightDelay * effectGroupInfo.PCrossPBack;

				int ar = right * effectGroupInfo.NCrossNBack;
				ar += left * effectGroupInfo.PCrossNBack;
				ar += rightDelay * effectGroupInfo.NCrossPBack;
				ar += leftDelay * effectGroupInfo.PCrossPBack;

				effectGroupInfo.DelayLine[effectGroupInfo.WritePos << 1] = (short)(al >> 16);
				effectGroupInfo.DelayLine[(effectGroupInfo.WritePos << 1) + 1] = (short)(ar >> 16);

				effectGroupInfo.WritePos++;
				if (effectGroupInfo.WritePos == effectGroupInfo.BufferSize)
					effectGroupInfo.WritePos = 0;

				// Output samples now
				dest[bufferOffset++] = ((left * effectGroupInfo.NMix + leftDelay * effectGroupInfo.PMix) >> 8) << 16;
				dest[bufferOffset++] = ((right * effectGroupInfo.NMix + rightDelay * effectGroupInfo.PMix) >> 8) << 16;

				todo -= 2;
			}
		}
		#endregion
	}
}
