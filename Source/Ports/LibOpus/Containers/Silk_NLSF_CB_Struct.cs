/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Structure containing NLSF codebook
	/// </summary>
	internal class Silk_NLSF_CB_Struct : IDeepCloneable<Silk_NLSF_CB_Struct>
	{
		public opus_int16 nVectors;
		public opus_int16 order;
		public opus_int16 quantStepSize_Q16;
		public opus_int16 invQuantStepSize_Q6;
		public CPointer<opus_uint8> CB1_NLSF_Q8;
		public CPointer<opus_int16> CB1_Wght_Q9;
		public CPointer<opus_uint8> CB1_iCDF;
		public CPointer<opus_uint8> pred_Q8;
		public CPointer<opus_uint8> ec_sel;
		public CPointer<opus_uint8> ec_iCDF;
		public CPointer<opus_uint8> ec_Rates_Q5;
		public CPointer<opus_int16> deltaMin_Q15;

		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Silk_NLSF_CB_Struct MakeDeepClone()
		{
			return new Silk_NLSF_CB_Struct
			{
				nVectors = nVectors,
				order = order,
				quantStepSize_Q16 = quantStepSize_Q16,
				invQuantStepSize_Q6 = invQuantStepSize_Q6,
				CB1_NLSF_Q8 = CB1_NLSF_Q8.MakeDeepClone(),
				CB1_Wght_Q9 = CB1_Wght_Q9.MakeDeepClone(),
				CB1_iCDF = CB1_iCDF.MakeDeepClone(),
				pred_Q8 = pred_Q8.MakeDeepClone(),
				ec_sel = ec_sel.MakeDeepClone(),
				ec_iCDF = ec_iCDF.MakeDeepClone(),
				ec_Rates_Q5 = ec_Rates_Q5.MakeDeepClone(),
				deltaMin_Q15 = deltaMin_Q15.MakeDeepClone(),
			};
		}
	}
}
