/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.SidTune
{
	/// <summary>
	/// Handle loading of the Amiga Workbench icon file format
	/// </summary>
	internal partial class SidTune
	{
		private static class Border
		{
			public const int LeftEdge = 0;								// ushort; Initial offsets from the origin
			public const int TopEdge = 2;								// ushort
			public const int FrontPen = 4;								// byte; Pens numbers for rendering
			public const int BackPen = 5;								// byte
			public const int DrawMode = 6;								// byte; Mode for rendering
			public const int Count = 7;									// byte; Number of XY pairs
			public const int XY = 8;									// short*; Vector coordinate pairs rel to LeftTop
			public const int NextBorder = 12;							// Border*; Pointer to any other Border too

			public const int SizeOf = 16;
		}

		private static class Image
		{
			public const int LeftEdge = 0;								// ushort; Starting offset relative to some origin
			public const int TopEdge = 2;								// ushort; Starting offset relative to some origin
			public const int Width = 4;									// ushort; Pixel size (though data is word-aligned)
			public const int Height = 6;								// ushort
			public const int Depth = 8;									// ushort; >= 0, for images you create
			public const int ImageData = 10;							// ushort*; Pointer to the actual word-aligned bits
			public const int PlanePick = 14;							// byte
			public const int PlaneOnOff = 15;							// byte
			public const int NextImage = 16;							// Image*

			public const int SizeOf = 20;
		}

		private static class GadgetInfo
		{
			public const int NextGadget = 0;							// Gadget*; Next gadget in the list
			public const int LeftEdge = 4;								// ushort; "Hit box" of gadget
			public const int TopEdge = 6;								// ushort
			public const int Width = 8;									// ushort; "Hit box" of gadget
			public const int Height = 10;								// ushort
			public const int Flags = 12;								// ushort; See below for list of defines
			public const int Activation = 14;							// ushort
			public const int GadgetType = 16;							// ushort; See below for defines
			public const int GadgetRender = 18;							// Image*
			public const int SelectRender = 22;							// Image*
			public const int GadgetText = 26;							// void*
			public const int MutualExclude = 30;						// uint
			public const int SpecialInfo = 34;							// void*
			public const int GadgetId = 38;								// ushort
			public const int UserData = 40;								// uint; Ptr to general purpose User data

			public const int SizeOf = 44;
		}

		private static class DiskObject
		{
			public const int Magic = 0;									// ushort; A magic num at the start of the file
			public const int Version = 2;								// ushort; A version number, so we can change it
			public const int Gadget = 4;								// GadgetInfo; A copy of in core gadget
			public const int Type = 4 + GadgetInfo.SizeOf;				// byte
			public const int PadByte = 4 + GadgetInfo.SizeOf + 1;		// byte; Pad it out to the next word boundary
			public const int DefaultTool = 4 + GadgetInfo.SizeOf + 2;	// byte*
			public const int ToolTypes = 4 + GadgetInfo.SizeOf + 6;		// byte**
			public const int CurrentX = 4 + GadgetInfo.SizeOf + 10;		// uint
			public const int CurrentY = 4 + GadgetInfo.SizeOf + 14;		// uint
			public const int DrawerData = 4 + GadgetInfo.SizeOf + 18;	// byte*
			public const int ToolWindow = 4 + GadgetInfo.SizeOf + 22;	// byte*; Only applies to tools
			public const int StackSize = 4 + GadgetInfo.SizeOf + 26;	// uint; Only applies to tools

			public const int SizeOf = 4 + GadgetInfo.SizeOf + 30;
		}

		// A magic number, not easily impersonated
		private const ushort DiskMagic = 0xe310;

		// Our current version number
		private const ushort DiskVersion = 1;

		// The Workbench object types
		private enum WorkBenchType : byte
		{
			Disk = 1,
			Drawer = 2,
			Tool = 3,
			Project = 4,
			Garbage = 5,
			Device = 6,
			Kick = 7,
			AppIcon = 8
		}

		// Gadget flags value
		[Flags]
		private enum GadgetFlags
		{
			// Combinations in these bits describe the highlight technique to be used
			GadgHighBits = 0x0003,

			// Complement the select box
			GadgHComp = 0x0000,

			// Draw a box around the image
			GadgHBox = 0x0001,

			// Blast in this alternate image
			GadgHImage = 0x0002,

			// Don't highlight
			GadgHNone = 0x0003,

			// Set if GadgetRender and SelectRender point to an Image structure,
			// clear if they point to Border structures
			GadgImage = 0x0004
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will test the file to see if it's an Amiga Workbench icon file
		/// </summary>
		/********************************************************************/
		private bool TestInfo(ModuleStream moduleStream)
		{
			// Check for a minimum file size. If it is smaller, we will not proceed
			if (moduleStream.Length < (1 + DiskObject.SizeOf))
				return false;

			// Read the DiskObject
			byte[] buf = new byte[DiskObject.SizeOf];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, buf.Length);

			// Require magic ID in the first two bytes of the file
			if (Endian.EndianBig16(buf, DiskObject.Magic) != DiskMagic)
				return false;

			// Only version 1.x supported
			if (Endian.EndianBig16(buf, DiskObject.Version) != DiskVersion)
				return false;

			// A PlaySID icon must be of type project
			if ((WorkBenchType)buf[DiskObject.Type] != WorkBenchType.Project)
				return false;

			sidType = SidType.IconInfo;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will load the file as an Amiga Workbench icon file
		/// </summary>
		/********************************************************************/
		private bool LoadInfo(byte[] infoBuf, out string errorMessage)
		{
			errorMessage = string.Empty;

			uint minSize = 1 + DiskObject.SizeOf;

			// We want to skip a possible Gadget Image item
			int icon = DiskObject.SizeOf;

			if (((GadgetFlags)Endian.EndianBig16(infoBuf, DiskObject.Gadget + GadgetInfo.Flags) & GadgetFlags.GadgImage) == 0)
			{
				// Calculate size of gadget borders (vector image)
				//
				// Border present?
				if ((infoBuf[DiskObject.Gadget + GadgetInfo.GadgetRender] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.GadgetRender + 1] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.GadgetRender + 2] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.GadgetRender + 3] != 0))
				{
					// Require another minimum safety size
					minSize += Border.SizeOf;
					if (infoBuf.Length < minSize)
					{
						errorMessage = Resources.IDS_SID_ERR_CORRUPT;
						return false;
					}

					int brd = icon;
					icon += Border.SizeOf;
					icon += infoBuf[brd + Border.Count] * (2 + 2);		// Pair of ushort
				}

				// Alternate border present?
				if ((infoBuf[DiskObject.Gadget + GadgetInfo.SelectRender] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.SelectRender + 1] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.SelectRender + 2] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.SelectRender + 3] != 0))
				{
					// Require another minimum safety size
					minSize += Border.SizeOf;
					if (infoBuf.Length < minSize)
					{
						errorMessage = Resources.IDS_SID_ERR_CORRUPT;
						return false;
					}

					int brd = icon;
					icon += Border.SizeOf;
					icon += infoBuf[brd + Border.Count] * (2 + 2);		// Pair of ushort
				}
			}
			else
			{
				// Calculate size of gadget images (bitmap image)
				//
				// Image present?
				if ((infoBuf[DiskObject.Gadget + GadgetInfo.GadgetRender] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.GadgetRender + 1] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.GadgetRender + 2] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.GadgetRender + 3] != 0))
				{
					// Require another minimum safety size
					minSize += Image.SizeOf;
					if (infoBuf.Length < minSize)
					{
						errorMessage = Resources.IDS_SID_ERR_CORRUPT;
						return false;
					}

					int img = icon;
					icon += Image.SizeOf;

					int imgSize = 0;
					for (int i = 0; i < Endian.EndianBig16(infoBuf, img + Image.Depth); i++)
					{
						if ((infoBuf[img + Image.PlanePick] & (1 << i)) != 0)
						{
							// NOTE: Intuition relies on PlanePick to know how many planes
							// of data are found in ImageData. There should be no more
							// '1'-bits in PlanePick than there are planes in ImageData
							imgSize++;
						}
					}

					imgSize *= ((Endian.EndianBig16(infoBuf, img + Image.Width) + 15) / 16) * 2;	// Bytes per line
					imgSize *= Endian.EndianBig16(infoBuf, img + Image.Height);					// Bytes per plane

					icon += imgSize;
				}
				
				// Alternate image present?
				if ((infoBuf[DiskObject.Gadget + GadgetInfo.SelectRender] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.SelectRender + 1] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.SelectRender + 2] != 0) || (infoBuf[DiskObject.Gadget + GadgetInfo.SelectRender + 3] != 0))
				{
					// Require another minimum safety size
					minSize += Image.SizeOf;
					if (infoBuf.Length < minSize)
					{
						errorMessage = Resources.IDS_SID_ERR_CORRUPT;
						return false;
					}

					int img = icon;
					icon += Image.SizeOf;

					int imgSize = 0;
					for (int i = 0; i < Endian.EndianBig16(infoBuf, img + Image.Depth); i++)
					{
						if ((infoBuf[img + Image.PlanePick] & (1 << i)) != 0)
						{
							// NOTE: Intuition relies on PlanePick to know how many planes
							// of data are found in ImageData. There should be no more
							// '1'-bits in PlanePick than there are planes in ImageData
							imgSize++;
						}
					}

					imgSize *= ((Endian.EndianBig16(infoBuf, img + Image.Width) + 15) / 16) * 2;	// Bytes per line
					imgSize *= Endian.EndianBig16(infoBuf, img + Image.Height);					// Bytes per plane

					icon += imgSize;
				}
			}

			int tool = icon;

			// Skip default tool
			tool += (int)Endian.EndianBig32(infoBuf, tool) + 4;

			// Defaults
			fileOffset = 0;			// No header in separate data file
			uint oldStyleSpeed = 0;

			// Set compatibility to PlaySID, since the format is so old, that is most probably only works in this mode
			info.Compatibility = Compatibility.PSid;

			// Flags for required entries
			bool hasAddress = false;
			bool hasName = false;
			bool hasAuthor = false;
			bool hasReleased = false;
			bool hasSongs = false;
			bool hasSpeed = false;
			bool hasInitAddr = false;

			// Calculate number of tool type strings
			int j = ((int)Endian.EndianBig32(infoBuf, tool) / 4) - 1;
			tool += 4;	// Skip size info

			Encoding encoder = EncoderCollection.Amiga;

			while (j-- > 0)
			{
				// Get length of this tool
				int toolLen = (int)Endian.EndianBig32(infoBuf, tool);
				tool += 4;	// Skip tool length

				string line = encoder.GetString(infoBuf, tool, toolLen);

				string[] keyValuePair = line.Trim().Split('=');
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
				else if (keyValuePair[0] == KeyWordMusPlayer)
				{
					if (keyValuePair[1].ToUpper() == KeyWordMusPlayerValue)
						info.MusPlayer = true;
				}
				else if (keyValuePair[0] == KeywordReleased)
				{
					info.Released = keyValuePair[1];
					hasReleased = true;
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

				// Skip to next tool
				tool += toolLen;
			}

			if (!(hasName && hasAuthor && hasReleased && hasSongs))
			{
				if (hasName || hasAuthor || hasReleased || hasSongs)
				{
					// Something is missing (or damaged?)
					errorMessage = Resources.IDS_SID_ERR_CORRUPT;
				}
				else
				{
					// No PlaySID conform info strings
					errorMessage = Resources.IDS_SID_ERR_NO_STRINGS;
				}

				return false;
			}

			switch (info.Compatibility)
			{
				case Compatibility.PSid:
				case Compatibility.C64:
				{
					if (!(hasAddress && hasSpeed))
					{
						errorMessage = Resources.IDS_SID_ERR_CORRUPT;
						return false;
					}
					break;
				}

				case Compatibility.R64:
				{
					if (!(hasInitAddr && hasAddress))
					{
						errorMessage = Resources.IDS_SID_ERR_CORRUPT;
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
			info.FormatString = Resources.IDS_SID_FORMAT_INFO;

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
