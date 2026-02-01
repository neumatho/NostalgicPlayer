/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// List of AVOptionRange structs
	/// </summary>
	public class AvOptionRanges
	{
		/// <summary>
		/// Array of option ranges.
		///
		/// Most of option types use just one component.
		/// Following describes multi-component option types:
		///
		/// AV_OPT_TYPE_IMAGE_SIZE:
		/// component index 0: range of pixel count (width * height).
		/// component index 1: range of width.
		/// component index 2: range of height.
		///
		/// Note: To obtain multi-component version of this structure, user must
		///       provide AV_OPT_MULTI_COMPONENT_RANGE to av_opt_query_ranges or
		///       av_opt_query_ranges_default function
		/// </summary>
		public CPointer<AvOptionRange> Range;

		/// <summary>
		/// Number of ranges per component
		/// </summary>
		public c_int Nb_Ranges;

		/// <summary>
		/// Number of components
		/// </summary>
		public c_int Nb_Components;
	}
}
