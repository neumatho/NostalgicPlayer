/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public static class AvProbe
	{
		/// <summary>
		/// 
		/// </summary>
		public const c_int Score_Retry = Score_Max / 4;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Score_Stream_Retry = (Score_Max / 4) - 1;

		/// <summary>
		/// Score for file extension
		/// </summary>
		public const c_int Score_Extension = 50;

		/// <summary>
		/// Score added for matching mime type
		/// </summary>
		public const c_int Score_Mime_Bonus = 30;

		/// <summary>
		/// Maximum score
		/// </summary>
		public const c_int Score_Max = 100;

		/// <summary>
		/// Extra allocated bytes at the end of the probe buffer
		/// </summary>
		public const c_int Padding_Size = 32;
	}
}
