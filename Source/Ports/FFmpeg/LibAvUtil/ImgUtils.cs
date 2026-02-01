/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Misc image utilities
	/// </summary>
	public static class ImgUtils
	{
		/// <summary>
		/// 
		/// </summary>
		private class _ImgUtils : AvClass
		{
			/// <summary></summary>
			public AvClass Class => this;

			/// <summary></summary>
			public c_int Log_Offset;

			/// <summary></summary>
			public IClass Log_Ctx;
		}

		private static readonly AvClass imgUtils_Class = new AvClass
		{
			Class_Name = "IMGUTILS".ToCharPointer(),
			Item_Name = Log.Av_Default_Item_Name,
			Option = null,
			Version = Version.Version_Int,
			Log_Level_Offset_Name = nameof(_ImgUtils.Log_Offset),
			Parent_Log_Context_Name = nameof(_ImgUtils.Log_Ctx)
		};

		private delegate void CopyPlane_Delegate(CPointer<uint8_t> dst, ptrdiff_t dst_LineSize, CPointer<uint8_t> src, ptrdiff_t src_LineSize, ptrdiff_t byteWidth, c_int height);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_Image_Copy2(CPointer<CPointer<uint8_t>> dst_Data, CPointer<c_int> dst_LineSizes, CPointer<CPointer<uint8_t>> src_Data, CPointer<c_int> src_LineSizes, AvPixelFormat pix_Fmt, c_int width, c_int height)
		{
			Av_Image_Copy(dst_Data, dst_LineSizes, src_Data, src_LineSizes, pix_Fmt, width, height);
		}



		/********************************************************************/
		/// <summary>
		/// Compute the max pixel step for each plane of an image with a
		/// format described by pixdesc.
		///
		/// The pixel step is the distance in bytes between the first byte of
		/// the group of bytes which describe a pixel component and the first
		/// byte of the successive group in the same plane for the same
		/// component
		/// </summary>
		/********************************************************************/
		public static void Av_Image_Fill_Max_PixSteps(CPointer<c_int> max_PixSteps, CPointer<c_int> max_PixStep_Comps, AVPixFmtDescriptor pixDesc)//XX 35
		{
			CMemory.memset(max_PixSteps, 0, 4);

			if (max_PixStep_Comps.IsNotNull)
				CMemory.memset(max_PixStep_Comps, 0, 4);

			for (c_int i = 0; i < 4; i++)
			{
				AvComponentDescriptor comp = pixDesc.Comp[i];

				if (comp.Step > max_PixSteps[comp.Plane])
				{
					max_PixSteps[comp.Plane] = comp.Step;

					if (max_PixStep_Comps.IsNotNull)
						max_PixStep_Comps[comp.Plane] = i;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Compute the size of an image line with format pix_fmt and width
		/// width for the plane plane
		/// </summary>
		/********************************************************************/
		public static c_int Av_Image_Get_LineSize(AvPixelFormat pix_Fmt, c_int width, c_int plane)//XX 76
		{
			AVPixFmtDescriptor desc = PixDesc.Av_Pix_Fmt_Desc_Get(pix_Fmt);
			CPointer<c_int> max_Step = new CPointer<c_int>(4);		// Max pixel step for each plane
			CPointer<c_int> max_Step_Comp = new CPointer<c_int>(4);	// The component for each plane which has the max pixel step

			if ((desc == null) || ((desc.Flags & AvPixelFormatFlag.HwAccel) != 0))
				return Error.EINVAL;

			Av_Image_Fill_Max_PixSteps(max_Step, max_Step_Comp, desc);

			return Image_Get_LineSize(width, plane, max_Step[plane], max_Step_Comp[plane], desc);
		}



		/********************************************************************/
		/// <summary>
		/// Fill plane linesizes for an image with pixel format pix_fmt and
		/// width width
		/// </summary>
		/********************************************************************/
		public static c_int Av_Image_Fill_LineSizes(CPointer<c_int> lineSizes, AvPixelFormat pix_Fmt, c_int width)//XX 89
		{
			AVPixFmtDescriptor desc = PixDesc.Av_Pix_Fmt_Desc_Get(pix_Fmt);
			CPointer<c_int> max_Step = new CPointer<c_int>(4);		// Max pixel step for each plane
			CPointer<c_int> max_Step_Comp = new CPointer<c_int>(4);	// The component for each plane which has the max pixel step

			CMemory.memset(lineSizes, 0, 4);

			if ((desc == null) || ((desc.Flags & AvPixelFormatFlag.HwAccel) != 0))
				return Error.EINVAL;

			Av_Image_Fill_Max_PixSteps(max_Step, max_Step_Comp, desc);

			for (c_int i = 0; i < 4; i++)
			{
				c_int ret = Image_Get_LineSize(width, i, max_Step[i], max_Step_Comp[i], desc);

				if (ret < 0)
					return ret;

				lineSizes[i] = ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Fill plane sizes for an image with pixel format pix_fmt and
		/// height height
		/// </summary>
		/********************************************************************/
		public static c_int Av_Image_Fill_Plane_Sizes(CPointer<size_t> sizes, AvPixelFormat pix_Fmt, c_int height, CPointer<ptrdiff_t> lineSizes)//XX 111
		{
			c_int[] has_Plane = [ 0, 0, 0, 0 ];

			AVPixFmtDescriptor desc = PixDesc.Av_Pix_Fmt_Desc_Get(pix_Fmt);
			CMemory.memset<size_t>(sizes, 0, 4);

			if ((desc == null) || ((desc.Flags & AvPixelFormatFlag.HwAccel) != 0))
				return Error.EINVAL;

			if ((size_t)lineSizes[0] > (size_t.MaxValue / (size_t)height))
				return Error.EINVAL;

			sizes[0] = (size_t)lineSizes[0] * (size_t)height;

			if ((desc.Flags & AvPixelFormatFlag.Pal) != 0)
			{
				sizes[1] = 256 * 4;		// Palette is stored here as 256 32 bits words

				return 0;
			}

			for (c_int i = 0; i < 4; i++)
				has_Plane[desc.Comp[i].Plane] = 1;

			for (c_int i = 1; (i < 4) && (has_Plane[i] != 0); i++)
			{
				c_int s = (i == 1) || (i == 2) ? desc.Log2_Chroma_H : 0;
				c_int h = (height + (1 << s) - 1) >> s;

				if ((size_t)lineSizes[i] > (size_t.MaxValue / (size_t)h))
					return Error.EINVAL;

				sizes[i] = (size_t)h * (size_t)lineSizes[i];
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Fill plane data pointers for an image with pixel format pix_fmt
		/// and height height
		/// </summary>
		/********************************************************************/
		public static c_int Av_Image_Fill_Pointers(CPointer<CPointer<uint8_t>> data, AvPixelFormat pix_Fmt, c_int height, CPointer<uint8_t> ptr, CPointer<c_int> lineSizes)//XX 145
		{
			CPointer<ptrdiff_t> lineSizes1 = new CPointer<ptrdiff_t>(4);
			CPointer<size_t> sizes = new CPointer<size_t>(4);

			CMemory.memset(data, null, 4);

			for (c_int i = 0; i < 4; i++)
				lineSizes1[i] = lineSizes[i];

			c_int ret = Av_Image_Fill_Plane_Sizes(sizes, pix_Fmt, height, lineSizes1);

			if (ret < 0)
				return ret;

			ret = 0;

			for (c_int i = 0; i < 4; i++)
			{
				if (sizes[i] > (size_t)(c_int.MaxValue - ret))
					return Error.EINVAL;

				ret += (c_int)sizes[i];
			}

			if (ptr.IsNull)
				return ret;

			data[0] = ptr;

			for (c_int i = 1; (i < 4) && (sizes[i] != 0); i++)
				data[i] = data[i - 1] + sizes[i - 1];

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the given dimension of an image is valid, meaning that
		/// all bytes of a plane of an image with the specified pix_fmt can
		/// be addressed with a signed int
		/// </summary>
		/********************************************************************/
		public static c_int Av_Image_Check_Size2(c_uint w, c_uint h, int64_t max_Pixels, AvPixelFormat pix_Fmt, c_int log_Offset, IClass log_Ctx)//XX 289
		{
			_ImgUtils imgUtils = new _ImgUtils
			{
				Log_Offset = log_Offset,
				Log_Ctx = log_Ctx
			};
			imgUtils_Class.CopyTo(imgUtils.Class);

			int64_t stride = Av_Image_Get_LineSize(pix_Fmt, (c_int)w, 0);

			if (stride <= 0)
				stride = 8L * w;

			stride += 128 * 8;

			if ((w == 0) || (h == 0) || (w > int32_t.MaxValue) || (h > int32_t.MaxValue) || (stride >= c_int.MaxValue) || ((stride * (h + 128)) >= c_int.MaxValue))
			{
				Log.Av_Log(imgUtils, Log.Av_Log_Error, "Picture size %ux%u is invalid\n", w, h);

				return Error.EINVAL;
			}

			if (max_Pixels < int64_t.MaxValue)
			{
				if ((w * (int64_t)h) > max_Pixels)
				{
					Log.Av_Log(imgUtils, Log.Av_Log_Error, "Picture size %ux%u exceeds specified max pixel count %lld, see the documentation if you wish to increase it\n", w, h, max_Pixels);

					return Error.EINVAL;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the given dimension of an image is valid, meaning that
		/// all bytes of the image can be addressed with a signed int
		/// </summary>
		/********************************************************************/
		public static c_int Av_Image_Check_Size(c_uint w, c_uint h, c_int log_Offset, IClass log_Ctx)//XX 318
		{
			return Av_Image_Check_Size2(w, h, int64_t.MaxValue, AvPixelFormat.None, log_Offset, log_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// Check if the given sample aspect ratio of an image is valid.
		///
		/// It is considered invalid if the denominator is 0 or if applying
		/// the ratio to the image size would make the smaller dimension less
		/// than 1. If the sar numerator is 0, it is considered unknown and
		/// will return as valid
		/// </summary>
		/********************************************************************/
		public static c_int Av_Image_Check_Sar(c_uint w, c_uint h, AvRational sar)//XX 323
		{
			int64_t scaled_Dim;

			if ((sar.Den <= 0) || (sar.Num <= 0))
				return Error.EINVAL;

			if ((sar.Num == 0) || (sar.Num == sar.Den))
				return 0;

			if (sar.Num < sar.Den)
				scaled_Dim = Mathematics.Av_Rescale_Rnd(w, sar.Num, sar.Den, AvRounding.Zero);
			else
				scaled_Dim = Mathematics.Av_Rescale_Rnd(h, sar.Den, sar.Num, AvRounding.Zero);

			if (scaled_Dim > 0)
				return 0;

			return Error.EINVAL;
		}



		/********************************************************************/
		/// <summary>
		/// Copy image in src_data to dst_data
		/// </summary>
		/********************************************************************/
		public static void Av_Image_Copy(CPointer<CPointer<uint8_t>> dst_Data, CPointer<c_int> dst_LineSizes, CPointer<CPointer<uint8_t>> src_Data, CPointer<c_int> src_LineSizes, AvPixelFormat pix_Fmt, c_int width, c_int height)
		{
			CPointer<ptrdiff_t> dst_LineSizes1 = new CPointer<ptrdiff_t>(4);
			CPointer<ptrdiff_t> src_LineSizes1 = new CPointer<ptrdiff_t>(4);

			for (c_int i = 0; i < 4; i++)
			{
				dst_LineSizes1[i] = dst_LineSizes[i];
				src_LineSizes1[i] = src_LineSizes[i];
			}

			Image_Copy(dst_Data, dst_LineSizes1, src_Data, src_LineSizes1, pix_Fmt, width, height, Image_Copy_Plane);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int Image_Get_LineSize(c_int width, c_int plane, c_int max_Step, c_int max_Step_Comp, AVPixFmtDescriptor desc)//XX 54
		{
			if (desc == null)
				return Error.EINVAL;

			if (width < 0)
				return Error.EINVAL;

			c_int s = (max_Step_Comp == 1) || (max_Step_Comp == 2) ? desc.Log2_Chroma_W : 0;
			c_int shifted_W = ((width + (1 << s) - 1)) >> s;

			if ((shifted_W != 0) && (max_Step > (c_int.MaxValue / shifted_W)))
				return Error.EINVAL;

			c_int lineSize = max_Step * shifted_W;

			if ((desc.Flags & AvPixelFormatFlag.Bitstream) != 0)
				lineSize = (lineSize + 7) >> 3;

			return lineSize;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Image_Copy_Plane(CPointer<uint8_t> dst, ptrdiff_t dst_LineSize, CPointer<uint8_t> src, ptrdiff_t src_LineSize, ptrdiff_t byteWidth, c_int height)//XX 344
		{
			if (dst.IsNull || src.IsNull)
				return;

			for (; height > 0; height--)
			{
				CMemory.memcpy(dst, src, (size_t)byteWidth);

				dst += dst_LineSize;
				src += src_LineSize;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Image_Copy(CPointer<CPointer<uint8_t>> dst_Data, CPointer<ptrdiff_t> dst_LineSizes, CPointer<CPointer<uint8_t>> src_Data, CPointer<ptrdiff_t> src_LineSizes, AvPixelFormat pix_Fmt, c_int width, c_int height, CopyPlane_Delegate copy_Plane)//XX 381
		{
			AVPixFmtDescriptor desc = PixDesc.Av_Pix_Fmt_Desc_Get(pix_Fmt);

			if ((desc == null) || ((desc.Flags & AvPixelFormatFlag.HwAccel) != 0))
				return;

			if ((desc.Flags & AvPixelFormatFlag.Pal) != 0)
			{
				copy_Plane(dst_Data[0], dst_LineSizes[0], src_Data[0], src_LineSizes[0], width, height);

				// Copy the palette
				if (((desc.Flags & AvPixelFormatFlag.Pal) != 0) || (dst_Data[1].IsNotNull && src_Data[1].IsNotNull))
					CMemory.memcpy(dst_Data[1], src_Data[1], 4 * 256);
			}
			else
			{
				c_int planes_Nb = 0;

				for (c_int i = 0; i < desc.Nb_Components; i++)
					planes_Nb = Macros.FFMax(planes_Nb, desc.Comp[i].Plane + 1);

				for (c_int i = 0; i < planes_Nb; i++)
				{
					c_int h = height;
					ptrdiff_t bWidth = Av_Image_Get_LineSize(pix_Fmt, width, i);

					if (bWidth < 0)
					{
						Log.Av_Log(null, Log.Av_Log_Error, "av_image_get_linesize failed\n");

						return;
					}

					if ((i == 1) || (i == 2))
						h = Common.Av_Ceil_RShift(height, desc.Log2_Chroma_H);

					copy_Plane(dst_Data[i], dst_LineSizes[i], src_Data[i], src_LineSizes[i], bWidth, h);
				}
			}
		}
		#endregion
	}
}
