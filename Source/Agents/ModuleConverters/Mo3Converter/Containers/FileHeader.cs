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
		public byte[] SongName { get; set; }
		public byte[] SongMessage { get; set; }

		public byte NumChannels { get; set; }			// 1...64 (limited by channel panning and volume)
		public ushort NumOrders { get; set; }
		public ushort RestartPos { get; set; }
		public ushort NumPatterns { get; set; }
		public ushort NumTracks { get; set; }
		public ushort NumInstruments { get; set; }
		public ushort NumSamples { get; set; }
		public byte DefaultSpeed { get; set; }
		public byte DefaultTempo { get; set; }
		public HeaderFlag Flags { get; set; }
		public byte GlobalVol { get; set; }				// 0...128 in IT, 0...64 in S3M
		public byte PanSeparation { get; set; }			// 0...128 in IT
		public sbyte SampleVolume { get; set; }			// Only used in IT
		public byte[] ChnVolume { get; } = new byte[64];// 0...64
		public byte[] ChnPan { get; } = new byte[64];	// 0...256, 127 = surround
		public byte[] SfxMacros { get; } = new byte[16];
		public byte[][] FixedMacros { get; } = ArrayHelper.Initialize2Arrays<byte>(2, 128);
	}
}
