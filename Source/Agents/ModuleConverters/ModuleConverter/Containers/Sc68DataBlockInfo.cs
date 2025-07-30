/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Containers
{
	/// <summary>
	/// Holds the start position and length of a single SC68 module
	/// </summary>
	internal class Sc68DataBlockInfo
	{
		public long ModuleStartPosition { get; set; }
		public int ModuleLength { get; set; }
		public long DataStartPosition { get; set; }
		public int DataLength { get; set; }
	}
}
