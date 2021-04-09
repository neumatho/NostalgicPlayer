/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
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
		private MainWindowForm mainWin;

		private Manager manager;
		private Manager.AgentType agentType;
		private Manager.AgentType[] extraAgentTypes;

		private ModuleHandler moduleHandler;

		private SettingsAgentsWindowSettings winSettings;
		private Settings settings;

		private Dictionary<Guid, bool> backupEnableStates;
		private Dictionary<Guid, bool> changedEnableStates;

		private Guid inUseAgent = Guid.Empty;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AgentsListUserControl()
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
			get
			{
				return agentsDataGridView.Columns[0].Visible;
			}

			set
			{
				agentsDataGridView.Columns[0].Visible = value;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will prepare to handle the settings
		/// </summary>
		/********************************************************************/
		public void InitSettings(Manager agentManager, ModuleHandler modHandler, MainWindowForm mainWindow, Settings userSettings, Settings windowSettings, Manager.AgentType type, params Manager.AgentType[] extraTypes)
		{
			mainWin = mainWindow;

			manager = agentManager;
			agentType = type;
			extraAgentTypes = extraTypes;

			moduleHandler = modHandler;

			winSettings = new SettingsAgentsWindowSettings(windowSettings, agentType);
			settings = userSettings;

			backupEnableStates = new Dictionary<Guid, bool>();
			changedEnableStates = new Dictionary<Guid, bool>();
		}



		/********************************************************************/
		/// <summary>
		/// Will make a backup of settings that can be changed in real-time
		/// </summary>
		/********************************************************************/
		public void MakeBackup()
		{
			// Make a backup of the enable state for all the agents
			foreach (AgentInfo agentInfo in GetAllAgents())
				backupEnableStates[agentInfo.TypeId] = agentInfo.Enabled;
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
				foreach (KeyValuePair<Guid, bool> pair in changedEnableStates)
					settings.SetBoolEntry(agentType + " Agents", pair.Key.ToString("D"), pair.Value);
			}

			changedEnableStates.Clear();
			backupEnableStates.Clear();
			MakeBackup();
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
			foreach (KeyValuePair<Guid, bool> pair in changedEnableStates)
			{
				if (backupEnableStates[pair.Key] != pair.Value)
				{
					AgentInfo agentInfo = GetAllAgents().FirstOrDefault(a => a.TypeId == pair.Key);
					if (agentInfo != null)
					{
						if (pair.Value)
							DisableAgent(agentInfo);
						else
							EnableAgent(agentInfo);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the page when a module is loaded/ejected
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
			if ((agentType == Manager.AgentType.Players) || (agentType == Manager.AgentType.Output))
			{
				Color itemColor = moduleHandler.IsModuleLoaded ? Color.Blue : Color.Black;
				Guid typeId = moduleHandler.IsModuleLoaded ? agentType == Manager.AgentType.Players ? moduleHandler.StaticModuleInformation.ConverterAgentInfo != null ? moduleHandler.StaticModuleInformation.ConverterAgentInfo.TypeId : moduleHandler.StaticModuleInformation.PlayerAgentInfo.TypeId : moduleHandler.OutputAgentInfo.TypeId : inUseAgent;

				if (typeId != Guid.Empty)
				{
					// Find the right row
					DataGridViewRow row = agentsDataGridView.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => ((AgentInfo)r.Tag)?.TypeId == typeId);
					if (row != null)
					{
						DataGridViewCellStyle style = row.Cells[1].Style;
						style.ForeColor = itemColor;
						style.SelectionForeColor = itemColor;

						style = row.Cells[2].Style;
						style.ForeColor = itemColor;
						style.SelectionForeColor = itemColor;

						inUseAgent = typeId;
					}
				}

				if (!moduleHandler.IsModuleLoaded)
					inUseAgent = Guid.Empty;
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
				AgentInfo agentInfo = ((AgentInfo)selectedRows[0].Tag);
				if (agentInfo != null)
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
				AgentInfo agentInfo = (AgentInfo)agentsDataGridView.Rows[e.RowIndex].Tag;

				if (agentInfo.Enabled)
				{
					DisableAgent(agentInfo);
					agentsDataGridView.Rows[e.RowIndex].Cells[0].Value = null;
				}
				else
				{
					EnableAgent(agentInfo);
					agentsDataGridView.Rows[e.RowIndex].Cells[0].Value = Resources.IDB_CHECKMARK;
				}

				// Remember the change
				changedEnableStates[agentInfo.TypeId] = agentInfo.Enabled;

				// Update the grid
				agentsDataGridView.InvalidateRow(e.RowIndex);
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
			AgentInfo agentInfo = (AgentInfo)agentsDataGridView.SelectedRows[0].Tag;

			mainWin.OpenAgentSettingsWindow(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user click on the display button
		/// </summary>
		/********************************************************************/
		private void DisplayButton_Click(object sender, EventArgs e)
		{
			AgentInfo agentInfo = (AgentInfo)agentsDataGridView.SelectedRows[0].Tag;

			mainWin.OpenAgentDisplayWindow(agentInfo);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will return all agents of the main and extra types
		/// </summary>
		/********************************************************************/
		private IEnumerable<AgentInfo> GetAllAgents()
		{
			foreach (AgentInfo agentInfo in manager.GetAllAgents(agentType))
				yield return agentInfo;

			if (extraAgentTypes != null)
			{
				foreach (Manager.AgentType type in extraAgentTypes)
				{
					foreach (AgentInfo agentInfo in manager.GetAllAgents(type))
						yield return agentInfo;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will add all the items in the list
		/// </summary>
		/********************************************************************/
		private void AddItems()
		{
			foreach (AgentInfo agentInfo in GetAllAgents())
			{
				DataGridViewRow row = new DataGridViewRow { Tag = agentInfo };
				row.Cells.AddRange(new DataGridViewCell[]
				{
					new DataGridViewImageCell { Value = agentInfo.Enabled ? Resources.IDB_CHECKMARK : null },
					new KryptonDataGridViewTextBoxCell { Value = agentInfo.TypeName },
					new KryptonDataGridViewTextBoxCell { Value = agentInfo.Version.ToString(3) }
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
				string description = ((AgentInfo)selectedRows[0].Tag)?.Description;

				int listWidth = descriptionDataGridView.Columns[0].Width;

				using (Graphics g = Graphics.FromHwnd(descriptionDataGridView.Handle))
				{
					while (!string.IsNullOrEmpty(description))
					{
						string tempStr;

						// See if there is any newlines
						int index = description.IndexOf('\n');
						if (index != -1)
						{
							// There is, get the line
							tempStr = description.Substring(0, index);
							description = description.Substring(index + 1);
						}
						else
						{
							tempStr = description;
							description = string.Empty;
						}

						// Adjust the description line
						tempStr = tempStr.Trim();

						// Just add empty lines
						if (string.IsNullOrEmpty(tempStr))
							descriptionDataGridView.Rows.Add(string.Empty);
						else
						{
							do
							{
								int lineWidth = (int)g.MeasureString(tempStr, descriptionDataGridView.StateCommon.DataCell.Content.Font).Width;

								string tempStr1 = string.Empty;

								while (lineWidth >= listWidth)
								{
									// We need to split the line
									index = tempStr.LastIndexOf(' ');
									if (index != -1)
									{
										// Found a space, check if the line can be showed now
										tempStr1 = tempStr.Substring(index) + tempStr1;
										tempStr = tempStr.Substring(0, index);

										lineWidth = (int)g.MeasureString(tempStr, descriptionDataGridView.StateCommon.DataCell.Content.Font).Width;
									}
									else
									{
										// Well, the line can't be showed and we can't split it :-(
										break;
									}
								}

								// Adjust the description line
								tempStr = tempStr.Trim();

								// Add the line in the grid
								descriptionDataGridView.Rows.Add(tempStr);

								tempStr = tempStr1.Trim();
							}
							while (!string.IsNullOrEmpty(tempStr));
						}
					}
				}

				// Resize the rows, so the lines are compacted
				descriptionDataGridView.AutoResizeRows();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will enable the agent
		/// </summary>
		/********************************************************************/
		private void EnableAgent(AgentInfo agentInfo)
		{
			// First check to see if we need to load the agent.
			// This is checked to see if other types is enabled or not
			// supported by this agent
			if (manager.GetAllTypes(agentInfo.AgentId).FirstOrDefault(a => (a.TypeId != agentInfo.TypeId) && a.Enabled) == null)
			{
				// Need to load the agent
				manager.LoadAgent(agentInfo.AgentId);
			}

			// Add any menu items
			mainWin.AddAgentToMenu(agentInfo);

			// And enable the agent
			agentInfo.Enabled = true;
		}



		/********************************************************************/
		/// <summary>
		/// Will disable the agent
		/// </summary>
		/********************************************************************/
		private void DisableAgent(AgentInfo agentInfo)
		{
			// If the agent is in use, free the playing module
			if (agentInfo.TypeId == inUseAgent)
			{
				mainWin.StopModule();

				if (agentType == Manager.AgentType.Output)
					moduleHandler.CloseOutputAgent();
			}

			// Close any opened windows
			mainWin.CloseAgentSettingsWindow(agentInfo);
			mainWin.CloseAgentDisplayWindow(agentInfo);

			// Remove any menu items
			mainWin.RemoveAgentFromMenu(agentInfo);

			// Change the enable status
			agentInfo.Enabled = false;

			// Check to see if we need to unload the agent
			if (manager.GetAllTypes(agentInfo.AgentId).FirstOrDefault(a => a.Enabled) == null)
			{
				// Unload the agent
				manager.UnloadAgent(agentInfo.AgentId);
			}

			// Update the buttons
			settingsButton.Enabled = false;
			displayButton.Enabled = false;
		}
		#endregion
	}
}
