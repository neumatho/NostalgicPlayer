/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Helper class to identify the different formats
	/// </summary>
	internal static class TfmxIdentifier
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions = [ "tfx", "mdat", "tfm" ];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		public static ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 512)
				return ModuleType.Unknown;

			int startOffset = 0;

			// First check for one-file format
			TfhdHeader tfhdHeader = IsTfhdFile(moduleStream);
			if (tfhdHeader != null)
			{
				// Check to see if the module is forced or not checked
				if (((tfhdHeader.Type & 128) != 0) || ((tfhdHeader.Type & 127) == 0))
				{
					// Well, we can't count on the type now, so we skip
					// the header and make our own check
					startOffset = (int)tfhdHeader.HeaderSize;
				}
				else
				{
					switch (tfhdHeader.Type & 127)
					{
						case 1:
							return ModuleType.Tfmx15;

						case 2:
							return ModuleType.TfmxPro;

						case 3:
							return ModuleType.Tfmx7V;
					}

					return ModuleType.Unknown;
				}
			}
			else
			{
				TfmxModHeader modHeader = IsTfmxModFile(moduleStream, false);
				if (modHeader != null)
					startOffset = 20;
			}

			// Check for two-file format. Read the mark
			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			uint mark1 = moduleStream.Read_B_UINT32();
			uint mark2 = moduleStream.Read_B_UINT32();
			byte mark3 = moduleStream.Read_UINT8();

			// And check it
			//
			// If the file starts with TFMX and does not have SONG, it's the old format
			if ((mark1 == 0x54464d58) && ((mark2 & 0xff000000) == 0x20000000) && ((mark2 & 0x00ffffff) != 0x00534f4e) && (mark3 != 0x47))
				return ModuleType.Tfmx15;

			// TFMX-SONG / TFMX_SONG / tfmxsong
			if (((mark1 == 0x54464d58) && ((mark2 == 0x2d534f4e) || (mark2 == 0x5f534f4e)) && (mark3 == 0x47)) || ((mark1 == 0x74666d78) && (mark2 == 0x736f6e67)))
			{
				// Okay, it is either a professional or 7 voices, so check for the difference
				ushort[] songStarts = new ushort[31];

				// Get the start positions
				moduleStream.Seek(startOffset + 0x100, SeekOrigin.Begin);
				moduleStream.ReadArray_B_UINT16s(songStarts, 0, 31);

				// Get the track step offset
				moduleStream.Seek(startOffset + 0x1d0, SeekOrigin.Begin);
				uint offset = moduleStream.Read_B_UINT32();
				if (offset == 0)
					offset = 0x800;

				// Take all the sub-songs
				short times = 0;
				bool gotTimeShare = false;

				for (int i = 0; i < 31; i++)
				{
					bool getNext = true;

					// Get the current sub-song start position
					ushort position = songStarts[i];
					if (position == 0x1ff)
						break;

					// Read the track step information
					while (getNext)
					{
						// Find the position in the file where the current track step
						// information to read is stored
						moduleStream.Seek(startOffset + offset + position * 16, SeekOrigin.Begin);

						// If the track step information isn't a command, stop
						// the checking
						if (moduleStream.Read_B_UINT16() != 0xeffe)
							getNext = false;
						else
						{
							// Get the command
							switch (moduleStream.Read_B_UINT16())
							{
								// Loop a section
								case 1:
								{
									if (times == 0)
									{
										times = -1;
										position++;
									}
									else
									{
										if (times < 0)
										{
											position = moduleStream.Read_B_UINT16();
											times = (short)(moduleStream.Read_B_UINT16() - 1);
										}
										else
										{
											times--;
											position = moduleStream.Read_B_UINT16();
										}
									}
									break;
								}

								// Set tempo + start master volume slide
								case 2:
								case 4:
								{
									position++;
									break;
								}

								// Time share
								case 3:
								{
									gotTimeShare = true;
									position++;
									break;
								}

								// Unknown command
								default:
								{
									getNext = false;
									gotTimeShare = false;
									break;
								}
							}
						}
					}

					if (gotTimeShare)
						break;
				}

				if (IsStModule(moduleStream, startOffset))
					return ModuleType.Unknown;

				return gotTimeShare ? ModuleType.Tfmx7V : ModuleType.TfmxPro;
			}

			return ModuleType.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Will check the current file to see if it's in TFHD format.
		/// If that is true, it will load the structure
		/// </summary>
		/********************************************************************/
		public static TfhdHeader IsTfhdFile(ModuleStream moduleStream)
		{
			// Seek to the start of the file
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check the mark
			uint mark = moduleStream.Read_B_UINT32();

			if (mark == 0x54464844)		// TFHD
			{
				// Ok, it seems it's a TFHD file, so read the whole structure
				TfhdHeader header = new TfhdHeader();

				header.HeaderSize = moduleStream.Read_B_UINT32();
				header.Type = moduleStream.Read_UINT8();
				header.Version = moduleStream.Read_UINT8();
				header.MdatSize = moduleStream.Read_B_UINT32();
				header.SmplSize = moduleStream.Read_B_UINT32();

				return header;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Will check the current file to see if it's a ST module
		/// </summary>
		/********************************************************************/
		public static bool IsStModule(ModuleStream moduleStream, int startOffset)
		{
			moduleStream.Seek(startOffset + 0x1d4, SeekOrigin.Begin);

			int startIndex;
			int endIndex = moduleStream.Read_B_INT32();

			if (endIndex == 0)
			{
				moduleStream.Seek(startOffset + 0x600, SeekOrigin.Begin);
				startIndex = moduleStream.Read_B_INT32();

				moduleStream.Seek(startOffset + 0x7fc, SeekOrigin.Begin);
				endIndex = moduleStream.Read_B_INT32();
			}
			else
			{
				startIndex = moduleStream.Read_B_INT32();

				moduleStream.Seek(startOffset + startIndex, SeekOrigin.Begin);
				startIndex = moduleStream.Read_B_INT32();
			}

			byte[] buffer = new byte[endIndex - startIndex];

			moduleStream.Seek(startOffset + startIndex, SeekOrigin.Begin);
			moduleStream.ReadInto(buffer, 0, buffer.Length);

			for (int i = 0; i < buffer.Length; i += 4)
			{
				if ((buffer[i] > 63) && (buffer[i] < 128))
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will check the current file to see if it's in TFMX-MOD format.
		/// If that is true, it will load the structure
		/// </summary>
		/********************************************************************/
		public static TfmxModHeader IsTfmxModFile(ModuleStream moduleStream, bool loadInfo)
		{
			// Seek to the start of the file
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check the mark
			uint mark1 = moduleStream.Read_B_UINT32();
			uint mark2 = moduleStream.Read_B_UINT32();

			if ((mark1 == 0x54464d58) && (mark2 == 0x2d4d4f44))		// TFMX-MOD
			{
				TfmxModHeader header = new TfmxModHeader();

				header.OffsetToSample = moduleStream.Read_L_UINT32();
				header.OffsetToInfo = moduleStream.Read_L_UINT32();
				header.Reserved = moduleStream.Read_L_UINT32();

				if (loadInfo)
				{
					Encoding encoder = EncoderCollection.Win1252;

					moduleStream.Seek(header.OffsetToInfo, SeekOrigin.Begin);

					while (moduleStream.Position < moduleStream.Length)
					{
						byte type = moduleStream.Read_UINT8();
						if (type == 0)
						{
							moduleStream.Seek(4, SeekOrigin.Current);
							header.StartSong = moduleStream.Read_UINT8();
							break;
						}

						ushort length = moduleStream.Read_L_UINT16();

						switch (type)
						{
							case 1:
							{
								header.Author = moduleStream.ReadString(encoder, length);
								break;
							}

							case 2:
							{
								header.Game = moduleStream.ReadString(encoder, length);
								break;
							}

							case 5:
							{
								header.Flag = moduleStream.Read_UINT8();
								break;
							}

							case 6:
							{
								header.Title = moduleStream.ReadString(encoder, length);
								break;
							}

							default:
							{
								moduleStream.Seek(length, SeekOrigin.Current);
								break;
							}
						}
					}
				}

				return header;
			}

			return null;
		}
	}
}
