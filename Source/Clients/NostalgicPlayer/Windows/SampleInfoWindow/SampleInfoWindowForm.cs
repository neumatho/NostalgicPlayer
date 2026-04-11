/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Services;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow
{
	/// <summary>
	/// This shows the sample information window
	/// </summary>
	public partial class SampleInfoWindowForm : WindowFormBase2
	{
		private IModuleHandlerService moduleHandler;

		private SampleInfoWindowSettings settings;

		private bool doNotUpdateAutoSelection;

		/// <summary></summary>
		public const int PolyphonyChannels = 3;

		private const int Page_Instruments = 0;
		private const int Page_Samples = 1;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SampleInfoWindowForm()
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
		public void InitializeForm(IModuleHandlerService moduleHandlerService)
		{
			// Remember the arguments
			moduleHandler = moduleHandlerService;

			// Load window settings
			LoadWindowSettings("SampleInfoWindow");
			settings = new SampleInfoWindowSettings(allWindowSettings);

			// Set the title of the window
			Text = Resources.IDS_SAMPLE_INFO_TITLE;

			// Set the tab titles
			tabControl.Pages[Page_Instruments].Text = Resources.IDS_SAMPLE_INFO_TAB_INSTRUMENT;
			tabControl.Pages[Page_Samples].Text = Resources.IDS_SAMPLE_INFO_TAB_SAMPLE;

			// Select the last used tab
			tabControl.SelectedIndex = settings.AutoSelectTab;

			instrumentPageControl.InitControl(settings);
			samplePageControl.InitControl(settings);

			RefreshWindow();
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
				doNotUpdateAutoSelection = true;

				ModuleInfoStatic staticInfo = moduleHandler.StaticModuleInformation;

				if (instrumentPageControl.RefreshControl(staticInfo))
				{
					// Show tab if auto-selected
					tabControl.Pages[Page_Instruments].Visible = true;

					if (settings.AutoSelectTab == Page_Instruments)
						tabControl.SelectedIndex = Page_Instruments;

				}
				else
					tabControl.Pages[Page_Instruments].Visible = false;

				samplePageControl.RefreshControl(staticInfo);

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
			return samplePageControl.ProcessKey(scanCode, key);
		}



		/********************************************************************/
		/// <summary>
		/// Checks the queue to see if there is any samples waiting to be
		/// played and if so, removes it from the queue and return it
		/// </summary>
		/********************************************************************/
		public PlaySampleInfo GetNextSampleFromQueue()
		{
			return samplePageControl.GetNextSampleFromQueue();
		}



		/********************************************************************/
		/// <summary>
		/// Tells whether polyphony is enabled or not
		/// </summary>
		/********************************************************************/
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsPolyphonyEnabled => samplePageControl.IsPolyphonyEnabled;
		#endregion

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected override string HelpUrl => "sampinfo.html";
		#endregion

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
				instrumentPageControl.SaveSettings(settings);
				samplePageControl.SaveSettings(settings);

				// Cleanup
				samplePageControl.CleanupControl();

				// Cleanup
				moduleHandler = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a tab is selected
		/// </summary>
		/********************************************************************/
		private void Tab_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!doNotUpdateAutoSelection)
				settings.AutoSelectTab = tabControl.SelectedIndex;
		}
		#endregion
	}
}
