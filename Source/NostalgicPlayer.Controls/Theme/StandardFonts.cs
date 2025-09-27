/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme
{
	/// <summary>
	/// Returns a collection of standard fonts
	/// </summary>
	internal class StandardFonts : IFonts, IDisposable
	{
		private const float DefaultFontSize = 8.0f;

		private Font regularFont;
		private Font monospaceFont;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StandardFonts()
		{
			regularFont = new Font("Microsoft Sans", DefaultFontSize, FontStyle.Regular, GraphicsUnit.Point);
			monospaceFont = new Font("Lucida Console", DefaultFontSize, FontStyle.Regular, GraphicsUnit.Point);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			regularFont.Dispose();
			monospaceFont.Dispose();

			regularFont = null;
			monospaceFont = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Font FormTitleFont => regularFont;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Font RegularFont => regularFont;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Font MonospaceFont => monospaceFont;
	}
}
