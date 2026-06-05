/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Tune data
	/// </summary>
	internal class Tune
	{
		/// <summary>
		/// The title of the tune
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// BPM tempo
		/// </summary>
		public ushort Tempo { get; set; }

		/// <summary>
		/// Speed
		/// </summary>
		public byte Speed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte Groove { get; set; }

		/// <summary>
		/// Master volume for this tune
		/// </summary>
		public ushort Volume { get; set; }

		/// <summary>
		/// 4 or 8 channel player
		/// </summary>
		public bool PlayMode { get; set; }

		/// <summary>
		/// Number of channels
		/// </summary>
		public byte Channels { get; set; }

		/// <summary>
		/// Sequences for each channel
		/// </summary>
		public ushort[][] Sequences { get; set; }
	}
}
