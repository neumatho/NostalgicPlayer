/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Gui.Interfaces;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("EE538BB1-9CE3-472D-B030-D46B321A405A")]

namespace Polycode.NostalgicPlayer.Agent.Output.CoreAudioSettings
{
	/// <summary>
	/// NostalgicPlayer GUI settings interface implementation
	/// </summary>
	public class CoreAudioGuiSettings : IAgentGuiSettings
	{
		#region IAgentGuiSettings implementation
		/********************************************************************/
		/// <summary>
		/// Tells which version of NostalgicPlayer this agent is compiled
		/// against
		/// </summary>
		/********************************************************************/
		public int NostalgicPlayerVersion => IAgent.NostalgicPlayer_Current_Version;



		/********************************************************************/
		/// <summary>
		/// Returns an unique ID for this setting agent
		/// </summary>
		/********************************************************************/
		public virtual Guid SettingAgentId => new Guid(Assembly.GetAssembly(GetType()).GetCustomAttribute<GuidAttribute>().Value);



		/********************************************************************/
		/// <summary>
		/// Return a new instance of the settings control
		/// </summary>
		/********************************************************************/
		public ISettingsControl GetSettingsControl()
		{
			return new SettingsControl();
		}



		/********************************************************************/
		/// <summary>
		/// Return the anchor name on the help page or null if none exists
		/// </summary>
		/********************************************************************/
		public string HelpAnchor => "coreaudio";
		#endregion
	}
}
