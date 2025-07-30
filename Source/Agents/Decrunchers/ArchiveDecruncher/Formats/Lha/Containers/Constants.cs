/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	internal static class Constants
	{
		// ReSharper disable InconsistentNaming
		public const int FileName_Length = 1024;

		public const string LzHuff0_Method = "-lh0-";
		public const string LzHuff1_Method = "-lh1-";
		public const string LzHuff2_Method = "-lh2-";
		public const string LzHuff3_Method = "-lh3-";
		public const string LzHuff4_Method = "-lh4-";
		public const string LzHuff5_Method = "-lh5-";
		public const string LzHuff6_Method = "-lh6-";
		public const string LzHuff7_Method = "-lh7-";
		public const string Larc_Method = "-lzs-";
		public const string Larc5_Method = "-lz5-";
		public const string Larc4_Method = "-lz4-";
		public const string LzhDirs_Method = "-lhd-";

		public const int Method_Type_Strage = 5;

		public const int LzHuff0_Method_Num = 0;
		public const int LzHuff1_Method_Num = 1;
		public const int LzHuff2_Method_Num = 2;
		public const int LzHuff3_Method_Num = 3;
		public const int LzHuff4_Method_Num = 4;
		public const int LzHuff5_Method_Num = 5;
		public const int LzHuff6_Method_Num = 6;
		public const int LzHuff7_Method_Num = 7;
		public const int Larc_Method_Num = 8;
		public const int Larc5_Method_Num = 9;
		public const int Larc4_Method_Num = 10;
		public const int LzhDirs_Method_Num = 11;

		public const int I_Header_Size = 0;
		public const int I_Header_Checksum = 1;
		public const int I_Method = 2;
		public const int I_Packed_Size = 7;
		public const int I_Original_Size = 11;
		public const int I_Last_Modified_Stamp = 15;
		public const int I_Attribute = 19;
		public const int I_Header_Level = 20;
		public const int I_Name_Length = 21;
		public const int I_Name = 22;

		public const int I_Crc = 22;						// + name_length;
		public const int I_Extend_Type = 24;				// + name_length;
		public const int I_Minor_Version = 25;				// + name_length;
		public const int I_Unix_Last_Modified_Stamp = 26;	// + name_length;
		public const int I_Unix_Mode = 30;					// + name_length;
		public const int I_Unix_UId = 32;					// + name_length;
		public const int I_Unix_GId = 34;					// + name_length;
		public const int I_Unix_Extend_Bottom = 36;			// + name_length;

		public const int I_Generic_Header_Botton = I_Extend_Type;

		public const byte Extend_Generic = 0;
		public const byte Extend_Unix = (byte)'U';
		public const byte Extend_MsDos = (byte)'M';
		public const byte Extend_MacOs = (byte)'m';
		public const byte Extend_Os9 = (byte)'9';
		public const byte Extend_Os2 = (byte)'2';
		public const byte Extend_Os68K = (byte)'K';
		public const byte Extend_Os386 = (byte)'3';			// OS-9000???
		public const byte Extend_Human = (byte)'H';
		public const byte Extend_Cpm = (byte)'C';
		public const byte Extend_Flex = (byte)'F';
		public const byte Extend_Runser = (byte)'R';

		// This OS type is not official
		public const byte Extend_TownsOs = (byte)'T';
		public const byte Extend_XosK = (byte)'X';

		public const byte Delim = (byte)'/';
		public const byte Delim2 = 0xff;

		public static readonly ushort Unix_File_TypeMask    = Convert.ToUInt16("0170000", 8);
		public static readonly ushort Unix_File_Regular     = Convert.ToUInt16("0100000", 8);
		public static readonly ushort Unix_File_Directory   = Convert.ToUInt16("0040000", 8);
		public static readonly ushort Unix_File_SymLink     = Convert.ToUInt16("0120000", 8);
		public static readonly ushort Unix_SetUId           = Convert.ToUInt16("0004000", 8);
		public static readonly ushort Unix_SetGId           = Convert.ToUInt16("0002000", 8);
		public static readonly ushort Unix_StyckyBit        = Convert.ToUInt16("0001000", 8);
		public static readonly ushort Unix_Owner_Read_Perm  = Convert.ToUInt16("0000400", 8);
		public static readonly ushort Unix_Owner_Write_Perm = Convert.ToUInt16("0000200", 8);
		public static readonly ushort Unix_Owner_Exec_Perm  = Convert.ToUInt16("0000100", 8);
		public static readonly ushort Unix_Group_Read_Perm  = Convert.ToUInt16("0000040", 8);
		public static readonly ushort Unix_Group_Write_Perm = Convert.ToUInt16("0000020", 8);
		public static readonly ushort Unix_Group_Exec_Perm  = Convert.ToUInt16("0000010", 8);
		public static readonly ushort Unix_Other_Read_Perm  = Convert.ToUInt16("0000004", 8);
		public static readonly ushort Unix_Other_Write_Perm = Convert.ToUInt16("0000002", 8);
		public static readonly ushort Unix_Other_Exec_Perm  = Convert.ToUInt16("0000001", 8);
		public static readonly ushort Unix_RW_RW_RW         = Convert.ToUInt16("0000666", 8);

		public const int LzHeader_Strage = 4096;

		public const int CharBit = 8;

		public const int NChar = 256 + 60 - Threshold + 1;
		public const int TreeSizeC = NChar * 2;
		public const int TreeSizeP = 128 * 2;
		public const int TreeSize = TreeSizeC + TreeSizeP;
		public const int RootC = 0;
		public const int RootP = TreeSizeC;

		public const int Np = MaxDicBit + 1;
		public const short Nt = UShrtBit + 3;
		public const short TBit = 5;
		public const short Nc = byte.MaxValue + MaxMatch + 2 - Threshold;
		public const int Npt = 0x80;

		public const int Magic0 = 18;
		public const int Magic5 = 19;

		public const int N1 = 286;
		public const int ExtraBits = 8;
		public const int BufBits = 16;
		public const byte LenField = 4;

		public const short MaxDicBit = 16;
		public const int MaxMatch = 256;
		public const int Threshold = 3;

		public const byte CBit = 9;
		public const short UShrtBit = 16;
		// ReSharper restore InconsistentNaming
	}
}
