/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Descriptor that unambiguously describes how the bits of a pixel are
	/// stored in the up to 4 data planes of an image. It also stores the
	/// subsampling factors and number of components.
	///
	/// Note: This is separate of the colorspace (RGB, YCbCr, YPbPr, JPEG-style YUV
	///       and all the YUV variants) AVPixFmtDescriptor just stores how values
	///       are stored not what these values represent
	/// </summary>
	public class AVPixFmtDescriptor
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// The number of components each pixel has, (1-4)
		/// </summary>
		public uint8_t Nb_Components;

		/// <summary>
		/// Amount to shift the luma width right to find the chroma width.
		/// For YV12 this is 1 for example.
		/// chroma_width = AV_CEIL_RSHIFT(luma_width, log2_chroma_w)
		/// The note above is needed to ensure rounding up.
		/// This value only refers to the chroma components
		/// </summary>
		public uint8_t Log2_Chroma_W;

		/// <summary>
		/// Amount to shift the luma height right to find the chroma height.
		/// For YV12 this is 1 for example.
		/// chroma_height= AV_CEIL_RSHIFT(luma_height, log2_chroma_h)
		/// The note above is needed to ensure rounding up.
		/// This value only refers to the chroma components
		/// </summary>
		public uint8_t Log2_Chroma_H;

		/// <summary>
		/// Combination of AV_PIX_FMT_FLAG_... flags
		/// </summary>
		public AvPixelFormatFlag Flags;

		/// <summary>
		/// Parameters that describe how pixels are packed.
		/// If the format has 1 or 2 components, then luma is 0.
		/// If the format has 3 or 4 components:
		///   if the RGB flag is set then 0 is red, 1 is green and 2 is blue;
		///   otherwise 0 is luma, 1 is chroma-U and 2 is chroma-V.
		///
		/// If present, the Alpha channel is always the last component
		/// </summary>
		public AvComponentDescriptor[] Comp;

		/// <summary>
		/// Alternative comma-separated names
		/// </summary>
		public CPointer<char> Alias;
	}
}
