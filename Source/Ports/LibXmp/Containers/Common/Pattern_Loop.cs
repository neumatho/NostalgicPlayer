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
		public ref c_int Start => ref _Start;
		private c_int _Start;
		public ref c_int Count => ref _Count;
		private c_int _Count;

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
