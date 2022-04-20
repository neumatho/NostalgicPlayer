/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AgentWindow
{
	/// <summary>
	/// This shows a display window for agents
	/// </summary>
	public partial class AgentDisplayWindowForm : WindowFormBase
	{
		private Manager agentManager;
		private ModuleHandler moduleHandler;

		private IVisualAgent visualAgent;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AgentDisplayWindowForm(Manager agentManager, AgentInfo agentInfo, ModuleHandler moduleHandler, MainWindowForm mainWindow, OptionSettings optionSettings)
		{
			InitializeComponent();

			// Remember the arguments
			this.agentManager = agentManager;
			this.moduleHandler = moduleHandler;

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Set the title of the window
				Text = string.Format(Resources.IDS_AGENTDISPLAY_TITLE, agentInfo.TypeName);

				// Create an instance of the agent type and display window
				IAgentWorker worker = agentInfo.Agent.CreateInstance(agentInfo.TypeId);
				if (worker is IAgentGuiDisplay guiDisplay)
				{
					// Add the control into the form
					UserControl userControl = guiDisplay.GetUserControl();
					userControl.Dock = DockStyle.Fill;

					Controls.Add(userControl);

					// Calculate the minimum size of the window
					Size minSize = userControl.MinimumSize;

					int minWidth = minSize.Width + RealWindowBorders.Size.Width;
					int minHeight = minSize.Height + RealWindowBorders.Size.Height;

					int width = Math.Max(Width, minWidth);
					int height = Math.Max(Height, minHeight);

					MinimumSize = new Size(minWidth, minHeight);

					if ((guiDisplay.Flags & DisplayFlag.StaticWindow) != 0)
					{
						FormBorderStyle = FormBorderStyle.FixedDialog;
						MaximizeBox = false;

						Size = new Size(minWidth, minHeight);
					}
					else
						Size = new Size(width, height);
				}

				visualAgent = worker as IVisualAgent;
				if (visualAgent != null)
				{
					agentManager.RegisterVisualAgent(visualAgent);

					if (moduleHandler.IsModuleLoaded)
						visualAgent.InitVisual(moduleHandler.StaticModuleInformation.Channels, moduleHandler.StaticModuleInformation.VirtualChannels);
				}

				// Load window settings
				LoadWindowSettings($"{agentInfo.TypeName.Replace(" ", string.Empty)}DisplayWindow");
			}
		}

		#region Event handlers

		#region Form events
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void AgentDisplayWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			// Unregister again
			if (visualAgent != null)
			{
				agentManager.UnregisterVisualAgent(visualAgent);

				if (moduleHandler.IsModuleLoaded)
					visualAgent.CleanupVisual();

				visualAgent = null;
			}

			moduleHandler = null;
			agentManager = null;
		}
		#endregion

		#endregion
	}
}
