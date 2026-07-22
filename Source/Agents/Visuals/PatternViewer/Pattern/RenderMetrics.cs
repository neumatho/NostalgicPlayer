//---------------------------------------------------------------------------------------
// <copyright file="RenderMetrics.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Common rendering constants
	/// </summary>
	internal static class RenderConstants
	{
		/// <summary>
		/// Horizontal padding for channel bar text clipping
		/// </summary>
		public const int ChannelBarClipPaddingHorizontal = 2;

		/// <summary>
		/// Vertical padding for channel bar text clipping
		/// </summary>
		public const int ChannelBarClipPaddingVertical = 0;
	}

	/// <summary>
	/// Number display mode (user setting)
	/// </summary>
	internal enum NumberDisplayMode
	{
		Auto, // Use tracker-specific default
		Decimal,
		Hexadecimal
	}

	/// <summary>
	/// Grid lines display mode (user setting)
	/// </summary>
	internal enum GridLinesMode
	{
		Auto, // Use tracker-specific default
		On,
		Off
	}

	/// <summary>
	/// Pattern compression mode (user setting)
	/// </summary>
	internal enum CompressPatternsMode
	{
		Auto, // Use player's AutoCompress setting
		On, // Always compress if possible
		Off // Never compress
	}

	/// <summary>
	/// Number format (resolved from NumberDisplayMode)
	/// </summary>
	internal enum NumberFormat
	{
		Decimal,
		Hexadecimal
	}

	/// <summary>
	/// Extension methods for NumberDisplayMode
	/// </summary>
	internal static class NumberDisplayModeExtensions
	{
		/// <summary>
		/// Resolve NumberDisplayMode to NumberFormat with tracker-specific default
		/// </summary>
		/// <param name="mode">The user setting</param>
		/// <param name="autoDefault">The tracker-specific default to use when mode is Auto</param>
		public static NumberFormat Resolve(this NumberDisplayMode mode, NumberFormat autoDefault)
		{
			return mode switch
			{
				NumberDisplayMode.Auto => autoDefault,
				NumberDisplayMode.Decimal => NumberFormat.Decimal,
				NumberDisplayMode.Hexadecimal => NumberFormat.Hexadecimal,
				_ => autoDefault
			};
		}
	}

	/// <summary>
	/// Extension methods for GridLinesMode
	/// </summary>
	internal static class GridLinesModeExtensions
	{
		/// <summary>
		/// Resolve GridLinesMode to bool with tracker-specific default
		/// </summary>
		/// <param name="mode">The user setting</param>
		/// <param name="autoDefault">The tracker-specific default to use when mode is Auto</param>
		public static bool Resolve(this GridLinesMode mode, bool autoDefault)
		{
			return mode switch
			{
				GridLinesMode.Auto => autoDefault,
				GridLinesMode.On => true,
				GridLinesMode.Off => false,
				_ => autoDefault
			};
		}
	}

	/// <summary>
	/// Extension methods for CompressPatternsMode
	/// </summary>
	internal static class CompressPatternsModeExtensions
	{
		/// <summary>
		/// Resolve CompressPatternsMode to bool with player-specific default
		/// </summary>
		/// <param name="mode">The user setting</param>
		/// <param name="autoDefault">The player's AutoCompress setting to use when mode is Auto</param>
		public static bool Resolve(this CompressPatternsMode mode, bool autoDefault)
		{
			return mode switch
			{
				CompressPatternsMode.Auto => autoDefault,
				CompressPatternsMode.On => true,
				CompressPatternsMode.Off => false,
				_ => autoDefault
			};
		}
	}

	/// <summary>
	/// Position info for a pattern column
	/// </summary>
	internal class ColumnPosition
	{
		public int? Text
		{
			get;
			set;
		}

		public int? Separator
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Rendering metrics calculated from font dimensions
	/// </summary>
	internal class RenderMetrics
	{
		public Font PatternFont
		{
			get;
			set;
		}

		public Font StatusFont
		{
			get;
			set;
		}

		public int TextPaddingWidth
		{
			get;
			set;
		}

		public int TextPaddingHeight
		{
			get;
			set;
		}

		public int LeftMargin
		{
			get;
			set;
		}

		public int RowNumberWidth
		{
			get;
			set;
		}

		public int RowHeight
		{
			get;
			set;
		}

		public int HeaderHeight
		{
			get;
			set;
		}

		public int StatusHeight
		{
			get;
			set;
		}

		public int StatusLineHeight
		{
			get;
			set;
		}

		public int ScrollButtonWidth
		{
			get;
			set;
		}

		public int ChannelWidth
		{
			get;
			set;
		}

		public int SeparatorWidth
		{
			get;
			set;
		}

		public int NoteWidth
		{
			get;
			set;
		}

		public int InstrumentWidth
		{
			get;
			set;
		}

		public int VolumeWidth
		{
			get;
			set;
		}

		public int EffectWidth
		{
			get;
			set;
		}

		public int DataWidth
		{
			get;
			set;
		}

		public ColumnPosition NotePos
		{
			get;
			set;
		}

		public ColumnPosition InstrumentPos
		{
			get;
			set;
		}

		public ColumnPosition VolumePos
		{
			get;
			set;
		}

		public ColumnPosition EffectPos
		{
			get;
			set;
		}

		// Format settings (resolved from user settings)
		public NumberFormat RowNumberFormat
		{
			get;
			set;
		} = NumberFormat.Decimal;

		public NumberFormat InstrumentFormat
		{
			get;
			set;
		} = NumberFormat.Hexadecimal;

		public NumberFormat VolumeFormat
		{
			get;
			set;
		} = NumberFormat.Hexadecimal;

		public NumberFormat TrackPatternFormat
		{
			get;
			set;
		} = NumberFormat.Decimal;

		public bool DrawGrid
		{
			get;
			set;
		} = true;

		public DisplayMode CurrentDisplayMode
		{
			get;
			set;
		} = DisplayMode.MultiEffect;

		/// <summary>
		/// Create VolumeBarRenderingContext from metrics
		/// </summary>
		public VolumeBarRenderingContext GetVolumeBarContext(int topOffset, int vuBarBottom, VuMeterBrushCache brushCache)
		{
			return new VolumeBarRenderingContext
			{
				TopY = topOffset + HeaderHeight,
				RowNumberWidth = RowNumberWidth,
				RowHeight = RowHeight,
				ChannelWidth = ChannelWidth,
				LeftMargin = LeftMargin,
				VuBarBottom = vuBarBottom,
				BrushCache = brushCache
			};
		}
	}

	/// <summary>
	/// Input parameters for CreateRenderMetrics - shared across all tracker MetricsCalculators
	/// </summary>
	internal readonly struct MetricsCalculatorInput
	{
		public MetricsCalculatorInput()
		{
		}

		public required NumberDisplayMode RowNumberDisplayMode
		{
			get;
			init;
		}

		public required NumberDisplayMode InstrumentDisplayMode
		{
			get;
			init;
		}

		public required NumberDisplayMode VolumeDisplayMode
		{
			get;
			init;
		}

		public required GridLinesMode GridLinesDisplayMode
		{
			get;
			init;
		}

		public required int MaxEffectCount
		{
			get;
			init;
		}

		public required int EffectCharCount
		{
			get;
			init;
		}

		public required DisplayMode CurrentDisplayMode
		{
			get;
			init;
		}

		public required bool HasVolumeColumn
		{
			get;
			init;
		}

		public required int ChannelSeparatorWidth
		{
			get;
			init;
		}

		public required int MaxRowCount
		{
			get;
			init;
		}

		public required int LeftMargin
		{
			get;
			init;
		}

		public required NumberDisplayMode TrackPatternDisplayMode
		{
			get;
			init;
		}

		/// <summary>
		/// Extra padding for status bar boxes (scheme-dependent)
		/// </summary>
		public int BoxPadding
		{
			get;
			init;
		}
	}
}
