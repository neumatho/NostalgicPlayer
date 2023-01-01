/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter
{
	/// <summary>
	/// Base class for all "ProWizard for PC" formats by Sylvain "Asle" Chipaux
	/// </summary>
	internal abstract class ProWizardForPcBase : ProWizardConverterWorker31SamplesBase
	{
		/********************************************************************/
		/// <summary>
		/// The sample information
		/// </summary>
		/********************************************************************/
		protected bool TestSample(ushort sampleSize, ushort loopStart, ushort loopSize, byte volume, byte fineTune)
		{
			if (loopStart > sampleSize)
				return false;

			if (loopSize > (sampleSize + 2))
				return false;

			if ((loopStart + loopSize) > (sampleSize + 2))
				return false;

			if ((loopStart != 0) && (loopSize < 2))
				return false;

			if (((loopStart != 0) || (loopSize > 2)) && (sampleSize == 0))
				return false;

			if ((volume > 0x40) || (fineTune > 0x0f))
				return false;

			return true;
		}
	}
}
