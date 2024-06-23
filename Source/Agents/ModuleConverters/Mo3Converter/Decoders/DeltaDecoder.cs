/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Decoders
{
	/// <summary>
	/// Main implementation for delta decoder
	/// </summary>
	internal class DeltaDecoder : DeltaDecoderBase
	{
		private readonly IDeltaDecoder decoder;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DeltaDecoder(IDeltaDecoder deltaDecoder)
		{
			decoder = deltaDecoder;
		}



		/********************************************************************/
		/// <summary>
		/// Will decode the given sample
		/// </summary>
		/********************************************************************/
		public void Decode(byte[] compressedSample, byte[] dst, uint length, byte numChannels)
		{
			byte dh = decoder.DhInit;
			ushort data = 0;
			short previous = 0;
			int compressedOffset = 0;

			for (byte chn = 0; chn < numChannels; chn++)
			{
				int p = chn * decoder.SampleSize;
				int pEnd = (int)(p + length * decoder.SampleSize * numChannels);

				while (p < pEnd)
				{
					ushort val = 0;

					decoder.Decode(compressedSample, ref compressedOffset, out sbyte carry, ref data, ref dh, ref val);
					byte cl = dh;

					while (cl > 0)
					{
						if (ReadCtrlBit(compressedSample, ref compressedOffset, ref data, out carry))
							break;

						val = (ushort)((val << 1) + carry);
						cl--;
					}

					cl = 1;

					if (val >= 4)
					{
						cl = decoder.Shift;

						while ((((1 << cl) & val) == 0) && (cl > 1))
							cl--;
					}

					dh += cl;
					dh >>= 1;					// Next length in bits of encoded delta second part
					carry = (sbyte)(val & 1);	// Sign of delta 1=+, 0=not
					val >>= 1;

					if (carry == 0)
						val = (ushort)~val;		// Negative delta

					val = (ushort)(val + previous);	// Previous value + delta

					decoder.WriteValue(val, dst, p);
					p += decoder.SampleSize * numChannels;

					previous = decoder.CastToSigned(val);
				}
			}
		}
	}
}
