/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha
{
	internal partial class LhaCore
	{
		private static readonly int[] dsboy = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };

		private byte[] getPtr;
		private int getOffset;

		/********************************************************************/
		/// <summary>
		/// Read the entry header
		/// </summary>
		/********************************************************************/
		public bool GetHeader(out LzHeader hdr)
		{
			hdr = new LzHeader();

			int headerSize = stream.ReadByte();
			if ((headerSize == -1) || (headerSize == 0))
				return false;	// Finish

			byte[] data = new byte[Constants.LzHeader_Strage];
			if (stream.Read(data, Constants.I_Header_Checksum, headerSize - 1) < headerSize - 1)
				throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_EOF, "Header"));

			byte[] dirName = new byte[Constants.FileName_Length];
			int dirLength = 0;

			SetupGet(data, Constants.I_Header_Level);
			hdr.HeaderLevel = GetByte();
			if ((hdr.HeaderLevel != 2) && (stream.Read(data, headerSize, 2) < 2))
				throw new DecruncherException(agentName, Resources.IDS_ARD_ERR_INVALID_HEADER);

			if (hdr.HeaderLevel >= 3)
				throw new DecruncherException(agentName, Resources.IDS_ARD_ERR_INVALID_HEADER);

			SetupGet(data, Constants.I_Header_Checksum);
			int checksum = GetByte();

			if (hdr.HeaderLevel == 2)
				hdr.HeaderSize = (byte)(headerSize + checksum * 256);	// ???
			else
				hdr.HeaderSize = (byte)headerSize;

			BCopy(data, Constants.I_Method, hdr.Method, Constants.Method_Type_Strage);

			SetupGet(data, Constants.I_Packed_Size);
			hdr.PackedSize = GetLongWord();
			hdr.OriginalSize = GetLongWord();
			hdr.LastModifiedStamp = GetLongWord();
			hdr.Attribute = GetByte();

			int nameLength, extendSize;

			if ((hdr.HeaderLevel = GetByte()) != 2)
			{
				if (CalcSum(data, Constants.I_Method, headerSize) != checksum)
					throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_CRC, "Header"));

				nameLength = GetByte();
				for (int i = 0; i < nameLength; i++)
					hdr.Name[i] = GetByte();

				hdr.Name[nameLength] = 0x00;
			}
			else
			{
				hdr.UnixLastModifiedStamp = hdr.LastModifiedStamp;
				nameLength = 0;
			}

			// Defaults for other type
			hdr.UnixMode = (ushort)(Constants.Unix_File_Regular | Constants.Unix_RW_RW_RW);
			hdr.UnixGid = 0;
			hdr.UnixUId = 0;

			if (hdr.HeaderLevel == 0)
			{
				extendSize = headerSize - nameLength - 22;
				if (extendSize < 0)
				{
					if (extendSize == -2)
					{
						hdr.ExtendType = Constants.Extend_Generic;
						hdr.HasCrc = false;
					}
					else
						throw new DecruncherException(agentName, Resources.IDS_ARD_ERR_UNKNOWN_HEADER);
				}
				else
				{
					hdr.HasCrc = true;
					hdr.Crc = GetWord();
				}

				if (extendSize >= 1)
				{
					hdr.ExtendType = GetByte();
					extendSize--;
				}

				if (hdr.ExtendType == Constants.Extend_Unix)
				{
					if (extendSize >= 11)
					{
						hdr.MinorVersion = GetByte();
						hdr.UnixLastModifiedStamp = GetLongWord();
						hdr.UnixMode = GetWord();
						hdr.UnixUId = GetWord();
						hdr.UnixGid = GetWord();
						extendSize -= 11;
					}
					else
						hdr.ExtendType = Constants.Extend_Generic;
				}

				while (extendSize-- > 0)
					GetByte();

				if (hdr.ExtendType == Constants.Extend_Unix)
				{
					hdr.DecodedName = EncoderCollection.Amiga.GetString(hdr.Name);
					return true;
				}
			}
			else if (hdr.HeaderLevel == 1)
			{
				hdr.HasCrc = true;
				extendSize = headerSize - nameLength - 25;
				hdr.Crc = GetWord();
				hdr.ExtendType = GetByte();

				while (extendSize-- > 0)
					GetByte();
			}
			else // Level 2
			{
				hdr.HasCrc = true;
				hdr.Crc = GetWord();
				hdr.ExtendType = GetByte();
			}

			if (hdr.HeaderLevel > 0)
			{
				// Extend header
				if (hdr.HeaderLevel != 2)
					SetupGet(data, hdr.HeaderSize);

				int ptr = getOffset;

				while ((headerSize = GetWord()) != 0)
				{
					if ((hdr.HeaderLevel != 2) && ((Constants.LzHeader_Strage - getOffset < headerSize) || (stream.Read(getPtr, getOffset, headerSize) < headerSize)))
						throw new DecruncherException(agentName, Resources.IDS_ARD_ERR_INVALID_HEADER);

					switch (GetByte())
					{
						// Header CRC
						case 0:
						{
							SetupGet(getPtr, getOffset + headerSize - 3);
							break;
						}

						// File name
						case 1:
						{
							for (int i = 0; i < headerSize - 3; i++)
								hdr.Name[i] = GetByte();

							hdr.Name[headerSize - 3] = 0x00;
							nameLength = headerSize - 3;
							break;
						}

						// Directory
						case 2:
						{
							for (int i = 0; i < headerSize - 3; i++)
								dirName[i] = GetByte();

							dirName[headerSize - 3] = 0x00;
							ConvDelim(dirName, Constants.Delim);
							dirLength = headerSize - 3;
							break;
						}

						// MS-DOS attribute
						case 0x40:
						{
							if ((hdr.ExtendType == Constants.Extend_MsDos) || (hdr.ExtendType == Constants.Extend_Human) || (hdr.ExtendType == Constants.Extend_Generic))
								hdr.Attribute = (byte)GetWord();

							break;
						}

						// UNIX permission
						case 0x50:
						{
							if (hdr.ExtendType == Constants.Extend_Unix)
								hdr.UnixMode = GetWord();

							break;
						}

						// UNIX gid and uid
						case 0x51:
						{
							if (hdr.ExtendType == Constants.Extend_Unix)
							{
								hdr.UnixGid = GetWord();
								hdr.UnixUId = GetWord();
							}
							break;
						}

						// UNIX group name
						case 0x52:
						{
							SetupGet(getPtr, getOffset + headerSize - 3);
							break;
						}

						// UNIX user name
						case 0x53:
						{
							SetupGet(getPtr, getOffset + headerSize - 3);
							break;
						}

						// UNIX last modified time
						case 0x54:
						{
							if (hdr.ExtendType == Constants.Extend_Unix)
								hdr.UnixLastModifiedStamp = GetLongWord();

							break;
						}

						// Other headers
						default:
						{
							SetupGet(getPtr, getOffset + headerSize - 3);
							break;
						}
					}
				}

				if ((hdr.HeaderLevel != 2) && (getOffset - ptr != 2))
				{
					hdr.PackedSize -= getOffset - ptr - 2;
					hdr.HeaderSize += (byte)(getOffset - ptr - 2);
				}
			}

			switch (hdr.ExtendType)
			{
				case Constants.Extend_MsDos:
				{
					MsDosToUnixFileName(hdr.Name, nameLength);
					MsDosToUnixFileName(dirName, dirLength);
					goto case Constants.Extend_Human;
				}

				case Constants.Extend_Human:
				{
					if (hdr.HeaderLevel == 2)
						hdr.UnixLastModifiedStamp = hdr.LastModifiedStamp;
					else
						hdr.UnixLastModifiedStamp = GenericToUnixStamp(hdr.LastModifiedStamp);

					break;
				}

				case Constants.Extend_Unix:
					break;

				case Constants.Extend_MacOs:
				{
					MacOsToUnixFileName(hdr.Name, nameLength);
					hdr.UnixLastModifiedStamp = GenericToUnixStamp(hdr.LastModifiedStamp);
					break;
				}

				default:
				{
					GenericToUnixFileName(hdr.Name, nameLength);
					GenericToUnixFileName(dirName, dirLength);

					if (hdr.HeaderLevel == 2)
						hdr.UnixLastModifiedStamp = hdr.LastModifiedStamp;
					else
						hdr.UnixLastModifiedStamp = GenericToUnixStamp(hdr.LastModifiedStamp);

					break;
				}
			}

			Encoding encoder;

			switch (hdr.ExtendType)
			{
				case Constants.Extend_MsDos:
				{
					encoder = EncoderCollection.Dos;
					break;
				}

				case Constants.Extend_MacOs:
				{
					encoder = EncoderCollection.Macintosh;
					break;
				}

				default:
				{
					encoder = EncoderCollection.Amiga;
					break;
				}
			}

			if (dirLength != 0)
				hdr.DecodedName = encoder.GetString(dirName) + encoder.GetString(hdr.Name);
			else
				hdr.DecodedName = encoder.GetString(hdr.Name);

			hdr.ConvertedLastModifiedStamp = DateTimeOffset.FromUnixTimeSeconds(hdr.UnixLastModifiedStamp).DateTime;

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Copy from one buffer to another
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void BCopy(byte[] s, int so, byte[] d, int n)
		{
			Array.Copy(s, so, d, 0, n);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate checksum
		/// </summary>
		/********************************************************************/
		private int CalcSum(byte[] p, int offset, int len)
		{
			int sum;

			for (sum = 0; len != 0; len--)
				sum += p[offset++];

			return sum & 0xff;
		}



		/********************************************************************/
		/// <summary>
		/// Setup the get pointer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetupGet(byte[] ptr, int offset)
		{
			getPtr = ptr;
			getOffset = offset;
		}



		/********************************************************************/
		/// <summary>
		/// Return a single byte
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private byte GetByte()
		{
			return getPtr[getOffset++];
		}



		/********************************************************************/
		/// <summary>
		/// Return a word
		/// </summary>
		/********************************************************************/
		private ushort GetWord()
		{
			int b0 = GetByte();
			int b1 = GetByte();

			return (ushort)((b1 << 8) + b0);
		}



		/********************************************************************/
		/// <summary>
		/// Return a long-word
		/// </summary>
		/********************************************************************/
		private int GetLongWord()
		{
			int b0 = GetByte();
			int b1 = GetByte();
			int b2 = GetByte();
			int b3 = GetByte();

			return (b3 << 24) + (b2 << 16) + (b1 << 8) + b0;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a file name from MS-DOS to UNIX format
		/// </summary>
		/********************************************************************/
		private void MsDosToUnixFileName(byte[] name, int len)
		{
			for (int i = 0; i < len; i++)
			{
				if (name[i] == (byte)'\\')
					name[i] = (byte)'/';
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert a file name from generic to UNIX format
		/// </summary>
		/********************************************************************/
		private void GenericToUnixFileName(byte[] name, int len)
		{
			for (int i = 0; i < len; i++)
			{
				if (name[i] == (byte)'\\')
					name[i] = (byte)'/';
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert a file name from Mac-OS to UNIX format
		/// </summary>
		/********************************************************************/
		private void MacOsToUnixFileName(byte[] name, int len)
		{
			for (int i = 0; i < len; i++)
			{
				if (name[i] == (byte)':')
					name[i] = (byte)'/';
				else if (name[i] == (byte)'/')
					name[i] = (byte)':';
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert a generic time stamp to UNIX format
		/// </summary>
		/********************************************************************/
		private int GenericToUnixStamp(int t)
		{
			// Generic stamp format:
			//
			// 31 30 29 28 27 26 25 24 23 22 21 20 19 18 17 16
			//|<------ year ------>|<- month ->|<---- day --->|
			//
			// 15 14 13 12 11 10  9  8  7  6  5  4  3  2  1  0
			//|<--- hour --->|<---- minute --->|<- second*2 ->|

			// Special case: if MSDOS format date and time were zero, then we
			// set time to be zero here too
			if (t == 0)
				return 0;

			int year = ((t >> 16 + 9) & 0x7f) + 1980;
			int month = (t >> 16 + 5) & 0x0f;			// 1..12 means Jan..Dec
			int day = (t >> 16) & 0x1f;					// 1..31 means 1st,...31st

			int hour = (t >> 11) & 0x1f;
			int min = (t >> 5) & 0x3f;
			int sec = (t & 0x1f) * 2;

			// Calculate days since 1970.01.01
			int days = (365 * (year - 1970) +			// Days due to whole years
					(year - 1970 + 1) / 4 +				// Days due to leap years
					dsboy[month - 1] +					// Days since beginning of this year
					day - 1);							// Days since beginning of month

			// If this is a leap year and month is March or later, add a day
			if ((year % 4 == 0) && ((year % 100 != 0) || (year % 400 == 0)) && (month >= 3))
				days++;

			// Knowing the days, we can find seconds
			int longTime = (((days * 24) + hour) * 60 + min) * 60 + sec;

			// longTime is now the time in seconds, since 1970/01/01 00:00:00
			return longTime;
		}
		#endregion
	}
}
