/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers
{
	/// <summary>
	/// The entry header
	/// </summary>
	internal class LzHeader
	{
		public byte HeaderSize;
		public byte[] Method = new byte[Constants.Method_Type_Strage];
		public int PackedSize;
		public int OriginalSize;
		public int LastModifiedStamp;
		public byte Attribute;
		public byte HeaderLevel;
		public byte[] Name = new byte[256];
		public ushort Crc;
		public bool HasCrc;
		public byte ExtendType;
		public byte MinorVersion;

		// ExtendType == Extend_Unix and convert from other type
		public int UnixLastModifiedStamp;
		public ushort UnixMode;
		public ushort UnixUId;
		public ushort UnixGid;

		// Converted properties to .NET data types
		public string DecodedName;			// Directory + file name decoded from the right character set
		public DateTime ConvertedLastModifiedStamp;
	}
}
