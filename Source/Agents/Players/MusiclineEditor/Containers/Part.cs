/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Contain a single part
	/// </summary>
	internal class Part
	{
		/// <summary>
		/// 
		/// </summary>
		public PartLine[] PartData { get; } = ArrayHelper.InitializeArray<PartLine>(128);
	}
}
