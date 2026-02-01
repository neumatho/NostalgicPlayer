/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Pixel format descriptor
	/// </summary>
	public static class PixDesc
	{
		private static readonly AVPixFmtDescriptor[] av_Pix_Fmt_Descriptors = BuildDescriptors();

		private static readonly string[] color_Range_Names =
		[
			"unknown",
			"tv",
			"pc"
		];

		private static readonly string[] color_Primaries_Names =
		[
			"reserved",
			"bt709",
			"unknown",
			"reserved",
			"bt470m",
			"bt470bg",
			"smpte170m",
			"smpte240m",
			"film",
			"bt2020",
			"smpte428",
			"smpte431",
			"smpte432",
			"ebu3213"
		];

		private static readonly string[] color_Transfer_Names =
		[
			"reserved",
			"bt709",
			"unknown",
			"reserved",
			"bt470m",
			"bt470bg",
			"smpte170m",
			"smpte240m",
			"linear",
			"log100",
			"log316",
			"iec61966-2-4",
			"bt1361e",
			"iec61966-2-1",
			"bt2020-10",
			"bt2020-12",
			"smpte2084",
			"smpte428",
			"arib-std-b67"
		];

		private static readonly string[] color_Space_Names =
		[
			"gbr",
			"bt709",
			"unknown",
			"reserved",
			"fcc",
			"bt470bg",
			"smpte170m",
			"smpte240m",
			"ycgco",
			"bt2020nc",
			"bt2020c",
			"smpte2085",
			"chroma-derived-nc",
			"chroma-derived-c",
			"ictcp",
			"ipt-c2",
			"ycgco-re",
			"ycgco-ro"
		];

		private static readonly string[] chroma_Location_Names =
		[
			"unspecified",
			"left",
			"center",
			"topleft",
			"top",
			"bottomleft",
			"bottom"
		];

		#region Build descriptors
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AVPixFmtDescriptor[] BuildDescriptors()
		{
			AVPixFmtDescriptor[] arr = new AVPixFmtDescriptor[(c_int)AvPixelFormat.Nb];

			arr[(c_int)AvPixelFormat.YUV420P] = new AVPixFmtDescriptor
			{
				Name = "yuv420p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUYV422] = new AVPixFmtDescriptor
			{
				Name = "yuyv422".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 8),	// Y
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// U
					new AvComponentDescriptor(0, 4, 3, 0, 8)		// V
				]
			};
			arr[(c_int)AvPixelFormat.YVYU422] = new AVPixFmtDescriptor
			{
				Name = "yvyu422".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 8),	// Y
					new AvComponentDescriptor(0, 4, 3, 0, 8),	// U
					new AvComponentDescriptor(0, 4, 1, 0, 8)		// V
				]
			};
			arr[(c_int)AvPixelFormat.Y210LE] = new AVPixFmtDescriptor
			{
				Name = "y210le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 6, 10),	// Y
					new AvComponentDescriptor(0, 8, 2, 6, 10),	// U
					new AvComponentDescriptor(0, 8, 6, 6, 10)	// V
				]
			};
			arr[(c_int)AvPixelFormat.Y210BE] = new AVPixFmtDescriptor
			{
				Name = "y210be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 6, 10),	// Y
					new AvComponentDescriptor(0, 8, 2, 6, 10),	// U
					new AvComponentDescriptor(0, 8, 6, 6, 10)	// V
				],
				Flags = AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.RGB24] = new AVPixFmtDescriptor
			{
				Name = "rgb24".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 3, 0, 0, 8),	// R
					new AvComponentDescriptor(0, 3, 1, 0, 8),	// G
					new AvComponentDescriptor(0, 3, 2, 0, 8)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.BGR24] = new AVPixFmtDescriptor
			{
				Name = "bgr24".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 3, 2, 0, 8),	// R
					new AvComponentDescriptor(0, 3, 1, 0, 8),	// G
					new AvComponentDescriptor(0, 3, 0, 0, 8)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.X2RGB10LE] = new AVPixFmtDescriptor
			{
				Name = "x2rgb10le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 2, 4, 10),	// R
					new AvComponentDescriptor(0, 4, 1, 2, 10),	// G
					new AvComponentDescriptor(0, 4, 0, 0, 10),	// B
					new AvComponentDescriptor(0, 4, 3, 6,  2)	// X
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.X2RGB10BE] = new AVPixFmtDescriptor
			{
				Name = "x2rgb10be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 4, 10),	// R
					new AvComponentDescriptor(0, 4, 1, 2, 10),	// G
					new AvComponentDescriptor(0, 4, 2, 0, 10),	// B
					new AvComponentDescriptor(0, 4, 3, 6,  2)	// X
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.X2BGR10LE] = new AVPixFmtDescriptor
			{
				Name = "x2bgr10le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 10),	// R
					new AvComponentDescriptor(0, 4, 1, 2, 10),	// G
					new AvComponentDescriptor(0, 4, 2, 4, 10),	// B
					new AvComponentDescriptor(0, 4, 3, 6,  2)	// X
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.X2BGR10BE] = new AVPixFmtDescriptor
			{
				Name = "x2bgr10be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 2, 0, 10),	// R
					new AvComponentDescriptor(0, 4, 1, 2, 10),	// G
					new AvComponentDescriptor(0, 4, 0, 4, 10),	// B
					new AvComponentDescriptor(0, 4, 3, 6,  2)	// X
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV422P] = new AVPixFmtDescriptor
			{
				Name = "yuv422p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV444P] = new AVPixFmtDescriptor
			{
				Name = "yuv444p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV410P] = new AVPixFmtDescriptor
			{
				Name = "yuv410p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 2,
				Log2_Chroma_H = 2,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV411P] = new AVPixFmtDescriptor
			{
				Name = "yuv411p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 2,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUVJ411P] = new AVPixFmtDescriptor
			{
				Name = "yuvj411p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 2,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.GRAY8] = new AVPixFmtDescriptor
			{
				Name = "gray".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8)		// Y
				],
				Alias = "gray8,y8".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.MONOWHITE] = new AVPixFmtDescriptor
			{
				Name = "monow".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 1)		// Y
				],
				Flags = AvPixelFormatFlag.Bitstream
			};
			arr[(c_int)AvPixelFormat.MONOBLACK] = new AVPixFmtDescriptor
			{
				Name = "monob".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 7, 1)		// Y
				],
				Flags = AvPixelFormatFlag.Bitstream
			};
			arr[(c_int)AvPixelFormat.PAL8] = new AVPixFmtDescriptor
			{
				Name = "pal8".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8)
				],
				Flags = AvPixelFormatFlag.Pal | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVJ420P] = new AVPixFmtDescriptor
			{
				Name = "yuvj420p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUVJ422P] = new AVPixFmtDescriptor
			{
				Name = "yuvj422p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUVJ444P] = new AVPixFmtDescriptor
			{
				Name = "yuvj444p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.UYVY422] = new AVPixFmtDescriptor
			{
				Name = "uyvy422".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 1, 0, 8),	// Y
					new AvComponentDescriptor(0, 4, 0, 0, 8),	// U
					new AvComponentDescriptor(0, 4, 2, 0, 8)		// V
				]
			};
			arr[(c_int)AvPixelFormat.UYYVYY411] = new AVPixFmtDescriptor
			{
				Name = "uyyvyy411".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 2,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// Y
					new AvComponentDescriptor(0, 6, 0, 0, 8),	// U
					new AvComponentDescriptor(0, 6, 3, 0, 8)		// V
				]
			};
			arr[(c_int)AvPixelFormat.BGR8] = new AVPixFmtDescriptor
			{
				Name = "bgr8".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 3),	// R
					new AvComponentDescriptor(0, 1, 0, 3, 3),	// G
					new AvComponentDescriptor(0, 1, 0, 6, 2)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.BGR4] = new AVPixFmtDescriptor
			{
				Name = "bgr4".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 3, 0, 1),	// R
					new AvComponentDescriptor(0, 4, 1, 0, 2),	// G
					new AvComponentDescriptor(0, 4, 0, 0, 1)		// B
				],
				Flags = AvPixelFormatFlag.Bitstream | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.BGR4_BYTE] = new AVPixFmtDescriptor
			{
				Name = "bgr4_byte".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 1),	// R
					new AvComponentDescriptor(0, 1, 0, 1, 2),	// G
					new AvComponentDescriptor(0, 1, 0, 3, 1)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.RGB8] = new AVPixFmtDescriptor
			{
				Name = "rgb8".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 5, 3),	// R
					new AvComponentDescriptor(0, 1, 0, 2, 3),	// G
					new AvComponentDescriptor(0, 1, 0, 0, 2)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.RGB4] = new AVPixFmtDescriptor
			{
				Name = "rgb4".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 1),	// R
					new AvComponentDescriptor(0, 4, 1, 0, 2),	// G
					new AvComponentDescriptor(0, 4, 3, 0, 1)		// B
				],
				Flags = AvPixelFormatFlag.Bitstream | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.RGB4_BYTE] = new AVPixFmtDescriptor
			{
				Name = "rgb4_byte".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 3, 1),	// R
					new AvComponentDescriptor(0, 1, 0, 1, 2),	// G
					new AvComponentDescriptor(0, 1, 0, 0, 1)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.NV12] = new AVPixFmtDescriptor
			{
				Name = "nv12".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 8),	// U
					new AvComponentDescriptor(1, 2, 1, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.NV21] = new AVPixFmtDescriptor
			{
				Name = "nv21".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 2, 1, 0, 8),	// U
					new AvComponentDescriptor(1, 2, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.ARGB] = new AVPixFmtDescriptor
			{
				Name = "argb".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// R
					new AvComponentDescriptor(0, 4, 2, 0, 8),	// G
					new AvComponentDescriptor(0, 4, 3, 0, 8),	// B
					new AvComponentDescriptor(0, 4, 0, 0, 8)		// A
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.RGBA] = new AVPixFmtDescriptor
			{
				Name = "rgba".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 8),	// R
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// G
					new AvComponentDescriptor(0, 4, 2, 0, 8),	// B
					new AvComponentDescriptor(0, 4, 3, 0, 8)		// A
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.ABGR] = new AVPixFmtDescriptor
			{
				Name = "abgr".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 3, 0, 8),	// R
					new AvComponentDescriptor(0, 4, 2, 0, 8),	// G
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// B
					new AvComponentDescriptor(0, 4, 0, 0, 8)		// A
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.BGRA] = new AVPixFmtDescriptor
			{
				Name = "bgra".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 2, 0, 8),	// R
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// G
					new AvComponentDescriptor(0, 4, 0, 0, 8),	// B
					new AvComponentDescriptor(0, 4, 3, 0, 8)		// A
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat._0RGB] = new AVPixFmtDescriptor
			{
				Name = "0rgb".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// R
					new AvComponentDescriptor(0, 4, 2, 0, 8),	// G
					new AvComponentDescriptor(0, 4, 3, 0, 8),	// B
					new AvComponentDescriptor(0, 4, 0, 0, 8)		// X
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.RGB0] = new AVPixFmtDescriptor
			{
				Name = "rgb0".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 8),	// R
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// G
					new AvComponentDescriptor(0, 4, 2, 0, 8),	// B
					new AvComponentDescriptor(0, 4, 3, 0, 8)		// X
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat._0BGR] = new AVPixFmtDescriptor
			{
				Name = "0bgr".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 3, 0, 8),	// R
					new AvComponentDescriptor(0, 4, 2, 0, 8),	// G
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// B
					new AvComponentDescriptor(0, 4, 0, 0, 8)		// X
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.BGR0] = new AVPixFmtDescriptor
			{
				Name = "bgr0".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 2, 0, 8),	// R
					new AvComponentDescriptor(0, 4, 1, 0, 8),	// G
					new AvComponentDescriptor(0, 4, 0, 0, 8),	// B
					new AvComponentDescriptor(0, 4, 3, 0, 8)		// X
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GRAY9BE] = new AVPixFmtDescriptor
			{
				Name = "gray9be".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9)		// Y
				],
				Flags = AvPixelFormatFlag.Be,
				Alias = "y9be".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY9LE] = new AVPixFmtDescriptor
			{
				Name = "gray9le".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9)		// Y
				],
				Alias = "y9le".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY10BE] = new AVPixFmtDescriptor
			{
				Name = "gray10be".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10)	// Y
				],
				Flags = AvPixelFormatFlag.Be,
				Alias = "y10be".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY10LE] = new AVPixFmtDescriptor
			{
				Name = "gray10le".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10)	// Y
				],
				Alias = "y10le".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY12BE] = new AVPixFmtDescriptor
			{
				Name = "gray12be".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12)	// Y
				],
				Flags = AvPixelFormatFlag.Be,
				Alias = "y12be".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY12LE] = new AVPixFmtDescriptor
			{
				Name = "gray12le".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12)	// Y
				],
				Alias = "y12le".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY14BE] = new AVPixFmtDescriptor
			{
				Name = "gray14be".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 14)	// Y
				],
				Flags = AvPixelFormatFlag.Be,
				Alias = "y14be".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY14LE] = new AVPixFmtDescriptor
			{
				Name = "gray14le".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 14)	// Y
				],
				Alias = "y14le".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY16BE] = new AVPixFmtDescriptor
			{
				Name = "gray16be".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16)	// Y
				],
				Flags = AvPixelFormatFlag.Be,
				Alias = "y16be".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY16LE] = new AVPixFmtDescriptor
			{
				Name = "gray16le".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16)	// Y
				],
				Alias = "y16le".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY32BE] = new AVPixFmtDescriptor
			{
				Name = "gray32be".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 32)	// Y
				],
				Flags = AvPixelFormatFlag.Be,
				Alias = "y32be".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAY32LE] = new AVPixFmtDescriptor
			{
				Name = "gray32le".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 32)	// Y
				],
				Alias = "y32le".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.YUV440P] = new AVPixFmtDescriptor
			{
				Name = "yuv440p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUVJ440P] = new AVPixFmtDescriptor
			{
				Name = "yuvj440p".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV440P10LE] = new AVPixFmtDescriptor
			{
				Name = "yuv440p10le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV440P10BE] = new AVPixFmtDescriptor
			{
				Name = "yuv440p10be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10)	// V
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV440P12LE] = new AVPixFmtDescriptor
			{
				Name = "yuv440p12le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 12)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV440P12BE] = new AVPixFmtDescriptor
			{
				Name = "yuv440p12be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 12)	// V
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUVA420P] = new AVPixFmtDescriptor
			{
				Name = "yuva420p".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8),	// V
					new AvComponentDescriptor(3, 1, 0, 0, 8)		// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA422P] = new AVPixFmtDescriptor
			{
				Name = "yuva422p".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8),	// V
					new AvComponentDescriptor(3, 1, 0, 0, 8)		// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA444P] = new AVPixFmtDescriptor
			{
				Name = "yuva444p".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// Y
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// U
					new AvComponentDescriptor(2, 1, 0, 0, 8),	// V
					new AvComponentDescriptor(3, 1, 0, 0, 8)		// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA420P9LE] = new AVPixFmtDescriptor
			{
				Name = "yuva420p9le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 9)		// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA420P9BE] = new AVPixFmtDescriptor
			{
				Name = "yuva420p9be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 9)		// A
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA422P9LE] = new AVPixFmtDescriptor
			{
				Name = "yuva422p9le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 9)		// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA422P9BE] = new AVPixFmtDescriptor
			{
				Name = "yuva422p9be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 9)		// A
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA444P9LE] = new AVPixFmtDescriptor
			{
				Name = "yuva444p9le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 9)		// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA444P9BE] = new AVPixFmtDescriptor
			{
				Name = "yuva444p9be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 9)		// A
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA420P10LE] = new AVPixFmtDescriptor
			{
				Name = "yuva420p10le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 10)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA420P10BE] = new AVPixFmtDescriptor
			{
				Name = "yuva420p10be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 10)	// A
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA422P10LE] = new AVPixFmtDescriptor
			{
				Name = "yuva422p10le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 10)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA422P10BE] = new AVPixFmtDescriptor
			{
				Name = "yuva422p10be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 10)	// A
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA444P10LE] = new AVPixFmtDescriptor
			{
				Name = "yuva444p10le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 10)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA444P10BE] = new AVPixFmtDescriptor
			{
				Name = "yuva444p10be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 10)	// A
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA420P16LE] = new AVPixFmtDescriptor
			{
				Name = "yuva420p16le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA420P16BE] = new AVPixFmtDescriptor
			{
				Name = "yuva420p16be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA422P16LE] = new AVPixFmtDescriptor
			{
				Name = "yuva422p16le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA422P16BE] = new AVPixFmtDescriptor
			{
				Name = "yuva422p16be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA444P16LE] = new AVPixFmtDescriptor
			{
				Name = "yuva444p16le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA444P16BE] = new AVPixFmtDescriptor
			{
				Name = "yuva444p16be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// V
					new AvComponentDescriptor(3, 2, 0, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.RGB48LE] = new AVPixFmtDescriptor
			{
				Name = "rgb48le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 6, 0, 0, 16),	// R
					new AvComponentDescriptor(0, 6, 2, 0, 16),	// G
					new AvComponentDescriptor(0, 6, 4, 0, 16)	// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.RGB48BE] = new AVPixFmtDescriptor
			{
				Name = "rgb48be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 6, 0, 0, 16),	// R
					new AvComponentDescriptor(0, 6, 2, 0, 16),	// G
					new AvComponentDescriptor(0, 6, 4, 0, 16)	// B
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.RGBA64LE] = new AVPixFmtDescriptor
			{
				Name = "rgba64le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 0, 0, 16),	// R
					new AvComponentDescriptor(0, 8, 2, 0, 16),	// G
					new AvComponentDescriptor(0, 8, 4, 0, 16),	// B
					new AvComponentDescriptor(0, 8, 6, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.RGBA64BE] = new AVPixFmtDescriptor
			{
				Name = "rgba64be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 0, 0, 16),	// R
					new AvComponentDescriptor(0, 8, 2, 0, 16),	// G
					new AvComponentDescriptor(0, 8, 4, 0, 16),	// B
					new AvComponentDescriptor(0, 8, 6, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.RGB565LE] = new AVPixFmtDescriptor
			{
				Name = "rgb565le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 1, 3, 5),	// R
					new AvComponentDescriptor(0, 2, 0, 5, 6),	// G
					new AvComponentDescriptor(0, 2, 0, 0, 5)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.RGB565BE] = new AVPixFmtDescriptor
			{
				Name = "rgb565be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, -1, 3, 5),	// R
					new AvComponentDescriptor(0, 2,  0, 5, 6),	// G
					new AvComponentDescriptor(0, 2,  0, 0, 5)	// B
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.RGB555LE] = new AVPixFmtDescriptor
			{
				Name = "rgb555le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 1, 2, 5),	// R
					new AvComponentDescriptor(0, 2, 0, 5, 5),	// G
					new AvComponentDescriptor(0, 2, 0, 0, 5)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.RGB555BE] = new AVPixFmtDescriptor
			{
				Name = "rgb555be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, -1, 2, 5),	// R
					new AvComponentDescriptor(0, 2,  0, 5, 5),	// G
					new AvComponentDescriptor(0, 2,  0, 0, 5)	// B
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.RGB444BE] = new AVPixFmtDescriptor
			{
				Name = "rgb444be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, -1, 0, 4),	// R
					new AvComponentDescriptor(0, 2,  0, 4, 4),	// G
					new AvComponentDescriptor(0, 2,  0, 0, 4)	// B
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.RGB444LE] = new AVPixFmtDescriptor
			{
				Name = "rgb444le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 1, 0, 4),	// R
					new AvComponentDescriptor(0, 2, 0, 4, 4),	// G
					new AvComponentDescriptor(0, 2, 0, 0, 4)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.BGR48BE] = new AVPixFmtDescriptor
			{
				Name = "bgr48be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 6, 4, 0, 16),	// R
					new AvComponentDescriptor(0, 6, 2, 0, 16),	// G
					new AvComponentDescriptor(0, 6, 0, 0, 16)	// B
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.BGR48LE] = new AVPixFmtDescriptor
			{
				Name = "bgr48le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 6, 4, 0, 16),	// R
					new AvComponentDescriptor(0, 6, 2, 0, 16),	// G
					new AvComponentDescriptor(0, 6, 0, 0, 16)	// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.BGRA64BE] = new AVPixFmtDescriptor
			{
				Name = "bgra64be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 4, 0, 16),	// R
					new AvComponentDescriptor(0, 8, 2, 0, 16),	// G
					new AvComponentDescriptor(0, 8, 0, 0, 16),	// B
					new AvComponentDescriptor(0, 8, 6, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.BGRA64LE] = new AVPixFmtDescriptor
			{
				Name = "bgra64le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 4, 0, 16),	// R
					new AvComponentDescriptor(0, 8, 2, 0, 16),	// G
					new AvComponentDescriptor(0, 8, 0, 0, 16),	// B
					new AvComponentDescriptor(0, 8, 6, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.BGR565BE] = new AVPixFmtDescriptor
			{
				Name = "bgr565be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2,  0, 0, 5),	// R
					new AvComponentDescriptor(0, 2,  0, 5, 6),	// G
					new AvComponentDescriptor(0, 2, -1, 3, 5)	// B
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.BGR565LE] = new AVPixFmtDescriptor
			{
				Name = "bgr565le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 5),	// R
					new AvComponentDescriptor(0, 2, 0, 5, 6),	// G
					new AvComponentDescriptor(0, 2, 1, 3, 5)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.BGR555BE] = new AVPixFmtDescriptor
			{
				Name = "bgr555be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2,  0, 0, 5),	// R
					new AvComponentDescriptor(0, 2,  0, 5, 5),	// G
					new AvComponentDescriptor(0, 2, -1, 2, 5)	// B
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.BGR555LE] = new AVPixFmtDescriptor
			{
				Name = "bgr555le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 5),	// R
					new AvComponentDescriptor(0, 2, 0, 5, 5),	// G
					new AvComponentDescriptor(0, 2, 1, 2, 5)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.BGR444BE] = new AVPixFmtDescriptor
			{
				Name = "bgr444be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2,  0, 0, 4),	// R
					new AvComponentDescriptor(0, 2,  0, 4, 4),	// G
					new AvComponentDescriptor(0, 2, -1, 0, 4)	// B
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.BGR444LE] = new AVPixFmtDescriptor
			{
				Name = "bgr444le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 4),	// R
					new AvComponentDescriptor(0, 2, 0, 4, 4),	// G
					new AvComponentDescriptor(0, 2, 1, 0, 4)		// B
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.VAAPI] = new AVPixFmtDescriptor
			{
				Name = "vaapi".ToCharPointer(),
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 0, 0, 0, 0)
				],
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.YUV420P9LE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p9le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV420P9BE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p9be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9)		// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV420P10LE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p10le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV420P10BE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p10be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV420P12LE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p12le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 12)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV420P12BE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p12be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 12)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV420P14LE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p14le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 14),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 14),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 14)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV420P14BE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p14be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 14),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 14),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 14)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV420P16LE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV420P16BE] = new AVPixFmtDescriptor
			{
				Name = "yuv420p16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV422P9LE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p9le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV422P9BE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p9be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9)		// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV422P10LE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p10le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV422P10BE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p10be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV422P12LE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p12le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 12)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV422P12BE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p12be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 12)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV422P14LE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p14le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 14),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 14),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 14)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV422P14BE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p14be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 14),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 14),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 14)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV422P16LE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV422P16BE] = new AVPixFmtDescriptor
			{
				Name = "yuv422p16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV444P16LE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV444P16BE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 16)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV444P10LE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p10le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV444P10BE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p10be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 10)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV444P10MSBLE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p10msble".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 6, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 6, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 6, 10)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV444P10MSBBE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p10msbbe".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 6, 10),	// Y
					new AvComponentDescriptor(1, 2, 0, 6, 10),	// U
					new AvComponentDescriptor(2, 2, 0, 6, 10)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV444P9LE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p9le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9)		// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV444P9BE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p9be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 9),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 9)		// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV444P12LE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p12le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 12)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV444P12BE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p12be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 12)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV444P12MSBLE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p12msble".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 4, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 4, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 4, 12)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV444P12MSBBE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p12msbbe".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 4, 12),	// Y
					new AvComponentDescriptor(1, 2, 0, 4, 12),	// U
					new AvComponentDescriptor(2, 2, 0, 4, 12)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.YUV444P14LE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p14le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 14),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 14),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 14)	// V
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.YUV444P14BE] = new AVPixFmtDescriptor
			{
				Name = "yuv444p14be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 14),	// Y
					new AvComponentDescriptor(1, 2, 0, 0, 14),	// U
					new AvComponentDescriptor(2, 2, 0, 0, 14)	// V
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.D3D11VA_VLD] = new AVPixFmtDescriptor
			{
				Name = "d3d11va_vld".ToCharPointer(),
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 0, 0, 0, 0)
				],
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.DXVA2_VLD] = new AVPixFmtDescriptor
			{
				Name = "dxva2_vld".ToCharPointer(),
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 0, 0, 0, 0)
				],
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.YA8] = new AVPixFmtDescriptor
			{
				Name = "ya8".ToCharPointer(),
				Nb_Components = 2,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 8),	// Y
					new AvComponentDescriptor(0, 2, 1, 0, 8)		// A
				],
				Flags = AvPixelFormatFlag.Alpha,
				Alias = "gray8a".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.YA16LE] = new AVPixFmtDescriptor
			{
				Name = "ya16le".ToCharPointer(),
				Nb_Components = 2,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 16),	// Y
					new AvComponentDescriptor(0, 4, 2, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YA16BE] = new AVPixFmtDescriptor
			{
				Name = "ya16be".ToCharPointer(),
				Nb_Components = 2,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 16),	// Y
					new AvComponentDescriptor(0, 4, 2, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.VIDEOTOOLBOX] = new AVPixFmtDescriptor
			{
				Name = "videotoolbox_vld".ToCharPointer(),
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 0, 0, 0, 0)
				],
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.GBRP] = new AVPixFmtDescriptor
			{
				Name = "gbrp".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 1, 0, 0, 8),	// R
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// G
					new AvComponentDescriptor(1, 1, 0, 0, 8)		// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRP9LE] = new AVPixFmtDescriptor
			{
				Name = "gbrp9le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 9),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 9)		// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRP9BE] = new AVPixFmtDescriptor
			{
				Name = "gbrp9be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 9),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 9),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 9)		// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.GBRP10LE] = new AVPixFmtDescriptor
			{
				Name = "gbrp10le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 10),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 10)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRP10BE] = new AVPixFmtDescriptor
			{
				Name = "gbrp10be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 10),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 10),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 10)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.GBRP10MSBLE] = new AVPixFmtDescriptor
			{
				Name = "gbrp10msble".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 6, 10),	// R
					new AvComponentDescriptor(0, 2, 0, 6, 10),	// G
					new AvComponentDescriptor(1, 2, 0, 6, 10)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRP10MSBBE] = new AVPixFmtDescriptor
			{
				Name = "gbrp10msbbe".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 6, 10),	// R
					new AvComponentDescriptor(0, 2, 0, 6, 10),	// G
					new AvComponentDescriptor(1, 2, 0, 6, 10)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.GBRP12LE] = new AVPixFmtDescriptor
			{
				Name = "gbrp12le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 12),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 12)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRP12BE] = new AVPixFmtDescriptor
			{
				Name = "gbrp12be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 12),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 12),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 12)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.GBRP12MSBLE] = new AVPixFmtDescriptor
			{
				Name = "gbrp12msble".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 4, 12),	// R
					new AvComponentDescriptor(0, 2, 0, 4, 12),	// G
					new AvComponentDescriptor(1, 2, 0, 4, 12)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRP12MSBBE] = new AVPixFmtDescriptor
			{
				Name = "gbrp12msbbe".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 4, 12),	// R
					new AvComponentDescriptor(0, 2, 0, 4, 12),	// G
					new AvComponentDescriptor(1, 2, 0, 4, 12)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.GBRP14LE] = new AVPixFmtDescriptor
			{
				Name = "gbrp14le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 14),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 14),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 14)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRP14BE] = new AVPixFmtDescriptor
			{
				Name = "gbrp14be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 14),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 14),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 14)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.GBRP16LE] = new AVPixFmtDescriptor
			{
				Name = "gbrp16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 16)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRP16BE] = new AVPixFmtDescriptor
			{
				Name = "gbrp16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 16)	// B
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.GBRAP] = new AVPixFmtDescriptor
			{
				Name = "gbrap".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 1, 0, 0, 8),	// R
					new AvComponentDescriptor(0, 1, 0, 0, 8),	// G
					new AvComponentDescriptor(1, 1, 0, 0, 8),	// B
					new AvComponentDescriptor(3, 1, 0, 0, 8)		// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.GBRAP16LE] = new AVPixFmtDescriptor
			{
				Name = "gbrap16le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// B
					new AvComponentDescriptor(3, 2, 0, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.GBRAP16BE] = new AVPixFmtDescriptor
			{
				Name = "gbrap16be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 16),	// R
					new AvComponentDescriptor(0, 2, 0, 0, 16),	// G
					new AvComponentDescriptor(1, 2, 0, 0, 16),	// B
					new AvComponentDescriptor(3, 2, 0, 0, 16)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.GBRAP32LE] = new AVPixFmtDescriptor
			{
				Name = "gbrap32le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 4, 0, 0, 32),	// R
					new AvComponentDescriptor(0, 4, 0, 0, 32),	// G
					new AvComponentDescriptor(1, 4, 0, 0, 32),	// B
					new AvComponentDescriptor(3, 4, 0, 0, 32)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.GBRAP32BE] = new AVPixFmtDescriptor
			{
				Name = "gbrap32be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 4, 0, 0, 32),	// R
					new AvComponentDescriptor(0, 4, 0, 0, 32),	// G
					new AvComponentDescriptor(1, 4, 0, 0, 32),	// B
					new AvComponentDescriptor(3, 4, 0, 0, 32)	// A
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.VDPAU] = new AVPixFmtDescriptor
			{
				Name = "vdpau".ToCharPointer(),
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 0, 0, 0, 0)
				],
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.XYZ12LE] = new AVPixFmtDescriptor
			{
				Name = "xyz12le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 6, 0, 4, 12),	// X
					new AvComponentDescriptor(0, 6, 2, 4, 12),	// Y
					new AvComponentDescriptor(0, 6, 4, 4, 12)	// Z
				],
				Flags = AvPixelFormatFlag.Xyz
			};
			arr[(c_int)AvPixelFormat.XYZ12BE] = new AVPixFmtDescriptor
			{
				Name = "xyz12be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 6, 0, 4, 12),	// X
					new AvComponentDescriptor(0, 6, 2, 4, 12),	// Y
					new AvComponentDescriptor(0, 6, 4, 4, 12)	// Z
				],
				Flags = AvPixelFormatFlag.Xyz | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.BAYER_BGGR8] = new AVPixFmtDescriptor
			{
				Name = "bayer_bggr8".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 2),
					new AvComponentDescriptor(0, 1, 0, 0, 4),
					new AvComponentDescriptor(0, 1, 0, 0, 2)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_RGGB8] = new AVPixFmtDescriptor
			{
				Name = "bayer_rggb8".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 2),
					new AvComponentDescriptor(0, 1, 0, 0, 4),
					new AvComponentDescriptor(0, 1, 0, 0, 2)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_GBRG8] = new AVPixFmtDescriptor
			{
				Name = "bayer_gbrg8".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 2),
					new AvComponentDescriptor(0, 1, 0, 0, 4),
					new AvComponentDescriptor(0, 1, 0, 0, 2)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_GRBG8] = new AVPixFmtDescriptor
			{
				Name = "bayer_grbg8".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 2),
					new AvComponentDescriptor(0, 1, 0, 0, 4),
					new AvComponentDescriptor(0, 1, 0, 0, 2)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_BGGR16LE] = new AVPixFmtDescriptor
			{
				Name = "bayer_bggr16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 4),
					new AvComponentDescriptor(0, 2, 0, 0, 8),
					new AvComponentDescriptor(0, 2, 0, 0, 4)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_BGGR16BE] = new AVPixFmtDescriptor
			{
				Name = "bayer_bggr16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 4),
					new AvComponentDescriptor(0, 2, 0, 0, 8),
					new AvComponentDescriptor(0, 2, 0, 0, 4)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_RGGB16LE] = new AVPixFmtDescriptor
			{
				Name = "bayer_rggb16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 4),
					new AvComponentDescriptor(0, 2, 0, 0, 8),
					new AvComponentDescriptor(0, 2, 0, 0, 4)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_RGGB16BE] = new AVPixFmtDescriptor
			{
				Name = "bayer_rggb16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 4),
					new AvComponentDescriptor(0, 2, 0, 0, 8),
					new AvComponentDescriptor(0, 2, 0, 0, 4)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_GBRG16LE] = new AVPixFmtDescriptor
			{
				Name = "bayer_gbrg16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 4),
					new AvComponentDescriptor(0, 2, 0, 0, 8),
					new AvComponentDescriptor(0, 2, 0, 0, 4)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_GBRG16BE] = new AVPixFmtDescriptor
			{
				Name = "bayer_gbrg16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 4),
					new AvComponentDescriptor(0, 2, 0, 0, 8),
					new AvComponentDescriptor(0, 2, 0, 0, 4)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_GRBG16LE] = new AVPixFmtDescriptor
			{
				Name = "bayer_grbg16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 4),
					new AvComponentDescriptor(0, 2, 0, 0, 8),
					new AvComponentDescriptor(0, 2, 0, 0, 4)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.BAYER_GRBG16BE] = new AVPixFmtDescriptor
			{
				Name = "bayer_grbg16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 4),
					new AvComponentDescriptor(0, 2, 0, 0, 8),
					new AvComponentDescriptor(0, 2, 0, 0, 4)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Bayer
			};
			arr[(c_int)AvPixelFormat.NV16] = new AVPixFmtDescriptor
			{
				Name = "nv16".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),
					new AvComponentDescriptor(1, 2, 0, 0, 8),
					new AvComponentDescriptor(1, 2, 1, 0, 8)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.NV20LE] = new AVPixFmtDescriptor
			{
				Name = "nv20le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),
					new AvComponentDescriptor(1, 4, 0, 0, 10),
					new AvComponentDescriptor(1, 4, 2, 0, 10)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.NV20BE] = new AVPixFmtDescriptor
			{
				Name = "nv20be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 10),
					new AvComponentDescriptor(1, 4, 0, 0, 10),
					new AvComponentDescriptor(1, 4, 2, 0, 10)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.QSV] = new AVPixFmtDescriptor
			{
				Name = "qsv".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.MEDIACODEC] = new AVPixFmtDescriptor
			{
				Name = "mediacodec".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.MMAL] = new AVPixFmtDescriptor
			{
				Name = "mmal".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.CUDA] = new AVPixFmtDescriptor
			{
				Name = "cuda".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.AMF_SURFACE] = new AVPixFmtDescriptor
			{
				Name = "amf".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.VYU444] = new AVPixFmtDescriptor
			{
				Name = "vyu444".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 3, 1, 0, 8),
					new AvComponentDescriptor(0, 3, 2, 0, 8),
					new AvComponentDescriptor(0, 3, 0, 0, 8)
				]
			};
			arr[(c_int)AvPixelFormat.UYVA] = new AVPixFmtDescriptor
			{
				Name = "uyva".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 1, 0, 8),
					new AvComponentDescriptor(0, 4, 0, 0, 8),
					new AvComponentDescriptor(0, 4, 2, 0, 8),
					new AvComponentDescriptor(0, 4, 3, 0, 8)
				],
				Flags = AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.AYUV] = new AVPixFmtDescriptor
			{
				Name = "ayuv".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 1, 0, 8),
					new AvComponentDescriptor(0, 4, 2, 0, 8),
					new AvComponentDescriptor(0, 4, 3, 0, 8),
					new AvComponentDescriptor(0, 4, 0, 0, 8)
				],
				Flags = AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.AYUV64LE] = new AVPixFmtDescriptor
			{
				Name = "ayuv64le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 2, 0, 16),
					new AvComponentDescriptor(0, 8, 4, 0, 16),
					new AvComponentDescriptor(0, 8, 6, 0, 16),
					new AvComponentDescriptor(0, 8, 0, 0, 16)
				],
				Flags = AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.AYUV64BE] = new AVPixFmtDescriptor
			{
				Name = "ayuv64be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 2, 0, 16),
					new AvComponentDescriptor(0, 8, 4, 0, 16),
					new AvComponentDescriptor(0, 8, 6, 0, 16),
					new AvComponentDescriptor(0, 8, 0, 0, 16)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.P010LE] = new AVPixFmtDescriptor
			{
				Name = "p010le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 2, 6, 10)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.P010BE] = new AVPixFmtDescriptor
			{
				Name = "p010be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 2, 6, 10)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.P012LE] = new AVPixFmtDescriptor
			{
				Name = "p012le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 2, 4, 12)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.P012BE] = new AVPixFmtDescriptor
			{
				Name = "p012be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 2, 4, 12)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.P016LE] = new AVPixFmtDescriptor
			{
				Name = "p016le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 2, 0, 16)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.P016BE] = new AVPixFmtDescriptor
			{
				Name = "p016be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 1,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 2, 0, 16)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.GBRAP14LE] = new AVPixFmtDescriptor
			{
				Name = "gbrap14le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 14),
					new AvComponentDescriptor(0, 2, 0, 0, 14),
					new AvComponentDescriptor(1, 2, 0, 0, 14),
					new AvComponentDescriptor(3, 2, 0, 0, 14)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.GBRAP14BE] = new AVPixFmtDescriptor
			{
				Name = "gbrap14be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 14),
					new AvComponentDescriptor(0, 2, 0, 0, 14),
					new AvComponentDescriptor(1, 2, 0, 0, 14),
					new AvComponentDescriptor(3, 2, 0, 0, 14)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.GBRAP12LE] = new AVPixFmtDescriptor
			{
				Name = "gbrap12le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 12),
					new AvComponentDescriptor(0, 2, 0, 0, 12),
					new AvComponentDescriptor(1, 2, 0, 0, 12),
					new AvComponentDescriptor(3, 2, 0, 0, 12)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.GBRAP12BE] = new AVPixFmtDescriptor
			{
				Name = "gbrap12be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 12),
					new AvComponentDescriptor(0, 2, 0, 0, 12),
					new AvComponentDescriptor(1, 2, 0, 0, 12),
					new AvComponentDescriptor(3, 2, 0, 0, 12)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.GBRAP10LE] = new AVPixFmtDescriptor
			{
				Name = "gbrap10le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 10),
					new AvComponentDescriptor(0, 2, 0, 0, 10),
					new AvComponentDescriptor(1, 2, 0, 0, 10),
					new AvComponentDescriptor(3, 2, 0, 0, 10)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.GBRAP10BE] = new AVPixFmtDescriptor
			{
				Name = "gbrap10be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 10),
					new AvComponentDescriptor(0, 2, 0, 0, 10),
					new AvComponentDescriptor(1, 2, 0, 0, 10),
					new AvComponentDescriptor(3, 2, 0, 0, 10)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.D3D11] = new AVPixFmtDescriptor
			{
				Name = "d3d11".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.D3D12] = new AVPixFmtDescriptor
			{
				Name = "d3d12".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.GBRPF32BE] = new AVPixFmtDescriptor
			{
				Name = "gbrpf32be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 4, 0, 0, 32),
					new AvComponentDescriptor(0, 4, 0, 0, 32),
					new AvComponentDescriptor(1, 4, 0, 0, 32)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.GBRPF32LE] = new AVPixFmtDescriptor
			{
				Name = "gbrpf32le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 4, 0, 0, 32),
					new AvComponentDescriptor(0, 4, 0, 0, 32),
					new AvComponentDescriptor(1, 4, 0, 0, 32)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Float | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRAPF32BE] = new AVPixFmtDescriptor
			{
				Name = "gbrapf32be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 4, 0, 0, 32),
					new AvComponentDescriptor(0, 4, 0, 0, 32),
					new AvComponentDescriptor(1, 4, 0, 0, 32),
					new AvComponentDescriptor(3, 4, 0, 0, 32)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.GBRAPF32LE] = new AVPixFmtDescriptor
			{
				Name = "gbrapf32le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 4, 0, 0, 32),
					new AvComponentDescriptor(0, 4, 0, 0, 32),
					new AvComponentDescriptor(1, 4, 0, 0, 32),
					new AvComponentDescriptor(3, 4, 0, 0, 32)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.GBRPF16BE] = new AVPixFmtDescriptor
			{
				Name = "gbrpf16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 16),
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 2, 0, 0, 16)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.GBRPF16LE] = new AVPixFmtDescriptor
			{
				Name = "gbrpf16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 16),
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 2, 0, 0, 16)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Float | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.GBRAPF16BE] = new AVPixFmtDescriptor
			{
				Name = "gbrapf16be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 16),
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 2, 0, 0, 16),
					new AvComponentDescriptor(3, 2, 0, 0, 16)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.GBRAPF16LE] = new AVPixFmtDescriptor
			{
				Name = "gbrapf16le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(2, 2, 0, 0, 16),
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 2, 0, 0, 16),
					new AvComponentDescriptor(3, 2, 0, 0, 16)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.DRM_PRIME] = new AVPixFmtDescriptor
			{
				Name = "drm_prime".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.OPENCL] = new AVPixFmtDescriptor
			{
				Name  = "opencl".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.GRAYF32BE] = new AVPixFmtDescriptor
			{
				Name = "grayf32be".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 32)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Float,
				Alias = "yf32be".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAYF32LE] = new AVPixFmtDescriptor
			{
				Name = "grayf32le".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 32)
				],
				Flags = AvPixelFormatFlag.Float,
				Alias = "yf32le".ToCharPointer()
			};
			arr[(c_int)AvPixelFormat.GRAYF16BE] = new AVPixFmtDescriptor
			{
				Name = "grayf16be".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.GRAYF16LE] = new AVPixFmtDescriptor
			{
				Name = "grayf16le".ToCharPointer(),
				Nb_Components = 1,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16)
				],
				Flags = AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.YAF32BE] = new AVPixFmtDescriptor
			{
				Name = "yaf32be".ToCharPointer(),
				Nb_Components = 2,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 0, 0, 32),
					new AvComponentDescriptor(0, 8, 4, 0, 32)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Float | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YAF32LE] = new AVPixFmtDescriptor
			{
				Name = "yaf32le".ToCharPointer(),
				Nb_Components = 2,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 0, 0, 32),
					new AvComponentDescriptor(0, 8, 4, 0, 32)
				],
				Flags = AvPixelFormatFlag.Float | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YAF16BE] = new AVPixFmtDescriptor
			{
				Name = "yaf16be".ToCharPointer(),
				Nb_Components = 2,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 16),
					new AvComponentDescriptor(0, 4, 2, 0, 16)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Float | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YAF16LE] = new AVPixFmtDescriptor
			{
				Name = "yaf16le".ToCharPointer(),
				Nb_Components = 2,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 16),
					new AvComponentDescriptor(0, 4, 2, 0, 16)
				],
				Flags = AvPixelFormatFlag.Float | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA422P12BE] = new AVPixFmtDescriptor
			{
				Name = "yuva422p12be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),
					new AvComponentDescriptor(1, 2, 0, 0, 12),
					new AvComponentDescriptor(2, 2, 0, 0, 12),
					new AvComponentDescriptor(3, 2, 0, 0, 12)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA422P12LE] = new AVPixFmtDescriptor
			{
				Name = "yuva422p12le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),
					new AvComponentDescriptor(1, 2, 0, 0, 12),
					new AvComponentDescriptor(2, 2, 0, 0, 12),
					new AvComponentDescriptor(3, 2, 0, 0, 12)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA444P12BE] = new AVPixFmtDescriptor
			{
				Name = "yuva444p12be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),
					new AvComponentDescriptor(1, 2, 0, 0, 12),
					new AvComponentDescriptor(2, 2, 0, 0, 12),
					new AvComponentDescriptor(3, 2, 0, 0, 12)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.YUVA444P12LE] = new AVPixFmtDescriptor
			{
				Name = "yuva444p12le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 12),
					new AvComponentDescriptor(1, 2, 0, 0, 12),
					new AvComponentDescriptor(2, 2, 0, 0, 12),
					new AvComponentDescriptor(3, 2, 0, 0, 12)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.NV24] = new AVPixFmtDescriptor
			{
				Name = "nv24".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),
					new AvComponentDescriptor(1, 2, 0, 0, 8),
					new AvComponentDescriptor(1, 2, 1, 0, 8)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.NV42] = new AVPixFmtDescriptor
			{
				Name = "nv42".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 1, 0, 0, 8),
					new AvComponentDescriptor(1, 2, 1, 0, 8),
					new AvComponentDescriptor(1, 2, 0, 0, 8)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.VULKAN] = new AVPixFmtDescriptor
			{
				Name = "vulkan".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};
			arr[(c_int)AvPixelFormat.P210BE] = new AVPixFmtDescriptor
			{
				Name = "p210be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 2, 6, 10)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.P210LE] = new AVPixFmtDescriptor
			{
				Name = "p210le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 2, 6, 10)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.P410BE] = new AVPixFmtDescriptor
			{
				Name = "p410be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 2, 6, 10)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.P410LE] = new AVPixFmtDescriptor
			{
				Name = "p410le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 0, 6, 10),
					new AvComponentDescriptor(1, 4, 2, 6, 10)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.P216BE] = new AVPixFmtDescriptor
			{
				Name = "p216be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 2, 0, 16)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.P216LE] = new AVPixFmtDescriptor
			{
				Name = "p216le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 2, 0, 16)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.P416BE] = new AVPixFmtDescriptor
			{
				Name = "p416be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 2, 0, 16)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.P416LE] = new AVPixFmtDescriptor
			{
				Name = "p416le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 0, 0, 16),
					new AvComponentDescriptor(1, 4, 2, 0, 16)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.VUYA] = new AVPixFmtDescriptor
			{
				Name = "vuya".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 2, 0, 8),
					new AvComponentDescriptor(0, 4, 1, 0, 8),
					new AvComponentDescriptor(0, 4, 0, 0, 8),
					new AvComponentDescriptor(0, 4, 3, 0, 8)
				],
				Flags = AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.VUYX] = new AVPixFmtDescriptor
			{
				Name = "vuyx".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 2, 0, 8),
					new AvComponentDescriptor(0, 4, 1, 0, 8),
					new AvComponentDescriptor(0, 4, 0, 0, 8),
					new AvComponentDescriptor(0, 4, 3, 0, 8)
				]
			};
			arr[(c_int)AvPixelFormat.RGBF16BE] = new AVPixFmtDescriptor
			{
				Name = "rgbf16be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 6, 0, 0, 16),
					new AvComponentDescriptor(0, 6, 2, 0, 16),
					new AvComponentDescriptor(0, 6, 4, 0, 16)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.RGBF16LE] = new AVPixFmtDescriptor
			{
				Name = "rgbf16le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 6, 0, 0, 16),
					new AvComponentDescriptor(0, 6, 2, 0, 16),
					new AvComponentDescriptor(0, 6, 4, 0, 16)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.RGBAF16BE] = new AVPixFmtDescriptor
			{
				Name = "rgbaf16be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 0, 0, 16),
					new AvComponentDescriptor(0, 8, 2, 0, 16),
					new AvComponentDescriptor(0, 8, 4, 0, 16),
					new AvComponentDescriptor(0, 8, 6, 0, 16)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.RGBAF16LE] = new AVPixFmtDescriptor
			{
				Name = "rgbaf16le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 0, 0, 16),
					new AvComponentDescriptor(0, 8, 2, 0, 16),
					new AvComponentDescriptor(0, 8, 4, 0, 16),
					new AvComponentDescriptor(0, 8, 6, 0, 16)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.Y212LE] = new AVPixFmtDescriptor
			{
				Name = "y212le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 4, 12),
					new AvComponentDescriptor(0, 8, 2, 4, 12),
					new AvComponentDescriptor(0, 8, 6, 4, 12)
				]
			};
			arr[(c_int)AvPixelFormat.Y212BE] = new AVPixFmtDescriptor
			{
				Name = "y212be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 4, 12),
					new AvComponentDescriptor(0, 8, 2, 4, 12),
					new AvComponentDescriptor(0, 8, 6, 4, 12)
				],
				Flags = AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.XV30LE] = new AVPixFmtDescriptor
			{
				Name = "xv30le".ToCharPointer(),
				Nb_Components= 3,
				Log2_Chroma_W= 0,
				Log2_Chroma_H= 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 1, 2, 10),
					new AvComponentDescriptor(0, 4, 0, 0, 10),
					new AvComponentDescriptor(0, 4, 2, 4, 10),
					new AvComponentDescriptor(0, 4, 3, 6, 2)
				]
			};
			arr[(c_int)AvPixelFormat.XV30BE] = new AVPixFmtDescriptor
			{
				Name = "xv30be".ToCharPointer(),
				Nb_Components= 3,
				Log2_Chroma_W= 0,
				Log2_Chroma_H= 0,
				Comp =
				[
					new AvComponentDescriptor(0, 32, 10, 0, 10),
					new AvComponentDescriptor(0, 32,  0, 0, 10),
					new AvComponentDescriptor(0, 32, 20, 0, 10),
					new AvComponentDescriptor(0, 32, 30, 0,  2)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Bitstream
			};
			arr[(c_int)AvPixelFormat.XV36LE] = new AVPixFmtDescriptor
			{
				Name = "xv36le".ToCharPointer(),
				Nb_Components= 3,
				Log2_Chroma_W= 0,
				Log2_Chroma_H= 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 2, 4, 12),
					new AvComponentDescriptor(0, 8, 0, 4, 12),
					new AvComponentDescriptor(0, 8, 4, 4, 12),
					new AvComponentDescriptor(0, 8, 6, 4, 12)
				]
			};
			arr[(c_int)AvPixelFormat.XV36BE] = new AVPixFmtDescriptor
			{
				Name = "xv36be".ToCharPointer(),
				Nb_Components= 3,
				Log2_Chroma_W= 0,
				Log2_Chroma_H= 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 2, 4, 12),
					new AvComponentDescriptor(0, 8, 0, 4, 12),
					new AvComponentDescriptor(0, 8, 4, 4, 12),
					new AvComponentDescriptor(0, 8, 6, 4, 12)
				],
				Flags = AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.XV48LE] = new AVPixFmtDescriptor
			{
				Name = "xv48le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 2, 0, 16),
					new AvComponentDescriptor(0, 8, 0, 0, 16),
					new AvComponentDescriptor(0, 8, 4, 0, 16),
					new AvComponentDescriptor(0, 8, 6, 0, 16)
				]
			};
			arr[(c_int)AvPixelFormat.XV48BE] = new AVPixFmtDescriptor
			{
				Name = "xv48be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 8, 2, 0, 16),
					new AvComponentDescriptor(0, 8, 0, 0, 16),
					new AvComponentDescriptor(0, 8, 4, 0, 16),
					new AvComponentDescriptor(0, 8, 6, 0, 16)
				],
				Flags = AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.V30XLE] = new AVPixFmtDescriptor
			{
				Name = "v30xle".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 1, 4, 10),
					new AvComponentDescriptor(0, 4, 0, 2, 10),
					new AvComponentDescriptor(0, 4, 2, 6, 10),
					new AvComponentDescriptor(0, 4, 0, 0, 2)
				]
			};
			arr[(c_int)AvPixelFormat.V30XBE] = new AVPixFmtDescriptor
			{
				Name = "v30xbe".ToCharPointer(),
				Nb_Components= 3,
				Log2_Chroma_W= 0,
				Log2_Chroma_H= 0,
				Comp =
				[
					new AvComponentDescriptor(0, 32, 12, 0, 10),
					new AvComponentDescriptor(0, 32,  2, 0, 10),
					new AvComponentDescriptor(0, 32, 22, 0, 10),
					new AvComponentDescriptor(0, 32,  0, 0,  2)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Bitstream
			};
			arr[(c_int)AvPixelFormat.RGBF32BE] = new AVPixFmtDescriptor
			{
				Name = "rgbf32be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 12, 0, 0, 32),
					new AvComponentDescriptor(0, 12, 4, 0, 32),
					new AvComponentDescriptor(0, 12, 8, 0, 32)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.RGBF32LE] = new AVPixFmtDescriptor
			{
				Name = "rgbf32le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 12, 0, 0, 32),
					new AvComponentDescriptor(0, 12, 4, 0, 32),
					new AvComponentDescriptor(0, 12, 8, 0, 32)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float
			};
			arr[(c_int)AvPixelFormat.RGB96BE] = new AVPixFmtDescriptor
			{
				Name = "rgb96be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 12, 0, 0, 32),
					new AvComponentDescriptor(0, 12, 4, 0, 32),
					new AvComponentDescriptor(0, 12, 8, 0, 32)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.RGB96LE] = new AVPixFmtDescriptor
			{
				Name = "rgb96le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 12, 0, 0, 32),
					new AvComponentDescriptor(0, 12, 4, 0, 32),
					new AvComponentDescriptor(0, 12, 8, 0, 32)
				],
				Flags = AvPixelFormatFlag.Rgb
			};
			arr[(c_int)AvPixelFormat.RGBAF32BE] = new AVPixFmtDescriptor
			{
				Name = "rgbaf32be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 16,  0, 0, 32),
					new AvComponentDescriptor(0, 16,  4, 0, 32),
					new AvComponentDescriptor(0, 16,  8, 0, 32),
					new AvComponentDescriptor(0, 16, 12, 0, 32)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Float | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.RGBAF32LE] = new AVPixFmtDescriptor
			{
				Name = "rgbaf32le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 16,  0, 0, 32),
					new AvComponentDescriptor(0, 16,  4, 0, 32),
					new AvComponentDescriptor(0, 16,  8, 0, 32),
					new AvComponentDescriptor(0, 16, 12, 0, 32)
				],
				Flags = AvPixelFormatFlag.Rgb| AvPixelFormatFlag.Float | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.RGBA128BE] = new AVPixFmtDescriptor
			{
				Name = "rgba128be".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 16,  0, 0, 32),
					new AvComponentDescriptor(0, 16,  4, 0, 32),
					new AvComponentDescriptor(0, 16,  8, 0, 32),
					new AvComponentDescriptor(0, 16, 12, 0, 32)
				],
				Flags = AvPixelFormatFlag.Be | AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.RGBA128LE] = new AVPixFmtDescriptor
			{
				Name = "rgba128le".ToCharPointer(),
				Nb_Components = 4,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 16,  0, 0, 32),
					new AvComponentDescriptor(0, 16,  4, 0, 32),
					new AvComponentDescriptor(0, 16,  8, 0, 32),
					new AvComponentDescriptor(0, 16, 12, 0, 32)
				],
				Flags = AvPixelFormatFlag.Rgb | AvPixelFormatFlag.Alpha
			};
			arr[(c_int)AvPixelFormat.P212BE] = new AVPixFmtDescriptor
			{
				Name = "p212be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 2, 4, 12)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.P212LE] = new AVPixFmtDescriptor
			{
				Name = "p212le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 2, 4, 12)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.P412BE] = new AVPixFmtDescriptor
			{
				Name = "p412be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 2, 4, 12)
				],
				Flags = AvPixelFormatFlag.Planar | AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.P412LE] = new AVPixFmtDescriptor
			{
				Name = "p412le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 0,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 2, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 0, 4, 12),
					new AvComponentDescriptor(1, 4, 2, 4, 12)
				],
				Flags = AvPixelFormatFlag.Planar
			};
			arr[(c_int)AvPixelFormat.Y216LE] = new AVPixFmtDescriptor
			{
				Name = "y216le".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 16),
					new AvComponentDescriptor(0, 8, 2, 0, 16),
					new AvComponentDescriptor(0, 8, 6, 0, 16)
				]
			};
			arr[(c_int)AvPixelFormat.Y216BE] = new AVPixFmtDescriptor
			{
				Name = "y216be".ToCharPointer(),
				Nb_Components = 3,
				Log2_Chroma_W = 1,
				Log2_Chroma_H = 0,
				Comp =
				[
					new AvComponentDescriptor(0, 4, 0, 0, 16),
					new AvComponentDescriptor(0, 8, 2, 0, 16),
					new AvComponentDescriptor(0, 8, 6, 0, 16)
				],
				Flags = AvPixelFormatFlag.Be
			};
			arr[(c_int)AvPixelFormat.OHCODEC] = new AVPixFmtDescriptor
			{
				Name = "ohcodec".ToCharPointer(),
				Flags = AvPixelFormatFlag.HwAccel
			};

			return arr;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return the short name for a pixel format, NULL in case pix_fmt
		/// is unknown
		///
		/// See av_get_pix_fmt(), av_get_pix_fmt_string()
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Get_Pix_Fmt_Name(AvPixelFormat pix_Fmt)//XX 3367
		{
			return pix_Fmt < AvPixelFormat.Nb ? av_Pix_Fmt_Descriptors[(c_int)pix_Fmt].Name : null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the pixel format corresponding to name.
		///
		/// If there is no pixel format with name name, then looks for a
		/// pixel format with the name corresponding to the native endian
		/// format of name.
		/// For example in a little-endian system, first looks for "gray16",
		/// then for "gray16le".
		///
		/// Finally if no pixel format has been found, returns
		/// AV_PIX_FMT_NONE
		/// </summary>
		/********************************************************************/
		public static AvPixelFormat Av_Get_Pix_Fmt(CPointer<char> name)//XX 3379
		{
			if (CString.strcmp(name, "rgb32") == 0)
				name = X_NE("argb", "bgra");
			else if (CString.strcmp(name, "bgr32") == 0)
				name = X_NE("abgr", "rgba");

			AvPixelFormat pix_Fmt = Get_Pix_Fmt_Internal(name);
			if (pix_Fmt == AvPixelFormat.None)
			{
				CPointer<char> name2 = new CPointer<char>(32);

				CString.snprintf(name2, (size_t)name2.Length, "%s%s", name, X_NE("be", "le"));
				pix_Fmt = Get_Pix_Fmt_Internal(name2);
			}

			return pix_Fmt;
		}



		/********************************************************************/
		/// <summary>
		/// Return a pixel format descriptor for provided pixel format or
		/// NULL if this pixel format is unknown
		/// </summary>
		/********************************************************************/
		public static AVPixFmtDescriptor Av_Pix_Fmt_Desc_Get(AvPixelFormat pix_Fmt)//XX 3447
		{
			if ((pix_Fmt < 0) || (pix_Fmt >= AvPixelFormat.Nb))
				return null;

			return av_Pix_Fmt_Descriptors[(c_int)pix_Fmt];
		}



		/********************************************************************/
		/// <summary>
		/// Return number of planes in pix_fmt, a negative AVERROR if
		/// pix_fmt is not a valid pixel format
		/// </summary>
		/********************************************************************/
		public static c_int Av_Pix_Fmt_Count_Planes(AvPixelFormat pix_Fmt)//XX 3487
		{
			AVPixFmtDescriptor desc = Av_Pix_Fmt_Desc_Get(pix_Fmt);
			c_int[] planes = new c_int[4];

			if (desc == null)
				return Error.EINVAL;

			for (c_int i = 0; i < desc.Nb_Components; i++)
				planes[desc.Comp[i].Plane] = 1;

			c_int ret = 0;

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(planes); i++)
				ret += planes[i];

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name for provided color range or NULL if unknown
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Color_Range_Name(AvColorRange range)//XX 3763
		{
			return (c_uint)range < (c_uint)AvColorRange.Nb ? color_Range_Names[(c_int)range].ToCharPointer() : null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the AVColorPrimaries value for name or an AVError if not
		/// found
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Color_Primaries_Name(AvColorPrimaries primaries)//XX 3781
		{
			return (c_uint)primaries < (c_uint)AvColorPrimaries.Nb ? color_Primaries_Names[(c_int)primaries].ToCharPointer() : null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name for provided color transfer or NULL if unknown
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Color_Transfer_Name(AvColorTransferCharacteristic transfer)//XX 3802
		{
			return (c_uint)transfer < (c_uint)AvColorTransferCharacteristic.Nb ? color_Transfer_Names[(c_int)transfer].ToCharPointer() : null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name for provided color space or NULL if unknown
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Color_Space_Name(AvColorSpace space)//XX 3823
		{
			return (c_uint)space < (c_uint)AvColorSpace.Nb ? color_Space_Names[(c_int)space].ToCharPointer() : null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name for provided chroma location or NULL if unknown
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Chroma_Location_Name(AvChromaLocation location)//XX 3844
		{
			return (c_uint)location < (c_uint)AvChromaLocation.Nb ? chroma_Location_Names[(c_int)location].ToCharPointer() : null;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvPixelFormat Get_Pix_Fmt_Internal(CPointer<char> name)
		{
			for (c_int pix_Fmt = 0; pix_Fmt < (c_int)AvPixelFormat.Nb; pix_Fmt++)
			{
				if (av_Pix_Fmt_Descriptors[pix_Fmt].Name.IsNotNull && ((CString.strcmp(av_Pix_Fmt_Descriptors[pix_Fmt].Name, name) == 0) || (AvString.Av_Match_Name(name, av_Pix_Fmt_Descriptors[pix_Fmt].Alias) != 0)))
					return (AvPixelFormat)pix_Fmt;
			}

			return AvPixelFormat.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static CPointer<char> X_NE(string be, string le)//XX 3373
		{
			return BitConverter.IsLittleEndian ? le.ToCharPointer() : be.ToCharPointer();
		}
		#endregion
	}
}
