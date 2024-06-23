/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Decoders
{
	/// <summary>
	/// Main implementation for delta prediction decoder
	/// </summary>
	internal class DeltaPredictionDecoder : DeltaDecoderBase
	{
		private readonly IDeltaDecoder decoder;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DeltaPredictionDecoder(IDeltaDecoder deltaDecoder)
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
			int next = 0;
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

					short delta = decoder.CastToSigned(val);
					val = (ushort)(val + (ushort)next);		// Predicted value + delta

					decoder.WriteValue(val, dst, p);
					p += decoder.SampleSize * numChannels;

					short sVal = decoder.CastToSigned(val);
					next = (sVal * (1 << 1)) + (delta >> 1) - previous;		// Corrected next value

					if (next < decoder.Min)
						next = decoder.Min;
					else if (next > decoder.Max)
						next = decoder.Max;

					previous = sVal;
				}
			}
		}
	}
}
