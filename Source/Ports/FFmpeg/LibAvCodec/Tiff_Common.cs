/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// TIFF common routines
	/// </summary>
	public static class Tiff_Common
	{
		private static readonly uint16_t[] ifd_Tags =
		[
			// EXIF IFD
			0x8769,

			// GPS IFD
			0x8825,

			// Interoperability IFD
			0xa005
		];

		/********************************************************************/
		/// <summary>
		/// Returns a value > 0 if the tag is a known IFD-tag. The return
		/// value is the array index + 1 within ifd_tags[]
		/// </summary>
		/********************************************************************/
		public static c_int FF_Tis_Ifd(c_uint tag)//XX 33
		{
			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(ifd_Tags); i++)
			{
				if (ifd_Tags[i] == tag)
					return i + 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reads a short from the bytestream using given endianness
		/// </summary>
		/********************************************************************/
		public static c_uint FF_TGet_Short(GetByteContext gb, c_int le)//XX 45
		{
			return le != 0 ? ByteStream.ByteStream2_Get_LE16(gb) : ByteStream.ByteStream2_Get_BE16(gb);
		}



		/********************************************************************/
		/// <summary>
		/// Reads a long from the bytestream using given endianness
		/// </summary>
		/********************************************************************/
		public static c_uint FF_TGet_Long(GetByteContext gb, c_int le)//XX 51
		{
			return le != 0 ? ByteStream.ByteStream2_Get_LE32(gb) : ByteStream.ByteStream2_Get_BE32(gb);
		}



		/********************************************************************/
		/// <summary>
		/// Reads a double from the bytestream using given endianness
		/// </summary>
		/********************************************************************/
		public static c_double FF_TGet_Double(GetByteContext gb, c_int le)//XX 57
		{
			uint64_t i = le != 0 ? ByteStream.ByteStream2_Get_LE64(gb) : ByteStream.ByteStream2_Get_BE64(gb);

			return BitConverter.ToDouble(BitConverter.GetBytes(i));
		}



		/********************************************************************/
		/// <summary>
		/// Decodes a TIFF header from the input bytestream and sets the
		/// endianness in *le and the offset to the first IFD in *ifd_offset
		/// accordingly
		/// </summary>
		/********************************************************************/
		public static c_int FF_TDecode_Header(GetByteContext gb, out c_int le, out c_int ifd_Offset)//XX 162
		{
			le = 0;
			ifd_Offset = 0;

			if (ByteStream.ByteStream2_Get_Bytes_Left(gb) < 8)
				return Error.InvalidData;

			le = (c_int)ByteStream.ByteStream2_Get_LE16U(gb);

			if (le == IntReadWrite.Av_RB16("II".ToCharPointer()))
				le = 1;
			else if (le == IntReadWrite.Av_RB16("MM".ToCharPointer()))
				le = 0;
			else
				return Error.InvalidData;

			if (FF_TGet_Short(gb, le) != 42)
				return Error.InvalidData;

			ifd_Offset = (c_int)FF_TGet_Long(gb, le);

			return 0;
		}
	}
}
