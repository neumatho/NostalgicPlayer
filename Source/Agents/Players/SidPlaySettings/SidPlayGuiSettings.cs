/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("AEC0DC9F-7854-40AD-9F30-DD87D374E996")]

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlaySettings
{
	/// <summary>
	/// NostalgicPlayer GUI settings interface implementation
	/// </summary>
	public class SidPlayGuiSettings : IAgentGuiSettings
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
		#endregion
	}
}
