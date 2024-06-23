/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Holds the whole file header
	/// </summary>
	internal class FileHeader
	{
		public byte[] SongName;
		public byte[] SongMessage;

		public byte NumChannels;				// 1...64 (limited by channel panning and volume)
		public ushort NumOrders;
		public ushort RestartPos;
		public ushort NumPatterns;
		public ushort NumTracks;
		public ushort NumInstruments;
		public ushort NumSamples;
		public byte DefaultSpeed;
		public byte DefaultTempo;
		public HeaderFlag Flags;
		public byte GlobalVol;					// 0...128 in IT, 0...64 in S3M
		public byte PanSeparation;				// 0...128 in IT
		public sbyte SampleVolume;				// Only used in IT
		public byte[] ChnVolume = new byte[64];	// 0...64
		public byte[] ChnPan = new byte[64];	// 0...256, 127 = surround
		public byte[] SfxMacros = new byte[16];
		public byte[][] FixedMacros = ArrayHelper.Initialize2Arrays<byte>(2, 128);
	}
}
