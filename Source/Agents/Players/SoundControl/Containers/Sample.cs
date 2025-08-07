/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers
{
	/// <summary>
	/// Holds information about a single sample
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }
		public ushort Length { get; set; }
		public ushort LoopStart { get; set; }
		public ushort LoopEnd { get; set; }
		public short NoteTranspose { get; set; }
		public sbyte[] SampleData { get; set; }
	}
}
