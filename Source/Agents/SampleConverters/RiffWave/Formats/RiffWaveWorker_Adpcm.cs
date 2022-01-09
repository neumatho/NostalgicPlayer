/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.RiffWave.Formats
{
	/// <summary>
	/// Reader/writer of RIFF-WAVE ADPCM format
	/// </summary>
	internal class RiffWaveWorker_Adpcm : RiffWaveSaverWorkerBase
	{
		private struct AdpcmCoefSet
		{
			public short coef1;
			public short coef2;
		}

		private static readonly int[] stepAdjustTable =
		{
			230, 230, 230, 230, 307, 409, 512, 614,
			768, 614, 512, 409, 307, 230, 230, 230
		};

		// Loader variables
		private long fileSize;

		private byte[] decodeBuffer;
		private int sourceOffset;

		private ushort samplesPerBlock;
		private ushort coefNum;
		private AdpcmCoefSet[] coefSets;

		private byte[] predictor;
		private short[] delta;
		private short[] sample1;
		private short[] sample2;

		// Saver variables
		private long factPos;

		private short[] encodedBuffer;
		private int encodeSize;
		private int encodeFilled;
		private int encodedSamples;

		private byte[] packet;
		private int[] state;

		#region RiffWaveWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the wave format ID
		/// </summary>
		/********************************************************************/
		protected override WaveFormat FormatId => WaveFormat.WAVE_FORMAT_ADPCM;
		#endregion

		#region ISampleLoaderAgent implementation
		/********************************************************************/
		/// <summary>
		/// Calculates how many samples that will be returned
		/// </summary>
		/********************************************************************/
		public override long GetTotalSampleLength(LoadSampleFormatInfo formatInfo)
		{
			return (fileSize / blockAlign) * samplesPerBlock * formatInfo.Channels;
		}
		#endregion

		#region RiffWaveLoaderWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Tells whether the fact chunk is required or not
		/// </summary>
		/********************************************************************/
		protected override bool NeedFact => false;



		/********************************************************************/
		/// <summary>
		/// Initialize the loader
		/// </summary>
		/********************************************************************/
		protected override void LoaderInitialize(long dataStart, long dataLength, long totalFileSize, LoadSampleFormatInfo formatInfo)
		{
			// Allocate buffer to hold the loaded data
			decodeBuffer = new byte[blockAlign];

			// Initialize member variables
			fileSize = Math.Min(dataLength, totalFileSize - dataStart);

			predictor = new byte[2];
			delta = new short[2];
			sample1 = new short[2];
			sample2 = new short[2];
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the loader
		/// </summary>
		/********************************************************************/
		protected override void LoaderCleanup()
		{
			decodeBuffer = null;
			coefSets = null;
		}



		/********************************************************************/
		/// <summary>
		/// Loads any extra header information from the 'fmt ' chunk
		/// </summary>
		/********************************************************************/
		protected override int LoadExtraHeaderInfo(ModuleStream moduleStream, LoadSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Test some of the standard information
			if (formatInfo.Bits != 4)
			{
				errorMessage = string.Format(Resources.IDS_RIFFWAVE_ERR_INVALIDBITSIZE, formatInfo.Bits);
				return 0;
			}

			// Get the length of the extra information
			ushort extraLen = moduleStream.Read_L_UINT16();

			// Read the header
			samplesPerBlock = moduleStream.Read_L_UINT16();		// Number of samples per block
			coefNum = moduleStream.Read_L_UINT16();				// Number of coef's

			// Is the header big enough
			if ((2 + 2 + 4 * coefNum) != extraLen)
			{
				errorMessage = string.Format(Resources.IDS_RIFFWAVE_ERR_EXTRAHEADER, extraLen);
				return 0;
			}

			// Allocate buffer to hold the coef's
			coefSets = new AdpcmCoefSet[coefNum];

			// Read the coef's
			for (int i = 0; i < coefNum; i++)
			{
				coefSets[i].coef1 = (short)moduleStream.Read_L_UINT16();
				coefSets[i].coef2 = (short)moduleStream.Read_L_UINT16();
			}

			return 2 * 3 + 4 * coefNum;
		}



		/********************************************************************/
		/// <summary>
		/// Load and decode a block of sample data
		/// </summary>
		/********************************************************************/
		protected override int DecodeSampleData(ModuleStream moduleStream, int[] buffer, int length, LoadSampleFormatInfo formatInfo)
		{
			int filled = 0;

			while (length > 0)
			{
				// Do we need to load some data from the file?
				if (samplesLeft == 0)
				{
					// Yes, read the block data
					int samplesThisBlock = GetFileData(moduleStream, decodeBuffer, blockAlign);
					if (samplesThisBlock == 0)
						break;			// End of file, stop filling

					// The number of bytes read is less than the required.
					// Composite for it
					if (samplesThisBlock < blockAlign)
						samplesThisBlock = (blockAlign - (6 * formatInfo.Channels));
					else
						samplesThisBlock = samplesPerBlock * formatInfo.Channels;

					// Reset the offset
					sourceOffset = 0;

					// Get the block header
					for (int i = 0; i < formatInfo.Channels; i++)
					{
						predictor[i] = decodeBuffer[sourceOffset++];
						if (predictor[i] > coefNum)
						{
							// Invalid predictor
							return filled;
						}
					}

					for (int i = 0; i < formatInfo.Channels; i++)
					{
						delta[i] = (short)((decodeBuffer[sourceOffset] & 0xff) | ((decodeBuffer[sourceOffset + 1] & 0xff) << 8));
						sourceOffset += 2;
					}

					for (int i = 0; i < formatInfo.Channels; i++)
					{
						sample1[i] = (short)((decodeBuffer[sourceOffset] & 0xff) | ((decodeBuffer[sourceOffset + 1] & 0xff) << 8));
						sourceOffset += 2;
					}

					for (int i = 0; i < formatInfo.Channels; i++)
					{
						sample2[i] = (short)((decodeBuffer[sourceOffset] & 0xff) | ((decodeBuffer[sourceOffset + 1] & 0xff) << 8));
						sourceOffset += 2;
					}

					// Decode two samples for the header
					for (int i = 0; i < formatInfo.Channels; i++)
						buffer[filled++] = sample2[i] << 16;

					for (int i = 0; i < formatInfo.Channels; i++)
						buffer[filled++] = sample1[i] << 16;

					// Subtract the two samples stored from the header
					samplesLeft = samplesThisBlock - 2 * formatInfo.Channels;

					// Update counter variables
					length -= 2 * formatInfo.Channels;
				}

				// Find the number of samples to return
				int todo = Math.Min(length, samplesLeft);

				// Update counter variables
				samplesLeft -= todo;
				length -= todo;

				// Begin to decode the samples
				while (todo > 0)
				{
					// Get the sample byte
					byte byt = decodeBuffer[sourceOffset++];

					// Decode first nibble
					buffer[filled++] = Decode((sbyte)((byt >> 4) & 0x0f), 0) << 16;

					// Decode second nibble
					if (formatInfo.Channels > 1)
						buffer[filled++] = Decode((sbyte)(byt & 0x0f), 1) << 16;
					else
						buffer[filled++] = Decode((sbyte)(byt & 0x0f), 0) << 16;

					// Decrement the counter
					todo -= 2;
				}
			}

			return filled;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the number of bytes to go into the file to reach the
		/// position given
		/// </summary>
		/********************************************************************/
		protected override long CalcFilePosition(long position, LoadSampleFormatInfo formatInfo)
		{
			// Calculate the block number the position is in
			int blockNum = (int)(position / (samplesPerBlock * formatInfo.Channels));
			return blockNum * blockAlign;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the number of samples from the byte position given
		/// </summary>
		/********************************************************************/
		protected override long CalcSamplePosition(long position, LoadSampleFormatInfo formatInfo)
		{
			return (position / blockAlign) * samplesPerBlock * formatInfo.Channels;
		}
		#endregion

		#region ISampleSaverAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		/********************************************************************/
		public override SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;
		#endregion

		#region RiffWaveSaverWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize the saver so it is prepared to save the sample
		/// </summary>
		/********************************************************************/
		public override bool InitSaver(SaveSampleFormatInfo formatInfo, out string errorMessage)
		{
			// Initialize the coef values
			coefNum = 7;

			coefSets = new AdpcmCoefSet[7];

			// Fill out the sets
			coefSets[0].coef1 = 256;
			coefSets[0].coef2 = 0;

			coefSets[1].coef1 = 512;
			coefSets[1].coef2 = -256;

			coefSets[2].coef1 = 0;
			coefSets[2].coef2 = 0;

			coefSets[3].coef1 = 192;
			coefSets[3].coef2 = 64;

			coefSets[4].coef1 = 240;
			coefSets[4].coef2 = 0;

			coefSets[5].coef1 = 460;
			coefSets[5].coef2 = -208;

			coefSets[6].coef1 = 392;
			coefSets[6].coef2 = -232;

			// Initialize the other variables
			state = new int[16];

			encodedSamples = 0;

			return base.InitSaver(formatInfo, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the saver
		/// </summary>
		/********************************************************************/
		public override void CleanupSaver()
		{
			packet = null;
			encodedBuffer = null;
			coefSets = null;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the average bytes per second
		/// </summary>
		/********************************************************************/
		protected override uint GetAverageBytesPerSecond()
		{
			ushort ba = GetBlockAlign();

			// Calculate the samples per block value
			samplesPerBlock = (ushort)((((ba - (7 * saveFormat.Channels)) * 8) / (4 * saveFormat.Channels)) + 2);

			return (uint)(saveFormat.Frequency / samplesPerBlock) * ba;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the block align
		/// </summary>
		/********************************************************************/
		protected override ushort GetBlockAlign()
		{
			return (ushort)(saveFormat.Channels * 256);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the bit size of each sample
		/// </summary>
		/********************************************************************/
		protected override ushort GetSampleSize(int sampleSize)
		{
			return 4;
		}



		/********************************************************************/
		/// <summary>
		/// Writes any extra information into the fmt chunk
		/// </summary>
		/********************************************************************/
		protected override void WriteExtraFmtInfo(WriterStream stream)
		{
			stream.Write_L_UINT16((ushort)(4 + 4 * coefNum));
			stream.Write_L_UINT16(samplesPerBlock);
			stream.Write_L_UINT16(coefNum);

			for (int i = 0; i < coefNum; i++)
			{
				stream.Write_L_UINT16((ushort)coefSets[i].coef1);
				stream.Write_L_UINT16((ushort)coefSets[i].coef2);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Writes the fact chunk
		/// </summary>
		/********************************************************************/
		protected override void WriteFactChunk(WriterStream stream)
		{
			stream.Write_B_UINT32(0x66616374);			// fact
			stream.Write_L_UINT32(4);					// Chunk size

			// Remember the position, so we can write back the right value when closing
			factPos = stream.Position;

			stream.Write_L_UINT32(0);					// Samples written
		}



		/********************************************************************/
		/// <summary>
		/// Writes a block of data
		/// </summary>
		/********************************************************************/
		protected override int WriteData(WriterStream stream, int[] buffer, int length)
		{
			int written = 0;

			encodedSamples += length;
			int bufferIndex = 0;

			while (length > 0)
			{
				// Do we need to allocate the temp buffer?
				if (encodedBuffer == null)
				{
					encodeSize = saveFormat.Channels * samplesPerBlock;
					encodedBuffer = new short[encodeSize];

					packet = new byte[GetBlockAlign()];

					encodeFilled = 0;
				}

				// Copy the sample data to the temp buffer and convert it to 16-bit
				int todo = Math.Min(length, encodeSize - encodeFilled);

				for (int i = 0, j = encodeFilled; i < todo; i++, j++)
					encodedBuffer[j] = (short)(buffer[bufferIndex++] >> 16);

				// Adjust the counter variables
				length -= todo;
				encodeFilled += todo;

				// Did we get enough data to do the encode?
				if (encodeFilled == encodeSize)
				{
					written += EncodeBuffer(stream);
					encodeFilled = 0;
				}
			}

			return written;
		}



		/********************************************************************/
		/// <summary>
		/// Write the last data or fixing up chunks
		/// </summary>
		/********************************************************************/
		protected override int WriteTail(WriterStream stream)
		{
			// Write what is missing
			int written = EncodeBuffer(stream);

			// Now seek back and write the FACT value
			stream.Seek(factPos, SeekOrigin.Begin);
			stream.Write_L_UINT32((uint)(encodedSamples / saveFormat.Channels));

			return written;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Decode one nibble to a single 16-bit sample using MSADPCM
		/// </summary>
		/********************************************************************/
		private short Decode(sbyte deltaCode, int channel)
		{
			// Compute next Adaptive Scale Factor (ASF)
			int iDelta = delta[channel];
			delta[channel] = (short)((stepAdjustTable[deltaCode] * iDelta) >> 8);

			// Check for minimum delta value
			if (delta[channel] < 16)
				delta[channel] = 16;

			// If the delta code is negative, extend the sign
			if ((deltaCode & 0x08) != 0)
				deltaCode -= 0x10;

			// Predict next sample
			int predict = ((sample1[channel] * coefSets[predictor[channel]].coef1) + (sample2[channel] * coefSets[predictor[channel]].coef2)) >> 8;

			// Reconstruct original PCM
			int sample = (deltaCode * iDelta) + predict;

			// Checking for over- and underflow
			if (sample > 32767)
				sample = 32767;

			if (sample < -32768)
				sample = -32768;

			// Update the states
			sample2[channel] = sample1[channel];
			sample1[channel] = (short)sample;

			return (short)sample;
		}



		/********************************************************************/
		/// <summary>
		/// Encode a whole buffer and writes it to the file
		/// </summary>
		/********************************************************************/
		private int EncodeBuffer(WriterStream stream)
		{
			// Did we get a filled buffer?
			if (encodeFilled < encodeSize)
			{
				// No, zero out the rest of the buffer
				Array.Clear(encodedBuffer, encodeFilled, encodeSize - encodeFilled);
			}

			// Encode the buffer
			ushort ba = GetBlockAlign();
			AdpcmBlockMashI(saveFormat.Channels, encodedBuffer, samplesPerBlock, state, packet, ba);

			// And write it to the file
			stream.Write(packet, 0, ba);

			return ba;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AdpcmBlockMashI(int chans, short[] ip, int n, int[] st, byte[] oBuff, int ba)
		{
			Array.Clear(oBuff, 7 * chans, ba - 7 * chans);

			for (int ch = 0; ch < chans; ch++)
				AdpcmMashChannel(ch, chans, ip, n, st, oBuff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AdpcmMashChannel(int ch, int chans, short[] ip, int n, int[] st, byte[] oBuff)
		{
			short[] v = new short[2];

			int n0 = n / 2;
			if (n0 > 32)
				n0 = 32;

			if (st[ch] < 16)
				st[ch] = 16;

			v[1] = ip[ch];
			v[0] = ip[ch + chans];

			int dMin = 0;
			int kMin = 0;
			int sMin = 0;

			// For each of 7 standard coeff sets, we try compression
			// beginning with last step-value, and with slightly
			// forward-adjusted step-value, taking the best of the 14
			for (int k = 0; k < 7; k++)
			{
				int s0;

				int ss = s0 = st[ch];
				int d0 = AdpcmMashS(ch, chans, v, coefSets[k], ip, n, ref ss, null);	// With step s0

				int s1 = s0;
				AdpcmMashS(ch, chans, v, coefSets[k], ip, n0, ref s1, null);

				ss = s1 = (3 * s0 + s1) / 4;
				int d1 = AdpcmMashS(ch, chans, v, coefSets[k], ip, n, ref ss, null);	// With step s1

				if ((k == 0) || (d0 < dMin) || (d1 < dMin))
				{
					kMin = k;

					if (d0 <= d1)
					{
						dMin = d0;
						sMin = s0;
					}
					else
					{
						dMin = d1;
						sMin = s1;
					}
				}
			}

			st[ch] = sMin;

			AdpcmMashS(ch, chans, v, coefSets[kMin], ip, n, ref st[ch], oBuff);
			oBuff[ch] = (byte)kMin;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int AdpcmMashS(int ch, int chans, short[] v, AdpcmCoefSet iCoef, short[] iBuff, int n, ref int ioStep, byte[] oBuff)
		{
			int ox = 0;

			int ip = ch;				// Point ip to 1st input sample for this channel
			int iTop = n * chans;

			int v0 = v[0];
			int v1 = v[1];

			int d = iBuff[ip] - v1;		// 1st input sample for this channel
			ip += chans;
			double d2 = d * d;			// d2 will be sum of squares of errors, given input v0 and *st
			d = iBuff[ip] - v0;			// 2nd input sample for this channel
			ip += chans;
			d2 += d * d;

			int step = ioStep;

			int op = 0;
			if (oBuff != null)          // Null means don't output, just compute the rms error
			{
				op += chans;			// Skip bpred indices
				op += 2 * ch;			// Channel step size
				oBuff[op] = (byte)step;
				oBuff[op + 1] = (byte)(step >> 8);

				op += 2 * chans;		// Skip to v0
				oBuff[op] = (byte)v0;
				oBuff[op + 1] = (byte)(v0 >> 8);

				op += 2 * chans;		// Skip to v1
				oBuff[op] = (byte)v1;
				oBuff[op + 1] = (byte)(v1 >> 8);

				op = 7 * chans;			// Point to base of output nibbles
				ox = 4 * ch;
			}

			for (; ip < iTop; ip += chans)
			{
				// Make linear prediction for next sample
				int vLin = (v0 * iCoef.coef1 + v1 * iCoef.coef2) >> 8;
				int d3 = iBuff[ip] - vLin;	// Difference between linear prediction and current sample
				int dp = d3 + (step << 3) + (step >> 1);
				int c = 0;

				if (dp > 0)
				{
					c = dp / step;
					if (c > 15)
						c = 15;
				}

				c -= 8;
				dp = c * step;			// Quantized estimate of sample - vLin
				c &= 0x0f;				// Mask to 4 bits

				v1 = v0;				// Shift history
				v0 = vLin + dp;

				if (v0 < -0x8000)
					v0 = -0x8000;
				else
				{
					if (v0 > 0x7fff)
						v0 = 0x7fff;
				}

				d3 = iBuff[ip] - v0;
				d2 += d3 * d3;			// Update square-error

				if (oBuff != null)
				{
					oBuff[op + (ox >> 3)] |= (byte)((ox & 4) != 0 ? c : c << 4);
					ox += 4 * chans;
				}

				// Update the step for the next sample
				step = (stepAdjustTable[c] * step) >> 8;
				if (step < 16)
					step = 16;
			}

			d2 /= n;					// Be sure it's non-negative

			ioStep = step;

			return (int)Math.Sqrt(d2);
		}
		#endregion
	}
}
