//---------------------------------------------------------------------------------------
// <copyright file="PatternRenderResult.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Drawing;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers
{
	/// <summary>
	/// Result of pattern row rendering
	/// </summary>
	internal readonly struct PatternRenderResult
	{
		/// <summary>
		/// Y position of the current row (VU bars grow upward from here)
		/// </summary>
		public int VuBarBottom
		{
			get;
			init;
		}

		/// <summary>
		/// Height of the current row (may differ from normal row height for double-height fonts)
		/// </summary>
		public int CurrentRowHeight
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Input parameters for DrawPatternRows
	/// </summary>
	internal readonly struct DrawPatternRowsInput
	{
		public required int DisplaySongPosition
		{
			get;
			init;
		}

		public required SongPatternViewData DisplaySongPattern
		{
			get;
			init;
		}

		public required int VisibleChannels
		{
			get;
			init;
		}

		public required int VisibleRows
		{
			get;
			init;
		}

		public required int CenterRow
		{
			get;
			init;
		}

		public required RenderMetrics Metrics
		{
			get;
			init;
		}

		public required int ScrollPosition
		{
			get;
			init;
		}

		public required Brush RowBgBrush
		{
			get;
			init;
		}

		public required List<SongPatternViewData> SongData
		{
			get;
			init;
		}

		public required bool RollingPatterns
		{
			get;
			init;
		}

		public required bool StripedChannels
		{
			get;
			init;
		}

		public required Brush ChannelStripeBgBrush
		{
			get;
			init;
		}

		public required int ChannelSeparatorWidth
		{
			get;
			init;
		}

		public required Pen GridPen
		{
			get;
			init;
		}

		public required Brush TextBrush
		{
			get;
			init;
		}

		public required int FirstVisibleChannel
		{
			get;
			init;
		}

		public required int ChannelCount
		{
			get;
			init;
		}

		public required int MaxRowCount
		{
			get;
			init;
		}

		public required bool HideEmpty
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

		public required Pen ColumnSeparatorPen
		{
			get;
			init;
		}

		public required Pen PatternBoundaryPen
		{
			get;
			init;
		}

		public required bool DrawSeparatorAfterLastChannel
		{
			get;
			init;
		}

		public required EffectSanitizer EffectSanitizer
		{
			get;
			init;
		}
	}
}
