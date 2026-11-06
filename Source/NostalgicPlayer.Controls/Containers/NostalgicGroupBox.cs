/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// Themed panel with a rounded border and a header text, similar to a
	/// WinForms GroupBox
	/// </summary>
	public class NostalgicGroupBox : Panel, IThemeControl
	{
		private const int BorderWidth = 1;
		private const int CornerRadius = 3;

		// Left offset of the header text and the empty space around it where
		// the top border line is interrupted
		private const int TextOffset = 7;
		private const int TextGap = 0;

		// Extra space between the border and the content area
		private const int InnerPadding = 2;

		private IGroupBoxColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicGroupBox()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);

			// Make the themed font available before the designer's first layout
			// pass, so DisplayRectangle/HeaderHeight is identical at serialize
			// time and on every reload. Otherwise the docked children would be
			// re-laid-out after deserialization (once the font arrives in
			// OnHandleCreated), shifting them a pixel and making the designer
			// keep marking the file as changed
			if (DesignerHelper.IsInDesignMode())
				SetTheme(new StandardTheme());
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// The header text shown at the top of the group
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("The header text shown at the top of the group.")]
		[Browsable(true)]
		[EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("")]
		public override string Text
		{
			get => base.Text;

			set
			{
				base.Text = value;

				// The header height depends on whether there is any text, so
				// the content area may have changed
				PerformLayout();
				Invalidate();
			}
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

				Invalidate();
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

			base.OnHandleCreated(e);
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
			colors = theme.GroupBoxColors;
			fonts = theme.StandardFonts;

			SetBackgroundColor();

			// The header height depends on the font, so the content area may
			// have changed
			PerformLayout();
			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Update the background color
		/// </summary>
		/********************************************************************/
		private void SetBackgroundColor()
		{
			BackColor = Color.Transparent;
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the client area minus the border and header, so anchoring
		/// and docking calculations use the correct size
		/// </summary>
		/********************************************************************/
		public override Rectangle DisplayRectangle
		{
			get
			{
				Rectangle rect = base.DisplayRectangle;

				int header = HeaderHeight;
				int pad = BorderWidth + InnerPadding;

				return new Rectangle(rect.X + pad, rect.Y + header + pad, rect.Width - (2 * pad), rect.Height - header - (2 * pad));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Repaint when the size changes so the rounded border stays sharp
		/// </summary>
		/********************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics g = e.Graphics;

			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

			Font font = GetFont();

			DrawGroup(g, font);
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
			PerformLayout();
			Invalidate();
		}
		#endregion

		#region Private properties
		/********************************************************************/
		/// <summary>
		/// The height reserved at the top for the header text
		/// </summary>
		/********************************************************************/
		private int HeaderHeight
		{
			get
			{
				Font font = GetFont();

				return string.IsNullOrEmpty(Text) ? 0 : font.Height;
			}
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Return the font to use
		/// </summary>
		/********************************************************************/
		private Font GetFont()
		{
			return fontConfiguration?.Font ?? fonts.GroupFont;
		}



		/********************************************************************/
		/// <summary>
		/// Draw the rounded border, the optional background and the header
		/// text
		/// </summary>
		/********************************************************************/
		private void DrawGroup(Graphics g, Font font)
		{
			int header = HeaderHeight;

			// The top border line runs through the vertical middle of the header text
			int top = header / 2;
			Rectangle border = new Rectangle(0, top, Width - 1, Height - 1 - top);

			using (GraphicsPath path = CreateRoundedRectangle(border, CornerRadius))
			{
				DrawBorder(g, font, path, header);
			}

			if (header > 0)
				TextRenderer.DrawText(g, Text, font, new Point(TextOffset, 0), colors.HeaderColor);
		}



		/********************************************************************/
		/// <summary>
		/// Draw the themed border, leaving a gap for the header text
		/// </summary>
		/********************************************************************/
		private void DrawBorder(Graphics g, Font font, GraphicsPath path, int header)
		{
			if (header > 0)
			{
				// Punch a hole in the top border line where the text sits
				Size textSize = TextRenderer.MeasureText(g, Text, font);
				Rectangle gap = new Rectangle(TextOffset - TextGap, 0, textSize.Width + (2 * TextGap), header);

				g.SetClip(gap, CombineMode.Exclude);
			}

			using (Pen borderPen = new Pen(colors.BorderColor, BorderWidth))
			{
				g.DrawPath(borderPen, path);
			}

			g.ResetClip();
		}



		/********************************************************************/
		/// <summary>
		/// Create a rounded rectangle path within the given bounds
		/// </summary>
		/********************************************************************/
		private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
		{
			GraphicsPath path = new GraphicsPath();

			// Make sure the radius never gets bigger than the bounds allow
			int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));

			if (diameter <= 0)
			{
				path.AddRectangle(bounds);
				return path;
			}

			path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
			path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
			path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
			path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
			path.CloseFigure();

			return path;
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicGroupBox()
		{
			TypeDescriptor.AddProvider(new NostalgicGroupBoxTypeDescriptionProvider(), typeof(NostalgicGroupBox));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer
		/// </summary>
		private sealed class NostalgicGroupBoxTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(Panel));

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
			public NostalgicGroupBoxTypeDescriptionProvider() : base(parent)
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
