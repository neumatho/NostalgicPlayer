/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Output.CoreAudio
{
	/// <summary>
	/// This class holds all the settings
	/// </summary>
	public class CoreAudioSettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CoreAudioSettings()
		{
			settings = DependencyInjection.Container.GetInstance<ISettings>();
			settings.LoadSettings("CoreAudio");

			// Remove obsolete settings
			settings.RemoveEntry("General", "Latency");
		}



		/********************************************************************/
		/// <summary>
		/// Return the main settings object
		/// </summary>
		/********************************************************************/
		public ISettings Settings => settings;



		/********************************************************************/
		/// <summary>
		/// Which output device to use
		/// </summary>
		/********************************************************************/
		public string OutputDevice
		{
			get => settings.GetStringEntry("General", "OutputDevice", string.Empty);

			set => settings.SetStringEntry("General", "OutputDevice", value);
		}
	}
}
