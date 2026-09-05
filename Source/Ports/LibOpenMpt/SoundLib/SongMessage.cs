/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std;

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
			leCr,

			/// <summary>
			/// Line Feed (0x0A \n)
			/// </summary>
			leLf,

			/// <summary>
			/// Carriage Return, Line Feed (0x0D0A, \r\n)
			/// </summary>
			leCrLf,

			/// <summary>
			/// It is not defined whether Carriage Return or Line Feed is the actual line ending. Both are accepted
			/// </summary>
			leMixed,

			/// <summary>
			/// Detect suitable line ending
			/// </summary>
			leAutodetect
		}

		/// <summary>
		/// The character that represents line endings internally
		/// </summary>
		private const uint8 InternalLineEnding = (uint8)'\r';

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
						case LineEnding.leCr:
						{
							comments.push_back((uint8_t)'\r');
							break;
						}

						case LineEnding.leCrLf:
						{
							comments.push_back((uint8_t)'\r');
							comments.push_back((uint8_t)'\n');
							break;
						}

						case LineEnding.leLf:
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
