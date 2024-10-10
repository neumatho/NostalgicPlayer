/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class SideInfoIndices : IDeepCloneable<SideInfoIndices>
	{
		public readonly opus_int8[] GainsIndicies = new opus_int8[Constants.Max_Nb_Subfr];
		public readonly opus_int8[] LTPIndex = new opus_int8[Constants.Max_Nb_Subfr];
		public readonly opus_int8[] NLSFIndices = new opus_int8[Constants.Max_Lpc_Order + 1];
		public opus_int16 lagIndex;
		public opus_int8 contourIndex;
		public SignalType signalType;
		public opus_int8 quantOffsetType;
		public opus_int8 NLSFInterpCoef_Q2;
		public opus_int8 PERIndex;
		public opus_int8 LTP_scaleIndex;
		public opus_int8 Seed;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Array.Clear(GainsIndicies);
			Array.Clear(LTPIndex);
			Array.Clear(NLSFIndices);

			lagIndex = 0;
			contourIndex = 0;
			signalType = 0;
			quantOffsetType = 0;
			NLSFInterpCoef_Q2 = 0;
			PERIndex = 0;
			LTP_scaleIndex = 0;
			Seed = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public SideInfoIndices MakeDeepClone()
		{
			SideInfoIndices clone = new SideInfoIndices
			{
				lagIndex = lagIndex,
				contourIndex = contourIndex,
				signalType = signalType,
				quantOffsetType = quantOffsetType,
				NLSFInterpCoef_Q2 = NLSFInterpCoef_Q2,
				PERIndex = PERIndex,
				LTP_scaleIndex = LTP_scaleIndex,
				Seed = Seed
			};

			Array.Copy(GainsIndicies, clone.GainsIndicies, GainsIndicies.Length);
			Array.Copy(LTPIndex, clone.LTPIndex, LTPIndex.Length);
			Array.Copy(NLSFIndices, clone.NLSFIndices, NLSFIndices.Length);

			return clone;
		}
	}
}
