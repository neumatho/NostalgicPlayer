/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Various utility functions
	/// </summary>
	public static class Utils
	{
		/********************************************************************/
		/// <summary>
		/// Return a string describing the media_type enum, NULL if
		/// media_type is unknown
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Get_Media_Type_String(AvMediaType media_Type)//XX 28
		{
			switch (media_Type)
			{
				case AvMediaType.Video:
					return "video".ToCharPointer();

				case AvMediaType.Audio:
					return "audio".ToCharPointer();

				case AvMediaType.Data:
					return "data".ToCharPointer();

				case AvMediaType.Subtitle:
					return "subtitle".ToCharPointer();

				case AvMediaType.Attachment:
					return "attachment".ToCharPointer();

				default:
					return null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Fill the provided buffer with a string containing a FourCC
		/// (four-character code) representation
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_FourCC_Make_String(CPointer<char> buf, uint32_t fourCC)//XX 75
		{
			CPointer<char> orig_Buf = buf;
			size_t buf_Size = AvUtil.Av_FourCC_Max_String_Size;

			for (c_int i = 0; i < 4; i++)
			{
				char c = (char)(fourCC & 0xff);
				bool print_Chr = ((c >= '0') && (c <= '9')) ||
								 ((c >= 'a') && (c <= 'z')) ||
								 ((c >= 'A') && (c <= 'Z')) ||
								 ((c != 0) && CString.strchr(". -_".ToCharPointer(), c).IsNotNull);

				c_int len = CString.snprintf(buf, buf_Size, print_Chr ? "%c" : "[%d]", c);

				if (len < 0)
					break;

				buf += len;
				buf_Size = buf_Size > (size_t)len ? buf_Size - (size_t)len : 0;
				fourCC >>= 8;
			}

			return orig_Buf;
		}
	}
}
