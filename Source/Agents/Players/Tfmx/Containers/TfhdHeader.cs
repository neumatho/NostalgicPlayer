/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// TFHD one file format structure
	/// </summary>
	internal class TfhdHeader
	{
		public uint HeaderSize { get; set; }
		public byte Type { get; set; }				// 0 = Unchecked, 1 = 1.5, 2 = Pro, 3 = 7V. Bit 7 = forced
		public byte Version { get; set; }
		public uint MdatSize { get; set; }
		public uint SmplSize { get; set; }
	}
}
