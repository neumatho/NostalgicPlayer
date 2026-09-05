/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String
{
	/// <summary>
	///
	/// </summary>
	internal class StringBufRefImpl
	{
		private readonly CPointer<uint8> buf;
		private readonly size_t size;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StringBufRefImpl(CPointer<uint8> buf_, size_t size_)
		{
			buf = buf_;
			size = size_;
		}



		/********************************************************************/
		/// <summary>
		/// Writes the given string into the buffer, cutting it if it does
		/// not fit and filling the rest of the buffer with nulls.
		///
		/// C++ uses the assignment operator for this
		/// (StringBufRefImpl ＆ operator = (const Tstring ＆ str)), which C#
		/// cannot overload, so it is a method here
		/// </summary>
		/********************************************************************/
		public StringBufRefImpl Assign(StdString str)
		{
			size_t len = Math.Min(str.length(), size - 1);

			Algorithm.copy<CPointer<uint8>, CPointer<uint8>, uint8>(str.data(), str.data() + len, buf);
			Algorithm.fill<CPointer<uint8>, uint8>(buf + len, buf + size, 0x00);

			return this;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static implicit operator StdString(StringBufRefImpl impl)
		{
			// Terminate at \0
			CPointer<uint8> end = Algorithm.find<CPointer<uint8>, uint8>(impl.buf, impl.buf + impl.size, 0x00);

			return new StdString(impl.buf, end);
		}
	}
}
