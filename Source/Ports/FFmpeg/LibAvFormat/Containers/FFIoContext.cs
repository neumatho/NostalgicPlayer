/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FFIoContext : AvIoContext, IClearable
	{
		/// <summary>
		/// 
		/// </summary>
		public AvIoContext Pub => this;

		/// <summary>
		/// A callback that is used instead of short_seek_threshold
		/// </summary>
		public FormatFunc.Short_Seek_Get_Delegate Short_Seek_Get;

		/// <summary>
		/// Threshold to favor readahead over seek
		/// </summary>
		public c_int Short_Seek_Threshold;

		/// <summary>
		/// 
		/// </summary>
		public AvIoDataMarkerType Current_Type;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Last_Time;

		/// <summary>
		/// Max filesize, used to limit allocations
		/// </summary>
		public int64_t MaxSize;

		// <summary>
		// Bytes read statistic
		// </summary>
//		public int64_t Bytes_Read;

		// <summary>
		// Bytes written statistic
		// </summary>
//		public int64_t Bytes_Written;

		/// <summary>
		/// Seek statistic
		/// </summary>
		public c_int Seek_Count;

		/// <summary>
		/// Writeout statistic
		/// </summary>
		public c_int WriteOut_Count;

		/// <summary>
		/// Original buffer size
		/// used after probing to ensure seekback and to reset the buffer size
		/// </summary>
		public c_int Orig_Buffer_Size;

		/// <summary>
		/// Written output size
		/// is updated each time a successful writeout ends up further position-wise
		/// </summary>
		public int64_t Written_Output_Size;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Clear()
		{
			base.Clear();

			Short_Seek_Get = null;
			Short_Seek_Threshold = 0;
			Current_Type = AvIoDataMarkerType.Unknown;
			Last_Time = 0;
			MaxSize = 0;
			Bytes_Read = 0;
			Bytes_Written = 0;
			Seek_Count = 0;
			WriteOut_Count = 0;
			Orig_Buffer_Size = 0;
			Written_Output_Size = 0;
		}
	}
}
