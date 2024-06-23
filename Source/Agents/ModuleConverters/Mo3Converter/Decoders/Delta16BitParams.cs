/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Decoders
{
	/// <summary>
	/// Using delta prediction decoder for 8-bit samples
	/// </summary>
	internal class Delta16BitParams : DeltaDecoderBase, IDeltaDecoder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte DhInit => 8;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte Shift => 15;



		/********************************************************************/
		/// <summary>
		/// Return number of bytes a single sample uses
		/// </summary>
		/********************************************************************/
		public int SampleSize => 2;



		/********************************************************************/
		/// <summary>
		/// Return min value of a sample
		/// </summary>
		/********************************************************************/
		public int Min => short.MinValue;



		/********************************************************************/
		/// <summary>
		/// Return max value of a sample
		/// </summary>
		/********************************************************************/
		public int Max => short.MaxValue;



		/********************************************************************/
		/// <summary>
		/// Make sure the value is cast correctly to a signed value
		/// </summary>
		/********************************************************************/
		public short CastToSigned(ushort val)
		{
			return (short)val;
		}



		/********************************************************************/
		/// <summary>
		/// Writes a sample to the destination array
		/// </summary>
		/********************************************************************/
		public void WriteValue(ushort val, byte[] dst, int offset)
		{
			dst[offset] = (byte)(val & 0xff);
			dst[offset + 1] = (byte)((val >> 8) & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Decode(byte[] compressedSample, ref int compressedOffset, out sbyte carry, ref ushort data, ref byte dh, ref ushort val)
		{
			if (dh < 5)
			{
				do
				{
					if (ReadCtrlBit(compressedSample, ref compressedOffset, ref data, out carry))
						break;

					val = (ushort)((val << 1) + carry);

					if (ReadCtrlBit(compressedSample, ref compressedOffset, ref data, out carry))
						break;

					val = (ushort)((val << 1) + carry);

					if (ReadCtrlBit(compressedSample, ref compressedOffset, ref data, out carry))
						break;
				}
				while (carry != 0);
			}
			else
			{
				do
				{
					if (ReadCtrlBit(compressedSample, ref compressedOffset, ref data, out carry))
						break;

					val = (ushort)((val << 1) + carry);

					if (ReadCtrlBit(compressedSample, ref compressedOffset, ref data, out carry))
						break;
				}
				while (carry != 0);
			}
		}
	}
}
