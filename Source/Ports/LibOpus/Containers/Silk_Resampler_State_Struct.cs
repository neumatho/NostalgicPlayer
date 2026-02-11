/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Silk_Resampler_State_Struct : IClearable, IDeepCloneable<Silk_Resampler_State_Struct>
	{
		public class SFirUnion : IClearable, IDeepCloneable<SFirUnion>
		{
			private readonly opus_int32[] data = new opus_int32[Constants.Silk_Resampler_Max_Fir_Order];

			public Span<opus_int32> i32 => data;
			public Span<opus_int16> i16 => MemoryMarshal.Cast<opus_int32, opus_int16>(data).Slice(0, Constants.Silk_Resampler_Max_Fir_Order);

			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Clear()
			{
				Array.Clear(data);
			}



			/********************************************************************/
			/// <summary>
			/// Clone the current object into a new one
			/// </summary>
			/********************************************************************/
			public SFirUnion MakeDeepClone()
			{
				SFirUnion clone = new SFirUnion();

				Array.Copy(data, clone.data, data.Length);

				return clone;
			}
		}

		/// <summary>
		/// This must be the first element of this struct
		/// </summary>
		public readonly opus_int32[] sIIR = new opus_int32[Constants.Silk_Resampler_Max_Iir_Order];

		public SFirUnion sFIR = new SFirUnion();
		public CPointer<opus_int16> delayBuf = new CPointer<opus_int16>(96);
		public ResamplerType resampler_function;
		public opus_int batchSize;
		public opus_int32 invRatio_Q16;
		public opus_int FIR_Order;
		public opus_int FIR_Fracs;
		public opus_int Fs_in_kHz;
		public opus_int Fs_out_kHz;
		public opus_int inputDelay;
		public CPointer<opus_int16> Coefs;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Array.Clear(sIIR);

			sFIR.Clear();

			delayBuf.Clear();

			resampler_function = 0;
			batchSize = 0;
			invRatio_Q16 = 0;
			FIR_Order = 0;
			FIR_Fracs = 0;
			Fs_in_kHz = 0;
			Fs_out_kHz = 0;
			inputDelay = 0;
			Coefs.SetToNull();
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Silk_Resampler_State_Struct MakeDeepClone()
		{
			Silk_Resampler_State_Struct clone = new Silk_Resampler_State_Struct
			{
				sFIR = sFIR.MakeDeepClone(),
				delayBuf = delayBuf.MakeDeepClone(),
				resampler_function = resampler_function,
				batchSize = batchSize,
				invRatio_Q16 = invRatio_Q16,
				FIR_Order = FIR_Order,
				FIR_Fracs = FIR_Fracs,
				Fs_in_kHz = Fs_in_kHz,
				Fs_out_kHz = Fs_out_kHz,
				inputDelay = inputDelay
			};

			Array.Copy(sIIR, clone.sIIR, sIIR.Length);

			if (Coefs.IsNotNull)
				clone.Coefs = Coefs.MakeDeepClone();

			return clone;
		}
	}
}
