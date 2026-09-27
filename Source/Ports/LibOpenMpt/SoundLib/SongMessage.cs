/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;
using FileReader = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.FileReader;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Various functions for processing song messages (allocating, reading from file...)
	///
	/// Notes  : Those functions should offer a rather high level of abstraction compared to
	///          previous ways of reading the song messages. There are still many things to do,
	///          though. Future versions of ReadMessage() could e.g. offer charset conversion
	///          and the code is not yet ready for unicode.
	///          Some functions for preparing the message text to be written to a file would
	///          also be handy
	/// </summary>
	internal class SongMessage : StdString
	{
		/// <summary>
		/// Line ending types (for reading song messages from module files)
		/// </summary>
		public enum LineEnding
		{
			/// <summary>
			/// Carriage Return (0x0D, \r)
			/// </summary>
			Cr,

			/// <summary>
			/// Line Feed (0x0A \n)
			/// </summary>
			Lf,

			/// <summary>
			/// Carriage Return, Line Feed (0x0D0A, \r\n)
			/// </summary>
			CrLf,

			/// <summary>
			/// It is not defined whether Carriage Return or Line Feed is the actual line ending. Both are accepted
			/// </summary>
			Mixed,

			/// <summary>
			/// Detect suitable line ending
			/// </summary>
			Autodetect
		}

		/// <summary>
		/// The character that represents line endings internally
		/// </summary>
		private const uint8 InternalLineEnding = (uint8)'\r';

		/********************************************************************/
		/// <summary>
		/// Read song message from mapped file
		/// </summary>
		/********************************************************************/
		public bool Read(CPointer<byte> data, size_t length, LineEnding lineEnding)//XX 28
		{
			CPointer<uint8> str = data;

			while ((length != 0) && (str[length - 1] == '\0'))
			{
				// Ignore trailing null character
				length--;
			}

			// Simple line-ending detection algorithm. VERY simple
			if (lineEnding == LineEnding.Autodetect)
			{
				size_t nCr = 0, nLf = 0, nCrLf = 0;

				// Find CRs, LFs and CRLFs
				for (size_t i = 0; i < length; i++)
				{
					uint8 c = str[i];

					if (c == '\r')
						nCr++;
					else if (c == '\n')
						nLf++;

					if ((i != 0) && (str[i - 1] == '\r') && (c == '\n'))
						nCrLf++;
				}

				// Evaluate findings
				if ((nCr == nLf) && (nCr == nCrLf))
					lineEnding = LineEnding.CrLf;
				else if ((nCr != 0) && (nLf == 0))
					lineEnding = LineEnding.Cr;
				else if ((nCr == 0) && (nLf != 0))
					lineEnding = LineEnding.Lf;
				else
					lineEnding = LineEnding.Mixed;
			}

			size_t finalLength = 0;

			// Calculate the final amount of characters to be allocated
			for (size_t i = 0; i < length; i++)
			{
				finalLength++;

				if ((str[i] == '\r') && (lineEnding == LineEnding.CrLf))
					i++;	// Skip the LF
			}

			clear();
			reserve(finalLength);

			for (size_t i = 0; i < length; i++)
			{
				uint8 c = str[i];

				switch ((char)c)
				{
					case '\r':
					{
						if (lineEnding != LineEnding.Lf)
							c = InternalLineEnding;
						else
							c = (uint8)' ';

						if (lineEnding == LineEnding.CrLf)
							i++;	// Skip the LF

						break;
					}

					case '\n':
					{
						if ((lineEnding != LineEnding.Cr) && (lineEnding != LineEnding.CrLf))
							c = InternalLineEnding;
						else
							c = (uint8)' ';

						break;
					}

					case '\0':
					{
						c = (uint8)' ';
						break;
					}
				}

				push_back(c);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read song message from mapped file
		/// </summary>
		/********************************************************************/
		public bool Read(FileReader file, size_t length, LineEnding lineEnding)//XX 110
		{
			size_t readLength = Math.Min(length, file.BytesLeft());
			FileCursor.PinnedView fileView = file.ReadPinnedView(readLength);

			bool success = Read(fileView.Data(), fileView.Size(), lineEnding);

			return success;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve song message
		/// </summary>
		/********************************************************************/
		public StdString GetFormatted(LineEnding lineEnding)//XX 172
		{
			StdString comments = new StdString();
			comments.reserve(length());

			foreach (uint8_t c in this)
			{
				if (c == InternalLineEnding)
				{
					switch (lineEnding)
					{
						case LineEnding.Cr:
						{
							comments.push_back((uint8_t)'\r');
							break;
						}

						case LineEnding.CrLf:
						{
							comments.push_back((uint8_t)'\r');
							comments.push_back((uint8_t)'\n');
							break;
						}

						case LineEnding.Lf:
						{
							comments.push_back((uint8_t)'\n');
							break;
						}

						default:
						{
							comments.push_back((uint8_t)'\r');
							break;
						}
					}
				}
				else
					comments.push_back(c);
			}

			return comments;
		}
	}
}
