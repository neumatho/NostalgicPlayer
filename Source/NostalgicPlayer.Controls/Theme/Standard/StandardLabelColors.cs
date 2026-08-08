/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Standard
{
	/// <summary>
	/// Different colors used by labels
	/// </summary>
	internal class StandardLabelColors : ILabelColors
	{
		private static readonly Color textColor = Color.FromArgb(30, 57, 91);

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color TextColor => textColor;
	}
}
