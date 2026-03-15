/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers
{
	/// <summary>
	/// Holds a waveform
	/// </summary>
	internal class Waveform : IModuleData
	{
		/// <summary>
		/// The offset where the waveform is stored in the module
		/// </summary>
		public int Offset { get; set; }

		/// <summary>
		/// The waveform itself
		/// </summary>
		public sbyte[] Data { get; set; }
	}
}
