/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvSideDataProps
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// The side data type can be used in stream-global structures.
		/// Side data types without this property are only meaningful on per-frame
		/// basis
		/// </summary>
		Global = 1 << 0,

		/// <summary>
		/// Multiple instances of this side data type can be meaningfully present in
		/// a single side data array
		/// </summary>
		Multi = 1 << 1,

		/// <summary>
		/// Side data depends on the video dimensions. Side data with this property
		/// loses its meaning when rescaling or cropping the image, unless
		/// either recomputed or adjusted to the new resolution
		/// </summary>
		Size_Dependent = 1 << 2,

		/// <summary>
		/// Side data depends on the video color space. Side data with this property
		/// loses its meaning when changing the video color encoding, e.g. by
		/// adapting to a different set of primaries or transfer characteristics
		/// </summary>
		Color_Dependent = 1 << 3,

		/// <summary>
		/// Side data depends on the channel layout. Side data with this property
		/// loses its meaning when downmixing or upmixing, unless either recomputed
		/// or adjusted to the new layout
		/// </summary>
		Channel_Dependent = 1 << 4
	}
}
