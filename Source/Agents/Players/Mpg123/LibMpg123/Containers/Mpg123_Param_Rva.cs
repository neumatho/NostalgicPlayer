/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Choices for MPG123_RVA
	/// </summary>
	internal enum Mpg123_Param_Rva
	{
		Rva_Off = 0,					// < RVA disabled (default)
		Rva_Mix = 1,					// < Use mix/track/radio gain
		Rva_Album = 2,					// < Use album/audiophile gain
		Rva_Max = Rva_Album				// < The maximum RVA code, may increase in future
	}
}
