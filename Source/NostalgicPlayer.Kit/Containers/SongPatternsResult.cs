/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Contains the pattern data from a player
	/// </summary>
	public class SongPatternsResult
	{
		/********************************************************************/
		/// <summary>
		/// Constructor. A player provides its patterns either calculated (from
		/// static data) or recorded (captured during playback) - either way they
		/// are simply the patterns.
		/// </summary>
		/********************************************************************/
		public SongPatternsResult(SongPatterns calculated, SongPatterns recorded = null)
		{
			Patterns = calculated ?? recorded;
		}

		/********************************************************************/
		/// <summary>
		/// The patterns to show
		/// </summary>
		/********************************************************************/
		public SongPatterns Patterns { get; }
	}
}
