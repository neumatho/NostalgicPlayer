/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Standard
{
	/// <summary></summary>
	public class StandardTheme : ITheme, IDisposable
	{
		private StandardFonts standardFonts;

		private static readonly StandardImageColors imageColors = new StandardImageColors();
		private static readonly StandardFormColors formColors = new StandardFormColors();
		private static readonly StandardBoxColors boxColors = new StandardBoxColors();
		private static readonly StandardButtonColors buttonColors = new StandardButtonColors();
		private static readonly StandardComboBoxColors comboBoxColors = new StandardComboBoxColors();
		private static readonly StandardTextBoxColors textBoxColors = new StandardTextBoxColors();
		private static readonly StandardDataGridViewColors dataGridViewColors = new StandardDataGridViewColors();
		private static readonly StandardScrollBarColors scrollBarColors = new StandardScrollBarColors();
		private static readonly StandardTabColors tabColors = new StandardTabColors();
		private static readonly StandardLabelColors labelColors = new StandardLabelColors();
		private static readonly StandardModuleListColors moduleListColors = new StandardModuleListColors();
		private static readonly StandardTrackBarColors trackBarColors = new StandardTrackBarColors();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StandardTheme()
		{
			standardFonts = new StandardFonts();
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			standardFonts.Dispose();
			standardFonts = null;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a unique ID for the theme
		/// </summary>
		/********************************************************************/
		public Guid Id => new Guid("DA6C654D-823C-41AB-AC97-B759943555CF");



		/********************************************************************/
		/// <summary>
		/// Return a collection of standard fonts
		/// </summary>
		/********************************************************************/
		public IFonts StandardFonts => standardFonts;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by images
		/// </summary>
		/********************************************************************/
		public IImageColors ImageColors => imageColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by forms
		/// </summary>
		/********************************************************************/
		public IFormColors FormColors => formColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by box
		/// </summary>
		/********************************************************************/
		public IBoxColors BoxColors => boxColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by buttons
		/// </summary>
		/********************************************************************/
		public IButtonColors ButtonColors => buttonColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by combo boxes
		/// </summary>
		/********************************************************************/
		public IComboBoxColors ComboBoxColors => comboBoxColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by text boxes
		/// </summary>
		/********************************************************************/
		public ITextBoxColors TextBoxColors => textBoxColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by DataGridView
		/// </summary>
		/********************************************************************/
		public IDataGridViewColors DataGridViewColors => dataGridViewColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by scroll bars
		/// </summary>
		/********************************************************************/
		public IScrollBarColors ScrollBarColors => scrollBarColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by tab controls
		/// </summary>
		/********************************************************************/
		public ITabColors TabColors => tabColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by label controls
		/// </summary>
		/********************************************************************/
		public ILabelColors LabelColors => labelColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by module list control
		/// </summary>
		/********************************************************************/
		public IModuleListColors ModuleListColors => moduleListColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by track bars
		/// </summary>
		/********************************************************************/
		public ITrackBarColors TrackBarColors => trackBarColors;
	}
}
