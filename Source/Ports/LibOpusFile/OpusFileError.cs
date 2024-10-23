/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.OpusFile
{
	/// <summary>
	/// List of possible error codes
	/// Many of the functions in this library return a negative error code when a
	/// function fails.
	/// This list provides a brief explanation of the common errors.
	/// See each individual function for more details on what a specific error code
	/// means in that context
	/// </summary>
	public enum OpusFileError
	{
		/// <summary>
		/// No errors
		/// </summary>
		Ok = 0,

		/// <summary>
		/// A request did not succeed
		/// </summary>
		False = -1,

		/// <summary>
		/// Currently not used externally
		/// </summary>
		EOF = -2,

		/// <summary>
		/// There was a hole in the page sequence numbers (e.g., a page was
		/// corrupt or missing)
		/// </summary>
		Hole = -3,

		/// <summary>
		/// An underlying read, seek, or tell operation failed when it should
		/// have succeeded
		/// </summary>
		Read = -128,

		/// <summary>
		/// A NULL pointer was passed where one was unexpected, or an
		/// internal memory allocation failed, or an internal library error
		/// was encountered
		/// </summary>
		Fault = -129,

		/// <summary>
		/// The stream used a feature that is not implemented, such as an
		/// unsupported channel family
		/// </summary>
		Impl = -130,

		/// <summary>
		/// One or more parameters to a function were invalid
		/// </summary>
		Inval = -131,

		/// <summary>
		/// A purported Ogg Opus stream did not begin with an Ogg page, a
		/// purported header packet did not start with one of the required
		/// strings, "OpusHead" or "OpusTags", or a link in a chained file
		/// was encountered that did not contain any logical Opus streams
		/// </summary>
		NotFormat = -132,

		/// <summary>
		/// A required header packet was not properly formatted, contained
		/// illegal values, or was missing altogether
		/// </summary>
		BadHeader = -133,

		/// <summary>
		/// The ID header contained an unrecognized version number
		/// </summary>
		Version = -134,

		/// <summary>
		/// Currently not used at all
		/// </summary>
		NotAudio = -135,

		/// <summary>
		/// An audio packet failed to decode properly.
		/// This is usually caused by a multistream Ogg packet where the
		/// durations of the individual Opus packets contained in it are not
		/// all the same
		/// </summary>
		BadPacket = -136,

		/// <summary>
		/// We failed to find data we had seen before, or the bitstream
		/// structure was sufficiently malformed that seeking to the target
		/// destination was impossible
		/// </summary>
		BadLink = -137,

		/// <summary>
		/// An operation that requires seeking was requested on an unseekable
		/// stream
		/// </summary>
		NoSeek = -138,

		/// <summary>
		/// The first or last granule position of a link failed basic
		/// validity checks
		/// </summary>
		BadTimestamp =  -139
	}
}
