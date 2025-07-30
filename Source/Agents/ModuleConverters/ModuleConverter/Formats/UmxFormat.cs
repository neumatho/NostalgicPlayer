/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats
{
	/// <summary>
	/// Can convert Epic Games UMX container format and extract the module inside
	/// </summary>
	internal class UmxFormat : ModuleConverterAgentBase
	{
		#pragma warning disable 649

		#region GenHist struct
		/// <summary>
		/// For UPkg versions >= 68
		/// </summary>
		private struct GenHist
		{
			public int ExportCount { get; set; }
			public int NameCount { get; set; }
		}
		#endregion

		#region UPkgHdr class
		private class UPkgHdr
		{
			public uint Tag { get; set; }				// UPkgHdrTag
			public int FileVersion { get; set; }
			public uint PkgFlags { get; set; }
			public int NameCount { get; set; }			// Number of names in name table (>= 0)
			public int NameOffset { get; set; }			// Offset to name table (>= 0)
			public int ExportCount { get; set; }		// Num. exports in export table (>= 0)
			public int ExportOffset { get; set; }		// Offset to export table (>= 0)
			public int ImportCount { get; set; }		// Num. imports in export table (>= 0)
			public int ImportOffset { get; set; }		// Offset to import table (>= 0)

			// Number of GUIDs in heritage table (>= 1) and table's offset:
			// only with versions < 68
			public int HeritageCount { get; set; }
			public int HeritageOffset { get; set; }

			// With versions >= 68: a GUID, a dword for generation count
			// and export_count and name_count dwords for each generation
			public uint[] Guid { get; }= new uint[4];
			public int GenerationCount { get; set; }

			public GenHist[] Gen { get; set; }
		}
		#endregion

		#region UmxInfo class
		private class UmxInfo
		{
			public UMusic Type { get; set; }
			public int Ofs { get; set; }
			public int Size { get; set; }
		}
		#endregion

		#pragma warning restore 649

		private enum UMusic
		{
			It = 0,
			S3M,
			Xm,
			Mod,
			Wav,
			Mp2
		}

		private static readonly string[] musType =
		[
			"IT", "S3M", "XM", "MOD",
			"WAV", "MP2", null
		];

		private const uint UPkgHdrTag = 0x9e2a83c1;
		private const int UPkgHdrSize = 64;

		private UmxInfo umxData;

		#region IModuleConverterAgent implementation
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

			int type = ProcessUPkg(moduleStream, out int ofs, out int size);
			if (type < 0)
				return AgentResult.Unknown;

			umxData = new UmxInfo();

			umxData.Type = (UMusic)type;
			umxData.Ofs = ofs;
			umxData.Size = size;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			errorMessage = string.Empty;

			// Try to read the module
			moduleStream.Seek(umxData.Ofs, SeekOrigin.Begin);

			// We just copy the whole module into the output stream
			Helpers.CopyData(moduleStream, converterStream, umxData.Size);

			return AgentResult.Ok;
		}
		#endregion

		#region Private methods
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
		private int GetFci(Span<byte> inBuf, ref int pos)
		{
			int size = 1;
			int a = inBuf[0] & 0x3f;

			if ((inBuf[0] & 0x40) != 0)
			{
				size++;
				a |= (inBuf[1] & 0x7f) << 6;

				if ((inBuf[1] & 0x80) != 0)
				{
					size++;
					a |= (inBuf[2] & 0x7f) << 13;

					if ((inBuf[2] & 0x80) != 0)
					{
						size++;
						a |= (inBuf[3] & 0x7f) << 20;

						if ((inBuf[3] & 0x80) != 0)
						{
							size++;
							a |= (inBuf[4] & 0x3f) << 27;
						}
					}
				}
			}

			if ((inBuf[0] & 0x80) != 0)
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
			Array.Clear(sig);

			moduleStream.Seek(ofs, SeekOrigin.Begin);
			moduleStream.ReadInto(sig, 0, 16);

			if (type == UMusic.It)
			{
				if (Encoding.Latin1.GetString(sig, 0, 4) == "IMPM")
					return (int)UMusic.It;

				return -1;
			}

			if (type == UMusic.Xm)
			{
				if (Encoding.Latin1.GetString(sig, 0, 16) != "Extended Module:")
					return -1;

				moduleStream.ReadInto(sig, 0, 16);
				if (sig[0] != ' ')
					return -1;

				moduleStream.ReadInto(sig, 0, 16);
				if (sig[5] != 0x1a)
					return -1;

				return (int)UMusic.Xm;
			}

			if (type == UMusic.Mp2)
			{
				ushort u = (ushort)(((sig[0] << 8) | sig[1]) & 0xfffe);
				if ((u == 0xfffc) || (u == 0xfff4))
					return (int)UMusic.Mp2;

				return -1;
			}

			if (type == UMusic.Wav)
			{
				if ((Encoding.Latin1.GetString(sig, 0, 4) == "RIFF") && (Encoding.Latin1.GetString(sig, 8, 4) == "WAVE"))
					return (int)UMusic.Wav;

				return -1;
			}

			moduleStream.Seek(ofs + 44, SeekOrigin.Begin);
			moduleStream.ReadInto(sig, 0, 4);

			if (type == UMusic.S3M)
			{
				if (Encoding.Latin1.GetString(sig, 0, 4) == "SCRM")
					return (int)UMusic.S3M;

				// SpaceMarines.umx and Starseek.umx from Return to NaPali
				// reports as "s3m" whereas the actual music format is "it"
				type = UMusic.It;
				goto retry;
			}

			moduleStream.Seek(ofs + 1080, SeekOrigin.Begin);
			moduleStream.ReadInto(sig, 0, 4);

			if (type == UMusic.Mod)
			{
				if ((Encoding.Latin1.GetString(sig, 0, 4) == "M.K.") || (Encoding.Latin1.GetString(sig, 0, 4) == "M!K!"))
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
			if (moduleStream.Read(buf, 0, 40) < 40)
				return -1;

			int idx = 0;

			if (hdr.FileVersion < 40)
				idx += 8;								// 00 00 00 00 00 00 00 00

			if (hdr.FileVersion < 60)
				idx += 16;								// 81 00 00 00 00 00 FF FF FF FF FF FF FF FF 00 00

			GetFci(buf.AsSpan(idx), ref idx);			// Skip junk
			int t = GetFci(buf.AsSpan(idx), ref idx);	// Type name

			if (hdr.FileVersion > 61)
				idx += 4;								// Skip export size

			objSize = GetFci(buf.AsSpan(idx), ref idx);
			ofs += idx;									// Offset for real data

			return t;									// Return type name index
		}



		/********************************************************************/
		/// <summary>
		/// Read type name
		/// </summary>
		/********************************************************************/
		private int ReadTypeName(ModuleStream moduleStream, UPkgHdr hdr, int idx, byte[] outBuf)
		{
			if (idx >= hdr.NameCount)
				return -1;

			byte[] buf = new byte[64];

			long l = 0;
			for (int i = 0; i <= idx; i++)
			{
				moduleStream.Seek(hdr.NameOffset + l, SeekOrigin.Begin);
				if (moduleStream.Read(buf, 0, 63) == 0)
					return -1;

				if (hdr.FileVersion >= 64)
				{
					sbyte s = (sbyte)buf[0];			// numchars *including* terminator
					if (s <= 0)
						return -1;

					l += s + 5;							// 1 for buf[0], 4 for int32 name_flags
				}
				else
				{
					l += EncoderCollection.Dos.GetCharCount(buf);
					l += 5;								// 1 for terminator, 4 for int32 name_flags
				}
			}

			int off = hdr.FileVersion >= 64 ? 1 : 0;
			Array.Copy(buf, off, outBuf, 0, 64 - off);

			return 0;
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
			moduleStream.ReadInto(buf, 0, 64);

			GetFci(buf.AsSpan(idx), ref idx);			// Skip class index
			GetFci(buf.AsSpan(idx), ref idx);			// Skip super index

			if (hdr.FileVersion >= 60)
				idx += 4;								// Skip int32 package index

			GetFci(buf.AsSpan(idx), ref idx);			// Skip object name
			idx += 4;									// Skip int32 object flags

			int s = GetFci(buf.AsSpan(idx), ref idx);	// Get serial size
			if (s <= 0)
				return -1;

			int pos = GetFci(buf.AsSpan(idx), ref idx);// Get serial offset
			if ((pos < 0) || (pos > (fSiz - 40)))
				return -1;

			int t = ReadExport(moduleStream, hdr, ref pos, ref s);
			if (t < 0)
				return -1;

			if ((s <= 0) || (s > (fSiz - pos)))
				return -1;

			if (ReadTypeName(moduleStream, hdr, t, buf) < 0)
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

			t = GetObjType(moduleStream, pos, (UMusic)t);
			if (t < 0)
				return -1;

			ofs = pos;
			objSize = s;

			return t;
		}



		/********************************************************************/
		/// <summary>
		/// Probe header
		/// </summary>
		/********************************************************************/
		private int ProbeHeader(ModuleStream moduleStream, UPkgHdr hdr)
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
				return -1;

			if (hdr.Tag != UPkgHdrTag)
				return -1;

			if ((hdr.NameCount < 0) || (hdr.ExportCount < 0) || (hdr.ImportCount < 0) || (hdr.NameOffset < 36) || (hdr.ExportOffset < 36) || (hdr.ImportOffset < 36))
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Process the package
		/// </summary>
		/********************************************************************/
		private int ProcessUPkg(ModuleStream moduleStream, out int ofs, out int objSize)
		{
			UPkgHdr header = new UPkgHdr();

			if (ProbeHeader(moduleStream, header) < 0)
			{
				ofs = -1;
				objSize = -1;

				return -1;
			}

			return ProbeUmx(moduleStream, header, out ofs, out objSize);
		}
		#endregion
	}
}
