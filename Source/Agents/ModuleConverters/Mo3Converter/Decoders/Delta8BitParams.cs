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
	internal class Delta8BitParams : DeltaDecoderBase, IDeltaDecoder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte DhInit => 4;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte Shift => 7;



		/********************************************************************/
		/// <summary>
		/// Return number of bytes a single sample uses
		/// </summary>
		/********************************************************************/
		public int SampleSize => 1;



		/********************************************************************/
		/// <summary>
		/// Return min value of a sample
		/// </summary>
		/********************************************************************/
		public int Min => sbyte.MinValue;



		/********************************************************************/
		/// <summary>
		/// Return max value of a sample
		/// </summary>
		/********************************************************************/
		public int Max => sbyte.MaxValue;



		/********************************************************************/
		/// <summary>
		/// Make sure the value is cast correctly to a signed value
		/// </summary>
		/********************************************************************/
		public short CastToSigned(ushort val)
		{
			return (sbyte)val;
		}



		/********************************************************************/
		/// <summary>
		/// Writes a sample to the destination array
		/// </summary>
		/********************************************************************/
		public void WriteValue(ushort val, byte[] dst, int offset)
		{
			dst[offset] = (byte)(val & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Decode(byte[] compressedSample, ref int compressedOffset, out sbyte carry, ref ushort data, ref byte dh, ref ushort val)
		{
			do
			{
				if (ReadCtrlBit(compressedSample, ref compressedOffset, ref data, out carry))
					break;

				val = (byte)((val << 1) + carry);

				if (ReadCtrlBit(compressedSample, ref compressedOffset, ref data, out carry))
					break;
			}
			while (carry != 0);
		}
	}
}
