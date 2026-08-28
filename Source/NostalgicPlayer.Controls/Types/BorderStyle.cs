/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Controls.Types
{
	/// <summary>
	/// Selects the visual style of the border drawn around a NostalgicForm
	/// </summary>
	public enum BorderStyle
	{
		/// <summary>
		/// The default style with a thick decorated frame, rounded corners
		/// and a title bar including caption buttons
		/// </summary>
		Normal,

		/// <summary>
		/// A minimal style that only draws a thin frame around the form.
		/// No title bar, caption buttons or icon are drawn
		/// </summary>
		Thin
	}
}
