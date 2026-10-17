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
	/// Inline storage for the 120-byte array
	/// </summary>
	[InlineArray(120)]
	internal struct Array120
	{
		private byte _element0;

		/********************************************************************/
		/// <summary>
		/// Return the items as a std array
		/// </summary>
		/********************************************************************/
		public array<uint8> ToArray()
		{
			ReadOnlySpan<byte> items = this;

			return new array<uint8>(items.ToArray());
		}
	}
}
