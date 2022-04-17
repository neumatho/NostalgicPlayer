/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.GuiKit.Components
{
	/// <summary>
	/// Add this palette to your forms to use standard NostalgicPlayer font.
	/// Set the palette property on all your controls
	/// </summary>
	[ToolboxItem(true)]
	public partial class FontPalette : PaletteOffice2010Blue
	{
		private const float DefaultFontSize = 8.0f;

		private static readonly Padding buttonPadding = new Padding(1, 0, 1, 1);
		private static readonly Padding gridPadding = new Padding(0);

		private bool monospaceOnGrid = false;
		private float baseFontSize = DefaultFontSize;

		private Font font;
		private Font tabFont;
		private Font gridFont;
		private Font groupFont;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FontPalette()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FontPalette(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
		}



		/********************************************************************/
		/// <summary> 
		/// Clean up any resources being used
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				font.Dispose();
				tabFont.Dispose();
				gridFont.Dispose();
				groupFont.Dispose();

				components.Dispose();
			}

			base.Dispose(disposing);
		}



		/********************************************************************/
		/// <summary>
		/// Return the regular font
		/// </summary>
		/********************************************************************/
		public static Font GetRegularFont(float size = DefaultFontSize)
		{
			return new Font("Lucida Unicode", size, FontStyle.Regular, GraphicsUnit.Point);
		}



		/********************************************************************/
		/// <summary>
		/// Return the monospace font
		/// </summary>
		/********************************************************************/
		public static Font GetMonospaceFont(float size = DefaultFontSize)
		{
			return new Font("Lucida Console", size, FontStyle.Regular, GraphicsUnit.Point);
		}



		/********************************************************************/
		/// <summary>
		/// Tell whether you want to use monospace font on data grid controls
		/// </summary>
		/********************************************************************/
		[DefaultValue(false)]
		public bool UseMonospaceOnGrid
		{
			get => monospaceOnGrid;

			set
			{
				if (value != monospaceOnGrid)
				{
					monospaceOnGrid = value;
					DefineFonts();
					OnPalettePaint(this, new PaletteLayoutEventArgs(true, false));
				}
			}
		}

		#region PaletteOffice2010Blue overrides
		/********************************************************************/
		/// <summary>
		/// Hide the base font name, since we do not allow it to be changed
		/// </summary>
		/********************************************************************/
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public override string BaseFontName => null;



		/********************************************************************/
		/// <summary>
		/// Hide the base font size, since we do not allow it to be changed
		/// </summary>
		/********************************************************************/
		[DefaultValue(DefaultFontSize)]
		public override float BaseFontSize
		{
			get => baseFontSize;

			set
			{
				if ((value > 0.0f) && (value != baseFontSize))
				{
					baseFontSize = value;
					DefineFonts();
					OnPalettePaint(this, new PaletteLayoutEventArgs(true, false));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return font for short text
		/// </summary>
		/********************************************************************/
		public override Font GetContentShortTextFont(PaletteContentStyle style, PaletteState state)
		{
			switch (style)
			{
				case PaletteContentStyle.TabHighProfile:
					return tabFont;

				case PaletteContentStyle.GridDataCellSheet:
				case PaletteContentStyle.GridDataCellList:
					return gridFont;

				case PaletteContentStyle.LabelGroupBoxCaption:
					return groupFont;
			}

			return font;
		}



		/********************************************************************/
		/// <summary>
		/// Return font for long text
		/// </summary>
		/********************************************************************/
		public override Font GetContentLongTextFont(PaletteContentStyle style, PaletteState state)
		{
			return font;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the padding between the border and content drawing
		/// </summary>
		/********************************************************************/
		public override Padding GetContentPadding(PaletteContentStyle style, PaletteState state)
		{
			switch (style)
			{
				case PaletteContentStyle.ButtonStandalone:
					return buttonPadding;

				case PaletteContentStyle.GridDataCellSheet:
				case PaletteContentStyle.GridDataCellList:
					return gridPadding;
			}

			return base.GetContentPadding(style, state);
		}



		/********************************************************************/
		/// <summary>
		/// Update the fonts to reflect system or user defined changes
		/// </summary>
		/********************************************************************/
		protected override void DefineFonts()
		{
			// Release existing resources
			font?.Dispose();
			tabFont?.Dispose();
			gridFont?.Dispose();
			groupFont?.Dispose();

			font = GetRegularFont(BaseFontSize);
			tabFont = GetRegularFont(BaseFontSize + 2.0f);
			gridFont = UseMonospaceOnGrid ? GetMonospaceFont(BaseFontSize) : GetRegularFont(BaseFontSize);
			groupFont = GetRegularFont(BaseFontSize + 1.0f);
		}
		#endregion
	}
}
