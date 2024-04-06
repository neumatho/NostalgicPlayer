/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
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
		public override void Mixing(int[][] channelMap, int offsetInSamples, int todoInSamples, MixerMode mode)
		{
			if ((mode & MixerMode.Stereo) != 0)
			{
				int leftVolume = voiceInfo[0].Enabled ? MasterVolume : 0;
				int rightVolume = voiceInfo[1].Enabled ? MasterVolume : 0;

				AddPlayerSamples(ref voiceInfo[0], channelMap[0], offsetInSamples, 2, todoInSamples, leftVolume);
				AddPlayerSamples(ref voiceInfo[1], channelMap[1], offsetInSamples + 1, 2, todoInSamples, rightVolume);
			}
			else
			{
				int volume = voiceInfo[0].Enabled ? MasterVolume : 0;

				AddPlayerSamples(ref voiceInfo[0], channelMap[0], offsetInSamples, 1, todoInSamples, volume);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert the mix buffer to the output format and store the result
		/// in the supplied buffer
		/// </summary>
		/********************************************************************/
		public override void ConvertMixedData(byte[] dest, int offsetInBytes, int[] source, int todoInSamples, int samplesToSkip, bool isStereo, bool swapSpeakers)
		{
			MixConvertTo32(MemoryMarshal.Cast<byte, int>(dest), offsetInBytes / 4, source, todoInSamples, samplesToSkip, isStereo, swapSpeakers);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert the output from the player to 32-bit samples
		/// </summary>
		/********************************************************************/
		private void AddPlayerSamples(ref VoiceInfo vnf, int[] dest, int destOffsetInSamples, int destSkip, int todoInSamples, int vol)
		{
			if (vnf.Kick)
			{
				vnf.Current = vnf.Start;
				vnf.Kick = false;
				vnf.Active = true;
			}

			int countInSamples = Math.Min(todoInSamples, (int)vnf.Size - (int)vnf.Current);
			if (countInSamples > 0)
			{
				vol *= 128;

				if ((vnf.Flags & SampleFlag._16Bits) != 0)
				{
					// 16-bit
					Span<short> source = SampleHelper.ConvertSampleTo16Bit(vnf.Addresses[0], 0);

					for (int i = (int)vnf.Current; i < vnf.Current + countInSamples; i++)
					{
						dest[destOffsetInSamples] = (int)((((long)source[i] << 16) * vol) / 32768);
						destOffsetInSamples += destSkip;
					}

					vnf.Current += countInSamples;
				}
				else
				{
					// 8-bit
					Span<sbyte> source = SampleHelper.ConvertSampleTo8Bit(vnf.Addresses[0], 0);

					for (int i = (int)vnf.Current; i < vnf.Current + countInSamples; i++)
					{
						dest[destOffsetInSamples] = (int)((((long)source[i] << 32) * vol) / 32768);
						destOffsetInSamples += destSkip;
					}

					vnf.Current += countInSamples;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void MixConvertTo32(Span<int> dest, int offsetInSamples, int[] source, int countInSamples, int samplesToSkip, bool isStereo, bool swapSpeakers)
		{
			int x1, x2, x3, x4;
			int remain;

			int sourceOffset = 0;

			if (swapSpeakers)
			{
				if (samplesToSkip == 0)
				{
					remain = countInSamples & 3;

					for (countInSamples >>= 2; countInSamples != 0; countInSamples--)
					{
						x1 = source[sourceOffset++];
						x2 = source[sourceOffset++];
						x3 = source[sourceOffset++];
						x4 = source[sourceOffset++];

						dest[offsetInSamples++] = x2;
						dest[offsetInSamples++] = x1;
						dest[offsetInSamples++] = x4;
						dest[offsetInSamples++] = x3;
					}
				}
				else
				{
					remain = countInSamples & 1;

					for (countInSamples >>= 1; countInSamples != 0; countInSamples--)
					{
						x1 = source[sourceOffset++];
						x2 = source[sourceOffset++];

						dest[offsetInSamples++] = x2;
						dest[offsetInSamples++] = x1;

						for (int i = 0; i < samplesToSkip; i++)
							dest[offsetInSamples++] = 0;
					}
				}
			}
			else
			{
				if (isStereo)
				{
					if (samplesToSkip == 0)
					{
						remain = countInSamples & 3;

						for (countInSamples >>= 2; countInSamples != 0; countInSamples--)
						{
							x1 = source[sourceOffset++];
							x2 = source[sourceOffset++];
							x3 = source[sourceOffset++];
							x4 = source[sourceOffset++];

							dest[offsetInSamples++] = x1;
							dest[offsetInSamples++] = x2;
							dest[offsetInSamples++] = x3;
							dest[offsetInSamples++] = x4;
						}
					}
					else
					{
						remain = countInSamples & 1;

						for (countInSamples >>= 1; countInSamples != 0; countInSamples--)
						{
							x1 = source[sourceOffset++];
							x2 = source[sourceOffset++];

							dest[offsetInSamples++] = x1;
							dest[offsetInSamples++] = x2;

							for (int i = 0; i < samplesToSkip; i++)
								dest[offsetInSamples++] = 0;
						}
					}
				}
				else
				{
					remain = 0;

					for (; countInSamples != 0; countInSamples--)
						dest[offsetInSamples++] = source[sourceOffset++];
				}
			}

			while (remain-- != 0)
				dest[offsetInSamples++] = source[sourceOffset++];
		}
		#endregion
	}
}
