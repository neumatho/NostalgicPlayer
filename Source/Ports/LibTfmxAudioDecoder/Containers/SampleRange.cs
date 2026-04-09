/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class SampleRange : IComparable<SampleRange>
	{
		public udword Start { get; set; }
		public udword End { get; set; }
		public udword LoopStart { get; set; }
		public udword LoopLength { get; set; }

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int CompareTo(SampleRange other)
		{
			return Start.CompareTo(other.Start);
		}
	}
}
