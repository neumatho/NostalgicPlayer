/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers
{
	/// <summary>
	/// Holds different information about an instrument
	/// </summary>
	public class InstrumentInformation
	{
		/// <summary>
		/// 
		/// </summary>
		public string Name;

		/// <summary>
		/// Number of different samples used by the instrument
		/// </summary>
		public uint NumberOfSamples;

		/// <summary>
		/// Tells which sample each note will play. The values are the
		/// sample numbers used by the module itself, where zero means
		/// that the note will not play any sample
		/// </summary>
		public ushort[] Map;
	}
}
