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
	/// Inline storage for the 24-items array
	/// </summary>
	[InlineArray(24)]
	internal struct Array24<T>
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
