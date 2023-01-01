/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.Extensions.DependencyInjection;
using Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display;
using Polycode.NostalgicPlayer.Kit;
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
			settings = DependencyInjection.GetDefaultProvider().GetService<ISettings>();
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
		public SpeakerOscilloscopeControl.ScopeType ScopeType
		{
			get
			{
				if (Enum.TryParse(settings.GetStringEntry("General", "ScopeType"), out SpeakerOscilloscopeControl.ScopeType type))
					return type;

				return SpeakerOscilloscopeControl.ScopeType.Filled;
			}

			set => settings.SetStringEntry("General", "ScopeType", value.ToString());
		}
	}
}
