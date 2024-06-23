/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Decoders
{
	internal interface IDeltaDecoder
	{
		/// <summary>
		/// 
		/// </summary>
		byte DhInit { get; }

		/// <summary>
		/// 
		/// </summary>
		byte Shift { get; }

		/// <summary>
		/// Return number of bytes a single sample uses
		/// </summary>
		int SampleSize { get; }

		/// <summary>
		/// Return min value of a sample
		/// </summary>
		int Min { get; }

		/// <summary>
		/// Return max value of a sample
		/// </summary>
		int Max { get; }

		/// <summary>
		/// Make sure the value is cast correctly to a signed value
		/// </summary>
		short CastToSigned(ushort val);

		/// <summary>
		/// Writes a sample to the destination array
		/// </summary>
		void WriteValue(ushort val, byte[] dst, int offset);

		/// <summary>
		/// 
		/// </summary>
		void Decode(byte[] compressedSample, ref int compressedOffset, out sbyte carry, ref ushort data, ref byte dh, ref ushort val);
	}
}
