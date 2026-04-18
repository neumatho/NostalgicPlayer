/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Contains all the images used in NostalgicPlayer
	/// </summary>
	public interface INostalgicImageBank
	{
		/// <summary>
		/// Holds all the images needed by the form
		/// </summary>
		internal IFormImages Form { get; }

		/// <summary>
		/// Holds all the images needed by the Main window
		/// </summary>
		IMainImages Main { get; }

		/// <summary>
		/// Holds all the images needed by the Module Information window
		/// </summary>
		IModuleInformationImages ModuleInformation { get; }

		/// <summary>
		/// Holds all the images needed by the Sample Information window
		/// </summary>
		ISampleInformationImages SampleInformation { get; }
	}
}
