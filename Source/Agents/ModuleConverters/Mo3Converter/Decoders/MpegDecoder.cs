/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Ports.LibMpg123;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Decoders
{
	/// <summary>
	/// Decode a Mpeg sample back to normal PCM
	/// </summary>
	internal class MpegDecoder
	{
		private const int MaxSampleLength = 16000000;

		/********************************************************************/
		/// <summary>
		/// Will decode the given sample
		/// </summary>
		/********************************************************************/
		public bool Decode(DecodeSampleInfo sampleInfo)
		{
			ushort encoderDelay = sampleInfo.SampleHeader.EncoderDelay;

			if (!ReadMp3Sample(sampleInfo, 0))
				return false;

			uint bytesPerSample = (sampleInfo.SampleHeader.Flags & SampleInfoFlag._16Bit) != 0 ? 2U : 1;
			uint sampleSizeInBytes = (uint)sampleInfo.SampleData.Length;

			if ((encoderDelay > 0) && (encoderDelay < sampleSizeInBytes))
			{
				uint lengthInBytes = sampleSizeInBytes - encoderDelay;

				uint lengthInSamples = lengthInBytes / bytesPerSample;
				if (lengthInSamples > sampleInfo.SampleHeader.Length)
					lengthInSamples = sampleInfo.SampleHeader.Length;

				sampleInfo.SampleHeader.Length = lengthInSamples;
				lengthInBytes = lengthInSamples * bytesPerSample;

				sampleInfo.SampleData = sampleInfo.SampleData.AsSpan(encoderDelay, (int)lengthInBytes).ToArray();

				if (sampleInfo.SampleHeader.LoopEnd > sampleInfo.SampleHeader.Length)
					sampleInfo.SampleHeader.LoopEnd = sampleInfo.SampleHeader.Length;
			}

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will decode the given sample
		/// </summary>
		/********************************************************************/
		private bool ReadMp3Sample(DecodeSampleInfo sampleInfo, int startIndex)
		{
			LibMpg123 mpg123Handle = LibMpg123.Mpg123_New(null, out Mpg123_Errors error);
			if (error != Mpg123_Errors.Ok)
				return false;

			try
			{
				if (mpg123Handle.Mpg123_Param(Mpg123_Parms.Add_Flags, (int)Mpg123_Param_Flags.Quiet, 0.0) != Mpg123_Errors.Ok)
					return false;

				if (mpg123Handle.Mpg123_Param(Mpg123_Parms.Add_Flags, (int)Mpg123_Param_Flags.Auto_Resample, 0.0) != Mpg123_Errors.Ok)
					return false;

				if (mpg123Handle.Mpg123_Param(Mpg123_Parms.Remove_Flags, (int)Mpg123_Param_Flags.Gapless, 0.0) != Mpg123_Errors.Ok)
					return false;

				if (mpg123Handle.Mpg123_Param(Mpg123_Parms.Add_Flags, (int)Mpg123_Param_Flags.Ignore_InfoFrame, 0.0) != Mpg123_Errors.Ok)
					return false;

				if (mpg123Handle.Mpg123_Param(Mpg123_Parms.Remove_Flags, (int)Mpg123_Param_Flags.Skip_Id3V2, 0.0) != Mpg123_Errors.Ok)
					return false;

				if (mpg123Handle.Mpg123_Param(Mpg123_Parms.Add_Flags, (int)Mpg123_Param_Flags.Ignore_StreamLength, 0.0) != Mpg123_Errors.Ok)
					return false;

				if (mpg123Handle.Mpg123_Param(Mpg123_Parms.Index_Size, -1000, 0.0) != Mpg123_Errors.Ok)
					return false;

				if (mpg123Handle.Mpg123_Reader64(Read, Seek, null) != Mpg123_Errors.Ok)
					return false;

				using (MemoryStream ms = new MemoryStream(sampleInfo.Chunk, startIndex, sampleInfo.Chunk.Length - startIndex))
				{
					if (mpg123Handle.Mpg123_Open_Handle64(ms) != Mpg123_Errors.Ok)
						return false;

					if (mpg123Handle.Mpg123_Scan() != Mpg123_Errors.Ok)
						return false;

					if (mpg123Handle.Mpg123_GetFormat(out long rate, out Mpg123_ChannelCount channels, out Mpg123_Enc_Enum encoding) != Mpg123_Errors.Ok)
						return false;

					if (((channels != Mpg123_ChannelCount.Mono) && (channels != Mpg123_ChannelCount.Stereo)) || ((encoding & (Mpg123_Enc_Enum.Enc_16 | Mpg123_Enc_Enum.Enc_Signed)) != (Mpg123_Enc_Enum.Enc_16 | Mpg123_Enc_Enum.Enc_Signed)))
						return false;

					if (mpg123Handle.Mpg123_Info(out Mpg123_FrameInfo frameInfo) != Mpg123_Errors.Ok)
						return false;

					if ((frameInfo.Layer < 1) || (frameInfo.Layer > 3))
						return false;

					// We force sample rate, channels and sample format, which in
					// combination with auto-resample (set above) will cause libmpg123
					// to stay with the given format even for completely confused
					// MPG123_FRANKENSTEIN streams.
					// Note that we cannot rely on mpg123_length() for the way we
					// decode the mpeg streams because it depends on the actual frame
					// sample rate instead of the returned sample rate
					if (mpg123Handle.Mpg123_Param(Mpg123_Parms.Force_Rate, rate, 0.0) != Mpg123_Errors.Ok)
						return false;

					if (mpg123Handle.Mpg123_Param(Mpg123_Parms.Add_Flags, (int)channels > 1 ? (int)Mpg123_ChannelCount.Stereo : (int)Mpg123_ChannelCount.Mono, 0.0) != Mpg123_Errors.Ok)
						return false;

					List<byte> data = new List<byte>();
					byte[] bufBytes = null;

					bool decodeError = false;
					bool decodeDone = false;

					while (!decodeError && !decodeDone)
					{
						ulong blockSize = mpg123Handle.Mpg123_OutBlock();
						if ((bufBytes == null) || (blockSize > (ulong)bufBytes.Length))
							bufBytes = new byte[blockSize];

						Mpg123_Errors result = mpg123Handle.Mpg123_Read(bufBytes, (ulong)bufBytes.Length, out ulong bufBytesDecoded);
						data.AddRange(bufBytes.AsSpan(0, (int)bufBytesDecoded));

						if ((data.Count / (int)channels) > MaxSampleLength)
							break;

						if (result == Mpg123_Errors.Ok)
						{
							// Continue
						}
						else if (result == Mpg123_Errors.New_Format)
						{
							// Continue
						}
						else if (result == Mpg123_Errors.Done)
						{
							decodeDone = true;
						}
						else
						{
							decodeError = true;
						}
					}

					if ((data.Count / (int)channels) > MaxSampleLength)
						return false;

					sampleInfo.SampleData = data.ToArray();
				}
			}
			finally
			{
				mpg123Handle.Mpg123_Delete();
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read from the file
		/// </summary>
		/********************************************************************/
		private int Read(object handle, Memory<byte> buf, ulong count, out ulong readCount)
		{
			Stream stream = (Stream)handle;

			readCount = (ulong)stream.Read(buf.Span.Slice(0, (int)count));

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Seek in the file
		/// </summary>
		/********************************************************************/
		private long Seek(object handle, long offset, SeekOrigin whence)
		{
			Stream stream = (Stream)handle;

			return stream.Seek(offset, whence);
		}
		#endregion
	}
}
