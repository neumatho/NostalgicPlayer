/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Gui.Interfaces;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AgentWindow
{
	/// <summary>
	/// This shows the settings window for agents
	/// </summary>
	public partial class AgentSettingsWindowForm : WindowFormBase
	{
		private AgentInfo agentInfo;

		private ISettingsControl settingsControl;
		private readonly string helpUrl;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AgentSettingsWindowForm(AgentInfo agentInfo)
		{
			InitializeComponent();

			// Remember the arguments
			this.agentInfo = agentInfo;

			if (!DesignMode)
			{
				IAgentManager agentManager = DependencyInjection.Container.GetInstance<IAgentManager>();

				InitializeWindow();

				// Set the title of the window
				Text = string.Format(Resources.IDS_AGENTSETTINGS_TITLE, agentInfo.TypeName);

				// Create an instance of the agent type and settings window
				if (agentInfo.Agent.CreateInstance(agentInfo.TypeId) is IAgentSettingsRegistrar settingsRegistrar)
				{
					// Find the settings agent
					IAgentGuiSettings guiSettings = agentManager.GetSettingAgent(settingsRegistrar.GetSettingsAgentId()) as IAgentGuiSettings;
					if (guiSettings != null)
					{
						settingsControl = guiSettings.GetSettingsControl();

						// Add it in the group control
						UserControl userControl = settingsControl.GetUserControl();
						userControl.Dock = DockStyle.Fill;

						settingsGroup.Panel.Controls.Add(userControl);

						// Calculate the minimum size of the window
						Size minSize = userControl.MinimumSize;
						Size groupSize = settingsGroup.Size;

						int minWidth = Width - groupSize.Width + minSize.Width;
						int minHeight = Height - groupSize.Height + minSize.Height;

						int width = Math.Max(Width, minWidth);
						int height = Math.Max(Height, minHeight);

						MinimumSize = new Size(minWidth, minHeight);
						Size = new Size(width, height);

						string helpAnchor = guiSettings.HelpAnchor;
						if (!string.IsNullOrEmpty(helpAnchor))
							helpUrl = $"agentsettings.html#{helpAnchor}";
					}
				}

				// Initialize settings
				InitSettings();
			}
		}

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected override string HelpUrl => helpUrl;
		#endregion

		#region Event handlers

		#region Form events
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void AgentSettingsWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (settingsControl != null)
			{
				// Cancel the settings
				CancelSettings();

				// Cleanup
				agentInfo = null;
				settingsControl = null;
			}
		}
		#endregion

		#region Button events
		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked the ok button
		/// </summary>
		/********************************************************************/
		private void OkButton_Click(object sender, EventArgs e)
		{
			// Save the settings and close
			SaveSettings();
			Close();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked the cancel button
		/// </summary>
		/********************************************************************/
		private void CancelButton_Click(object sender, EventArgs e)
		{
			// Just close the window, which will also do a cancel
			Close();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked the apply button
		/// </summary>
		/********************************************************************/
		private void ApplyButton_Click(object sender, EventArgs e)
		{
			// Save the settings
			SaveSettings();
		}
		#endregion

		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will initialize all the pages, so they show the current settings
		/// </summary>
		/********************************************************************/
		private void InitSettings()
		{
			// Make a backup of the settings. This is used for real-time
			// settings, that can be restored back when clicking cancel
			settingsControl.MakeBackup();

			// Then load all the settings
			LoadSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the settings
		/// </summary>
		/********************************************************************/
		private void LoadSettings()
		{
			// Load window settings
			LoadWindowSettings($"{agentInfo.TypeName.Replace(" ", string.Empty)}SettingsWindow");

			// Load all the settings
			settingsControl.ReadSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Save all the settings
		/// </summary>
		/********************************************************************/
		private void SaveSettings()
		{
			// Save all the settings
			settingsControl.WriteSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will restore real-time values
		/// </summary>
		/********************************************************************/
		private void CancelSettings()
		{
			// Cancel all the settings
			settingsControl.CancelSettings();
		}
		#endregion
	}
}
