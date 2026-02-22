/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Purple
{
	/// <summary>
	/// Purple theme colors for Box
	/// </summary>
	internal class PurpleBoxColors : IBoxColors
	{
		private static readonly Color borderColor = Color.FromArgb(140, 125, 160);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color BorderColor => borderColor;
	}
}
