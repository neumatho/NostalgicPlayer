/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	internal static class Constants
	{
		/// <summary>
		/// Max number of tracks OctaMED supports
		/// </summary>
		public const int MaxTracks = 64;

		/// <summary>
		/// Max number of instruments
		/// </summary>
		public const uint MaxInstr = 64;

		/// <summary>
		/// Number of octaves supported
		/// </summary>
		public const int Octaves = 6;

		// Special note numbers
		public const byte NoteStp = 0x80;
		public const byte NoteDef = 0x81;
		public const byte Note11k = 0x82;
		public const byte Note22k = 0x83;
		public const byte Note44k = 0x84;
		public const byte NoteNum = 0x84;
	}
}
