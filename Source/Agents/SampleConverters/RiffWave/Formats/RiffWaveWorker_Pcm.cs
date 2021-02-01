/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.RiffWave.Formats
{
	/// <summary>
	/// Reader/writer of RIFF-WAVE PCM format
	/// </summary>
	internal class RiffWaveWorker_Pcm : RiffWaveSaverWorkerBase
	{
		#region ISampleSaverAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		/********************************************************************/
		public override SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.Support8Bit | SampleSaverSupportFlag.Support16Bit | SampleSaverSupportFlag.Support32Bit | SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;
		#endregion

		#region RiffWaveWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the wave format ID
		/// </summary>
		/********************************************************************/
		protected override WaveFormat FormatId => WaveFormat.WAVE_FORMAT_PCM;
		#endregion

		#region RiffWaveSaverWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Returns the average bytes per second
		/// </summary>
		/********************************************************************/
		protected override uint GetAverageBytesPerSecond()
		{
			return (uint)((format.Channels * format.Bits * format.Frequency + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the block align
		/// </summary>
		/********************************************************************/
		protected override ushort GetBlockAlign()
		{
			return (ushort)((format.Channels * format.Bits + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Writes any extra information into the fmt chunk
		/// </summary>
		/********************************************************************/
		protected override void WriteExtraFmtInfo(WriterStream writerStream)
		{
			writerStream.Write_L_UINT16(GetSampleSize(format.Bits));		// Sample size
		}



		/********************************************************************/
		/// <summary>
		/// Writes a block of data
		/// </summary>
		/********************************************************************/
		protected override int WriteData(int[] buffer, int length, byte[] outputBuffer)
		{
			if (format.Bits == 8)
			{
				// Convert to unsigned 8-bit
				for (int i = 0; i < length; i++)
					outputBuffer[i] = (byte)((buffer[i] >> 24) + 128);

				return length;
			}

			if (format.Bits == 16)
			{
				// Convert to signed 16-bit
				for (int i = 0, j = 0; i < length; i++, j += 2)
				{
					int sample = buffer[i];
					byte[] shortArray = BitConverter.GetBytes((short)(sample >> 16));

					outputBuffer[j] = shortArray[0];
					outputBuffer[j + 1] = shortArray[1];
				}

				return length * 2;
			}

			if (format.Bits == 32)
			{
				// Convert to signed 32-bit
				for (int i = 0, j = 0; i < length; i++, j += 4)
				{
					int sample = buffer[i];
					byte[] intArray = BitConverter.GetBytes(sample);

					outputBuffer[j] = intArray[0];
					outputBuffer[j + 1] = intArray[1];
					outputBuffer[j + 2] = intArray[2];
					outputBuffer[j + 3] = intArray[3];
				}

				return length * 4;
			}

			return 0;
		}
		#endregion
	}
}
