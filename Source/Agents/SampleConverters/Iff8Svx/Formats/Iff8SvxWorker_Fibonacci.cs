/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Iff8Svx.Formats
{
	/// <summary>
	/// Reader/writer of IFF-8SVX Fibonacci format
	/// </summary>
	internal class Iff8SvxWorker_Fibonacci : Iff8SvxSaverWorkerBase
	{
		private static readonly sbyte[] fibTable =
		{
			-34, -21, -13, -8, -5, -3, -2, -1, 0, 1, 2, 3, 5, 8, 13, 21
		};

		// Loader variables
		private uint decompressedSize;

		private byte[] decodeBuffer;
		private int sourceOffset;

		private bool loadFirstBuffer;
		private sbyte startVal1;
		private sbyte startVal2;

		// Saver variables
		private bool saveFirstBuffer;
		private sbyte lastVal1;
		private sbyte lastVal2;

		#region Iff8SvxWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the format ID
		/// </summary>
		/********************************************************************/
		protected override Format FormatId => Format.Fibonacci;
		#endregion

		#region ISampleLoaderAgent implementation
		/********************************************************************/
		/// <summary>
		/// Calculates how many samples that will be returned
		/// </summary>
		/********************************************************************/
		public override long GetTotalSampleLength(LoadSampleFormatInfo formatInfo)
		{
			return decompressedSize;
		}
		#endregion

		#region Iff8SvxLoaderWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize the loader
		/// </summary>
		/********************************************************************/
		protected override void LoaderInitialize(long dataLength)
		{
			decodeBuffer = new byte[DecodeBufferSize];

			// Initialize member variables
			decompressedSize = (uint)dataLength;
			loadFirstBuffer = true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the loader
		/// </summary>
		/********************************************************************/
		protected override void LoaderCleanup()
		{
			decodeBuffer = null;
		}



		/********************************************************************/
		/// <summary>
		/// Reset the loader
		/// </summary>
		/********************************************************************/
		protected override void ResetLoader(long position)
		{
			if (position == 0)
				loadFirstBuffer = true;
		}



		/********************************************************************/
		/// <summary>
		/// Load and decode a block of sample data
		/// </summary>
		/********************************************************************/
		protected override int DecodeSampleData(ModuleStream stream, ModuleStream stream2, int[] buffer, int length)
		{
			int filled = 0;

			int bufSize = stream2 == null ? decodeBuffer.Length : decodeBuffer.Length / 2;

			while (length > 0)
			{
				// Do we need to load some data from the file?
				if (samplesLeft < 2)
				{
					// Yes, do it
					if (stream2 != null)
					{
						samplesLeft = GetFileData2(stream2, decodeBuffer, bufSize, bufSize);

						if (samplesLeft == 0)
							break;			// End of file, stop filling

						samplesLeft += GetFileData1(stream, decodeBuffer, 0, samplesLeft);
					}
					else
					{
						samplesLeft = GetFileData1(stream, decodeBuffer, 0, bufSize);

						if (samplesLeft == 0)
							break;			// End of file, stop filling
					}

					sourceOffset = 0;
				}

				// Copy the sample data
				int todo;

				if (stream2 == null)
				{
					// Mono sample
					//
					// Find the number of bytes to decode
					todo = Math.Min(length / 2, samplesLeft);

					// Should we get the start value?
					if (loadFirstBuffer)
					{
						loadFirstBuffer = false;

						startVal1 = (sbyte)decodeBuffer[sourceOffset + 1];

						sourceOffset += 2;
						todo -= 2;
						samplesLeft -= 2;

						// Store the value twice, so we get an even number to store in the buffer
						buffer[filled++] = startVal1 << 24;
						buffer[filled++] = startVal1 << 24;

						length -= 2;
					}

					startVal1 = DecodeBuffer(decodeBuffer, sourceOffset, buffer, filled, todo, startVal1, false);

					sourceOffset += todo;
					filled += todo * 2;
				}
				else
				{
					// Stereo sample
					//
					// Find the number of bytes to decode
					todo = Math.Min(length / 2, samplesLeft) / 2;
					if (todo == 0)
						break;			// Too small amount left, so just break the loop

					// Should we get the start value?
					if (loadFirstBuffer)
					{
						loadFirstBuffer = false;

						startVal1 = (sbyte)decodeBuffer[sourceOffset + 1];
						startVal2 = (sbyte)decodeBuffer[bufSize + sourceOffset + 1];

						sourceOffset += 2;
						todo -= 4;
						samplesLeft -= 4;

						// Store the value twice, so we get an even number to store in the buffer
						buffer[filled++] = startVal1 << 24;
						buffer[filled++] = startVal2 << 24;
						buffer[filled++] = startVal1 << 24;
						buffer[filled++] = startVal2 << 24;

						length -= 4;
					}

					startVal1 = DecodeBuffer(decodeBuffer, sourceOffset, buffer, filled, todo, startVal1, true);
					startVal2 = DecodeBuffer(decodeBuffer, bufSize + sourceOffset, buffer, filled + 1, todo, startVal2, true);

					sourceOffset += todo;
					todo *= 2;
					filled += todo * 2;
				}

				// Update the counter variables
				length -= todo * 2;
				samplesLeft -= todo;
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
			if (position == 0)
			{
				loadFirstBuffer = true;
				return 0;
			}

			// Because the Fibonacci algorithm depends on all previous
			// samples to get the next value, it isn't right possible
			// to jump around in the file. We does support it anyway,
			// but sometimes the sound will overpeak. To reduce that
			// situation, we reset the start values
			startVal1 = 0;
			startVal2 = 0;

			if (formatInfo.Channels == 2)
				return (position / 4) + 2;

			return (position / 2) + 2;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the number of samples from the byte position given
		/// </summary>
		/********************************************************************/
		protected override long CalcSamplePosition(long position, LoadSampleFormatInfo formatInfo)
		{
			return position;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the position where the right channel starts relative to
		/// the left channel
		/// </summary>
		/********************************************************************/
		protected override long GetRightChannelPosition(long totalLength)
		{
			return totalLength / 4 + 2;
		}
		#endregion

		#region ISampleSaverAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		/********************************************************************/
		public override SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.Support8Bit | SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;
		#endregion

		#region Iff8SvxSaverWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize the saver so it is prepared to save the sample
		/// </summary>
		/********************************************************************/
		public override bool InitSaver(SaveSampleFormatInfo formatInfo, out string errorMessage)
		{
			saveFirstBuffer = true;

			return base.InitSaver(formatInfo, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Writes a block of data
		/// </summary>
		/********************************************************************/
		protected override uint WriteData(WriterStream stream, Stream stereoStream, int[] buffer, int length)
		{
			uint written;

			if (saveFormat.Channels == 2)
			{
				// Stereo sample
				//
				// Allocate buffer to hold the encoded data
				byte[] fibBuf1 = new byte[length / 4 + 2];
				byte[] fibBuf2 = new byte[length / 4 + 2];

				// Calculate the number of samples written
				written = (uint)length / 2;

				// If it is the first buffer, write the start byte
				int i = 0;
				int j = 0;

				if (saveFirstBuffer)
				{
					saveFirstBuffer = false;

					lastVal1 = (sbyte)(buffer[0] >> 24);
					lastVal2 = (sbyte)(buffer[1] >> 24);

					fibBuf1[0] = (byte)lastVal1;
					fibBuf1[1] = (byte)lastVal1;
					fibBuf2[0] = (byte)lastVal2;
					fibBuf2[1] = (byte)lastVal2;

					fibBuf1[2] = (byte)(0x80 | GetNextNibble((sbyte)(buffer[2] >> 24), ref lastVal1));
					fibBuf2[2] = (byte)(0x80 | GetNextNibble((sbyte)(buffer[3] >> 24), ref lastVal2));

					written -= 4;
					i = 3;
					j = 4;
				}

				// Encode the data
				int count = length / 4;

				for (; i < count; i++, j += 4)
				{
					// Left channel
					fibBuf1[i] = (byte)(GetNextNibble((sbyte)(buffer[j] >> 24), ref lastVal1) << 4);
					fibBuf1[i] |= GetNextNibble((sbyte)(buffer[j + 2] >> 24), ref lastVal1);

					// Right channel
					fibBuf2[i] = (byte)(GetNextNibble((sbyte)(buffer[j + 1] >> 24), ref lastVal2) << 4);
					fibBuf2[i] |= GetNextNibble((sbyte)(buffer[j + 3] >> 24), ref lastVal2);
				}

				// Write the data in the two files
				stream.Write(fibBuf1, 0, i);
				stereoStream.Write(fibBuf2, 0, i);
			}
			else
			{
				// Mono sample
				//
				// Allocate buffer to hold the encoded data
				byte[] fibBuf = new byte[length / 2 + 2];

				// Calculate the number of samples written
				written = (uint)length;

				// If it is the first buffer, write the start byte
				int i = 0;
				int j = 0;

				if (saveFirstBuffer)
				{
					saveFirstBuffer = false;

					lastVal1 = (sbyte)(buffer[0] >> 24);

					fibBuf[0] = (byte)lastVal1;
					fibBuf[1] = (byte)lastVal1;

					fibBuf[2] = (byte)(0x80 | GetNextNibble((sbyte)(buffer[1] >> 24), ref lastVal1));

					written -= 4;
					i = 3;
					j = 2;
				}

				// Encode the data
				int count = length / 2;

				for (; i < count; i++, j += 2)
				{
					fibBuf[i] = (byte)(GetNextNibble((sbyte)(buffer[j] >> 24), ref lastVal1) << 4);
					fibBuf[i] |= GetNextNibble((sbyte)(buffer[j + 1] >> 24), ref lastVal1);
				}

				// Write the data in the two files
				stream.Write(fibBuf, 0, i);
			}

			return written;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will decode the source buffer using Fibonacci Delta algorithm
		/// and store the result in the destination buffer
		/// </summary>
		/********************************************************************/
		private sbyte DecodeBuffer(byte[] source, int offset, int[] destination, int destOffset, int todo, sbyte startVal, bool skip)
		{
			int add = skip ? 2 : 1;
			sbyte val = startVal;

			for (int i = 0; i < todo; i++)
			{
				val += fibTable[(source[offset + i] & 0xf0) >> 4];
				destination[destOffset] = val << 24;
				destOffset += add;

				val += fibTable[source[offset + i] & 0x0f];
				destination[destOffset] = val << 24;
				destOffset += add;
			}

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// Will calculate the next Fibonacci value
		/// </summary>
		/********************************************************************/
		private byte GetNextNibble(sbyte sample, ref sbyte lastVal)
		{
			byte i;
			for (i = 1; i < 16; i++)
			{
				if ((lastVal + fibTable[i]) > sample)
					break;
			}

			i--;
			int calcVal = lastVal + fibTable[i];
			if (calcVal < -128)
				i++;

			// Remember the new value
			lastVal += fibTable[i];

			return i;
		}
		#endregion
	}
}
