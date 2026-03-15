/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// This is used as the result from the Interrupt() method
	/// </summary>
	public class InterruptResult
	{
		/// <summary>
		/// Holds the new sample address to use
		/// </summary>
		public Array NewSampleAddress { get; set; }

		/// <summary>
		/// Start offset into the sample
		/// </summary>
		public uint StartOffset { get; set; }

		/// <summary>
		/// The length in bytes of the sample
		/// </summary>
		public uint Length { get; set; }
	}
}
