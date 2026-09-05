/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Utility inner loops for mixer-related functionality
	/// </summary>
	internal static class MixerLoops
	{
		private const c_int OfsDecayShift = 8;
		private const c_int OfsDecayMask = 0xff;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void InitMixBuffer(CPointer<mixsample_t> pBuffer, uint32 nSamples)//XX 43
		{
			CMemory.memset(pBuffer, 0, nSamples);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void InterleaveFrontRear(CPointer<mixsample_t> pFrontBuf, CPointer<mixsample_t> pRearBuf, uint32 nFrames)//XX 50
		{
			// Copy backwards as we are writing back into FrontBuf
			for (c_int i = (c_int)nFrames - 1; i >= 0; i--)
			{
				pFrontBuf[(i * 4) + 3] = pRearBuf[(i * 2) + 1];
				pFrontBuf[(i * 4) + 2] = pRearBuf[(i * 2) + 0];
				pFrontBuf[(i * 4) + 1] = pFrontBuf[(i * 2) + 1];
				pFrontBuf[(i * 4) + 0] = pFrontBuf[(i * 2) + 0];
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void MonoFromStereo(CPointer<mixsample_t> pMixBuf, uint32 nSamples)//XX 64
		{
			for (uint32 i = 0; i < nSamples; ++i)
				pMixBuf[i] = (pMixBuf[i * 2] + pMixBuf[(i * 2) + 1]) / 2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void StereoFill(CPointer<mixsample_t> pBuffer, uint32 nSamples, ref mixsample_t rofs, ref mixsample_t lofs)//XX 79
		{
			if ((rofs == 0) && (lofs == 0))
			{
				InitMixBuffer(pBuffer, nSamples * 2);
				return;
			}

			for (uint32 i = 0; i < nSamples; i++)
			{
				// Equivalent to int x_r = (rofs + (rofs > 0 ? 255 : -255)) / 256;
				mixsample_t x_r = Arithmetic_Shift.RShift_Signed(rofs + (Arithmetic_Shift.RShift_Signed(-rofs, (sizeof(mixsample_t) * 8) - 1) & OfsDecayMask), OfsDecayShift);
				mixsample_t x_l = Arithmetic_Shift.RShift_Signed(lofs + (Arithmetic_Shift.RShift_Signed(-lofs, (sizeof(mixsample_t) * 8) - 1) & OfsDecayMask), OfsDecayShift);

				rofs -= x_r;
				lofs -= x_l;

				pBuffer[i * 2] = rofs;
				pBuffer[(i * 2) + 1] = lofs;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void EndChannelOfs(ModChannel chn, CPointer<mixsample_t> pBuffer, uint32 nSamples)//XX 109
		{
			mixsample_t rofs = chn.nROfs;
			mixsample_t lofs = chn.nLOfs;

			if ((rofs == 0) && (lofs == 0))
				return;

			for (uint32 i = 0; i < nSamples; i++)
			{
				mixsample_t x_r = Arithmetic_Shift.RShift_Signed(rofs + (Arithmetic_Shift.RShift_Signed(-rofs, (sizeof(mixsample_t) * 8) - 1) & OfsDecayMask), OfsDecayShift);
				mixsample_t x_l = Arithmetic_Shift.RShift_Signed(lofs + (Arithmetic_Shift.RShift_Signed(-lofs, (sizeof(mixsample_t) * 8) - 1) & OfsDecayMask), OfsDecayShift);

				rofs -= x_r;
				lofs -= x_l;

				pBuffer[i * 2] += rofs;
				pBuffer[(i * 2) + 1] += lofs;
			}

			chn.nROfs = rofs;
			chn.nLOfs = lofs;
		}
	}
}
