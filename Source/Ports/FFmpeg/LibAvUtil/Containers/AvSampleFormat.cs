/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Audio sample formats
	///
	/// - The data described by the sample format is always in native-endian order.
	///   Sample values can be expressed by native C types, hence the lack of a signed
	///   24-bit sample format even though it is a common raw audio data format.
	///
	/// - The floating-point formats are based on full volume being in the range
	///   [-1.0, 1.0]. Any values outside this range are beyond full volume level.
	///
	/// - The data layout as used in av_samples_fill_arrays() and elsewhere in FFmpeg
	///   (such as AVFrame in libavcodec) is as follows:
	///
	/// For planar sample formats, each audio channel is in a separate data plane,
	/// and linesize is the buffer size, in bytes, for a single plane. All data
	/// planes must be the same size. For packed sample formats, only the first data
	/// plane is used, and samples for each channel are interleaved. In this case,
	/// linesize is the buffer size, in bytes, for the 1 plane
	/// </summary>
	public enum AvSampleFormat
	{
		/// <summary>
		/// 
		/// </summary>
		None = -1,

		/// <summary>
		/// Unsigned 8 bits
		/// </summary>
		U8,

		/// <summary>
		/// Signed 16 bits
		/// </summary>
		S16,

		/// <summary>
		/// Signed 32 bits
		/// </summary>
		S32,

		/// <summary>
		/// Float
		/// </summary>
		Flt,

		/// <summary>
		/// Double
		/// </summary>
		Dbl,

		/// <summary>
		/// Unsigned 8 bits, planar
		/// </summary>
		U8P,

		/// <summary>
		/// Signed 16 bits, planar
		/// </summary>
		S16P,

		/// <summary>
		/// Signed 32 bits, planar
		/// </summary>
		S32P,

		/// <summary>
		/// Float, planar
		/// </summary>
		FltP,

		/// <summary>
		/// Double, planar
		/// </summary>
		DblP,

		/// <summary>
		/// Signed 64 bits
		/// </summary>
		S64,

		/// <summary>
		/// Signed 64 bits, planar
		/// </summary>
		S64P,

		/// <summary>
		/// Number of sample formats
		/// </summary>
		Nb
	}
}
