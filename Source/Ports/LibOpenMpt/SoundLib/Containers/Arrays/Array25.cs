/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays
{
	/// <summary>
	/// Inline storage for the 25-items array
	/// </summary>
	[InlineArray(25)]
	internal struct Array25<T> where T : struct
	{
		private T _element0;

		/********************************************************************/
		/// <summary>
		/// Return the items as a std array
		/// </summary>
		/********************************************************************/
		public array<T> ToArray()
		{
			ReadOnlySpan<T> items = this;

			return new array<T>(items.ToArray());
		}
	}
}
