/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Different themes implement this interface
	/// </summary>
	public interface ITheme
	{
		/// <summary>
		/// Returns a unique ID for the theme
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// Return a collection of standard fonts
		/// </summary>
		IFonts StandardFonts { get; }

		/// <summary>
		/// Return a collection of colors used by forms
		/// </summary>
		IFormColors FormColors { get; }

		/// <summary>
		/// Return a collection of colors used by buttons
		/// </summary>
		IButtonColors ButtonColors { get; }

		/// <summary>
		/// Return a collection of colors used by combo boxes
		/// </summary>
		IComboBoxColors ComboBoxColors { get; }

		/// <summary>
		/// Return a collection of colors used by DataGridView
		/// </summary>
		IDataGridViewColors DataGridViewColors { get; }
	}
}
