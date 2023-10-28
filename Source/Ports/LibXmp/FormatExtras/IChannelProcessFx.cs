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
	internal interface IChannelProcessFx : IChannelExtra
	{
		void Process_Fx(c_int chn, uint8 note, uint8 fxT, uint8 fxP, c_int fNum);
	}
}
