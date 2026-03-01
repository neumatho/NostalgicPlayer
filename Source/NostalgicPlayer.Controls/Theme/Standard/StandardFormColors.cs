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
	/// Different colors used by forms
	/// </summary>
	internal class StandardFormColors : IFormColors
	{
		private static readonly Color activatedFormOuterColor = Color.FromArgb(144, 154, 166);
		private static readonly Color activatedFormMiddleColor = Color.FromArgb(212, 230, 245);
		private static readonly Color activatedFormInnerStartColor = Color.FromArgb(193, 212, 236);
		private static readonly Color activatedFormInnerStopColor = Color.FromArgb(187, 206, 230);
		private static readonly Color activatedWindowTitleColor = Color.FromArgb(30, 57, 91);

		private static readonly Color deactivatedFormOuterColor = Color.FromArgb(162, 173, 185);
		private static readonly Color deactivatedFormMiddleColor = Color.FromArgb(223, 235, 247);
		private static readonly Color deactivatedFormInnerStartColor = Color.FromArgb(223, 235, 247);
		private static readonly Color deactivatedFormInnerStopColor = Color.FromArgb(223, 235, 247);
		private static readonly Color deactivatedWindowTitleColor = Color.FromArgb(106, 128, 168);

		private static readonly Color closeCaptionButtonHoverStartColor = Color.FromArgb(255, 132, 130);
		private static readonly Color closeCaptionButtonHoverStopColor = Color.FromArgb(227, 97, 98);
		private static readonly Color closeCaptionButtonPressStartColor = Color.FromArgb(242, 119, 118);
		private static readonly Color closeCaptionButtonPressStopColor = Color.FromArgb(206, 85, 84);

		private static readonly Color captionButtonHoverStartColor = Color.FromArgb(214, 234, 255);
		private static readonly Color captionButtonHoverStopColor = Color.FromArgb(188, 207, 231);
		private static readonly Color captionButtonPressStartColor = Color.FromArgb(187, 206, 230);
		private static readonly Color captionButtonPressStopColor = Color.FromArgb(166, 182, 213);

		private static readonly Color formBackgroundColor = Color.FromArgb(240, 240, 240);

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
