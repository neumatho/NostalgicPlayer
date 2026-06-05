/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// A single arpeggio line
	/// </summary>
	internal class ArpeggioLine
	{
		/// <summary>
		/// 61 = Jump
		/// 62 = Restart
		///
		/// Else add 61 to this value to get the real transpose value
		/// </summary>
		public sbyte NoteTranspose { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte SampleNumber { get; set;}

		/// <summary>
		/// 
		/// </summary>
		public ArpeggioEffectEntry[] Effects { get; } = ArrayHelper.InitializeArray<ArpeggioEffectEntry>(2);
	}
}
