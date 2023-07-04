/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC STREAMINFO structure
	/// </summary>
	public class Flac__StreamMetadata_StreamInfo : IMetadata
	{
		/// <summary></summary>
		public uint32_t Min_BlockSize, Max_BlockSize;
		/// <summary></summary>
		public uint32_t Min_FrameSize, Max_FrameSize;
		/// <summary></summary>
		public uint32_t Sample_Rate;
		/// <summary></summary>
		public uint32_t Channels;
		/// <summary></summary>
		public uint32_t Bits_Per_Sample;
		/// <summary></summary>
		public Flac__uint64 Total_Samples;
		/// <summary></summary>
		public Flac__byte[] Md5Sum = new Flac__byte[16];
	}
}
