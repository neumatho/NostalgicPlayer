/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	///     Search popup control for filtering module list
	/// </summary>
	public partial class SearchPopupControl : UserControl
	{
		/********************************************************************/
		/// <summary>
		///     Constructor
		/// </summary>
		/********************************************************************/
		public SearchPopupControl()
		{
			InitializeComponent();

			searchTextBox.TextChanged += SearchTextBox_TextChanged;
			searchTextBox.KeyDown += SearchTextBox_KeyDown;
			resultsListControl.KeyPress += ResultsListControl_KeyPress;
			resultsListControl.KeyDown += ResultsListControl_KeyDown;
			resultsListControl.MouseDoubleClick += ResultsListControl_MouseDoubleClick;
		}


		/********************************************************************/
		/// <summary>
		///     Get current search text
		/// </summary>
		/********************************************************************/
		public string SearchText => searchTextBox.Text;

		/********************************************************************/
		/// <summary>
		///     Raised when user selects an item
		/// </summary>
		/********************************************************************/
		public event EventHandler<ModuleListItem> ItemSelected;

		/********************************************************************/
		/// <summary>
		///     Raised when user cancels the search
		/// </summary>
		/********************************************************************/
		public event EventHandler SearchCancelled;

		/********************************************************************/
		/// <summary>
		///     Raised when search text changes
		/// </summary>
		/********************************************************************/
		public event EventHandler SearchTextChanged;

		/********************************************************************/
		/// <summary>
		///     Raised when result count changes
		/// </summary>
		/********************************************************************/
		public event EventHandler<int> ResultCountChanged;


		/********************************************************************/
		/// <summary>
		///     Set initial search text
		/// </summary>
		/********************************************************************/
		public void SetInitialText(string text)
		{
			searchTextBox.Text = text;
			searchTextBox.SelectionStart = text.Length;
			searchTextBox.Focus();
		}


		/********************************************************************/
		/// <summary>
		///     Update the search results
		/// </summary>
		/********************************************************************/
		public void UpdateResults(ModuleListItem[] items)
		{
			resultsListControl.Items.Clear();

			foreach (var item in items)
				resultsListControl.Items.Add(item);

			if (resultsListControl.Items.Count > 0)
			{
				resultsListControl.SelectedIndex = 0;
				resultsListControl.SetLastItemSelected(0);
			}

			ResultCountChanged?.Invoke(this, items.Length);
		}


		/********************************************************************/
		/// <summary>
		///     Enable/disable list number viewing
		/// </summary>
		/********************************************************************/
		public void EnableListNumber(bool enable)
		{
			resultsListControl.EnableListNumber(enable);
		}

		#region Event handlers

		/********************************************************************/
		/// <summary>
		///     Is called when search text changes
		/// </summary>
		/********************************************************************/
		private void SearchTextBox_TextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(searchTextBox.Text))
				SearchCancelled?.Invoke(this, EventArgs.Empty);
			else
				SearchTextChanged?.Invoke(this, EventArgs.Empty);
		}


		/********************************************************************/
		/// <summary>
		///     Is called when user presses key in search box
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
		///     Is called when user presses key in results list
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
		///     Is called when user presses key down in results list
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
		///     Is called when user double-clicks an item
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