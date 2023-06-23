/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Enumeration of the parameters types that it is possible to set/get
	/// </summary>
	public enum Mpg123_Parms
	{
		/// <summary>
		/// Set verbosity value for enabling messages to stderr, >= 0 makes sense (integer)
		/// </summary>
		Verbose = 0,
		/// <summary>
		/// Set all flags, p.ex val = MPG123_GAPLESS|MPG123_MONO_MIX (integer)
		/// </summary>
		Flags,
		/// <summary>
		/// Add some flags (integer)
		/// </summary>
		Add_Flags,
		/// <summary>
		/// When value > 0, force output rate to that value (integer)
		/// </summary>
		Force_Rate,
		/// <summary>
		/// 0=native rate, 1=half rate, 2=quarter rate (integer)
		/// </summary>
		Down_Sample,
		/// <summary>
		/// One of the RVA choices above (integer)
		/// </summary>
		Rva,
		/// <summary>
		/// Play a frame N times (integer)
		/// </summary>
		DownSpeed,
		/// <summary>
		/// Play every Nth frame (integer)
		/// </summary>
		UpSpeed,
		/// <summary>
		/// Start with this frame (skip frames before that, integer)
		/// </summary>
		Start_Frame,
		/// <summary>
		/// Decode only this number of frames (integer)
		/// </summary>
		Decode_Frames,
		/// <summary>
		/// Stream contains ICY metadata with this interval (integer)
		/// Make sure to set this _before_ opening a stream
		/// </summary>
		Icy_Interval,
		/// <summary>
		/// The scale for output samples (amplitude - integer or float according to mpg123 output format, normally integer)
		/// </summary>
		Outscale,
		/// <summary>
		/// Timeout for reading from a stream (not supported on win32, integer)
		/// </summary>
		Timeout,
		/// <summary>
		/// Remove some flags (inverse of MPG123_ADD_FLAGS, integer)
		/// </summary>
		Remove_Flags,
		/// <summary>
		/// Try resync on frame parsing for that many bytes or until end of stream (&lt;0 ... integer). This can enlarge the limit for skipping junk on beginning, too (but not reduce it)
		/// </summary>
		Resync_Limit,
		/// <summary>
		/// Set the frame index size (if supported). Values &lt;0 mean that the index is allowed to grow dynamically in these steps (in positive direction, of course) -- Use this when you really want a full index with every individual frame
		/// </summary>
		Index_Size,
		/// <summary>
		/// Decode/ignore that many frames in advance for layer 3. This is needed to fill bit reservoir after seeking, for example (but also at least one frame in advance is needed to have all "normal" data for layer 3). Give a positive integer value, please
		/// </summary>
		Preframes,
		/// <summary>
		/// For feeder mode, keep that many buffers in a pool to avoid frequent malloc/free. The pool is allocated on mpg123_open_feed(). If you change this parameter afterwards, you can trigger growth and shrinkage during decoding. The default value could change any time. If you care about this, then set it. (integer)
		/// </summary>
		FeedPool,
		/// <summary>
		/// Minimal size of one internal feeder buffer, again, the default value is subject to change. (integer)
		/// </summary>
		FeedBuffer,
		/// <summary>
		/// Tell the parser a free-format frame size to
		/// avoid read-ahead to get it. A value of -1 (default) means that the parser
		/// will determine it. The parameter value is applied during decoder setup
		/// for a freshly opened stream only
		/// </summary>
		FreeFormat_Size
	}
}
