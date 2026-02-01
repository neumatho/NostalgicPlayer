/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// A single allowed range of values, or a single allowed value
	/// </summary>
	public class AvOptionRange
	{
		/// <summary>
		/// 
		/// </summary>
		public string Str;

		/// <summary>
		/// Value range.
		/// For string ranges this represents the min/max length.
		/// For dimensions this represents the min/max pixel count or width/height in multi-component case
		/// </summary>
		public c_double Value_Min;

		/// <summary>
		/// 
		/// </summary>
		public c_double Value_Max;

		/// <summary>
		/// Value's component range.
		/// For string this represents the unicode range for chars, 0-127 limits to ASCII
		/// </summary>
		public c_double Component_Min;

		/// <summary>
		/// 
		/// </summary>
		public c_double Component_Max;

		/// <summary>
		/// Range flag.
		/// If set to 1 the struct encodes a range, if set to 0 a single value.
		/// </summary>
		public bool Is_Range;
	}
}
