/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.GuiKit.Components;
using Polycode.NostalgicPlayer.GuiKit.Extensions;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	/// <summary>
	/// Common control used by the agent pages
	/// </summary>
	public partial class AgentsListUserControl : UserControl
	{
		/// <summary>
		/// Holds the needed information for each agent
		/// </summary>
		protected class AgentListInfo
		{
			/// <summary>
			/// The ID to use that make the agent unique
			/// </summary>
			public Guid Id;

			/// <summary>
			/// The name of the agent
			/// </summary>
			public string Name;

			/// <summary>
			/// Some description of the agent
			/// </summary>
			public string Description;

			/// <summary>
			/// Extra agent information
			/// </summary>
			public AgentInfo AgentInfo;
		}

		private MainWindowForm mainWin;

		/// <summary></summary>
		protected Manager manager;

		private ModuleHandler moduleHandler;

		private SettingsAgentsWindowSettings winSettings;
		private ISettings settings;
		private string settingsPrefix;

		private AgentsListUserControl[] reloadControls;
		private HashSet<Guid> changedEnableStates;

		private Guid[] inUseAgents = null;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected AgentsListUserControl()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				// Add the columns to the agent grid
				agentsDataGridView.Columns.Add(new DataGridViewImageColumn
					{
						Name = string.Empty,
						Resizable = DataGridViewTriState.False,
						SortMode = DataGridViewColumnSortMode.NotSortable,
						DefaultCellStyle = new DataGridViewCellStyle { NullValue = null },
						Width = 20
					});

				agentsDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SETTINGS_AGENTS_AGENTS_COLUMN_NAME,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic
					});

				agentsDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SETTINGS_AGENTS_AGENTS_COLUMN_VERSION,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
					});

				// Add the columns to the description grid
				descriptionDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SETTINGS_AGENTS_DESCRIPTION_COLUMN_DESCRIPTION,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.NotSortable
					});
			}
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the check mark column should be shown or not
		/// </summary>
		/********************************************************************/
		[Category("Layout")]
		[DefaultValue(true)]
		public bool EnableCheckColumn
		{
			get => agentsDataGridView.Columns[0].Visible;

			set => agentsDataGridView.Columns[0].Visible = value;
		}



		/********************************************************************/
		/// <summary>
		/// Will prepare to handle the settings
		/// </summary>
		/********************************************************************/
		public void InitSettings(Manager agentManager, ModuleHandler modHandler, MainWindowForm mainWindow, ISettings userSettings, ISettings windowSettings, string prefix, HashSet<Guid> changedStates, params AgentsListUserControl[] controlsToReload)
		{
			mainWin = mainWindow;

			manager = agentManager;

			moduleHandler = modHandler;

			winSettings = new SettingsAgentsWindowSettings(windowSettings, prefix);
			settings = userSettings;
			settingsPrefix = prefix;

			reloadControls = controlsToReload;
			changedEnableStates = changedStates;
		}



		/********************************************************************/
		/// <summary>
		/// Will read the settings and set all the controls
		/// </summary>
		/********************************************************************/
		public void ReadSettings()
		{
			AddItems();
		}



		/********************************************************************/
		/// <summary>
		/// Will read the window settings
		/// </summary>
		/********************************************************************/
		public void ReadWindowSettings()
		{
			// Setup the agents list
			agentsDataGridView.Columns[0].DisplayIndex = winSettings.AgentsColumn1Pos;
			agentsDataGridView.Columns[1].DisplayIndex = winSettings.AgentsColumn2Pos;
			agentsDataGridView.Columns[2].DisplayIndex = winSettings.AgentsColumn3Pos;

			agentsDataGridView.Columns[1].Width = winSettings.AgentsColumn2Width;
			agentsDataGridView.Columns[2].Width = winSettings.AgentsColumn3Width;

			// Setup description list
			descriptionDataGridView.Columns[0].Width = winSettings.DescriptionColumn1Width;
		}



		/********************************************************************/
		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		/********************************************************************/
		public void WriteSettings()
		{
			if (EnableCheckColumn)
			{
				foreach (AgentListInfo agentListInfo in GetAllAgents().Where(agentListInfo => changedEnableStates.Contains(agentListInfo.AgentInfo.TypeId)))
				{
					settings.SetBoolEntry(settingsPrefix + " Agents", agentListInfo.AgentInfo.TypeId.ToString("D"), agentListInfo.AgentInfo.Enabled);
					changedEnableStates.Remove(agentListInfo.AgentInfo.TypeId);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will store window specific settings
		/// </summary>
		/********************************************************************/
		public void WriteWindowSettings()
		{
			// Remember agents list
			winSettings.AgentsColumn1Pos = agentsDataGridView.Columns[0].DisplayIndex;
			winSettings.AgentsColumn2Pos = agentsDataGridView.Columns[1].DisplayIndex;
			winSettings.AgentsColumn3Pos = agentsDataGridView.Columns[2].DisplayIndex;

			winSettings.AgentsColumn2Width = agentsDataGridView.Columns[1].Width;
			winSettings.AgentsColumn3Width = agentsDataGridView.Columns[2].Width;

			winSettings.AgentsSortKey = agentsDataGridView.SortedColumn.Index;
			winSettings.AgentsSortOrder = agentsDataGridView.SortOrder;

			// Remember description list
			winSettings.DescriptionColumn1Width = descriptionDataGridView.Columns[0].Width;
		}



		/********************************************************************/
		/// <summary>
		/// Will restore real-time values
		/// </summary>
		/********************************************************************/
		public void CancelSettings()
		{
			// Look at the changed states and see if they are different from
			// the backed up
			foreach (AgentListInfo agentListInfo in GetAllAgents().Where(agentListInfo => changedEnableStates.Contains(agentListInfo.AgentInfo.TypeId)))
			{
				if (agentListInfo.AgentInfo.Enabled)
					DisableAgent(agentListInfo);
				else
					EnableAgent(agentListInfo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the page when a module is loaded/ejected
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
			void FindAndColorItems(Guid[] agentIds, Color itemColor)
			{
				foreach (Guid id in agentIds)
				{
					// Find the right row
					DataGridViewRow row = agentsDataGridView.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => ((AgentListInfo)r.Tag)?.Id == id);
					if (row != null)
					{
						DataGridViewCellStyle style = row.Cells[1].Style;
						style.ForeColor = itemColor;
						style.SelectionForeColor = itemColor;

						style = row.Cells[2].Style;
						style.ForeColor = itemColor;
						style.SelectionForeColor = itemColor;
					}
				}
			}

			if (inUseAgents != null)
			{
				FindAndColorItems(inUseAgents, Color.Black);
				inUseAgents = null;
			}

			if (moduleHandler.IsModuleLoaded)
			{
				inUseAgents = GetAgentIdsInUse(moduleHandler);
				if (inUseAgents != null)
					FindAndColorItems(inUseAgents, Color.Blue);
			}
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when an agent has been selected
		/// </summary>
		/********************************************************************/
		private void AgentsDataGridView_SelectionChanged(object sender, EventArgs e)
		{
			ShowDescription();

			bool enableSettings = false;
			bool enableDisplay = false;

			DataGridViewSelectedRowCollection selectedRows = agentsDataGridView.SelectedRows;
			if (selectedRows.Count > 0)
			{
				AgentInfo agentInfo = ((AgentListInfo)selectedRows[0].Tag)?.AgentInfo;
				if ((agentInfo != null) && agentInfo.Enabled)
				{
					enableSettings = agentInfo.HasSettings;
					enableDisplay = agentInfo.HasDisplay;
				}
			}

			settingsButton.Enabled = enableSettings;
			displayButton.Enabled = enableDisplay;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user double clicks on a row in the agent view
		/// </summary>
		/********************************************************************/
		private void AgentsDataGridView_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (EnableCheckColumn)
			{
				// Get the agent information
				AgentListInfo agentListInfo = (AgentListInfo)agentsDataGridView.Rows[e.RowIndex].Tag;
				if (agentListInfo != null)
				{
					if (agentListInfo.AgentInfo.Enabled)
					{
						DisableAgent(agentListInfo);
						agentsDataGridView.Rows[e.RowIndex].Cells[0].Value = null;
					}
					else
					{
						EnableAgent(agentListInfo);
						agentsDataGridView.Rows[e.RowIndex].Cells[0].Value = Resources.IDB_CHECKMARK;
					}

					// Update the grid
					agentsDataGridView.InvalidateRow(e.RowIndex);

					// Update other lists if needed
					if (reloadControls != null)
					{
						foreach (AgentsListUserControl listControl in reloadControls)
							listControl.ReloadItems();
					}

					// Update the buttons
					settingsButton.Enabled = agentListInfo.AgentInfo.Enabled && agentListInfo.AgentInfo.HasSettings;
					displayButton.Enabled = agentListInfo.AgentInfo.Enabled && agentListInfo.AgentInfo.HasDisplay;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the description column is resized
		/// </summary>
		/********************************************************************/
		private void DescriptionDataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
		{
			ShowDescription();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user click on the settings button
		/// </summary>
		/********************************************************************/
		private void SettingsButton_Click(object sender, EventArgs e)
		{
			AgentInfo agentInfo = ((AgentListInfo)agentsDataGridView.SelectedRows[0].Tag)?.AgentInfo;
			if (agentInfo != null)
				mainWin.OpenAgentSettingsWindow(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user click on the display button
		/// </summary>
		/********************************************************************/
		private void DisplayButton_Click(object sender, EventArgs e)
		{
			AgentInfo agentInfo = ((AgentListInfo)agentsDataGridView.SelectedRows[0].Tag)?.AgentInfo;
			if (agentInfo != null)
				mainWin.OpenAgentDisplayWindow(agentInfo);
		}
		#endregion

		#region Overridden methods
		/********************************************************************/
		/// <summary>
		/// Will return all agents of the main and extra types
		/// </summary>
		/********************************************************************/
		protected virtual IEnumerable<AgentListInfo> GetAllAgents()
		{
			yield break;
		}



		/********************************************************************/
		/// <summary>
		/// Return the IDs of the agents in use if any
		/// </summary>
		/********************************************************************/
		protected virtual Guid[] GetAgentIdsInUse(ModuleHandler handler)
		{
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the agents to enable/disable for the given agent
		/// </summary>
		/********************************************************************/
		protected virtual IEnumerable<AgentInfo> GetAgentCollection(AgentListInfo agentListInfo)
		{
			yield return agentListInfo.AgentInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Will make some extra closing, if an agent is disabled
		/// </summary>
		/********************************************************************/
		protected virtual void CloseAgent(ModuleHandler handler)
		{
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will reload all the items in the list
		/// </summary>
		/********************************************************************/
		private void ReloadItems()
		{
			AddItems();
		}



		/********************************************************************/
		/// <summary>
		/// Will add all the items in the list
		/// </summary>
		/********************************************************************/
		private void AddItems()
		{
			agentsDataGridView.Rows.Clear();

			foreach (AgentListInfo agentListInfo in GetAllAgents())
			{
				DataGridViewRow row = new DataGridViewRow { Tag = agentListInfo };
				row.Cells.AddRange(new DataGridViewCell[]
				{
					new DataGridViewImageCell { Value = GetAgentCollection(agentListInfo).FirstOrDefault(agentInfo => agentInfo.Enabled) != null ? Resources.IDB_CHECKMARK : null },
					new KryptonDataGridViewTextBoxCell { Value = agentListInfo.Name },
					new KryptonDataGridViewTextBoxCell { Value = agentListInfo.AgentInfo.Version.ToString(3) }
				});

				agentsDataGridView.Rows.Add(row);
			}

			// Sort the list
			agentsDataGridView.Sort(agentsDataGridView.Columns[winSettings.AgentsSortKey], Enum.Parse<ListSortDirection>(winSettings.AgentsSortOrder.ToString()));

			// Resize the rows, so the lines are compacted
			agentsDataGridView.AutoResizeRows();
		}



		/********************************************************************/
		/// <summary>
		/// Will show the description in the description view
		/// </summary>
		/********************************************************************/
		private void ShowDescription()
		{
			// Remove any old description
			descriptionDataGridView.Rows.Clear();

			DataGridViewSelectedRowCollection selectedRows = agentsDataGridView.SelectedRows;
			if (selectedRows.Count > 0)
			{
				string description = ((AgentListInfo)selectedRows[0].Tag)?.Description;
				int listWidth = descriptionDataGridView.Columns[0].Width;

				foreach (string line in description.SplitIntoLines(descriptionDataGridView.Handle, listWidth, FontPalette.GetRegularFont()))
					descriptionDataGridView.Rows.Add(line);

				// Resize the rows, so the lines are compacted
				descriptionDataGridView.AutoResizeRows();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will enable the agent
		/// </summary>
		/********************************************************************/
		private void EnableAgent(AgentListInfo agentListInfo)
		{
			// First check to see if we need to load the agent.
			// This is checked to see if other types is enabled or not
			// supported by this agent
			if (manager.GetAllTypes(agentListInfo.AgentInfo.AgentId).FirstOrDefault(a => (a.TypeId != agentListInfo.AgentInfo.TypeId) && a.Enabled) == null)
			{
				// Need to load the agent
				manager.LoadAgent(agentListInfo.AgentInfo.AgentId);
			}

			foreach (AgentInfo agentInfo in GetAgentCollection(agentListInfo))
			{
				if (!agentInfo.Enabled)
				{
					// Add any menu items
					mainWin.AddAgentToMenu(agentInfo);

					// And enable the agent
					agentInfo.Enabled = true;

					// Remember the change
					if (changedEnableStates.Contains(agentInfo.TypeId))
						changedEnableStates.Remove(agentInfo.TypeId);
					else
						changedEnableStates.Add(agentInfo.TypeId);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will disable the agent
		/// </summary>
		/********************************************************************/
		private void DisableAgent(AgentListInfo agentListInfo)
		{
			// If the agent is in use, free the playing module
			if ((inUseAgents != null) && inUseAgents.Contains(agentListInfo.Id))
			{
				mainWin.StopModule();
				CloseAgent(moduleHandler);
			}

			foreach (AgentInfo agentInfo in GetAgentCollection(agentListInfo))
			{
				// Close any opened windows
				mainWin.CloseAgentSettingsWindow(agentInfo);
				mainWin.CloseAgentDisplayWindow(agentInfo);

				// Remove any menu items
				mainWin.RemoveAgentFromMenu(agentInfo);

				// Change the enable status
				agentInfo.Enabled = false;

				// Remember the change
				if (changedEnableStates.Contains(agentInfo.TypeId))
					changedEnableStates.Remove(agentInfo.TypeId);
				else
					changedEnableStates.Add(agentInfo.TypeId);
			}

			// Check to see if we need to unload the agent
			if (manager.GetAllTypes(agentListInfo.AgentInfo.AgentId).FirstOrDefault(a => a.Enabled) == null)
			{
				// Unload the agent
				manager.UnloadAgent(agentListInfo.AgentInfo.AgentId);
			}
		}
		#endregion
	}
}
