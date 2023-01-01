/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.Ahx.Implementation
{
	/// <summary>
	/// Handles the generation of output samples
	/// </summary>
	internal class AhxOutput
	{
		private AhxWorker player;

		private int bits;
		private int frequency;
		private int mixLen;
		private int hz;

		private readonly int[,] volumeTable = new int[65, 256];

		private readonly int[] pos = { 0, 0, 0, 0 };

		/********************************************************************/
		/// <summary>
		/// Initialize the output class
		/// </summary>
		/********************************************************************/
		public void Init(AhxWorker player, int frequency, int bits, int mixLen, float boost, int hz)
		{
			this.player = player;

			this.mixLen = mixLen;
			this.frequency = frequency;
			this.bits = bits;
			this.hz = hz;

			// Generate the volume table
			for (int i = 0; i < 65; i++)
			{
				for (int j = -128; j < 128; j++)
					volumeTable[i, j + 128] = (int)(i * j * boost) / 64;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generate the sampling buffer
		/// </summary>
		/********************************************************************/
		public void PrepareBuffers(short[][] buffers)
		{
			int nrSamples = frequency / hz / player.song.SpeedMultiplier;
			int[] mb = new int[4];

			// Remember the sample buffer pointers
			for (int v = 0; v < 4; v++)
			{
				mb[v] = 0;
				Array.Clear(buffers[v], 0, mixLen * frequency / hz);
			}

			for (int f = 0; f < mixLen * player.song.SpeedMultiplier; f++)
			{
				// Call the play function
				player.PlayIrq();

				// Create the sample buffers
				for (int v = 0; v < 4; v++)
					GenerateBuffer(nrSamples, buffers[v], ref mb[v], v);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Generate one sampling buffer
		/// </summary>
		/********************************************************************/
		private void GenerateBuffer(int nrSamples, short[] buffer, ref int mb, int v)
		{
			if (player.voices[v].voiceVolume != 0)
			{
				float freq = Period2Freq(player.voices[v].voicePeriod);
				int delta = (int)(freq * (1 << 16) / frequency);
				int samplesToMix = nrSamples;
				int mixPos = 0;

				while (samplesToMix != 0)
				{
					if (pos[v] > (0x280 << 16))
						pos[v] -= 0x280 << 16;

					int thisCount = Math.Min(samplesToMix, ((0x280 << 16) - pos[v] - 1) / delta + 1);
					samplesToMix -= thisCount;

					int volTab1 = player.voices[v].voiceVolume;
					int volTab2 = 128;

					for (int i = 0; i < thisCount; i++)
					{
						buffer[mb + mixPos++] = (short)(volumeTable[volTab1, volTab2 + (player.voices[v].voiceBuffer[pos[v] >> 16])] << 7);
						pos[v] += delta;
					}
				}
			}

			mb += nrSamples;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a period to frequency
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float Period2Freq(float period)
		{
			return 3579545.25f / period;
		}
		#endregion
	}
}
