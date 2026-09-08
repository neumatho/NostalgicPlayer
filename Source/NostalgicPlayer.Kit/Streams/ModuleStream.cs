/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This class is used when loading modules
	/// </summary>
	public class ModuleStream : ReaderStream
	{
		private readonly bool leaveSampleStreamOpen;
		private readonly ConvertSamplePosition[] samplePositions;
		private readonly Stream sampleStream;
		private readonly long convertedLength;
		private int currentSampleNumber;
		private long virtualPosition;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleStream(Stream wrapperStream, bool leaveOpen) : base(wrapperStream, leaveOpen)
		{
			leaveSampleStreamOpen = true;
			samplePositions = null;

			virtualPosition = wrapperStream.Position;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor - used by module converters
		/// </summary>
		/********************************************************************/
		public ModuleStream(Stream wrapperStream, Stream sampleStream, ConvertSamplePosition[] samplePositions, long totalLength, bool leaveSampleStreamOpen) : base(wrapperStream, false)
		{
			this.sampleStream = sampleStream;
			this.samplePositions = samplePositions;
			convertedLength = totalLength;
			this.leaveSampleStreamOpen = leaveSampleStreamOpen;

			virtualPosition = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!leaveSampleStreamOpen)
				sampleStream.Dispose();
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length
		{
			get
			{
				if (sampleStream != null)
					return convertedLength;

				return base.Length;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => virtualPosition;

			set => Seek(value, SeekOrigin.Begin);
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			long length = Length;

			switch (origin)
			{
				case SeekOrigin.Begin:
				{
					virtualPosition = offset;
					break;
				}

				case SeekOrigin.Current:
				{
					virtualPosition += offset;
					break;
				}

				case SeekOrigin.End:
				{
					virtualPosition = length + offset;
					break;
				}
			}

			if (virtualPosition < 0)
				virtualPosition = 0;

			EndOfStream = virtualPosition > length;

			if (virtualPosition > length)
				virtualPosition = length;

			SeekToPosition();

			return virtualPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			int sampleCount = samplePositions?.Length ?? 0;

			if (sampleCount == 0)
			{
				int read = wrapperStream.Read(buffer, offset, count);
				virtualPosition += read;

				EndOfStream = read < count;

				return read;
			}

			int totalRead = 0;

			while (count > 0)
			{
				ConvertSamplePosition convertSamplePos = currentSampleNumber < sampleCount ? samplePositions![currentSampleNumber] : null;

				Stream streamToReadFrom;
				int toRead = count;

				if (convertSamplePos == null)
				{
					// No more samples, so the rest of the data is module data
					streamToReadFrom = wrapperStream;
				}
				else if (virtualPosition < convertSamplePos.StartPosition)
				{
					// We are in module data, so read at most up to the start of the next sample
					toRead = (int)Math.Min(toRead, convertSamplePos.StartPosition - virtualPosition);
					streamToReadFrom = wrapperStream;
				}
				else
				{
					// We are inside a sample, so read at most the rest of the sample
					toRead = (int)Math.Min(toRead, convertSamplePos.EndPosition - virtualPosition);

					if (toRead == 0)
					{
						// Empty sample, so just skip it
						currentSampleNumber++;
						continue;
					}

					// Make sure the sample stream is at the right position. It may not
					// be, if the samples are not stored in the sample stream in the same
					// order as they appear in the module
					long wantedPosition = convertSamplePos.SampleStreamStartPosition + (virtualPosition - convertSamplePos.StartPosition);

					if (sampleStream.Position != wantedPosition)
						sampleStream.Seek(wantedPosition, SeekOrigin.Begin);

					streamToReadFrom = sampleStream;
				}

				int read = streamToReadFrom.Read(buffer, offset, toRead);
				if (read == 0)
					break;

				offset += read;
				count -= read;
				totalRead += read;
				virtualPosition += read;

				if ((convertSamplePos != null) && (virtualPosition == convertSamplePos.EndPosition))
					currentSampleNumber++;
			}

			EndOfStream = count > 0;

			return totalRead;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Will read a line. The line is a character sequence which is
		/// terminated by \0 or new-line characters
		/// </summary>
		/********************************************************************/
		public string ReadLine(Encoding encoder)
		{
			StringBuilder sb = new StringBuilder();

			byte[] tempBuf = new byte[80];

			// Read until we reach EOF or the line has been read
			while (!EndOfStream)
			{
				int bytesRead = Read(tempBuf, 0, 80);
				if (bytesRead != 0)
				{
					char[] chars = encoder.GetChars(tempBuf, 0, bytesRead);

					// Check to see if any new lines or null terminator are found
					bool found = false;
					bool newLine = false;
					int foundPos = chars.Length;

					for (int i = 0; i < chars.Length; i++)
					{
						char chr = chars[i];

						if (chr == '\r')
						{
							foundPos = i;
							newLine = true;
							continue;
						}

						if ((chr == '\n') || (chr == 0x00))
						{
							// Found it
							if (!newLine)
								foundPos = i;

							found = true;
							break;
						}

						if (newLine)
						{
							// Found it
							found = true;
							break;
						}
					}

					sb.Append(chars, 0, foundPos);

					int byteCount = encoder.GetByteCount(chars, 0, foundPos);
					if (byteCount != bytesRead)
					{
						// Seek a little bit back
						int nullCharLen = encoder.GetByteCount("\0");

						Seek(-(bytesRead - byteCount - nullCharLen), SeekOrigin.Current);
						break;
					}

					if (found)
						break;
				}
			}

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Read sample data
		/// </summary>
		/********************************************************************/
		public sbyte[] ReadSampleData(int length, out int readBytes)
		{
			// Allocate buffer to hold the sample data
			sbyte[] sampleData = new sbyte[length];

			readBytes = ReadSampleData(sampleData, length);

			return sampleData;
		}



		/********************************************************************/
		/// <summary>
		/// Read sample data into the given buffer
		/// </summary>
		/********************************************************************/
		public int ReadSampleData(sbyte[] sampleData, int length)
		{
			return ReadSigned(sampleData, 0, length);
		}



		/********************************************************************/
		/// <summary>
		/// Read 16-bit big endian sample data. Length is the number of
		/// samples, not bytes
		/// </summary>
		/********************************************************************/
		public short[] Read_B_16BitSampleData(int length, out int readSamples)
		{
			// Allocate buffer to hold the sample data
			short[] sampleData = new short[length];

			readSamples = Read_B_16BitSampleData(sampleData, length);

			return sampleData;
		}



		/********************************************************************/
		/// <summary>
		/// Read 16-bit big endian sample data into the given buffer. Length
		/// is the number of samples, not bytes
		/// </summary>
		/********************************************************************/
		public int Read_B_16BitSampleData(short[] sampleData, int length)
		{
			long position = Position;

			ReadArray_B_INT16s(sampleData, 0, length);

			return (int)((Position - position) / 2);
		}



		/********************************************************************/
		/// <summary>
		/// Read 16-bit little endian sample data. Length is the number of
		/// samples, not bytes
		/// </summary>
		/********************************************************************/
		public short[] Read_L_16BitSampleData(int length, out int readSamples)
		{
			// Allocate buffer to hold the sample data
			short[] sampleData = new short[length];

			readSamples = Read_L_16BitSampleData(sampleData, length);

			return sampleData;
		}



		/********************************************************************/
		/// <summary>
		/// Read 16-bit little endian sample data into the given buffer. Length
		/// is the number of samples, not bytes
		/// </summary>
		/********************************************************************/
		public int Read_L_16BitSampleData(short[] sampleData, int length)
		{
			long position = Position;

			ReadArray_L_INT16s(sampleData, 0, length);

			return (int)((Position - position) / 2);
		}



		/********************************************************************/
		/// <summary>
		/// Will open a new handle to the current stream and return a new
		/// stream
		/// </summary>
		/********************************************************************/
		public ModuleStream Duplicate()
		{
			ModuleStream newStream = null;

			if (wrapperStream is FileStream fs)
				newStream = new ModuleStream(new FileStream(fs.Name, FileMode.Open, FileAccess.Read), false);

			if ((wrapperStream is DecruncherStream) || (wrapperStream is SeekableStream))
			{
				// Need to decrunch the whole file into memory
				long position = wrapperStream.Position;

				MemoryStream ms = new MemoryStream((int)wrapperStream.Length);
				wrapperStream.Seek(0, SeekOrigin.Begin);
				wrapperStream.CopyTo(ms);

				wrapperStream.Seek(position, SeekOrigin.Begin);

				newStream = new ModuleStream(ms, false);
			}

			if (wrapperStream is MemoryStream)
			{
				// Copy the whole stream into a new one
				long position = wrapperStream.Position;

				MemoryStream ms = new MemoryStream((int)wrapperStream.Length);
				wrapperStream.Seek(0, SeekOrigin.Begin);
				wrapperStream.CopyTo(ms);

				wrapperStream.Seek(position, SeekOrigin.Begin);

				newStream = new ModuleStream(ms, false);
			}

			if (newStream == null)
				throw new NotSupportedException($"Stream of type {wrapperStream.GetType()} cannot be duplicated");

			newStream.Seek(Position, SeekOrigin.Begin);

			return newStream;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will seek the module data or sample stream to the right position
		/// </summary>
		/********************************************************************/
		private void SeekToPosition()
		{
			int sampleCount = samplePositions?.Length ?? 0;

			if (sampleCount == 0)
			{
				wrapperStream.Seek(virtualPosition, SeekOrigin.Begin);
				return;
			}

			long positionCounter = 0;
			long wrapperNewPosition = 0;
			long sampleNewPosition = samplePositions![0].SampleStreamStartPosition;

			currentSampleNumber = 0;

			while (positionCounter < virtualPosition)
			{
				ConvertSamplePosition convertSamplePos = currentSampleNumber < sampleCount ? samplePositions[currentSampleNumber] : null;

				if (convertSamplePos == null)
				{
					// No more samples, so the rest of the data is module data
					wrapperNewPosition += virtualPosition - positionCounter;
					positionCounter = virtualPosition;
				}
				else if (positionCounter < convertSamplePos.StartPosition)
				{
					// We are in module data, so move either to the wanted position
					// or to the start of the next sample
					long toMove = Math.Min(virtualPosition - positionCounter, convertSamplePos.StartPosition - positionCounter);
					wrapperNewPosition += toMove;
					positionCounter += toMove;

					// If reading continues past this block of module data, the next
					// sample data to be read is the beginning of this sample
					sampleNewPosition = convertSamplePos.SampleStreamStartPosition;
				}
				else
				{
					// We are inside a sample, so move either to the wanted position
					// or to the end of the sample
					long toMove = Math.Min(virtualPosition - positionCounter, convertSamplePos.EndPosition - positionCounter);
					sampleNewPosition = convertSamplePos.SampleStreamStartPosition + toMove;
					positionCounter += toMove;

					if (positionCounter == convertSamplePos.EndPosition)
					{
						// The whole sample has been skipped, so the next sample data
						// to be read is the beginning of the following sample
						currentSampleNumber++;

						if (currentSampleNumber < sampleCount)
							sampleNewPosition = samplePositions[currentSampleNumber].SampleStreamStartPosition;
					}
				}
			}

			wrapperStream.Seek(wrapperNewPosition, SeekOrigin.Begin);
			sampleStream.Seek(sampleNewPosition, SeekOrigin.Begin);
		}
		#endregion
	}
}
