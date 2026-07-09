//---------------------------------------------------------------------------------------
// <copyright file="ModernTrackerPainter.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Modern tracker renderer for SystemLight and SystemDark styles
	/// </summary>
	internal partial class ModernTrackerPainter : TrackerPainterBase
	{
		// Rendering constants
		private const int TEXT_PADDING_WIDTH = 4;
		private const int TEXT_PADDING_HEIGHT = 2;
		private const int SEPARATOR_WIDTH = 1;

		private readonly Brush volumeBarBrush = new SolidBrush(Color.FromArgb(0, 255, 0));

		// Theme colors
		private Color bgColor;
		private Pen channelSeparatorPen;
		private Brush channelStripeBgBrush;
		private Pen columnSeparatorPen;
		private Color controlBgColor;
		private Brush currentRowBrush;
		private Brush effectColorBrush;
		private Pen gridPen;
		private Brush headerBgBrush;
		private Brush headerBrush;
		private Font headerFont;
		private Font headerFontSmall;
		private Brush headerMutedBrush;
		private Brush instrumentColorBrush;
		private Brush noteColorBrush;
		private Pen patternBoundaryPen;
		private Brush rowBgBrush;
		private Color scrollButtonDisabledColor;
		private Color scrollButtonHoverColor;
		private Color scrollButtonNormalColor;
		private int scrollPosition;
		private Brush statusBgBrush;
		private Brush textBrush;
		private Brush volumeColorBrush;

		/********************************************************************/
		/// <summary>
		/// Dispose resources
		/// </summary>
		/********************************************************************/
		public override void Dispose()
		{
			headerFont?.Dispose();
			headerFontSmall?.Dispose();
			volumeBarBrush?.Dispose();
			gridPen?.Dispose();
			patternBoundaryPen?.Dispose();
			channelSeparatorPen?.Dispose();
			columnSeparatorPen?.Dispose();
			textBrush?.Dispose();
			headerBrush?.Dispose();
			headerMutedBrush?.Dispose();
			currentRowBrush?.Dispose();
			rowBgBrush?.Dispose();
			statusBgBrush?.Dispose();
			headerBgBrush?.Dispose();
			channelStripeBgBrush?.Dispose();
			noteColorBrush?.Dispose();
			instrumentColorBrush?.Dispose();
			volumeColorBrush?.Dispose();
			effectColorBrush?.Dispose();
		}

		/********************************************************************/
		/// <summary>
		/// Update style based on TrackerStyle
		/// </summary>
		/********************************************************************/
		internal override TrackerStyleColors UpdateStyle(string styleId)
		{
			bool isDark = styleId == StyleIdDark;
			DisplayColors colors = DisplayColors.GetDisplayColors(isDark);

			// Store theme colors
			bgColor = isDark ? Color.FromArgb(0x1E, 0x1E, 0x1E) : Color.White;
			controlBgColor = isDark ? Color.FromArgb(0x2D, 0x2D, 0x2D) : Color.FromArgb(0xF0, 0xF0, 0xF0);

			// Scroll button colors
			if (isDark)
			{
				scrollButtonHoverColor = Color.FromArgb(0x50, 0x50, 0x50);
				scrollButtonNormalColor = Color.FromArgb(0x40, 0x40, 0x40);
				scrollButtonDisabledColor = Color.FromArgb(0x30, 0x30, 0x30);
			}
			else
			{
				scrollButtonHoverColor = Color.FromArgb(0xe0, 0xe0, 0xe0);
				scrollButtonNormalColor = Color.FromArgb(0xf0, 0xf0, 0xf0);
				scrollButtonDisabledColor = Color.FromArgb(0xe6, 0xe6, 0xe6);
			}

			// Dispose old resources
			gridPen?.Dispose();
			patternBoundaryPen?.Dispose();
			channelSeparatorPen?.Dispose();
			columnSeparatorPen?.Dispose();
			textBrush?.Dispose();
			headerBrush?.Dispose();
			headerMutedBrush?.Dispose();
			headerFont?.Dispose();
			headerFontSmall?.Dispose();
			currentRowBrush?.Dispose();
			rowBgBrush?.Dispose();
			statusBgBrush?.Dispose();
			headerBgBrush?.Dispose();
			channelStripeBgBrush?.Dispose();
			noteColorBrush?.Dispose();
			instrumentColorBrush?.Dispose();
			volumeColorBrush?.Dispose();
			effectColorBrush?.Dispose();

			// Store style and settings
			CurrentStyleId = styleId;
			ChannelSeparatorWidth = colors.ChannelSeparatorWidth;
			DrawSeparatorAfterLastChannel = colors.DrawSeparatorAfterLastChannel;

			// Create header fonts (small version for when track + transpose is shown)
			headerFont = new Font("Consolas", GetPatternFontSize());
			headerFontSmall = new Font("Consolas", GetPatternFontSize() - 2);

			// Create new pens/brushes
			gridPen = new Pen(colors.GridColor);
			patternBoundaryPen = new Pen(colors.PatternBoundaryColor);
			channelSeparatorPen = new Pen(colors.ChannelSeparatorColor, colors.ChannelSeparatorWidth);
			columnSeparatorPen = new Pen(colors.ColumnSeparatorColor);
			textBrush = new SolidBrush(colors.TextColor);
			headerBrush = new SolidBrush(colors.HeaderColor);
			headerMutedBrush = new SolidBrush(Color.FromArgb(0x60, 0x60, 0x60));
			currentRowBrush = new SolidBrush(colors.CurrentRowColor);
			rowBgBrush = colors.RowBgColor.HasValue ? new SolidBrush(colors.RowBgColor.Value) : null;
			statusBgBrush = new SolidBrush(colors.StatusBgColor);
			headerBgBrush = new SolidBrush(colors.HeaderBgColor);
			channelStripeBgBrush = new SolidBrush(colors.ChannelStripeBgColor);

			// Create colored brushes for different pattern elements (adjust for dark/light mode)
			if (isDark)
			{
				noteColorBrush = new SolidBrush(Color.FromArgb(100, 200, 255)); // Light blue for notes
				instrumentColorBrush = new SolidBrush(Color.FromArgb(255, 200, 100)); // Orange for instruments
				volumeColorBrush = new SolidBrush(Color.FromArgb(150, 255, 150)); // Light green for volume
				effectColorBrush = new SolidBrush(Color.FromArgb(180, 140, 255)); // Violet for effects
			}
			else
			{
				noteColorBrush = new SolidBrush(Color.FromArgb(30, 100, 180)); // Darker blue for notes
				instrumentColorBrush = new SolidBrush(Color.FromArgb(200, 100, 0)); // Darker orange for instruments
				volumeColorBrush = new SolidBrush(Color.FromArgb(0, 150, 0)); // Darker green for volume
				effectColorBrush = new SolidBrush(Color.FromArgb(120, 80, 180)); // Darker violet for effects
			}

			// Return background colors
			return new TrackerStyleColors {PatternPanelBackColor = bgColor, ControlBackColor = controlBgColor};
		}

		/********************************************************************/
		/// <summary>
		/// Create rendering metrics for Modern tracker style
		/// </summary>
		/********************************************************************/
		protected RenderMetrics CreateRenderMetrics(Graphics g)
		{
			Font patternFont = new("Consolas", GetPatternFontSize());
			Font statusFont = new("Consolas", GetPatternFontSize());

			// Resolve number formats from display modes
			// Modern Tracker defaults: RowNum=Hexadecimal, Instrument=Hexadecimal, Volume=Hexadecimal
			NumberFormat rowNumFormat = RowNumberDisplayMode.Resolve(NumberFormat.Hexadecimal);
			NumberFormat instrumentFormat = InstrumentDisplayMode.Resolve(NumberFormat.Hexadecimal);
			NumberFormat volumeFormat = VolumeDisplayMode.Resolve(NumberFormat.Hexadecimal);

			// Resolve grid lines mode
			// Modern Tracker default: Grid lines ON
			bool drawGrid = GridLinesDisplayMode.Resolve(true);

			// Measure character dimensions
			float charWidth, charHeight;
			using (StringFormat format = StringFormat.GenericTypographic)
			{
				SizeF oneChar = g.MeasureString("W", patternFont, PointF.Empty, format);
				charWidth = oneChar.Width;
				charHeight = oneChar.Height;
			}

			// Calculate row number digits based on max row number and format
			int rowNumberDigits = PatternRenderingHelpers.CalculateRowNumberDigits(MaxRowCount, rowNumFormat);

			int rowNumberWidth = (TEXT_PADDING_WIDTH * 2) + RendererCharHelper.CalcCharsWidth(rowNumberDigits, charWidth);
			int rowHeight = (TEXT_PADDING_HEIGHT * 2) + RendererCharHelper.CalcLinesHeight(1, charHeight);
			int headerHeight = (TEXT_PADDING_HEIGHT * 2) + RendererCharHelper.CalcLinesHeight(1, charHeight);

			int statusLineHeight = (int)charHeight;
			int statusHeight = (TEXT_PADDING_HEIGHT * 4) + RendererCharHelper.CalcLinesHeight(3, charHeight);

			int scrollButtonWidth = (rowNumberWidth - 2) / 2;

			// Calculate channel width and column positions (with separators for modern styles)
			int effectChars = CurrentDisplayMode.GetEffectChars(MaxEffectCount, EffectCharCount);
			int channelWidth = 0;

			// Start with channel separator space (gap before channel content)
			channelWidth += ChannelSeparatorWidth;

			// note
			int noteTextX = channelWidth + TEXT_PADDING_WIDTH;
			channelWidth += (TEXT_PADDING_WIDTH * 2) + RendererCharHelper.CalcCharsWidth(3, charWidth);
			int noteSeparatorX = channelWidth;
			channelWidth += SEPARATOR_WIDTH;

			// instrument - shown in Full and CompactX modes
			int? instrumentTextX = null;
			int? instrumentSeparatorX = null;
			if (CurrentDisplayMode != DisplayMode.NotesOnly)
			{
				instrumentTextX = channelWidth + TEXT_PADDING_WIDTH;
				channelWidth += (TEXT_PADDING_WIDTH * 2) + RendererCharHelper.CalcCharsWidth(2, charWidth);
				instrumentSeparatorX = channelWidth;
				channelWidth += SEPARATOR_WIDTH;
			}

			int? volumeTextX = null;
			int? volumeSeparatorX = null;
			int? effectTextX = null;

			// Volume and Effects in Full and SingleEffect modes
			if (CurrentDisplayMode.ShowsEffects(effectChars))
			{
				if (HasVolumeColumn)
				{
					// volume
					volumeTextX = channelWidth + TEXT_PADDING_WIDTH;
					channelWidth += (TEXT_PADDING_WIDTH * 2) + RendererCharHelper.CalcCharsWidth(2, charWidth);
					volumeSeparatorX = channelWidth;
					channelWidth += SEPARATOR_WIDTH;
				}

				// effect
				effectTextX = channelWidth + TEXT_PADDING_WIDTH;
				channelWidth += (TEXT_PADDING_WIDTH * 2) + RendererCharHelper.CalcCharsWidth(effectChars, charWidth);
				channelWidth += SEPARATOR_WIDTH;
			}

			// Remove last column separator (channel separator was already added at the start)
			channelWidth -= SEPARATOR_WIDTH;

			return new RenderMetrics
			{
				PatternFont = patternFont,
				StatusFont = statusFont,
				TextPaddingWidth = TEXT_PADDING_WIDTH,
				TextPaddingHeight = TEXT_PADDING_HEIGHT,
				RowNumberWidth = rowNumberWidth,
				RowHeight = rowHeight,
				HeaderHeight = headerHeight,
				StatusHeight = statusHeight,
				StatusLineHeight = statusLineHeight,
				ScrollButtonWidth = scrollButtonWidth,
				ChannelWidth = channelWidth,
				SeparatorWidth = SEPARATOR_WIDTH,
				NoteWidth = (TEXT_PADDING_WIDTH * 2) + RendererCharHelper.CalcCharsWidth(3, charWidth),
				InstrumentWidth = (TEXT_PADDING_WIDTH * 2) + RendererCharHelper.CalcCharsWidth(2, charWidth),
				VolumeWidth =
					HasVolumeColumn
						? (TEXT_PADDING_WIDTH * 2) + RendererCharHelper.CalcCharsWidth(2, charWidth)
						: 0,
				EffectWidth = (TEXT_PADDING_WIDTH * 2) + RendererCharHelper.CalcCharsWidth(effectChars, charWidth),
				DataWidth = 0,
				NotePos = new ColumnPosition {Text = noteTextX, Separator = noteSeparatorX},
				InstrumentPos = new ColumnPosition {Text = instrumentTextX, Separator = instrumentSeparatorX},
				VolumePos = new ColumnPosition {Text = volumeTextX, Separator = volumeSeparatorX},
				EffectPos = new ColumnPosition {Text = effectTextX, Separator = null},
				RowNumberFormat = rowNumFormat,
				InstrumentFormat = instrumentFormat,
				VolumeFormat = volumeFormat,
				DrawGrid = drawGrid
			};
		}

		/********************************************************************/
		/// <summary>
		/// Draw the complete pattern grid (legacy - now handled by RenderViewer)
		/// </summary>
		/********************************************************************/
		internal override void DrawSimpleGrid(Graphics g)
		{
			// Empty - rendering is now handled by RenderViewer in ModernTrackerRenderer.Render.cs
		}
	}
}
