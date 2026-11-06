/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Controls;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images;
using Polycode.NostalgicPlayer.Controls.Theme.Purple.Controls;

namespace Polycode.NostalgicPlayer.Controls.Theme.Purple.Images
{
	/// <summary>
	/// Purple theme colors for Module Information window images
	/// </summary>
	internal class PurpleImageModuleInformationColors : IImageModuleInformationColors
	{
		private readonly ILabelColors labelColors = new PurpleLabelColors();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PreviousPictureColor => labelColors.TextColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NextPictureColor => labelColors.TextColor;
	}
}
