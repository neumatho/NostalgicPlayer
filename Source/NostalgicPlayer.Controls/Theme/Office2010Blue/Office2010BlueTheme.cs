/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Office2010Blue
{
	/// <summary></summary>
	public class Office2010BlueTheme : ITheme, IDisposable
	{
		private StandardFonts standardFonts;

		private static readonly Office2010BlueFormColors formColors = new Office2010BlueFormColors();
		private static readonly Office2010BlueButtonColors buttonColors = new Office2010BlueButtonColors();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Office2010BlueTheme()
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
		/// Return a collection of colors used by forms
		/// </summary>
		/********************************************************************/
		public IFormColors FormColors => formColors;



		/********************************************************************/
		/// <summary>
		/// Return a collection of colors used by buttons
		/// </summary>
		/********************************************************************/
		public IButtonColors ButtonColors => buttonColors;
	}
}
