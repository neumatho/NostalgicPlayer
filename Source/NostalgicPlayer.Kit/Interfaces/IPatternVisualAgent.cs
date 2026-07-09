/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
    /// <summary>
    /// Visual agents that want to receive pattern-specific events should implement this interface.
    /// You also need to implement the IAgentGuiDisplay interface
    /// </summary>
    public interface IPatternVisualAgent : IVisualAgent
    {
        /// <summary>
        /// Called when a new module is loaded. The SongModule contains pattern data
        /// if the player supports patterns (SongModule.Patterns will be null otherwise).
        /// </summary>
        void SongModuleLoaded(SongModule data);

        /// <summary>
        /// Called when playback moves to a new row in the pattern
        /// </summary>
        void PatternRowChanged(SongRowChangeInfo rowInfo);
    }
}