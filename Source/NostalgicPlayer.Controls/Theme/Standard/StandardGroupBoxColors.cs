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
	/// Standard theme colors for group boxes
	/// </summary>
	internal class StandardGroupBoxColors : IGroupBoxColors
	{
		private readonly IBoxColors boxColors = new StandardBoxColors();
		private readonly ILabelColors labelColors = new StandardLabelColors();

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
