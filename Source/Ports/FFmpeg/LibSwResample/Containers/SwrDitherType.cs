/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// Dithering algorithms
	/// </summary>
	public enum SwrDitherType
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// 
		/// </summary>
		Rectangular,

		/// <summary>
		/// 
		/// </summary>
		Triangular,

		/// <summary>
		/// 
		/// </summary>
		Triangular_HighPass,

		/// <summary>
		/// Not part of API/ABI
		/// </summary>
		Ns = 64,

		/// <summary>
		/// 
		/// </summary>
		Ns_Lipshitz,

		/// <summary>
		/// 
		/// </summary>
		Ns_F_Weighted,

		/// <summary>
		/// 
		/// </summary>
		Ns_Modified_E_Weighted,

		/// <summary>
		/// 
		/// </summary>
		Ns_Improved_E_Weighted,

		/// <summary>
		/// 
		/// </summary>
		Ns_Shibata,

		/// <summary>
		/// 
		/// </summary>
		Ns_Low_Shibata,

		/// <summary>
		/// 
		/// </summary>
		Ns_High_Shibata,

		/// <summary>
		/// Not part of API/ABI
		/// </summary>
		Nb
	}
}
