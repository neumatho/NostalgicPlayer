/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Timer = System.Windows.Forms.Timer;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow
{
	/// <summary>
	/// Search popup control for filtering module list
	/// </summary>
	public partial class SearchPopupControl : UserControl
	{
		private readonly Timer debounceTimer;
		private IEnumerable<ModuleListItem> dataSource;
		private CancellationTokenSource searchCancellationTokenSource;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SearchPopupControl()
		{
			InitializeComponent();

			// Setup debounce timer
			debounceTimer = new Timer();
			debounceTimer.Interval = 200; // 200ms delay
			debounceTimer.Tick += DebounceTimer_Tick;

			searchTextBox.TextChanged += SearchTextBox_TextChanged;
			searchTextBox.KeyDown += SearchTextBox_KeyDown;
			resultsListControl.KeyPress += ResultsListControl_KeyPress;
			resultsListControl.KeyDown += ResultsListControl_KeyDown;
			resultsListControl.MouseDoubleClick += ResultsListControl_MouseDoubleClick;
		}


		/********************************************************************/
		/// <summary>
		/// Raised when user selects an item
		/// </summary>
		/********************************************************************/
		public event EventHandler<ModuleListItem> ItemSelected;

		/********************************************************************/
		/// <summary>
		/// Raised when user cancels the search
		/// </summary>
		/********************************************************************/
		public event EventHandler SearchCancelled;


		/********************************************************************/
		/// <summary>
		/// Set the data source for searching
		/// </summary>
		/********************************************************************/
		public void SetDataSource(IEnumerable<ModuleListItem> items)
		{
			dataSource = items;
		}


		/********************************************************************/
		/// <summary>
		/// Set initial search text and start searching
		/// </summary>
		/********************************************************************/
		public void SetInitialText(string text)
		{
			// Clear old results
			resultsListControl.Items.Clear();

			searchTextBox.Text = text;
			searchTextBox.SelectionStart = text.Length;
			searchTextBox.Focus();
		}


		/********************************************************************/
		/// <summary>
		/// Enable/disable list number viewing
		/// </summary>
		/********************************************************************/
		public void EnableListNumber(bool enable)
		{
			resultsListControl.EnableListNumber(enable);
		}

		#region Event handlers

		/********************************************************************/
		/// <summary>
		/// Is called when search text changes
		/// </summary>
		/********************************************************************/
		private void SearchTextBox_TextChanged(object sender, EventArgs e)
		{
			// Stop and restart the debounce timer
			debounceTimer.Stop();

			if (string.IsNullOrEmpty(searchTextBox.Text))
				SearchCancelled?.Invoke(this, EventArgs.Empty);
			else
				debounceTimer.Start();
		}


		/********************************************************************/
		/// <summary>
		/// Is called when debounce timer ticks
		/// </summary>
		/********************************************************************/
		private void DebounceTimer_Tick(object sender, EventArgs e)
		{
			debounceTimer.Stop();

			// Cancel any previous search
			if (searchCancellationTokenSource != null)
			{
				searchCancellationTokenSource.Cancel();
				searchCancellationTokenSource = null;
			}

			// Start new search
			searchCancellationTokenSource = new CancellationTokenSource();
			PerformSearchAsync(searchCancellationTokenSource);
		}


		/********************************************************************/
		/// <summary>
		/// Perform the search asynchronously
		/// </summary>
		/********************************************************************/
		private async void PerformSearchAsync(CancellationTokenSource cts)
		{
			if (dataSource == null)
				return;

			string searchText = searchTextBox.Text;

			try
			{
				// Perform search in background
				var results = await ModuleSearchHelper.SearchAsync(dataSource, searchText, cts.Token);

				// Synchronize and update UI if not cancelled
				if (searchCancellationTokenSource == cts && !cts.Token.IsCancellationRequested)
				{
					resultsListControl.Items.Clear();

					foreach (var item in results)
						resultsListControl.Items.Add(item);

					if (resultsListControl.Items.Count > 0)
					{
						resultsListControl.SelectedIndex = 0;
						resultsListControl.SetLastItemSelected(0);
					}

					// Clear reference
					searchCancellationTokenSource = null;
				}
			}
			catch (OperationCanceledException)
			{
				// Expected when search is cancelled
			}
		}


		/********************************************************************/
		/// <summary>
		/// Is called when user presses key in search box
		/// </summary>
		/********************************************************************/
		private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				if (resultsListControl.Items.Count > 1)
				{
					resultsListControl.SelectedIndex = 1;
					resultsListControl.SetLastItemSelected(1);
					resultsListControl.Focus();
					e.Handled = true;
				}
			}
			else if (e.KeyCode == Keys.Escape)
			{
				SearchCancelled?.Invoke(this, EventArgs.Empty);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Enter)
				if (resultsListControl.SelectedItem != null)
				{
					ItemSelected?.Invoke(this, resultsListControl.SelectedItem);
					e.Handled = true;
				}
		}


		/********************************************************************/
		/// <summary>
		/// Is called when user presses key in results list
		/// </summary>
		/********************************************************************/
		private void ResultsListControl_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\x1B') // Escape
			{
				SearchCancelled?.Invoke(this, EventArgs.Empty);
				e.Handled = true;
			}
			else if (e.KeyChar == '\r') // Enter
			{
				if (resultsListControl.SelectedItem != null)
				{
					ItemSelected?.Invoke(this, resultsListControl.SelectedItem);
					e.Handled = true;
				}
			}
			else if (e.KeyChar == '\b') // Backspace
			{
				// Remove last character and focus textbox
				if (searchTextBox.Text.Length > 0)
				{
					searchTextBox.Text = searchTextBox.Text.Substring(0, searchTextBox.Text.Length - 1);
					searchTextBox.SelectionStart = searchTextBox.Text.Length;
				}

				searchTextBox.Focus();
				e.Handled = true;
			}
			else if (char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '*' || e.KeyChar == '?')
			{
				// Append to search text and focus textbox
				searchTextBox.Text += e.KeyChar;
				searchTextBox.SelectionStart = searchTextBox.Text.Length;
				searchTextBox.Focus();
				e.Handled = true;
			}
		}


		/********************************************************************/
		/// <summary>
		/// Is called when user presses key down in results list
		/// </summary>
		/********************************************************************/
		private void ResultsListControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up && resultsListControl.SelectedIndex == 0)
			{
				searchTextBox.Focus();
				searchTextBox.SelectionStart = searchTextBox.Text.Length;
				e.Handled = true;
			}
		}


		/********************************************************************/
		/// <summary>
		/// Is called when user double-clicks an item
		/// </summary>
		/********************************************************************/
		private void ResultsListControl_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (resultsListControl.SelectedItem != null)
				ItemSelected?.Invoke(this, resultsListControl.SelectedItem);
		}

		#endregion
	}
}