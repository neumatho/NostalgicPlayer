/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.OpusFile.Containers
{
	/// <summary>
	/// Information cached for a single link in a chained Ogg Opus file.
	/// We choose the first Opus stream encountered in each link to play back (and
	/// require at least one)
	/// </summary>
	internal class OggOpusLink
	{
		/// <summary>
		/// The byte offset of the first header page in this link
		/// </summary>
		public opus_int64 offset;

		/// <summary>
		/// The byte offset of the first data page from the chosen Opus
		/// stream in this link (after the headers)
		/// </summary>
		public opus_int64 data_offset;

		/// <summary>
		/// The byte offset of the last page from the chosen Opus stream in
		/// this link. This is used when seeking to ensure we find a page
		/// before the last one, so that end-trimming calculations work
		/// properly. This is only valid for seekable sources
		/// </summary>
		public opus_int64 end_offset;

		/// <summary>
		/// The total duration of all prior links.
		/// This is always zero for non-seekable sources
		/// </summary>
		public ogg_int64_t pcm_file_offset;

		/// <summary>
		/// The granule position of the last sample.
		/// This is only valid for seekable sources
		/// </summary>
		public ogg_int64_t pcm_end;

		/// <summary>
		/// The granule position before the first sample
		/// </summary>
		public ogg_int64_t pcm_start;

		/// <summary>
		/// The serial number
		/// </summary>
		public ogg_uint32_t serialno;

		/// <summary>
		/// The contents of the info header
		/// </summary>
		public OpusHead head = new OpusHead();

		/// <summary>
		/// The contents of the comment header
		/// </summary>
		public OpusTags tags = new OpusTags();
	}
}
