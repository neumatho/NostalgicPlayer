/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Helper class to identify the different formats
	/// </summary>
	internal static class TfmxIdentifier
	{
		private static readonly uint[] tfmx15Variants =
		[
			// Danger Freak (1989)
			0x48960d8c, 0x5dcd624f, 0x3f0b151f,

			// Hard'n'Heavy (1989)
			0x27f8998c, 0x26447707, 0xd404651b, 0xb5348633,

			// R-Type (1989)
			0x8ac70fc8
		];

		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions = [ "tfx", "mdat", "tfm", "tfmx" ];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is.
		///
		/// I still use my original detection routine, since the real
		/// detection is merged into the init functionality in
		/// LibTfmxAudioDecoder
		/// </summary>
		/********************************************************************/
		public static (ModuleType moduleType, bool singleFile) TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 512)
				return (ModuleType.Unknown, false);

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
							return (ModuleType.Tfmx15, true);

						case 2:
							return (ModuleType.TfmxPro, true);

						case 3:
							return (ModuleType.Tfmx7V, true);
					}

					return (ModuleType.Unknown, false);
				}
			}
			else
			{
				if (IsTfmxModFile(moduleStream))
					startOffset = 20;
				else
					startOffset = IsTfmxPakFile(moduleStream);
			}

			if (IsStModule(moduleStream, startOffset))
				return (ModuleType.Unknown, false);

			// Check for two-file format. Read the mark
			bool singleFile = startOffset > 0;
			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			uint mark1 = moduleStream.Read_B_UINT32();
			uint mark2 = moduleStream.Read_B_UINT32();
			byte mark3 = moduleStream.Read_UINT8();

			// And check it
			//
			// If the file starts with TFMX and does not have SONG, it's the old format
			if ((mark1 == 0x54464d58) && ((mark2 & 0xff000000) == 0x20000000) && ((mark2 & 0x00ffffff) != 0x00534f4e) && (mark3 != 0x47))
				return (ModuleType.Tfmx15, singleFile);

			// Make a check for special modules, since they are a special variant of TFMX 1.5
			moduleStream.Seek(startOffset + 0x1d4, SeekOrigin.Begin);
			uint offset = moduleStream.Read_B_UINT32();
			if (offset == 0)
				offset = 0x400;

			moduleStream.Seek(startOffset + offset, SeekOrigin.Begin);
			offset = moduleStream.Read_B_UINT32();

			byte[] checkBuffer = new byte[0x100];
			moduleStream.Seek(startOffset + offset, SeekOrigin.Begin);
			moduleStream.ReadInto(checkBuffer, 0, checkBuffer.Length);

			uint crc = CrcLight.Get(checkBuffer, 0, 0x100);
			if (tfmx15Variants.Contains(crc))
				return (ModuleType.Tfmx15, singleFile);

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
				offset = moduleStream.Read_B_UINT32();
				if (offset == 0)
					offset = 0x800;

				// Only check the first sub-song
				short times = 0;
				bool gotTimeShare = false;

				bool getNext = true;

				// Get the current sub-song start position
				ushort position = songStarts[0];

				// Read the track step information
				while (getNext)
				{
					// Find the position in the file where the current track step
					// information to read is stored
					moduleStream.Seek(startOffset + offset + (position * 16), SeekOrigin.Begin);

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

				return (gotTimeShare ? ModuleType.Tfmx7V : ModuleType.TfmxPro, singleFile);
			}

			return (ModuleType.Unknown, false);
		}



		/********************************************************************/
		/// <summary>
		/// Will check the current file to see if it's in TFHD format.
		/// If that is true, it will load the structure
		/// </summary>
		/********************************************************************/
		private static TfhdHeader IsTfhdFile(ModuleStream moduleStream)
		{
			// Seek to the start of the file
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check the mark
			if (moduleStream.ReadMark() == "TFHD")
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
		/// Will check the current file to see if it's in TFMX-MOD format
		/// </summary>
		/********************************************************************/
		private static bool IsTfmxModFile(ModuleStream moduleStream)
		{
			// Seek to the start of the file
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check the mark
			return moduleStream.ReadMark(8) == "TFMX-MOD";
		}



		/********************************************************************/
		/// <summary>
		/// Will check the current file to see if it's in TFMXPAK format
		/// </summary>
		/********************************************************************/
		private static int IsTfmxPakFile(ModuleStream moduleStream)
		{
			// Seek to the start of the file
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check the mark
			if (moduleStream.ReadMark(7) == "TFMXPAK")
			{
				for (int i = 8; i < 32; i++)
				{
					if (moduleStream.Read_UINT8() == '>')
					{
						if (moduleStream.ReadMark(2) == ">>")
							return (int)moduleStream.Position;
					}
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Will check the current file to see if it's an ST module
		/// </summary>
		/********************************************************************/
		private static bool IsStModule(ModuleStream moduleStream, int startOffset)
		{
			moduleStream.Seek(startOffset + 0x1d8, SeekOrigin.Begin);
			int macroIndex = moduleStream.Read_B_INT32();

			if (macroIndex == 0)
				macroIndex = 0x600;

			// Only check the first 7 macros
			moduleStream.Seek(startOffset + macroIndex, SeekOrigin.Begin);

			uint[] offsets = new uint[8];
			moduleStream.ReadArray_B_UINT32s(offsets, 0, offsets.Length);

			bool foundHighMacroCmd = false;

			for (int i = 0; i < offsets.Length - 1; i++)
			{
				uint macroOffs = offsets[i];
				uint macroEnd = offsets[i + 1];

				if (macroEnd <= macroOffs)
					break;

				bool foundStop = false;

				byte[] buffer = new byte[macroEnd - macroOffs];

				moduleStream.Seek(startOffset + macroOffs, SeekOrigin.Begin);
				moduleStream.ReadInto(buffer, 0, buffer.Length);

				for (int j = 0; j < buffer.Length; j += 4)
				{
					if (buffer[j] == 7)
					{
						foundStop = true;
						break;
					}

					if (buffer[j] >= 64)
						foundHighMacroCmd = true;
				}

				if (foundStop && foundHighMacroCmd)
					return true;
			}

			return false;
		}
	}
}
