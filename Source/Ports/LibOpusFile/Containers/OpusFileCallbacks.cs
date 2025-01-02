/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpusFile.Containers
{
	/// <summary>
	/// The callbacks used to access non-FILE stream resources.
	/// The function prototypes are basically the same as for the stdio functions
	/// fread(), fseek(), ftell(), and fclose().
	/// The differences are that the FILE * arguments have been
	/// replaced with a void *, which is to be used as a pointer to
	/// whatever internal data these functions might need, that #seek and #tell
	/// take and return 64-bit offsets, and that #seek _must_ return -1 if
	/// the stream is unseekable
	/// </summary>
	public class OpusFileCallbacks : IDeepCloneable<OpusFileCallbacks>
	{
		/// <summary>
		/// Reads up to _nbytes bytes of data from _stream
		/// </summary>
		public delegate c_int Op_Read_Func(object _stream, CPointer<byte> _ptr, c_int nbytes);

		/// <summary>
		/// Sets the position indicator for _stream.
		/// The new position, measured in bytes, is obtained by adding
		/// _offset bytes to the position specified by _whence.
		/// If _whence is set to SEEK_SET, SEEK_CUR, or SEEK_END, the offset
		/// is relative to the start of the stream, the current position
		/// indicator, or end-of-file, respectively
		/// </summary>
		public delegate c_int Op_Seek_Func(object _stream, opus_int64 _offset, SeekOrigin _whence);

		/// <summary>
		/// Obtains the current value of the position indicator for \a _stream
		/// </summary>
		public delegate opus_int64 Op_Tell_Func(object _stream);

		/// <summary>
		/// Closes the underlying stream
		/// </summary>
		public delegate c_int Op_Close_Func(object _stream);

		/// <summary>
		/// Used to read data from the stream.
		/// This must not be NULL
		/// </summary>
		public Op_Read_Func Read;

		/// <summary>
		/// Used to seek in the stream.
		/// This may be NULL if seeking is not implemented
		/// </summary>
		public Op_Seek_Func Seek;

		/// <summary>
		/// Used to return the current read position in the stream.
		/// This may be NULL if seeking is not implemented
		/// </summary>
		public Op_Tell_Func Tell;

		/// <summary>
		/// Used to close the stream when the decoder is freed.
		/// This may be NULL to leave the stream open
		/// </summary>
		public Op_Close_Func Close;

		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public OpusFileCallbacks MakeDeepClone()
		{
			return new OpusFileCallbacks
			{
				Read = Read,
				Seek = Seek,
				Tell = Tell,
				Close = Close
			};
		}
	}
}
