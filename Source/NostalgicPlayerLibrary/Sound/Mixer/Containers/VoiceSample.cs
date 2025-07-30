/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers
{
	/// <summary>
	/// Holds addresses for a single sample
	/// </summary>
	internal class VoiceSample
	{
		public Array SampleData { get; set; }
		public uint Start { get; set; }
		public uint Length { get; set; }
	}
}
