/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Formats
{
	/// <summary>
	/// Different helper methods used by the savers
	/// </summary>
	internal static class FormatHelper
	{
		/********************************************************************/
		/// <summary>
		/// Return the middle-C frequency of the sample (IT and S3M)
		/// </summary>
		/********************************************************************/
		public static uint GetC5Speed(Mo3Module module, Sample sample)
		{
			bool frequencyInHertz = (module.Version >= 5) || ((module.Header.Flags & HeaderFlag.LinearSlides) == 0);
			if (frequencyInHertz)
				return sample.FreqFineTune;

			return (uint)Math.Round(8363.0 * Math.Pow(2.0, ((int)sample.FreqFineTune + 1408) / 1536.0), MidpointRounding.AwayFromZero);
		}
	}
}
