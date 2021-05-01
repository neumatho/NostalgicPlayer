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
		public ModuleStream(Stream wrapperStream, Stream sampleDataStream) : base(wrapperStream, true)
		{
			sampleStream = sampleDataStream;
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
		public sbyte[] ReadSampleData(int sampleNumber, int length)
		{
			if (sampleInfo != null)
				throw new Exception("ReadSampleData() may not be called from module converter");

			// Allocate buffer to hold the sample data
			sbyte[] data = new sbyte[length];

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

				sampleStream.Read((byte[])(Array)data, 0, length);

				return data;
			}

			// Not converted module or anything, just a plain file, so read the data
			ReadSigned(data, 0, length);

			return data;
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

			if (newStream == null)
				throw new NotSupportedException($"Stream of type {wrapperStream.GetType()} cannot be duplicated");

			newStream.Seek(Position, SeekOrigin.Begin);

			return newStream;
		}
	}
}
