/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// MMD0 expansion structure
	/// </summary>
	internal class Mmd0ExpData
	{
		public uint NextHdr { get; set; }
		public uint InsTextOffs { get; set; }
		public ushort InsTextEntries { get; set; }
		public ushort InsTextEntrySize { get; set; }
		public uint AnnoTextOffs { get; set; }
		public uint AnnoTextLength { get; set; }
		public uint InstInfoOffs { get; set; }
		public ushort InstInfoEntries { get; set; }
		public ushort InstInfoEntrySize { get; set; }
		public uint Obsolete0 { get; set; }
		public uint Obsolete1 { get; set; }
		public byte[] ChannelSplit { get; } = new byte[4];
		public uint NotInfoOffs { get; set; }
		public uint SongNameOffs { get; set; }
		public uint SongNameLen { get; set; }
		public uint DumpsOffs { get; set; }
		public uint MmdInfoOffs { get; set; }
		public uint MmdARexxOffs { get; set; }
		public uint MmdCmd3xOffs { get; set; }
		public uint TrackInfoOffs { get; set; }				// Pointer to song->numTracks pointers to tag lists
		public uint EffectInfoOffs { get; set; }			// Pointer to group pointers
	}
}
