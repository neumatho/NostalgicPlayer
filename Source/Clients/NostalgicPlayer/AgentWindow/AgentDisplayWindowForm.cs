/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
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

		private IVisualAgent visualAgent;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AgentDisplayWindowForm(Manager agentManager, AgentInfo agentInfo, ModuleHandler moduleHandler)
		{
			InitializeComponent();

			// Remember the arguments
			this.agentManager = agentManager;

			if (!DesignMode)
			{
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

					int width = Math.Max(Width, minSize.Width);
					int height = Math.Max(Height, minSize.Height);

					Size = new Size(width, height);
					MinimumSize = new Size(width, height);
				}

				visualAgent = worker as IVisualAgent;
				if (visualAgent != null)
				{
					agentManager.RegisterVisualAgent(visualAgent);

					if (moduleHandler.IsPlaying)
						visualAgent.InitVisual(moduleHandler.StaticModuleInformation.Channels);
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
				visualAgent = null;
			}

			agentManager = null;
		}
		#endregion

		#endregion
	}
}
