/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Holds all pattern information
	/// </summary>
	internal class PatternInfo
	{
		public byte[] PositionList;
		public ushort[,] Sequences;
		public ushort[] RowLengths;
	}
}
