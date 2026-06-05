/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Hold a single arpeggio table
	/// </summary>
	internal class Arpeggio
	{
		/// <summary>
		/// 
		/// </summary>
		public ArpeggioLine[] ArpeggioData { get; } = ArrayHelper.InitializeArray<ArpeggioLine>(128);
	}
}
