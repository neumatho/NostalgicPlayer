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
	internal class Cmd : IDeepCloneable<Cmd>
	{
		public ubyte Aa;
		public ubyte Bb;
		public ubyte Cd;
		public ubyte Ee;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Cmd MakeDeepClone()
		{
			return (Cmd)MemberwiseClone();
		}
	}
}
