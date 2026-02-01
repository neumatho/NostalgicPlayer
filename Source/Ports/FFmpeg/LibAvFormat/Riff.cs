/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Riff
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public const string FF_Pri_Guid = "%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x {%02x%02x%02x%02x-%02x%02x-%02x%02x-%02x%02x-%02x%02x%02x%02x%02x%02x}";



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8_t[] FF_Arg_Guid(FF_Asf_Guid g)
		{
			CPointer<uint8_t> buf = g.Data;

			return
			[
				buf[0], buf[1], buf[2], buf[3], buf[4], buf[5], buf[6], buf[7],
				buf[8], buf[9], buf[10], buf[11], buf[12], buf[13], buf[14], buf[15],
				buf[0], buf[1], buf[2], buf[3], buf[4], buf[5], buf[6], buf[7],
				buf[8], buf[9], buf[10], buf[11], buf[12], buf[13], buf[14], buf[15]
			];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly uint8_t[] FF_MediaSubType_Base_Guid =
		[
			0x00, 0x00, 0x10, 0x00, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly uint8_t[] FF_Ambisonic_Base_Guid =
		[
			0x21, 0x07, 0xd3, 0x11, 0x86, 0x44, 0xc8, 0xc1, 0xca, 0x00, 0x00, 0x00
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly uint8_t[] FF_Broken_Base_Guid =
		[
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x80, 0x00, 0x00, 0xaa
		];



		/********************************************************************/
		/// <summary>
		/// Note: When encoding, the first matching tag is used, so order is
		/// important if multiple tags are possible for a given codec.
		/// Note also that this list is used for more than just riff, other
		/// files use it as well
		/// </summary>
		/********************************************************************/
		public static readonly AvCodecTag[] FF_Codec_Bmp_Tags =
		[
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('H', '2', '6', '4')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('h', '2', '6', '4')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('X', '2', '6', '4')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('x', '2', '6', '4')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('a', 'v', 'c', '1')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('D', 'A', 'V', 'C')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('S', 'M', 'V', '2')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('V', 'S', 'S', 'H')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('Q', '2', '6', '4')),			// QNAP surveillance system
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('V', '2', '6', '4')),			// CCTV recordings
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('G', 'A', 'V', 'C')),			// GeoVision camera
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('U', 'M', 'S', 'V')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('t', 's', 'h', 'd')),
			new AvCodecTag(AvCodecId.H264, Macros.MkTag('I', 'N', 'M', 'C')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('H', '2', '6', '3')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('X', '2', '6', '3')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('T', '2', '6', '3')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('L', '2', '6', '3')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('V', 'X', '1', 'K')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('Z', 'y', 'G', 'o')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('M', '2', '6', '3')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('l', 's', 'v', 'm')),
			new AvCodecTag(AvCodecId.H263P, Macros.MkTag('H', '2', '6', '3')),
			new AvCodecTag(AvCodecId.H263I, Macros.MkTag('I', '2', '6', '3')),		// Intel H.263
			new AvCodecTag(AvCodecId.H261, Macros.MkTag('H', '2', '6', '1')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('U', '2', '6', '3')),
			new AvCodecTag(AvCodecId.H263, Macros.MkTag('V', 'S', 'M', '4')),			// Needs -vf il=l=i:c=i
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('F', 'M', 'P', '4')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'I', 'V', 'X')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'X', '5', '0')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('X', 'V', 'I', 'D')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('M', 'P', '4', 'S')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('M', '4', 'S', '2')),

			// Some broken AVIs use this
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag( 4 ,  0 ,  0 ,  0 )),

			// Some broken AVIs use this
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('Z', 'M', 'P', '4')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'I', 'V', '1')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('B', 'L', 'Z', '0')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('m', 'p', '4', 'v')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('U', 'M', 'P', '4')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('W', 'V', '1', 'F')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('S', 'E', 'D', 'G')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('R', 'M', 'P', '4')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('3', 'I', 'V', '2')),

			// WaWv MPEG-4 Video Codec
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('W', 'A', 'W', 'V')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('F', 'F', 'D', 'S')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('F', 'V', 'F', 'W')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'C', 'O', 'D')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('M', 'V', 'X', 'M')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('P', 'M', '4', 'V')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('S', 'M', 'P', '4')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'X', 'G', 'M')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('V', 'I', 'D', 'M')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('M', '4', 'T', '3')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('G', 'E', 'O', 'X')),

			// Flipped video
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('G', '2', '6', '4')),

			// Flipped video
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('H', 'D', 'X', '4')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'M', '4', 'V')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'M', 'K', '2')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'Y', 'M', '4')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'I', 'G', 'I')),

			// Ephv MPEG-4
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('E', 'P', 'H', 'V')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('E', 'M', '4', 'A')),

			// Divio MPEG-4
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('M', '4', 'C', 'C')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('S', 'N', '4', '0')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('V', 'S', 'P', 'X')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('U', 'L', 'D', 'X')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('G', 'E', 'O', 'V')),

			// Samsung SHR-6040
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('S', 'I', 'P', 'P')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('S', 'M', '4', 'V')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('X', 'V', 'I', 'X')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('D', 'r', 'e', 'X')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('Q', 'M', 'P', '4')),		// QNAP Systems
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('P', 'L', 'V', '1')),		// Pelco DVR MPEG-4
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('G', 'L', 'V', '4')),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('G', 'M', 'P', '4')),		// GeoVision camera
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('M', 'N', 'M', '4')),		// March Networks DVR
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag('G', 'T', 'M', '4')),		// Telefactor
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('M', 'P', '4', '3')),
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('D', 'I', 'V', '3')),
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('M', 'P', 'G', '3')),
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('D', 'I', 'V', '5')),
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('D', 'I', 'V', '6')),
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('D', 'I', 'V', '4')),
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('D', 'V', 'X', '3')),
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('A', 'P', '4', '1')),
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('C', 'O', 'L', '1')),
			new AvCodecTag(AvCodecId.MsMpeg4v3, Macros.MkTag('C', 'O', 'L', '0')),
			new AvCodecTag(AvCodecId.MsMpeg4v2, Macros.MkTag('M', 'P', '4', '2')),
			new AvCodecTag(AvCodecId.MsMpeg4v2, Macros.MkTag('D', 'I', 'V', '2')),
			new AvCodecTag(AvCodecId.MsMpeg4v1, Macros.MkTag('M', 'P', 'G', '4')),
			new AvCodecTag(AvCodecId.MsMpeg4v1, Macros.MkTag('M', 'P', '4', '1')),
			new AvCodecTag(AvCodecId.Wmv1, Macros.MkTag('W', 'M', 'V', '1')),
			new AvCodecTag(AvCodecId.Wmv2, Macros.MkTag('W', 'M', 'V', '2')),
			new AvCodecTag(AvCodecId.Wmv2, Macros.MkTag('G', 'X', 'V', 'E')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', 's', 'd')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', 'h', 'd')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', 'h', '1')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', 's', 'l')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', '2', '5')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', '5', '0')),

			// Canopus DV
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('c', 'd', 'v', 'c')),

			// Canopus DV
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('C', 'D', 'V', 'H')),

			// Canopus DV
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('C', 'D', 'V', '5')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', 'c', ' ')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', 'c', 's')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', 'h', '1')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('d', 'v', 'i', 's')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('p', 'd', 'v', 'c')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('S', 'L', '2', '5')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('S', 'L', 'D', 'V')),
			new AvCodecTag(AvCodecId.DvVideo, Macros.MkTag('A', 'V', 'd', '1')),
			new AvCodecTag(AvCodecId.Mpeg1Video, Macros.MkTag('m', 'p', 'g', '1')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('m', 'p', 'g', '2')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('M', 'P', 'E', 'G')),
			new AvCodecTag(AvCodecId.Mpeg1Video, Macros.MkTag('P', 'I', 'M', '1')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('P', 'I', 'M', '2')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('V', 'C', 'R', '2')),
			new AvCodecTag(AvCodecId.Mpeg1Video, Macros.MkTag( 1 ,  0 ,  0 ,  16)),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag( 2 ,  0 ,  0 ,  16)),
			new AvCodecTag(AvCodecId.Mpeg4, Macros.MkTag( 4 ,  0 ,  0 ,  16)),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('D', 'V', 'R', ' ')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('M', 'M', 'E', 'S')),

			// Lead MPEG-2 in AVI
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('L', 'M', 'P', '2')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('s', 'l', 'i', 'f')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('E', 'M', '2', 'V')),

			// Matrox MPEG-2 intra-only
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('M', '7', '0', '1')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('M', '7', '0', '2')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('M', '7', '0', '3')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('M', '7', '0', '4')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('M', '7', '0', '5')),
			new AvCodecTag(AvCodecId.Mpeg2Video, Macros.MkTag('m', 'p', 'g', 'v')),
			new AvCodecTag(AvCodecId.Mpeg1Video, Macros.MkTag('B', 'W', '1', '0')),
			new AvCodecTag(AvCodecId.Mpeg1Video, Macros.MkTag('X', 'M', 'P', 'G')),	// Xing MPEG intra only
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('M', 'J', 'P', 'G')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('M', 'S', 'C', '2')),		// Multiscope II
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('L', 'J', 'P', 'G')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('d', 'm', 'b', '1')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('m', 'j', 'p', 'a')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('J', 'R', '2', '4')),		// Quadrox Mjpeg
			new AvCodecTag(AvCodecId.LJpeg, Macros.MkTag('L', 'J', 'P', 'G')),

			// Pegasus lossless JPEG
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('J', 'P', 'G', 'L')),

			// JPEG-LS custom FOURCC for AVI - encoder
			new AvCodecTag(AvCodecId.JpegLs, Macros.MkTag('M', 'J', 'L', 'S')),
			new AvCodecTag(AvCodecId.JpegLs, Macros.MkTag('M', 'J', 'P', 'G')),

			// JPEG-LS custom FOURCC for AVI - decoder
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('M', 'J', 'L', 'S')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('j', 'p', 'e', 'g')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('I', 'J', 'P', 'G')),
			new AvCodecTag(AvCodecId.Avrn, Macros.MkTag('A', 'V', 'R', 'n')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('A', 'C', 'D', 'V')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('Q', 'I', 'V', 'G')),

			// SL M-JPEG
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('S', 'L', 'M', 'J')),

			// Creative Webcam JPEG
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('C', 'J', 'P', 'G')),

			// Intel JPEG Library Video Codec
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('I', 'J', 'L', 'V')),

			// Midvid JPEG Video Codec
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('M', 'V', 'J', 'P')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('A', 'V', 'I', '1')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('A', 'V', 'I', '2')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('M', 'T', 'S', 'J')),

			// Paradigm Matrix M-JPEG Codec
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('Z', 'J', 'P', 'G')),
			new AvCodecTag(AvCodecId.MJpeg, Macros.MkTag('M', 'M', 'J', 'P')),
			new AvCodecTag(AvCodecId.HuffYuv, Macros.MkTag('H', 'F', 'Y', 'U')),
			new AvCodecTag(AvCodecId.FfvHuff, Macros.MkTag('F', 'F', 'V', 'H')),
			new AvCodecTag(AvCodecId.Cyuv, Macros.MkTag('C', 'Y', 'U', 'V')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag( 0 ,  0 ,  0 ,  0 )),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag( 3 ,  0 ,  0 ,  0 )),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '2', '0')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'U', 'Y', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '2', '1', '0')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '2', '1', '6')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '4', '1', '6')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '4', '2', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('V', '4', '2', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '4', '1', '0')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'U', 'N', 'V')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('U', 'Y', 'N', 'V')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('U', 'Y', 'N', 'Y')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('u', 'y', 'v', '1')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('2', 'V', 'u', '1')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('2', 'v', 'u', 'y')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('y', 'u', 'v', 's')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('y', 'u', 'v', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('P', '4', '2', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'V', '1', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'V', '1', '6')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'V', '2', '4')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('U', 'Y', 'V', 'Y')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('V', 'Y', 'U', 'Y')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', 'Y', 'U', 'V')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('A', 'Y', 'U', 'V')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '8', '0', '0')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '8', ' ', ' ')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('H', 'D', 'Y', 'C')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'V', 'U', '9')),

			// SoftLab-NSK VideoTizer
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('V', 'D', 'T', 'Z')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '4', '1', '1')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('N', 'V', '1', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('N', 'V', '2', '1')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '4', '1', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', '4', '2', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'U', 'V', '9')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'V', 'U', '9')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('a', 'u', 'v', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'V', 'Y', 'U')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'U', 'Y', 'V')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '1', '0')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '1', '1')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '2', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '4', '0')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '4', '4')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('J', '4', '2', '0')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('J', '4', '2', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('J', '4', '4', '0')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('J', '4', '4', '4')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('Y', 'U', 'V', 'A')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '0', 'A')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '2', 'A')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('R', 'G', 'B', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('R', 'V', '1', '5')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('R', 'V', '1', '6')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('R', 'V', '2', '4')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('R', 'V', '3', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('R', 'G', 'B', 'A')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('A', 'V', '3', '2')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('G', 'R', 'E', 'Y')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '0', '9', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '0', '9', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '2', '9', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '2', '9', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '9', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', '9', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '0', 'A', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '0', 'A', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '2', 'A', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '2', 'A', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', 'A', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', 'A', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', 'F', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', 'F', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '0', 'C', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '0', 'C', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '2', 'C', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '2', 'C', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', 'C', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '4', 'C', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '0', 'F', 'L')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('I', '0', 'F', 'B')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('v', '3', '0', '8')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('v', '4', '0', '8')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('v', '4', '1', '0')),
			new AvCodecTag(AvCodecId.RawVideo, Macros.MkTag('y', '4', '0', '8')),
			new AvCodecTag(AvCodecId.Frwu, Macros.MkTag('F', 'R', 'W', 'U')),
			new AvCodecTag(AvCodecId.R10K, Macros.MkTag('R', '1', '0', 'k')),
			new AvCodecTag(AvCodecId.R210, Macros.MkTag('r', '2', '1', '0')),
			new AvCodecTag(AvCodecId.V210, Macros.MkTag('v', '2', '1', '0')),
			new AvCodecTag(AvCodecId.V210, Macros.MkTag('C', '2', '1', '0')),
			new AvCodecTag(AvCodecId.YUV4, Macros.MkTag('y', 'u', 'v', '4')),
			new AvCodecTag(AvCodecId.Indeo3, Macros.MkTag('I', 'V', '3', '1')),
			new AvCodecTag(AvCodecId.Indeo3, Macros.MkTag('I', 'V', '3', '2')),
			new AvCodecTag(AvCodecId.Indeo4, Macros.MkTag('I', 'V', '4', '1')),
			new AvCodecTag(AvCodecId.Indeo5, Macros.MkTag('I', 'V', '5', '0')),
			new AvCodecTag(AvCodecId.Vp3, Macros.MkTag('V', 'P', '3', '1')),
			new AvCodecTag(AvCodecId.Vp3, Macros.MkTag('V', 'P', '3', '0')),
			new AvCodecTag(AvCodecId.Vp4, Macros.MkTag('V', 'P', '4', '0')),
			new AvCodecTag(AvCodecId.Vp5, Macros.MkTag('V', 'P', '5', '0')),
			new AvCodecTag(AvCodecId.Vp6, Macros.MkTag('V', 'P', '6', '0')),
			new AvCodecTag(AvCodecId.Vp6, Macros.MkTag('V', 'P', '6', '1')),
			new AvCodecTag(AvCodecId.Vp6, Macros.MkTag('V', 'P', '6', '2')),
			new AvCodecTag(AvCodecId.Vp6A, Macros.MkTag('V', 'P', '6', 'A')),
			new AvCodecTag(AvCodecId.Vp6F, Macros.MkTag('V', 'P', '6', 'F')),
			new AvCodecTag(AvCodecId.Vp6F, Macros.MkTag('F', 'L', 'V', '4')),
			new AvCodecTag(AvCodecId.Vp7, Macros.MkTag('V', 'P', '7', '0')),
			new AvCodecTag(AvCodecId.Vp7, Macros.MkTag('V', 'P', '7', '1')),
			new AvCodecTag(AvCodecId.Vp8, Macros.MkTag('V', 'P', '8', '0')),
			new AvCodecTag(AvCodecId.Vp9, Macros.MkTag('V', 'P', '9', '0')),
			new AvCodecTag(AvCodecId.Asv1, Macros.MkTag('A', 'S', 'V', '1')),
			new AvCodecTag(AvCodecId.Asv2, Macros.MkTag('A', 'S', 'V', '2')),
			new AvCodecTag(AvCodecId.Vcr1, Macros.MkTag('V', 'C', 'R', '1')),
			new AvCodecTag(AvCodecId.Ffv1, Macros.MkTag('F', 'F', 'V', '1')),
			new AvCodecTag(AvCodecId.Xan_Wc4, Macros.MkTag('X', 'x', 'a', 'n')),
			new AvCodecTag(AvCodecId.Mimic, Macros.MkTag('L', 'M', '2', '0')),
			new AvCodecTag(AvCodecId.MsRle, Macros.MkTag('m', 'r', 'l', 'e')),
			new AvCodecTag(AvCodecId.MsRle, Macros.MkTag( 1 ,  0 ,  0 ,  0 )),
			new AvCodecTag(AvCodecId.MsRle, Macros.MkTag( 2 ,  0 ,  0 ,  0 )),
			new AvCodecTag(AvCodecId.MsVideo1, Macros.MkTag('M', 'S', 'V', 'C')),
			new AvCodecTag(AvCodecId.MsVideo1, Macros.MkTag('m', 's', 'v', 'c')),
			new AvCodecTag(AvCodecId.MsVideo1, Macros.MkTag('C', 'R', 'A', 'M')),
			new AvCodecTag(AvCodecId.MsVideo1, Macros.MkTag('c', 'r', 'a', 'm')),
			new AvCodecTag(AvCodecId.MsVideo1, Macros.MkTag('W', 'H', 'A', 'M')),
			new AvCodecTag(AvCodecId.MsVideo1, Macros.MkTag('w', 'h', 'a', 'm')),
			new AvCodecTag(AvCodecId.Cinepak, Macros.MkTag('c', 'v', 'i', 'd')),
			new AvCodecTag(AvCodecId.TrueMotion1, Macros.MkTag('D', 'U', 'C', 'K')),
			new AvCodecTag(AvCodecId.TrueMotion1, Macros.MkTag('P', 'V', 'E', 'Z')),
			new AvCodecTag(AvCodecId.Mszh, Macros.MkTag('M', 'S', 'Z', 'H')),
			new AvCodecTag(AvCodecId.ZLib, Macros.MkTag('Z', 'L', 'I', 'B')),
			new AvCodecTag(AvCodecId.Snow, Macros.MkTag('S', 'N', 'O', 'W')),
			new AvCodecTag(AvCodecId._4Xm, Macros.MkTag('4', 'X', 'M', 'V')),
			new AvCodecTag(AvCodecId.Flv1, Macros.MkTag('F', 'L', 'V', '1')),
			new AvCodecTag(AvCodecId.Flv1, Macros.MkTag('S', '2', '6', '3')),
			new AvCodecTag(AvCodecId.FlashSv, Macros.MkTag('F', 'S', 'V', '1')),
			new AvCodecTag(AvCodecId.Svq1, Macros.MkTag('s', 'v', 'q', '1')),
			new AvCodecTag(AvCodecId.Tscc, Macros.MkTag('t', 's', 'c', 'c')),
			new AvCodecTag(AvCodecId.Ulti, Macros.MkTag('U', 'L', 'T', 'I')),
			new AvCodecTag(AvCodecId.Vixl, Macros.MkTag('V', 'I', 'X', 'L')),
			new AvCodecTag(AvCodecId.Qpeg, Macros.MkTag('Q', 'P', 'E', 'G')),
			new AvCodecTag(AvCodecId.Qpeg, Macros.MkTag('Q', '1', '.', '0')),
			new AvCodecTag(AvCodecId.Qpeg, Macros.MkTag('Q', '1', '.', '1')),
			new AvCodecTag(AvCodecId.Wmv3, Macros.MkTag('W', 'M', 'V', '3')),
			new AvCodecTag(AvCodecId.Wmv3Image, Macros.MkTag('W', 'M', 'V', 'P')),
			new AvCodecTag(AvCodecId.Vc1, Macros.MkTag('W', 'V', 'C', '1')),
			new AvCodecTag(AvCodecId.Vc1, Macros.MkTag('W', 'M', 'V', 'A')),
			new AvCodecTag(AvCodecId.Vc1Image, Macros.MkTag('W', 'V', 'P', '2')),
			new AvCodecTag(AvCodecId.Loco, Macros.MkTag('L', 'O', 'C', 'O')),
			new AvCodecTag(AvCodecId.Wnv1, Macros.MkTag('W', 'N', 'V', '1')),
			new AvCodecTag(AvCodecId.Wnv1, Macros.MkTag('Y', 'U', 'V', '8')),
			new AvCodecTag(AvCodecId.Aasc, Macros.MkTag('A', 'A', 'S', '4')),			// Autodesk 24 bit RLE compressor
			new AvCodecTag(AvCodecId.Aasc, Macros.MkTag('A', 'A', 'S', 'C')),
			new AvCodecTag(AvCodecId.Indeo2, Macros.MkTag('R', 'T', '2', '1')),
			new AvCodecTag(AvCodecId.Fraps, Macros.MkTag('F', 'P', 'S', '1')),
			new AvCodecTag(AvCodecId.Theora, Macros.MkTag('t', 'h', 'e', 'o')),
			new AvCodecTag(AvCodecId.TrueMotion2, Macros.MkTag('T', 'M', '2', '0')),
			new AvCodecTag(AvCodecId.TrueMotion2Rt,Macros.MkTag('T', 'R', '2', '0')),
			new AvCodecTag(AvCodecId.Cscd, Macros.MkTag('C', 'S', 'C', 'D')),
			new AvCodecTag(AvCodecId.Zmbv, Macros.MkTag('Z', 'M', 'B', 'V')),
			new AvCodecTag(AvCodecId.Kmvc, Macros.MkTag('K', 'M', 'V', 'C')),
			new AvCodecTag(AvCodecId.Cavs, Macros.MkTag('C', 'A', 'V', 'S')),
			new AvCodecTag(AvCodecId.Avs2, Macros.MkTag('A', 'V', 'S', '2')),
			new AvCodecTag(AvCodecId.Jpeg2000, Macros.MkTag('m', 'j', 'p', '2')),
			new AvCodecTag(AvCodecId.Jpeg2000, Macros.MkTag('M', 'J', '2', 'C')),
			new AvCodecTag(AvCodecId.Jpeg2000, Macros.MkTag('L', 'J', '2', 'C')),
			new AvCodecTag(AvCodecId.Jpeg2000, Macros.MkTag('L', 'J', '2', 'K')),
			new AvCodecTag(AvCodecId.Jpeg2000, Macros.MkTag('I', 'P', 'J', '2')),
			new AvCodecTag(AvCodecId.Jpeg2000, Macros.MkTag('A', 'V', 'j', '2')),		// Avid jpeg2000
			new AvCodecTag(AvCodecId.Vmnc, Macros.MkTag('V', 'M', 'n', 'c')),
			new AvCodecTag(AvCodecId.Targa, Macros.MkTag('t', 'g', 'a', ' ')),
			new AvCodecTag(AvCodecId.Png, Macros.MkTag('M', 'P', 'N', 'G')),
			new AvCodecTag(AvCodecId.Png, Macros.MkTag('P', 'N', 'G', '1')),
			new AvCodecTag(AvCodecId.Png, Macros.MkTag('p', 'n', 'g', ' ')),			// ImageJ
			new AvCodecTag(AvCodecId.Cljr, Macros.MkTag('C', 'L', 'J', 'R')),
			new AvCodecTag(AvCodecId.Dirac, Macros.MkTag('d', 'r', 'a', 'c')),
			new AvCodecTag(AvCodecId.Rpza, Macros.MkTag('a', 'z', 'p', 'r')),
			new AvCodecTag(AvCodecId.Rpza, Macros.MkTag('R', 'P', 'Z', 'A')),
			new AvCodecTag(AvCodecId.Rpza, Macros.MkTag('r', 'p', 'z', 'a')),
			new AvCodecTag(AvCodecId.SP5X, Macros.MkTag('S', 'P', '5', '4')),
			new AvCodecTag(AvCodecId.Aura, Macros.MkTag('A', 'U', 'R', 'A')),
			new AvCodecTag(AvCodecId.Aura2, Macros.MkTag('A', 'U', 'R', '2')),
			new AvCodecTag(AvCodecId.Dpx, Macros.MkTag('d', 'p', 'x', ' ')),
			new AvCodecTag(AvCodecId.Kgv1, Macros.MkTag('K', 'G', 'V', '1')),
			new AvCodecTag(AvCodecId.Lagarith, Macros.MkTag('L', 'A', 'G', 'S')),
			new AvCodecTag(AvCodecId.Amv, Macros.MkTag('A', 'M', 'V', 'F')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'L', 'R', 'A')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'L', 'R', 'G')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'L', 'Y', '0')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'L', 'Y', '2')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'L', 'Y', '4')),

			// Ut Video version 13.0.1 BT.709 codecs
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'L', 'H', '0')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'L', 'H', '2')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'L', 'H', '4')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'Q', 'Y', '0')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'Q', 'Y', '2')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'Q', 'R', 'A')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'Q', 'R', 'G')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'M', 'Y', '2')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'M', 'H', '2')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'M', 'Y', '4')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'M', 'H', '4')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'M', 'R', 'A')),
			new AvCodecTag(AvCodecId.UtVideo, Macros.MkTag('U', 'M', 'R', 'G')),
			new AvCodecTag(AvCodecId.Vble, Macros.MkTag('V', 'B', 'L', 'E')),
			new AvCodecTag(AvCodecId.Escape130, Macros.MkTag('E', '1', '3', '0')),
			new AvCodecTag(AvCodecId.Dxtory, Macros.MkTag('x', 't', 'o', 'r')),
			new AvCodecTag(AvCodecId.ZeroCodec, Macros.MkTag('Z', 'E', 'C', 'O')),
			new AvCodecTag(AvCodecId.Y41P, Macros.MkTag('Y', '4', '1', 'P')),
			new AvCodecTag(AvCodecId.Flic, Macros.MkTag('A', 'F', 'L', 'C')),
			new AvCodecTag(AvCodecId.Mss1, Macros.MkTag('M', 'S', 'S', '1')),
			new AvCodecTag(AvCodecId.Msa1, Macros.MkTag('M', 'S', 'A', '1')),
			new AvCodecTag(AvCodecId.Tscc2, Macros.MkTag('T', 'S', 'C', '2')),
			new AvCodecTag(AvCodecId.Mts2, Macros.MkTag('M', 'T', 'S', '2')),
			new AvCodecTag(AvCodecId.Cllc, Macros.MkTag('C', 'L', 'L', 'C')),
			new AvCodecTag(AvCodecId.Mss2, Macros.MkTag('M', 'S', 'S', '2')),
			new AvCodecTag(AvCodecId.Svq3, Macros.MkTag('S', 'V', 'Q', '3')),
			new AvCodecTag(AvCodecId._012V, Macros.MkTag('0', '1', '2', 'v')),
			new AvCodecTag(AvCodecId._012V, Macros.MkTag('a', '1', '2', 'v')),
			new AvCodecTag(AvCodecId.G2M, Macros.MkTag('G', '2', 'M', '2')),
			new AvCodecTag(AvCodecId.G2M, Macros.MkTag('G', '2', 'M', '3')),
			new AvCodecTag(AvCodecId.G2M, Macros.MkTag('G', '2', 'M', '4')),
			new AvCodecTag(AvCodecId.G2M, Macros.MkTag('G', '2', 'M', '5')),
			new AvCodecTag(AvCodecId.Fic, Macros.MkTag('F', 'I', 'C', 'V')),
			new AvCodecTag(AvCodecId.Hqx, Macros.MkTag('C', 'H', 'Q', 'X')),
			new AvCodecTag(AvCodecId.Tdsc, Macros.MkTag('T', 'D', 'S', 'C')),
			new AvCodecTag(AvCodecId.Hq_Hqa, Macros.MkTag('C', 'U', 'V', 'C')),
			new AvCodecTag(AvCodecId.Rv40, Macros.MkTag('R', 'V', '4', '0')),
			new AvCodecTag(AvCodecId.ScreenPresso, Macros.MkTag('S', 'P', 'V', '1')),
			new AvCodecTag(AvCodecId.Rscc, Macros.MkTag('R', 'S', 'C', 'C')),
			new AvCodecTag(AvCodecId.Rscc, Macros.MkTag('I', 'S', 'C', 'C')),
			new AvCodecTag(AvCodecId.Cfhd, Macros.MkTag('C', 'F', 'H', 'D')),
			new AvCodecTag(AvCodecId.M101, Macros.MkTag('M', '1', '0', '1')),
			new AvCodecTag(AvCodecId.M101, Macros.MkTag('M', '1', '0', '2')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', 'A', 'G', 'Y')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '8', 'R', 'G')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '8', 'R', 'A')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '8', 'G', '0')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '8', 'Y', '0')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '8', 'Y', '2')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '8', 'Y', '4')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '8', 'Y', 'A')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '0', 'R', 'A')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '0', 'R', 'G')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '0', 'G', '0')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '0', 'Y', '0')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '0', 'Y', '2')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '0', 'Y', '4')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '2', 'R', 'A')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '2', 'R', 'G')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '4', 'R', 'A')),
			new AvCodecTag(AvCodecId.MagicYuv, Macros.MkTag('M', '4', 'R', 'G')),
			new AvCodecTag(AvCodecId.Ylc, Macros.MkTag('Y', 'L', 'C', '0')),
			new AvCodecTag(AvCodecId.SpeedHq, Macros.MkTag('S', 'H', 'Q', '0')),
			new AvCodecTag(AvCodecId.SpeedHq, Macros.MkTag('S', 'H', 'Q', '1')),
			new AvCodecTag(AvCodecId.SpeedHq, Macros.MkTag('S', 'H', 'Q', '2')),
			new AvCodecTag(AvCodecId.SpeedHq, Macros.MkTag('S', 'H', 'Q', '3')),
			new AvCodecTag(AvCodecId.SpeedHq, Macros.MkTag('S', 'H', 'Q', '4')),
			new AvCodecTag(AvCodecId.SpeedHq, Macros.MkTag('S', 'H', 'Q', '5')),
			new AvCodecTag(AvCodecId.SpeedHq, Macros.MkTag('S', 'H', 'Q', '7')),
			new AvCodecTag(AvCodecId.SpeedHq, Macros.MkTag('S', 'H', 'Q', '9')),
			new AvCodecTag(AvCodecId.Fmvc, Macros.MkTag('F', 'M', 'V', 'C')),
			new AvCodecTag(AvCodecId.Scpr, Macros.MkTag('S', 'C', 'P', 'R')),
			new AvCodecTag(AvCodecId.ClearVideo, Macros.MkTag('U', 'C', 'O', 'D')),
			new AvCodecTag(AvCodecId.Av1, Macros.MkTag('A', 'V', '0', '1')),
			new AvCodecTag(AvCodecId.Mscc, Macros.MkTag('M', 'S', 'C', 'C')),
			new AvCodecTag(AvCodecId.Srgc, Macros.MkTag('S', 'R', 'G', 'C')),
			new AvCodecTag(AvCodecId.Imm4, Macros.MkTag('I', 'M', 'M', '4')),
			new AvCodecTag(AvCodecId.Prosumer, Macros.MkTag('B', 'T', '2', '0')),
			new AvCodecTag(AvCodecId.Mwsc, Macros.MkTag('M', 'W', 'S', 'C')),
			new AvCodecTag(AvCodecId.Wcmv, Macros.MkTag('W', 'C', 'M', 'V')),
			new AvCodecTag(AvCodecId.Rasc, Macros.MkTag('R', 'A', 'S', 'C')),
			new AvCodecTag(AvCodecId.Hymt, Macros.MkTag('H', 'Y', 'M', 'T')),
			new AvCodecTag(AvCodecId.Arbc, Macros.MkTag('A', 'R', 'B', 'C')),
			new AvCodecTag(AvCodecId.Agm, Macros.MkTag('A', 'G', 'M', '0')),
			new AvCodecTag(AvCodecId.Agm, Macros.MkTag('A', 'G', 'M', '1')),
			new AvCodecTag(AvCodecId.Agm, Macros.MkTag('A', 'G', 'M', '2')),
			new AvCodecTag(AvCodecId.Agm, Macros.MkTag('A', 'G', 'M', '3')),
			new AvCodecTag(AvCodecId.Agm, Macros.MkTag('A', 'G', 'M', '4')),
			new AvCodecTag(AvCodecId.Agm, Macros.MkTag('A', 'G', 'M', '5')),
			new AvCodecTag(AvCodecId.Agm, Macros.MkTag('A', 'G', 'M', '6')),
			new AvCodecTag(AvCodecId.Agm, Macros.MkTag('A', 'G', 'M', '7')),
			new AvCodecTag(AvCodecId.Lscr, Macros.MkTag('L', 'S', 'C', 'R')),
			new AvCodecTag(AvCodecId.Imm5, Macros.MkTag('I', 'M', 'M', '5')),
			new AvCodecTag(AvCodecId.Mvdv, Macros.MkTag('M', 'V', 'D', 'V')),
			new AvCodecTag(AvCodecId.Mvha, Macros.MkTag('M', 'V', 'H', 'A')),
			new AvCodecTag(AvCodecId.Mv30, Macros.MkTag('M', 'V', '3', '0')),
			new AvCodecTag(AvCodecId.Notchlc, Macros.MkTag('n', 'l', 'c', '1')),
			new AvCodecTag(AvCodecId.Vqc, Macros.MkTag('V', 'Q', 'C', '1')),
			new AvCodecTag(AvCodecId.Vqc, Macros.MkTag('V', 'Q', 'C', '2')),
			new AvCodecTag(AvCodecId.Rtv1, Macros.MkTag('R', 'T', 'V', '1')),
			new AvCodecTag(AvCodecId.Vmix, Macros.MkTag('V', 'M', 'X', '1')),
			new AvCodecTag(AvCodecId.Lead, Macros.MkTag('L', 'E', 'A', 'D')),
			new AvCodecTag(AvCodecId.Evc, Macros.MkTag('e', 'v', 'c', '1')),
			new AvCodecTag(AvCodecId.Rv60, Macros.MkTag('R', 'V', '6', '0')),
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly AvCodecTag[] FF_Codec_Bmp_Tags_Unofficial =
		[
			new AvCodecTag(AvCodecId.Hevc, Macros.MkTag('H', 'E', 'V', 'C')),
			new AvCodecTag(AvCodecId.Hevc, Macros.MkTag('H', '2', '6', '5'))
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly AvCodecTag[] FF_Codec_Wav_Tags =
		[
			new AvCodecTag(AvCodecId.Pcm_S16Le, 0x0001),

			// Must come after s16le in this list
			new AvCodecTag(AvCodecId.Pcm_U8,    0x0001),
			new AvCodecTag(AvCodecId.Pcm_S24Le, 0x0001),
			new AvCodecTag(AvCodecId.Pcm_S32Le, 0x0001),
			new AvCodecTag(AvCodecId.Pcm_S64Le, 0x0001),
			new AvCodecTag(AvCodecId.Adpcm_Ms, 0x0002),
			new AvCodecTag(AvCodecId.Pcm_F32Le, 0x0003),

			// Must come after f32le in this list
			new AvCodecTag(AvCodecId.Pcm_F64Le, 0x0003),
			new AvCodecTag(AvCodecId.Pcm_Alaw, 0x0006),
			new AvCodecTag(AvCodecId.Pcm_Mulaw, 0x0007),
			new AvCodecTag(AvCodecId.WmaVoice, 0x000A),
			new AvCodecTag(AvCodecId.Adpcm_Ima_Oki, 0x0010),
			new AvCodecTag(AvCodecId.Adpcm_Ima_Wav, 0x0011),

			// Must come after adpcm_ima_wav in this list
			new AvCodecTag(AvCodecId.Adpcm_Zork, 0x0011),
			new AvCodecTag(AvCodecId.Adpcm_Ima_Oki, 0x0017),
			new AvCodecTag(AvCodecId.Adpcm_Yamaha, 0x0020),
			new AvCodecTag(AvCodecId.TrueSpeech, 0x0022),
			new AvCodecTag(AvCodecId.Gsm_Ms, 0x0031),
			new AvCodecTag(AvCodecId.Gsm_Ms, 0x0032),		// Msn audio
			new AvCodecTag(AvCodecId.Amr_Nb, 0x0038),		// Rogue format number
			new AvCodecTag(AvCodecId.G723_1, 0x0042),
			new AvCodecTag(AvCodecId.Adpcm_G726, 0x0045),
			new AvCodecTag(AvCodecId.Adpcm_G726, 0x0014),	// G723 Antex
			new AvCodecTag(AvCodecId.Adpcm_G726, 0x0040),	// G721 Antex
			new AvCodecTag(AvCodecId.G728, 0x0041),
			new AvCodecTag(AvCodecId.Mp2, 0x0050),
			new AvCodecTag(AvCodecId.Mp3, 0x0055),
			new AvCodecTag(AvCodecId.Amr_Nb, 0x0057),
			new AvCodecTag(AvCodecId.Amr_Wb, 0x0058),

			// Rogue format number
			new AvCodecTag(AvCodecId.Adpcm_Ima_Dk4, 0x0061),

			// Rogue format number
			new AvCodecTag(AvCodecId.Adpcm_Ima_Dk3, 0x0062),
			new AvCodecTag(AvCodecId.Adpcm_G726, 0x0064),
			new AvCodecTag(AvCodecId.Adpcm_Ima_Xbox, 0x0069),
			new AvCodecTag(AvCodecId.Metasound, 0x0075),
			new AvCodecTag(AvCodecId.G729, 0x0083),
			new AvCodecTag(AvCodecId.Aac, 0x00ff),
			new AvCodecTag(AvCodecId.G723_1, 0x0111),
			new AvCodecTag(AvCodecId.Adpcm_Sanyo, 0x0125),
			new AvCodecTag(AvCodecId.Sipr, 0x0130),
			new AvCodecTag(AvCodecId.Acelp_Kelvin, 0x0135),
			new AvCodecTag(AvCodecId.WmaV1, 0x0160),
			new AvCodecTag(AvCodecId.WmaV2, 0x0161),
			new AvCodecTag(AvCodecId.WmaPro, 0x0162),
			new AvCodecTag(AvCodecId.WmaLossless, 0x0163),
			new AvCodecTag(AvCodecId.Xma1, 0x0165),
			new AvCodecTag(AvCodecId.Xma2, 0x0166),
			new AvCodecTag(AvCodecId.Ftr, 0x0180),
			new AvCodecTag(AvCodecId.Adpcm_Ct, 0x0200),
			new AvCodecTag(AvCodecId.DvAudio, 0x0215),
			new AvCodecTag(AvCodecId.DvAudio, 0x0216),
			new AvCodecTag(AvCodecId.Atrac3, 0x0270),
			new AvCodecTag(AvCodecId.MsnSiren, 0x028E),
			new AvCodecTag(AvCodecId.Adpcm_G722, 0x028F),
			new AvCodecTag(AvCodecId.Misc4, 0x0350),
			new AvCodecTag(AvCodecId.Imc, 0x0401),
			new AvCodecTag(AvCodecId.Iac, 0x0402),
			new AvCodecTag(AvCodecId.On2Avc, 0x0500),
			new AvCodecTag(AvCodecId.On2Avc, 0x0501),
			new AvCodecTag(AvCodecId.Gsm_Ms, 0x1500),
			new AvCodecTag(AvCodecId.TrueSpeech, 0x1501),

			// ADTS AAC
			new AvCodecTag(AvCodecId.Aac, 0x1600),
			new AvCodecTag(AvCodecId.Aac_Latm, 0x1602),
			new AvCodecTag(AvCodecId.Ac3, 0x2000),

			// There is no Microsoft Format Tag for E-AC3, the GUID has to be used
			new AvCodecTag(AvCodecId.Eac3, 0x2000),
			new AvCodecTag(AvCodecId.Dts, 0x2001),
			new AvCodecTag(AvCodecId.Sonic, 0x2048),
			new AvCodecTag(AvCodecId.Sonic_Ls, 0x2048),
			new AvCodecTag(AvCodecId.G729, 0x2222),
			new AvCodecTag(AvCodecId.Pcm_Mulaw, 0x6c75),
			new AvCodecTag(AvCodecId.Aac, 0x706d),
			new AvCodecTag(AvCodecId.Aac, 0x4143),
			new AvCodecTag(AvCodecId.Ftr, 0x4180),
			new AvCodecTag(AvCodecId.Xan_Dpcm, 0x594a),
			new AvCodecTag(AvCodecId.G729, 0x729A),
			new AvCodecTag(AvCodecId.Ftr, 0x8180),
			new AvCodecTag(AvCodecId.G723_1, 0xA100),		// Comverse Infosys Ltd. G723 1
			new AvCodecTag(AvCodecId.Aac, 0xA106),
			new AvCodecTag(AvCodecId.Speex, 0xA109),
			new AvCodecTag(AvCodecId.G728, 0xCD02),
			new AvCodecTag(AvCodecId.Flac, 0xF1AC),

			// DFPWM does not have an assigned format tag; it uses a GUID in WAVEFORMATEX instead
			new AvCodecTag(AvCodecId.Dfpwm, 0xFFFE),
			new AvCodecTag(AvCodecId.Adpcm_Swf, ('S' << 8) + 'F'),

			// HACK/FIXME: Does Vorbis in WAV/AVI have an (in)official ID?
			new AvCodecTag(AvCodecId.Vorbis, ('V' << 8) + 'o'),
			new AvCodecTag(AvCodecId.None, 0)
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly AvCodecGuid[] FF_Codec_Wav_Guids =
		[
			new AvCodecGuid(AvCodecId.Ac3, new FF_Asf_Guid([ 0x2C, 0x80, 0x6D, 0xE0, 0x46, 0xDB, 0xCF, 0x11, 0xB4, 0xD1, 0x00, 0x80, 0x5F, 0x6C, 0xBB, 0xEA ])),
			new AvCodecGuid(AvCodecId.Atrac3P, new FF_Asf_Guid([ 0xBF, 0xAA, 0x23, 0xE9, 0x58, 0xCB, 0x71, 0x44, 0xA1, 0x19, 0xFF, 0xFA, 0x01, 0xE4, 0xCE, 0x62 ])),
			new AvCodecGuid(AvCodecId.Atrac9, new FF_Asf_Guid([ 0xD2, 0x42, 0xE1, 0x47, 0xBA, 0x36, 0x8D, 0x4D, 0x88, 0xFC, 0x61, 0x65, 0x4F, 0x8C, 0x83, 0x6C ])),
			new AvCodecGuid(AvCodecId.Eac3, new FF_Asf_Guid([ 0xAF, 0x87, 0xFB, 0xA7, 0x02, 0x2D, 0xFB, 0x42, 0xA4, 0xD4, 0x05, 0xCD, 0x93, 0x84, 0x3B, 0xDD ])),
			new AvCodecGuid(AvCodecId.Mp2, new FF_Asf_Guid([ 0x2B, 0x80, 0x6D, 0xE0, 0x46, 0xDB, 0xCF, 0x11, 0xB4, 0xD1, 0x00, 0x80, 0x5F, 0x6C, 0xBB, 0xEA ])),
			new AvCodecGuid(AvCodecId.Adpcm_Agm, new FF_Asf_Guid([ 0x82, 0xEC, 0x1F, 0x6A, 0xCA, 0xDB, 0x19, 0x45, 0xBD, 0xE7, 0x56, 0xD3, 0xB3, 0xEF, 0x98, 0x1D ])),
			new AvCodecGuid(AvCodecId.Dfpwm, new FF_Asf_Guid([ 0x3A, 0xC1, 0xFA, 0x38, 0x81, 0x1D, 0x43, 0x61, 0xA4, 0x0D, 0xCE, 0x53, 0xCA, 0x60, 0x7C, 0xD1 ]))
		];
	}
}
