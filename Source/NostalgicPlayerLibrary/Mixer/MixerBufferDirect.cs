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
		public override void ConvertMixedData(byte[] dest, int offset, int[] source, int todo, bool swapSpeakers)
		{
			if (bytesPerSample == 2)
				MixConvertTo16(MemoryMarshal.Cast<byte, short>(dest), offset / 2, source, todo, swapSpeakers);
			else if (bytesPerSample == 4)
				MixConvertTo32(MemoryMarshal.Cast<byte, int>(dest), offset / 4, source, todo, swapSpeakers);
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
		/// Converts the mixed data to a 16 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void MixConvertTo16(Span<short> dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			int x1, x2, x3, x4;

			int remain = count & 3;
			int sourceOffset = 0;

			if (swapSpeakers)
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = source[sourceOffset++];
					x2 = source[sourceOffset++];
					x3 = source[sourceOffset++];
					x4 = source[sourceOffset++];

					dest[offset++] = (short)(x2 >> 16);
					dest[offset++] = (short)(x1 >> 16);
					dest[offset++] = (short)(x4 >> 16);
					dest[offset++] = (short)(x3 >> 16);
				}

				// We know it is always stereo samples when coming here
				while (remain > 0)
				{
					x1 = source[sourceOffset++];
					x2 = source[sourceOffset++];

					dest[offset++] = (short)(x2 >> 16);
					dest[offset++] = (short)(x1 >> 16);

					remain -= 2;
				}
			}
			else
			{
				for (count >>= 2; count != 0; count--)
				{
					x1 = source[sourceOffset++];
					x2 = source[sourceOffset++];
					x3 = source[sourceOffset++];
					x4 = source[sourceOffset++];

					dest[offset++] = (short)(x1 >> 16);
					dest[offset++] = (short)(x2 >> 16);
					dest[offset++] = (short)(x3 >> 16);
					dest[offset++] = (short)(x4 >> 16);
				}

				while (remain-- != 0)
				{
					x1 = source[sourceOffset++];
					dest[offset++] = (short)(x1 >> 16);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private void MixConvertTo32(Span<int> dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			int x1, x2, x3, x4;

			int remain = count & 3;
			int sourceOffset = 0;

			if (swapSpeakers)
			{
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

				// We know it is always stereo samples when coming here
				while (remain > 0)
				{
					x1 = source[sourceOffset++];
					x2 = source[sourceOffset++];

					dest[offset++] = x2;
					dest[offset++] = x1;

					remain -= 2;
				}
			}
			else
			{
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

				while (remain-- != 0)
				{
					x1 = source[sourceOffset++];
					dest[offset++] = x1;
				}
			}
		}
		#endregion
	}
}
