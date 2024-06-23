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
		public byte[] SampleName;
		public byte[] FileName;

		public uint FreqFineTune;					// Frequency in S3M and IT, finetune (0...255) in MOD, MTM, XM
		public sbyte Transpose;
		public byte DefaultVolume;					// 0...64
		public ushort Panning;						// 0...256 if enabled, 0xffff otherwise
		public uint Length;
		public uint LoopStart;
		public uint LoopEnd;
		public SampleInfoFlag Flags;
		public byte VibType;
		public byte VibSweep;
		public byte VibDepth;
		public byte VibRate;
		public byte GlobalVol;						// 0...64 in IT, in XM it represents the instrument number
		public uint SustainStart;
		public uint SustainEnd;
		public int CompressedSize;
		public ushort EncoderDelay;					// MP3: Ignore first n bytes of decoded output. Ogg: Shared Ogg header size
		public short SharedOggHeader;
	}
}
