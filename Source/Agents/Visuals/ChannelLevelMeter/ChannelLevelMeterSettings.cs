/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Visual.ChannelLevelMeter.Display;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.ChannelLevelMeter
{
	/// <summary>
	/// This class holds the ChannelLevelMeter specific settings
	/// </summary>
	internal class ChannelLevelMeterSettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ChannelLevelMeterSettings()
		{
			settings = DependencyInjection.Container.GetInstance<ISettings>();
			settings.LoadSettings("ChannelLevelMeter");
		}



		/********************************************************************/
		/// <summary>
		/// Return the main settings object
		/// </summary>
		/********************************************************************/
		public ISettings Settings => settings;



		/********************************************************************/
		/// <summary>
		/// Orientation (Horizontal/Vertical)
		/// </summary>
		/********************************************************************/
		public Orientation Orientation
		{
			get => (Orientation)settings.GetIntEntry("ChannelLevelMeter", "Orientation", (int)Orientation.Horizontal);
			set
			{
				settings.SetIntEntry("ChannelLevelMeter", "Orientation", (int)value);
			}
		}
	}
}
