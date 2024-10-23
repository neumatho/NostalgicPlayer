/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.OpusFile.Containers
{
	/// <summary>
	/// We use this to remember the pages we found while enumerating the links of a
	/// chained stream.
	/// We keep track of the starting and ending offsets, as well as the point we
	/// started searching from, so we know where to bisect.
	/// We also keep the serial number, so we can tell if the page belonged to the
	/// current link or not, as well as the granule position, to aid in estimating
	/// the start of the link
	/// </summary>
	internal class OpusSeekRecord
	{
		/// <summary>
		/// The earliest byte we know of such that reading forward from it
		/// causes capture to be regained at this page
		/// </summary>
		public opus_int64 search_start;

		/// <summary>
		/// The offset of this page
		/// </summary>
		public opus_int64 offset;

		/// <summary>
		/// The size of this page
		/// </summary>
		public opus_int32 size;

		/// <summary>
		/// The serial number of this page
		/// </summary>
		public ogg_uint32_t serialno;

		/// <summary>
		/// The granule position of this page
		/// </summary>
		public ogg_int64_t gp;
	}
}
