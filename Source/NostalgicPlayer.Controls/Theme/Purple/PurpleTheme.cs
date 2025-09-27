/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Purple
{
	/// <summary>
	/// A simple purple themed test implementation of ITheme
	/// </summary>
	public class PurpleTheme : ITheme, IDisposable
	{
		private StandardFonts standardFonts;
		private static readonly PurpleFormColors formColors = new PurpleFormColors();
		private static readonly PurpleButtonColors buttonColors = new PurpleButtonColors();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PurpleTheme()
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
		public Guid Id => new Guid("A4F0D3C0-5D2E-44A2-9CF1-8A1C6D2E0B12");



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
