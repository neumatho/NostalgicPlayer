/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.SidTune
{
	/// <summary>
	/// Handle loading of the SID file format
	/// </summary>
	internal partial class SidTune
	{
		private const string KeywordId = "SIDPLAY INFOFILE";

		private const string KeywordName = "NAME";
		private const string KeywordAuthor = "AUTHOR";
		private const string KeywordCopyright = "COPYRIGHT";
		private const string KeywordReleased = "RELEASED";
		private const string KeywordAddress = "ADDRESS";
		private const string KeywordSongs = "SONGS";
		private const string KeywordSpeed = "SPEED";
		private const string KeyWordMusPlayer = "SIDSONG";
		private const string KeyWordMusPlayerValue = "YES";
		private const string KeywordReloc = "RELOC";
		private const string KeywordClock = "CLOCK";
		private const string KeywordSidModel = "SIDMODEL";
		private const string KeywordCompatibility = "COMPATIBILITY";

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will test the file to see if it's a SID file
		/// </summary>
		/********************************************************************/
		private bool TestSid(ModuleStream moduleStream)
		{
			// Check for a minimum file size. If it is smaller, we will not proceed
			if (moduleStream.Length < (1 + KeywordId.Length))
				return false;

			// Read the ID
			byte[] buf = new byte[KeywordId.Length];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, buf.Length);

			if (Encoding.ASCII.GetString(buf) == KeywordId)
			{
				sidType = SidType.SidInfo;
				return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will load the file as a SID file
		/// </summary>
		/********************************************************************/
		private bool LoadSid(byte[] sidBuf, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Set defaults
			fileOffset = 0;			// No header in separate data file
			info.SidChipBase1 = 0xd400;
			info.SidChipBase2 = 0;
			info.MusPlayer = false;
			info.Title = string.Empty;
			info.Author = string.Empty;
			info.Released = string.Empty;

			// Set compatibility to PlaySID, since the format is so old, that is most probably only works in this mode
			info.Compatibility = Compatibility.PSid;

			uint oldStyleSpeed = 0;

			// Flags for required entries
			bool hasAddress = false;
			bool hasName = false;
			bool hasAuthor = false;
			bool hasReleased = false;
			bool hasSongs = false;
			bool hasSpeed = false;
			bool hasInitAddr = false;

			using (MemoryStream ms = new MemoryStream(sidBuf))
			{
				using (StreamReader sr = new StreamReader(ms, EncoderCollection.Win1252))
				{
					// Skip header
					sr.ReadLine();

					// Parse all entries
					while (!sr.EndOfStream)
					{
						string nextLine = sr.ReadLine();
						if (string.IsNullOrEmpty(nextLine))
							break;

						string[] keyValuePair = nextLine.Trim().Split('=');
						if (keyValuePair.Length != 2)
						{
							errorMessage = Resources.IDS_SID_ERR_INVALID_DATA;
							return false;
						}

						keyValuePair[0] = keyValuePair[0].TrimEnd().ToUpper();
						keyValuePair[1] = keyValuePair[1].TrimStart();

						// Now check for possible keywords
						if (keyValuePair[0] == KeywordAddress)
						{
							int nextPos = 0;
							info.LoadAddr = (ushort)ReadHex(keyValuePair[1], ref nextPos);
							info.InitAddr = info.LoadAddr;
							hasInitAddr = true;

							if (nextPos < keyValuePair[1].Length)
							{
								info.InitAddr = (ushort)ReadHex(keyValuePair[1], ref nextPos);
								if (nextPos == keyValuePair[1].Length)
									break;

								info.PlayAddr = (ushort)ReadHex(keyValuePair[1], ref nextPos);
								hasAddress = true;
							}
						}
						else if (keyValuePair[0] == KeywordName)
						{
							info.Title = keyValuePair[1];
							hasName = true;
						}
						else if (keyValuePair[0] == KeywordAuthor)
						{
							info.Author = keyValuePair[1];
							hasAuthor = true;
						}
						else if (keyValuePair[0] == KeywordCopyright)
						{
							info.Released = keyValuePair[1];
							hasReleased = true;
						}
						else if (keyValuePair[0] == KeywordReleased)
						{
							info.Released = keyValuePair[1];
							hasReleased = true;
						}
						else if (keyValuePair[0] == KeywordSongs)
						{
							int nextPos = 0;
							info.Songs = (ushort)ReadDec(keyValuePair[1], ref nextPos);
							info.StartSong = (ushort)ReadDec(keyValuePair[1], ref nextPos);
							hasSongs = true;
						}
						else if (keyValuePair[0] == KeywordSpeed)
						{
							int nextPos = 0;
							oldStyleSpeed = ReadHex(keyValuePair[1], ref nextPos);
							hasSpeed = true;
						}
						else if (keyValuePair[0] == KeyWordMusPlayer)
						{
							if (keyValuePair[1].ToUpper() == KeyWordMusPlayerValue)
								info.MusPlayer = true;
						}
						else if (keyValuePair[0] == KeywordReloc)
						{
							int nextPos = 0;
							info.RelocStartPage = (byte)ReadHex(keyValuePair[1], ref nextPos);
							if (nextPos == keyValuePair[1].Length)
								break;

							info.RelocPages = (byte)ReadHex(keyValuePair[1], ref nextPos);
						}
						else if (keyValuePair[0] == KeywordClock)
						{
							string value = keyValuePair[1].ToUpper();

							if (value == "UNKNOWN")
								info.ClockSpeed = Clock.Unknown;
							else if (value == "PAL")
								info.ClockSpeed = Clock.Pal;
							else if (value == "NTSC")
								info.ClockSpeed = Clock.Ntsc;
							else if (value == "ANY")
								info.ClockSpeed = Clock.Any;
						}
						else if (keyValuePair[0] == KeywordSidModel)
						{
							string value = keyValuePair[1].ToUpper();

							if (value == "UNKNOWN")
								info.SidModel1 = SidModel.Unknown;
							else if (value == "6581")
								info.SidModel1 = SidModel._6581;
							else if (value == "8580")
								info.SidModel1 = SidModel._8580;
							else if (value == "ANY")
								info.SidModel1 = SidModel.Any;
						}
						else if (keyValuePair[0] == KeywordCompatibility)
						{
							string value = keyValuePair[1].ToUpper();

							if (value == "C64")
								info.Compatibility = Compatibility.C64;
							else if (value == "PSID")
								info.Compatibility = Compatibility.PSid;
							else if (value == "R64")
								info.Compatibility = Compatibility.R64;
							else if (value == "BASIC")
								info.Compatibility = Compatibility.Basic;
						}
					}
				}
			}

			if (!(hasName && hasAuthor && hasReleased && hasSongs))
			{
				// Something is missing (or damaged?)
				errorMessage = Resources.IDS_SID_ERR_TRUNCATED;
				return false;
			}

			switch (info.Compatibility)
			{
				case Compatibility.PSid:
				case Compatibility.C64:
				{
					if (!(hasAddress && hasSpeed))
					{
						errorMessage = Resources.IDS_SID_ERR_TRUNCATED;
						return false;
					}
					break;
				}

				case Compatibility.R64:
				{
					if (!(hasInitAddr && hasAddress))
					{
						errorMessage = Resources.IDS_SID_ERR_TRUNCATED;
						return false;
					}

					// Allow user to provide single address
					if (!hasAddress)
						info.LoadAddr = 0;
					else if ((info.LoadAddr != 0) || (info.PlayAddr != 0))
					{
						errorMessage = Resources.IDS_SID_ERR_INVALID_DATA;
						return false;
					}

					goto case Compatibility.Basic;
				}

				case Compatibility.Basic:
				{
					oldStyleSpeed = uint.MaxValue;
					break;
				}
			}

			// Create the speed/clock setting table
			ConvertOldStyleSpeedToTables(oldStyleSpeed, info.ClockSpeed);

			// We finally accept the input data
			info.FormatString = Resources.IDS_SID_FORMAT_SID;

			if (info.MusPlayer)
			{
				errorMessage = Resources.IDS_SID_ERR_PSID_MUS;
				return false;
			}

			return true;
		}
		#endregion
	}
}
