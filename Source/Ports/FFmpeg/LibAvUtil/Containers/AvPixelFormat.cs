/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Pixel format.
	///
	/// AV_PIX_FMT_RGB32 is handled in an endian-specific manner. An RGBA
	/// color is put together as:
	///  (A ‹‹ 24) | (R ‹‹ 16) | (G ‹‹ 8) | B
	/// This is stored as BGRA on little-endian CPU architectures and ARGB on
	/// big-endian CPUs.
	///
	/// If the resolution is not a multiple of the chroma subsampling factor
	/// then the chroma plane resolution must be rounded up.
	///
	/// When the pixel format is palettized RGB32 (AV_PIX_FMT_PAL8), the palettized
	/// image data is stored in AVFrame.data[0]. The palette is transported in
	/// AVFrame.data[1], is 1024 bytes long (256 4-byte entries) and is
	/// formatted the same as in AV_PIX_FMT_RGB32 described above (i.e., it is
	/// also endian-specific). Note also that the individual RGB32 palette
	/// components stored in AVFrame.data[1] should be in the range 0..255.
	/// This is important as many custom PAL8 video codecs that were designed
	/// to run on the IBM VGA graphics adapter use 6-bit palette components.
	///
	/// For all the 8 bits per pixel formats, an RGB32 palette is in data[1] like
	/// for pal8. This palette is filled in automatically by the function
	/// allocating the picture
	/// </summary>
	public enum AvPixelFormat
	{
        /// <summary>
        /// No pixel format
        /// </summary>
		None = -1,

        /// <summary>
        /// Planar YUV 4:2:0, 12bpp, (1 Cr and Cb sample per 2x2 Y samples)
        /// </summary>
        YUV420P,

        /// <summary>
        /// Packed YUV 4:2:2, 16bpp, Y0 Cb Y1 Cr
        /// </summary>
        YUYV422,

        /// <summary>
        /// Packed RGB 8:8:8, 24bpp, RGBRGB...
        /// </summary>
        RGB24,

        /// <summary>
        /// Packed RGB 8:8:8, 24bpp, BGRBGR...
        /// </summary>
        BGR24,

        /// <summary>
        /// Planar YUV 4:2:2, 16bpp, (1 Cr and Cb sample per 2x1 Y samples)
        /// </summary>
        YUV422P,

        /// <summary>
        /// Planar YUV 4:4:4, 24bpp, (1 Cr and Cb sample per 1x1 Y samples)
        /// </summary>
        YUV444P,

        /// <summary>
        /// Planar YUV 4:1:0,  9bpp, (1 Cr and Cb sample per 4x4 Y samples)
        /// </summary>
        YUV410P,

        /// <summary>
        /// Planar YUV 4:1:1, 12bpp, (1 Cr and Cb sample per 4x1 Y samples)
        /// </summary>
        YUV411P,

        /// <summary>
        /// Y, 8bpp
        /// </summary>
        GRAY8,

        /// <summary>
        /// Y, 1bpp, 0 is white, 1 is black, pixels ordered msb->lsb
        /// </summary>
        MONOWHITE,

        /// <summary>
        /// Y, 1bpp, 0 is black, 1 is white, pixels ordered msb->lsb
        /// </summary>
        MONOBLACK,

        /// <summary>
        /// 8 bits with AV_PIX_FMT_RGB32 palette
        /// </summary>
        PAL8,

        /// <summary>
        /// Planar YUV 4:2:0, 12bpp, full scale (JPEG), deprecated in favor of YUV420P and setting color_range
        /// </summary>
        YUVJ420P,

        /// <summary>
        /// Planar YUV 4:2:2, 16bpp, full scale (JPEG), deprecated in favor of YUV422P and setting color_range
        /// </summary>
        YUVJ422P,

        /// <summary>
        /// Planar YUV 4:4:4, 24bpp, full scale (JPEG), deprecated in favor of YUV444P and setting color_range
        /// </summary>
        YUVJ444P,

        /// <summary>
        /// Packed YUV 4:2:2, 16bpp, Cb Y0 Cr Y1
        /// </summary>
        UYVY422,

        /// <summary>
        /// Packed YUV 4:1:1, 12bpp, Cb Y0 Y1 Cr Y2 Y3
        /// </summary>
        UYYVYY411,

        /// <summary>
        /// Packed RGB 3:3:2,  8bpp, (msb)2B 3G 3R(lsb)
        /// </summary>
        BGR8,

        /// <summary>
        /// Packed RGB 1:2:1 bitstream,  4bpp, first pixel is 4 msb bits
        /// </summary>
        BGR4,

        /// <summary>
        /// Packed RGB 1:2:1,  8bpp, (msb)1B 2G 1R(lsb)
        /// </summary>
        BGR4_BYTE,

        /// <summary>
        /// Packed RGB 3:3:2,  8bpp, (msb)3R 3G 2B(lsb)
        /// </summary>
        RGB8,

        /// <summary>
        /// Packed RGB 1:2:1 bitstream,  4bpp, first pixel is 4 msb bits
        /// </summary>
        RGB4,

        /// <summary>
        /// Packed RGB 1:2:1,  8bpp, (msb)1R 2G 1B(lsb)
        /// </summary>
        RGB4_BYTE,

        /// <summary>
        /// Planar YUV 4:2:0, 12bpp, 1 plane for Y and 1 plane for interleaved UV (U first)
        /// </summary>
        NV12,

        /// <summary>
        /// As NV12, but U and V bytes are swapped
        /// </summary>
        NV21,

        /// <summary>
        /// Packed ARGB 8:8:8:8, 32bpp
        /// </summary>
        ARGB,

        /// <summary>
        /// Packed RGBA 8:8:8:8, 32bpp
        /// </summary>
        RGBA,

        /// <summary>
        /// Packed ABGR 8:8:8:8, 32bpp
        /// </summary>
        ABGR,

        /// <summary>
        /// Packed BGRA 8:8:8:8, 32bpp
        /// </summary>
        BGRA,

        /// <summary>
        /// Y, 16bpp, big-endian
        /// </summary>
        GRAY16BE,

        /// <summary>
        /// Y, 16bpp, little-endian
        /// </summary>
        GRAY16LE,

        /// <summary>
        /// Planar YUV 4:4:0 (1 Cr and Cb sample per 1x2 Y samples)
        /// </summary>
        YUV440P,

        /// <summary>
        /// Planar YUV 4:4:0 full scale (JPEG); deprecated use YUV440P + color_range
        /// </summary>
        YUVJ440P,

        /// <summary>
        /// Planar YUV 4:2:0, 20bpp, (1 Cr and Cb sample per 2x2 Y and A samples)
        /// </summary>
        YUVA420P,

        /// <summary>
        /// Packed RGB 16:16:16, 48bpp big-endian
        /// </summary>
        RGB48BE,

        /// <summary>
        /// Packed RGB 16:16:16, 48bpp little-endian
        /// </summary>
        RGB48LE,

        /// <summary>
        /// Packed RGB 5:6:5, 16bpp big-endian
        /// </summary>
        RGB565BE,

        /// <summary>
        /// Packed RGB 5:6:5, 16bpp little-endian
        /// </summary>
        RGB565LE,

        /// <summary>
        /// Packed RGB 5:5:5, 16bpp (msb)1X 5R 5G 5B(lsb), big-endian, X unused
        /// </summary>
        RGB555BE,

        /// <summary>
        /// Packed RGB 5:5:5, 16bpp (msb)1X 5R 5G 5B(lsb), little-endian, X unused
        /// </summary>
        RGB555LE,

        /// <summary>
        /// Packed BGR 5:6:5, 16bpp big-endian
        /// </summary>
        BGR565BE,

        /// <summary>
        /// Packed BGR 5:6:5, 16bpp little-endian
        /// </summary>
        BGR565LE,

        /// <summary>
        /// Packed BGR 5:5:5, 16bpp big-endian
        /// </summary>
        BGR555BE,

        /// <summary>
        /// Packed BGR 5:5:5, 16bpp little-endian
        /// </summary>
        BGR555LE,

        /// <summary>
        /// Hardware acceleration through VA-API, data[3] contains a VASurfaceID
        /// </summary>
        VAAPI,

        /// <summary>
        /// Planar YUV 4:2:0, 24bpp little-endian
        /// </summary>
        YUV420P16LE,

        /// <summary>
        /// Planar YUV 4:2:0, 24bpp big-endian
        /// </summary>
        YUV420P16BE,

        /// <summary>
        /// Planar YUV 4:2:2, 32bpp little-endian
        /// </summary>
        YUV422P16LE,

        /// <summary>
        /// Planar YUV 4:2:2, 32bpp big-endian
        /// </summary>
        YUV422P16BE,

        /// <summary>
        /// Planar YUV 4:4:4, 48bpp little-endian
        /// </summary>
        YUV444P16LE,

        /// <summary>
        /// Planar YUV 4:4:4, 48bpp big-endian
        /// </summary>
        YUV444P16BE,

        /// <summary>
        /// HW decoding through DXVA2, Picture.data[3] contains LPDIRECT3DSURFACE9 pointer
        /// </summary>
        DXVA2_VLD,

        /// <summary>
        /// Packed RGB 4:4:4, 16bpp little-endian
        /// </summary>
        RGB444LE,

        /// <summary>
        /// Packed RGB 4:4:4, 16bpp big-endian
        /// </summary>
        RGB444BE,

        /// <summary>
        /// Packed BGR 4:4:4, 16bpp little-endian
        /// </summary>
        BGR444LE,

        /// <summary>
        /// Packed BGR 4:4:4, 16bpp big-endian
        /// </summary>
        BGR444BE,

        /// <summary>
        /// 8 bits gray, 8 bits alpha
        /// </summary>
        YA8,

        /// <summary>
        /// Alias for YA8
        /// </summary>
        Y400A = YA8,

        /// <summary>
        /// Alias for YA8
        /// </summary>
        GRAY8A = YA8,

        /// <summary>
        /// Packed BGR 16:16:16, 48bpp big-endian
        /// </summary>
        BGR48BE,

        /// <summary>
        /// Packed BGR 16:16:16, 48bpp little-endian
        /// </summary>
        BGR48LE,

        /// <summary>
        /// Planar YUV 4:2:0, 13.5bpp big-endian
        /// </summary>
        YUV420P9BE,

        /// <summary>
        /// Planar YUV 4:2:0, 13.5bpp little-endian
        /// </summary>
        YUV420P9LE,

        /// <summary>
        /// Planar YUV 4:2:0, 15bpp big-endian
        /// </summary>
        YUV420P10BE,

        /// <summary>
        /// Planar YUV 4:2:0, 15bpp little-endian
        /// </summary>
        YUV420P10LE,

        /// <summary>
        /// Planar YUV 4:2:2, 20bpp big-endian
        /// </summary>
        YUV422P10BE,

        /// <summary>
        /// Planar YUV 4:2:2, 20bpp little-endian
        /// </summary>
        YUV422P10LE,

        /// <summary>
        /// Planar YUV 4:4:4, 27bpp big-endian
        /// </summary>
        YUV444P9BE,

        /// <summary>
        /// Planar YUV 4:4:4, 27bpp little-endian
        /// </summary>
        YUV444P9LE,

        /// <summary>
        /// Planar YUV 4:4:4, 30bpp big-endian
        /// </summary>
        YUV444P10BE,

        /// <summary>
        /// Planar YUV 4:4:4, 30bpp little-endian
        /// </summary>
        YUV444P10LE,

        /// <summary>
        /// Planar YUV 4:2:2, 18bpp big-endian
        /// </summary>
        YUV422P9BE,

        /// <summary>
        /// Planar YUV 4:2:2, 18bpp little-endian
        /// </summary>
        YUV422P9LE,

        /// <summary>
        /// Planar GBR 4:4:4 24bpp
        /// </summary>
        GBRP,

        /// <summary>
        /// Alias for GBRP
        /// </summary>
        GBR24P = GBRP,

        /// <summary>
        /// Planar GBR 4:4:4 27bpp big-endian
        /// </summary>
        GBRP9BE,

        /// <summary>
        /// Planar GBR 4:4:4 27bpp little-endian
        /// </summary>
        GBRP9LE,

        /// <summary>
        /// Planar GBR 4:4:4 30bpp big-endian
        /// </summary>
        GBRP10BE,

        /// <summary>
        /// Planar GBR 4:4:4 30bpp little-endian
        /// </summary>
        GBRP10LE,

        /// <summary>
        /// Planar GBR 4:4:4 48bpp big-endian
        /// </summary>
        GBRP16BE,

        /// <summary>
        /// Planar GBR 4:4:4 48bpp little-endian
        /// </summary>
        GBRP16LE,

        /// <summary>
        /// Planar YUV 4:2:2 24bpp (1 Cr and Cb sample per 2x1 Y and A samples)
        /// </summary>
        YUVA422P,

        /// <summary>
        /// Planar YUV 4:4:4 32bpp (1 Cr and Cb sample per 1x1 Y and A samples)
        /// </summary>
        YUVA444P,

        /// <summary>
        /// Planar YUV 4:2:0 22.5bpp big-endian
        /// </summary>
        YUVA420P9BE,

        /// <summary>
        /// Planar YUV 4:2:0 22.5bpp little-endian
        /// </summary>
        YUVA420P9LE,

        /// <summary>
        /// Planar YUV 4:2:2 27bpp big-endian
        /// </summary>
        YUVA422P9BE,

        /// <summary>
        /// Planar YUV 4:2:2 27bpp little-endian
        /// </summary>
        YUVA422P9LE,

        /// <summary>
        /// Planar YUV 4:4:4 36bpp big-endian
        /// </summary>
        YUVA444P9BE,

        /// <summary>
        /// Planar YUV 4:4:4 36bpp little-endian
        /// </summary>
        YUVA444P9LE,

        /// <summary>
        /// Planar YUV 4:2:0 25bpp big-endian
        /// </summary>
        YUVA420P10BE,

        /// <summary>
        /// Planar YUV 4:2:0 25bpp little-endian
        /// </summary>
        YUVA420P10LE,

        /// <summary>
        /// Planar YUV 4:2:2 30bpp big-endian
        /// </summary>
        YUVA422P10BE,

        /// <summary>
        /// Planar YUV 4:2:2 30bpp little-endian
        /// </summary>
        YUVA422P10LE,

        /// <summary>
        /// Planar YUV 4:4:4 40bpp big-endian
        /// </summary>
        YUVA444P10BE,

        /// <summary>
        /// Planar YUV 4:4:4 40bpp little-endian
        /// </summary>
        YUVA444P10LE,

        /// <summary>
        /// Planar YUV 4:2:0 40bpp big-endian
        /// </summary>
        YUVA420P16BE,

        /// <summary>
        /// Planar YUV 4:2:0 40bpp little-endian
        /// </summary>
        YUVA420P16LE,

        /// <summary>
        /// Planar YUV 4:2:2 48bpp big-endian
        /// </summary>
        YUVA422P16BE,

        /// <summary>
        /// Planar YUV 4:2:2 48bpp little-endian
        /// </summary>
        YUVA422P16LE,

        /// <summary>
        /// Planar YUV 4:4:4 64bpp big-endian
        /// </summary>
        YUVA444P16BE,

        /// <summary>
        /// Planar YUV 4:4:4 64bpp little-endian
        /// </summary>
        YUVA444P16LE,

        /// <summary>
        /// HW acceleration through VDPAU, Picture.data[3] contains VdpVideoSurface
        /// </summary>
        VDPAU,

        /// <summary>
        /// Packed XYZ 4:4:4, 36 bpp little-endian
        /// </summary>
        XYZ12LE,

        /// <summary>
        /// Packed XYZ 4:4:4, 36 bpp big-endian
        /// </summary>
        XYZ12BE,

        /// <summary>
        /// Interleaved chroma YUV 4:2:2, 16bpp
        /// </summary>
        NV16,

        /// <summary>
        /// Interleaved chroma YUV 4:2:2, 20bpp little-endian
        /// </summary>
        NV20LE,

        /// <summary>
        /// Interleaved chroma YUV 4:2:2, 20bpp big-endian
        /// </summary>
        NV20BE,

        /// <summary>
        /// Packed RGBA 16:16:16:16, 64bpp big-endian
        /// </summary>
        RGBA64BE,

        /// <summary>
        /// Packed RGBA 16:16:16:16, 64bpp little-endian
        /// </summary>
        RGBA64LE,

        /// <summary>
        /// Packed RGBA (B,G,R,A) 16:16:16:16, 64bpp big-endian
        /// </summary>
        BGRA64BE,

        /// <summary>
        /// Packed RGBA (B,G,R,A) 16:16:16:16, 64bpp little-endian
        /// </summary>
        BGRA64LE,

        /// <summary>
        /// Packed YUV 4:2:2, 16bpp, Y0 Cr Y1 Cb
        /// </summary>
        YVYU422,

        /// <summary>
        /// 16 bits gray, 16 bits alpha big-endian
        /// </summary>
        YA16BE,

        /// <summary>
        /// 16 bits gray, 16 bits alpha little-endian
        /// </summary>
        YA16LE,

        /// <summary>
        /// Planar GBRA 4:4:4:4 32bpp
        /// </summary>
        GBRAP,

        /// <summary>
        /// Planar GBRA 4:4:4:4 64bpp big-endian
        /// </summary>
        GBRAP16BE,

        /// <summary>
        /// Planar GBRA 4:4:4:4 64bpp little-endian
        /// </summary>
        GBRAP16LE,

        /// <summary>
        /// HW acceleration through QSV, data[3] contains pointer to mfxFrameSurface1 structure
        /// </summary>
        QSV,

        /// <summary>
        /// HW acceleration though MMAL, data[3] contains pointer to MMAL_BUFFER_HEADER_T
        /// </summary>
        MMAL,

        /// <summary>
        /// HW decoding through Direct3D11 via old API, Picture.data[3] contains ID3D11VideoDecoderOutputView pointer
        /// </summary>
        D3D11VA_VLD,

        /// <summary>
        /// HW acceleration through CUDA. data[i] contain CUdeviceptr pointers
        /// </summary>
        CUDA,

        /// <summary>
        /// Packed RGB 8:8:8, 32bpp, XRGBXRGB... X unused
        /// </summary>
        _0RGB,

        /// <summary>
        /// Packed RGB 8:8:8, 32bpp, RGBXRGBX... X unused
        /// </summary>
        RGB0,

        /// <summary>
        /// Packed BGR 8:8:8, 32bpp, XBGRXBGR... X unused
        /// </summary>
        _0BGR,

        /// <summary>
        /// Packed BGR 8:8:8, 32bpp, BGRXBGRX... X unused
        /// </summary>
        BGR0,

        /// <summary>
        /// Planar YUV 4:2:0,18bpp big-endian
        /// </summary>
        YUV420P12BE,

        /// <summary>
        /// Planar YUV 4:2:0,18bpp little-endian
        /// </summary>
        YUV420P12LE,

        /// <summary>
        /// Planar YUV 4:2:0,21bpp big-endian
        /// </summary>
        YUV420P14BE,

        /// <summary>
        /// Planar YUV 4:2:0,21bpp little-endian
        /// </summary>
        YUV420P14LE,

        /// <summary>
        /// Planar YUV 4:2:2,24bpp big-endian
        /// </summary>
        YUV422P12BE,

        /// <summary>
        /// Planar YUV 4:2:2,24bpp little-endian
        /// </summary>
        YUV422P12LE,

        /// <summary>
        /// Planar YUV 4:2:2,28bpp big-endian
        /// </summary>
        YUV422P14BE,

        /// <summary>
        /// Planar YUV 4:2:2,28bpp little-endian
        /// </summary>
        YUV422P14LE,

        /// <summary>
        /// Planar YUV 4:4:4,36bpp big-endian
        /// </summary>
        YUV444P12BE,

        /// <summary>
        /// Planar YUV 4:4:4,36bpp little-endian
        /// </summary>
        YUV444P12LE,

        /// <summary>
        /// Planar YUV 4:4:4,42bpp big-endian
        /// </summary>
        YUV444P14BE,

        /// <summary>
        /// Planar YUV 4:4:4,42bpp little-endian
        /// </summary>
        YUV444P14LE,

        /// <summary>
        /// Planar GBR 4:4:4 36bpp big-endian
        /// </summary>
        GBRP12BE,

        /// <summary>
        /// Planar GBR 4:4:4 36bpp little-endian
        /// </summary>
        GBRP12LE,

        /// <summary>
        /// Planar GBR 4:4:4 42bpp big-endian
        /// </summary>
        GBRP14BE,

        /// <summary>
        /// Planar GBR 4:4:4 42bpp little-endian
        /// </summary>
        GBRP14LE,

        /// <summary>
        /// Planar YUV 4:1:1, 12bpp full scale (JPEG), deprecated
        /// </summary>
        YUVJ411P,

        /// <summary>
        /// Bayer BGGR 8-bit
        /// </summary>
        BAYER_BGGR8,

        /// <summary>
        /// Bayer RGGB 8-bit
        /// </summary>
        BAYER_RGGB8,

        /// <summary>
        /// Bayer GBRG 8-bit
        /// </summary>
        BAYER_GBRG8,

        /// <summary>
        /// Bayer GRBG 8-bit
        /// </summary>
        BAYER_GRBG8,

        /// <summary>
        /// Bayer BGGR 16-bit little-endian
        /// </summary>
        BAYER_BGGR16LE,

        /// <summary>
        /// Bayer BGGR 16-bit big-endian
        /// </summary>
        BAYER_BGGR16BE,

        /// <summary>
        /// Bayer RGGB 16-bit little-endian
        /// </summary>
        BAYER_RGGB16LE,

        /// <summary>
        /// Bayer RGGB 16-bit big-endian
        /// </summary>
        BAYER_RGGB16BE,

        /// <summary>
        /// Bayer GBRG 16-bit little-endian
        /// </summary>
        BAYER_GBRG16LE,

        /// <summary>
        /// Bayer GBRG 16-bit big-endian
        /// </summary>
        BAYER_GBRG16BE,

        /// <summary>
        /// Bayer GRBG 16-bit little-endian
        /// </summary>
        BAYER_GRBG16LE,

        /// <summary>
        /// Bayer GRBG 16-bit big-endian
        /// </summary>
        BAYER_GRBG16BE,

        /// <summary>
        /// Planar YUV 4:4:0,20bpp little-endian
        /// </summary>
        YUV440P10LE,

        /// <summary>
        /// Planar YUV 4:4:0,20bpp big-endian
        /// </summary>
        YUV440P10BE,

        /// <summary>
        /// Planar YUV 4:4:0,24bpp little-endian
        /// </summary>
        YUV440P12LE,

        /// <summary>
        /// Planar YUV 4:4:0,24bpp big-endian
        /// </summary>
        YUV440P12BE,

        /// <summary>
        /// Packed AYUV 4:4:4,64bpp little-endian
        /// </summary>
        AYUV64LE,

        /// <summary>
        /// Packed AYUV 4:4:4,64bpp big-endian
        /// </summary>
        AYUV64BE,

        /// <summary>
        /// Hardware decoding through Videotoolbox
        /// </summary>
        VIDEOTOOLBOX,

        /// <summary>
        /// NV12-like with 10bpp per component, high bits, little-endian
        /// </summary>
        P010LE,

        /// <summary>
        /// NV12-like with 10bpp per component, high bits, big-endian
        /// </summary>
        P010BE,

        /// <summary>
        /// Planar GBR 4:4:4:4 48bpp big-endian
        /// </summary>
        GBRAP12BE,

        /// <summary>
        /// Planar GBR 4:4:4:4 48bpp little-endian
        /// </summary>
        GBRAP12LE,

        /// <summary>
        /// Planar GBR 4:4:4:4 40bpp big-endian
        /// </summary>
        GBRAP10BE,

        /// <summary>
        /// Planar GBR 4:4:4:4 40bpp little-endian
        /// </summary>
        GBRAP10LE,

        /// <summary>
        /// Hardware decoding through MediaCodec
        /// </summary>
        MEDIACODEC,

        /// <summary>
        /// Y, 12bpp big-endian
        /// </summary>
        GRAY12BE,

        /// <summary>
        /// Y, 12bpp little-endian
        /// </summary>
        GRAY12LE,

        /// <summary>
        /// Y, 10bpp big-endian
        /// </summary>
        GRAY10BE,

        /// <summary>
        /// Y, 10bpp little-endian
        /// </summary>
        GRAY10LE,

        /// <summary>
        /// NV12-like with 16bpp per component little-endian
        /// </summary>
        P016LE,

        /// <summary>
        /// NV12-like with 16bpp per component big-endian
        /// </summary>
        P016BE,

        /// <summary>
        /// Direct3D11 hardware surfaces (preferred over legacy D3D11VA_VLD)
        /// </summary>
        D3D11,

        /// <summary>
        /// Y, 9bpp big-endian
        /// </summary>
        GRAY9BE,

        /// <summary>
        /// Y, 9bpp little-endian
        /// </summary>
        GRAY9LE,

        /// <summary>
        /// IEEE-754 single precision planar GBR 4:4:4 96bpp big-endian
        /// </summary>
        GBRPF32BE,

        /// <summary>
        /// IEEE-754 single precision planar GBR 4:4:4 96bpp little-endian
        /// </summary>
        GBRPF32LE,

        /// <summary>
        /// IEEE-754 single precision planar GBRA 4:4:4:4 128bpp big-endian
        /// </summary>
        GBRAPF32BE,

        /// <summary>
        /// IEEE-754 single precision planar GBRA 4:4:4:4 128bpp little-endian
        /// </summary>
        GBRAPF32LE,

        /// <summary>
        /// DRM-managed buffers exposed through PRIME buffer sharing (data[0] -> AVDRMFrameDescriptor)
        /// </summary>
        DRM_PRIME,

        /// <summary>
        /// OpenCL hardware surfaces (data[i] contain cl_mem image2d_t objects)
        /// </summary>
        OPENCL,

        /// <summary>
        /// Y, 14bpp big-endian
        /// </summary>
        GRAY14BE,

        /// <summary>
        /// Y, 14bpp little-endian
        /// </summary>
        GRAY14LE,

        /// <summary>
        /// IEEE-754 single precision Y, 32bpp big-endian
        /// </summary>
        GRAYF32BE,

        /// <summary>
        /// IEEE-754 single precision Y, 32bpp little-endian
        /// </summary>
        GRAYF32LE,

        /// <summary>
        /// Planar YUV 4:2:2,24bpp 12b alpha big-endian
        /// </summary>
        YUVA422P12BE,

        /// <summary>
        /// Planar YUV 4:2:2,24bpp 12b alpha little-endian
        /// </summary>
        YUVA422P12LE,

        /// <summary>
        /// Planar YUV 4:4:4,36bpp 12b alpha big-endian
        /// </summary>
        YUVA444P12BE,

        /// <summary>
        /// Planar YUV 4:4:4,36bpp 12b alpha little-endian
        /// </summary>
        YUVA444P12LE,

        /// <summary>
        /// Planar YUV 4:4:4, 24bpp interleaved UV (U first)
        /// </summary>
        NV24,

        /// <summary>
        /// As NV24 but U and V swapped
        /// </summary>
        NV42,

        /// <summary>
        /// Vulkan hardware images (data[0] -> AVVkFrame)
        /// </summary>
        VULKAN,

        /// <summary>
        /// Packed YUV 4:2:2 like YUYV422, 20bpp big-endian high bits used
        /// </summary>
        Y210BE,

        /// <summary>
        /// Packed YUV 4:2:2 like YUYV422, 20bpp little-endian high bits used
        /// </summary>
        Y210LE,

        /// <summary>
        /// Packed RGB 10:10:10 30bpp little-endian X unused
        /// </summary>
        X2RGB10LE,

        /// <summary>
        /// Packed RGB 10:10:10 30bpp big-endian X unused
        /// </summary>
        X2RGB10BE,

        /// <summary>
        /// Packed BGR 10:10:10 30bpp little-endian X unused
        /// </summary>
        X2BGR10LE,

        /// <summary>
        /// Packed BGR 10:10:10 30bpp big-endian X unused
        /// </summary>
        X2BGR10BE,

        /// <summary>
        /// Interleaved chroma YUV 4:2:2, 20bpp high bits big-endian
        /// </summary>
        P210BE,

        /// <summary>
        /// Interleaved chroma YUV 4:2:2, 20bpp high bits little-endian
        /// </summary>
        P210LE,

        /// <summary>
        /// Interleaved chroma YUV 4:4:4, 30bpp high bits big-endian
        /// </summary>
        P410BE,

        /// <summary>
        /// Interleaved chroma YUV 4:4:4, 30bpp high bits little-endian
        /// </summary>
        P410LE,

        /// <summary>
        /// Interleaved chroma YUV 4:2:2, 32bpp big-endian
        /// </summary>
        P216BE,

        /// <summary>
        /// Interleaved chroma YUV 4:2:2, 32bpp little-endian
        /// </summary>
        P216LE,

        /// <summary>
        /// Interleaved chroma YUV 4:4:4, 48bpp big-endian
        /// </summary>
        P416BE,

        /// <summary>
        /// Interleaved chroma YUV 4:4:4, 48bpp little-endian
        /// </summary>
        P416LE,

        /// <summary>
        /// Packed VUYA 4:4:4:4, 32bpp VUYAVUYA...
        /// </summary>
        VUYA,

        /// <summary>
        /// IEEE-754 half precision packed RGBA 16:16:16:16 64bpp big-endian
        /// </summary>
        RGBAF16BE,

        /// <summary>
        /// IEEE-754 half precision packed RGBA 16:16:16:16 64bpp little-endian
        /// </summary>
        RGBAF16LE,

        /// <summary>
        /// Packed VUYX 4:4:4:4, 32bpp variant of VUYA with undefined alpha
        /// </summary>
        VUYX,

        /// <summary>
        /// NV12-like 12bpp per component high bits little-endian
        /// </summary>
        P012LE,

        /// <summary>
        /// NV12-like 12bpp per component high bits big-endian
        /// </summary>
        P012BE,

        /// <summary>
        /// Packed YUV 4:2:2 like YUYV422, 24bpp high bits zeros low bits big-endian
        /// </summary>
        Y212BE,

        /// <summary>
        /// Packed YUV 4:2:2 like YUYV422, 24bpp high bits zeros low bits little-endian
        /// </summary>
        Y212LE,

        /// <summary>
        /// Packed XVYU 4:4:4, 32bpp big-endian variant of Y410 w/o alpha
        /// </summary>
        XV30BE,

        /// <summary>
        /// Packed XVYU 4:4:4, 32bpp little-endian variant of Y410 w/o alpha
        /// </summary>
        XV30LE,

        /// <summary>
        /// Packed XVYU 4:4:4, 48bpp big-endian variant of Y412 w/o alpha
        /// </summary>
        XV36BE,

        /// <summary>
        /// Packed XVYU 4:4:4, 48bpp little-endian variant of Y412 w/o alpha
        /// </summary>
        XV36LE,

        /// <summary>
        /// IEEE-754 single precision packed RGB 32:32:32 96bpp big-endian
        /// </summary>
        RGBF32BE,

        /// <summary>
        /// IEEE-754 single precision packed RGB 32:32:32 96bpp little-endian
        /// </summary>
        RGBF32LE,

        /// <summary>
        /// IEEE-754 single precision packed RGBA 32:32:32:32 128bpp big-endian
        /// </summary>
        RGBAF32BE,

        /// <summary>
        /// IEEE-754 single precision packed RGBA 32:32:32:32 128bpp little-endian
        /// </summary>
        RGBAF32LE,

        /// <summary>
        /// Interleaved chroma YUV 4:2:2, 24bpp high bits big-endian
        /// </summary>
        P212BE,

        /// <summary>
        /// Interleaved chroma YUV 4:2:2, 24bpp high bits little-endian
        /// </summary>
        P212LE,

        /// <summary>
        /// Interleaved chroma YUV 4:4:4, 36bpp high bits big-endian
        /// </summary>
        P412BE,

        /// <summary>
        /// Interleaved chroma YUV 4:4:4, 36bpp high bits little-endian
        /// </summary>
        P412LE,

        /// <summary>
        /// Planar GBR 4:4:4:4 56bpp big-endian
        /// </summary>
        GBRAP14BE,

        /// <summary>
        /// Planar GBR 4:4:4:4 56bpp little-endian
        /// </summary>
        GBRAP14LE,

        /// <summary>
        /// Direct3D 12 hardware surfaces (data[0] -> AVD3D12VAFrame)
        /// </summary>
        D3D12,

        /// <summary>
        /// Packed AYUV 4:4:4:4, 32bpp AYUVAYUV...
        /// </summary>
        AYUV,

        /// <summary>
        /// Packed UYVA 4:4:4:4, 32bpp UYVAUYVA...
        /// </summary>
        UYVA,

        /// <summary>
        /// Packed VYU 4:4:4, 24bpp VYUVYU...
        /// </summary>
        VYU444,

        /// <summary>
        /// Packed VYUX 4:4:4 like XV30 32bpp big-endian
        /// </summary>
        V30XBE,

        /// <summary>
        /// Packed VYUX 4:4:4 like XV30 32bpp little-endian
        /// </summary>
        V30XLE,

        /// <summary>
        /// IEEE-754 half precision packed RGB 16:16:16 48bpp big-endian
        /// </summary>
        RGBF16BE,

        /// <summary>
        /// IEEE-754 half precision packed RGB 16:16:16 48bpp little-endian
        /// </summary>
        RGBF16LE,

        /// <summary>
        /// Packed RGBA 32:32:32:32 128bpp big-endian
        /// </summary>
        RGBA128BE,

        /// <summary>
        /// Packed RGBA 32:32:32:32 128bpp little-endian
        /// </summary>
        RGBA128LE,

        /// <summary>
        /// Packed RGB 32:32:32 96bpp big-endian
        /// </summary>
        RGB96BE,

        /// <summary>
        /// Packed RGB 32:32:32 96bpp little-endian
        /// </summary>
        RGB96LE,

        /// <summary>
        /// Packed YUV 4:2:2 like YUYV422, 32bpp big-endian
        /// </summary>
        Y216BE,

        /// <summary>
        /// Packed YUV 4:2:2 like YUYV422, 32bpp little-endian
        /// </summary>
        Y216LE,

        /// <summary>
        /// Packed XVYU 4:4:4, 64bpp big-endian (Y416 without alpha)
        /// </summary>
        XV48BE,

        /// <summary>
        /// Packed XVYU 4:4:4, 64bpp little-endian (Y416 without alpha)
        /// </summary>
        XV48LE,

        /// <summary>
        /// IEEE-754 half precision planar GBR 4:4:4 48bpp big-endian
        /// </summary>
        GBRPF16BE,

        /// <summary>
        /// IEEE-754 half precision planar GBR 4:4:4 48bpp little-endian
        /// </summary>
        GBRPF16LE,

        /// <summary>
        /// IEEE-754 half precision planar GBRA 4:4:4:4 64bpp big-endian
        /// </summary>
        GBRAPF16BE,

        /// <summary>
        /// IEEE-754 half precision planar GBRA 4:4:4:4 64bpp little-endian
        /// </summary>
        GBRAPF16LE,

        /// <summary>
        /// IEEE-754 half precision Y 16bpp big-endian
        /// </summary>
        GRAYF16BE,

        /// <summary>
        /// IEEE-754 half precision Y 16bpp little-endian
        /// </summary>
        GRAYF16LE,

        /// <summary>
        /// HW acceleration through AMF. data[0] contains AMFSurface pointer
        /// </summary>
        AMF_SURFACE,

        /// <summary>
        /// Y, 32bpp big-endian
        /// </summary>
        GRAY32BE,

        /// <summary>
        /// Y, 32bpp little-endian
        /// </summary>
        GRAY32LE,

        /// <summary>
        /// IEEE-754 single precision packed YA 32+32 64bpp big-endian
        /// </summary>
        YAF32BE,

        /// <summary>
        /// IEEE-754 single precision packed YA 32+32 64bpp little-endian
        /// </summary>
        YAF32LE,

        /// <summary>
        /// IEEE-754 half precision packed YA 16+16 32bpp big-endian
        /// </summary>
        YAF16BE,

        /// <summary>
        /// IEEE-754 half precision packed YA 16+16 32bpp little-endian
        /// </summary>
        YAF16LE,

        /// <summary>
        /// Planar GBRA 4:4:4:4 128bpp big-endian
        /// </summary>
        GBRAP32BE,

        /// <summary>
        /// Planar GBRA 4:4:4:4 128bpp little-endian
        /// </summary>
        GBRAP32LE,

        /// <summary>
        /// Planar YUV 4:4:4 30bpp lowest bits zero big-endian
        /// </summary>
        YUV444P10MSBBE,

        /// <summary>
        /// Planar YUV 4:4:4 30bpp lowest bits zero little-endian
        /// </summary>
        YUV444P10MSBLE,

        /// <summary>
        /// Planar YUV 4:4:4 30bpp lowest bits zero big-endian (12 bits nominal?)
        /// </summary>
        YUV444P12MSBBE,

        /// <summary>
        /// Planar YUV 4:4:4 30bpp lowest bits zero little-endian (12 bits nominal?)
        /// </summary>
        YUV444P12MSBLE,

        /// <summary>
        /// Planar GBR 4:4:4 30bpp lowest bits zero big-endian
        /// </summary>
        GBRP10MSBBE,

        /// <summary>
        /// Planar GBR 4:4:4 30bpp lowest bits zero little-endian
        /// </summary>
        GBRP10MSBLE,

        /// <summary>
        /// Planar GBR 4:4:4 36bpp lowest bits zero big-endian
        /// </summary>
        GBRP12MSBBE,

        /// <summary>
        /// Planar GBR 4:4:4 36bpp lowest bits zero little-endian
        /// </summary>
        GBRP12MSBLE,

        /// <summary>
        /// Hardware decoding through openharmony
        /// </summary>
        OHCODEC,

        /// <summary>
        /// Number of pixel formats marker (do not use for ABI-sensitive code)
        /// </summary>
        Nb
	}
}
