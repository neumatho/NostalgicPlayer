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

namespace Polycode.NostalgicPlayer.Controls.Inputs
{
	/// <summary>
	/// Themed single-line text box
	/// </summary>
	public partial class NostalgicTextBox : UserControl, IThemeControl, IFontConfiguration
	{
		private IFonts fonts;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicTextBox()
		{
			InitializeComponent();

			nostalgicTextBoxInternal.TextChanged += NostalgicRichTextBox_TextChanged;
			nostalgicTextBoxInternal.FontChanged += NostalgicTextBoxInternal_FontChanged;
		}

		#region Designer properties
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
			get => nostalgicTextBoxInternal.UseFont;

			set => nostalgicTextBoxInternal.UseFont = value;
		}
		#endregion

		#region Redirected properties
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectedText
		{
			get => nostalgicTextBoxInternal.SelectedText;

			set => nostalgicTextBoxInternal.SelectedText = value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionLength
		{
			get => nostalgicTextBoxInternal.SelectionLength;

			set => nostalgicTextBoxInternal.SelectionLength = value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionStart
		{
			get => nostalgicTextBoxInternal.SelectionStart;

			set => nostalgicTextBoxInternal.SelectionStart = value;
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
			fonts = theme.StandardFonts;

			ClampHeight();
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

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Forward the text to the inner text box
		/// </summary>
		/********************************************************************/
		public override string Text
		{
			get => nostalgicTextBoxInternal.Text;

			set => nostalgicTextBoxInternal.Text = value;
		}



		/********************************************************************/
		/// <summary>
		/// The computed height acts as a minimum. A larger height set by the
		/// designer or layout is kept; a smaller one is bumped up. Enforced
		/// here so it also applies in the designer, where the theme (and
		/// thus ApplyResolvedFont) may not have run yet
		/// </summary>
		/********************************************************************/
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			// Only enforce once a real theme font is resolved. Before that
			// (e.g. during InitializeComponent) we must not clamp using the
			// default WinForms font, or a wrong minimum would be locked in
			int minHeight = CalculateMinimumHeight();
			if ((minHeight > 0) && (height < minHeight))
				height = minHeight;

			base.SetBoundsCore(x, y, width, height, specified);
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Re-raise the inner text box TextChanged event as our own
		/// </summary>
		/********************************************************************/
		private void NostalgicRichTextBox_TextChanged(object sender, EventArgs e)
		{
			OnTextChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// Re-clamp the height when the inner control's font changes. The
		/// inner control owns the FontConfiguration and pushes the resolved
		/// font onto itself; setting its Font raises this event, which is
		/// also where a runtime FontType / FontStyle / FontSize change ends
		/// up
		/// </summary>
		/********************************************************************/
		private void NostalgicTextBoxInternal_FontChanged(object sender, EventArgs e)
		{
			ClampHeight();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Re-clamp the current height. Growing the font bumps the height
		/// up; shrinking it keeps a larger height (the computed value is
		/// only a floor, enforced live in SetBoundsCore - never stored in
		/// MinimumSize so the designer cannot serialize a stale value). The
		/// inner control owns the FontConfiguration and applies the resolved
		/// font to itself
		/// </summary>
		/********************************************************************/
		private void ClampHeight()
		{
			int minHeight = CalculateMinimumHeight();
			if (Height < minHeight)
				Height = minHeight;
		}



		/********************************************************************/
		/// <summary>
		/// The font to use for sizing: the FontConfiguration font if set,
		/// otherwise the theme's regular font. Null until a theme has been
		/// applied (we never fall back to the default WinForms font, which
		/// would give a wrong height)
		/// </summary>
		/********************************************************************/
		private Font GetResolvedFont()
		{
			return nostalgicTextBoxInternal.UseFont?.Font ?? fonts?.RegularFont;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the minimum height the control may have: the resolved
		/// font height plus the chrome NostalgicRichTextBox draws (1px
		/// border + the vertical padding on top and bottom). Returns 0 when
		/// no theme font is resolved yet, meaning "do not enforce"
		/// </summary>
		/********************************************************************/
		private int CalculateMinimumHeight()
		{
			Font font = GetResolvedFont();
			if (font == null)
				return 0;

			return font.Height + ((NostalgicTextBoxInternal.BorderWidth + NostalgicTextBoxInternal.VerticalPadding) * 2);
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicTextBox()
		{
			TypeDescriptor.AddProvider(new NostalgicTextBoxTypeDescriptionProvider(), typeof(NostalgicTextBox));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicTextBoxTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(RichTextBox));

			private static readonly string[] propertiesToHide =
			[
				nameof(AutoSize),
				nameof(AutoSizeMode),
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
			public NostalgicTextBoxTypeDescriptionProvider() : base(parent)
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
