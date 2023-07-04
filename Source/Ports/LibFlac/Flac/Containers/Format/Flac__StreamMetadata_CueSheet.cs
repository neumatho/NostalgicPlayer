/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC CUESHEET structure
	/// </summary>
	public class Flac__StreamMetadata_CueSheet : IMetadata
	{
		/// <summary>
		/// Media catalog number, in ASCII printable characters 0x20-0x7e. In
		/// general, the media catalog number may be 0 to 128 bytes long; any
		/// unused characters should be right-padded with NUL characters
		/// </summary>
		public byte[] Media_Catalog_Number = new byte[129];

		/// <summary>
		/// The number of lead-in samples
		/// </summary>
		public Flac__uint64 Lead_In;

		/// <summary>
		/// True if CUESHEET corresponds to a Compact Disc, else false
		/// </summary>
		public Flac__bool Is_Cd;

		/// <summary>
		/// The number of tracks
		/// </summary>
		public uint32_t Num_Tracks;

		/// <summary>
		/// Null if Num_Tracks == 0, else pointer to array of tracks
		/// </summary>
		public Flac__StreamMetadata_CueSheet_Track[] Tracks;
	}
}
