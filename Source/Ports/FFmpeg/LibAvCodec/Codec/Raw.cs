/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec
{
	/// <summary>
	/// Raw video codec
	/// </summary>
	public static class Raw
	{
		#region raw_Pix_Fmt_Tags table
		private static readonly PixelFormatTag[] raw_Pix_Fmt_Tags =
		[
			new PixelFormatTag(AvPixelFormat.YUV420P, Macros.MkTag('I', '4', '2', '0')),		// Planar formats
			new PixelFormatTag(AvPixelFormat.YUV420P, Macros.MkTag('I', 'Y', 'U', 'V')),
			new PixelFormatTag(AvPixelFormat.YUV420P, Macros.MkTag('y', 'v', '1', '2')),
			new PixelFormatTag(AvPixelFormat.YUV420P, Macros.MkTag('Y', 'V', '1', '2')),
			new PixelFormatTag(AvPixelFormat.YUV410P, Macros.MkTag('Y', 'U', 'V', '9')),
			new PixelFormatTag(AvPixelFormat.YUV410P, Macros.MkTag('Y', 'V', 'U', '9')),
			new PixelFormatTag(AvPixelFormat.YUV411P, Macros.MkTag('Y', '4', '1', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV422P, Macros.MkTag('Y', '4', '2', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV422P, Macros.MkTag('P', '4', '2', '2')),
			new PixelFormatTag(AvPixelFormat.YUV422P, Macros.MkTag('Y', 'V', '1', '6')),

			// yuvjXXX formats are deprecated hacks specific to libav*,
			// they are identical to yuvXXX
			new PixelFormatTag(AvPixelFormat.YUVJ420P, Macros.MkTag('I', '4', '2', '0')),	// Planar formats
			new PixelFormatTag(AvPixelFormat.YUVJ420P, Macros.MkTag('I', 'Y', 'U', 'V')),
			new PixelFormatTag(AvPixelFormat.YUVJ420P, Macros.MkTag('Y', 'V', '1', '2')),
			new PixelFormatTag(AvPixelFormat.YUVJ422P, Macros.MkTag('Y', '4', '2', 'B')),
			new PixelFormatTag(AvPixelFormat.YUVJ422P, Macros.MkTag('P', '4', '2', '2')),
			new PixelFormatTag(AvPixelFormat.GRAY8, Macros.MkTag('Y', '8', '0', '0')),
			new PixelFormatTag(AvPixelFormat.GRAY8, Macros.MkTag('Y', '8', ' ', ' ')),

			new PixelFormatTag(AvPixelFormat.YUYV422, Macros.MkTag('Y', 'U', 'Y', '2')),		// Packed formats
			new PixelFormatTag(AvPixelFormat.YUYV422, Macros.MkTag('Y', '4', '2', '2')),
			new PixelFormatTag(AvPixelFormat.YUYV422, Macros.MkTag('V', '4', '2', '2')),
			new PixelFormatTag(AvPixelFormat.YUYV422, Macros.MkTag('V', 'Y', 'U', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUYV422, Macros.MkTag('Y', 'U', 'N', 'V')),
			new PixelFormatTag(AvPixelFormat.YUYV422, Macros.MkTag('Y', 'U', 'Y', 'V')),
			new PixelFormatTag(AvPixelFormat.YVYU422, Macros.MkTag('Y', 'V', 'Y', 'U')),		// Philips
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('U', 'Y', 'V', 'Y')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('H', 'D', 'Y', 'C')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('U', 'Y', 'N', 'V')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('U', 'Y', 'N', 'Y')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('u', 'y', 'v', '1')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('2', 'V', 'u', '1')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('A', 'V', 'R', 'n')),		// Avid AVI Codec 1:1
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('A', 'V', '1', 'x')),		// Avid 1:1x
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('A', 'V', 'u', 'p')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('V', 'D', 'T', 'Z')),		// SoftLab-NSK VideoTizer
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('a', 'u', 'v', '2')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('c', 'y', 'u', 'v')),		// CYUV is also Creative YUV
			new PixelFormatTag(AvPixelFormat.UYYVYY411, Macros.MkTag('Y', '4', '1', '1')),
			new PixelFormatTag(AvPixelFormat.GRAY8, Macros.MkTag('G', 'R', 'E', 'Y')),
			new PixelFormatTag(AvPixelFormat.NV12, Macros.MkTag('N', 'V', '1', '2')),
			new PixelFormatTag(AvPixelFormat.NV21, Macros.MkTag('N', 'V', '2', '1')),
			new PixelFormatTag(AvPixelFormat.VUYA, Macros.MkTag('A', 'Y', 'U', 'V')),		// MS 4:4:4:4
			new PixelFormatTag(AvPixelFormat.XV30LE, Macros.MkTag('Y', '4', '1', '0')),
			new PixelFormatTag(AvPixelFormat.XV48LE, Macros.MkTag('Y', '4', '1', '6')),
			new PixelFormatTag(AvPixelFormat.Y210LE, Macros.MkTag('Y', '2', '1', '0')),
			new PixelFormatTag(AvPixelFormat.Y216LE, Macros.MkTag('Y', '2', '1', '6')),

			// Nut
			new PixelFormatTag(AvPixelFormat.RGB555LE, Macros.MkTag('R', 'G', 'B', 15)),
			new PixelFormatTag(AvPixelFormat.BGR555LE, Macros.MkTag('B', 'G', 'R', 15)),
			new PixelFormatTag(AvPixelFormat.RGB565LE, Macros.MkTag('R', 'G', 'B', 16)),
			new PixelFormatTag(AvPixelFormat.BGR565LE, Macros.MkTag('B', 'G', 'R', 16)),
			new PixelFormatTag(AvPixelFormat.RGB555BE, Macros.MkTag(15 , 'B', 'G', 'R')),
			new PixelFormatTag(AvPixelFormat.BGR555BE, Macros.MkTag(15 , 'R', 'G', 'B')),
			new PixelFormatTag(AvPixelFormat.RGB565BE, Macros.MkTag(16 , 'B', 'G', 'R')),
			new PixelFormatTag(AvPixelFormat.BGR565BE, Macros.MkTag(16 , 'R', 'G', 'B')),
			new PixelFormatTag(AvPixelFormat.RGB444LE, Macros.MkTag('R', 'G', 'B', 12)),
			new PixelFormatTag(AvPixelFormat.BGR444LE, Macros.MkTag('B', 'G', 'R', 12)),
			new PixelFormatTag(AvPixelFormat.RGB444BE, Macros.MkTag(12 , 'B', 'G', 'R')),
			new PixelFormatTag(AvPixelFormat.BGR444BE, Macros.MkTag(12 , 'R', 'G', 'B')),
			new PixelFormatTag(AvPixelFormat.RGBA64LE, Macros.MkTag('R', 'B', 'A', 64 )),
			new PixelFormatTag(AvPixelFormat.BGRA64LE, Macros.MkTag('B', 'R', 'A', 64 )),
			new PixelFormatTag(AvPixelFormat.RGBA64BE, Macros.MkTag(64 , 'R', 'B', 'A')),
			new PixelFormatTag(AvPixelFormat.BGRA64BE, Macros.MkTag(64 , 'B', 'R', 'A')),
			new PixelFormatTag(AvPixelFormat.RGBA, Macros.MkTag('R', 'G', 'B', 'A')),
			new PixelFormatTag(AvPixelFormat.RGB0, Macros.MkTag('R', 'G', 'B',  0 )),
			new PixelFormatTag(AvPixelFormat.BGRA, Macros.MkTag('B', 'G', 'R', 'A')),
			new PixelFormatTag(AvPixelFormat.BGR0, Macros.MkTag('B', 'G', 'R',  0 )),
			new PixelFormatTag(AvPixelFormat.ABGR, Macros.MkTag('A', 'B', 'G', 'R')),
			new PixelFormatTag(AvPixelFormat._0BGR, Macros.MkTag( 0 , 'B', 'G', 'R')),
			new PixelFormatTag(AvPixelFormat.ARGB, Macros.MkTag('A', 'R', 'G', 'B')),
			new PixelFormatTag(AvPixelFormat._0RGB, Macros.MkTag( 0 , 'R', 'G', 'B')),
			new PixelFormatTag(AvPixelFormat.RGB24, Macros.MkTag('R', 'G', 'B', 24 )),
			new PixelFormatTag(AvPixelFormat.BGR24, Macros.MkTag('B', 'G', 'R', 24 )),
			new PixelFormatTag(AvPixelFormat.YUV411P, Macros.MkTag('4', '1', '1', 'P')),
			new PixelFormatTag(AvPixelFormat.YUV422P, Macros.MkTag('4', '2', '2', 'P')),
			new PixelFormatTag(AvPixelFormat.YUVJ422P, Macros.MkTag('4', '2', '2', 'P')),
			new PixelFormatTag(AvPixelFormat.YUV440P, Macros.MkTag('4', '4', '0', 'P')),
			new PixelFormatTag(AvPixelFormat.YUVJ440P, Macros.MkTag('4', '4', '0', 'P')),
			new PixelFormatTag(AvPixelFormat.YUV444P, Macros.MkTag('4', '4', '4', 'P')),
			new PixelFormatTag(AvPixelFormat.YUVJ444P, Macros.MkTag('4', '4', '4', 'P')),
			new PixelFormatTag(AvPixelFormat.MONOWHITE, Macros.MkTag('B', '1', 'W', '0')),
			new PixelFormatTag(AvPixelFormat.MONOBLACK, Macros.MkTag('B', '0', 'W', '1')),
			new PixelFormatTag(AvPixelFormat.BGR8, Macros.MkTag('B', 'G', 'R',  8 )),
			new PixelFormatTag(AvPixelFormat.RGB8, Macros.MkTag('R', 'G', 'B',  8 )),
			new PixelFormatTag(AvPixelFormat.BGR4, Macros.MkTag('B', 'G', 'R',  4 )),
			new PixelFormatTag(AvPixelFormat.RGB4, Macros.MkTag('R', 'G', 'B',  4 )),
			new PixelFormatTag(AvPixelFormat.RGB4_BYTE, Macros.MkTag('B', '4', 'B', 'Y')),
			new PixelFormatTag(AvPixelFormat.BGR4_BYTE, Macros.MkTag('R', '4', 'B', 'Y')),
			new PixelFormatTag(AvPixelFormat.RGB48LE, Macros.MkTag('R', 'G', 'B', 48 )),
			new PixelFormatTag(AvPixelFormat.RGB48BE, Macros.MkTag( 48, 'R', 'G', 'B')),
			new PixelFormatTag(AvPixelFormat.BGR48LE, Macros.MkTag('B', 'G', 'R', 48 )),
			new PixelFormatTag(AvPixelFormat.BGR48BE, Macros.MkTag( 48, 'B', 'G', 'R')),
			new PixelFormatTag(AvPixelFormat.GRAY9LE, Macros.MkTag('Y', '1',  0 ,  9 )),
			new PixelFormatTag(AvPixelFormat.GRAY9BE, Macros.MkTag( 9 ,  0 , '1', 'Y')),
			new PixelFormatTag(AvPixelFormat.GRAY10LE, Macros.MkTag('Y', '1',  0 , 10 )),
			new PixelFormatTag(AvPixelFormat.GRAY10BE, Macros.MkTag(10 ,  0 , '1', 'Y')),
			new PixelFormatTag(AvPixelFormat.GRAY12LE, Macros.MkTag('Y', '1',  0 , 12 )),
			new PixelFormatTag(AvPixelFormat.GRAY12BE, Macros.MkTag(12 ,  0 , '1', 'Y')),
			new PixelFormatTag(AvPixelFormat.GRAY14LE, Macros.MkTag('Y', '1',  0 , 14 )),
			new PixelFormatTag(AvPixelFormat.GRAY14BE, Macros.MkTag(14 ,  0 , '1', 'Y')),
			new PixelFormatTag(AvPixelFormat.GRAY16LE, Macros.MkTag('Y', '1',  0 , 16 )),
			new PixelFormatTag(AvPixelFormat.GRAY16BE, Macros.MkTag(16 ,  0 , '1', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV420P9LE, Macros.MkTag('Y', '3', 11 ,  9 )),
			new PixelFormatTag(AvPixelFormat.YUV420P9BE, Macros.MkTag( 9 , 11 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV422P9LE, Macros.MkTag('Y', '3', 10 ,  9 )),
			new PixelFormatTag(AvPixelFormat.YUV422P9BE, Macros.MkTag( 9 , 10 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV444P9LE, Macros.MkTag('Y', '3',  0 ,  9 )),
			new PixelFormatTag(AvPixelFormat.YUV444P9BE, Macros.MkTag( 9 ,  0 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV420P10LE, Macros.MkTag('Y', '3', 11 , 10 )),
			new PixelFormatTag(AvPixelFormat.YUV420P10BE, Macros.MkTag(10 , 11 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV422P10LE, Macros.MkTag('Y', '3', 10 , 10 )),
			new PixelFormatTag(AvPixelFormat.YUV422P10BE, Macros.MkTag(10 , 10 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV444P10LE, Macros.MkTag('Y', '3',  0 , 10 )),
			new PixelFormatTag(AvPixelFormat.YUV444P10BE, Macros.MkTag(10 ,  0 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV420P12LE, Macros.MkTag('Y', '3', 11 , 12 )),
			new PixelFormatTag(AvPixelFormat.YUV420P12BE, Macros.MkTag(12 , 11 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV422P12LE, Macros.MkTag('Y', '3', 10 , 12 )),
			new PixelFormatTag(AvPixelFormat.YUV422P12BE, Macros.MkTag(12 , 10 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV444P12LE, Macros.MkTag('Y', '3',  0 , 12 )),
			new PixelFormatTag(AvPixelFormat.YUV444P12BE, Macros.MkTag(12 ,  0 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV420P14LE, Macros.MkTag('Y', '3', 11 , 14 )),
			new PixelFormatTag(AvPixelFormat.YUV420P14BE, Macros.MkTag(14 , 11 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV422P14LE, Macros.MkTag('Y', '3', 10 , 14 )),
			new PixelFormatTag(AvPixelFormat.YUV422P14BE, Macros.MkTag(14 , 10 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV444P14LE, Macros.MkTag('Y', '3',  0 , 14 )),
			new PixelFormatTag(AvPixelFormat.YUV444P14BE, Macros.MkTag(14 ,  0 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV420P16LE, Macros.MkTag('Y', '3', 11 , 16 )),
			new PixelFormatTag(AvPixelFormat.YUV420P16BE, Macros.MkTag(16 , 11 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV422P16LE, Macros.MkTag('Y', '3', 10 , 16 )),
			new PixelFormatTag(AvPixelFormat.YUV422P16BE, Macros.MkTag(16 , 10 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUV444P16LE, Macros.MkTag('Y', '3',  0 , 16 )),
			new PixelFormatTag(AvPixelFormat.YUV444P16BE, Macros.MkTag(16 ,  0 , '3', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA420P, Macros.MkTag('Y', '4', 11 ,  8 )),
			new PixelFormatTag(AvPixelFormat.YUVA422P, Macros.MkTag('Y', '4', 10 ,  8 )),
			new PixelFormatTag(AvPixelFormat.YUVA444P, Macros.MkTag('Y', '4',  0 ,  8 )),
			new PixelFormatTag(AvPixelFormat.YA8, Macros.MkTag('Y', '2',  0 ,  8 )),
			new PixelFormatTag(AvPixelFormat.PAL8, Macros.MkTag('P', 'A', 'L',  8 )),

			new PixelFormatTag(AvPixelFormat.YUVA420P9LE, Macros.MkTag('Y', '4', 11 ,  9 )),
			new PixelFormatTag(AvPixelFormat.YUVA420P9BE, Macros.MkTag( 9 , 11 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA422P9LE, Macros.MkTag('Y', '4', 10 ,  9 )),
			new PixelFormatTag(AvPixelFormat.YUVA422P9BE, Macros.MkTag( 9 , 10 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA444P9LE, Macros.MkTag('Y', '4',  0 ,  9 )),
			new PixelFormatTag(AvPixelFormat.YUVA444P9BE, Macros.MkTag( 9 ,  0 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA420P10LE, Macros.MkTag('Y', '4', 11 , 10 )),
			new PixelFormatTag(AvPixelFormat.YUVA420P10BE, Macros.MkTag(10 , 11 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA422P10LE, Macros.MkTag('Y', '4', 10 , 10 )),
			new PixelFormatTag(AvPixelFormat.YUVA422P10BE, Macros.MkTag(10 , 10 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA444P10LE, Macros.MkTag('Y', '4',  0 , 10 )),
			new PixelFormatTag(AvPixelFormat.YUVA444P10BE, Macros.MkTag(10 ,  0 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA422P12LE, Macros.MkTag('Y', '4', 10 , 12 )),
			new PixelFormatTag(AvPixelFormat.YUVA422P12BE, Macros.MkTag(12 , 10 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA444P12LE, Macros.MkTag('Y', '4',  0 , 12 )),
			new PixelFormatTag(AvPixelFormat.YUVA444P12BE, Macros.MkTag(12 ,  0 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA420P16LE, Macros.MkTag('Y', '4', 11 , 16 )),
			new PixelFormatTag(AvPixelFormat.YUVA420P16BE, Macros.MkTag(16 , 11 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA422P16LE, Macros.MkTag('Y', '4', 10 , 16 )),
			new PixelFormatTag(AvPixelFormat.YUVA422P16BE, Macros.MkTag(16 , 10 , '4', 'Y')),
			new PixelFormatTag(AvPixelFormat.YUVA444P16LE, Macros.MkTag('Y', '4',  0 , 16 )),
			new PixelFormatTag(AvPixelFormat.YUVA444P16BE, Macros.MkTag(16 ,  0 , '4', 'Y')),

			new PixelFormatTag(AvPixelFormat.GBRP, Macros.MkTag('G', '3', 00 ,  8 )),
			new PixelFormatTag(AvPixelFormat.GBRP9LE, Macros.MkTag('G', '3', 00 ,  9 )),
			new PixelFormatTag(AvPixelFormat.GBRP9BE, Macros.MkTag( 9 , 00 , '3', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRP10LE, Macros.MkTag('G', '3', 00 , 10 )),
			new PixelFormatTag(AvPixelFormat.GBRP10BE, Macros.MkTag(10 , 00 , '3', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRP12LE, Macros.MkTag('G', '3', 00 , 12 )),
			new PixelFormatTag(AvPixelFormat.GBRP12BE, Macros.MkTag(12 , 00 , '3', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRP14LE, Macros.MkTag('G', '3', 00 , 14 )),
			new PixelFormatTag(AvPixelFormat.GBRP14BE, Macros.MkTag(14 , 00 , '3', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRP16LE, Macros.MkTag('G', '3', 00 , 16 )),
			new PixelFormatTag(AvPixelFormat.GBRP16BE, Macros.MkTag(16 , 00 , '3', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRPF16LE, Macros.MkTag('G', '3', 00 , 17 )),
			new PixelFormatTag(AvPixelFormat.GBRPF16BE, Macros.MkTag(17 , 00 , '3', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRPF32LE, Macros.MkTag('G', '3', 00 , 33 )),
			new PixelFormatTag(AvPixelFormat.GBRPF32BE, Macros.MkTag(33 , 00 , '3', 'G')),

			new PixelFormatTag(AvPixelFormat.GBRAP, Macros.MkTag('G', '4', 00 ,  8 )),
			new PixelFormatTag(AvPixelFormat.GBRAP10LE, Macros.MkTag('G', '4', 00 , 10 )),
			new PixelFormatTag(AvPixelFormat.GBRAP10BE, Macros.MkTag(10 , 00 , '4', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRAP12LE, Macros.MkTag('G', '4', 00 , 12 )),
			new PixelFormatTag(AvPixelFormat.GBRAP12BE, Macros.MkTag(12 , 00 , '4', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRAP14LE, Macros.MkTag('G', '4', 00 , 14 )),
			new PixelFormatTag(AvPixelFormat.GBRAP14BE, Macros.MkTag(14 , 00 , '4', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRAP16LE, Macros.MkTag('G', '4', 00 , 16 )),
			new PixelFormatTag(AvPixelFormat.GBRAP16BE, Macros.MkTag(16 , 00 , '4', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRAPF16LE, Macros.MkTag('G', '4', 00 , 17 )),
			new PixelFormatTag(AvPixelFormat.GBRAPF16BE, Macros.MkTag(17 , 00 , '4', 'G')),
			new PixelFormatTag(AvPixelFormat.GBRAPF32LE, Macros.MkTag('G', '4', 00 , 33 )),
			new PixelFormatTag(AvPixelFormat.GBRAPF32BE, Macros.MkTag(33 , 00 , '4', 'G')),

			new PixelFormatTag(AvPixelFormat.XYZ12LE, Macros.MkTag('X', 'Y', 'Z' , 36 )),
			new PixelFormatTag(AvPixelFormat.XYZ12BE, Macros.MkTag(36 , 'Z' , 'Y', 'X')),

			new PixelFormatTag(AvPixelFormat.BAYER_BGGR8, Macros.MkTag(0xBA, 'B', 'G', 8)),
			new PixelFormatTag(AvPixelFormat.BAYER_BGGR16LE, Macros.MkTag(0xBA, 'B', 'G', 16)),
			new PixelFormatTag(AvPixelFormat.BAYER_BGGR16BE, Macros.MkTag(16  , 'G', 'B', 0xBA)),
			new PixelFormatTag(AvPixelFormat.BAYER_RGGB8, Macros.MkTag(0xBA, 'R', 'G', 8)),
			new PixelFormatTag(AvPixelFormat.BAYER_RGGB16LE, Macros.MkTag(0xBA, 'R', 'G', 16)),
			new PixelFormatTag(AvPixelFormat.BAYER_RGGB16BE, Macros.MkTag(16  , 'G', 'R', 0xBA)),
			new PixelFormatTag(AvPixelFormat.BAYER_GBRG8, Macros.MkTag(0xBA, 'G', 'B', 8)),
			new PixelFormatTag(AvPixelFormat.BAYER_GBRG16LE, Macros.MkTag(0xBA, 'G', 'B', 16)),
			new PixelFormatTag(AvPixelFormat.BAYER_GBRG16BE, Macros.MkTag(16,   'B', 'G', 0xBA)),
			new PixelFormatTag(AvPixelFormat.BAYER_GRBG8, Macros.MkTag(0xBA, 'G', 'R', 8)),
			new PixelFormatTag(AvPixelFormat.BAYER_GRBG16LE, Macros.MkTag(0xBA, 'G', 'R', 16)),
			new PixelFormatTag(AvPixelFormat.BAYER_GRBG16BE, Macros.MkTag(16,   'R', 'G', 0xBA)),

			// Quicktime
			new PixelFormatTag(AvPixelFormat.YUV420P, Macros.MkTag('R', '4', '2', '0')),		// Radius DV YUV PAL
			new PixelFormatTag(AvPixelFormat.YUV411P, Macros.MkTag('R', '4', '1', '1')),		// Radius DV YUV NTSC
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('2', 'v', 'u', 'y')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('2', 'V', 'u', 'y')),
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('A', 'V', 'U', 'I')),		// FIXME merge both fields
			new PixelFormatTag(AvPixelFormat.UYVY422, Macros.MkTag('b', 'x', 'y', 'v')),
			new PixelFormatTag(AvPixelFormat.YUYV422, Macros.MkTag('y', 'u', 'v', '2')),
			new PixelFormatTag(AvPixelFormat.YUYV422, Macros.MkTag('y', 'u', 'v', 's')),
			new PixelFormatTag(AvPixelFormat.YUYV422, Macros.MkTag('D', 'V', 'O', 'O')),		// Digital Voodoo SD 8 Bit
			new PixelFormatTag(AvPixelFormat.VYU444,  Macros.MkTag('v', '3', '0', '8')),
			new PixelFormatTag(AvPixelFormat.UYVA, Macros.MkTag('v', '4', '0', '8')),
			new PixelFormatTag(AvPixelFormat.V30XLE, Macros.MkTag('v', '4', '1', '0')),
			new PixelFormatTag(AvPixelFormat.AYUV, Macros.MkTag('y', '4', '0', '8')),
			new PixelFormatTag(AvPixelFormat.RGB555LE, Macros.MkTag('L', '5', '5', '5')),
			new PixelFormatTag(AvPixelFormat.RGB565LE, Macros.MkTag('L', '5', '6', '5')),
			new PixelFormatTag(AvPixelFormat.RGB565BE, Macros.MkTag('B', '5', '6', '5')),
			new PixelFormatTag(AvPixelFormat.BGR24, Macros.MkTag('2', '4', 'B', 'G')),
			new PixelFormatTag(AvPixelFormat.BGR24, Macros.MkTag('b', 'x', 'b', 'g')),
			new PixelFormatTag(AvPixelFormat.BGRA, Macros.MkTag('B', 'G', 'R', 'A')),
			new PixelFormatTag(AvPixelFormat.RGBA, Macros.MkTag('R', 'G', 'B', 'A')),
			new PixelFormatTag(AvPixelFormat.RGB24, Macros.MkTag('b', 'x', 'r', 'g')),
			new PixelFormatTag(AvPixelFormat.ABGR, Macros.MkTag('A', 'B', 'G', 'R')),
			new PixelFormatTag(AvPixelFormat.GRAY16BE, Macros.MkTag('b', '1', '6', 'g')),
			new PixelFormatTag(AvPixelFormat.RGB48BE,  Macros.MkTag('b', '4', '8', 'r')),
			new PixelFormatTag(AvPixelFormat.RGBA64BE, Macros.MkTag('b', '6', '4', 'a')),
			new PixelFormatTag(AvPixelFormat.BAYER_RGGB16BE, Macros.MkTag('B', 'G', 'G', 'R')),

			// Vlc
			new PixelFormatTag(AvPixelFormat.YUV410P, Macros.MkTag('I', '4', '1', '0')),
			new PixelFormatTag(AvPixelFormat.YUV411P, Macros.MkTag('I', '4', '1', '1')),
			new PixelFormatTag(AvPixelFormat.YUV422P, Macros.MkTag('I', '4', '2', '2')),
			new PixelFormatTag(AvPixelFormat.YUV440P, Macros.MkTag('I', '4', '4', '0')),
			new PixelFormatTag(AvPixelFormat.YUV444P, Macros.MkTag('I', '4', '4', '4')),
			new PixelFormatTag(AvPixelFormat.YUVJ420P, Macros.MkTag('J', '4', '2', '0')),
			new PixelFormatTag(AvPixelFormat.YUVJ422P, Macros.MkTag('J', '4', '2', '2')),
			new PixelFormatTag(AvPixelFormat.YUVJ440P, Macros.MkTag('J', '4', '4', '0')),
			new PixelFormatTag(AvPixelFormat.YUVJ444P, Macros.MkTag('J', '4', '4', '4')),
			new PixelFormatTag(AvPixelFormat.YUVA444P, Macros.MkTag('Y', 'U', 'V', 'A')),
			new PixelFormatTag(AvPixelFormat.YUVA420P, Macros.MkTag('I', '4', '0', 'A')),
			new PixelFormatTag(AvPixelFormat.YUVA422P, Macros.MkTag('I', '4', '2', 'A')),
			new PixelFormatTag(AvPixelFormat.RGB8, Macros.MkTag('R', 'G', 'B', '2')),
			new PixelFormatTag(AvPixelFormat.RGB555LE, Macros.MkTag('R', 'V', '1', '5')),
			new PixelFormatTag(AvPixelFormat.RGB565LE, Macros.MkTag('R', 'V', '1', '6')),
			new PixelFormatTag(AvPixelFormat.BGR24, Macros.MkTag('R', 'V', '2', '4')),
			new PixelFormatTag(AvPixelFormat.BGR0, Macros.MkTag('R', 'V', '3', '2')),
			new PixelFormatTag(AvPixelFormat.RGBA, Macros.MkTag('A', 'V', '3', '2')),
			new PixelFormatTag(AvPixelFormat.YUV420P9LE,  Macros.MkTag('I', '0', '9', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV420P9BE,  Macros.MkTag('I', '0', '9', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV422P9LE,  Macros.MkTag('I', '2', '9', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV422P9BE,  Macros.MkTag('I', '2', '9', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV444P9LE,  Macros.MkTag('I', '4', '9', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV444P9BE,  Macros.MkTag('I', '4', '9', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV420P10LE, Macros.MkTag('I', '0', 'A', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV420P10BE, Macros.MkTag('I', '0', 'A', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV422P10LE, Macros.MkTag('I', '2', 'A', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV422P10BE, Macros.MkTag('I', '2', 'A', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV444P10LE, Macros.MkTag('I', '4', 'A', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV444P10BE, Macros.MkTag('I', '4', 'A', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV420P12LE, Macros.MkTag('I', '0', 'C', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV420P12BE, Macros.MkTag('I', '0', 'C', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV422P12LE, Macros.MkTag('I', '2', 'C', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV422P12BE, Macros.MkTag('I', '2', 'C', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV444P12LE, Macros.MkTag('I', '4', 'C', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV444P12BE, Macros.MkTag('I', '4', 'C', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV420P16LE, Macros.MkTag('I', '0', 'F', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV420P16BE, Macros.MkTag('I', '0', 'F', 'B')),
			new PixelFormatTag(AvPixelFormat.YUV444P16LE, Macros.MkTag('I', '4', 'F', 'L')),
			new PixelFormatTag(AvPixelFormat.YUV444P16BE, Macros.MkTag('I', '4', 'F', 'B')),

			// Special
			new PixelFormatTag(AvPixelFormat.RGB565LE, Macros.MkTag( 3 ,  0 ,  0 ,  0 )),	// Flipped RGB565LE
			new PixelFormatTag(AvPixelFormat.YUV444P, Macros.MkTag('Y', 'V', '2', '4')),		// YUV444P, swapped UV
		];
		#endregion

		#region pix_Fmt_Bps_Avi table
		private static readonly PixelFormatTag[] pix_Fmt_Bps_Avi =
		[
			new PixelFormatTag(AvPixelFormat.PAL8, 1),
			new PixelFormatTag(AvPixelFormat.PAL8, 2),
			new PixelFormatTag(AvPixelFormat.PAL8, 4),
			new PixelFormatTag(AvPixelFormat.PAL8, 8),
			new PixelFormatTag(AvPixelFormat.RGB444LE, 12),
			new PixelFormatTag(AvPixelFormat.RGB555LE, 15),
			new PixelFormatTag(AvPixelFormat.RGB555LE, 16),
			new PixelFormatTag(AvPixelFormat.BGR24,24),
			new PixelFormatTag(AvPixelFormat.BGRA, 32)
		];
		#endregion

		#region pix_Fmt_Bps_Mov table
		private static readonly PixelFormatTag[] pix_Fmt_Bps_Mov =
		[
			new PixelFormatTag(AvPixelFormat.PAL8, 1),
			new PixelFormatTag(AvPixelFormat.PAL8, 2),
			new PixelFormatTag(AvPixelFormat.PAL8, 4),
			new PixelFormatTag(AvPixelFormat.PAL8, 8),
			new PixelFormatTag(AvPixelFormat.RGB555BE, 16),
			new PixelFormatTag(AvPixelFormat.RGB24, 24),
			new PixelFormatTag(AvPixelFormat.ARGB, 32),
			new PixelFormatTag(AvPixelFormat.PAL8, 33)
		];
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return a value representing the fourCC code associated to the
		/// pixel format pix_fmt, or 0 if no associated fourCC code can be
		/// found
		/// </summary>
		/********************************************************************/
		public static c_uint AvCodec_Pix_Fmt_To_Codec_Tag(AvPixelFormat fmt)//XX 31
		{
			foreach (PixelFormatTag tag in raw_Pix_Fmt_Tags)
			{
				if (tag.Pix_Fmt == fmt)
					return tag.FourCC;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static AvPixelFormat AvPriv_Pix_Fmt_Find(PixelFormatTagLists list, c_uint fourCC)//XX 78
		{
			PixelFormatTag[] tags = null;

			switch (list)
			{
				case PixelFormatTagLists.Raw:
				{
					tags = raw_Pix_Fmt_Tags;
					break;
				}

				case PixelFormatTagLists.Avi:
				{
					tags = pix_Fmt_Bps_Avi;
					break;
				}

				case PixelFormatTagLists.Mov:
				{
					tags = pix_Fmt_Bps_Mov;
					break;
				}
			}

			return Find_Pix_Fmt(tags, fourCC);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvPixelFormat Find_Pix_Fmt(PixelFormatTag[] tags, c_uint fourCC)//XX 67
		{
			foreach (PixelFormatTag tag in tags)
			{
				if (tag.FourCC == fourCC)
					return tag.Pix_Fmt;
			}

			return AvPixelFormat.None;
		}
		#endregion
	}
}
