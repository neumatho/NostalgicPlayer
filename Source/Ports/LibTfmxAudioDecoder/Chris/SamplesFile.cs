/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris
{
	/// <summary>
	/// Support for loading the separate file containing the samples
	/// </summary>
	public partial class TfmxDecoder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool LoadSamplesFile(IPointer sample, udword sampleLength)
		{
			if (sample.IsNull || (sampleLength == 0))
				return false;

			return AppendSamplesFile(sample, sampleLength);
		}
	}
}
