/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Encoder;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibFlac.Protected.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibFlac.Protected
{
	/// <summary>
	/// 
	/// </summary>
	internal class Flac__StreamEncoderProtected
	{
		public Flac__StreamEncoderState State;
		public Flac__bool Streamable_Subset;
		public Flac__bool Do_Md5;
		public Flac__bool Do_Mid_Side_Stereo;
		public Flac__bool Loose_Mid_Side_Stereo;
		public uint32_t Channels;
		public uint32_t Bits_Per_Sample;
		public uint32_t Sample_Rate;
		public uint32_t BlockSize;
		public uint32_t Num_Apodizations;
		public Flac__ApodizationSpecification[] Apodizations = ArrayHelper.InitializeArray<Flac__ApodizationSpecification>((int)Constants.Flac__Max_Apodization_Functions);
		public uint32_t Max_Lpc_Order;
		public uint32_t Qlp_Coeff_Precision;
		public Flac__bool Do_Qlp_Coeff_Prec_Search;
		public Flac__bool Do_Exhaustive_Model_Search;
		public uint32_t Min_Residual_Partition_Order;
		public uint32_t Max_Residual_Partition_Order;
		public Flac__uint64 Total_Samples_Estimate;
		public Flac__StreamMetadata[] Metadata;
		public uint32_t Num_Metadata_Blocks;
		public Flac__uint64 StreamInfo_Offset;
		public Flac__uint64 Seekable_Offset;
		public Flac__uint64 Audio_Offset;
	}
}
