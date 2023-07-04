/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// Vorbis comment entry structure used in VORBIS_COMMENT blocks.
	///
	/// For convenience, the APIs maintain a trailing NUL character at the end of
	/// entry which is not counted toward Length
	/// </summary>
	public class Flac__StreamMetadata_VorbisComment_Entry
	{
		/// <summary></summary>
		public Flac__uint32 Length;
		/// <summary></summary>
		public Flac__byte[] Entry;
	}
}
