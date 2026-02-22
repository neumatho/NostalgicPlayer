/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Components
{
	/// <summary>
	/// Add this component to your forms if you want to change the
	/// standard font size and/or style
	/// </summary>
	[ToolboxItem(true)]
	public partial class FontConfiguration : Component
	{
		private FontType fontType = FontType.Regular;
		private FontStyle fontStyle = FontStyle.Regular;
		private int relativeFontSize = 0;

		private Font font;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FontConfiguration()
		{
			InitializeComponent();

			Disposed += FontConfiguration_Disposed;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FontConfiguration(IContainer container) : this()
		{
			container.Add(this);
		}

		#region Properties
		/********************************************************************/
		/// <summary>
		/// Holds the defined font or null if the standard font should be
		/// used
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Font Font => font;
		#endregion

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Tell what font type you want
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("The font type to use.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(FontType.Regular)]
		public FontType FontType
		{
			get => fontType;

			set
			{
				if (value != fontType)
				{
					fontType = value;
					DefineFont();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tell what font style you want
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("The font style to use.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(FontStyle.Regular)]
		public FontStyle FontStyle
		{
			get => fontStyle;

			set
			{
				if (value != fontStyle)
				{
					fontStyle = value;
					DefineFont();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tell the size of the font relative to the standard font size
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("The relative font size to use.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(0)]
		public int FontSize
		{
			get => relativeFontSize;

			set
			{
				if (value != relativeFontSize)
				{
					relativeFontSize = value;
					DefineFont();
				}
			}
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FontConfiguration_Disposed(object sender, EventArgs e)
		{
			font?.Dispose();
			font = null;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Update the font to reflect user defined changes
		/// </summary>
		/********************************************************************/
		private void DefineFont()
		{
			font?.Dispose();
			font = null;

			IThemeManager themeManager = ThemeManagerFactory.GetThemeManager();

			if ((FontSize != 0) || (FontStyle != FontStyle.Regular) || (FontType != FontType.Regular))
			{
				IFonts standardFonts = themeManager.CurrentTheme.StandardFonts;
				Font baseFont = FontType == FontType.Monospace ? standardFonts.MonospaceFont : standardFonts.RegularFont;

				font = new Font(baseFont.FontFamily, baseFont.Size + FontSize, fontStyle, GraphicsUnit.Point);
			}

			themeManager.RefreshControls();
		}
		#endregion
	}
}
