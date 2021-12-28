/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This class wraps another stream and adds some helper methods to read the data
	/// </summary>
	public class ModuleStream : ReaderStream
	{
		private readonly Dictionary<int, ConvertSampleInfo> sampleInfo;
		private readonly Stream sampleStream;
		private readonly long convertedLength;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleStream(Stream wrapperStream, bool leaveOpen) : base(wrapperStream, leaveOpen)
		{
			sampleInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor - used by module converters
		/// </summary>
		/********************************************************************/
		public ModuleStream(Stream wrapperStream, Dictionary<int, ConvertSampleInfo> sampleInfo) : base(wrapperStream, true)
		{
			this.sampleInfo = sampleInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor - used by player loader
		/// </summary>
		/********************************************************************/
		public ModuleStream(Stream wrapperStream, Stream sampleDataStream, long totalLength) : base(wrapperStream, true)
		{
			sampleStream = sampleDataStream;
			convertedLength = totalLength;
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
		#endregion

		/********************************************************************/
		/// <summary>
		/// Will read a line. The line is a character sequence4 which is
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
		/// Will read a comment
		/// </summary>
		/********************************************************************/
		public string ReadComment(int len, Encoding encoder)
		{
			if (len == 0)
				return string.Empty;

			byte[] buffer = new byte[len + 1];
			ReadString(buffer, len);

			string comment = encoder.GetString(buffer);

			// Translate linefeeds
			comment = comment.Replace('\u266a', '\n');

			return comment;
		}



		/********************************************************************/
		/// <summary>
		/// Will read a comment field which is stored as a block of lines
		/// and return them
		/// </summary>
		/********************************************************************/
		public string[] ReadCommentBlock(int blockSize, int lineLength, Encoding encoder)
		{
			if (blockSize == 0)
				return new string[0];

			int numberOfLines = (blockSize + lineLength - 1) / lineLength;
			string[] lines = new string[numberOfLines];

			byte[] lineBuffer = new byte[lineLength + 1];

			// Store the lines in reverse order in the array.
			// This helps to skip empty lines in the each later on
			for (int i = numberOfLines - 1; blockSize > 0; i--)
			{
				int todo = Math.Min(lineLength, blockSize);
				Read(lineBuffer, 0, todo);
				lineBuffer[todo] = 0x00;			// Null terminator, just in case

				string singleLine = encoder.GetString(lineBuffer, 0, lineLength);
				singleLine = singleLine.Replace('\r', ' ').Replace('\n', ' ').TrimEnd();

				lines[i] = singleLine;

				blockSize -= todo;
			}

			return lines.SkipWhile(string.IsNullOrWhiteSpace).Reverse().ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Remember the sample position and size. Only used in module
		/// converters
		/// </summary>
		/********************************************************************/
		public void SetSampleDataInfo(int sampleNumber, int length)
		{
			if (sampleInfo == null)
				throw new Exception("SetSampleDataInfo() may only be called from module converter");

			// Called from a module converter
			if (sampleInfo.ContainsKey(sampleNumber))
			{
				// If the sample number already is in the list, it is
				// probably because the converter is called from a second
				// or more round and therefore the sample data information
				// is already stored
				return;
			}

			sampleInfo[sampleNumber] = new ConvertSampleInfo { Position = (uint)Position, Length = length };

			// Set "end of stream" if all the data isn't there
			EndOfStream = (Length - Position) < length;

			// Skip the sample data
			if (EndOfStream)
				Seek(0, SeekOrigin.End);
			else
				Seek(length, SeekOrigin.Current);
		}



		/********************************************************************/
		/// <summary>
		/// Read sample data. If called from a module converter, no data is
		/// returned, but the position and size are stored to use for the
		/// player
		/// </summary>
		/********************************************************************/
		public sbyte[] ReadSampleData(int sampleNumber, int length, out int readBytes)
		{
			// Allocate buffer to hold the sample data
			sbyte[] sampleData = new sbyte[length];

			readBytes = ReadSampleData(sampleNumber, sampleData, length);

			return sampleData;
		}



		/********************************************************************/
		/// <summary>
		/// Read sample data. If called from a module converter, no data is
		/// returned, but the position and size are stored to use for the
		/// player. Else the sample data are stored in the given buffer
		/// </summary>
		/********************************************************************/
		public int ReadSampleData(int sampleNumber, sbyte[] sampleData, int length)
		{
			if (sampleInfo != null)
				throw new Exception("ReadSampleData() may not be called from module converter");

			if (sampleStream != null)
			{
				// Is called from a player with a converted module
				//
				// Read position and length from the converted stream
				uint pos = Read_B_UINT32();
				int len = (int)Read_B_UINT32();

				if (len != length)
					throw new Exception($"Something is wrong when reading sample data. The given sample length {length} does not match the one stored in the converted data {len} for sample number {sampleNumber}");

				// Seek to the right position in the original file and read the data there
				sampleStream.Seek(pos, SeekOrigin.Begin);

				return sampleStream.Read((byte[])(Array)sampleData, 0, length);
			}

			// Not converted module or anything, just a plain file, so read the data
			return ReadSigned(sampleData, 0, length);
		}



		/********************************************************************/
		/// <summary>
		/// Return a stream to the sample data. May only be called from
		/// players
		/// </summary>
		/********************************************************************/
		public ModuleStream GetSampleDataStream(int sampleNumber, int length)
		{
			if (sampleInfo != null)
				throw new Exception("GetSampleDataStream() may not be called from a module converter");

			if (sampleStream != null)
			{
				// Converted module
				//
				// Read position and length from the converted stream
				uint pos = Read_B_UINT32();
				int len = (int)Read_B_UINT32();

				if (len != length)
					throw new Exception($"Something is wrong when reading sample data. The given sample length {length} does not match the one stored in the converted data {len} for sample number {sampleNumber}");

				// Seek to the right position in the original file
				sampleStream.Seek(pos, SeekOrigin.Begin);

				// Return new stream
				return new ModuleStream(sampleStream, true);
			}

			// Not converted, so just return current stream wrapped
			return new ModuleStream(wrapperStream, true);
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

			if (newStream == null)
				throw new NotSupportedException($"Stream of type {wrapperStream.GetType()} cannot be duplicated");

			newStream.Seek(Position, SeekOrigin.Begin);

			return newStream;
		}
	}
}
