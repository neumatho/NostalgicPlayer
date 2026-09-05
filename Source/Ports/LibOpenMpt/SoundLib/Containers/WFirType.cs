/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum WFirType : uint8
	{
		/// <summary>
		/// Hann
		/// </summary>
		Hann = 0,

		/// <summary>
		/// Hamming
		/// </summary>
		Hamming = 1,

		/// <summary>
		/// Blackman Exact
		/// </summary>
		BlackmanExact = 2,

		/// <summary>
		/// Blackman 3-Tap 61
		/// </summary>
		Blackman3T61 = 3,

		/// <summary>
		/// Blackman 3-Tap 67
		/// </summary>
		Blackman3T67 = 4,

		/// <summary>
		/// Blackman-Harris
		/// </summary>
		Blackman4T92 = 5,

		/// <summary>
		/// Blackman 4-Tap 74
		/// </summary>
		Blackman4T74 = 6,

		/// <summary>
		/// Kaiser a=7.5
		/// </summary>
		Kaiser4T = 7
	}
}
