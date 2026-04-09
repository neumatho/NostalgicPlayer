/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Containers
{
	/// <summary>
	/// Hold information about a single sample
	/// </summary>
	public class Sample
	{
		/// <summary></summary>
		public CPointer<ubyte> Start;
		/// <summary></summary>
		public udword Length;
		/// <summary></summary>
		public udword LoopStartOffset;
		/// <summary></summary>
		public udword LoopLength;
	}
}
