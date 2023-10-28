/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IInstrumentPlay : IInstrumentExtra
	{
		void Play(Channel_Data xc, c_int chn);
	}
}
