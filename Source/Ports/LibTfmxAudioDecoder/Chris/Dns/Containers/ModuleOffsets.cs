/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Dns.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class ModuleOffsets
	{
		public udword Header;
		public udword TrackTable;
		public udword SongDefs;
		public udword Patterns;
		public udword SampleHeaders;
		public udword SampleData;
		public udword Silence;

		// The absolute load address to be subtracted from offsets read at runtime
		public udword Base;
	}
}
