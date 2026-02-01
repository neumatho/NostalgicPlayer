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
	public enum AvOptSearch
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Search in possible children of the given object first
		/// </summary>
		Search_Children = 1 << 0,

		/// <summary>
		/// The obj passed to av_opt_find() is fake -- only a double pointer to AVClass
		/// instead of a required pointer to a struct containing AVClass. This is
		/// useful for searching for options without needing to allocate the corresponding
		/// object
		/// </summary>
		Search_Fake_Obj = 1 << 1,

		/// <summary>
		/// In av_opt_get, return NULL if the option has a pointer type and is set to NULL,
		/// rather than returning an empty string
		/// </summary>
		Allow_Null = 1 << 2,

		/// <summary>
		/// May be used with av_opt_set_array() to signal that new elements should
		/// replace the existing ones in the indicated range
		/// </summary>
		Array_Replace = 1 << 3,

		/// <summary>
		/// Allows av_opt_query_ranges and av_opt_query_ranges_default to return more than
		/// one component for certain option types.
		/// See AVOptionRanges for details
		/// </summary>
		Multi_Component_Range = 1 << 12
	}
}
