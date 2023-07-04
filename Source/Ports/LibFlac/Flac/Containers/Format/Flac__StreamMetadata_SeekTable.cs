/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC SEEKTABLE structure
	///
	/// NOTE: From the format specification:
	/// - The seek points must be sorted by ascending sample number.
	/// - Each seek point's sample number must be the first sample of the
	///   target frame.
	/// - Each seek point's sample number must be unique within the table.
	/// - Existence of a SEEKTABLE block implies a correct setting of
	///   Total_Samples in the StreamInfo block.
	/// - Behaviour is undefined when more than one SEEKTABLE block is
	///   present in a stream.
	/// </summary>
	public class Flac__StreamMetadata_SeekTable : IMetadata
	{
		/// <summary></summary>
		public uint32_t Num_Points;
		/// <summary></summary>
		public Flac__StreamMetadata_SeekPoint[] Points;
	}
}
