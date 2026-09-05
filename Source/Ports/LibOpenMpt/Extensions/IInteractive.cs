/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	public interface IInteractive : IExtension
	{
		/// <summary>
		/// 
		/// </summary>
		void Set_Channel_Mute_Status(int32_t channel, bool mute);
	}
}
