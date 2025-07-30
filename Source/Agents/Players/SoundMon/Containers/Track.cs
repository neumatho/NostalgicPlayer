/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers
{
	/// <summary>
	/// Track structure
	/// </summary>
	internal class Track
	{
		public sbyte Note { get; set; }
		public byte Instrument { get; set; }
		public Optional Optional { get; set; }
		public byte OptionalData { get; set; }
	}
}
