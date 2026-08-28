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
	/// Different purple inspired colors used by forms
	/// </summary>
	internal class PurpleFormColors : IFormColors
	{
		private static readonly Color activatedFormOuterColor = Color.FromArgb(120, 100, 150);
		private static readonly Color activatedFormMiddleColor = Color.FromArgb(215, 205, 232);
		private static readonly Color activatedFormInnerStartColor = Color.FromArgb(200, 190, 225);
		private static readonly Color activatedFormInnerStopColor = Color.FromArgb(190, 180, 215);
		private static readonly Color activatedWindowTitleColor = Color.FromArgb(55, 30, 85);

		private static readonly Color deactivatedFormOuterColor = Color.FromArgb(140, 125, 160);
		private static readonly Color deactivatedFormMiddleColor = Color.FromArgb(220, 212, 235);
		private static readonly Color deactivatedFormInnerStartColor = Color.FromArgb(220, 212, 235);
		private static readonly Color deactivatedFormInnerStopColor = Color.FromArgb(220, 212, 235);
		private static readonly Color deactivatedWindowTitleColor = Color.FromArgb(105, 90, 125);

		private static readonly Color closeCaptionButtonHoverStartColor = Color.FromArgb(255, 132, 130);
		private static readonly Color closeCaptionButtonHoverStopColor = Color.FromArgb(227, 97, 98);
		private static readonly Color closeCaptionButtonPressStartColor = Color.FromArgb(242, 119, 118);
		private static readonly Color closeCaptionButtonPressStopColor = Color.FromArgb(206, 85, 84);

		private static readonly Color captionButtonHoverStartColor = Color.FromArgb(210, 200, 230);
		private static readonly Color captionButtonHoverStopColor = Color.FromArgb(195, 183, 215);
		private static readonly Color captionButtonPressStartColor = Color.FromArgb(185, 170, 205);
		private static readonly Color captionButtonPressStopColor = Color.FromArgb(165, 150, 190);

		private static readonly Color formBackgroundColor = Color.FromArgb(225, 220, 230);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color ActivatedFormOuterColor => activatedFormOuterColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color ActivatedFormMiddleColor => activatedFormMiddleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color ActivatedFormInnerStartColor => activatedFormInnerStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color ActivatedFormInnerStopColor => activatedFormInnerStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color ActivatedWindowTitleColor => activatedWindowTitleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DeactivatedFormOuterColor => deactivatedFormOuterColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DeactivatedFormMiddleColor => deactivatedFormMiddleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DeactivatedFormInnerStartColor => deactivatedFormInnerStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DeactivatedFormInnerStopColor => deactivatedFormInnerStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DeactivatedWindowTitleColor => deactivatedWindowTitleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color CloseCaptionButtonHoverStartColor => closeCaptionButtonHoverStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color CloseCaptionButtonHoverStopColor => closeCaptionButtonHoverStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color CloseCaptionButtonPressStartColor => closeCaptionButtonPressStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color CloseCaptionButtonPressStopColor => closeCaptionButtonPressStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color CaptionButtonHoverStartColor => captionButtonHoverStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color CaptionButtonHoverStopColor => captionButtonHoverStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color CaptionButtonPressStartColor => captionButtonPressStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color CaptionButtonPressStopColor => captionButtonPressStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FormBackgroundColor => formBackgroundColor;
	}
}
