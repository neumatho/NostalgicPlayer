/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// Holds internal methods
	/// </summary>
	internal interface IPointerInternal : IPointer
	{
		/// <summary>
		/// Return the type of the elements
		/// </summary>
		Type GetElementType();
	}
}
