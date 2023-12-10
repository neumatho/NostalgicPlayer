/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.PlayerLibrary.Mixer.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Utility;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer
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
		public override void Mixing(int[][] channelMap, int offset, int todo, MixerMode mode)
		{
			if ((mode & MixerMode.Stereo) != 0)
			{
				int leftVolume = voiceInfo[0].Enabled ? MasterVolume : 0;
				int rightVolume = voiceInfo[1].Enabled ? MasterVolume : 0;

				AddPlayerSamples(ref voiceInfo[0], channelMap[0], offset, 2, todo, leftVolume);
				AddPlayerSamples(ref voiceInfo[1], channelMap[1], offset + 1, 2, todo, rightVolume);
			}
			else
			{
				int volume = voiceInfo[0].Enabled ? MasterVolume : 0;

				AddPlayerSamples(ref voiceInfo[0], channelMap[0], offset, 1, todo, volume);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert the mix buffer to the output format and store the result
		/// in the supplied buffer
		/// </summary>
		/********************************************************************/
		public override void ConvertMixedData(byte[] dest, int offset, int[] source, int todo, int samplesToSkip, bool isStereo, bool swapSpeakers)
		{
			MixConvertTo32(MemoryMarshal.Cast<byte, int>(dest), offset / 4, source, todo, samplesToSkip, isStereo, swapSpeakers);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert the output from the player to 32-bit samples
		/// </summary>
		/********************************************************************/
		private void AddPlayerSamples(ref VoiceInfo vnf, int[] dest, int destOffset, int destSkip, int todo, int vol)
		{
			if (vnf.Kick)
			{
				vnf.Current = vnf.Start;
				vnf.Kick = false;
				vnf.Active = true;
			}

			int count = Math.Min(todo, (int)vnf.Size - (int)vnf.Current);
			if (count > 0)
			{
				vol *= 128;

				if ((vnf.Flags & SampleFlag._16Bits) != 0)
				{
					// 16-bit
					Span<short> source = SampleHelper.ConvertSampleTo16Bit(vnf.Addresses[0], 0);

					for (int i = (int)vnf.Current; i < vnf.Current + count; i++)
					{
						dest[destOffset] = (int)((((long)source[i] << 16) * vol) / 32768);
						destOffset += destSkip;
					}

					vnf.Current += count;
				}
				else
				{
					// 8-bit
					Span<sbyte> source = SampleHelper.ConvertSampleTo8Bit(vnf.Addresses[0], 0);

					for (int i = (int)vnf.Current; i < vnf.Current + count; i++)
					{
						dest[destOffset] = (int)((((long)source[i] << 32) * vol) / 32768);
						destOffset += destSkip;
					}

					vnf.Current += count;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void MixConvertTo32(Span<int> dest, int offset, int[] source, int count, int samplesToSkip, bool isStereo, bool swapSpeakers)
		{
			int x1, x2, x3, x4;
			int remain;

			int sourceOffset = 0;

			if (swapSpeakers)
			{
				if (samplesToSkip == 0)
				{
					remain = count & 3;

					for (count >>= 2; count != 0; count--)
					{
						x1 = source[sourceOffset++];
						x2 = source[sourceOffset++];
						x3 = source[sourceOffset++];
						x4 = source[sourceOffset++];

						dest[offset++] = x2;
						dest[offset++] = x1;
						dest[offset++] = x4;
						dest[offset++] = x3;
					}
				}
				else
				{
					remain = count & 1;

					for (count >>= 1; count != 0; count--)
					{
						x1 = source[sourceOffset++];
						x2 = source[sourceOffset++];

						dest[offset++] = x2;
						dest[offset++] = x1;

						for (int i = 0; i < samplesToSkip; i++)
							dest[offset++] = 0;
					}
				}
			}
			else
			{
				if (isStereo)
				{
					if (samplesToSkip == 0)
					{
						remain = count & 3;

						for (count >>= 2; count != 0; count--)
						{
							x1 = source[sourceOffset++];
							x2 = source[sourceOffset++];
							x3 = source[sourceOffset++];
							x4 = source[sourceOffset++];

							dest[offset++] = x1;
							dest[offset++] = x2;
							dest[offset++] = x3;
							dest[offset++] = x4;
						}
					}
					else
					{
						remain = count & 1;

						for (count >>= 1; count != 0; count--)
						{
							x1 = source[sourceOffset++];
							x2 = source[sourceOffset++];

							dest[offset++] = x1;
							dest[offset++] = x2;

							for (int i = 0; i < samplesToSkip; i++)
								dest[offset++] = 0;
						}
					}
				}
				else
				{
					remain = 0;

					for (; count != 0; count--)
						dest[offset++] = source[sourceOffset++];
				}
			}

			while (remain-- != 0)
				dest[offset++] = source[sourceOffset++];
		}
		#endregion
	}
}
