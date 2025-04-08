/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.ObjectModel;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Utility;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer
{
	/// <summary>
	/// Used when a player using buffer direct mode
	/// </summary>
	internal class MixerBufferDirect : MixerBase
	{
		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Will initialize mixer stuff
		/// </summary>
		/********************************************************************/
		protected override void InitMixer()
		{
			if (virtualChannelCount != mixerChannelCount)
				throw new ArgumentException("Both virtual and mixer channel counts need to be the same");
		}



		/********************************************************************/
		/// <summary>
		/// Will cleanup mixer stuff
		/// </summary>
		/********************************************************************/
		protected override void CleanupMixer()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Returns the click constant value
		/// </summary>
		/********************************************************************/
		public override int GetClickConstant()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main mixer method
		/// </summary>
		/********************************************************************/
		public override void Mixing(MixerInfo currentMixerInfo, int[][][] channelMap, int offsetInFrames, int todoInFrames, ReadOnlyDictionary<SpeakerFlag, int> playerSpeakerToChannelMap)
		{
			for (int i = 0; i < virtualChannelCount; i++)
				AddPlayerSamples(voiceInfo[i], channelMap[i][i], offsetInFrames, todoInFrames);
		}



		/********************************************************************/
		/// <summary>
		/// Convert the mixing buffer to 32 bit
		/// </summary>
		/********************************************************************/
		public override void ConvertMixedData(int[] buffer, int todoInFrames)
		{
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert the output from the player to 32-bit samples
		/// </summary>
		/********************************************************************/
		private void AddPlayerSamples(VoiceInfo vnf, int[] mixingBuffer, int offsetInFrames, int todoInFrames)
		{
			VoiceSampleInfo vsi = vnf.SampleInfo;

			if (vnf.Kick)
			{
				vnf.Current = vsi.Sample.Start;
				vnf.Kick = false;
				vnf.Active = true;
			}

			int countInFrames = Math.Min(todoInFrames, (int)vsi.Sample.Length - (int)vnf.Current);
			if (countInFrames > 0)
			{
				int vol = vnf.Enabled ? MasterVolume : 0;
				vol *= 128;

				if ((vsi.Flags & SampleFlag._16Bits) != 0)
				{
					// 16-bit
					Span<short> source = SampleHelper.ConvertSampleTypeTo16Bit(vsi.Sample.SampleData, 0);

					for (int i = (int)vnf.Current; i < (vnf.Current + countInFrames); i++)
						mixingBuffer[offsetInFrames++] = (int)((((long)source[i] << 16) * vol) / 32768);

					vnf.Current += countInFrames;
				}
				else
				{
					// 8-bit
					Span<sbyte> source = SampleHelper.ConvertSampleTypeTo8Bit(vsi.Sample.SampleData, 0);

					for (int i = (int)vnf.Current; i < (vnf.Current + countInFrames); i++)
						mixingBuffer[offsetInFrames++] = (int)((((long)source[i] << 32) * vol) / 32768);

					vnf.Current += countInFrames;
				}
			}
		}
		#endregion
	}
}
