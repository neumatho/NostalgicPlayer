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
	/// Purple theme colors for group boxes
	/// </summary>
	internal class PurpleGroupBoxColors : IGroupBoxColors
	{
		private readonly IBoxColors boxColors = new PurpleBoxColors();
		private readonly ILabelColors labelColors = new PurpleLabelColors();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color BorderColor => boxColors.BorderColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color HeaderColor => labelColors.TextColor;
	}
}
