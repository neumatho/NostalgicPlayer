/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Enumeration of the message and error codes and returned by libmpg123 functions
	/// </summary>
	public enum Mpg123_Errors
	{
		/// <summary>
		/// Message: Track ended. Stop decoding
		/// </summary>
		Done = -12,
		/// <summary>
		/// Message: Output format will be different on next call. Note that some libmpg123 versions between 1.4.3 and 1.8.0 insist on you calling mpg123_getformat() after getting this message code. Newer verisons behave like advertised: You have the chance to call mpg123_getformat(), but you can also just continue decoding and get your data
		/// </summary>
		New_Format = -11,
		/// <summary>
		/// Message: For feed reader: "Feed me more!" (call mpg123_feed() or mpg123_decode() with some new input data)
		/// </summary>
		Need_More = -10,
		/// <summary>
		/// Generic Error
		/// </summary>
		Err = -1,
		/// <summary>
		/// Success
		/// </summary>
		Ok = 0,
		/// <summary>
		/// Unable to set up output format!
		/// </summary>
		Bad_OutFormat,
		/// <summary>
		/// Invalid channel number specified
		/// </summary>
		Bad_Channel,
		/// <summary>
		/// Invalid sample rate specified
		/// </summary>
		Bad_Rate,
		/// <summary>
		/// Unable to allocate memory for 16 to 8 converter table!
		/// </summary>
		Err_16To8Table,
		/// <summary>
		/// Bad parameter id!
		/// </summary>
		Bad_Param,
		/// <summary>
		/// Bad buffer given -- invalid pointer or too small size
		/// </summary>
		Bad_Buffer,
		/// <summary>
		/// Out of memory -- some malloc() failed
		/// </summary>
		Out_Of_Mem,
		/// <summary>
		/// You didn't initialize the library!
		/// </summary>
		Not_Initialized,
		/// <summary>
		/// Invalid decoder choice
		/// </summary>
		Bad_Decoder,
		/// <summary>
		/// Invalid mpg123 handle
		/// </summary>
		Bad_Handle,
		/// <summary>
		/// Unable to initialize frame buffers (out of memory?)
		/// </summary>
		No_Buffers,
		/// <summary>
		/// Invalid RVA mode
		/// </summary>
		Bad_Rva,
		/// <summary>
		/// This build doesn't support gapless decoding
		/// </summary>
		No_Gapless,
		/// <summary>
		/// Not enough buffer space
		/// </summary>
		No_Space,
		/// <summary>
		/// Incompatible numeric data types
		/// </summary>
		Bad_Types,
		/// <summary>
		/// Bad equalizer band
		/// </summary>
		Bad_Band,
		/// <summary>
		/// Null pointer given where valid storage address needed
		/// </summary>
		Err_Null,
		/// <summary>
		/// Error reading the stream
		/// </summary>
		Err_Reader,
		/// <summary>
		/// Cannot seek from end (end is not known)
		/// </summary>
		No_Seek_From_End,
		/// <summary>
		/// Invalid 'whence' for seek function
		/// </summary>
		Bad_Whence,
		/// <summary>
		/// Build does not support stream timeouts
		/// </summary>
		No_Timeout,
		/// <summary>
		/// File access error
		/// </summary>
		Bad_File,
		/// <summary>
		/// Seek not supported by stream
		/// </summary>
		No_Seek,
		/// <summary>
		/// No stream opened
		/// </summary>
		No_Reader,
		/// <summary>
		/// Bad parameter handle
		/// </summary>
		Bad_Pars,
		/// <summary>
		/// Bad parameters to mpg123_index() and mpg123_set_index()
		/// </summary>
		Bad_Index_Par,
		/// <summary>
		/// Lost track in bytestream and did not try to resync
		/// </summary>
		Out_Of_Sync,
		/// <summary>
		/// Resync failed to find valid MPEG data
		/// </summary>
		Resync_Fail,
		/// <summary>
		/// No 8bit encoding possible
		/// </summary>
		No_8Bit,
		/// <summary>
		/// Stack alignment error
		/// </summary>
		Bad_Align,
		/// <summary>
		/// NULL input buffer with non-zero size...
		/// </summary>
		Null_Buffer,
		/// <summary>
		/// Relative seek not possible (screwed up file offset)
		/// </summary>
		No_RelSeek,
		/// <summary>
		/// You gave a null pointer somewhere where you shouldn't have
		/// </summary>
		Null_Pointer,
		/// <summary>
		/// Bad key value given
		/// </summary>
		Bad_Key,
		/// <summary>
		/// No frame index in this build
		/// </summary>
		No_Index,
		/// <summary>
		/// Something with frame index went wrong
		/// </summary>
		Index_Fail,
		/// <summary>
		/// Something prevents a proper decoder setup
		/// </summary>
		Bad_Decoder_Setup,
		/// <summary>
		/// This feature has not been built into libmpg123
		/// </summary>
		Missing_Feature,
		/// <summary>
		/// A bad value has been given, somewhere
		/// </summary>
		Bad_Value,
		/// <summary>
		/// Low-level seek failed
		/// </summary>
		LSeek_Failed,
		/// <summary>
		/// Custom I/O not prepared
		/// </summary>
		Bad_Custom_Io,
		/// <summary>
		/// Offset value overflow during translation of large file API calls -- your client program cannot handle that large file
		/// </summary>
		Lfs_Overflow,
		/// <summary>
		/// Some integer overflow
		/// </summary>
		Int_Overflow,
		/// <summary>
		/// Floating-point computations work not as expected
		/// </summary>
		Bad_Float
	}
}
