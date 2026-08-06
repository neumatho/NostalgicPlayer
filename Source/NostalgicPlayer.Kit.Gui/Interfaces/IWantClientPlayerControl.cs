/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Gui.Interfaces
{
	/// <summary>
	/// Implement this on a UserControl that wants a reference to the host
	/// client's playback/navigation control surface (IClientPlayerControl).
	/// The host calls SetClientPlayerControl(...) once when adding the control.
	/// </summary>
	public interface IWantClientPlayerControl
	{
		/// <summary>
		/// Called by the host to provide the client player control instance.
		/// </summary>
		void SetClientPlayerControl(IClientPlayerControl clientPlayer);
	}
}
