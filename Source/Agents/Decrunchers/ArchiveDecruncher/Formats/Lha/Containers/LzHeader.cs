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
		public byte HeaderSize { get; set; }
		public byte[] Method { get; } = new byte[Constants.Method_Type_Strage];
		public int PackedSize { get; set; }
		public int OriginalSize { get; set; }
		public int LastModifiedStamp { get; set; }
		public byte Attribute { get; set; }
		public byte HeaderLevel { get; set; }
		public byte[] Name { get; } = new byte[256];
		public ushort Crc { get; set; }
		public bool HasCrc { get; set; }
		public byte ExtendType { get; set; }
		public byte MinorVersion { get; set; }

		// ExtendType == Extend_Unix and convert from other type
		public int UnixLastModifiedStamp { get; set; }
		public ushort UnixMode { get; set; }
		public ushort UnixUId { get; set; }
		public ushort UnixGid { get; set; }

		// Converted properties to .NET data types
		public string DecodedName { get; set; }			// Directory + file name decoded from the right character set
		public DateTime ConvertedLastModifiedStamp { get; set; }
	}
}
