/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Holds information for a single sample
	/// </summary>
	internal class Sample
	{
		public byte[] SampleName { get; set; }
		public byte[] FileName { get; set; }

		public uint FreqFineTune { get; set; }			// Frequency in S3M and IT, finetune (0...255) in MOD, MTM, XM
		public sbyte Transpose { get; set; }
		public byte DefaultVolume { get; set; }			// 0...64
		public ushort Panning { get; set; }				// 0...256 if enabled, 0xffff otherwise
		public uint Length { get; set; }
		public uint LoopStart { get; set; }
		public uint LoopEnd { get; set; }
		public SampleInfoFlag Flags { get; set; }
		public byte VibType { get; set; }
		public byte VibSweep { get; set; }
		public byte VibDepth { get; set; }
		public byte VibRate { get; set; }
		public byte GlobalVol { get; set; }				// 0...64 in IT, in XM it represents the instrument number
		public uint SustainStart { get; set; }
		public uint SustainEnd { get; set; }
		public int CompressedSize { get; set; }
		public ushort EncoderDelay { get; set; }		// MP3: Ignore first n bytes of decoded output. Ogg: Shared Ogg header size
		public short SharedOggHeader { get; set; }
	}
}
