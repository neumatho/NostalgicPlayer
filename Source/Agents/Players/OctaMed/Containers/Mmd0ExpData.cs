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
		public uint NextHdr;
		public uint InsTextOffs;
		public ushort InsTextEntries;
		public ushort InsTextEntrySize;
		public uint AnnoTextOffs;
		public uint AnnoTextLength;
		public uint InstInfoOffs;
		public ushort InstInfoEntries;
		public ushort InstInfoEntrySize;
		public uint Obsolete0;
		public uint Obsolete1;
		public byte[] ChannelSplit = new byte[4];
		public uint NotInfoOffs;
		public uint SongNameOffs;
		public uint SongNameLen;
		public uint DumpsOffs;
		public uint MmdInfoOffs;
		public uint MmdARexxOffs;
		public uint MmdCmd3xOffs;
		public uint TrackInfoOffs;				// Pointer to song->numTracks pointers to tag lists
		public uint EffectInfoOffs;				// Pointer to group pointers
	}
}
