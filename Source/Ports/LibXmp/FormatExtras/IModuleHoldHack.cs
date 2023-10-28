/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IModuleHoldHack : IModuleExtra
	{
		void Hold_Hack(c_int pat, c_int chn, c_int row);
	}
}
