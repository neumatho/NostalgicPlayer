﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.RiffWave
{
	/// <summary>
	/// Base class for all the formats
	/// </summary>
	internal abstract class RiffWaveWorkerBase
	{
		/********************************************************************/
		/// <summary>
		/// Return the wave format ID
		/// </summary>
		/********************************************************************/
		protected abstract WaveFormat FormatId { get; }
	}
}
