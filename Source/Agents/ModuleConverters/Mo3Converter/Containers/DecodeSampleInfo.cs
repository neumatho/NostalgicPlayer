/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// We need all this information for Ogg-compressed samples with shared headers:
	/// A shared header can be taken from a sample that has not been read yet, so
	/// we first need to read all headers, and then load the Ogg samples afterward
	/// </summary>
	internal class DecodeSampleInfo
	{
		public byte[] Chunk { get; set; }
		public byte[] SampleData { get; set; }
		public byte[] OplData { get; set; }
		public Sample SampleHeader { get; set; }
		public short SharedHeader { get; set; }
	}
}
