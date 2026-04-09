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
			// Got both the MDAT and SMPL file?
			if (input.SmplLoaded)	// If loaded before, reuse it
			{
				input.MdatSize = input.MdatSizeCurrent;
				input.SmplSize = input.SmplSizeCurrent;

				return true;
			}

			// TNE: This has been rewritten to use the sample buffer from input
			// instead of loading files
			if (sample.IsNull || (sampleLength == 0))
				return false;

			CPointer<ubyte> newInputBuf = new CPointer<ubyte>(sampleLength + input.BufLen);
			CMemory.memcpy(newInputBuf, input.Buf, input.BufLen);
			input.Buf = newInputBuf;

			CMemory.memcpy(newInputBuf + input.Len, sample, sampleLength);

			input.SmplSize = sampleLength;
			input.MdatSize = input.Len - offsets.Header;
			offsets.SampleData = input.Len;
			input.BufLen += input.SmplSize;
			input.Len += input.SmplSize;

			// Update smart pointers
			pBuf = new CPointer<ubyte>(input.Buf.AsMemory((int)input.BufLen));

			return true;
		}
	}
}
