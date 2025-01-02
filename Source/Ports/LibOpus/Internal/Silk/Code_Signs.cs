/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Code_Signs
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static opus_int16 Silk_Dec_Map(c_int a)
		{
			return (opus_int16)(SigProc_Fix.Silk_LSHIFT(a, 1) - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Decodes signs of excitation
		/// </summary>
		/********************************************************************/
		public static void Silk_Decode_Signs(Ec_Dec psRangeDec, CPointer<opus_int16> pulses, opus_int length, SignalType signalType, opus_int quantOffsetType, CPointer<opus_int> sum_pulses)
		{
			opus_uint8[] icdf = new opus_uint8[2];

			icdf[1] = 0;
			CPointer<opus_int16> q_ptr = pulses;
			opus_int i = Macros.Silk_SMULBB(7, SigProc_Fix.Silk_ADD_LSHIFT(quantOffsetType, (int)signalType, 1));
			CPointer<opus_uint8> icdf_ptr = new CPointer<opus_uint8>(Tables_Pulses_Per_Block.Silk_Sign_iCDF, i);
			length = SigProc_Fix.Silk_RSHIFT(length + Constants.Shell_Codec_Frame_Length / 2, Constants.Log2_Shell_Codec_Frame_Length);

			for (i = 0; i < length; i++)
			{
				opus_int p = sum_pulses[i];

				if (p > 0)
				{
					icdf[0] = icdf_ptr[SigProc_Fix.Silk_Min(p & 0x1f, 6)];

					for (opus_int j = 0; j < Constants.Shell_Codec_Frame_Length; j++)
					{
						if (q_ptr[j] > 0)
						{
							// Attach sign
							// Implementation with shift, subtraction, multiplication
							q_ptr[j] *= Silk_Dec_Map(EntDec.Ec_Dec_Icdf(psRangeDec, icdf, 8));
						}
					}
				}

				q_ptr += Constants.Shell_Codec_Frame_Length;
			}
		}
	}
}
