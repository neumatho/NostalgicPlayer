/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Decoders
{
	/// <summary>
	/// Base class with helper methods for delta decoders
	/// </summary>
	internal abstract class DeltaDecoderBase
	{
		/********************************************************************/
		/// <summary>
		/// Shift control bits until it is empty:
		/// a 0 bit means literal : the next data byte is copied
		/// a 1 means compressed data
		/// then the next 2 bits determines what is the LZ ptr
		/// ('00' same as previous, else stored in stream)
		/// </summary>
		/********************************************************************/
		protected bool ReadCtrlBit(byte[] compressedSample, ref int compressedOffset, ref ushort data, out sbyte carry)
		{
			data <<= 1;
			carry = (sbyte)(data > 0xff ? 1 : 0);
			data &= 0xff;

			if (data == 0)
			{
				if (compressedOffset >= compressedSample.Length)
					return true;

				byte nextByte = compressedSample[compressedOffset++];

				data = nextByte;
				data <<= 1;
				data += 1;
				carry = (sbyte)(data > 0xff ? 1 : 0);
				data &= 0xff;
			}

			return false;
		}
	}
}
