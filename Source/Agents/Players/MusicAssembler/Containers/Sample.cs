/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MusicAssembler.Containers
{
	/// <summary>
	/// Holds information about a single sample
	/// </summary>
	internal class Sample
	{
		public string Name;
		public sbyte[] SampleData;
		public ushort Length;
		public ushort LoopLength;
	}
}
