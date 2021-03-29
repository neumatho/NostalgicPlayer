/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.RiffWave
{
	/// <summary>
	/// Derive from this class, if your format supports saving
	/// </summary>
	internal abstract class RiffWaveSaverWorkerBase : RiffWaveLoaderWorkerBase, ISampleSaverAgent
	{
		protected SaveSampleFormatInfo format;

		private uint dataPosition;
		private byte[] saveBuffer;

		private int total;

		#region ISampleSaverAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		/********************************************************************/
		public abstract SampleSaverSupportFlag SaverSupportFlags
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the file extension that is used by the saver
		/// </summary>
		/********************************************************************/
		public string FileExtension => "wav";



		/********************************************************************/
		/// <summary>
		/// Initialize the saver so it is prepared to save the sample
		/// </summary>
		/********************************************************************/
		public virtual bool InitSaver(SaveSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Remember arguments
			format = formatInfo;

			// Initialize variables
			saveBuffer = null;
			total = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the saver
		/// </summary>
		/********************************************************************/
		public virtual void CleanupSaver()
		{
			saveBuffer = null;
			format = null;
		}



		/********************************************************************/
		/// <summary>
		/// Save the header of the sample
		/// </summary>
		/********************************************************************/
		public void SaveHeader(Stream stream)
		{
			using (WriterStream writerStream = new WriterStream(stream))
			{
				// Write RIFF header
				writerStream.Write_B_UINT32(0x52494646);						// RIFF
				writerStream.Write_L_UINT32(0);									// File size
				writerStream.Write_B_UINT32(0x57415645);						// WAVE

				// Write the fmt chunk
				writerStream.Write_B_UINT32(0x666d7420);						// fmt
				writerStream.Write_L_UINT32(0);									// Chunk size
				writerStream.Write_L_UINT16((ushort)FormatId);						// Format ID
				writerStream.Write_L_UINT16((ushort)format.Channels);				// Number of channels
				writerStream.Write_L_UINT32((uint)format.Frequency);				// Sampling rate
				writerStream.Write_L_UINT32(GetAverageBytesPerSecond());		// Average bytes per second
				writerStream.Write_L_UINT16(GetBlockAlign());					// Block align

				// Write extra fmt chunk information
				WriteExtraFmtInfo(writerStream);

				// Write the chunk size
				uint pos = (uint)writerStream.Position;
				writerStream.Seek(16, SeekOrigin.Begin);
				writerStream.Write_L_UINT32(pos - 36 + 16);
				writerStream.Seek(0, SeekOrigin.End);

				// Write the fact chunk
				WriteFactChunk(writerStream);

				// Write the data chunk
				dataPosition = (uint)writerStream.Position;		// Remember the position of the data chunk

				writerStream.Write_B_UINT32(0x64617461);						// data
				writerStream.Write_L_UINT32(0);									// Chunk size
			}
		}



		/********************************************************************/
		/// <summary>
		/// Save a part of the sample
		/// </summary>
		/********************************************************************/
		public void SaveData(Stream stream, int[] buffer, int length)
		{
			if (length > 0)
			{
				// Do we need to reallocate the buffer?
				if ((saveBuffer == null) || (length > saveBuffer.Length))
				{
					// Allocate new buffer to store the converted samples into
					saveBuffer = new byte[length * (format.Bits / 8)];
				}

				int bytesWritten = WriteData(buffer, length, saveBuffer);
				if (bytesWritten > 0)
				{
					stream.Write(saveBuffer, 0, bytesWritten);
					total += bytesWritten;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Save the tail of the sample
		/// </summary>
		/********************************************************************/
		public void SaveTail(Stream stream)
		{
			using (WriterStream writerStream = new WriterStream(stream))
			{
				// Write anything left
				int bytesWritten = WriteTail(saveBuffer);
				if (bytesWritten > 0)
				{
					writerStream.Write(saveBuffer, 0, bytesWritten);
					total += bytesWritten;
				}

				// Change the data chunk size
				writerStream.Seek(dataPosition + 4, SeekOrigin.Begin);
				writerStream.Write_L_UINT32((uint)total);

				// Change the RIFF size
				writerStream.Seek(4, SeekOrigin.Begin);
				writerStream.Write_L_UINT32((uint)(dataPosition + (8 + total) - 8));
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Returns the average bytes per second
		/// </summary>
		/********************************************************************/
		protected abstract uint GetAverageBytesPerSecond();



		/********************************************************************/
		/// <summary>
		/// Returns the block align
		/// </summary>
		/********************************************************************/
		protected abstract ushort GetBlockAlign();



		/********************************************************************/
		/// <summary>
		/// Returns the bit size of each sample
		/// </summary>
		/********************************************************************/
		protected virtual ushort GetSampleSize(int sampleSize)
		{
			return (ushort)sampleSize;
		}



		/********************************************************************/
		/// <summary>
		/// Writes any extra information into the fmt chunk
		/// </summary>
		/********************************************************************/
		protected virtual void WriteExtraFmtInfo(WriterStream writerStream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Writes the fact chunk
		/// </summary>
		/********************************************************************/
		protected virtual void WriteFactChunk(WriterStream writerStream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Writes a block of data
		/// </summary>
		/********************************************************************/
		protected abstract int WriteData(int[] buffer, int length, byte[] outputBuffer);



		/********************************************************************/
		/// <summary>
		/// Write the last data or fixing up chunks
		/// </summary>
		/********************************************************************/
		protected virtual int WriteTail(byte[] outputBuffer)
		{
			return 0;
		}
	}
}
