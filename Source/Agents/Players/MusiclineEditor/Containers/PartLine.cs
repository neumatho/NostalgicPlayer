/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Contains information about a single part line
	/// </summary>
	internal class PartLine
	{
		/// <summary>
		/// 
		/// </summary>
		public byte Note { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public byte Instrument { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public PartEffectEntry[] Effects { get; } = ArrayHelper.InitializeArray<PartEffectEntry>(5);
	}
}
