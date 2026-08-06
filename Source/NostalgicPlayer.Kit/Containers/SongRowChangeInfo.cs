/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
    /// <summary>
    /// Contains information when playback moves to a new row
    /// </summary>
    public class SongRowChangeInfo
    {
        /// <summary>
        /// The song position (index in the position list)
        /// </summary>
        public required int SongPosition { get; set; }

        /// <summary>
        /// The current row in the pattern
        /// </summary>
        public required int Row { get; set; }

        /// <summary>
        /// Current speed (ticks per row)
        /// </summary>
        public required int Speed { get; set; }

        /// <summary>
        /// Current BPM (tempo), null if not applicable for this player format
        /// </summary>
        public required int? Bpm { get; set; }

        /// <summary>
        /// Current track/block/pattern numbers for each channel (for debug purposes).
        /// Index = channel number, Value = track/block/pattern number currently playing.
        /// Null if not applicable for this player format.
        /// </summary>
        public required uint?[] ChannelTracks { get; set; }

        /// <summary>
        /// Current position for each channel (for formats where channels can be at different positions).
        /// Index = channel number, Value = position in position list.
        /// Null if all channels are at the same position (use SongPosition instead).
        /// </summary>
        public required uint[] ChannelPositions { get; set; }

        /// <summary>
        /// Transpose values for each channel (for debug purposes).
        /// Index = channel number, Value = transpose value.
        /// Null if not applicable for this player format.
        /// </summary>
        public sbyte?[] ChannelTranspose { get; set; }
    }
}