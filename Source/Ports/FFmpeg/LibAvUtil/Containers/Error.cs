/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Errors
	/// </summary>
	public static class Error
	{
		/// <summary>
		/// Bitstream filter not found
		/// </summary>
		public static readonly c_int Bsf_Not_Found = FFErrTag(0xf8, 'B', 'S', 'F');

		/// <summary>
		/// Internal bug, also see AVERROR_BUG2
		/// </summary>
		public static readonly c_int Bug = FFErrTag('B', 'U', 'G', '!');

		/// <summary>
		/// Decoder not found
		/// </summary>
		public static readonly c_int Decoder_Not_Found = FFErrTag(0xf8, 'D', 'E', 'C');

		/// <summary>
		/// End of file
		/// </summary>
		public static readonly c_int EOF = FFErrTag('E', 'O', 'F', ' ');

		/// <summary>
		/// Immediate exit was requested; the called function should not be restarted
		/// </summary>
		public static readonly c_int Exit = FFErrTag('E', 'X', 'I', 'T');

		/// <summary>
		/// Invalid data found when processing input
		/// </summary>
		public static readonly c_int InvalidData = FFErrTag('I', 'N', 'D', 'A');

		/// <summary>
		/// Option not found
		/// </summary>
		public static readonly c_int Option_Not_Found = FFErrTag(0xf8, 'O', 'P', 'T');

		/// <summary>
		/// Not yet implemented in FFmpeg, patches welcome
		/// </summary>
		public static readonly c_int PatchWelcome = FFErrTag('P', 'A', 'W', 'E');

		/// <summary>
		/// Protocol not found
		/// </summary>
		public static readonly c_int Protocol_Not_Found = FFErrTag(0xf8, 'P', 'R', 'O');

		/// <summary>
		/// Stream not found
		/// </summary>
		public static readonly c_int Stream_Not_Found = FFErrTag(0xf8, 'S', 'T', 'R');

		/// <summary>
		/// Requested feature is flagged experimental. Set strict_std_compliance if you really want to use it
		/// </summary>
		public static readonly c_int Experimental = -0x2bb2afa8;

		/// <summary>
		/// Returned by demuxers to indicate that data was consumed but discarded
		/// (ignored streams or junk data). The framework will re-call the demuxer
		/// </summary>
		public static readonly c_int Redo = FFErrTag('R', 'E', 'D', 'O');

		// System errors

		/// <summary>
		/// Interrupted system call
		/// </summary>
		public const c_int EINTR = -4;

		/// <summary>
		/// Input/output error
		/// </summary>
		public const c_int EIO = -5;

		/// <summary>
		/// Resource temporarily unavailable
		/// </summary>
		public const c_int EAGAIN = -11;

		/// <summary>
		/// Cannot allocate memory
		/// </summary>
		public const c_int ENOMEM = -12;

		/// <summary>
		/// File exists
		/// </summary>
		public const c_int EEXIST = -17;

		/// <summary>
		/// Invalid argument
		/// </summary>
		public const c_int EINVAL = -22;

		/// <summary>
		/// Broken pipe
		/// </summary>
		public const c_int EPIPE = -32;

		/// <summary>
		/// Numerical result out of range
		/// </summary>
		public const c_int ERANGE = -34;

		/// <summary>
		/// Function not implemented
		/// </summary>
		public const c_int ENOSYS = -38;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Max_String_Size = 64;

		private class Error_Entry
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Error_Entry(c_int num, string str)
			{
				Num = num;
				Str = str.ToCharPointer();
			}

			/// <summary></summary>
			public c_int Num { get; }
			/// <summary></summary>
			public CPointer<char> Str { get; }
 		}

		private static readonly Error_Entry[] error_Entries =
		[
			new Error_Entry(Bsf_Not_Found, "Bitstream filter not found"),
			new Error_Entry(Bug, "Internal bug, should not have happened"),
			new Error_Entry(Decoder_Not_Found, "Decoder not found"),
			new Error_Entry(EOF, "End of file"),
			new Error_Entry(Exit, "Immediate exit requested"),
			new Error_Entry(InvalidData, "Invalid data found when processing input"),
			new Error_Entry(Option_Not_Found, "Option not found"),
			new Error_Entry(PatchWelcome, "Not yet implemented in FFmpeg, patches welcome"),
			new Error_Entry(Protocol_Not_Found, "Protocol not found"),
			new Error_Entry(Stream_Not_Found, "Stream not found"),
			new Error_Entry(Experimental, "Experimental feature"),

			new Error_Entry(EAGAIN, "Resource temporarily unavailable"),
			new Error_Entry(EINTR, "Interrupted system call"),
			new Error_Entry(EINVAL, "Invalid argument"),
			new Error_Entry(EIO, "I/O error"),
			new Error_Entry(ENOMEM, "Cannot allocate memory"),
			new Error_Entry(EEXIST, "File exists"),
			new Error_Entry(ENOSYS, "Function not implemented"),
			new Error_Entry(EPIPE, "Broken pipe"),
			new Error_Entry(ERANGE, "Result too large")
		];

		/********************************************************************/
		/// <summary>
		/// Fill the provided buffer with a string containing an error string
		/// corresponding to the AVERROR code errnum
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<char> Av_Make_Error_String(CPointer<char> errBuf, size_t errBuf_Size, c_int errNum)
		{
			Av_StrError(errNum, errBuf, errBuf_Size);

			return errBuf;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<char> Av_Err2Str(c_int errNum)
		{
			CPointer<char> buf = new CPointer<char>(Max_String_Size);

			return Av_Make_Error_String(buf, Max_String_Size, errNum);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_StrError(c_int errNum, CPointer<char> errBuf, size_t errBuf_Size)
		{
			c_int ret = 0;
			Error_Entry entry = null;

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(error_Entries); i++)
			{
				if (errNum == error_Entries[i].Num)
				{
					entry = error_Entries[i];
					break;
				}
			}

			if (entry != null)
				AvString.Av_Strlcpy(errBuf, entry.Str, errBuf_Size);
			else
			{
				ret = -1;
				CString.snprintf(errBuf, errBuf_Size, "Error number %d occurred", errNum);
			}

			return ret;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int FFErrTag(c_int a, c_int b, c_int c, c_int d)
		{
			return -(c_int)Macros.MkTag(a, b, c, d);
		}
		#endregion
	}
}
