/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow.Pages
{
	/// <summary>
	/// 
	/// </summary>
	public partial class InstrumentPageControl : UserControl
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public InstrumentPageControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		/// </summary>
		/********************************************************************/
		public void InitControl(SampleInfoWindowSettings settings)
		{
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
		}



		/********************************************************************/
		/// <summary>
		/// Save settings
		/// </summary>
		/********************************************************************/
		public void SaveSettings(SampleInfoWindowSettings settings)
		{
			settings.InstColumn1Width = instrumentDataGridView.Columns[0].Width;
			settings.InstColumn1Pos = instrumentDataGridView.Columns[0].DisplayIndex;

			settings.InstColumn2Width = instrumentDataGridView.Columns[1].Width;
			settings.InstColumn2Pos = instrumentDataGridView.Columns[1].DisplayIndex;

			settings.InstColumn3Width = instrumentDataGridView.Columns[2].Width;
			settings.InstColumn3Pos = instrumentDataGridView.Columns[2].DisplayIndex;

			settings.InstSortKey = instrumentDataGridView.SortedColumn.Index;
			settings.InstSortOrder = instrumentDataGridView.SortOrder;
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the control with new data
		/// </summary>
		/********************************************************************/
		public bool RefreshControl(ModuleInfoStatic staticInfo)
		{
			// Remove all the items from the lists
			RemoveInstrumentItems();

			// Now add the items
			return AddInstrumentItems(staticInfo);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Retrieve all the instruments from the player
		/// </summary>
		/********************************************************************/
		private bool AddInstrumentItems(ModuleInfoStatic staticInfo)
		{
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

				return true;
			}

			return false;
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
		#endregion
	}
}
