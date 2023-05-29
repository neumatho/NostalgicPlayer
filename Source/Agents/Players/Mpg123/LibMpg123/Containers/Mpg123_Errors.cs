/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Enumeration of the message and error codes and returned by libmpg123 functions
	/// </summary>
	internal enum Mpg123_Errors
	{
		Done = -12,				// < Message: Track ended. Stop decoding
		New_Format = -11,		// < Message: Output format will be different on next call. Note that some libmpg123 versions between 1.4.3 and 1.8.0 insist on you calling mpg123_getformat() after getting this message code. Newer verisons behave like advertised: You have the chance to call mpg123_getformat(), but you can also just continue decoding and get your data
		Need_More = -10,		// < Message: For feed reader: "Feed me more!" (call mpg123_feed() or mpg123_decode() with some new input data)
		Err = -1,				// < Generic Error
		Ok = 0,					// < Success
		Bad_OutFormat,			// < Unable to set up output format!
		Bad_Channel,			// < Invalid channel number specified
		Bad_Rate,				// < Invalid sample rate specified
		Err_16To8Table,			// < Unable to allocate memory for 16 to 8 converter table!
		Bad_Param,				// < Bad parameter id!
		Bad_Buffer,				// < Bad buffer given -- invalid pointer or too small size
		Out_Of_Mem,				// < Out of memory -- some malloc() failed
		Not_Initialized,		// < You didn't initialize the library!
		Bad_Decoder,			// < Invalid decoder choice
		Bad_Handle,				// < Invalid mpg123 handle
		No_Buffers,				// < Unable to initialize frame buffers (out of memory?)
		Bad_Rva,				// < Invalid RVA mode
		No_Gapless,				// < This build doesn't support gapless decoding
		No_Space,				// < Not enough buffer space
		Bad_Types,				// < Incompatible numeric data types
		Bad_Band,				// < Bad equalizer band
		Err_Null,				// < Null pointer given where valid storage address needed
		Err_Reader,				// < Error reading the stream
		No_Seek_From_End,		// < Cannot seek from end (end is not known)
		Bad_Whence,				// < Invalid 'whence' for seek function
		No_Timeout,				// < Build does not support stream timeouts
		Bad_File,				// < File access error
		No_Seek,				// < Seek not supported by stream
		No_Reader,				// < No stream opened
		Bad_Pars,				// < Bad parameter handle
		Bad_Index_Par,			// < Bad parameters to mpg123_index() and mpg123_set_index()
		Out_Of_Sync,			// < Lost track in bytestream and did not try to resync
		Resync_Fail,			// < Resync failed to find valid MPEG data
		No_8Bit,				// < No 8bit encoding possible
		Bad_Align,				// < Stack aligmnent error
		Null_Buffer,			// < NULL input buffer with non-zero size...
		No_RelSeek,				// < Relative seek not possible (screwed up file offset)
		Null_Pointer,			// < You gave a null pointer somewhere where you shouldn't have
		Bad_Key,				// < Bad key value given
		No_Index,				// < No frame index in this build
		Index_Fail,				// < Something with frame index went wrong
		Bad_Decoder_Setup,		// < Something prevents a proper decoder setup
		Missing_Feature,		// < This feature has not been built into libmpg123
		Bad_Value,				// < A bad value has been given, somewhere
		LSeek_Failed,			// < Low-level seek failed
		Bad_Custom_Io,			// < Custom I/O not prepared
		Lfs_Overflow,			// < Offset value overflow during translation of large file API calls -- your client program cannot handle that large file
		Int_Overflow,			// < Some integer overflow
		Bad_Float				// < Floating-point computations work not as expected
	}
}
