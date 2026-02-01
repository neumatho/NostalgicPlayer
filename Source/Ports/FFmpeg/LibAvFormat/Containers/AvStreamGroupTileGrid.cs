/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// AVStreamGroupTileGrid holds information on how to combine several
	/// independent images on a single canvas for presentation.
	///
	/// The output should be a AVStreamGroupTileGrid.background "background"
	/// colored AVStreamGroupTileGrid.coded_width "coded_width" x
	/// AVStreamGroupTileGrid.coded_height "coded_height" canvas where a
	/// AVStreamGroupTileGrid.nb_tiles "nb_tiles" amount of tiles are placed in
	/// the order they appear in the AVStreamGroupTileGrid.offsets "offsets"
	/// array, at the exact offset described for them. In particular, if two or more
	/// tiles overlap, the image with higher index in the
	/// AVStreamGroupTileGrid.offsets "offsets" array takes priority.
	/// Note that a single image may be used multiple times, i.e. multiple entries
	/// in AVStreamGroupTileGrid.offsets "offsets" may have the same value of
	/// idx.
	///
	/// The following is an example of a simple grid with 3 rows and 4 columns:
	///
	/// +---+---+---+---+
	/// | 0 | 1 | 2 | 3 |
	/// +---+---+---+---+
	/// | 4 | 5 | 6 | 7 |
	/// +---+---+---+---+
	/// | 8 | 9 |10 |11 |
	/// +---+---+---+---+
	///
	/// Assuming all tiles have a dimension of 512x512, the
	/// AVStreamGroupTileGrid.offsets "offset" of the topleft pixel of
	/// the first AVStreamGroup.streams "stream" in the group is "0,0", the
	/// AVStreamGroupTileGrid.offsets "offset" of the topleft pixel of
	/// the second AVStreamGroup.streams "stream" in the group is "512,0", the
	/// AVStreamGroupTileGrid.offsets "offset" of the topleft pixel of
	/// the fifth AVStreamGroup.streams "stream" in the group is "0,512", the
	/// AVStreamGroupTileGrid.offsets "offset", of the topleft pixel of
	/// the sixth AVStreamGroup.streams "stream" in the group is "512,512",
	/// etc.
	///
	/// The following is an example of a canvas with overlapping tiles:
	///
	/// +-----------+
	/// |   %%%%%   |
	/// |***%%3%%@@@|
	/// |**0%%%%%2@@|
	/// |***##1@@@@@|
	/// |   #####   |
	/// +-----------+
	///
	/// Assuming a canvas with size 1024x1024 and all tiles with a dimension of
	/// 512x512, a possible AVStreamGroupTileGrid.offsets "offset" for the
	/// topleft pixel of the first AVStreamGroup.streams "stream" in the group
	/// would be 0x256, the AVStreamGroupTileGrid.offsets "offset" for the
	/// topleft pixel of the second AVStreamGroup.streams "stream" in the group
	/// would be 256x512, the AVStreamGroupTileGrid.offsets "offset" for the
	/// topleft pixel of the third AVStreamGroup.streams "stream" in the group
	/// would be 512x256, and the AVStreamGroupTileGrid.offsets "offset" for
	/// the topleft pixel of the fourth AVStreamGroup.streams "stream" in the
	/// group would be 256x0.
	///
	/// sizeof(AVStreamGroupTileGrid) is not a part of the ABI and may only be
	/// allocated by avformat_stream_group_create()
	/// </summary>
	public class AvStreamGroupTileGrid : AvClass, IGroupType
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// Amount of tiles in the grid.
		///
		/// Must be > 0
		/// </summary>
		public c_uint Nb_Tiles;

		/// <summary>
		/// Width of the canvas.
		///
		/// Must be > 0
		/// </summary>
		public c_int Coded_Width;

		/// <summary>
		/// Width of the canvas.
		///
		/// Must be > 0
		/// </summary>
		public c_int Coded_Height;

		/// <summary>
		/// An nb_tiles sized array of offsets in pixels from the topleft edge
		/// of the canvas, indicating where each stream should be placed.
		/// It must be allocated with the av_malloc() family of functions.
		///
		/// - demuxing: set by libavformat, must not be modified by the caller.
		/// - muxing: set by the caller before avformat_write_header().
		///
		/// Freed by libavformat in avformat_free_context()
		/// </summary>
		public CPointer<(
			// Index of the stream in the group this tile references.
			// 
			// Must be < AVStreamGroup.nb_streams "nb_streams"
			c_uint Idx,

			// Offset in pixels from the left edge of the canvas where the tile
			// should be placed
			c_int Horizontal,

			// Offset in pixels from the top edge of the canvas where the tile
			// should be placed
			c_int Vertical
		)> Offsets;

		/// <summary>
		/// The pixel value per channel in RGBA format used if no pixel of any tile
		/// is located at a particular pixel location.
		///
		/// See av_image_fill_color()
		/// See av_parse_color()
		/// </summary>
		public readonly uint8_t[] Background = new uint8_t[4];

		/// <summary>
		/// Offset in pixels from the left edge of the canvas where the actual image
		/// meant for presentation starts.
		///
		/// This field must be ›= 0 and ‹ coded_width
		/// </summary>
		public c_int Horizontal_Offset;

		/// <summary>
		/// Offset in pixels from the top edge of the canvas where the actual image
		/// meant for presentation starts.
		///
		/// This field must be ›= 0 and ‹ coded_height
		/// </summary>
		public c_int Verical_Offset;

		/// <summary>
		/// Width of the final image for presentation.
		///
		/// Must be › 0 and ‹= (coded_width - horizontal_offset).
		/// When it's not equal to (coded_width - horizontal_offset), the
		/// result of (coded_width - width - horizontal_offset) is the
		/// amount of pixels to be cropped from the right edge of the
		/// final image before presentation
		/// </summary>
		public c_int Width;

		/// <summary>
		/// Height of the final image for presentation.
		///
		/// Must be › 0 and ‹= (coded_height - vertical_offset).
		/// When it's not equal to (coded_height - vertical_offset), the
		/// result of (coded_height - height - vertical_offset) is the
		/// amount of pixels to be cropped from the bottom edge of the
		/// final image before presentation
		/// </summary>
		public c_int Height;

		/// <summary>
		/// Additional data associated with the grid.
		///
		/// Should be allocated with av_packet_side_data_new() or
		/// av_packet_side_data_add(), and will be freed by avformat_free_context()
		/// </summary>
		public CPointer<AvPacketSideData> Coded_Side_Data;

		/// <summary>
		/// Amount of entries in coded_side_data
		/// </summary>
		public c_int Nb_Coded_Side_Data;
	}
}
