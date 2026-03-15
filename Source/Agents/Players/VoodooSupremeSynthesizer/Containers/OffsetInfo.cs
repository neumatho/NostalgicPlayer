/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers
{
	/// <summary>
	/// Hold information of a single offset
	/// </summary>
	internal class OffsetInfo
	{
		/// <summary>
		/// The offset into the file
		/// </summary>
		public int Offset { get; set; }

		/// <summary>
		/// The type of the offset
		/// </summary>
		public OffsetType Type { get; set; }

		/// <summary>
		/// The data itself
		/// </summary>
		public IModuleData Data { get; set; }
	}
}
