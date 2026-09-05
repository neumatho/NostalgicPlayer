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
	/// Inline storage for the 128-byte array
	/// </summary>
	[InlineArray(128)]
	internal struct Array128
	{
		private byte _element0;

		/********************************************************************/
		/// <summary>
		/// Return the bytes as a std array
		/// </summary>
		/********************************************************************/
		public array<uint8> ToArray()
		{
			ReadOnlySpan<byte> bytes = this;

			return new array<uint8>(bytes.ToArray());
		}
	}
}
