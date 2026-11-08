/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal class OplPatch : array<uint8>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OplPatch() : base(12)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OplPatch(uint8[] items) : base(items)
		{
		}
	}
}
