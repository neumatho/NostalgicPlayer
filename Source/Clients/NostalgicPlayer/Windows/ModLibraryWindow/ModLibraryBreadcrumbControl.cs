/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Events;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Shows the breadcrumb navigation
	/// </summary>
	public partial class ModLibraryBreadcrumbControl : UserControl
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLibraryBreadcrumbControl()
		{
			InitializeComponent();
		}

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Initialize()
		{
			parentButton.Enabled = false;
			breadcrumb.Items.Clear();
			countLabel.Text = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Return the breadcrumb path
		/// </summary>
		/********************************************************************/
		public string GetPath()
		{
			string path = breadcrumb.Path;

			string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

			return string.Join('/', parts.Skip(1));
		}



		/********************************************************************/
		/// <summary>
		/// Will validate the given path and set the breadcrumb control
		/// </summary>
		/********************************************************************/
		public void SetPath(string path)
		{
			breadcrumb.Path = Resources.IDS_MODLIBRARY_HOME + "/" + path;
		}



		/********************************************************************/
		/// <summary>
		/// Show the number of folders and files in the current directory.
		/// If a search is active, the total number of files found is shown
		/// too
		/// </summary>
		/********************************************************************/
		public void SetCounts(int folderCount, int fileCount, int? totalFilesFound)
		{
			countLabel.Text = totalFilesFound.HasValue
				? string.Format(Resources.IDS_MODLIBRARY_SEARCH_COUNT, folderCount, fileCount, totalFilesFound.Value)
				: string.Format(Resources.IDS_MODLIBRARY_FOLDERS_FILES_COUNT, folderCount, fileCount);

			PerformLayout();
		}



		/********************************************************************/
		/// <summary>
		/// Remove the counts
		/// </summary>
		/********************************************************************/
		public void ClearCounts()
		{
			countLabel.Text = string.Empty;

			PerformLayout();
		}



		/********************************************************************/
		/// <summary>
		/// Go one level up. Nothing happens if already at the root
		/// </summary>
		/********************************************************************/
		public void GotoParent()
		{
			breadcrumb.GotoParent();
		}
		#endregion

		#region Public events
		/********************************************************************/
		/// <summary>
		/// Fires when the breadcrumb path changes
		/// </summary>
		/********************************************************************/
		[Browsable(true)]
		[Category("Property Changed")]
		[Description("Fires when the breadcrumb path changes.")]
		public event BreadcrumbPathChangedEventHandler PathChanged;
		#endregion

		#region Events
		/********************************************************************/
		/// <summary>
		/// Place the breadcrumb so it only takes the space it needs, and
		/// put the label right after it without going past the right edge
		/// </summary>
		/********************************************************************/
		private void ModLibraryBreadcrumbControl_Layout(object sender, LayoutEventArgs e)
		{
			const int Gap = 4;

			int left = parentButton.Right + Gap;
			int maxWidth = ClientSize.Width - left - Gap - countLabel.Width;
			int width = Math.Max(0, Math.Min(breadcrumb.GetPreferredSize(Size.Empty).Width, maxWidth));

			breadcrumb.SetBounds(left, breadcrumb.Top, width, breadcrumb.Height);
			countLabel.Left = breadcrumb.Right + Gap;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Parent_Click(object sender, EventArgs e)
		{
			breadcrumb.GotoParent();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Breadcrumb_ItemClicked(object sender, BreadcrumbItemClickedEventArgs e)
		{
			breadcrumb.Items.RemoveAfter(e.Index);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Breadcrumb_PathChanged(object sender, BreadcrumbPathChangedEventArgs e)
		{
			parentButton.Enabled = breadcrumb.Items.Count > 1;

			PathChanged?.Invoke(this, e);
		}
		#endregion
	}
}
