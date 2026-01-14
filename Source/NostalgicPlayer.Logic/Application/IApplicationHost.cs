/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Logic.Application
{
	/// <summary>
	/// Create your own application class that derives from this
	/// which runs your application
	/// </summary>
	public interface IApplicationHost
	{
		/// <summary>
		/// Start the application
		/// </summary>
		void Run(string[] args);
	}
}
