/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Gui.Components;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages
{
	/// <summary>
	/// 
	/// </summary>
	public partial class LyricsPageControl : UserControl
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LyricsPageControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the control with new data
		/// </summary>
		/********************************************************************/
		public bool RefreshControl(bool isPlaying, ModuleInfoStatic staticInfo)
		{
			bool visible = false;

			moduleInfoLyricsReadOnlyTextBox.Lines = null;

			// Check to see if there are any module loaded at the moment
			if (isPlaying)
			{
				// Add lyrics
				if (staticInfo.Lyrics?.Length > 0)
				{
					visible = true;

					// Switch font
					moduleInfoLyricsReadOnlyTextBox.Font = staticInfo.LyricsFont ?? FontPalette.GetMonospaceFont();

					// Set text
					moduleInfoLyricsReadOnlyTextBox.Lines = staticInfo.Lyrics;
				}
			}

			return visible;
		}
	}
}
