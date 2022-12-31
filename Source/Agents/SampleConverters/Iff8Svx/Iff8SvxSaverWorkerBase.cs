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
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Iff8Svx
{
	/// <summary>
	/// Derive from this class, if your format supports saving
	/// </summary>
	internal abstract class Iff8SvxSaverWorkerBase : Iff8SvxLoaderWorkerBase, ISampleSaverAgent
	{
		protected SaveSampleFormatInfo saveFormat;

		private FileStream stereoFile;

		private uint dataPosition;

		private uint total;
		private uint loopLength;

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
		public string FileExtension => "8svx";



		/********************************************************************/
		/// <summary>
		/// Initialize the saver so it is prepared to save the sample
		/// </summary>
		/********************************************************************/
		public virtual bool InitSaver(SaveSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Remember arguments
			saveFormat = formatInfo;

			// Initialize variables
			stereoFile = null;
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
			saveFormat = null;
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
				byte[] strBuf;
				int strLen;

				Encoding encoder = EncoderCollection.Amiga;

				// Write FORM header
				writerStream.Write_B_UINT32(0x464f524d);						// FORM
				writerStream.Write_B_UINT32(0);									// File size
				writerStream.Write_B_UINT32(0x38535658);						// 8SVX

				// Write the VHDR chunk
				writerStream.Write_B_UINT32(0x56484452);						// VHDR
				writerStream.Write_B_UINT32(20);								// Chunk size
				writerStream.Write_B_UINT32(0);									// One-shot part length

				if (saveFormat.LoopLength <= 2)
					loopLength = 0;
				else
					loopLength = (uint)saveFormat.LoopLength;

				writerStream.Write_B_UINT32(loopLength);							// Repeat part length

				writerStream.Write_B_UINT32(0);									// Samples/cycle in the high octave (0 means unknown)
				writerStream.Write_B_UINT16((ushort)saveFormat.Frequency);			// Samples per second
				writerStream.Write_UINT8(1);									// Number of octaves
				writerStream.Write_UINT8((byte)FormatId);							// Format value
				writerStream.Write_B_UINT32(0x00010000);						// Volume

				// Write the NAME chunk
				if (!string.IsNullOrEmpty(saveFormat.Name))
				{
					strBuf = encoder.GetBytes(saveFormat.Name);
					strLen = strBuf.Length;

					writerStream.Write_B_UINT32(0x4e414d45);					// NAME
					writerStream.Write_B_UINT32((uint)strLen);						// Chunk size
					writerStream.Write(strBuf, 0, strLen);

					if ((strLen % 2) != 0)
						writerStream.Write_UINT8(0);
				}

				// Write the AUTH chunk
				if (!string.IsNullOrEmpty(saveFormat.Author))
				{
					strBuf = encoder.GetBytes(saveFormat.Author);
					strLen = strBuf.Length;

					writerStream.Write_B_UINT32(0x41555448);					// AUTH
					writerStream.Write_B_UINT32((uint)strLen);						// Chunk size
					writerStream.Write(strBuf, 0, strLen);

					if ((strLen % 2) != 0)
						writerStream.Write_UINT8(0);
				}

				// Write the ANNO chunk
				strBuf = encoder.GetBytes(Resources.IDS_IFF8SVX_ANNO);
				strLen = strBuf.Length;

				writerStream.Write_B_UINT32(0x414e4e4f);					// ANNO
				writerStream.Write_B_UINT32((uint)strLen);						// Chunk size
				writerStream.Write(strBuf, 0, strLen);

				if ((strLen % 2) != 0)
					writerStream.Write_UINT8(0);

				// Should we write in stereo?
				if (saveFormat.Channels == 2)
				{
					// Yes, write a CHAN chunk
					writerStream.Write_B_UINT32(0x4348414e);				// CHAN
					writerStream.Write_B_UINT32(4);							// Chunk size
					writerStream.Write_B_UINT32(6);							// Stereo indicator

					// We need to open an extra file, because IFF-8SVX
					// stereo files is not stored like normal left, right, left, right...
					// IFF-8SVX files are stored with all the left channel samples
					// first and then the right channel samples. The extra file
					// is used to store the right channel samples. When all the samples
					// are written, the two files (main + extra) will be appended together
					FileStream fs = stream as FileStream;
					if (fs == null)
						throw new NotSupportedException($"Writing IFF-8SVX stereo files using stream of type {stream.GetType()} is not supported");

					// Build a new file name
					string newName = fs.Name + "nps" + new Random().Next();

					stereoFile = new FileStream(newName, FileMode.CreateNew, FileAccess.ReadWrite);
				}

				// Write the BODY chunk
				dataPosition = (uint)writerStream.Position;						// Remember the position of the BODY chunk

				writerStream.Write_B_UINT32(0x424f4459);					// BODY
				writerStream.Write_L_UINT32(0);								// Chunk size
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
				using (WriterStream writerStream = new WriterStream(stream))
				{
					total += WriteData(writerStream, stereoFile, buffer, length);
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
				total += WriteTail(writerStream, stereoFile);

				// Change the one-shot length
				writerStream.Seek(20, SeekOrigin.Begin);

				if (saveFormat.LoopStart != 0)
					writerStream.Write_B_UINT32((uint)saveFormat.LoopStart);
				else
					writerStream.Write_B_UINT32(total - loopLength);

				// Append the right channel file with the main file if any
				if (stereoFile != null)
				{
					// Set the file pointers to the right positions
					stereoFile.Seek(0, SeekOrigin.Begin);
					writerStream.Seek(0, SeekOrigin.End);

					// Copy the data
					stereoFile.CopyTo(writerStream);

					// Get the full path to the file
					string stereoName = stereoFile.Name;

					// Close the file
					stereoFile.Dispose();
					stereoFile = null;

					// Delete the file
					File.Delete(stereoName);
				}

				// Change the BODY chunk size
				uint bodyLen = (uint)writerStream.Length - dataPosition - 8;
				writerStream.Seek(dataPosition + 4, SeekOrigin.Begin);
				writerStream.Write_B_UINT32(bodyLen);

				// Change the FORM size
				writerStream.Seek(4, SeekOrigin.Begin);
				writerStream.Write_B_UINT32(dataPosition + 8 + bodyLen - 8);
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Writes a block of data
		/// </summary>
		/********************************************************************/
		protected abstract uint WriteData(WriterStream stream, Stream stereoStream, int[] buffer, int length);



		/********************************************************************/
		/// <summary>
		/// Write the last data or fixing up chunks
		/// </summary>
		/********************************************************************/
		protected virtual uint WriteTail(WriterStream stream, Stream stereoStream)
		{
			return 0;
		}
	}
}
