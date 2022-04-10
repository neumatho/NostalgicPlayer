/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.GuiKit.Controls;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SampleInfoWindow
{
	/// <summary>
	/// This shows the sample information window
	/// </summary>
	public partial class SampleInfoWindowForm : WindowFormBase
	{
		private ModuleHandler moduleHandler;

		private readonly SampleInfoWindowSettings settings;

		private readonly Dictionary<int, Bitmap> combinedImages = new Dictionary<int, Bitmap>();

		private bool doNotUpdateAutoSelection;

		/// <summary></summary>
		public const int PolyphonyChannels = 3;

		private int samplePlayOctave;
		private bool samplePlayPolyEnabled;
		private bool samplePlayLoops;
		private int samplePlayKeyCount;
		private readonly Keys[] samplePlayKeys;

		private readonly Queue<PlaySampleInfo> samplePlayQueue;

		private SaveFileDialog sampleFileDialog;

		private static readonly int[] keyTab =
		{
			-1, -1, -1, 13, 15, -1, 18, 20, 22, -1, 25, 27, -1, 30, -1, -1,
			12, 14, 16, 17, 19, 21, 23, 24, 26, 28, 29, 31, -1, -1, -1,  1,
			 3, -1,  6,  8, 10, -1, 13, 15, -1, -1, -1, -1,  0,  2,  4,  5,
			 7,  9, 11, 12, 14, 16
		};

		private const int Page_Instruments = 0;
		private const int Page_Samples = 1;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SampleInfoWindowForm(Manager agentManager, ModuleHandler moduleHandler, MainWindowForm mainWindow, OptionSettings optionSettings)
		{
			InitializeComponent();

			// Remember the arguments
			this.moduleHandler = moduleHandler;

			// Initialize sample play
			samplePlayOctave = 48;
			samplePlayPolyEnabled = false;
			samplePlayKeyCount = 1;
			samplePlayKeys = new Keys[PolyphonyChannels];
			Array.Fill(samplePlayKeys, Keys.None);

			samplePlayQueue = new Queue<PlaySampleInfo>();

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("SampleInfoWindow");
				settings = new SampleInfoWindowSettings(allWindowSettings);

				// Set the title of the window
				Text = Resources.IDS_SAMPLE_INFO_TITLE;

				// Set the tab titles
				navigator.Pages[Page_Instruments].Text = Resources.IDS_SAMPLE_INFO_TAB_INSTRUMENT;
				navigator.Pages[Page_Samples].Text = Resources.IDS_SAMPLE_INFO_TAB_SAMPLE;

				// Select the last used tab
				navigator.SelectedIndex = settings.AutoSelectTab;

				// Add the columns to the instrument grid
				instrumentDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = "#",
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.InstColumn1Width,
						DisplayIndex = settings.InstColumn1Pos
					});

				instrumentDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_INST_COLUMN_NAME,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						Width = settings.InstColumn2Width,
						DisplayIndex = settings.InstColumn2Pos
					});

				instrumentDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_INST_COLUMN_SAMPLENUM,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.InstColumn3Width,
						DisplayIndex = settings.InstColumn3Pos
					});

				instrumentDataGridView.Sort(instrumentDataGridView.Columns[settings.InstSortKey], Enum.Parse<ListSortDirection>(settings.InstSortOrder.ToString()));

				// Add the columns to the sample grid
				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = "#",
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn1Width,
						DisplayIndex = settings.SampColumn1Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_NAME,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						Width = settings.SampColumn2Width,
						DisplayIndex = settings.SampColumn2Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_LENGTH,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn3Width,
						DisplayIndex = settings.SampColumn3Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_LOOPSTART,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn4Width,
						DisplayIndex = settings.SampColumn4Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_LOOPLENGTH,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn5Width,
						DisplayIndex = settings.SampColumn5Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_BITSIZE,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn6Width,
						DisplayIndex = settings.SampColumn6Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_VOLUME,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn7Width,
						DisplayIndex = settings.SampColumn7Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_PANNING,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn8Width,
						DisplayIndex = settings.SampColumn8Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_MIDDLEC,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn9Width,
						DisplayIndex = settings.SampColumn9Pos
					});

				sampleDataGridView.Columns.Add(new DataGridViewImageColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_INFO,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.NotSortable,
						DefaultCellStyle = new DataGridViewCellStyle { NullValue = null },
						Width = settings.SampColumn10Width,
						DisplayIndex = settings.SampColumn10Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_TYPE,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						Width = settings.SampColumn11Width,
						DisplayIndex = settings.SampColumn11Pos
					});

				sampleDataGridView.Sort(sampleDataGridView.Columns[settings.SampSortKey], Enum.Parse<ListSortDirection>(settings.SampSortOrder.ToString()));

				// Add the sample converters to the format list
				Guid saveFormat = settings.SampleSaveFormat;

				foreach (AgentInfo agentInfo in agentManager.GetAllAgents(Manager.AgentType.SampleConverters))
				{
					KryptonListItem listItem = new KryptonListItem(agentInfo.TypeName);
					listItem.Tag = agentInfo;

					int index = saveFormatComboBox.Items.Add(listItem);

					if (agentInfo.TypeId == saveFormat)
						saveFormatComboBox.SelectedIndex = index;
				}

				// Make sure that the content is up-to date
				ChangeOctave(samplePlayOctave / 12);
				IsPolyphonyEnabled = false;

				RefreshWindow();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will clear the window and add all the items again
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
			if (moduleHandler != null)
			{
				// Remove all the items from the lists
				RemoveInstrumentItems();
				RemoveSampleItems();

				// Now add the items
				doNotUpdateAutoSelection = true;

				AddInstrumentItems();
				AddSampleItems();

				doNotUpdateAutoSelection = false;
			}
		}

		#region Sample playing
		/********************************************************************/
		/// <summary>
		/// Processes the raw scan code to find the note to play
		/// </summary>
		/********************************************************************/
		public bool ProcessKey(int scanCode, Keys key)
		{
			if (samplePlayKeyCount > 0)
				return PlaySample(scanCode, key);

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Checks the queue to see if there is any samples waiting to be
		/// played and if so, removes it from the queue and return it
		/// </summary>
		/********************************************************************/
		public PlaySampleInfo GetNextSampleFromQueue()
		{
			PlaySampleInfo playInfo = null;

			lock (samplePlayQueue)
			{
				// Check to see if there is any items in the list
				if (samplePlayQueue.Count > 0)
				{
					// Get the item and remove it
					playInfo = samplePlayQueue.Dequeue();
				}
			}

			return playInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Tells whether polyphony is enabled or not
		/// </summary>
		/********************************************************************/
		public bool IsPolyphonyEnabled
		{
			get
			{
				return samplePlayPolyEnabled;
			}

			private set
			{
				samplePlayPolyEnabled = value;
				samplePlayKeyCount = value ? PolyphonyChannels : 1;

				polyphonyLabel.Text = string.Format(Resources.IDS_SAMPLE_INFO_SAMP_POLYPHONY, value ? Resources.IDS_SAMPLE_INFO_SAMP_ON : Resources.IDS_SAMPLE_INFO_SAMP_OFF);
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Is called when the form is closed
		/// </summary>
		/********************************************************************/
		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			sampleFileDialog?.Dispose();
			sampleFileDialog = null;

			base.OnFormClosed(e);
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void SampleInfoWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (moduleHandler != null)
			{
				// Save the settings
				settings.InstColumn1Width = instrumentDataGridView.Columns[0].Width;
				settings.InstColumn1Pos = instrumentDataGridView.Columns[0].DisplayIndex;

				settings.InstColumn2Width = instrumentDataGridView.Columns[1].Width;
				settings.InstColumn2Pos = instrumentDataGridView.Columns[1].DisplayIndex;

				settings.InstColumn3Width = instrumentDataGridView.Columns[2].Width;
				settings.InstColumn3Pos = instrumentDataGridView.Columns[2].DisplayIndex;

				settings.InstSortKey = instrumentDataGridView.SortedColumn.Index;
				settings.InstSortOrder = instrumentDataGridView.SortOrder;

				settings.SampColumn1Width = sampleDataGridView.Columns[0].Width;
				settings.SampColumn1Pos = sampleDataGridView.Columns[0].DisplayIndex;

				settings.SampColumn2Width = sampleDataGridView.Columns[1].Width;
				settings.SampColumn2Pos = sampleDataGridView.Columns[1].DisplayIndex;

				settings.SampColumn3Width = sampleDataGridView.Columns[2].Width;
				settings.SampColumn3Pos = sampleDataGridView.Columns[2].DisplayIndex;

				settings.SampColumn4Width = sampleDataGridView.Columns[3].Width;
				settings.SampColumn4Pos = sampleDataGridView.Columns[3].DisplayIndex;

				settings.SampColumn5Width = sampleDataGridView.Columns[4].Width;
				settings.SampColumn5Pos = sampleDataGridView.Columns[4].DisplayIndex;

				settings.SampColumn6Width = sampleDataGridView.Columns[5].Width;
				settings.SampColumn6Pos = sampleDataGridView.Columns[5].DisplayIndex;

				settings.SampColumn7Width = sampleDataGridView.Columns[6].Width;
				settings.SampColumn7Pos = sampleDataGridView.Columns[6].DisplayIndex;

				settings.SampColumn8Width = sampleDataGridView.Columns[7].Width;
				settings.SampColumn8Pos = sampleDataGridView.Columns[7].DisplayIndex;

				settings.SampColumn9Width = sampleDataGridView.Columns[8].Width;
				settings.SampColumn9Pos = sampleDataGridView.Columns[8].DisplayIndex;

				settings.SampColumn10Width = sampleDataGridView.Columns[9].Width;
				settings.SampColumn10Pos = sampleDataGridView.Columns[9].DisplayIndex;

				settings.SampColumn11Width = sampleDataGridView.Columns[10].Width;
				settings.SampColumn11Pos = sampleDataGridView.Columns[10].DisplayIndex;

				settings.SampSortKey = sampleDataGridView.SortedColumn.Index;
				settings.SampSortOrder = sampleDataGridView.SortOrder;

				// Cleanup
				moduleHandler = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a tab is selected
		/// </summary>
		/********************************************************************/
		private void Navigator_SelectedPageChanged(object sender, EventArgs e)
		{
			if (!doNotUpdateAutoSelection)
				settings.AutoSelectTab = navigator.SelectedIndex;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is hold down in the sample list
		/// </summary>
		/********************************************************************/
		private void SamplesDataGridView_KeyDown(object sender, KeyEventArgs e)
		{
			// Check for different keyboard shortcuts
			Keys modifiers = e.Modifiers & Keys.Modifiers;
			Keys key = e.KeyData & Keys.KeyCode;

			if (modifiers == Keys.None)
			{
				switch (key)
				{
					// Delete - Turn on/off polyphony
					case Keys.Delete:
					{
						IsPolyphonyEnabled = !IsPolyphonyEnabled;
						e.Handled = true;
						break;
					}

					// Space - Panic button. Mute all samples
					case Keys.Space:
					{
						// Add an empty sample
						AddSampleToQueue(null, -1);

						// Clear up the key variables
						if (IsPolyphonyEnabled)
						{
							samplePlayKeyCount = PolyphonyChannels;
							for (int i = 0; i < PolyphonyChannels; i++)
								samplePlayKeys[i] = Keys.None;
						}
						else
							samplePlayKeyCount = 1;

						e.Handled = true;
						break;
					}

					// F1 - Switch octave
					case Keys.F1:
					{
						samplePlayOctave = 0;
						ChangeOctave(0);
						e.Handled = true;
						break;
					}

					// F2 - Switch octave
					case Keys.F2:
					{
						samplePlayOctave = 12;
						ChangeOctave(1);
						e.Handled = true;
						break;
					}

					// F3 - Switch octave
					case Keys.F3:
					{
						samplePlayOctave = 24;
						ChangeOctave(2);
						e.Handled = true;
						break;
					}

					// F4 - Switch octave
					case Keys.F4:
					{
						samplePlayOctave = 36;
						ChangeOctave(3);
						e.Handled = true;
						break;
					}

					// F5 - Switch octave
					case Keys.F5:
					{
						samplePlayOctave = 48;
						ChangeOctave(4);
						e.Handled = true;
						break;
					}

					// F6 - Switch octave
					case Keys.F6:
					{
						samplePlayOctave = 60;
						ChangeOctave(5);
						e.Handled = true;
						break;
					}

					// F7 - Switch octave
					case Keys.F7:
					{
						samplePlayOctave = 72;
						ChangeOctave(6);
						e.Handled = true;
						break;
					}

					// F8 - Switch octave
					case Keys.F8:
					{
						samplePlayOctave = 84;
						ChangeOctave(7);
						e.Handled = true;
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is released in the sample list
		/// </summary>
		/********************************************************************/
		private void SamplesDataGridView_KeyUp(object sender, KeyEventArgs e)
		{
			Keys key = e.KeyCode & Keys.KeyCode;

			// Try to find the key in the key array
			for (int i = 0; i < PolyphonyChannels; i++)
			{
				if (samplePlayKeys[i] == key)
				{
					// One more key can be pressed
					samplePlayKeys[i] = Keys.None;
					samplePlayKeyCount++;

					// Add an empty sample if the sample loops
					if (samplePlayLoops)
						AddSampleToQueue(null, -1);

					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a sample has been selected
		/// </summary>
		/********************************************************************/
		private void SampleDataGridView_SelectionChanged(object sender, EventArgs e)
		{
			EnableSaveButton();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a save format has changed
		/// </summary>
		/********************************************************************/
		private void SaveFormatComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnableSaveButton();

			settings.SampleSaveFormat = ((AgentInfo)((KryptonListItem)saveFormatComboBox.SelectedItem).Tag).TypeId;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the save button is clicked
		/// </summary>
		/********************************************************************/
		private void SaveButton_Click(object sender, EventArgs e)
		{
			DataGridViewSelectedRowCollection selectedRows = sampleDataGridView.SelectedRows;
			if (selectedRows.Count > 0)
			{
				if (selectedRows[0].Tag is SampleInfo sampleInfo)
				{
					if (sampleInfo.Length == 0)
					{
						ShowSimpleErrorMessage(Resources.IDS_ERR_SAMPLE_INFO_SAMP_NOLENGTH);
						return;
					}

					if (sampleInfo.Type != SampleInfo.SampleType.Sample)
					{
						ShowSimpleErrorMessage(Resources.IDS_ERR_SAMPLE_INFO_SAMP_NOTASAMPLE);
						return;
					}

					// Create the dialog if not already created
					if (sampleFileDialog == null)
						sampleFileDialog = new SaveFileDialog();

					sampleFileDialog.FileName = sampleInfo.Name.Trim();

					DialogResult result = sampleFileDialog.ShowDialog();
					if (result == DialogResult.OK)
						SaveSample(sampleInfo, sampleFileDialog.FileName);
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will show an error message to the user
		/// </summary>
		/********************************************************************/
		private void ShowSimpleErrorMessage(string message)
		{
			using (CustomMessageBox dialog = new CustomMessageBox(message, Resources.IDS_MAIN_TITLE, CustomMessageBox.IconType.Error))
			{
				dialog.AddButton(Resources.IDS_BUT_OK, 'O');
				dialog.ShowDialog(this);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve all the instruments from the player
		/// </summary>
		/********************************************************************/
		private void AddInstrumentItems()
		{
			ModuleInfoStatic staticInfo = moduleHandler.StaticModuleInformation;

			if (staticInfo.Instruments != null)
			{
				for (int i = 0, cnt = staticInfo.Instruments.Length; i < cnt; i++)
				{
					InstrumentInfo instrument = staticInfo.Instruments[i];

					// Find the number of samples used in the instrument
					HashSet<int> sampleUsed = new HashSet<int>();

					for (int o = 0; o < InstrumentInfo.Octaves; o++)
					{
						for (int n = 0; n < InstrumentInfo.NotesPerOctave; n++)
						{
							int sample = instrument.Notes[o, n];
							if (sample >= 0)
								sampleUsed.Add(sample);
						}
					}

					DataGridViewRow row = new DataGridViewRow();
					row.Cells.AddRange(new DataGridViewCell[]
					{
						new KryptonDataGridViewTextBoxCell { Value = i + 1 },
						new KryptonDataGridViewTextBoxCell { Value = instrument.Name, ToolTipText = instrument.Name },
						new KryptonDataGridViewTextBoxCell { Value = sampleUsed.Count }
					});

					instrumentDataGridView.Rows.Add(row);
				}

				// Sort the items
				instrumentDataGridView.Sort(instrumentDataGridView.SortedColumn, Enum.Parse<ListSortDirection>(instrumentDataGridView.SortOrder.ToString()));

				// Resize the rows, so the lines are compacted
				instrumentDataGridView.AutoResizeRows();

				// Show tab if auto-selected
				navigator.Pages[Page_Instruments].Visible = true;

				if (settings.AutoSelectTab == Page_Instruments)
					navigator.SelectedIndex = Page_Instruments;
			}
			else
				navigator.Pages[Page_Instruments].Visible = false;
		}



		/********************************************************************/
		/// <summary>
		/// Remove all the instrument items
		/// </summary>
		/********************************************************************/
		private void RemoveInstrumentItems()
		{
			instrumentDataGridView.Rows.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve all the samples from the player
		/// </summary>
		/********************************************************************/
		private void AddSampleItems()
		{
			ModuleInfoStatic staticInfo = moduleHandler.StaticModuleInformation;

			if (staticInfo.Samples != null)
			{
				for (int i = 0, cnt = staticInfo.Samples.Length; i < cnt; i++)
				{
					SampleInfo sample = staticInfo.Samples[i];

					Bitmap bitmap = null;

					if ((sample.Flags & SampleInfo.SampleFlags.MultiOctave) != 0)
						bitmap = AppendBitmap(i, bitmap, Resources.IDB_SAMPLE_OCTAVES);

					if ((sample.Flags & SampleInfo.SampleFlags.Stereo) != 0)
						bitmap = AppendBitmap(i, bitmap, Resources.IDB_SAMPLE_STEREO);

					if ((sample.Flags & SampleInfo.SampleFlags.Loop) != 0)
						bitmap = AppendBitmap(i, bitmap, Resources.IDB_SAMPLE_LOOP);

					if ((sample.Flags & SampleInfo.SampleFlags.PingPong) != 0)
						bitmap = AppendBitmap(i, bitmap, Resources.IDB_SAMPLE_PINGPONG);

					DataGridViewRow row = new DataGridViewRow();
					row.Cells.AddRange(new DataGridViewCell[]
					{
						new KryptonDataGridViewTextBoxCell { Value = i + 1 },
						new KryptonDataGridViewTextBoxCell { Value = sample.Name, ToolTipText = sample.Name },
						new KryptonDataGridViewTextBoxCell { Value = sample.Length },
						new KryptonDataGridViewTextBoxCell { Value = sample.LoopStart },
						new KryptonDataGridViewTextBoxCell { Value = sample.LoopLength },
						new KryptonDataGridViewTextBoxCell { Value = sample.BitSize },
						new KryptonDataGridViewTextBoxCell { Value = sample.Volume },
						new KryptonDataGridViewTextBoxCell { Value = sample.Panning == -1 ? "-" : sample.Panning },
						new KryptonDataGridViewTextBoxCell { Value = sample.MiddleC },
						new DataGridViewImageCell { Value =  bitmap },
						new KryptonDataGridViewTextBoxCell { Value = GetTypeString(sample.Type) }
					});

					row.Tag = sample;

					sampleDataGridView.Rows.Add(row);
				}

				// Sort the items
				sampleDataGridView.Sort(sampleDataGridView.SortedColumn, Enum.Parse<ListSortDirection>(sampleDataGridView.SortOrder.ToString()));

				// Resize the rows, so the lines are compacted
				sampleDataGridView.AutoResizeRows();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Remove all the sample items
		/// </summary>
		/********************************************************************/
		private void RemoveSampleItems()
		{
			sampleDataGridView.Rows.Clear();

			foreach (Bitmap bitmap in combinedImages.Values)
				bitmap.Dispose();

			combinedImages.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Returns a string with the type of the sample
		/// </summary>
		/********************************************************************/
		private string GetTypeString(SampleInfo.SampleType type)
		{
			switch (type)
			{
				case SampleInfo.SampleType.Sample:
					return Resources.IDS_SAMPLE_INFO_SAMP_TYPE_SAMPLE;

				case SampleInfo.SampleType.Synth:
					return Resources.IDS_SAMPLE_INFO_SAMP_TYPE_SYNTH;

				case SampleInfo.SampleType.Hybrid:
					return Resources.IDS_SAMPLE_INFO_SAMP_TYPE_HYBRID;
			}

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a string with the type of the sample
		/// </summary>
		/********************************************************************/
		private Bitmap AppendBitmap(int index, Bitmap previousBitmap, Bitmap appendBitmap)
		{
			if (previousBitmap == null)
				return appendBitmap;

			int padding = 2;
			int newWidth = previousBitmap.Width + padding + appendBitmap.Width;
			int newHeight = Math.Max(previousBitmap.Height, appendBitmap.Height);

			// Create new bitmap and draw the two into it
			Bitmap newBitmap = new Bitmap(newWidth, newHeight);

			using (Graphics g = Graphics.FromImage(newBitmap))
			{
				g.DrawImage(previousBitmap, new Rectangle(new Point(0, 0), previousBitmap.Size), new Rectangle(new Point(0, 0), previousBitmap.Size), GraphicsUnit.Pixel);
				g.DrawImage(appendBitmap, new Rectangle(new Point(previousBitmap.Width + padding, 0), appendBitmap.Size), new Rectangle(new Point(0, 0), appendBitmap.Size), GraphicsUnit.Pixel);
			}

			// Remember it, so it will be disposed later on
			if (combinedImages.TryGetValue(index, out Bitmap existingBitmap))
				existingBitmap.Dispose();

			combinedImages[index] = newBitmap;

			return newBitmap;
		}



		/********************************************************************/
		/// <summary>
		/// Change the current octave
		/// </summary>
		/********************************************************************/
		private void ChangeOctave(int octave)
		{
			octaveLabel.Text = string.Format(Resources.IDS_SAMPLE_INFO_SAMP_OCTAVE, octave, octave + 1);
		}



		/********************************************************************/
		/// <summary>
		/// Play the selected sample
		/// </summary>
		/********************************************************************/
		private bool PlaySample(int scanCode, Keys key)
		{
			DataGridViewSelectedRowCollection selectedRows = sampleDataGridView.SelectedRows;
			if (selectedRows.Count > 0)
			{
				if (selectedRows[0].Tag is SampleInfo sampleInfo)
				{
					// Check if the pressed key is already registered and if so, ignore it (repeated keys)
					for (int i = 0; i < PolyphonyChannels; i++)
					{
						if (samplePlayKeys[i] == key)
							return false;
					}

					// Set the loop flag
					samplePlayLoops = (sampleInfo.Flags & SampleInfo.SampleFlags.Loop) != 0;

					// Count down number of simulated keys
					samplePlayKeyCount--;

					// Find an empty space in the key array
					for (int i = 0; i < PolyphonyChannels; i++)
					{
						if (samplePlayKeys[i] == Keys.None)
						{
							samplePlayKeys[i] = key;
							break;
						}
					}

					// Find the note out from the pressed key
					int note = FindNoteFromScanCode(scanCode);
					if (note != -1)
					{
						// Add the sample info to the queue
						AddSampleToQueue(sampleInfo, note);

						return true;
					}
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Tries to find the scan code given in the key map and then return
		/// the corresponding note number
		/// </summary>
		/********************************************************************/
		private int FindNoteFromScanCode(int scanCode)
		{
			if (scanCode > 0x35)
				return -1;

			// Lookup the key in the table
			int note = keyTab[scanCode];
			if (note != -1)
				note += samplePlayOctave;

			return note;
		}



		/********************************************************************/
		/// <summary>
		/// Adds the sample information to the play queue. The sample will
		/// then be played later on
		/// </summary>
		/********************************************************************/
		private void AddSampleToQueue(SampleInfo sampleInfo, int note)
		{
			// Only add the sample if the type is sample.
			// Null means to stop the current playing sample
			if ((sampleInfo == null) || (sampleInfo.Type == SampleInfo.SampleType.Sample) || (sampleInfo.Type == SampleInfo.SampleType.Hybrid))
			{
				PlaySampleInfo playInfo = new PlaySampleInfo
				{
					SampleInfo = sampleInfo,
					Note = note
				};

				lock (samplePlayQueue)
				{
					samplePlayQueue.Enqueue(playInfo);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Enable/disable the save button
		/// </summary>
		/********************************************************************/
		private void EnableSaveButton()
		{
			saveButton.Enabled = (sampleDataGridView.SelectedRows.Count > 0) && (saveFormatComboBox.SelectedIndex != -1);
		}



		/********************************************************************/
		/// <summary>
		/// Save the given sample
		/// </summary>
		/********************************************************************/
		private void SaveSample(SampleInfo sampleInfo, string fileName)
		{
			// Create an instance of the sample saver agent to use
			AgentInfo agentInfo = ((KryptonListItem)saveFormatComboBox.SelectedItem).Tag as AgentInfo;
			if (agentInfo != null)
			{
				ISampleSaverAgent converterAgent = (ISampleSaverAgent)agentInfo.Agent.CreateInstance(agentInfo.TypeId);

				// Create the file name
				fileName = Path.ChangeExtension(fileName, converterAgent.FileExtension);

				// Initialize the converter
				bool stereo = (sampleInfo.Flags & SampleInfo.SampleFlags.Stereo) != 0;
				int channels = stereo ? 2 : 1;

				// Build list of buffers to be saved
				sbyte[][] leftSamples, rightSamples = null;

				if ((sampleInfo.Flags & SampleInfo.SampleFlags.MultiOctave) != 0)
				{
					leftSamples = sampleInfo.MultiOctaveAllSamples[0].OrderBy(s => s.Length).ToArray();

					if (stereo)
						rightSamples = sampleInfo.MultiOctaveAllSamples[1].OrderBy(s => s.Length).ToArray();
				}
				else
				{
					leftSamples = new[] { sampleInfo.Sample };

					if (stereo)
						rightSamples = new[] { sampleInfo.SecondSample };
				}

				SaveSampleFormatInfo formatInfo = new SaveSampleFormatInfo(sampleInfo.BitSize, channels, sampleInfo.MiddleC, sampleInfo.LoopStart, sampleInfo.LoopLength, sampleInfo.Name, string.Empty);
				if (!converterAgent.InitSaver(formatInfo, out string errorMessage))
				{
					ShowSimpleErrorMessage(errorMessage);
					return;
				}

				try
				{
					// Open file to store the sound
					using (FileStream fs = new FileStream(fileName, FileMode.Create))
					{
						// Write the header
						converterAgent.SaveHeader(fs);

						// Convert sample to 32-bit
						int[] buffer = new int[leftSamples.Max(s => s.Length) * channels];

						for (int b = 0; b < leftSamples.Length; b++)
						{
							if (stereo)
							{
								if (sampleInfo.BitSize == 8)
								{
									sbyte[] left = leftSamples[b];
									sbyte[] right = rightSamples[b];

									for (int i = 0, j = 0, cnt = left.Length; i < cnt; i++, j += 2)
									{
										buffer[j] = left[i] << 24;
										buffer[j + 1] = right[i] << 24;
									}
								}
								else
								{
									Span<short> left = MemoryMarshal.Cast<sbyte, short>(leftSamples[b]);
									Span<short> right = MemoryMarshal.Cast<sbyte, short>(rightSamples[b]);

									for (int i = 0, j = 0, cnt = left.Length; i < cnt; i++, j += 2)
									{
										buffer[j] = left[i] << 16;
										buffer[j + 1] = right[i] << 16;
									}
								}

								converterAgent.SaveData(fs, buffer, leftSamples[b].Length * 2);
							}
							else
							{
								if (sampleInfo.BitSize == 8)
								{
									sbyte[] left = leftSamples[b];

									for (int i = 0, cnt = left.Length; i < cnt; i++)
										buffer[i] = left[i] << 24;
								}
								else
								{
									Span<short> left = MemoryMarshal.Cast<sbyte, short>(leftSamples[b]);

									for (int i = 0, cnt = left.Length; i < cnt; i++)
										buffer[i] = left[i] << 16;
								}

								converterAgent.SaveData(fs, buffer, leftSamples[b].Length);
							}
						}

						// Save any left overs
						converterAgent.SaveTail(fs);
					}
				}
				catch (Exception ex)
				{
					ShowSimpleErrorMessage(string.Format(Resources.IDS_ERR_SAMPLE_INFO_SAMP_ERRORSAVING, ex.Message));
				}
				finally
				{
					converterAgent.CleanupSaver();
				}
			}
		}
		#endregion
	}
}
