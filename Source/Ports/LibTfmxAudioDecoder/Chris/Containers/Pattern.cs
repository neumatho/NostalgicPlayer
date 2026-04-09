/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Pattern : IDeepCloneable<Pattern>
	{
		public udword Offset;
		public udword OffsetSaved;
		public uword Step;
		public uword StepSaved;
		public ubyte Wait;
		public sbyte Loops;
		public bool EvalNext;
		public bool InfiniteLoop;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Pattern MakeDeepClone()
		{
			return (Pattern)MemberwiseClone();
		}
	}
}
