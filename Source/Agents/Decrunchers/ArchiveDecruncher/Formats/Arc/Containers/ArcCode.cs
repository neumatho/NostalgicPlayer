/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Arc.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class ArcCode
	{
		public ushort Prev { get; set; }
		public ushort Length { get; set; }
		public byte Value { get; set; }
	}
}
