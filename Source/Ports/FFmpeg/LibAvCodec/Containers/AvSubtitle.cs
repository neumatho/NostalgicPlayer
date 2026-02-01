/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvSubtitle : IClearable
	{
		/// <summary>
		/// 0 = graphics
		/// </summary>
		public uint16_t Format;

		/// <summary>
		/// Relative to packet pts, in ms
		/// </summary>
		public uint32_t Start_Display_Time;

		/// <summary>
		/// Relative to packet pts, in ms
		/// </summary>
		public uint32_t End_Display_Time;

		/// <summary>
		/// 
		/// </summary>
		public c_uint Num_Rects;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<AvSubtitleRect> Rects;

		/// <summary>
		/// Same as packet pts, in AV_TIME_BASE
		/// </summary>
		public int64_t Pts;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Format = 0;
			Start_Display_Time = 0;
			End_Display_Time = 0;
			Num_Rects = 0;
			Rects.SetToNull();
			Pts = 0;
		}
	}
}
