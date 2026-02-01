/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvAudioServiceType
	{
		/// <summary>
		/// 
		/// </summary>
		Main = 0,

		/// <summary>
		/// 
		/// </summary>
		Effects = 1,

		/// <summary>
		/// 
		/// </summary>
		Visually_Impaired = 2,

		/// <summary>
		/// 
		/// </summary>
		Hearing_Impaired = 3,

		/// <summary>
		/// 
		/// </summary>
		Dialogue = 4,

		/// <summary>
		/// 
		/// </summary>
		Commentary = 5,

		/// <summary>
		/// 
		/// </summary>
		Emergency = 6,

		/// <summary>
		/// 
		/// </summary>
		Voice_Over = 7,

		/// <summary>
		/// 
		/// </summary>
		Karaoke = 8,

		/// <summary>
		/// Not part of ABI
		/// </summary>
		Nb
	}
}
