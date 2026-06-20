/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Texts
{
	/// <summary>
	/// Themed label with custom rendering
	/// </summary>
	public class NostalgicLabel : Label, IThemeControl, IFontConfiguration
	{
		private ILabelColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		// The bottom edge (in parent coordinates) the designer intended for a
		// bottom-anchored label, captured from the placeholder size before
		// AutoSize discards it. int.MinValue means not captured yet
		private int anchoredBottom = int.MinValue;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicLabel()
		{
			AutoSize = true;

			// Make the themed font available before the designer's first layout
			// pass. Otherwise the font would first arrive in OnHandleCreated,
			// after deserialization, and the AutoSize re-measure would change
			// the serialized size on every reload, making the designer keep
			// marking the file as changed
			if (DesignerHelper.IsInDesignMode())
				SetTheme(new StandardTheme());
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Indicates whether the control is automatically resized to fit its
		/// contents. Defaults to true for this control
		/// </summary>
		/********************************************************************/
		[DefaultValue(true)]
		public override bool AutoSize
		{
			get => base.AutoSize;
			set => base.AutoSize = value;
		}



		/********************************************************************/
		/// <summary>
		/// Set the FontConfiguration component to use for this control
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("Which font configuration to use if you want to change the default font.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(null)]
		public FontConfiguration UseFont
		{
			get => fontConfiguration;

			set
			{
				if (fontConfiguration != null)
					fontConfiguration.FontChanged -= FontConfiguration_FontChanged;

				fontConfiguration = value;

				if (fontConfiguration != null)
					fontConfiguration.FontChanged += FontConfiguration_FontChanged;

				if (IsHandleCreated)
				{
					UpdateFont();
					Invalidate();
				}
			}
		}
		#endregion

		#region Initialize
		/********************************************************************/
		/// <summary>
		/// Initialize the control to use custom rendering
		/// </summary>
		/********************************************************************/
		protected override void OnHandleCreated(EventArgs e)
		{
			if (DesignerHelper.IsInDesignMode(this))
				SetTheme(new StandardTheme());

			UpdateFont();

			base.OnHandleCreated(e);
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// The default WinForms AutoSize keeps the top left corner fixed
		/// and grows the width to the right and the height downwards. For a
		/// label that is anchored to the right and/or bottom edge (but not
		/// the opposite edge), that pushes the text past that edge instead
		/// of keeping the edge fixed. We want the label to grow towards the
		/// anchored edge instead, so move the location so the anchored edge
		/// stays put when the size changes due to auto-sizing
		/// </summary>
		/********************************************************************/
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (AutoSize && !DesignerHelper.IsInDesignMode(this))
			{
				// A bottom-anchored auto-size label grows downwards from its top
				// left, so its bottom edge ends up below where the designer
				// placed it. The designer's intended bottom is Location.Y plus
				// the small placeholder height it assigned - but AutoSize discards
				// that explicit size right after, so this explicit size set is the
				// only place to read it. The placeholder is an explicit size
				// shorter than the text content needs
				bool placeholder = ((specified & BoundsSpecified.Size) != 0) && (height < base.GetPreferredSize(Size.Empty).Height);

				if (placeholder && IsBottomAnchoredOnly())
				{
					// Capture the intended bottom and immediately move the label
					// so its real (auto-sized) height sits with that bottom edge.
					// Doing it here - before the first layout captures the anchor
					// distance - lets the WinForms bottom anchor then track from
					// the correct edge, so the label still follows the parent on
					// resize (it must not be pinned to an absolute position)
					anchoredBottom = y + height;
					y = anchoredBottom - base.GetPreferredSize(Size.Empty).Height;
				}

				// Right-anchored (but not left): keep the right edge fixed when
				// the text grows the width, so the label grows to the left
				if ((width != Width) && !placeholder && IsRightAnchoredOnly())
					x = Right - width;

				// Bottom-anchored (but not top): keep the bottom edge fixed when
				// the text auto-sizes the height (a size change), so the label
				// grows upwards. Not on anchor repositioning (height unchanged) -
				// that is left to the anchor so the label follows the parent on
				// resize
				if ((height != Height) && !placeholder && IsBottomAnchoredOnly() && (anchoredBottom != int.MinValue))
					y = anchoredBottom - height;
			}

			base.SetBoundsCore(x, y, width, height, specified);
		}



		/********************************************************************/
		/// <summary>
		/// When the label is anchored to the far (right/bottom) edge, the
		/// size set in the designer defines the fixed anchor corner. Keep
		/// that size at design time instead of letting auto-size collapse it
		/// to the (empty) text size, so the corner stays where it was
		/// placed. This is what allows the label to keep a fixed size like
		/// 6;2 here
		/// </summary>
		/********************************************************************/
		public override Size GetPreferredSize(Size proposedSize)
		{
			if (DesignerHelper.IsInDesignMode(this) && IsAnchoredToFarEdge())
				return Size;

			return base.GetPreferredSize(proposedSize);
		}
		#endregion

		#region Theme
		/********************************************************************/
		/// <summary>
		/// Will setup the theme for the control
		/// </summary>
		/********************************************************************/
		public void SetTheme(ITheme theme)
		{
			colors = theme.LabelColors;
			fonts = theme.StandardFonts;

			BackColor = Color.Transparent;
			ForeColor = colors.TextColor;

			UpdateFont();

			Invalidate();
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// React when the attached FontConfiguration recalculates its font
		/// (e.g. theme manager just initialized, or one of FontType /
		/// FontStyle / FontSize changed at runtime)
		/// </summary>
		/********************************************************************/
		private void FontConfiguration_FontChanged(object sender, EventArgs e)
		{
			if (IsHandleCreated)
			{
				UpdateFont();
				Invalidate();
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Set font to use
		/// </summary>
		/********************************************************************/
		private void UpdateFont()
		{
			Font = fontConfiguration?.Font ?? fonts.RegularFont;
		}



		/********************************************************************/
		/// <summary>
		/// Is the label anchored to the right edge, but not the left
		/// </summary>
		/********************************************************************/
		private bool IsRightAnchoredOnly()
		{
			return ((Anchor & AnchorStyles.Right) != 0) && ((Anchor & AnchorStyles.Left) == 0);
		}



		/********************************************************************/
		/// <summary>
		/// Is the label anchored to the bottom edge, but not the top
		/// </summary>
		/********************************************************************/
		private bool IsBottomAnchoredOnly()
		{
			return ((Anchor & AnchorStyles.Bottom) != 0) && ((Anchor & AnchorStyles.Top) == 0);
		}



		/********************************************************************/
		/// <summary>
		/// Is the label anchored to a far (right or bottom) edge, where the
		/// designer size defines the fixed anchor corner
		/// </summary>
		/********************************************************************/
		private bool IsAnchoredToFarEdge()
		{
			return IsRightAnchoredOnly() || IsBottomAnchoredOnly();
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicLabel()
		{
			TypeDescriptor.AddProvider(new NostalgicLabelTypeDescriptionProvider(), typeof(NostalgicLabel));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicLabelTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(Label));

			private static readonly string[] propertiesToHide =
			[
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(BorderStyle),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft)
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicLabelTypeDescriptionProvider() : base(parent)
			{
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
			{
				return new HidingTypeDescriptor(base.GetTypeDescriptor(objectType, instance), propertiesToHide);
			}
		}
		#endregion
	}
}
