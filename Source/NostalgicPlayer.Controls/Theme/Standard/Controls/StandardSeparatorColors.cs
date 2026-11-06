/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Controls;

namespace Polycode.NostalgicPlayer.Controls.Theme.Standard.Controls
{
	/// <summary>
	/// Different colors used by separators
	/// </summary>
	internal class StandardSeparatorColors : ISeparatorColors
	{
		private static readonly Color lineColor = Color.FromArgb(133, 158, 191);

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color LineColor => lineColor;
	}
}
