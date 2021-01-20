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
using System.Text;
using System.Windows.Forms;
using Krypton.Navigator;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SampleInfoWindow
{
	/// <summary>
	/// This shows the sample information window
	/// </summary>
	public partial class SampleInfoWindowForm : WindowFormBase
	{
		private ModuleHandler moduleHandler;

		private readonly SampleInfoSettings settings;

		private Dictionary<int, Bitmap> combinedImages = new Dictionary<int, Bitmap>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SampleInfoWindowForm(ModuleHandler moduleHandler)
		{
			InitializeComponent();

			// Some controls need to be initialized here, since the
			// designer remove the properties
			navigator.Panel.PanelBackStyle = PaletteBackStyle.TabLowProfile;
			navigator.Button.CloseButtonDisplay = ButtonDisplay.Hide;
			navigator.Button.ContextButtonDisplay = ButtonDisplay.Hide;

			instrumentDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = PaletteDrawBorders.BottomRight;
			instrumentDataGridView.StateCommon.DataCell.Border.DrawBorders = PaletteDrawBorders.None;

			sampleDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = PaletteDrawBorders.BottomRight;
			sampleDataGridView.StateCommon.DataCell.Border.DrawBorders = PaletteDrawBorders.None;

			// Remember the arguments
			this.moduleHandler = moduleHandler;

			if (!DesignMode)
			{
				// Load window settings
				LoadWindowSettings("SampleInfoWindow");
				settings = new SampleInfoSettings(allWindowSettings);

				// Set the title of the window
				Text = Resources.IDS_SAMPLE_INFO_TITLE;

				// Set the tab titles
				navigator.Pages[0].Text = Resources.IDS_SAMPLE_INFO_TAB_INSTRUMENT;
				navigator.Pages[1].Text = Resources.IDS_SAMPLE_INFO_TAB_SAMPLE;

				navigator.SelectedIndex = settings.ActiveTab;

				// Add the columns to the instrument grid
				instrumentDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = "#",
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
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
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
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
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
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
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn3Width,
						DisplayIndex = settings.SampColumn3Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_LOOPSTART,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn4Width,
						DisplayIndex = settings.SampColumn4Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_LOOPLENGTH,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn5Width,
						DisplayIndex = settings.SampColumn5Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_BITSIZE,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn6Width,
						DisplayIndex = settings.SampColumn6Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_VOLUME,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn7Width,
						DisplayIndex = settings.SampColumn7Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_PANNING,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn8Width,
						DisplayIndex = settings.SampColumn8Pos
					});

				sampleDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_MIDDLEC,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.Automatic,
						DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
						Width = settings.SampColumn9Width,
						DisplayIndex = settings.SampColumn9Pos
					});

				sampleDataGridView.Columns.Add(new DataGridViewImageColumn
					{
						Name = Resources.IDS_SAMPLE_INFO_SAMP_COLUMN_INFO,
						Resizable = DataGridViewTriState.True,
						SortMode = DataGridViewColumnSortMode.NotSortable,
						DefaultCellStyle = new DataGridViewCellStyle() { NullValue = null },
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
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will clear the window and add all the items again
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
			// Remove all the items from the lists
			RemoveInstrumentItems();
			RemoveSampleItems();

			// Now add the items
			AddInstrumentItems();
			AddSampleItems();
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void SampleInfoWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			// Save the settings
			settings.ActiveTab = navigator.SelectedIndex;

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
		#endregion

		#region Private methods
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

					instrumentDataGridView.Rows.Add(i, instrument.Name, sampleUsed.Count);
				}

				// Sort the items
				instrumentDataGridView.Sort(instrumentDataGridView.SortedColumn, Enum.Parse<ListSortDirection>(instrumentDataGridView.SortOrder.ToString()));

				// Resize the rows, so the lines are compacted
				instrumentDataGridView.AutoResizeRows();
			}
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

					if ((sample.Flags & SampleInfo.SampleFlags.Loop) != 0)
						bitmap = AppendBitmap(i, bitmap, Resources.IDB_SAMPLE_LOOP);

					if ((sample.Flags & SampleInfo.SampleFlags.PingPong) != 0)
						bitmap = AppendBitmap(i, bitmap, Resources.IDB_SAMPLE_PINGPONG);

					sampleDataGridView.Rows.Add(i, sample.Name, sample.Length, sample.LoopStart, sample.LoopLength, sample.BitSize, sample.Volume, sample.Panning == -1 ? "-" : sample.Panning, sample.MiddleC, bitmap, GetTypeString(sample.Type));
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
		#endregion
	}
}
