/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvChapter
	{
		/// <summary>
		/// Unique ID to identify the chapter
		/// </summary>
		public int64_t Id;

		/// <summary>
		/// Time base in which the start/end timestamps are specified
		/// </summary>
		public AvRational Time_Base;

		/// <summary>
		/// Chapter start/end time in time_base units
		/// </summary>
		public int64_t Start;

		/// <summary>
		/// 
		/// </summary>
		public int64_t End;

		/// <summary>
		/// 
		/// </summary>
		public AvDictionary Metadata;
	}
}
