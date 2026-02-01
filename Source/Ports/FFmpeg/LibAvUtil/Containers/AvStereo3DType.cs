/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// List of possible 3D Types
	/// </summary>
	public enum AvStereo3DType
	{
		/// <summary>
		/// Video is not stereoscopic (and metadata has to be there)
		/// </summary>
		_2D,

		/// <summary>
		/// Views are next to each other
		///
		///     LLLLRRRR
		///     LLLLRRRR
		///     LLLLRRRR
		///     ...
		/// </summary>
		SideBySide,

		/// <summary>
		/// Views are on top of each other
		///
		///     LLLLLLLL
		///     LLLLLLLL
		///     RRRRRRRR
		///     RRRRRRRR
		/// </summary>
		TopBottom,

		/// <summary>
		/// Views are alternated temporally
		///
		///      frame0   frame1   frame2   ...
		///     LLLLLLLL RRRRRRRR LLLLLLLL
		///     LLLLLLLL RRRRRRRR LLLLLLLL
		///     LLLLLLLL RRRRRRRR LLLLLLLL
		///     ...      ...      ...
		/// </summary>
		FrameSequence,

		/// <summary>
		/// Views are packed in a checkerboard-like structure per pixel
		///
		///     LRLRLRLR
		///     RLRLRLRL
		///     LRLRLRLR
		///     ...
		/// </summary>
		Checkerboard,

		/// <summary>
		/// Views are next to each other, but when upscaling
		/// apply a checkerboard pattern
		///
		///      LLLLRRRR          L L L L    R R R R
		///      LLLLRRRR    =>     L L L L  R R R R
		///      LLLLRRRR          L L L L    R R R R
		///      LLLLRRRR           L L L L  R R R R
		/// </summary>
		SideBySide_Quincunx,

		/// <summary>
		/// Views are packed per line, as if interlaced
		///
		///     LLLLLLLL
		///     RRRRRRRR
		///     LLLLLLLL
		///     ...
		/// </summary>
		Lines,

		/// <summary>
		/// Views are packed per column
		///
		///     LRLRLRLR
		///     LRLRLRLR
		///     LRLRLRLR
		///     ...
		/// </summary>
		Columns,

		/// <summary>
		/// Video is stereoscopic but the packing is unspecified
		/// </summary>
		Unspec
	}
}
