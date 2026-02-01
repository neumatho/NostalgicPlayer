/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// Different data types that can be returned via the AVIO
	/// write_data_type callback
	/// </summary>
	public enum AvIoDataMarkerType
	{
		/// <summary>
		/// Header data; this needs to be present for the stream to be decodeable
		/// </summary>
		Header,

		/// <summary>
		/// A point in the output bytestream where a decoder can start decoding
		/// (i.e. a keyframe). A demuxer/decoder given the data flagged with
		/// AVIO_DATA_MARKER_HEADER, followed by any AVIO_DATA_MARKER_SYNC_POINT,
		/// should give decodeable results
		/// </summary>
		Sync_Point,

		/// <summary>
		/// A point in the output bytestream where a demuxer can start parsing
		/// (for non self synchronizing bytestream formats). That is, any
		/// non-keyframe packet start point
		/// </summary>
		Boundary_Point,

		/// <summary>
		/// This is any, unlabelled data. It can either be a muxer not marking
		/// any positions at all, it can be an actual boundary/sync point
		/// that the muxer chooses not to mark, or a later part of a packet/fragment
		/// that is cut into multiple write callbacks due to limited IO buffer size
		/// </summary>
		Unknown,

		/// <summary>
		/// Trailer data, which doesn't contain actual content, but only for
		/// finalizing the output file
		/// </summary>
		Trailer,

		/// <summary>
		/// A point in the output bytestream where the underlying AVIOContext might
		/// flush the buffer depending on latency or buffering requirements. Typically
		/// means the end of a packet
		/// </summary>
		Flush_Point
	}
}
