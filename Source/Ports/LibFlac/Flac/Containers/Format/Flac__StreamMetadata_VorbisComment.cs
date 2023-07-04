/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC VORBIS_COMMENT structure
	/// </summary>
	public class Flac__StreamMetadata_VorbisComment : IMetadata
	{
		/// <summary></summary>
		public Flac__StreamMetadata_VorbisComment_Entry Vendor_String = new Flac__StreamMetadata_VorbisComment_Entry();
		/// <summary></summary>
		public Flac__uint32 Num_Comments;
		/// <summary></summary>
		public Flac__StreamMetadata_VorbisComment_Entry[] Comments;
	}
}
