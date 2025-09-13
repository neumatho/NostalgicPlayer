/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Holds all the colors used by a form
	/// </summary>
	public interface IFormColors
	{
		/// <summary></summary>
		Color ActivatedFormOuterColor { get; }
		/// <summary></summary>
		Color ActivatedFormMiddleColor { get; }
		/// <summary></summary>
		Color ActivatedFormInnerStartColor { get; }
		/// <summary></summary>
		Color ActivatedFormInnerStopColor { get; }
		/// <summary></summary>
		Color ActivatedWindowTitleColor { get; }

		/// <summary></summary>
		Color DeactivatedFormOuterColor { get; }
		/// <summary></summary>
		Color DeactivatedFormMiddleColor { get; }
		/// <summary></summary>
		Color DeactivatedFormInnerStartColor { get; }
		/// <summary></summary>
		Color DeactivatedFormInnerStopColor { get; }
		/// <summary></summary>
		Color DeactivatedWindowTitleColor { get; }

		/// <summary></summary>
		Color CloseCaptionButtonHoverStartColor { get; }
		/// <summary></summary>
		Color CloseCaptionButtonHoverStopColor { get; }
		/// <summary></summary>
		Color CloseCaptionButtonPressStartColor { get; }
		/// <summary></summary>
		Color CloseCaptionButtonPressStopColor { get; }

		/// <summary></summary>
		Color CaptionButtonHoverStartColor { get; }
		/// <summary></summary>
		Color CaptionButtonHoverStopColor { get; }
		/// <summary></summary>
		Color CaptionButtonPressStartColor { get; }
		/// <summary></summary>
		Color CaptionButtonPressStopColor { get; }

		/// <summary></summary>
		Color FormBackgroundColor { get; }
	}
}
