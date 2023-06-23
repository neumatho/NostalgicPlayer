/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Choices for MPG123_RVA
	/// </summary>
	public enum Mpg123_Param_Rva
	{
		/// <summary>
		/// RVA disabled (default)
		/// </summary>
		Rva_Off = 0,
		/// <summary>
		/// Use mix/track/radio gain
		/// </summary>
		Rva_Mix = 1,
		/// <summary>
		/// Use album/audiophile gain
		/// </summary>
		Rva_Album = 2,
		/// <summary>
		/// The maximum RVA code, may increase in future
		/// </summary>
		Rva_Max = Rva_Album
	}
}
