/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Containers;
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope
{
	/// <summary>
	/// This class holds all the settings
	/// </summary>
	internal class OscilloscopeSettings
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OscilloscopeSettings()
		{
			settings = DependencyInjection.Container.GetInstance<ISettings>();
			settings.LoadSettings("Oscilloscope");
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
		public ScopeType ScopeType
		{
			get
			{
				if (Enum.TryParse(settings.GetStringEntry("General", "ScopeType"), out ScopeType type))
					return type;

				return ScopeType.Filled;
			}

			set => settings.SetStringEntry("General", "ScopeType", value.ToString());
		}
	}
}
