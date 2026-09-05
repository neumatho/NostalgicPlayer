/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings
{
	/// <summary>
	/// Inline storage for the 20-byte string
	/// </summary>
	[InlineArray(20)]
	internal struct String20
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



		/********************************************************************/
		/// <summary>
		/// Return the string value
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			ReadOnlySpan<byte> bytes = this;

			int length = bytes.IndexOf((byte)0);
			if (length < 0)
				length = bytes.Length;

			return Encoding.Latin1.GetString(bytes.Slice(0, length));
		}
	}
}
