/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Chunks
{
	/// <summary>
	/// Holds the data from the PRHI chunk (pattern row highlights)
	/// </summary>
	internal class PrhiChunk : IChunk
	{
		public byte RowsPerBeat { get; set; }
		public byte RowsPerMeasure { get; set; }
	}
}
