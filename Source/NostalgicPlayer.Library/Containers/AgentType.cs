/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Containers
{
	/// <summary>
	/// The different types of agents
	/// </summary>
	public enum AgentType
	{
		/// <summary>
		/// Output agents plays the actual sound
		/// </summary>
		Output,

		/// <summary>
		/// Player agents can parse and play a specific file format
		/// </summary>
		Players,

		/// <summary>
		/// Streamer agents can play from a network stream with a specific
		/// audio format
		/// </summary>
		Streamers,

		/// <summary>
		/// Converters that can read and/or write samples
		/// </summary>
		SampleConverters,

		/// <summary>
		/// Converters that can convert from one module format to another
		/// </summary>
		ModuleConverters,

		/// <summary>
		/// Show what is playing in a window
		/// </summary>
		Visuals,

		/// <summary>
		/// Can decrunch a single file
		/// </summary>
		FileDecrunchers,

		/// <summary>
		/// Can decrunch archive files
		/// </summary>
		ArchiveDecrunchers
	}
}
