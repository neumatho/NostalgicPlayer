/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Controls.Buttons
{
	/// <summary>
	/// Themed image button that can be toggled between checked and
	/// unchecked state. When checked, the button is rendered with
	/// pressed colors
	/// </summary>
	public class NostalgicToggleImageButton : NostalgicImageButton
	{
		private bool isChecked;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicToggleImageButton()
		{
			AccessibleRole = AccessibleRole.CheckButton;
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Whether the button is in the checked state
		/// </summary>
		/********************************************************************/
		[Category("Behavior")]
		[Description("Whether the button is in the checked state.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(false)]
		public bool Checked
		{
			get => isChecked;

			set
			{
				if (value != isChecked)
				{
					isChecked = value;

					OnCheckedChanged(EventArgs.Empty);
					Invalidate();
				}
			}
		}
		#endregion

		#region Events
		/********************************************************************/
		/// <summary>
		/// Fires when Checked changes
		/// </summary>
		/********************************************************************/
		[Category("Behavior")]
		[Description("Fires when Checked changes.")]
		public event EventHandler CheckedChanged;



		/********************************************************************/
		/// <summary>
		/// Fires the CheckedChanged event
		/// </summary>
		/********************************************************************/
		protected virtual void OnCheckedChanged(EventArgs e)
		{
			CheckedChanged?.Invoke(this, e);
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Toggle the checked state on click
		/// </summary>
		/********************************************************************/
		protected override void OnClick(EventArgs e)
		{
			Checked = !Checked;

			base.OnClick(e);
		}



		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the current state
		/// </summary>
		/********************************************************************/
		protected override StateColors GetColors()
		{
			// Let base handle disabled, active press, and hover states first
			if (!Enabled || isPressed || isHovered)
				return base.GetColors();

			// When checked (and not pressed/hovered/disabled), use pressed colors
			if (isChecked)
			{
				return new StateColors
				{
					BorderColor = colors.PressedBorderColor,
					BackgroundStartColor = colors.PressedBackgroundStartColor,
					BackgroundStopColor = colors.PressedBackgroundStopColor
				};
			}

			// Let base handle focused/normal
			return base.GetColors();
		}
		#endregion
	}
}
