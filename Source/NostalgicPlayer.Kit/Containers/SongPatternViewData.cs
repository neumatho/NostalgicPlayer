//---------------------------------------------------------------------------------------
// <copyright file="IPatternDataProvider.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
    /// <summary>
    /// Note values for pattern display
    /// </summary>
    public enum SongPatternViewNote
    {
        /// <summary>
        /// No note
        /// </summary>
        None = 0,

        /// <summary>
        /// C note
        /// </summary>
        C,

        /// <summary>
        /// C# note
        /// </summary>
        Cis,

        /// <summary>
        /// D note
        /// </summary>
        D,

        /// <summary>
        /// D# note
        /// </summary>
        Dis,

        /// <summary>
        /// E note
        /// </summary>
        E,

        /// <summary>
        /// F note
        /// </summary>
        F,

        /// <summary>
        /// F# note
        /// </summary>
        Fis,

        /// <summary>
        /// G note
        /// </summary>
        G,

        /// <summary>
        /// G# note
        /// </summary>
        Gis,

        /// <summary>
        /// A note
        /// </summary>
        A,

        /// <summary>
        /// A# note
        /// </summary>
        Ais,

        /// <summary>
        /// B note
        /// </summary>
        B,

        /// <summary>
        /// Note off command
        /// </summary>
        NoteOff
    }

    /// <summary>
    /// Container for pattern data
    /// </summary>
    public class SongPatternViewData
    {
        /// <summary>
        /// Pattern number (null for trackers with separate patterns per channel)
        /// </summary>
        public required int? PatternNumber { get; set; }

        /// <summary>
        /// Number of rows in the pattern
        /// </summary>
        public required int RowCount { get; set; }

        /// <summary>
        /// Number of channels
        /// </summary>
        public required int ChannelCount { get; set; }

        /// <summary>
        /// The pattern rows
        /// </summary>
        public required SongPatternViewRow[] Rows { get; set; }

        /// <summary>
        /// Skip factor for compressed patterns (1 = no compression, 2 = every 2nd row, 4 = every 4th row, etc.)
        /// </summary>
        public int Skip { get; set; } = 1;
    }

    /// <summary>
    /// Container for a single pattern row
    /// </summary>
    public class SongPatternViewRow
    {
        /// <summary>
        /// Row number
        /// </summary>
        public required int RowNumber { get; set; }

        /// <summary>
        /// Channel entries for this row
        /// </summary>
        public required SongPatternViewChannel[] Channels { get; set; }
    }

    /// <summary>
    /// Container for a single pattern entry (one channel in one row)
    /// </summary>
    public class SongPatternViewChannel
    {
        /// <summary>
        /// Note value
        /// </summary>
        public required SongPatternViewNote Note { get; set; }

        /// <summary>
        /// Octave (0-9, null = no note)
        /// </summary>
        public required byte? Octave { get; set; }

        /// <summary>
        /// Instrument/sample number (null = no instrument or invalid instrument like 0 in ProTracker)
        /// </summary>
        public required byte? Instrument { get; set; }

        /// <summary>
        /// Volume (0-64, null = no volume command)
        /// </summary>
        public required byte? Volume { get; set; }

        /// <summary>
        /// Effect texts for display (e.g., ["C40", "300"], empty array if no effects)
        /// </summary>
        public required string[] EffectText { get; set; }

        /// <summary>
        /// Track/pattern number for this channel (null if not applicable, for formats like David Whittaker)
        /// </summary>
        public int? TrackNumber { get; set; }

        /// <summary>
        /// Transpose value for this channel (null if not applicable or no transpose)
        /// </summary>
        public sbyte? Transpose { get; set; }

        /// <summary>
        /// Playing position for this channel (null if not applicable, for debug display)
        /// </summary>
        public int? PlayingPosition { get; set; }

        /// <summary>
        /// Debug info for this channel (null if not applicable, for debug display in pattern viewer)
        /// </summary>
        public string DebugInfo { get; set; }
    }

}