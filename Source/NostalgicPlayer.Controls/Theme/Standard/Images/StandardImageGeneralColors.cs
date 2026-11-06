/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces.Images;

namespace Polycode.NostalgicPlayer.Controls.Theme.Standard.Images
{
	/// <summary>
	/// Standard theme colors for General images
	/// </summary>
	internal class StandardImageGeneralColors : IImageGeneralColors
	{
		private static readonly Color messageBoxErrorColor = Color.FromArgb(185, 6, 10);
		private static readonly Color messageBoxWarningColor = Color.FromArgb(255, 235, 45);
		private static readonly Color messageBoxInformationColor = Color.FromArgb(26, 64, 171);
		private static readonly Color messageBoxQuestionColor = Color.FromArgb(26, 64, 171);
		private static readonly Color searchColor = Color.FromArgb(0, 0, 0);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color MessageBoxErrorColor => messageBoxErrorColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color MessageBoxWarningColor => messageBoxWarningColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color MessageBoxInformationColor => messageBoxInformationColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color MessageBoxQuestionColor => messageBoxQuestionColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SearchColor => searchColor;
	}
}
