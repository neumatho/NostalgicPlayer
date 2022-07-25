/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Formats
{
	/// <summary>
	/// MikMod loader for UMX (Unreal Music File) format
	///
	/// Epic Games Unreal UMX container loading for libmikmod
	/// Written by O. Sezer [sezero@users.sourceforge.net]
	///
	/// Records data type/offset info in its Test() function, then acts
	/// as a middle-man, forwarding calls to the real loader units. It
	/// requires that the MREADER implementation in use always respects
	/// its iobase fields. Like all other libmikmod loaders, this code
	/// is not reentrant yet.
	///
	/// UPKG parsing partially based on Unreal Media Ripper (UMR) v0.3
	/// by Andy Ward [wardwh@swbell.net], with additional updates
	/// by O. Sezer - see git repo at https://github.com/sezero/umr.git
	/// </summary>
	internal class UmxFormat : MikModConverterWorkerBase
	{
		#region GenHist struct
		/// <summary>
		/// For UPkg versions >= 68
		/// </summary>
		#pragma warning disable 649
		private struct GenHist
		{
			public int ExportCount;
			public int NameCount;
		}
		#pragma warning restore 649
		#endregion

		#region UPkgHdr class
		#pragma warning disable 649
		private class UPkgHdr
		{
			public uint Tag;											// UPkgHdrTag
			public int FileVersion;
			public uint PkgFlags;
			public int NameCount;										// Number of names in name table (>= 0)
			public int NameOffset;										// Offset to name table (>= 0)
			public int ExportCount;										// Num. exports in export table (>= 0)
			public int ExportOffset;									// Offset to export table (>= 0)
			public int ImportCount;										// Num. imports in export table (>= 0)
			public int ImportOffset;									// Offset to import table (>= 0)

			// Number of GUIDs in heritage table (>= 1) and table's offset:
			// only with versions < 68
			public int HeritageCount;
			public int HeritageOffset;

			// With versions >= 68: a GUID, a dword for generation count
			// and export_count and name_count dwords for each generation:
			public readonly uint[] Guid = new uint[4];
			public int GenerationCount;

			public GenHist[] Gen;
		}
		#pragma warning restore 649
		#endregion

		#region UmxInfo class
		private class UmxInfo
		{
			public UMusic Type;
			public int Ofs;
			public int Size;
			public int FileVersion;
		}
		#endregion

		private const uint UPkgHdrTag = 0x9e2a83c1;
		private const int UPkgHdrSize = 64;

		private enum UMusic
		{
			It = 0,
			S3M,
			Xm,
			Mod
		}

		private static readonly string[] musType =
		{
			"IT", "S3M", "XM", "MOD",
			null
		};

		private static readonly Dictionary<int, string> versions = new Dictionary<int, string>()
		{
			{ 35, Resources.IDS_MIKCONV_NAME_UMX_BETA }, { 37, Resources.IDS_MIKCONV_NAME_UMX_BETA },
			{ 40, Resources.IDS_MIKCONV_NAME_UMX_1998 }, { 41, Resources.IDS_MIKCONV_NAME_UMX_1998 },
			{ 61, Resources.IDS_MIKCONV_NAME_UMX_UNREAL },
			{ 62, Resources.IDS_MIKCONV_NAME_UMX_TOURNAMENT }, { 64, Resources.IDS_MIKCONV_NAME_UMX_TOURNAMENT }, { 66, Resources.IDS_MIKCONV_NAME_UMX_TOURNAMENT }, { 68, Resources.IDS_MIKCONV_NAME_UMX_TOURNAMENT },
			{ 63, Resources.IDS_MIKCONV_NAME_UMX_NAPALI },
			{ 69, Resources.IDS_MIKCONV_NAME_UMX_TACTICAL },
			{ 83, Resources.IDS_MIKCONV_NAME_UMX_MOBILE }
		};

		private UmxInfo umxData;

		#region MikModConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First check the length
			long fileSize = moduleStream.Length;
			if (fileSize < UPkgHdrSize)						// Size of header
				return AgentResult.Unknown;

			// Parse the structure and find the type of the module
			moduleStream.Seek(0, SeekOrigin.Begin);
			int type = ProcessUPkg(moduleStream, out int ofs, out int size, out int fileVersion);
			if ((type < 0) || (type > (int)UMusic.Mod))
				return AgentResult.Unknown;

			umxData = new UmxInfo();

			umxData.Type = (UMusic)type;
			umxData.Ofs = ofs;
			umxData.Size = size;
			umxData.FileVersion = fileVersion;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Tells if the converter loads data into the Module (of) or not
		/// </summary>
		/********************************************************************/
		protected override bool ConvertModuleData => false;



		/********************************************************************/
		/// <summary>
		/// Extract a module from a container and store it in the writer
		/// stream
		/// </summary>
		/********************************************************************/
		protected override bool ExtractModule(ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Try to read the module
			moduleStream.Seek(umxData.Ofs, SeekOrigin.Begin);

			// In MikMod, it will call the right loader depending on the type.
			// We won't do that. We will just copy all the data
			Helpers.CopyData(moduleStream, converterStream, umxData.Size);

			// Set the module type
			if (!versions.TryGetValue(umxData.FileVersion, out string format))
				format = Resources.IDS_MIKCONV_NAME_UMX_UNKNOWN;

			originalFormat = string.Format(Resources.IDS_MIKCONV_NAME_UMX, format, musType[(int)umxData.Type]);

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Process the package
		/// </summary>
		/********************************************************************/
		private int ProcessUPkg(ModuleStream moduleStream, out int ofs, out int objSize, out int version)
		{
			UPkgHdr header = new UPkgHdr();

			if (!ProbeHeader(moduleStream, header))
			{
				ofs = -1;
				objSize = -1;
				version = -1;

				return -1;
			}

			version = header.FileVersion;

			return ProbeUmx(moduleStream, header, out ofs, out objSize);
		}



		/********************************************************************/
		/// <summary>
		/// Probe header
		/// </summary>
		/********************************************************************/
		private bool ProbeHeader(ModuleStream moduleStream, UPkgHdr hdr)
		{
			hdr.Tag = moduleStream.Read_L_UINT32();
			hdr.FileVersion = (int)moduleStream.Read_L_UINT32();
			hdr.PkgFlags = moduleStream.Read_L_UINT32();
			hdr.NameCount = (int)moduleStream.Read_L_UINT32();
			hdr.NameOffset = (int)moduleStream.Read_L_UINT32();
			hdr.ExportCount = (int)moduleStream.Read_L_UINT32();
			hdr.ExportOffset = (int)moduleStream.Read_L_UINT32();
			hdr.ImportCount = (int)moduleStream.Read_L_UINT32();
			hdr.ImportOffset = (int)moduleStream.Read_L_UINT32();

			if (moduleStream.EndOfStream)
				return false;

			if (hdr.Tag != UPkgHdrTag)
				return false;

			if ((hdr.NameCount < 0) || (hdr.ExportCount < 0) || (hdr.ImportCount < 0) || (hdr.NameOffset < 36) || (hdr.ExportOffset < 36) || (hdr.ImportOffset < 36))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Probe container
		/// </summary>
		/********************************************************************/
		private int ProbeUmx(ModuleStream moduleStream, UPkgHdr hdr, out int ofs, out int objSize)
		{
			int idx = 0;
			long fSiz = moduleStream.Length;

			ofs = -1;
			objSize = -1;

			if ((hdr.NameOffset >= fSiz) || (hdr.ExportOffset >= fSiz) || (hdr.ImportOffset >= fSiz))
				return -1;

			// Find the offset and size of the first IT, S3M or XM
			// by parsing the exports table. The umx files should
			// have only one export. Kran32.umx from Unreal has two,
			// but both pointing to the same music
			if (hdr.ExportOffset >= fSiz)
				return -1;

			byte[] buf = new byte[64];

			moduleStream.Seek(hdr.ExportOffset, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 64);

			GetFci(buf, idx, ref idx);			// Skip class index
			GetFci(buf, idx, ref idx);			// Skip super index

			if (hdr.FileVersion >= 60)
				idx += 4;								// Skip int32 package index

			GetFci(buf, idx, ref idx);			// Skip object name
			idx += 4;									// Skip int32 object flags

			int s = GetFci(buf, idx, ref idx);	// Get serial size
			if (s <= 0)
				return -1;

			int pos = GetFci(buf, idx, ref idx);// Get serial offset
			if ((pos < 0) || (pos > fSiz - 40))
				return -1;

			int t;
			if ((t = ReadExport(moduleStream, hdr, ref pos, ref s)) < 0)
				return -1;

			if ((s <= 0) || (s > fSiz - pos))
				return -1;

			if (!ReadTypeName(moduleStream, hdr, t, buf, fSiz))
				return -1;

			int i;
			for (i = 0; musType[i] != null; i++)
			{
				if (EncoderCollection.Dos.GetString(buf).ToUpper() == musType[i])
				{
					t = i;
					break;
				}
			}

			if (musType[i] == null)
				return -1;

			if ((t = GetObjType(moduleStream, pos, (UMusic)t)) < 0)
				return -1;

			ofs = pos;
			objSize = s;

			return t;
		}



		/********************************************************************/
		/// <summary>
		/// Decode an FCompactIndex.
		/// 
		/// Original documentation by Tim Sweeney was at
		/// http://unreal.epicgames.com/Packages.htm
		/// 
		/// Also see Unreal Wiki:
		/// http://wiki.beyondunreal.com/Legacy:Package_File_Format/Data_Details
		/// </summary>
		/********************************************************************/
		private int GetFci(byte[] inBuf, int offset, ref int pos)
		{
			int size = 1;
			int a = inBuf[offset] & 0x3f;

			if ((inBuf[offset] & 0x40) != 0)
			{
				size++;
				a |= (inBuf[offset + 1] & 0x7f) << 6;

				if ((inBuf[offset + 1] & 0x80) != 0)
				{
					size++;
					a |= (inBuf[offset + 2] & 0x7f) << 13;

					if ((inBuf[offset + 2] & 0x80) != 0)
					{
						size++;
						a |= (inBuf[offset + 3] & 0x7f) << 20;

						if ((inBuf[offset + 3] & 0x80) != 0)
						{
							size++;
							a |= (inBuf[offset + 4] & 0x3f) << 27;
						}
					}
				}
			}

			if ((inBuf[offset] & 0x80) != 0)
				a = -a;

			pos += size;

			return a;
		}



		/********************************************************************/
		/// <summary>
		/// Return the object type
		/// </summary>
		/********************************************************************/
		private int GetObjType(ModuleStream moduleStream, int ofs, UMusic type)
		{
			byte[] sig = new byte[16];

			retry:
			moduleStream.Seek(ofs, SeekOrigin.Begin);
			moduleStream.Read(sig, 0, 16);

			if (type == UMusic.It)
			{
				if (Encoding.ASCII.GetString(sig, 0, 4) == "IMPM")
					return (int)UMusic.It;

				return -1;
			}

			if (type == UMusic.Xm)
			{
				if (Encoding.ASCII.GetString(sig, 0, 16) != "Extended Module:")
					return -1;

				moduleStream.Read(sig, 0, 16);
				if (sig[0] != ' ')
					return -1;

				moduleStream.Read(sig, 0, 16);
				if (sig[5] != 0x1a)
					return -1;

				return (int)UMusic.Xm;
			}

			moduleStream.Seek(ofs + 44, SeekOrigin.Begin);
			moduleStream.Read(sig, 0, 4);

			if (type == UMusic.S3M)
			{
				if (Encoding.ASCII.GetString(sig, 0, 4) == "SCRM")
					return (int)UMusic.S3M;

				// SpaceMarines.umx and Starseek.umx from Return to NaPali
				// reports as "s3m" whereas the actual music format is "it"
				type = UMusic.It;
				goto retry;
			}

			moduleStream.Seek(ofs + 1080, SeekOrigin.Begin);
			moduleStream.Read(sig, 0, 4);

			if (type == UMusic.Mod)
			{
				if ((sig[0] == 'M') && (sig[2] == 'K') && (((sig[1] == '.') && (sig[3] == '.')) || ((sig[1] == '!') && (sig[3] == '!'))))
					return (int)UMusic.Mod;

				return -1;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Read export table
		/// </summary>
		/********************************************************************/
		private int ReadExport(ModuleStream moduleStream, UPkgHdr hdr, ref int ofs, ref int objSize)
		{
			byte[] buf = new byte[40];

			moduleStream.Seek(ofs, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 40);

			if (moduleStream.EndOfStream)
				return -1;

			int idx = 0;

			if (hdr.FileVersion < 40)
				idx += 8;								// 00 00 00 00 00 00 00 00

			if (hdr.FileVersion < 60)
				idx += 16;								// 81 00 00 00 00 00 FF FF FF FF FF FF FF FF 00 00

			GetFci(buf, idx, ref idx);			// Skip junk
			int t = GetFci(buf, idx, ref idx);	// Type name

			if (hdr.FileVersion > 61)
				idx += 4;								// Skip export size

			objSize = GetFci(buf, idx, ref idx);
			ofs += idx;									// Offset for real data

			return t;									// Return type name index
		}



		/********************************************************************/
		/// <summary>
		/// Read type name
		/// </summary>
		/********************************************************************/
		private bool ReadTypeName(ModuleStream moduleStream, UPkgHdr hdr, int idx, byte[] outBuf, long end)
		{
			if (idx >= hdr.NameCount)
				return false;

			byte[] buf = new byte[64];

			long l = 0;
			for (int i = 0; i <= idx; i++)
			{
				if (hdr.NameOffset + l >= end)
					return false;

				moduleStream.Seek(hdr.NameOffset + l, SeekOrigin.Begin);
				moduleStream.Read(buf, 0, 63);

				if (hdr.FileVersion >= 64)
				{
					sbyte s = (sbyte)buf[0];			// numchars *including* terminator
					if (s <= 0)
						return false;

					l += s + 5;							// 1 for buf[0], 4 for int32_t name_flags
				}
				else
				{
					l += EncoderCollection.Dos.GetString(buf).Length;
					l += 5;								// 1 for terminator, 4 for int32_t name_flags
				}
			}

			int off = hdr.FileVersion >= 64 ? 1 : 0;
			Array.Copy(buf, off, outBuf, 0, 64 - off);

			return true;
		}
		#endregion
	}
}
