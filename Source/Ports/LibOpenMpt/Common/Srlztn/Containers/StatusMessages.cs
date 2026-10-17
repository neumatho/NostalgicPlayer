/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum StatusMessages : uint32
	{
		None = 0,

		// Read notes and warnings
		ZeroEntryCount = 0x00_00_00_01,
		No_EntryIds_With_CustomId_Defined = 0x00_00_00_02,
		Loading_Object_With_Larger_Version = 0x00_00_00_04,

		// Read failures
		BadStream_After_MapHeaderSeek = 0x00_00_01_00,
		StartByte_Mismatch = 0x00_00_02_00,
		BadStream_At_Map_Read = 0x00_00_04_00,
		Insufficient_Stream_OffType = 0x00_00_08_00,
		ObjectClass_IdMismatch = 0x00_00_10_00,
		Too_Many_Entries_To_Read = 0x00_00_20_00,
		Insufficient_RPosType = 0x00_00_40_00,

		BadGiven_Stream = 0x01_00_00_00
	}
}
