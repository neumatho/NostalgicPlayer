/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.HelpWindow
{
	/// <summary>
	/// This shows the help documentation
	/// </summary>
	public partial class HelpWindowForm : WindowFormBase
	{
		private IMainWindowApi mainWindow;
		private WebView2 webView;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HelpWindowForm()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the form
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeForm(IMainWindowApi mainWindow, IPlatformPath platformPath)
		{
			this.mainWindow = mainWindow;

			// Add the browser control and store its user data in a writable folder.
			// The default location is next to the executable, which is read-only
			// when the application is installed as an MSIX package
			webView = new WebView2();
			webView.Dock = DockStyle.Fill;
			webView.CreationProperties = new CoreWebView2CreationProperties
			{
				UserDataFolder = platformPath.WebBrowserPath
			};
			webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
			Controls.Add(webView);

			// Set the title of the window
			Text = Resources.IDS_HELP_TITLE;
		}



		/********************************************************************/
		/// <summary>
		/// Return the window settings name
		/// </summary>
		/********************************************************************/
		protected override string WindowSettingsName => "HelpWindow";



		/********************************************************************/
		/// <summary>
		/// Navigate to a specific page
		/// </summary>
		/********************************************************************/
		public void Navigate(string page)
		{
			// Load the version specific documentation
			string url = $"https://nostalgicplayer.dk/appdoc/{Env.CurrentVersion}/{page}";

			// Make sure the WebView2 Evergreen runtime is present (it ships with
			// Windows 11 and updated Windows 10 installations)
			if (string.IsNullOrEmpty(CoreWebView2Environment.GetAvailableBrowserVersionString()))
			{
				mainWindow.ShowSimpleErrorMessage(this, Resources.IDS_ERR_WEBVIEW2_MISSING);

				Close();
				return;
			}

			// Setting the source initializes the control (if needed) and navigates.
			// Initialization runs asynchronously on the UI thread, so the call stays
			// synchronous here. Any initialization failure is reported through the
			// CoreWebView2InitializationCompleted event handler below
			webView.Source = new Uri(url);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Is called when the form is closed
		/// </summary>
		/********************************************************************/
		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);

			webView?.Dispose();
			webView = null;
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Show an error if the WebView2 control failed to initialize
		/// </summary>
		/********************************************************************/
		private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{
			if (!e.IsSuccess)
				mainWindow.ShowSimpleErrorMessage(string.Format(Resources.IDS_ERR_WEBVIEW2_FAILED, e.InitializationException?.Message));
		}
		#endregion
	}
}
