/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String
{
	/// <summary>
	/// 
	/// </summary>
	internal class StringModeBufRefImpl
	{
		private readonly CPointer<uint8> buf;
		private readonly size_t size;
		private readonly ReadWriteMode mode;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StringModeBufRefImpl(CPointer<uint8> buf_, size_t size_, ReadWriteMode mode_)
		{
			buf = buf_;
			size = size_;
			mode = mode_;
		}



		/********************************************************************/
		/// <summary>
		/// Writes the given string into the buffer using the mode given in
		/// the constructor.
		///
		/// C++ uses the assignment operator for this
		/// (StringModeBufRefImpl ＆ operator = (const std::string ＆ str)),
		/// which C# cannot overload, so it is a method here
		/// </summary>
		/********************************************************************/
		public StringModeBufRefImpl Assign(StdString str)
		{
			MptStringBuffer.WriteStringBuffer(mode, buf, size, str.data(), str.size());

			return this;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static implicit operator StdString(StringModeBufRefImpl impl)
		{
			return MptStringBuffer.ReadStringBuffer(impl.mode, impl.buf, impl.size);
		}
	}
}
