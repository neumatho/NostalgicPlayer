/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	///     Popup form for searching modules
	/// </summary>
	public partial class SearchPopupForm : Form
	{
		/********************************************************************/
		/// <summary>
		///     Constructor
		/// </summary>
		/********************************************************************/
		public SearchPopupForm()
		{
			InitializeComponent();

			searchPopupControl.ItemSelected += SearchPopupControl_ItemSelected;
			searchPopupControl.SearchCancelled += SearchPopupControl_SearchCancelled;
			searchPopupControl.SearchTextChanged += SearchPopupControl_SearchTextChanged;

			Deactivate += SearchPopupForm_Deactivate;
		}


		/********************************************************************/
		/// <summary>
		///     Get current search text
		/// </summary>
		/********************************************************************/
		public string SearchText => searchPopupControl.SearchText;

		/********************************************************************/
		/// <summary>
		///     Raised when user selects an item
		/// </summary>
		/********************************************************************/
		public event EventHandler<ModuleListItem> ItemSelected;

		/********************************************************************/
		/// <summary>
		///     Raised when search text changes
		/// </summary>
		/********************************************************************/
		public event EventHandler SearchTextChanged;


		/********************************************************************/
		/// <summary>
		///     Set initial search text
		/// </summary>
		/********************************************************************/
		public void SetInitialText(string text)
		{
			searchPopupControl.SetInitialText(text);
		}


		/********************************************************************/
		/// <summary>
		///     Update search results
		/// </summary>
		/********************************************************************/
		public void UpdateResults(ModuleListItem[] items)
		{
			searchPopupControl.UpdateResults(items);
		}


		/********************************************************************/
		/// <summary>
		///     Enable/disable list number viewing
		/// </summary>
		/********************************************************************/
		public void EnableListNumber(bool enable)
		{
			searchPopupControl.EnableListNumber(enable);
		}


		/********************************************************************/
		/// <summary>
		///     Show the popup at specified location with specified size
		/// </summary>
		/********************************************************************/
		public void ShowAt(Point location, Size size)
		{
			Location = location;
			Size = size;
			Show();
		}

		#region Event handlers

		/********************************************************************/
		/// <summary>
		///     Is called when user selects an item
		/// </summary>
		/********************************************************************/
		private void SearchPopupControl_ItemSelected(object sender, ModuleListItem item)
		{
			ItemSelected?.Invoke(this, item);
			Close();
		}


		/********************************************************************/
		/// <summary>
		///     Is called when user cancels search
		/// </summary>
		/********************************************************************/
		private void SearchPopupControl_SearchCancelled(object sender, EventArgs e)
		{
			Close();
		}


		/********************************************************************/
		/// <summary>
		///     Is called when search text changes
		/// </summary>
		/********************************************************************/
		private void SearchPopupControl_SearchTextChanged(object sender, EventArgs e)
		{
			SearchTextChanged?.Invoke(this, EventArgs.Empty);
		}


		/********************************************************************/
		/// <summary>
		///     Is called when form loses focus
		/// </summary>
		/********************************************************************/
		private void SearchPopupForm_Deactivate(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}