/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Linq;
using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.OpenUrlWindow
{
	/// <summary>
	/// This shows the open URL window
	/// </summary>
	public partial class OpenUrlWindowForm : KryptonForm
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OpenUrlWindowForm()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				// Set the title of the window
				Text = Resources.IDS_OPENURL_TITLE;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the entered name
		/// </summary>
		/********************************************************************/
		public string GetName()
		{
			char[] invalidChars = Path.GetInvalidFileNameChars();

			string name = nameTextBox.Text.Trim();
			name = string.Concat(name.Where(c => !invalidChars.Contains(c)));

			if (string.IsNullOrEmpty(name))
			{
				// If no name is entered, use the URL as the name
				name = GetUrl();
			}

			return name;
		}



		/********************************************************************/
		/// <summary>
		/// Return the entered URL
		/// </summary>
		/********************************************************************/
		public string GetUrl()
		{
			return urlTextBox.Text.Trim();
		}
	}
}
