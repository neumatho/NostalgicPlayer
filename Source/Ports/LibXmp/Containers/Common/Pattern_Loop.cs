/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Pattern_Loop : IDeepCloneable<Pattern_Loop>
	{
		public c_int Start;
		public c_int Count;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Pattern_Loop MakeDeepClone()
		{
			return (Pattern_Loop)MemberwiseClone();
		}
	}
}
