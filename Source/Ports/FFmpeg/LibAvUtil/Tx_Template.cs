/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	internal abstract class Tx_Template<TScaleType, TXSample, TXUSample, TXAccumulate, TXComplex>
		where TScaleType : INumber<TScaleType>
		where TXSample : unmanaged, INumber<TXSample>
		where TXUSample : unmanaged, INumber<TXUSample>
		where TXAccumulate : unmanaged, INumber<TXAccumulate>, IAdditionOperators<TXAccumulate, TXAccumulate, TXAccumulate>, IMultiplyOperators<TXAccumulate, TXAccumulate, TXAccumulate>
		where TXComplex : unmanaged, ITxComplex, ITxComplexType<TXSample>
	{
		// Power of two tables
		private readonly TXSample[] ff_Tx_Tab_8 = new TXSample[(8 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_16 = new TXSample[(16 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_32 = new TXSample[(32 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_64 = new TXSample[(64 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_128 = new TXSample[(128 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_256 = new TXSample[(256 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_512 = new TXSample[(512 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_1024 = new TXSample[(1024 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_2048 = new TXSample[(2048 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_4096 = new TXSample[(4096 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_8192 = new TXSample[(8192 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_16384 = new TXSample[(16384 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_32768 = new TXSample[(32768 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_65536 = new TXSample[(65536 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_131072 = new TXSample[(131072 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_262144 = new TXSample[(262144 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_524288 = new TXSample[(524288 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_1048576 = new TXSample[(1048576 / 4) + 1];
		private readonly TXSample[] ff_Tx_Tab_2097152 = new TXSample[(2097152 / 4) + 1];

		// Other factors' tables
		private readonly TXSample[] ff_Tx_Tab_53 = new TXSample[12];
		private readonly TXSample[] ff_Tx_Tab_7 = new TXSample[6];
		private readonly TXSample[] ff_Tx_Tab_9 = new TXSample[8];

		private readonly pthread_once_t[] sr_Tabs_Init_Once =
		[
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t()
		];

		private readonly CThread.ThreadOnce_Init_Delegate[] sr_Tabs_Init_Funcs;

		private readonly pthread_once_t[] npTwo_Tabs_Init_Once =
		[
			new pthread_once_t(),
			new pthread_once_t(),
			new pthread_once_t()
		];

		private readonly FFTabInitData[] npTwo_Tabs_Init_Data;

		// Split-Radix codelets
		private FFTxCodelet ff_Tx_Fft2_Ns_Def;
		private FFTxCodelet ff_Tx_Fft4_Ns_Def;
		private FFTxCodelet ff_Tx_Fft8_Ns_Def;
		private FFTxCodelet ff_Tx_Fft16_Ns_Def;
		private FFTxCodelet ff_Tx_Fft32_Ns_Def;
		private FFTxCodelet ff_Tx_Fft64_Ns_Def;
		private FFTxCodelet ff_Tx_Fft128_Ns_Def;
		private FFTxCodelet ff_Tx_Fft256_Ns_Def;
		private FFTxCodelet ff_Tx_Fft512_Ns_Def;
		private FFTxCodelet ff_Tx_Fft1024_Ns_Def;
		private FFTxCodelet ff_Tx_Fft2048_Ns_Def;
		private FFTxCodelet ff_Tx_Fft4096_Ns_Def;
		private FFTxCodelet ff_Tx_Fft8192_Ns_Def;
		private FFTxCodelet ff_Tx_Fft16384_Ns_Def;
		private FFTxCodelet ff_Tx_Fft32768_Ns_Def;
		private FFTxCodelet ff_Tx_Fft65536_Ns_Def;
		private FFTxCodelet ff_Tx_Fft131072_Ns_Def;
		private FFTxCodelet ff_Tx_Fft262144_Ns_Def;
		private FFTxCodelet ff_Tx_Fft524288_Ns_Def;
		private FFTxCodelet ff_Tx_Fft1048576_Ns_Def;
		private FFTxCodelet ff_Tx_Fft2097152_Ns_Def;

		// Prime factor codelets
		private FFTxCodelet ff_Tx_Fft3_Ns_Def;
		private FFTxCodelet ff_Tx_Fft5_Ns_Def;
		private FFTxCodelet ff_Tx_Fft7_Ns_Def;
		private FFTxCodelet ff_Tx_Fft9_Ns_Def;
		private FFTxCodelet ff_Tx_Fft15_Ns_Def;

		// We get these for free
		private FFTxCodelet ff_Tx_Fft3_Fwd_Def;
		private FFTxCodelet ff_Tx_Fft5_Fwd_Def;
		private FFTxCodelet ff_Tx_Fft7_Fwd_Def;
		private FFTxCodelet ff_Tx_Fft9_Fwd_Def;

		// Standalone transforms
		private FFTxCodelet ff_Tx_Fft_Def;
		private FFTxCodelet ff_Tx_Fft_Inplace_Def;
		private FFTxCodelet ff_Tx_Fft_Inplace_Small_Def;
		private FFTxCodelet ff_Tx_Fft_Pfa_Def;
		private FFTxCodelet ff_Tx_Fft_Pfa_Ns_Def;
		private FFTxCodelet ff_Tx_Fft_Naive_Def;
		private FFTxCodelet ff_Tx_Fft_Naive_Small_Def;
		private FFTxCodelet ff_Tx_Mdct_Fwd_Def;
		private FFTxCodelet ff_Tx_Mdct_Inv_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_3xM_Fwd_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_5xM_Fwd_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_7xM_Fwd_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_9xM_Fwd_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_15xM_Fwd_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_3xM_Inv_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_5xM_Inv_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_7xM_Inv_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_9xM_Inv_Def;
		private FFTxCodelet ff_Tx_Mdct_Pfa_15xM_Inv_Def;
		private FFTxCodelet ff_Tx_Mdct_Naive_Fwd_Def;
		private FFTxCodelet ff_Tx_Mdct_Naive_Inv_Def;
		private FFTxCodelet ff_Tx_Mdct_Inv_Full_Def;
		private FFTxCodelet ff_Tx_Rdft_R2C_Def;
		private FFTxCodelet ff_Tx_Rdft_R2R_Def;
		private FFTxCodelet ff_Tx_Rdft_R2R_Mod2_Def;
		private FFTxCodelet ff_Tx_Rdft_R2I_Def;
		private FFTxCodelet ff_Tx_Rdft_R2I_Mod2_Def;
		private FFTxCodelet ff_Tx_Rdft_C2R_Def;
		private FFTxCodelet ff_Tx_DctII_Def;
		private FFTxCodelet ff_Tx_DctIII_Def;
		private FFTxCodelet ff_Tx_DctI_Def;
		private FFTxCodelet ff_Tx_DstI_Def;

		private readonly string typeName;

		private delegate void Fft(CPointer<TXComplex> @out, CPointer<TXComplex> @in, ptrdiff_t stride);

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Tx_Template(string typeName)
		{
			this.typeName = typeName;

			sr_Tabs_Init_Funcs =
			[
				() => FF_Tx_Init_Tab(ff_Tx_Tab_8, 8),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_16, 16),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_32, 32),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_64, 64),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_128, 128),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_256, 256),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_512, 512),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_1024, 1024),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_2048, 2048),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_4096, 4096),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_8192, 8192),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_16384, 16384),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_32768, 32768),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_65536, 65536),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_131072, 131072),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_262144, 262144),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_524288, 524288),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_1048576, 1048576),
				() => FF_Tx_Init_Tab(ff_Tx_Tab_2097152, 2097152)
			];

			npTwo_Tabs_Init_Data =
			[
				new FFTabInitData(FF_Tx_Init_Tab_53, 15, 5, 3),
				new FFTabInitData(FF_Tx_Init_Tab_9, 9),
				new FFTabInitData(FF_Tx_Init_Tab_7, 7)
			];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public FFTxCodelet[] Build_Codelet_List()
		{
			Build_Codelet_Definitions();

			return
			[
				// Split-Radix codelets
				ff_Tx_Fft2_Ns_Def,
				ff_Tx_Fft4_Ns_Def,
				ff_Tx_Fft8_Ns_Def,
				ff_Tx_Fft16_Ns_Def,
				ff_Tx_Fft32_Ns_Def,
				ff_Tx_Fft64_Ns_Def,
				ff_Tx_Fft128_Ns_Def,
				ff_Tx_Fft256_Ns_Def,
				ff_Tx_Fft512_Ns_Def,
				ff_Tx_Fft1024_Ns_Def,
				ff_Tx_Fft2048_Ns_Def,
				ff_Tx_Fft4096_Ns_Def,
				ff_Tx_Fft8192_Ns_Def,
				ff_Tx_Fft16384_Ns_Def,
				ff_Tx_Fft32768_Ns_Def,
				ff_Tx_Fft65536_Ns_Def,
				ff_Tx_Fft131072_Ns_Def,
				ff_Tx_Fft262144_Ns_Def,
				ff_Tx_Fft524288_Ns_Def,
				ff_Tx_Fft1048576_Ns_Def,
				ff_Tx_Fft2097152_Ns_Def,

				// Prime factor codelets
				ff_Tx_Fft3_Ns_Def,
				ff_Tx_Fft5_Ns_Def,
				ff_Tx_Fft7_Ns_Def,
				ff_Tx_Fft9_Ns_Def,
				ff_Tx_Fft15_Ns_Def,

				// We get these for free
				ff_Tx_Fft3_Fwd_Def,
				ff_Tx_Fft5_Fwd_Def,
				ff_Tx_Fft7_Fwd_Def,
				ff_Tx_Fft9_Fwd_Def,

				// Standalone transforms
				ff_Tx_Fft_Def,
				ff_Tx_Fft_Inplace_Def,
				ff_Tx_Fft_Inplace_Small_Def,
				ff_Tx_Fft_Pfa_Def,
				ff_Tx_Fft_Pfa_Ns_Def,
				ff_Tx_Fft_Naive_Def,
				ff_Tx_Fft_Naive_Small_Def,
				ff_Tx_Mdct_Fwd_Def,
				ff_Tx_Mdct_Inv_Def,
				ff_Tx_Mdct_Pfa_3xM_Fwd_Def,
				ff_Tx_Mdct_Pfa_5xM_Fwd_Def,
				ff_Tx_Mdct_Pfa_7xM_Fwd_Def,
				ff_Tx_Mdct_Pfa_9xM_Fwd_Def,
				ff_Tx_Mdct_Pfa_15xM_Fwd_Def,
				ff_Tx_Mdct_Pfa_3xM_Inv_Def,
				ff_Tx_Mdct_Pfa_5xM_Inv_Def,
				ff_Tx_Mdct_Pfa_7xM_Inv_Def,
				ff_Tx_Mdct_Pfa_9xM_Inv_Def,
				ff_Tx_Mdct_Pfa_15xM_Inv_Def,
				ff_Tx_Mdct_Naive_Fwd_Def,
				ff_Tx_Mdct_Naive_Inv_Def,
				ff_Tx_Mdct_Inv_Full_Def,
				ff_Tx_Rdft_R2C_Def,
				ff_Tx_Rdft_R2R_Def,
				ff_Tx_Rdft_R2R_Mod2_Def,
				ff_Tx_Rdft_R2I_Def,
				ff_Tx_Rdft_R2I_Mod2_Def,
				ff_Tx_Rdft_C2R_Def,
				ff_Tx_DctII_Def,
				ff_Tx_DctIII_Def,
				ff_Tx_DctI_Def,
				ff_Tx_DstI_Def
			];
		}

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual TXSample Mult(TXSample x, TXSample m)
		{
			return x * m;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void CMul<T>(out T dre, out T dim, TXSample are, TXSample aim, TXSample bre, TXSample bim) where T : INumber<T>
		{
			dre = T.CreateChecked((are * bre) - (aim * bim));
			dim = T.CreateChecked((are * bim) + (aim * bre));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void SMul(out TXSample dre, out TXSample dim, TXSample are, TXSample aim, TXSample bre, TXSample bim)
		{
			dre = (are * bre) - (aim * bim);
			dim = (are * bim) - (aim * bre);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual TXSample Unscale(TXSample x)
		{
			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual TXSample Rescale(TXSample x)
		{
			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual TXSample Fold(TXSample a, TXSample b)
		{
			return a + b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void Bf<T>(out T x, out T y, T a, T b) where T : INumber<T>
		{
			x = a - b;
			y = a + b;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CMul3(out TXComplex c, TXComplex a, TXComplex b)
		{
			CMul(out var re, out TXSample im, a.Re, a.Im, b.Re, b.Im);

			c = new TXComplex
			{
				Re = re,
				Im = im
			};
		}



		/********************************************************************/
		/// <summary>
		/// This function embeds a Ruritanian PFA input map into an existing
		/// lookup table to avoid double permutation. This allows for
		/// compound factors to be synthesized as fast PFA FFTs and embedded
		/// into either other or standalone transforms.
		/// The output CRT map must still be pre-baked into the transform
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Tx_Embed_Input_Pfa_Map(CPointer<c_int> map, c_int tot_Len, c_int d1, c_int d2)
		{
			CPointer<c_int> mTmp = new CPointer<c_int>(d1 * d2);

			for (c_int k = 0; k < tot_Len; k += (d1 * d2))
			{
				CMemory.memcpy(mTmp, map + k, (size_t)(d1 * d2));

				for (c_int m = 0; m < d2; m++)
				{
					for (c_int n = 0; n < d1; n++)
						map[k + (m * d1) + n] = mTmp[((m * d1) + (n * d2)) % (d1 * d2)];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create all definition objects
		/// </summary>
		/********************************************************************/
		private void Build_Codelet_Definitions()
		{
			Build_SplitRadix_Codelet_Definitions();
			Build_PrimeFactor_Codelet_Definitions();
			Build_StandaloneTransforms_Codelet_Definitions();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Build_SplitRadix_Codelet_Definitions()
		{
			ff_Tx_Fft2_Ns_Def = Build_Sr_Codelet_Def(2, FF_Tx_Fft2_Ns);
			ff_Tx_Fft4_Ns_Def = Build_Sr_Codelet_Def(4, FF_Tx_Fft4_Ns);
			ff_Tx_Fft8_Ns_Def = Build_Sr_Codelet_Def(8, FF_Tx_Fft8_Ns);
			ff_Tx_Fft16_Ns_Def = Build_Sr_Codelet_Def(16, FF_Tx_Fft16_Ns);
			ff_Tx_Fft32_Ns_Def = Build_Sr_Codelet_Def(32, FF_Tx_Fft32_Ns);
			ff_Tx_Fft64_Ns_Def = Build_Sr_Codelet_Def(64, FF_Tx_Fft64_Ns);
			ff_Tx_Fft128_Ns_Def = Build_Sr_Codelet_Def(128, FF_Tx_Fft128_Ns);
			ff_Tx_Fft256_Ns_Def = Build_Sr_Codelet_Def(256, FF_Tx_Fft256_Ns);
			ff_Tx_Fft512_Ns_Def = Build_Sr_Codelet_Def(512, FF_Tx_Fft512_Ns);
			ff_Tx_Fft1024_Ns_Def = Build_Sr_Codelet_Def(1024, FF_Tx_Fft1024_Ns);
			ff_Tx_Fft2048_Ns_Def = Build_Sr_Codelet_Def(2048, FF_Tx_Fft2048_Ns);
			ff_Tx_Fft4096_Ns_Def = Build_Sr_Codelet_Def(4096, FF_Tx_Fft4096_Ns);
			ff_Tx_Fft8192_Ns_Def = Build_Sr_Codelet_Def(8192, FF_Tx_Fft8192_Ns);
			ff_Tx_Fft16384_Ns_Def = Build_Sr_Codelet_Def(16384, FF_Tx_Fft16384_Ns);
			ff_Tx_Fft32768_Ns_Def = Build_Sr_Codelet_Def(32768, FF_Tx_Fft32768_Ns);
			ff_Tx_Fft65536_Ns_Def = Build_Sr_Codelet_Def(65536, FF_Tx_Fft65536_Ns);
			ff_Tx_Fft131072_Ns_Def = Build_Sr_Codelet_Def(131072, FF_Tx_Fft131072_Ns);
			ff_Tx_Fft262144_Ns_Def = Build_Sr_Codelet_Def(262144, FF_Tx_Fft262144_Ns);
			ff_Tx_Fft524288_Ns_Def = Build_Sr_Codelet_Def(524288, FF_Tx_Fft524288_Ns);
			ff_Tx_Fft1048576_Ns_Def = Build_Sr_Codelet_Def(1048576, FF_Tx_Fft1048576_Ns);
			ff_Tx_Fft2097152_Ns_Def = Build_Sr_Codelet_Def(2097152, FF_Tx_Fft2097152_Ns);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Build_PrimeFactor_Codelet_Definitions()
		{
			ff_Tx_Fft3_Ns_Def = Build_Factor_S_Codelet_Def(3, FF_Tx_Fft3);
			ff_Tx_Fft3_Fwd_Def = Build_Factor_F_Codelet_Def(3, FF_Tx_Fft3);
			ff_Tx_Fft5_Ns_Def = Build_Factor_S_Codelet_Def(5, FF_Tx_Fft5);
			ff_Tx_Fft5_Fwd_Def = Build_Factor_F_Codelet_Def(5, FF_Tx_Fft5);
			ff_Tx_Fft7_Ns_Def = Build_Factor_S_Codelet_Def(7, FF_Tx_Fft7);
			ff_Tx_Fft7_Fwd_Def = Build_Factor_F_Codelet_Def(7, FF_Tx_Fft7);
			ff_Tx_Fft9_Ns_Def = Build_Factor_S_Codelet_Def(9, FF_Tx_Fft9);
			ff_Tx_Fft9_Fwd_Def = Build_Factor_F_Codelet_Def(9, FF_Tx_Fft9);
			ff_Tx_Fft15_Ns_Def = Build_Factor_S_Codelet_Def(15, FF_Tx_Fft15);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Build_StandaloneTransforms_Codelet_Definitions()
		{
			Build_StandaloneTransforms_Fft_Codelet_Definitions();
			Build_StandaloneTransforms_Mdct_Codelet_Definitions();
			Build_StandaloneTransforms_Rdft_Codelet_Definitions();
			Build_StandaloneTransforms_Dct_Codelet_Definitions();
			Build_StandaloneTransforms_Dst_Codelet_Definitions();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Build_StandaloneTransforms_Fft_Codelet_Definitions()
		{
			ff_Tx_Fft_Def = new FFTxCodelet
			{
				Name = $"fft_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Fft,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place,
				Nb_Factors = 1,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Fft_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_Fft_Def.Factors[0] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Fft_Inplace_Small_Def = new FFTxCodelet
			{
				Name = $"fft_inplace_small_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Fft,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place | AvTxFlags.Inplace,
				Nb_Factors = 1,
				Min_Len = 2,
				Max_Len = 65536,
				Init = FF_Tx_Fft_Inplace_Small_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base - 256
			};
			ff_Tx_Fft_Inplace_Small_Def.Factors[0] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Fft_Inplace_Def = new FFTxCodelet
			{
				Name = $"fft_inplace_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Fft_Inplace,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place | AvTxFlags.Inplace,
				Nb_Factors = 1,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Fft_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base - 512
			};
			ff_Tx_Fft_Inplace_Def.Factors[0] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Fft_Naive_Small_Def = new FFTxCodelet
			{
				Name = $"fft_naive_small_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Fft_Naive_Small,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place,
				Nb_Factors = 1,
				Min_Len = 2,
				Max_Len = 1024,
				Init = FF_Tx_Fft_Init_Naive_Small,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Min / 2
			};
			ff_Tx_Fft_Naive_Small_Def.Factors[0] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Fft_Naive_Def = new FFTxCodelet
			{
				Name = $"fft_naive_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Fft_Naive,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place,
				Nb_Factors = 1,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = null,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Min
			};
			ff_Tx_Fft_Naive_Def.Factors[0] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Fft_Pfa_Def = new FFTxCodelet
			{
				Name = $"fft_pfa_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Fft_Pfa,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place,
				Nb_Factors = 2,
				Min_Len = 2 * 3,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Fft_Pfa_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_Fft_Pfa_Def.Factors[0] = 7;
			ff_Tx_Fft_Pfa_Def.Factors[1] = 5;
			ff_Tx_Fft_Pfa_Def.Factors[2] = 3;
			ff_Tx_Fft_Pfa_Def.Factors[3] = 2;
			ff_Tx_Fft_Pfa_Def.Factors[4] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Fft_Pfa_Ns_Def = new FFTxCodelet
			{
				Name = $"fft_pfa_ns_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Fft_Pfa_Ns,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Preshuffle,
				Nb_Factors = 2,
				Min_Len = 2 * 3,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Fft_Pfa_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_Fft_Pfa_Ns_Def.Factors[0] = 7;
			ff_Tx_Fft_Pfa_Ns_Def.Factors[1] = 5;
			ff_Tx_Fft_Pfa_Ns_Def.Factors[2] = 3;
			ff_Tx_Fft_Pfa_Ns_Def.Factors[3] = 2;
			ff_Tx_Fft_Pfa_Ns_Def.Factors[4] = UtilConstants.Tx_Factor_Any;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Build_StandaloneTransforms_Mdct_Codelet_Definitions()
		{
			ff_Tx_Mdct_Naive_Fwd_Def = new FFTxCodelet
			{
				Name = $"mdct_naive_fwd_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Mdct_Naive_Fwd,
				Type = Tx_Type("Mdct"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Forward_Only,
				Nb_Factors = 2,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Mdct_Naive_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_Mdct_Naive_Fwd_Def.Factors[0] = 2;
			ff_Tx_Mdct_Naive_Fwd_Def.Factors[1] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Mdct_Naive_Inv_Def = new FFTxCodelet
			{
				Name = $"mdct_naive_inv_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Mdct_Naive_Inv,
				Type = Tx_Type("Mdct"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Inverse_Only,
				Nb_Factors = 2,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Mdct_Naive_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_Mdct_Naive_Inv_Def.Factors[0] = 2;
			ff_Tx_Mdct_Naive_Inv_Def.Factors[1] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Mdct_Fwd_Def = new FFTxCodelet
			{
				Name = $"mdct_fwd_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Mdct_Fwd,
				Type = Tx_Type("Mdct"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Forward_Only,
				Nb_Factors = 2,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Mdct_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_Mdct_Fwd_Def.Factors[0] = 2;
			ff_Tx_Mdct_Fwd_Def.Factors[1] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Mdct_Inv_Def = new FFTxCodelet
			{
				Name = $"mdct_inv_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Mdct_Inv,
				Type = Tx_Type("Mdct"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Inverse_Only,
				Nb_Factors = 2,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Mdct_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_Mdct_Inv_Def.Factors[0] = 2;
			ff_Tx_Mdct_Inv_Def.Factors[1] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Mdct_Inv_Full_Def = new FFTxCodelet
			{
				Name = $"mdct_inv_full_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_Mdct_Inv_Full,
				Type = Tx_Type("Mdct"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place | AvTxFlags.Full_Imdct,
				Nb_Factors = 2,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Mdct_Inv_Full_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_Mdct_Inv_Full_Def.Factors[0] = 2;
			ff_Tx_Mdct_Inv_Full_Def.Factors[1] = UtilConstants.Tx_Factor_Any;

			ff_Tx_Mdct_Pfa_3xM_Inv_Def = Build_Comp_Imdct_Codelet_Def(3, FF_Tx_Mdct_Pfa_3xM_Inv);
			ff_Tx_Mdct_Pfa_5xM_Inv_Def = Build_Comp_Imdct_Codelet_Def(5, FF_Tx_Mdct_Pfa_5xM_Inv);
			ff_Tx_Mdct_Pfa_7xM_Inv_Def = Build_Comp_Imdct_Codelet_Def(7, FF_Tx_Mdct_Pfa_7xM_Inv);
			ff_Tx_Mdct_Pfa_9xM_Inv_Def = Build_Comp_Imdct_Codelet_Def(9, FF_Tx_Mdct_Pfa_9xM_Inv);
			ff_Tx_Mdct_Pfa_15xM_Inv_Def = Build_Comp_Imdct_Codelet_Def(15, FF_Tx_Mdct_Pfa_15xM_Inv);

			ff_Tx_Mdct_Pfa_3xM_Fwd_Def = Build_Comp_Mdct_Codelet_Def(3, FF_Tx_Mdct_Pfa_3xM_Fwd);
			ff_Tx_Mdct_Pfa_5xM_Fwd_Def = Build_Comp_Mdct_Codelet_Def(5, FF_Tx_Mdct_Pfa_5xM_Fwd);
			ff_Tx_Mdct_Pfa_7xM_Fwd_Def = Build_Comp_Mdct_Codelet_Def(7, FF_Tx_Mdct_Pfa_7xM_Fwd);
			ff_Tx_Mdct_Pfa_9xM_Fwd_Def = Build_Comp_Mdct_Codelet_Def(9, FF_Tx_Mdct_Pfa_9xM_Fwd);
			ff_Tx_Mdct_Pfa_15xM_Fwd_Def = Build_Comp_Mdct_Codelet_Def(15, FF_Tx_Mdct_Pfa_15xM_Fwd);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Build_StandaloneTransforms_Rdft_Codelet_Definitions()
		{
			ff_Tx_Rdft_R2C_Def = Build_Rdft_Codelet_Def("r2c", false, FF_Tx_Rdft_R2C);
			ff_Tx_Rdft_C2R_Def = Build_Rdft_Codelet_Def("c2r", true, FF_Tx_Rdft_C2R);

			ff_Tx_Rdft_R2R_Def = Build_Rdft_Half_Codelet_Def("r2r", AvTxFlags.Real_To_Real, false, FF_Tx_Rdft_R2R);
			ff_Tx_Rdft_R2R_Mod2_Def = Build_Rdft_Half_Codelet_Def("r2r_mod2", AvTxFlags.Real_To_Real, true, FF_Tx_Rdft_R2R_Mod2);
			ff_Tx_Rdft_R2I_Def = Build_Rdft_Half_Codelet_Def("r2i", AvTxFlags.Real_To_Imaginary, false, FF_Tx_Rdft_R2I);
			ff_Tx_Rdft_R2I_Mod2_Def = Build_Rdft_Half_Codelet_Def("r2i_mod2", AvTxFlags.Real_To_Imaginary, true, FF_Tx_Rdft_R2I_Mod2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Build_StandaloneTransforms_Dct_Codelet_Definitions()
		{
			ff_Tx_DctII_Def = new FFTxCodelet
			{
				Name = $"dctII_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_DctII,
				Type = Tx_Type("Dct"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Forward_Only,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Dct_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_DctII_Def.Factors[0] = 2;
			ff_Tx_DctII_Def.Factors[1] = UtilConstants.Tx_Factor_Any;

			ff_Tx_DctIII_Def = new FFTxCodelet
			{
				Name = $"dctIII_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_DctIII,
				Type = Tx_Type("Dct"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Inverse_Only,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Dct_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_DctIII_Def.Factors[0] = 2;
			ff_Tx_DctIII_Def.Factors[1] = UtilConstants.Tx_Factor_Any;

			ff_Tx_DctI_Def = new FFTxCodelet
			{
				Name = $"dctI_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_DctI,
				Type = Tx_Type("Dct_I"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place,
				Nb_Factors = 2,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_DcstI_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_DctI_Def.Factors[0] = 2;
			ff_Tx_DctI_Def.Factors[1] = UtilConstants.Tx_Factor_Any;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Build_StandaloneTransforms_Dst_Codelet_Definitions()
		{
			ff_Tx_DstI_Def = new FFTxCodelet
			{
				Name = $"dstI_{typeName.ToLower()}_c".ToCharPointer(),
				Function = FF_Tx_DstI,
				Type = Tx_Type("Dst_I"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place,
				Nb_Factors = 2,
				Min_Len = 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_DcstI_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			ff_Tx_DstI_Def.Factors[0] = 2;
			ff_Tx_DstI_Def.Factors[1] = UtilConstants.Tx_Factor_Any;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private FFTxCodelet Build_Factor_S_Codelet_Def(c_int n, UtilFunc.Av_Tx_Fn f)//XX 506
		{
			FFTxCodelet def = new FFTxCodelet
			{
				Name = $"fft{n}_ns_{typeName.ToLower()}_c".ToCharPointer(),
				Function = f,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place | AvTxFlags.Unaligned | AvTxFlags.FF_Preshuffle,
				Nb_Factors = 1,
				Min_Len = n,
				Max_Len = n,
				Init = FF_Tx_Fft_Factor_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};

			def.Factors[0] = n;

			return def;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private FFTxCodelet Build_Factor_F_Codelet_Def(c_int n, UtilFunc.Av_Tx_Fn f)//XX 521
		{
			FFTxCodelet def = new FFTxCodelet
			{
				Name = $"fft{n}_fwd_{typeName.ToLower()}_c".ToCharPointer(),
				Function = f,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place | AvTxFlags.Unaligned | AvTxFlags.FF_Forward_Only,
				Nb_Factors = 1,
				Min_Len = n,
				Max_Len = n,
				Init = FF_Tx_Fft_Factor_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};

			def.Factors[0] = n;

			return def;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private FFTxCodelet Build_Sr_Codelet_Def(c_int n, UtilFunc.Av_Tx_Fn f)//XX 604
		{
			FFTxCodelet def = new FFTxCodelet
			{
				Name = $"fft{n}_ns_{typeName.ToLower()}_c".ToCharPointer(),
				Function = f,
				Type = Tx_Type("Fft"),
				Flags = AvTxFlags.FF_Out_Of_Place | AvTxFlags.Inplace | AvTxFlags.Unaligned | AvTxFlags.FF_Preshuffle,
				Nb_Factors = 1,
				Min_Len = n,
				Max_Len = n,
				Init = FF_Tx_Fft_Sr_Codelet_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};

			def.Factors[0] = 2;

			return def;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private FFTxCodelet Build_Comp_Imdct_Codelet_Def(c_int n, UtilFunc.Av_Tx_Fn f)//XX 1521
		{
			FFTxCodelet def = new FFTxCodelet
			{
				Name = $"mdct_pfa_{n}xM_inv_{typeName.ToLower()}_c".ToCharPointer(),
				Function = f,
				Type = Tx_Type("Mdct"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Inverse_Only,
				Nb_Factors = 2,
				Min_Len = n * 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Mdct_Pfa_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			def.Factors[0] = n;
			def.Factors[1] = UtilConstants.Tx_Factor_Any;

			return def;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private FFTxCodelet Build_Comp_Mdct_Codelet_Def(c_int n, UtilFunc.Av_Tx_Fn f)//XX 1589
		{
			FFTxCodelet def = new FFTxCodelet
			{
				Name = $"mdct_pfa_{n}xM_fwd_{typeName.ToLower()}_c".ToCharPointer(),
				Function = f,
				Type = Tx_Type("Mdct"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Forward_Only,
				Nb_Factors = 2,
				Min_Len = n * 2,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Mdct_Pfa_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			def.Factors[0] = n;
			def.Factors[1] = UtilConstants.Tx_Factor_Any;

			return def;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private FFTxCodelet Build_Rdft_Codelet_Def(string n, bool inv, UtilFunc.Av_Tx_Fn f)//XX 1716
		{
			FFTxCodelet def = new FFTxCodelet
			{
				Name = $"rdft_{n}_{typeName.ToLower()}_c".ToCharPointer(),
				Function = f,
				Type = Tx_Type("Rdft"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.Inplace | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Forward_Only | (inv ? AvTxFlags.FF_Inverse_Only : AvTxFlags.FF_Forward_Only),
				Nb_Factors = 2,
				Min_Len = 4,
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Rdft_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			def.Factors[0] = 4;
			def.Factors[1] = UtilConstants.Tx_Factor_Any;

			return def;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private FFTxCodelet Build_Rdft_Half_Codelet_Def(string n, AvTxFlags mode, bool mod2, UtilFunc.Av_Tx_Fn f)//XX 1820
		{
			FFTxCodelet def = new FFTxCodelet
			{
				Name = $"rdft_{n}_{typeName.ToLower()}_c".ToCharPointer(),
				Function = f,
				Type = Tx_Type("Rdft"),
				Flags = AvTxFlags.Unaligned | AvTxFlags.Inplace | mode | AvTxFlags.FF_Out_Of_Place | AvTxFlags.FF_Forward_Only,
				Nb_Factors = 2,
				Min_Len = 2 + (2 * (!mod2 ? 1 : 0)),
				Max_Len = UtilConstants.Tx_Len_Unlimited,
				Init = FF_Tx_Rdft_Init,
				Cpu_Flags = AvCpuFlag.FF_All,
				Prio = FFTxCodeletPriority.Base
			};
			def.Factors[0] = 2 + (2 * (!mod2 ? 1 : 0));
			def.Factors[1] = UtilConstants.Tx_Factor_Any;

			return def;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private AvTxType Tx_Type(string x)
		{
			return Enum.Parse<AvTxType>($"{typeName}_{x}");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Init_Tab(TXSample[] table, c_int len)//XX 70
		{
			c_double freq = 2 * Math.PI / len;
			CPointer<TXSample> tab = table;

			for (c_int i = 0; i < (len / 4); i++)
				tab[0, 1] = Rescale(TXSample.CreateChecked(CMath.cos(i * freq)));

			tab[0] = TXSample.Zero;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Init_Tab_53()//XX 95
		{
			// 5pt, doubled to eliminate AVX lane shuffles
			ff_Tx_Tab_53[0] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 5)));
			ff_Tx_Tab_53[1] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 5)));
			ff_Tx_Tab_53[2] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 10)));
			ff_Tx_Tab_53[3] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 10)));
			ff_Tx_Tab_53[4] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 5)));
			ff_Tx_Tab_53[5] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 5)));
			ff_Tx_Tab_53[6] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 10)));
			ff_Tx_Tab_53[7] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 10)));

			// 3pt
			ff_Tx_Tab_53[8] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 12)));
			ff_Tx_Tab_53[9] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 12)));
			ff_Tx_Tab_53[10] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 6)));
			ff_Tx_Tab_53[11] = Rescale(TXSample.CreateChecked(CMath.cos(8 * Math.PI / 6)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Init_Tab_7()//XX 114
		{
			ff_Tx_Tab_7[0] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 7)));
			ff_Tx_Tab_7[1] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 7)));
			ff_Tx_Tab_7[2] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 28)));
			ff_Tx_Tab_7[3] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 28)));
			ff_Tx_Tab_7[4] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 14)));
			ff_Tx_Tab_7[5] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 14)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Init_Tab_9()//XX 124
		{
			ff_Tx_Tab_9[0] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 3)));
			ff_Tx_Tab_9[1] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 3)));
			ff_Tx_Tab_9[2] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 9)));
			ff_Tx_Tab_9[3] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 9)));
			ff_Tx_Tab_9[4] = Rescale(TXSample.CreateChecked(CMath.cos(2 * Math.PI / 36)));
			ff_Tx_Tab_9[5] = Rescale(TXSample.CreateChecked(CMath.sin(2 * Math.PI / 36)));
			ff_Tx_Tab_9[6] = ff_Tx_Tab_9[2] + ff_Tx_Tab_9[5];
			ff_Tx_Tab_9[7] = ff_Tx_Tab_9[3] - ff_Tx_Tab_9[4];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Init_Tabs(c_int len)//XX 148
		{
			c_int factor_2 = IntMath.FF_Ctz(len);

			if (factor_2 != 0)
			{
				c_int idx = factor_2 - 3;

				for (c_int i = 0; i <= idx; i++)
					CThread.pthread_once(sr_Tabs_Init_Once[i], sr_Tabs_Init_Funcs[i]);

				len >>= factor_2;
			}

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(npTwo_Tabs_Init_Data); i++)
			{
				c_int f, f_Idx = 0;

				if (len <= 1)
					return;

				while ((f = npTwo_Tabs_Init_Data[i].Factors[f_Idx++]) != 0)
				{
					if ((f % len) != 0)
						continue;

					CThread.pthread_once(npTwo_Tabs_Init_Once[i], npTwo_Tabs_Init_Data[i].Func);

					len /= f;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fft3(CPointer<TXComplex> @out, CPointer<TXComplex> @in, ptrdiff_t stride)//XX 177
		{
			TXComplex[] tmp = new TXComplex[3];
			TXSample[] tab = ff_Tx_Tab_53;
			TXSample o1, o2;

			tmp[0] = @in[0];

			Bf(out o1, out o2, @in[1].Im, @in[2].Im);
			tmp[1].Re = o1;
			tmp[2].Im = o2;

			Bf(out o1, out o2, @in[1].Re, @in[2].Re);
			tmp[1].Im = o1;
			tmp[2].Re = o2;

			DoFft3(@out, @in, tmp, tab, stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void DoFft3(CPointer<TXComplex> @out, CPointer<TXComplex> @in, TXComplex[] tmp, TXSample[] tab, ptrdiff_t stride)
		{
			@out[0 * stride].Re = tmp[0].Re + tmp[2].Re;
			@out[0 * stride].Im = tmp[0].Im + tmp[2].Im;
			tmp[1].Re = tab[8] * tmp[1].Re;
			tmp[1].Im = tab[9] * tmp[1].Im;
			tmp[2].Re = tab[10] * tmp[2].Re;
			tmp[2].Im = tab[10] * tmp[2].Im;
			@out[1 * stride].Re = tmp[0].Re - tmp[2].Re + tmp[1].Re;
			@out[1 * stride].Im = tmp[0].Im - tmp[2].Im - tmp[1].Im;
			@out[2 * stride].Re = tmp[0].Re - tmp[2].Re - tmp[1].Re;
			@out[2 * stride].Im = tmp[0].Im - tmp[2].Im + tmp[1].Im;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fft5_Decl(CPointer<TXComplex> @out, CPointer<TXComplex> @in, c_int d0, c_int d1, c_int d2, c_int d3, c_int d4, ptrdiff_t stride)//XX 216
		{
			TXComplex[] z0 = new TXComplex[4];
			TXComplex[] t = new TXComplex[6];
			TXSample[] tab = ff_Tx_Tab_53;
			TXSample o1, o2;

			TXComplex dc = @in[0];

			Bf(out o1, out o2, @in[1].Re, @in[4].Re);
			t[1].Im = o1;
			t[0].Re = o2;

			Bf(out o1, out o2, @in[1].Im, @in[4].Im);
			t[1].Re = o1;
			t[0].Im = o2;

			Bf(out o1, out o2, @in[2].Re, @in[3].Re);
			t[3].Im = o1;
			t[2].Re = o2;

			Bf(out o1, out o2, @in[2].Im, @in[3].Im);
			t[3].Re = o1;
			t[2].Im = o2;

			@out[d0 * stride].Re = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Re) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(t[0].Re)) + TXAccumulate.CreateChecked(t[2].Re));
			@out[d0 * stride].Im = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Im) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(t[0].Im)) + TXAccumulate.CreateChecked(t[2].Im));

			SMul(out o1, out o2, tab[0], tab[2], t[2].Re, t[0].Re);
			t[4].Re = o1;
			t[0].Re = o2;

			SMul(out o1, out o2, tab[0], tab[2], t[2].Im, t[0].Im);
			t[4].Im = o1;
			t[0].Im = o2;

			CMul(out o1, out o2, tab[4], tab[6], t[3].Re, t[1].Re);
			t[5].Re = o1;
			t[1].Re = o2;

			CMul(out o1, out o2, tab[4], tab[6], t[3].Im, t[1].Im);
			t[5].Im = o1;
			t[1].Im = o2;

			Bf(out o1, out o2, t[0].Re, t[1].Re);
			z0[0].Re = o1;
			z0[3].Re = o2;

			Bf(out o1, out o2, t[0].Im, t[1].Im);
			z0[0].Im = o1;
			z0[3].Im = o2;

			Bf(out o1, out o2, t[4].Re, t[5].Re);
			z0[2].Re = o1;
			z0[1].Re = o2;

			Bf(out o1, out o2, t[4].Im, t[5].Im);
			z0[2].Im = o1;
			z0[1].Im = o2;

			@out[d1 * stride].Re = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Re) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(z0[3].Re)));
			@out[d1 * stride].Im = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Im) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(z0[0].Im)));
			@out[d2 * stride].Re = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Re) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(z0[2].Re)));
			@out[d2 * stride].Im = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Im) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(z0[1].Im)));
			@out[d3 * stride].Re = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Re) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(z0[1].Re)));
			@out[d3 * stride].Im = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Im) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(z0[2].Im)));
			@out[d4 * stride].Re = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Re) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(z0[0].Re)));
			@out[d4 * stride].Im = TXSample.CreateChecked(TXAccumulate.CreateChecked(dc.Im) + TXAccumulate.CreateChecked(TXUSample.CreateChecked(z0[3].Im)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fft5(CPointer<TXComplex> @out, CPointer<TXComplex> @in, ptrdiff_t stride)//XX 251
		{
			Fft5_Decl(@out, @in, 0, 1, 2, 3, 4, stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fft5_M1(CPointer<TXComplex> @out, CPointer<TXComplex> @in, ptrdiff_t stride)//XX 252
		{
			Fft5_Decl(@out, @in, 0, 6, 12, 3, 9, stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fft5_M2(CPointer<TXComplex> @out, CPointer<TXComplex> @in, ptrdiff_t stride)//XX 253
		{
			Fft5_Decl(@out, @in, 10, 1, 7, 13, 4, stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fft5_M3(CPointer<TXComplex> @out, CPointer<TXComplex> @in, ptrdiff_t stride)//XX 254
		{
			Fft5_Decl(@out, @in, 5, 11, 2, 8, 14, stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fft7(CPointer<TXComplex> @out, CPointer<TXComplex> @in, ptrdiff_t stride)//XX 256
		{
			TXComplex[] t = new TXComplex[6];
			TXComplex[] z = new TXComplex[3];
			CPointer<TXComplex> tab = new CPointer<TXSample>(ff_Tx_Tab_7).Cast<TXSample, TXComplex>();
			TXSample o1, o2;

			TXComplex dc = @in[0];

			Bf(out o1, out o2, @in[1].Re, @in[6].Re);
			t[1].Re = o1;
			t[0].Re = o2;

			Bf(out o1, out o2, @in[1].Im, @in[6].Im);
			t[1].Im = o1;
			t[0].Im = o2;

			Bf(out o1, out o2, @in[2].Re, @in[5].Re);
			t[3].Re = o1;
			t[2].Re = o2;

			Bf(out o1, out o2, @in[2].Im, @in[5].Im);
			t[3].Im = o1;
			t[2].Im = o2;

			Bf(out o1, out o2, @in[3].Re, @in[4].Re);
			t[5].Re = o1;
			t[4].Re = o2;

			Bf(out o1, out o2, @in[3].Im, @in[4].Im);
			t[5].Im = o1;
			t[4].Im = o2;

			@out[0 * stride].Re = dc.Re + t[0].Re + t[2].Re + t[4].Re;
			@out[0 * stride].Im = dc.Im + t[0].Im + t[2].Im + t[4].Im;

			DoFft7(@out, @in, tab, t, z);

			Bf(out o1, out o2, z[0].Re, t[4].Re);
			t[1].Re = o1;
			z[0].Re = o2;

			Bf(out o1, out o2, z[1].Re, t[2].Re);
			t[3].Re = o1;
			z[1].Re = o2;

			Bf(out o1, out o2, z[2].Re, t[0].Re);
			t[5].Re = o1;
			z[2].Re = o2;

			Bf(out o1, out o2, z[0].Im, t[0].Im);
			t[1].Im = o1;
			z[0].Im = o2;

			Bf(out o1, out o2, z[1].Im, t[2].Im);
			t[3].Im = o1;
			z[1].Im = o2;

			Bf(out o1, out o2, z[2].Im, t[4].Im);
			t[5].Im = o1;
			z[2].Im = o2;

			@out[1 * stride].Re = dc.Re + z[0].Re;
			@out[1 * stride].Im = dc.Im + t[1].Im;
			@out[2 * stride].Re = dc.Re + t[3].Re;
			@out[2 * stride].Im = dc.Im + z[1].Im;
			@out[3 * stride].Re = dc.Re + z[2].Re;
			@out[3 * stride].Im = dc.Im + t[5].Im;
			@out[4 * stride].Re = dc.Re + t[5].Re;
			@out[4 * stride].Im = dc.Im + z[2].Im;
			@out[5 * stride].Re = dc.Re + z[1].Re;
			@out[5 * stride].Im = dc.Im + t[3].Im;
			@out[6 * stride].Re = dc.Re + t[1].Re;
			@out[6 * stride].Im = dc.Im + z[0].Im;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void DoFft7(CPointer<TXComplex> @out, CPointer<TXComplex> @in, CPointer<TXComplex> tab, TXComplex[] t, TXComplex[] z)
		{
			z[0].Re = (tab[0].Re * t[0].Re) - (tab[2].Re * t[4].Re) - (tab[1].Re * t[2].Re);
			z[1].Re = (tab[0].Re * t[4].Re) - (tab[1].Re * t[0].Re) - (tab[2].Re * t[2].Re);
			z[2].Re = (tab[0].Re * t[2].Re) - (tab[2].Re * t[0].Re) - (tab[1].Re * t[4].Re);
			z[0].Im = (tab[0].Re * t[0].Im) - (tab[1].Re * t[2].Im) - (tab[2].Re * t[4].Im);
			z[1].Im = (tab[0].Re * t[4].Im) - (tab[1].Re * t[0].Im) - (tab[2].Re * t[2].Im);
			z[2].Im = (tab[0].Re * t[2].Im) - (tab[2].Re * t[0].Im) - (tab[1].Re * t[4].Im);

			// It's possible to do t[4].re and t[0].im with 2 multiplies only by
			// multiplying the sum of all with the average of the twiddles

			t[0].Re = (tab[2].Im * t[1].Im) + (tab[1].Im * t[5].Im) - (tab[0].Im * t[3].Im);
			t[2].Re = (tab[0].Im * t[5].Im) + (tab[2].Im * t[3].Im) - (tab[1].Im * t[1].Im);
			t[4].Re = (tab[2].Im * t[5].Im) + (tab[1].Im * t[3].Im) + (tab[0].Im * t[1].Im);
			t[0].Im = (tab[0].Im * t[1].Re) + (tab[1].Im * t[3].Re) + (tab[2].Im * t[5].Re);
			t[2].Im = (tab[2].Im * t[3].Re) + (tab[0].Im * t[5].Re) - (tab[1].Im * t[1].Re);
			t[4].Im = (tab[2].Im * t[1].Re) + (tab[1].Im * t[5].Re) - (tab[0].Im * t[3].Re);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fft9(CPointer<TXComplex> @out, CPointer<TXComplex> @in, ptrdiff_t stride)//XX 344
		{
			TXComplex[] t = new TXComplex[16];
			TXComplex[] w = new TXComplex[4];
			TXComplex[] x = new TXComplex[5];
			TXComplex[] y = new TXComplex[5];
			TXComplex[] z = new TXComplex[2];
			CPointer<TXComplex> tab = new CPointer<TXSample>(ff_Tx_Tab_9).Cast<TXSample, TXComplex>();
			TXSample o1, o2;

			TXComplex dc = @in[0];

			Bf(out o1, out o2, @in[1].Re, @in[8].Re);
			t[1].Re = o1;
			t[0].Re = o2;

			Bf(out o1, out o2, @in[1].Im, @in[8].Im);
			t[1].Im = o1;
			t[0].Im = o2;

			Bf(out o1, out o2, @in[2].Re, @in[7].Re);
			t[3].Re = o1;
			t[2].Re = o2;

			Bf(out o1, out o2, @in[2].Im, @in[7].Im);
			t[3].Im = o1;
			t[2].Im = o2;

			Bf(out o1, out o2, @in[3].Re, @in[6].Re);
			t[5].Re = o1;
			t[4].Re = o2;

			Bf(out o1, out o2, @in[3].Im, @in[6].Im);
			t[5].Im = o1;
			t[4].Im = o2;

			Bf(out o1, out o2, @in[4].Re, @in[5].Re);
			t[7].Re = o1;
			t[6].Re = o2;

			Bf(out o1, out o2, @in[4].Im, @in[5].Im);
			t[7].Im = o1;
			t[6].Im = o2;

			w[0].Re = t[0].Re - t[6].Re;
			w[0].Im = t[0].Im - t[6].Im;
			w[1].Re = t[2].Re - t[6].Re;
			w[1].Im = t[2].Im - t[6].Im;
			w[2].Re = t[1].Re - t[7].Re;
			w[2].Im = t[1].Im - t[7].Im;
			w[3].Re = t[3].Re + t[7].Re;
			w[3].Im = t[3].Im + t[7].Im;

			z[0].Re = dc.Re + t[4].Re;
			z[0].Im = dc.Im + t[4].Im;

			z[1].Re = t[0].Re + t[2].Re + t[6].Re;
			z[1].Im = t[0].Im + t[2].Im + t[6].Im;

			@out[0 * stride].Re = z[0].Re + z[1].Re;
			@out[0 * stride].Im = z[0].Im + z[1].Im;

			DoFft9(@out, @in, dc, tab, t, w, x, y, z);

			x[4].Re = x[1].Re + x[2].Re;
			x[4].Im = x[1].Im + x[2].Im;

			y[4].Re = y[1].Re - y[2].Re;
			y[4].Im = y[1].Im - y[2].Im;
			x[1].Re = z[0].Re + x[1].Re;
			x[1].Im = z[0].Im + x[1].Im;
			y[1].Re = y[0].Re + y[1].Re;
			y[1].Im = y[0].Im + y[1].Im;
			x[2].Re = z[0].Re + x[2].Re;
			x[2].Im = z[0].Im + x[2].Im;
			y[2].Re = y[2].Re - y[0].Re;
			y[2].Im = y[2].Im - y[0].Im;
			x[4].Re = z[0].Re - x[4].Re;
			x[4].Im = z[0].Im - x[4].Im;
			y[4].Re = y[0].Re - y[4].Re;
			y[4].Im = y[0].Im - y[4].Im;

			@out[1 * stride].Re = x[1].Re + y[1].Im;
			@out[1 * stride].Im = x[1].Im - y[1].Re;
			@out[2 * stride].Re = x[2].Re + y[2].Im;
			@out[2 * stride].Im = x[2].Im - y[2].Re;
			@out[3 * stride].Re = x[3].Re + y[3].Im;
			@out[3 * stride].Im = x[3].Im - y[3].Re;
			@out[4 * stride].Re = x[4].Re + y[4].Im;
			@out[4 * stride].Im = x[4].Im - y[4].Re;
			@out[5 * stride].Re = x[4].Re - y[4].Im;
			@out[5 * stride].Im = x[4].Im + y[4].Re;
			@out[6 * stride].Re = x[3].Re - y[3].Im;
			@out[6 * stride].Im = x[3].Im + y[3].Re;
			@out[7 * stride].Re = x[2].Re - y[2].Im;
			@out[7 * stride].Im = x[2].Im + y[2].Re;
			@out[8 * stride].Re = x[1].Re - y[1].Im;
			@out[8 * stride].Im = x[1].Im + y[1].Re;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void DoFft9(CPointer<TXComplex> @out, CPointer<TXComplex> @in, TXComplex dc, CPointer<TXComplex> tab, TXComplex[] t, TXComplex[] w, TXComplex[] x, TXComplex[] y, TXComplex[] z)
		{
			y[3].Re = tab[0].Im * (t[1].Re - t[3].Re + t[7].Re);
			y[3].Im = tab[0].Im * (t[1].Im - t[3].Im + t[7].Im);

			x[3].Re = z[0].Re + (tab[0].Re * z[1].Re);
			x[3].Im = z[0].Im + (tab[0].Re * z[1].Im);
			z[0].Re = dc.Re + (tab[0].Re * t[4].Re);
			z[0].Im = dc.Im + (tab[0].Re * t[4].Im);

			x[1].Re = (tab[1].Re * w[0].Re) + (tab[2].Im * w[1].Re);
			x[1].Im = (tab[1].Re * w[0].Im) + (tab[2].Im * w[1].Im);
			x[2].Re = (tab[2].Re * w[0].Re) - (tab[3].Re * w[1].Re);
			x[2].Im = (tab[2].Re * w[0].Im) - (tab[3].Re * w[1].Im);
			y[1].Re = (tab[1].Im * w[2].Re) + (tab[2].Re * w[3].Re);
			y[1].Im = (tab[1].Im * w[2].Im) + (tab[2].Re * w[3].Im);
			y[2].Re = (tab[2].Re * w[2].Re) - (tab[3].Im * w[3].Re);
			y[2].Im = (tab[2].Re * w[2].Im) - (tab[3].Im * w[3].Im);

			y[0].Re = tab[0].Im * t[5].Re;
			y[0].Im = tab[0].Im * t[5].Im;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Fft15(CPointer<TXComplex> @out, CPointer<TXComplex> @in, ptrdiff_t stride)//XX 469
		{
			CPointer<TXComplex> tmp = new CPointer<TXComplex>(15);

			for (c_int i = 0; i < 5; i++)
				Fft3(tmp + i, @in + (i * 3), 5);

			Fft5_M1(@out, tmp, stride);
			Fft5_M2(@out, tmp + 5, stride);
			Fft5_M3(@out, tmp + 10, stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Fft_Factor_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 482
		{
			c_int ret = 0;

			FF_Tx_Init_Tabs(len);

			if (len == 15)
				ret = Tx.FF_Tx_Gen_Pfa_Input_Map(s, opts, 3, 5);
			else if ((flags & AvTxFlags.FF_Preshuffle) != 0)
				ret = Tx.FF_Tx_Gen_Default_Map(s, opts);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft3(AvTxContext s, IPointer dst, IPointer src, ptrdiff_t stride)//XX 501
		{
			Fft3(dst.ToPointer<TXComplex>(), src.ToPointer<TXComplex>(), stride / Marshal.SizeOf<TXComplex>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft5(AvTxContext s, IPointer dst, IPointer src, ptrdiff_t stride)//XX 501
		{
			Fft5(dst.ToPointer<TXComplex>(), src.ToPointer<TXComplex>(), stride / Marshal.SizeOf<TXComplex>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft7(AvTxContext s, IPointer dst, IPointer src, ptrdiff_t stride)//XX 501
		{
			Fft7(dst.ToPointer<TXComplex>(), src.ToPointer<TXComplex>(), stride / Marshal.SizeOf<TXComplex>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft9(AvTxContext s, IPointer dst, IPointer src, ptrdiff_t stride)//XX 501
		{
			Fft9(dst.ToPointer<TXComplex>(), src.ToPointer<TXComplex>(), stride / Marshal.SizeOf<TXComplex>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft15(AvTxContext s, IPointer dst, IPointer src, ptrdiff_t stride)//XX 501
		{
			Fft15(dst.ToPointer<TXComplex>(), src.ToPointer<TXComplex>(), stride / Marshal.SizeOf<TXComplex>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Butterflies<T>(ref TXComplex a0, ref TXComplex a1, ref TXComplex a2, ref TXComplex a3, ref T t1, ref T t2, out T t3, out T t4, ref T t5,  ref T t6) where T : INumber<T> //XX 544
		{
			T r0 = T.CreateChecked(a0.Re);
			T i0 = T.CreateChecked(a0.Im);
			T r1 = T.CreateChecked(a1.Re);
			T i1 = T.CreateChecked(a1.Im);
			T o1, o2;

			Bf(out t3, out t5, t5, t1);

			Bf(out o1, out o2, r0, t5);
			a2.Re = TXSample.CreateChecked(o1);
			a0.Re = TXSample.CreateChecked(o2);

			Bf(out o1, out o2, i1, t3);
			a3.Im = TXSample.CreateChecked(o1);
			a1.Im = TXSample.CreateChecked(o2);

			Bf(out t4, out t6, t2, t6);

			Bf(out o1, out o2, r1, t4);
			a3.Re = TXSample.CreateChecked(o1);
			a1.Re = TXSample.CreateChecked(o2);

			Bf(out o1, out o2, i0, t6);
			a2.Im = TXSample.CreateChecked(o1);
			a0.Im = TXSample.CreateChecked(o2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Transform<T>(ref TXComplex a0, ref TXComplex a1, ref TXComplex a2, ref TXComplex a3, TXSample wre, TXSample wim, out T t1, out T t2, out T t3, out T t4, out T t5,  out T t6) where T : INumber<T> //XX 558
		{
			CMul(out t1, out t2, a2.Re, a2.Im, wre, -wim);
			CMul(out t5, out t6, a3.Re, a3.Im, wre, wim);

			Butterflies(ref a0, ref a1, ref a2, ref a3, ref t1, ref t2, out t3, out t4, ref t5, ref t6);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FF_Tx_Fft_Sr_Combine(CPointer<TXComplex> z, CPointer<TXSample> cos, c_int len)//XX 566
		{
			c_int o1 = 2 * len;
			c_int o2 = 4 * len;
			c_int o3 = 6 * len;

			CPointer<TXSample> wim = cos + o1 - 7;
			TXSample t1, t2, t3, t4, t5, t6;

			for (c_int i = 0; i < len; i += 4)
			{
				Transform(ref z[0], ref z[o1 + 0], ref z[o2 + 0], ref z[o3 + 0], cos[0], wim[7], out t1, out t2, out t3, out t4, out t5, out t6);
				Transform(ref z[2], ref z[o1 + 2], ref z[o2 + 2], ref z[o3 + 2], cos[2], wim[5], out t1, out t2, out t3, out t4, out t5, out t6);
				Transform(ref z[4], ref z[o1 + 4], ref z[o2 + 4], ref z[o3 + 4], cos[4], wim[3], out t1, out t2, out t3, out t4, out t5, out t6);
				Transform(ref z[6], ref z[o1 + 6], ref z[o2 + 6], ref z[o3 + 6], cos[6], wim[6], out t1, out t2, out t3, out t4, out t5, out t6);

				Transform(ref z[1], ref z[o1 + 1], ref z[o2 + 1], ref z[o3 + 1], cos[1], wim[6], out t1, out t2, out t3, out t4, out t5, out t6);
				Transform(ref z[3], ref z[o1 + 3], ref z[o2 + 3], ref z[o3 + 3], cos[3], wim[4], out t1, out t2, out t3, out t4, out t5, out t6);
				Transform(ref z[5], ref z[o1 + 5], ref z[o2 + 5], ref z[o3 + 5], cos[5], wim[2], out t1, out t2, out t3, out t4, out t5, out t6);
				Transform(ref z[7], ref z[o1 + 7], ref z[o2 + 7], ref z[o3 + 7], cos[7], wim[0], out t1, out t2, out t3, out t4, out t5, out t6);

				z += 2 * 4;
				cos += 2 * 4;
				wim -= 2 * 4;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Fft_Sr_Codelet_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 592
		{
			FF_Tx_Init_Tabs(len);

			return Tx.FF_Tx_Gen_PTwo_RevTab(s, opts);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft32_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_32;

			FF_Tx_Fft16_Ns(s, dst, src, stride);
			FF_Tx_Fft8_Ns(s, dst + (8 * 2), src + (8 * 2), stride);
			FF_Tx_Fft8_Ns(s, dst + (8 * 3), src + (8 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 8 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft64_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_64;

			FF_Tx_Fft32_Ns(s, dst, src, stride);
			FF_Tx_Fft16_Ns(s, dst + (16 * 2), src + (16 * 2), stride);
			FF_Tx_Fft16_Ns(s, dst + (16 * 3), src + (16 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 16 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft128_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_128;

			FF_Tx_Fft64_Ns(s, dst, src, stride);
			FF_Tx_Fft32_Ns(s, dst + (32 * 2), src + (32 * 2), stride);
			FF_Tx_Fft32_Ns(s, dst + (32 * 3), src + (32 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 32 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft256_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_256;

			FF_Tx_Fft128_Ns(s, dst, src, stride);
			FF_Tx_Fft64_Ns(s, dst + (64 * 2), src + (64 * 2), stride);
			FF_Tx_Fft64_Ns(s, dst + (64 * 3), src + (64 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 64 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft512_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_512;

			FF_Tx_Fft256_Ns(s, dst, src, stride);
			FF_Tx_Fft128_Ns(s, dst + (128 * 2), src + (128 * 2), stride);
			FF_Tx_Fft128_Ns(s, dst + (128 * 3), src + (128 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 128 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft1024_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_1024;

			FF_Tx_Fft512_Ns(s, dst, src, stride);
			FF_Tx_Fft256_Ns(s, dst + (256 * 2), src + (256 * 2), stride);
			FF_Tx_Fft256_Ns(s, dst + (256 * 3), src + (256 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 256 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft2048_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_2048;

			FF_Tx_Fft1024_Ns(s, dst, src, stride);
			FF_Tx_Fft512_Ns(s, dst + (512 * 2), src + (512 * 2), stride);
			FF_Tx_Fft512_Ns(s, dst + (512 * 3), src + (512 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 512 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft4096_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_4096;

			FF_Tx_Fft2048_Ns(s, dst, src, stride);
			FF_Tx_Fft1024_Ns(s, dst + (1024 * 2), src + (1024 * 2), stride);
			FF_Tx_Fft1024_Ns(s, dst + (1024 * 3), src + (1024 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 1024 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft8192_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_8192;

			FF_Tx_Fft4096_Ns(s, dst, src, stride);
			FF_Tx_Fft2048_Ns(s, dst + (2048 * 2), src + (2048 * 2), stride);
			FF_Tx_Fft2048_Ns(s, dst + (2048 * 3), src + (2048 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 2048 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft16384_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_16384;

			FF_Tx_Fft8192_Ns(s, dst, src, stride);
			FF_Tx_Fft4096_Ns(s, dst + (4096 * 2), src + (4096 * 2), stride);
			FF_Tx_Fft4096_Ns(s, dst + (4096 * 3), src + (4096 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 4096 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft32768_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_32768;

			FF_Tx_Fft16384_Ns(s, dst, src, stride);
			FF_Tx_Fft8192_Ns(s, dst + (8192 * 2), src + (8192 * 2), stride);
			FF_Tx_Fft8192_Ns(s, dst + (8192 * 3), src + (8192 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 8192 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft65536_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_65536;

			FF_Tx_Fft32768_Ns(s, dst, src, stride);
			FF_Tx_Fft16384_Ns(s, dst + (16384 * 2), src + (16384 * 2), stride);
			FF_Tx_Fft16384_Ns(s, dst + (16384 * 3), src + (16384 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 16384 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft131072_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_131072;

			FF_Tx_Fft65536_Ns(s, dst, src, stride);
			FF_Tx_Fft32768_Ns(s, dst + (32768 * 2), src + (32768 * 2), stride);
			FF_Tx_Fft32768_Ns(s, dst + (32768 * 3), src + (32768 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 32768 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft262144_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_262144;

			FF_Tx_Fft131072_Ns(s, dst, src, stride);
			FF_Tx_Fft65536_Ns(s, dst + (65536 * 2), src + (65536 * 2), stride);
			FF_Tx_Fft65536_Ns(s, dst + (65536 * 3), src + (65536 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 65536 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft524288_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_524288;

			FF_Tx_Fft262144_Ns(s, dst, src, stride);
			FF_Tx_Fft131072_Ns(s, dst + (131072 * 2), src + (131072 * 2), stride);
			FF_Tx_Fft131072_Ns(s, dst + (131072 * 3), src + (131072 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 131072 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft1048576_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_1048576;

			FF_Tx_Fft524288_Ns(s, dst, src, stride);
			FF_Tx_Fft262144_Ns(s, dst + (262144 * 2), src + (262144 * 2), stride);
			FF_Tx_Fft262144_Ns(s, dst + (262144 * 3), src + (262144 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 262144 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft2097152_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 620
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<TXSample> cos = ff_Tx_Tab_2097152;

			FF_Tx_Fft1048576_Ns(s, dst, src, stride);
			FF_Tx_Fft524288_Ns(s, dst + (524288 * 2), src + (524288 * 2), stride);
			FF_Tx_Fft524288_Ns(s, dst + (524288 * 3), src + (524288 * 3), stride);

			FF_Tx_Fft_Sr_Combine(dst, cos, 524288 >> 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft2_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 635
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();
			TXComplex tmp = new TXComplex();
			TXSample o1, o2;

			Bf(out o1, out o2, src[0].Re, src[1].Re);
			tmp.Re = o1;
			dst[0].Re = o2;

			Bf(out o1, out o2, src[0].Im, src[1].Im);
			tmp.Im = o1;
			dst[0].Im = o2;

			dst[1] = tmp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft4_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 647
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			TXSample t1, t2, t3, t4, t5, t6, t7, t8;
			TXSample o1, o2;

			Bf(out t3, out t1, src[0].Re, src[1].Re);
			Bf(out t8, out t6, src[3].Re, src[2].Re);

			Bf(out o1, out o2, t1, t6);
			dst[2].Re = o1;
			dst[0].Re = o2;

			Bf(out t4, out t2, src[0].Im, src[1].Im);
			Bf(out t7, out t5, src[2].Im, src[3].Im);

			Bf(out o1, out o2, t4, t8);
			dst[3].Im = o1;
			dst[1].Im = o2;

			Bf(out o1, out o2, t3, t7);
			dst[3].Re = o1;
			dst[1].Re = o2;

			Bf(out o1, out o2, t2, t5);
			dst[2].Im = o1;
			dst[0].Im = o2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft8_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 664
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			TXSample t1, t2, t3, t4, t5, t6;
			TXSample o2;

			TXSample cos = ff_Tx_Tab_8[1];

			FF_Tx_Fft4_Ns(s, dst, src, stride);

			Bf(out t1, out o2, src[4].Re, -src[5].Re);
			dst[5].Re = o2;

			Bf(out t2, out o2, src[4].Im, -src[5].Im);
			dst[5].Im = o2;

			Bf(out t5, out o2, src[6].Re, -src[7].Re);
			dst[7].Re = o2;

			Bf(out t6, out o2, src[6].Im, -src[7].Im);
			dst[7].Im = o2;

			Butterflies(ref dst[0], ref dst[2], ref dst[4], ref dst[6], ref t1, ref t2, out t3, out t4, ref t5, ref t6);
			Transform(ref dst[1], ref dst[3], ref dst[5], ref dst[7], cos, cos, out t1, out t2, out t3, out t4, out t5, out t6);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft16_Ns(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 683
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			TXUSample t1, t2, t3, t4, t5, t6;

			CPointer<TXSample> cos = ff_Tx_Tab_16;

			TXSample cos_16_1 = cos[1];
			TXSample cos_16_2 = cos[2];
			TXSample cos_16_3 = cos[3];

			FF_Tx_Fft8_Ns(s, dst, src, stride);
			FF_Tx_Fft4_Ns(s, dst + 8, src + 8, stride);
			FF_Tx_Fft4_Ns(s, dst + 12, src + 12, stride);

			t1 = TXUSample.CreateChecked(dst[8].Re);
			t2 = TXUSample.CreateChecked(dst[8].Im);
			t5 = TXUSample.CreateChecked(dst[12].Re);
			t6 = TXUSample.CreateChecked(dst[12].Im);

			Butterflies(ref dst[0], ref dst[4], ref dst[8], ref dst[12], ref t1, ref t2, out t3, out t4, ref t5, ref t6);

			Transform(ref dst[2], ref dst[6], ref dst[10], ref dst[14], cos_16_2, cos_16_2, out t1, out t2, out t3, out t4, out t5, out t6);
			Transform(ref dst[1], ref dst[5], ref dst[9], ref dst[13], cos_16_1, cos_16_3, out t1, out t2, out t3, out t4, out t5, out t6);
			Transform(ref dst[3], ref dst[7], ref dst[11], ref dst[15], cos_16_3, cos_16_1, out t1, out t2, out t3, out t4, out t5, out t6);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Fft_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 732
		{
			bool is_Inplace = (flags & AvTxFlags.Inplace) != 0;

			FFTxCodeletOptions sub_Opts = new FFTxCodeletOptions
			{
				Map_Dir = is_Inplace ? FFTxMapDirection.Scatter : FFTxMapDirection.Gather
			};

			flags &= ~AvTxFlags.FF_Out_Of_Place;	// We want the subtransform to be
			flags |= AvTxFlags.Inplace;				// in-place
			flags |= AvTxFlags.FF_Preshuffle;		// This function handles the permute step

			TScaleType _scale = (TScaleType)scale;

			c_int ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Fft"), flags, sub_Opts, len, inv, _scale);

			if (ret != 0)
				return ret;

			if (is_Inplace && ((ret = Tx.FF_Tx_Gen_Inplace_Map(s, len)) != 0))
				return ret;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Fft_Inplace_Small_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 758
		{
			s.Tmp = Mem.Av_MAlloc<TXComplex>((size_t)len);

			if (s.Tmp == null)
				return Error.ENOMEM;

			flags &= ~AvTxFlags.Inplace;

			return FF_Tx_Fft_Init(s, cd, flags, opts, len, inv, scale);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 771
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst2 = _dst.ToPointer<TXComplex>();
			CPointer<TXComplex> dst1 = (s.Flags & AvTxFlags.Inplace) != 0 ? s.Tmp.ToPointer<TXComplex>() : dst2;

			CPointer<c_int> map = s.Sub[0].Map;
			c_int len = s.Len;

			for (c_int i = 0; i < len; i++)
				dst1[i] = src[map[i]];

			s.Fn[0](s.Sub[0], dst2, dst1, stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft_Inplace(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 788
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();

			CPointer<c_int> map = s.Sub[0].Map;
			CPointer<c_int> inplace_Idx = s.Map;

			c_int src_Idx = inplace_Idx[0, 1];

			do
			{
				TXComplex tmp = src[src_Idx];
				c_int dst_Idx = map[src_Idx];

				do
				{
					Macros.FFSwap(ref tmp, ref src[dst_Idx]);

					dst_Idx = map[dst_Idx];
				}
				while (dst_Idx != src_Idx);     // Can be > as well, but was less predictable

				src[dst_Idx] = tmp;
			}
			while ((src_Idx = inplace_Idx[0, 1]) != 0);

			s.Fn[0](s.Sub[0], dst, src, stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Fft_Init_Naive_Small(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 854
		{
			c_double phase = s.Inv != 0 ? 2.0 * Math.PI / len : -2.0 * Math.PI / len;

			CPointer<TXComplex> exp = Mem.Av_MAlloc<TXComplex>((size_t)(len * len));
			s.Exp = exp;

			if (s.Exp.IsNull)
				return Error.ENOMEM;

			for (c_int i = 0; i < len; i++)
			{
				for (c_int j = 0; j < len; j++)
				{
					c_double factor = phase * i * j;

					exp[i * j] = new TXComplex
					{
						Re = Rescale(TXSample.CreateChecked(CMath.cos(factor))),
						Im = Rescale(TXSample.CreateChecked(CMath.sin(factor)))
					};
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft_Naive(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 879
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();
			c_int n = s.Len;
			c_double phase = s.Inv != 0 ? 2.0 * Math.PI / n : -2.0 * Math.PI / n;

			stride /= Marshal.SizeOf<TXComplex>();

			for (c_int i = 0; i < n; i++)
			{
				TXComplex tmp = new TXComplex();

				for (c_int j = 0; j < n; j++)
				{
					c_double factor = phase * i * j;

					TXComplex mult = new TXComplex
					{
						Re = Rescale(TXSample.CreateChecked(CMath.cos(factor))),
						Im = Rescale(TXSample.CreateChecked(CMath.sin(factor)))
					};

					CMul3(out TXComplex res, src[j], mult);

					tmp.Re += res.Re;
					tmp.Im += res.Im;
				}

				dst[i * stride] = tmp;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft_Naive_Small(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 906
		{
			CPointer<TXComplex> src = _src.ToPointer<TXComplex>();
			CPointer<TXComplex> dst = _dst.ToPointer<TXComplex>();
			c_int n = s.Len;

			stride /= Marshal.SizeOf<TXComplex>();

			CPointer<TXComplex> exp = s.Exp.ToPointer<TXComplex>();

			for (c_int i = 0; i < n; i++)
			{
				TXComplex tmp = new TXComplex();

				for (c_int j = 0; j < n; j++)
				{
					TXComplex mult = exp[i * j];
					CMul3(out TXComplex res, src[j], mult);

					tmp.Re += res.Re;
					tmp.Im += res.Im;
				}

				dst[i * stride] = tmp;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Fft_Pfa_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 956
		{
			c_int ps = (flags & AvTxFlags.FF_Preshuffle) != 0 ? 1 : 0;
			size_t extra_Tmp_Len = 0;
			CPointer<c_int> len_List = new CPointer<c_int>(UtilConstants.Tx_Max_Decompositions);

			FFTxCodeletOptions sub_Opts = new FFTxCodeletOptions
			{
				Map_Dir = FFTxMapDirection.Gather
			};

			TScaleType _scale = (TScaleType)scale;

			c_int ret = Tx.FF_Tx_Decompose_Length(len_List, Tx_Type("Fft"), len, inv);

			if (ret < 0)
				return ret;

			// Two iterations to test both orderings
			for (c_int i = 0; i < ret; i++)
			{
				c_int len1 = len_List[i];
				c_int len2 = len / len1;

				// Our ptwo transforms don't support striding the output
				if ((len2 != 0) && ((len2 - 1) != 0))
					Macros.FFSwap(ref len1, ref len2);

				Tx.FF_Tx_Clear_Ctx(s);

				// First transform
				sub_Opts.Map_Dir = FFTxMapDirection.Gather;

				flags &= ~AvTxFlags.Inplace;
				flags |= AvTxFlags.FF_Out_Of_Place;
				flags |= AvTxFlags.FF_Preshuffle;		// This function handles the permute step

				ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Fft"), flags, sub_Opts, len, inv, _scale);

				if (ret == Error.ENOMEM)
					return ret;
				else if (ret < 0)	// Try again without a preshuffle flag
				{
					flags &= ~AvTxFlags.FF_Preshuffle;

					ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Fft"), flags, sub_Opts, len, inv, _scale);

					if (ret == Error.ENOMEM)
						return ret;
					else if (ret < 0)
						continue;
				}

				// Second transform
				sub_Opts.Map_Dir = FFTxMapDirection.Scatter;
				flags |= AvTxFlags.FF_Preshuffle;

				Retry:
				flags &= ~AvTxFlags.FF_Out_Of_Place;
				flags |= AvTxFlags.Inplace;

				ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Fft"), flags, sub_Opts, len, inv, _scale);

				if (ret == Error.ENOMEM)
					return ret;
				else if (ret < 0)	// Try again with an out-of-place transform
				{
					flags |= AvTxFlags.FF_Out_Of_Place;
					flags &= ~AvTxFlags.Inplace;

					ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Fft"), flags, sub_Opts, len, inv, _scale);

					if (ret == Error.ENOMEM)
						return ret;
					else if (ret < 0)
					{
						if ((flags & AvTxFlags.FF_Preshuffle) != 0)		// Retry again without a preshuf flag
						{
							flags &= ~AvTxFlags.FF_Preshuffle;

							goto Retry;
						}
						else
							continue;
					}
				}

				// Success
				break;
			}

			// If nothing was successful, error out
			if (ret < 0)
				return ret;

			// Generate PFA map
			ret = Tx.FF_Tx_Gen_Compound_Mapping(s, opts, 0, s.Sub[0].Len, s.Sub[1].Len);

			if (ret != 0)
				return ret;

			s.Tmp = Mem.Av_MAlloc<TXComplex>((size_t)len);

			if (s.Tmp.IsNull)
				return Error.ENOMEM;

			// Flatten input map
			CPointer<c_int> tmp = s.Tmp.Cast<TXComplex, c_int>();

			for (c_int k = 0; k < len; k += s.Sub[0].Len)
			{
				CMemory.memcpy(tmp, s.Map + k, (size_t)s.Sub[0].Len);

				for (c_int i = 0; i < s.Sub[0].Len; i++)
					s.Map[k + i] = tmp[s.Sub[0].Map[i]];
			}

			// Only allocate extra temporary memory if we need it
			if ((s.Sub[1].Flags & AvTxFlags.Inplace) == 0)
				extra_Tmp_Len = (size_t)len;
			else if (ps == 0)
				extra_Tmp_Len = (size_t)s.Sub[0].Len;

			if ((extra_Tmp_Len != 0) && (s.Exp = Mem.Av_MAlloc<TXComplex>(extra_Tmp_Len)).IsNull)
				return Error.ENOMEM;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft_Pfa(AvTxContext s, IPointer _out, IPointer _in, ptrdiff_t stride)//XX 1066
		{
			c_int n = s.Sub[0].Len, m = s.Sub[1].Len, l = s.Len;
			CPointer<c_int> in_Map = s.Map, out_Map = in_Map + l;
			CPointer<c_int> sub_Map = s.Sub[1].Map;
			CPointer<TXComplex> tmp = s.Tmp.ToPointer<TXComplex>();
			CPointer<TXComplex> exp = s.Exp.ToPointer<TXComplex>();
			CPointer<TXComplex> tmp1 = (s.Sub[1].Flags & AvTxFlags.Inplace) != 0 ? tmp : exp;
			CPointer<TXComplex> @in = _in.ToPointer<TXComplex>(), @out = _out.ToPointer<TXComplex>();

			stride /= Marshal.SizeOf<TXComplex>();

			for (c_int i = 0; i < m; i++)
			{
				for (c_int j = 0; j < n; j++)
					exp[j] = @in[in_Map[(i * n) + j]];

				s.Fn[0](s.Sub[0], tmp + sub_Map[i], s.Exp, m * Marshal.SizeOf<TXComplex>());
			}

			for (c_int i = 0; i < n; i++)
				s.Fn[1](s.Sub[1], tmp1 + (m * i), tmp + (m * i), Marshal.SizeOf<TXComplex>());

			for (c_int i = 0; i < l; i++)
				@out[i * stride] = tmp1[out_Map[i]];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Fft_Pfa_Ns(AvTxContext s, IPointer _out, IPointer _in, ptrdiff_t stride)//XX 1090
		{
			c_int n = s.Sub[0].Len, m = s.Sub[1].Len, l = s.Len;
			CPointer<c_int> in_Map = s.Map, out_Map = in_Map + l;
			CPointer<c_int> sub_Map = s.Sub[1].Map;
			CPointer<TXComplex> tmp = s.Tmp.ToPointer<TXComplex>();
			CPointer<TXComplex> exp = s.Exp.ToPointer<TXComplex>();
			CPointer<TXComplex> tmp1 = (s.Sub[1].Flags & AvTxFlags.Inplace) != 0 ? tmp : exp;
			CPointer<TXComplex> @in = _in.ToPointer<TXComplex>(), @out = _out.ToPointer<TXComplex>();

			stride /= Marshal.SizeOf<TXComplex>();

			for (c_int i = 0; i < m; i++)
				s.Fn[0](s.Sub[0], tmp + sub_Map[i], @in + (i * n), m * Marshal.SizeOf<TXComplex>());

			for (c_int i = 0; i < n; i++)
				s.Fn[1](s.Sub[1], tmp1 + (m * i), tmp + (m * i), Marshal.SizeOf<TXComplex>());

			for (c_int i = 0; i < l; i++)
				@out[i * stride] = tmp1[out_Map[i]];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Mdct_Naive_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 1140
		{
			s.Scale_D = c_double.CreateChecked((TScaleType)scale);
			s.Scale_F = (c_float)s.Scale_D;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Naive_Fwd(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1152
		{
			CPointer<TXSample> src = _src.ToPointer<TXSample>();
			CPointer<TXSample> dst = _dst.ToPointer<TXSample>();
			c_double scale = s.Scale_D;
			c_int len = s.Len;
			c_double phase = Math.PI / (4.0 * len);

			stride /= Marshal.SizeOf<TXSample>();

			for (c_int i = 0; i < len; i++)
			{
				c_double sum = 0.0;

				for (c_int j = 0; j < (len * 2); j++)
				{
					c_int a = ((2 * j) + 1 + len) * ((2 * i) + 1);
					sum += c_double.CreateChecked(Unscale(TXSample.CreateChecked(src[j]))) * CMath.cos(a * phase);
				}

				dst[i * stride] = Rescale(TXSample.CreateChecked(sum * scale));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Naive_Inv(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1173
		{
			CPointer<TXSample> src = _src.ToPointer<TXSample>();
			CPointer<TXSample> dst = _dst.ToPointer<TXSample>();
			c_double scale = s.Scale_D;
			c_int len = s.Len >> 1;
			c_int len2 = len * 2;
			c_double phase = Math.PI / (4.0 * len2);

			stride /= Marshal.SizeOf<TXSample>();

			for (c_int i = 0; i < len; i++)
			{
				c_double sum_d = 0.0;
				c_double sum_u = 0.0;
				c_double i_d = phase * ((4 * len) - (2 * i) - 1);
				c_double i_u = phase * ((3 * len2) + (2 * i) + 1);

				for (c_int j = 0; j < len2; j++)
				{
					c_double a = (2 * j) + 1;
					c_double a_d = CMath.cos(a * i_d);
					c_double a_u = CMath.cos(a * i_u);
					c_double val = c_double.CreateChecked(Unscale(TXSample.CreateChecked(src[j * stride])));

					sum_d += a_d * val;
					sum_u += a_u * val;
				}

				dst[i] = Rescale(TXSample.CreateChecked(sum_d * scale));
				dst[i + len] = Rescale(TXSample.CreateChecked(-sum_u * scale));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Mdct_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 1231
		{
			FFTxCodeletOptions sub_Opts = new FFTxCodeletOptions
			{
				Map_Dir = inv == 0 ? FFTxMapDirection.Scatter : FFTxMapDirection.Gather
			};

			TScaleType _scale = (TScaleType)scale;

			s.Scale_D = c_double.CreateChecked(_scale);
			s.Scale_F = (c_float)s.Scale_D;

			flags &= ~AvTxFlags.FF_Out_Of_Place;	// We want the subtransform to be
			flags |= AvTxFlags.Inplace;				// in-place
			flags |= AvTxFlags.FF_Preshuffle;		// First try with an in-place transform

			c_int ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Fft"), flags, sub_Opts, len >> 1, inv, _scale);

			if (ret != 0)
			{
				flags &= ~AvTxFlags.FF_Preshuffle;	// Now try with a generic FFT

				ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Fft"), flags, sub_Opts, len >> 1, inv, _scale);

				if (ret != 0)
					return ret;
			}

			s.Map = Mem.Av_MAlloc<c_int>((size_t)(len >> 1));

			if (s.Map.IsNull)
				return Error.ENOMEM;

			// If we need to preshuffle copy the map from the subcontext
			if ((s.Sub[0].Flags & AvTxFlags.FF_Preshuffle) != 0)
				CMemory.memcpy(s.Map, s.Sub[0].Map, (size_t)(len >> 1));
			else
			{
				for (c_int i = 0; i < (len >> 1); i++)
					s.Map[i] = i;
			}

			ret = FF_Tx_Mdct_Gen_Exp(s, inv != 0 ? s.Map : null);

			if (ret != 0)
				return ret;

			// Saves a multiply in a hot path
			if (inv != 0)
			{
				for (c_int i = 0; i < (s.Len >> 1); i++)
					s.Map[i] <<= 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Fwd(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1281
		{
			CPointer<TXSample> src = _src.ToPointer<TXSample>();
			CPointer<TXSample> dst = _dst.ToPointer<TXSample>();
			CPointer<TXComplex> exp = s.Exp.ToPointer<TXComplex>();
			TXComplex tmp = new TXComplex();
			CPointer<TXComplex> z = _dst.Cast<TXSample, TXComplex>();
			c_int len2 = s.Len >> 1;
			c_int len4 = s.Len >> 2;
			c_int len3 = len2 * 3;
			CPointer<c_int> sub_Map = s.Map;
			TXSample o1, o2;

			stride /= Marshal.SizeOf<TXSample>();

			for (c_int i = 0; i < len2; i++)	// Folding and pre-indexing
			{
				c_int k = 2 * i;
				c_int idx = sub_Map[i];

				if (k < len2)
				{
					tmp.Re = Fold(-src[len2 + k], src[(1 * len2) - 1 - k]);
					tmp.Im = Fold(-src[len3 + k], -src[(1 * len3) - 1 - k]);
				}
				else
				{
					tmp.Re = Fold(-src[len2 + k], -src[(5 * len2) - 1 - k]);
					tmp.Im = Fold(src[-len2 + k], -src[(1 * len3) - 1 - k]);
				}

				CMul(out o1, out o2, tmp.Re, tmp.Im, exp[i].Re, exp[i].Im);
				z[idx].Im = o1;
				z[idx].Re = o2;
			}

			s.Fn[0](s.Sub[0], z, z, Marshal.SizeOf<TXComplex>());

			for (c_int i = 0; i < len4; i++)
			{
				c_int i0 = len4 + i, i1 = len4 - i - 1;
				TXComplex src1 = new TXComplex { Re = z[i1].Re, Im = z[i1].Im };
				TXComplex src0 = new TXComplex { Re = z[i0].Re, Im = z[i0].Im };

				CMul(out o1, out o2, src0.Re, src0.Im, exp[i0].Im, exp[i0].Re);
				dst[(2 * i1 * stride) + stride] = o1;
				dst[2 * i0 * stride] = o2;

				CMul(out o1, out o2, src1.Re, src1.Im, exp[i1].Im, exp[i1].Re);
				dst[(2 * i0 * stride) + stride] = o1;
				dst[2 * i1 * stride] = o2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Inv(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1320
		{
			CPointer<TXComplex> z = _dst.Cast<TXSample, TXComplex>();
			CPointer<TXComplex> exp = s.Exp.ToPointer<TXComplex>();
			CPointer<TXSample> src = _src.ToPointer<TXSample>();
			c_int len2 = s.Len >> 1;
			c_int len4 = s.Len >> 2;
			CPointer<c_int> sub_Map = s.Map;
			TXSample o1, o2;

			stride /= Marshal.SizeOf<TXSample>();

			CPointer<TXSample> in1 = src;
			CPointer<TXSample> in2 = src + (((len2 * 2) - 1) * stride);

			for (c_int i = 0; i < len2; i++)
			{
				c_int k = sub_Map[i];
				TXComplex tmp = new TXComplex { Re = in2[-k * stride], Im = in1[k * stride] };

				CMul3(out z[i], tmp, exp[i]);
			}

			s.Fn[0](s.Sub[0], z, z, Marshal.SizeOf<TXComplex>());

			exp += len2;

			for (c_int i = 0; i < len4; i++)
			{
				c_int i0 = len4 + i, i1 = len4 - i - 1;
				TXComplex src1 = new TXComplex { Re = z[i1].Im, Im = z[i1].Re };
				TXComplex src0 = new TXComplex { Re = z[i0].Im, Im = z[i0].Re };

				CMul(out o1, out o2, src1.Re, src1.Im, exp[i1].Im, exp[i1].Re);
				z[i1].Re = o1;
				z[i0].Im = o2;

				CMul(out o1, out o2, src0.Re, src0.Im, exp[i0].Im, exp[i0].Re);
				z[i0].Re = o1;
				z[i1].Im = o2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Mdct_Inv_Full_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 1380
		{
			TScaleType _scale = (TScaleType)scale;

			s.Scale_D = c_double.CreateChecked(_scale);
			s.Scale_F = (c_float)s.Scale_D;

			flags &= ~AvTxFlags.Full_Imdct;

			c_int ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Mdct"), flags, null, len, 1, _scale);

			if (ret != 0)
				return ret;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Inv_Full(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1400
		{
			c_int len = s.Len << 1;
			c_int len2 = len >> 1;
			c_int len4 = len >> 2;
			CPointer<TXSample> dst = _dst.ToPointer<TXSample>();

			s.Fn[0](s.Sub[0], dst + len4, _src, stride);

			stride /= Marshal.SizeOf<TXSample>();

			for (c_int i = 0; i < len4; i++)
			{
				dst[i * stride] = -dst[(len2 - i - 1) * stride];
				dst[(len - i - 1) * stride] = dst[(len2 + i + 0) * stride];
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Mdct_Pfa_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 1433
		{
			FFTxCodeletOptions sub_Opts = new FFTxCodeletOptions
			{
				Map_Dir = FFTxMapDirection.Scatter
			};

			len >>= 1;
			c_int sub_Len = len / cd.Factors[0];

			TScaleType _scale = (TScaleType)scale;

			s.Scale_D = c_double.CreateChecked(_scale);
			s.Scale_F = (c_float)s.Scale_D;

			flags &= ~AvTxFlags.FF_Out_Of_Place;	// We want the subtransform to be
			flags |= AvTxFlags.Inplace;				// in-place
			flags |= AvTxFlags.FF_Preshuffle;		// This function handles the permute step

			c_int ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Fft"), flags, sub_Opts, sub_Len, inv, _scale);

			if (ret != 0)
				return ret;

			ret = Tx.FF_Tx_Gen_Compound_Mapping(s, opts, s.Inv, cd.Factors[0], sub_Len);

			if (ret != 0)
				return ret;

			// Our 15-point transform is also a compound one, so embed its input map
			if (cd.Factors[0] == 15)
				Tx_Embed_Input_Pfa_Map(s.Map, len, 3, 5);

			ret = FF_Tx_Mdct_Gen_Exp(s, inv != 0 ? s.Map : null);

			if (ret != 0)
				return ret;

			// Saves multiplies in loops
			for (c_int i = 0; i < len; i++)
				s.Map[i] <<= 1;

			s.Tmp = Mem.Av_MAlloc<TXComplex>((size_t)len);

			if (s.Tmp.IsNull)
				return Error.ENOMEM;

			FF_Tx_Init_Tabs(len / sub_Len);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Comp_Imdct_Decl(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride, c_int N, Fft fft)//XX 1480
		{
			CPointer<TXComplex> fft_In = new CPointer<TXComplex>(N);
			CPointer<TXComplex> z = _dst.Cast<TXSample, TXComplex>();
			CPointer<TXComplex> exp = s.Exp.ToPointer<TXComplex>();
			CPointer<TXSample> src = _src.ToPointer<TXSample>();
			c_int len4 = s.Len >> 2;
			c_int len2 = s.Len >> 1;
			c_int m = s.Sub[0].Len;
			CPointer<c_int> in_Map = s.Map;
			CPointer<c_int> out_Map = in_Map + (N * m);
			CPointer<c_int> sub_Map = s.Sub[0].Map;
			TXSample o1, o2;

			stride /= Marshal.SizeOf<TXSample>();

			CPointer<TXSample> in1 = src;
			CPointer<TXSample> in2 = src + (((N * m * 2) - 1) * stride);

			CPointer<TXComplex> sTmp = s.Tmp.ToPointer<TXComplex>();

			for (c_int i = 0; i < len2; i += N)
			{
				for (c_int j = 0; j < N; j++)
				{
					c_int k = in_Map[j];
					TXComplex tmp = new TXComplex { Re = in2[-k * stride], Im = in1[k * stride] };

					CMul3(out fft_In[j], tmp, exp[j]);
				}

				fft(sTmp + sub_Map[0, 1], fft_In, m);

				exp += N;
				in_Map += N;
			}

			for (c_int i = 0; i < N; i++)
				s.Fn[0](s.Sub[0], sTmp + (m * i), sTmp + (m * i), Marshal.SizeOf<TXComplex>());

			for (c_int i = 0; i < len4; i++)
			{
				c_int i0 = len4 + i, i1 = len4 - i - 1;
				c_int s0 = out_Map[i0], s1 = out_Map[i1];

				TXComplex src1 = new TXComplex { Re = sTmp[s1].Im, Im = sTmp[s1].Re };
				TXComplex src0 = new TXComplex { Re = sTmp[s0].Im, Im = sTmp[s0].Re };

				CMul(out o1, out o2, src1.Re, src1.Im, exp[i1].Im, exp[i1].Re);
				z[i1].Re = o1;
				z[i0].Im = o2;

				CMul(out o1, out o2, src0.Re, src0.Im, exp[i0].Im, exp[i0].Re);
				z[i0].Re = o1;
				z[i1].Im = o2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_3xM_Inv(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1480
		{
			Comp_Imdct_Decl(s, _dst, _src, stride, 3, Fft3);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_5xM_Inv(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1480
		{
			Comp_Imdct_Decl(s, _dst, _src, stride, 5, Fft5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_7xM_Inv(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1480
		{
			Comp_Imdct_Decl(s, _dst, _src, stride, 7, Fft7);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_9xM_Inv(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1480
		{
			Comp_Imdct_Decl(s, _dst, _src, stride, 9, Fft9);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_15xM_Inv(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1480
		{
			Comp_Imdct_Decl(s, _dst, _src, stride, 15, Fft15);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Comp_Mdct_Decl(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride, c_int N, Fft fft)//XX 1542
		{
			CPointer<TXComplex> fft_In = new CPointer<TXComplex>(N);
			CPointer<TXSample> src = _src.ToPointer<TXSample>();
			CPointer<TXSample> dst = _dst.ToPointer<TXSample>();
			CPointer<TXComplex> exp = s.Exp.ToPointer<TXComplex>();
			TXComplex tmp = new TXComplex();
			c_int m = s.Sub[0].Len;
			c_int len4 = N * m;
			c_int len3 = len4 * 3;
			c_int len8 = s.Len >> 2;
			CPointer<c_int> in_Map = s.Map;
			CPointer<c_int> out_Map = in_Map + (N * m);
			CPointer<c_int> sub_Map = s.Sub[0].Map;
			TXSample o1, o2;

			stride /= Marshal.SizeOf<TXSample>();

			CPointer<TXComplex> sTmp = s.Tmp.ToPointer<TXComplex>();

			for (c_int i = 0; i < m; i++)		// Folding and pre-reindexing
			{
				for (c_int j = 0; j < N; j++)
				{
					c_int k = in_Map[(i * N) + j];

					if (k < len4)
					{
						tmp.Re = Fold(-src[len4 + k], src[(1 * len4) - 1 - k]);
						tmp.Im = Fold(-src[len3 + k], -src[(1 * len3) - 1 - k]);
					}
					else
					{
						tmp.Re = Fold(-src[len4 + k], -src[(5 * len4) - 1 - k]);
						tmp.Im = Fold(src[-len4 + k], -src[(1 * len3) - 1 - k]);
					}

					CMul(out o1, out o2, tmp.Re, tmp.Im, exp[k >> 1].Re, exp[k >> 1].Im);
					fft_In[j].Im = o1;
					fft_In[j].Re = o2;
				}

				fft(sTmp + sub_Map[i], fft_In, m);
			}

			for (c_int i = 0; i < N; i++)
				s.Fn[0](s.Sub[0], sTmp + (m * i), sTmp + (m * i), Marshal.SizeOf<TXComplex>());

			for (c_int i = 0; i < len8; i++)
			{
				c_int i0 = len8 + i, i1 = len8 - i - 1;
				c_int s0 = out_Map[i0], s1 = out_Map[i1];

				TXComplex src1 = new TXComplex { Re = sTmp[s1].Re, Im = sTmp[s1].Im };
				TXComplex src0 = new TXComplex { Re = sTmp[s0].Re, Im = sTmp[s0].Im };

				CMul(out dst[(2 * i1 * stride) + stride], out dst[2 * i0 * stride], src0.Re, src0.Im, exp[i0].Im, exp[i0].Re);
				CMul(out dst[(2 * i0 * stride) + stride], out dst[2 * i1 * stride], src1.Re, src1.Im, exp[i1].Im, exp[i1].Re);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_3xM_Fwd(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1542
		{
			Comp_Mdct_Decl(s, _dst, _src, stride, 3, Fft3);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_5xM_Fwd(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1542
		{
			Comp_Mdct_Decl(s, _dst, _src, stride, 5, Fft5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_7xM_Fwd(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1542
		{
			Comp_Mdct_Decl(s, _dst, _src, stride, 7, Fft7);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_9xM_Fwd(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1542
		{
			Comp_Mdct_Decl(s, _dst, _src, stride, 9, Fft9);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Mdct_Pfa_15xM_Fwd(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1542
		{
			Comp_Mdct_Decl(s, _dst, _src, stride, 15, Fft15);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Rdft_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 1609
		{
			uint64_t r2r = (flags & AvTxFlags.Real_To_Real) != 0 ? 1U : 0U;
			c_int len4 = Macros.FFAlign(len, 4) / 4;

			TScaleType _scale = (TScaleType)scale;

			s.Scale_D = c_double.CreateChecked(_scale);
			s.Scale_F = (c_float)s.Scale_D;

			flags &= ~(AvTxFlags.Real_To_Real | AvTxFlags.Real_To_Imaginary);

			c_int ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Fft"), flags, null, len >> 1, inv, _scale);

			if (ret != 0)
				return ret;

			s.Exp = Mem.Av_MAlloc<TXComplex>((size_t)(8 + (2 * len4)));

			if (s.Exp.IsNull)
				return Error.ENOMEM;

			CPointer<TXSample> tab = s.Exp.Cast<TXComplex, TXSample>();

			c_double f = 2 * Math.PI / len;

			c_double m = inv != 0 ? 2 * s.Scale_D : s.Scale_D;

			tab[0, 1] = Rescale(TXSample.CreateChecked((inv != 0 ? 0.5 : 1.0) * m));
			tab[0, 1] = Rescale(TXSample.CreateChecked(inv != 0 ? 0.5 * m : 1.0 * m));
			tab[0, 1] = Rescale(TXSample.CreateChecked(m));
			tab[0, 1] = Rescale(TXSample.CreateChecked(-m));

			tab[0, 1] = Rescale(TXSample.CreateChecked((0.5 - 0.0) * m));

			if (r2r != 0)
				tab[0, 1] = TXSample.CreateChecked(1 / s.Scale_F);
			else
				tab[0, 1] = Rescale(TXSample.CreateChecked((0.0 - 0.5) * m));

			tab[0, 1] = Rescale(TXSample.CreateChecked((0.5 - inv) * m));
			tab[0, 1] = Rescale(TXSample.CreateChecked(-(0.5 - inv) * m));

			for (c_int i = 0; i < len4; i++)
				tab[0, 1] = Rescale(TXSample.CreateChecked(CMath.cos(i * f)));

			tab = s.Exp.Cast<TXComplex, TXSample>() + len4 + 8;

			for (c_int i = 0; i < len4; i++)
				tab[0, 1] = TXSample.CreateChecked(TXAccumulate.CreateChecked(Rescale(TXSample.CreateChecked(CMath.cos(((len - (i * 4)) / 4.0) * f)))) * TXAccumulate.CreateChecked(inv != 0 ? 1 : -1));

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Rdft_Decl(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride, c_int inv)//XX 1664
		{
			c_int len2 = s.Len >> 1;
			c_int len4 = s.Len >> 2;
			CPointer<TXSample> fact = s.Exp.Cast<TXComplex, TXSample>();
			CPointer<TXSample> tCos = fact + 8;
			CPointer<TXSample> tSin = tCos + len4;
			CPointer<TXComplex> data = inv != 0 ? _src.ToPointer<TXComplex>() : _dst.ToPointer<TXComplex>();
			CPointer<TXComplex> t = new CPointer<TXComplex>(3);
			TXSample o1, o2;

			if (inv == 0)
				s.Fn[0](s.Sub[0], data, _src, Marshal.SizeOf<TXComplex>());
			else
				data[0].Im = data[len2].Re;

			// The DC value's both components are real, but we need to change them
			// into complex values. Also, the middle of the array is special-cased.
			// These operations can be done before or after the loop
			t[0].Re = data[0].Re;
			data[0].Re = t[0].Re + data[0].Im;
			data[0].Im = t[0].Re - data[0].Im;
			data[0].Re = Mult(fact[0], data[0].Re);
			data[0].Im = Mult(fact[1], data[0].Im);
			data[len4].Re = Mult(fact[2], data[len4].Re);
			data[len4].Im = Mult(fact[3], data[len4].Im);

			for (c_int i = 1; i < len4; i++)
			{
				// Separate even and odd FFTs
				t[0].Re = Mult(fact[4], (data[i].Re + data[len2 - i].Re));
				t[0].Im = Mult(fact[5], (data[i].Im - data[len2 - i].Im));
				t[1].Re = Mult(fact[6], (data[i].Im + data[len2 - i].Im));
				t[1].Im = Mult(fact[7], (data[i].Re - data[len2 - i].Re));

				// Apply twiddle factors to the odd FFT and add to the even FFT
				CMul(out o1, out o2, t[1].Re, t[1].Im, tCos[i], tSin[i]);
				t[2].Re = o1;
				t[2].Im = o2;

				data[i].Re = t[0].Re + t[2].Re;
				data[i].Im = t[2].Im - t[0].Im;
				data[len2 - i].Re = t[0].Re - t[2].Re;
				data[len2 - i].Im = t[2].Im + t[0].Im;
			}

			if (inv != 0)
				s.Fn[0](s.Sub[0], _dst, data, Marshal.SizeOf<TXComplex>());
			else
			{
				// Move [0].im to the last position, as convention requires
				data[len2].Re = data[0].Im;
				data[0].Im = data[len2].Im = TXSample.Zero;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Rdft_R2C(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1664
		{
			Rdft_Decl(s, _dst, _src, stride, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Rdft_C2R(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1664
		{
			Rdft_Decl(s, _dst, _src, stride, 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Rdft_Half_Decl(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride, AvTxFlags mode, c_int mod2)//XX 1735
		{
			c_int len = s.Len;
			c_int len2 = len >> 1;
			c_int len4 = len >> 2;
			c_int aligned_Len4 = Macros.FFAlign(len, 4) / 4;
			CPointer<TXSample> fact = s.Exp.Cast<TXComplex, TXSample>();
			CPointer<TXSample> tCos = fact + 8;
			CPointer<TXSample> tSin = tCos + aligned_Len4;
			CPointer<TXComplex> data = _dst.ToPointer<TXComplex>();
			CPointer<TXSample> @out = _dst.Cast<TXComplex, TXSample>();		// Half-complex is forward only
			TXSample tmp_Mid = TXSample.Zero;
			CPointer<TXSample> tmp = new CPointer<TXSample>(4);
			TXComplex sf, sl;

			s.Fn[0](s.Sub[0], _dst, _src, Marshal.SizeOf<TXComplex>());

			TXSample tmp_Dc = data[0].Re;
			data[0].Re = tmp_Dc + data[0].Im;
			tmp_Dc = tmp_Dc - data[0].Im;

			data[0].Re = Mult(fact[0], data[0].Re);
			tmp_Dc = Mult(fact[1], tmp_Dc);
			data[len4].Re = Mult(fact[2], data[len4].Re);

			if (mod2 == 0)
				data[len4].Im = Mult(fact[3], data[len4].Im);
			else
			{
				sf = data[len4];
				sl = data[len4 + 1];

				if (mode == AvTxFlags.Real_To_Real)
					tmp[0] = Mult(fact[4], (sf.Re + sl.Re));
				else
					tmp[0] = Mult(fact[5], (sf.Im - sl.Im));

				tmp[1] = Mult(fact[6], (sf.Im + sl.Im));
				tmp[2] = Mult(fact[7], (sf.Re - sl.Re));

				if (mode == AvTxFlags.Real_To_Real)
				{
					tmp[3] = (tmp[1] * tCos[len4]) - (tmp[2] * tSin[len4]);
					tmp_Mid = (tmp[0] - tmp[3]);
				}
				else
				{
					tmp[3] = (tmp[1] * tSin[len4]) + (tmp[2] * tCos[len4]);
					tmp_Mid = (tmp[0] + tmp[3]);
				}
			}

			// NOTE: unrolling this breaks non-mod8 lengths
			for (c_int i = 1; i <= len4; i++)
			{
				sf = data[i];
				sl = data[len2 - i];

				if (mode == AvTxFlags.Real_To_Real)
					tmp[0] = Mult(fact[4], (sf.Re + sl.Re));
				else
					tmp[0] = Mult(fact[5], (sf.Im - sl.Im));

				tmp[1] = Mult(fact[6], (sf.Im + sl.Im));
				tmp[2] = Mult(fact[7], (sf.Re - sl.Re));

				if (mode == AvTxFlags.Real_To_Real)
				{
					tmp[3] = (tmp[1] * tCos[i]) - (tmp[2] * tSin[i]);
					@out[i] = (tmp[0] + tmp[3]);
					@out[len - i] = (tmp[0] - tmp[3]);
				}
				else
				{
					tmp[3] = (tmp[1] * tSin[i]) + (tmp[2] * tCos[i]);
					@out[i - 1] = (tmp[3] - tmp[0]);
					@out[len - i - 1] = (tmp[0] + tmp[3]);
				}
			}

			for (c_int i = 1; i < (len4 + (mode == AvTxFlags.Real_To_Imaginary ? 1 : 0)); i++)
				@out[len2 - i] = @out[len - i];

			if (mode == AvTxFlags.Real_To_Real)
			{
				@out[len2] = tmp_Dc;

				if (mod2 != 0)
					@out[len4 + 1] = tmp_Mid * fact[5];
			}
			else if (mod2 != 0)
				@out[len4] = tmp_Mid;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Rdft_R2R(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1735
		{
			Rdft_Half_Decl(s, _dst, _src, stride, AvTxFlags.Real_To_Real, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Rdft_R2R_Mod2(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1735
		{
			Rdft_Half_Decl(s, _dst, _src, stride, AvTxFlags.Real_To_Real, 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Rdft_R2I(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1735
		{
			Rdft_Half_Decl(s, _dst, _src, stride, AvTxFlags.Real_To_Imaginary, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_Rdft_R2I_Mod2(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1735
		{
			Rdft_Half_Decl(s, _dst, _src, stride, AvTxFlags.Real_To_Imaginary, 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Dct_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 1840
		{
			TScaleType rsc = (TScaleType)scale;

			if (inv != 0)
			{
				len *= 2;
				s.Len *= 2;
				rsc *= TScaleType.CreateChecked(0.5);
			}

			c_int ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Rdft"), flags, null, len, inv, rsc);

			if (ret != 0)
				return ret;

			s.Exp = Mem.Av_MAlloc<TXSample>((size_t)((len / 2) * 3));

			if (s.Exp.IsNull)
				return Error.ENOMEM;

			CPointer<TXSample> tab = s.Exp.ToPointer<TXSample>();

			c_double freq = Math.PI / (len * 2);

			for (c_int i = 0; i < len; i++)
				tab[i] = Rescale(TXSample.CreateChecked(CMath.cos(i * freq) * ((inv == 0 ? 1 : 0) + 1)));

			if (inv != 0)
			{
				for (c_int i = 0; i < (len / 2); i++)
					tab[len + i] = Rescale(TXSample.CreateChecked(0.5 / CMath.sin(((2 * i) + 1) * freq)));
			}
			else
			{
				for (c_int i = 0; i < (len / 2); i++)
					tab[len + i] = Rescale(TXSample.CreateChecked(CMath.cos(((len - (2 * i)) - 1) * freq)));
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_DctII(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1883
		{
			DoDctII(s, _dst.ToPointer<TXSample>(), _src.ToPointer<TXSample>(), stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void DoDctII(AvTxContext s, CPointer<TXSample> dst, CPointer<TXSample> src, ptrdiff_t stride)//XX 1883
		{
			c_int len = s.Len;
			c_int len2 = len >> 1;
			CPointer<TXSample> exp = s.Exp.ToPointer<TXSample>();
			TXSample tmp1, tmp2;

			for (c_int i = 0; i < len2; i++)
			{
				TXSample in1 = src[i];
				TXSample in2 = src[len - i - 1];
				TXSample _s = exp[len + i];

				tmp1 = (in1 + in2) * TXSample.CreateChecked(0.5);
				tmp2 = (in1 - in2) * _s;

				src[i] = tmp1 + tmp2;
				src[len - i - 1] = tmp1 - tmp2;
			}

			s.Fn[0](s.Sub[0], dst, src, Marshal.SizeOf<TXComplex>());

			TXSample next = dst[len];

			for (c_int i = len - 2; i > 0; i -= 2)
			{
				CMul(out TXSample tmp, out dst[i], exp[len - i], exp[i], dst[i + 0], dst[i + 1]);

				dst[i + 1] = next;

				next += tmp;
			}

			dst[0] = exp[0] * dst[0];
			dst[1] = next;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_DctIII(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 1943
		{
			DoDctII(s, _dst.ToPointer<TXSample>(), _src.ToPointer<TXSample>(), stride);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void DoDctIII(AvTxContext s, CPointer<TXSample> dst, CPointer<TXSample> src, ptrdiff_t stride)//XX 1943
		{
			c_int len = s.Len;
			c_int len2 = len >> 1;
			CPointer<TXSample> exp = s.Exp.ToPointer<TXSample>();
			TXSample tmp1, tmp2 = TXSample.CreateChecked(2) * src[len - 1];

			src[len] = tmp2;

			for (c_int i = len - 2; i >= 2; i -= 2)
			{
				TXSample val1 = src[i - 0];
				TXSample val2 = src[i - 1] - src[i + 1];

				CMul(out src[i + 1], out src[i], exp[len - i], exp[i], val1, val2);
			}

			s.Fn[0](s.Sub[0], dst, src, sizeof(c_float));

			for (c_int i = 0; i < len2; i++)
			{
				TXSample in1 = dst[i];
				TXSample in2 = dst[len - i - 1];
				TXSample c = exp[len + i];

				tmp1 = in1 + in2;
				tmp2 = in1 - in2;
				tmp2 *= c;

				dst[i] = tmp1 + tmp2;
				dst[len -i - 1] = tmp1 - tmp2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_DcstI_Init(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale)//XX 2014
		{
			TScaleType rsc = (TScaleType)scale;

			if (inv != 0)
			{
				len *= 2;
				s.Len *= 2;
				rsc *= TScaleType.CreateChecked(0.5);
			}

			// We want a half-complex RDFT
			flags |= cd.Type == Tx_Type("Dct_I") ? AvTxFlags.Real_To_Real : AvTxFlags.Real_To_Imaginary;

			c_int ret = Tx.FF_Tx_Init_SubTx(s, Tx_Type("Rdft"), flags, null, (len - 1 + (2 * (cd.Type == Tx_Type("Dst_I") ? 1 : 0))) * 2, 0, rsc);

			if (ret != 0)
				return ret;

			s.Tmp = Mem.Av_MAlloc<TXSample>((size_t)((len + 1) * 2));

			if (s.Tmp.IsNull)
				return Error.ENOMEM;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_DctI(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 2046
		{
			CPointer<TXSample> dst = _dst.ToPointer<TXSample>();
			CPointer<TXSample> src = _src.ToPointer<TXSample>();
			c_int len = s.Len - 1;
			CPointer<TXSample> tmp = s.Tmp.ToPointer<TXSample>();

			stride /= Marshal.SizeOf<TXSample>();

			for (c_int i = 0; i < len; i++)
				tmp[i] = tmp[(2 * len) - i] = src[i * stride];

			tmp[len] = src[len * stride];	// Middle

			s.Fn[0](s.Sub[0], dst, tmp, Marshal.SizeOf<TXSample>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FF_Tx_DstI(AvTxContext s, IPointer _dst, IPointer _src, ptrdiff_t stride)//XX 2064
		{
			CPointer<TXSample> dst = _dst.ToPointer<TXSample>();
			CPointer<TXSample> src = _src.ToPointer<TXSample>();
			c_int len = s.Len - 1;
			CPointer<TXSample> tmp = s.Tmp.ToPointer<TXSample>();

			stride /= Marshal.SizeOf<TXSample>();

			tmp[0] = TXSample.Zero;

			for (c_int i = 1; i < len; i++)
			{
				TXSample a = src[(i - 1) * stride];

				tmp[i] = -a;
				tmp[(2 * len) - i] = a;
			}

			tmp[len] = TXSample.Zero;	// i == n, Nyquist

			s.Fn[0](s.Sub[0], dst, tmp, sizeof(c_float));
		}



		/********************************************************************/
		/// <summary>
		/// Typed init function to initialize an MDCT exptab in a context.
		/// If pre_tab is set, duplicates the entire table, with the first
		/// copy being shuffled according to pre_tab, and the second copy
		/// being the original
		/// </summary>
		/********************************************************************/
		private c_int FF_Tx_Mdct_Gen_Exp(AvTxContext s, CPointer<c_int> pre_Tab)//XX 2115
		{
			c_int off = 0;
			c_int len4 = s.Len >> 1;
			c_double scale = s.Scale_D;
			c_double theta = (scale < 0 ? len4 : 0) + (1.0 / 8.0);
			size_t alloc = (size_t)(pre_Tab.IsNotNull ? 2 * len4 : len4);

			CPointer<TXComplex> exp = Mem.Av_MAlloc_Array<TXComplex>(alloc);
			s.Exp = exp;

			if (s.Exp.IsNull)
				return Error.ENOMEM;

			scale = CMath.sqrt(CMath.fabs(scale));

			if (pre_Tab.IsNotNull)
				off = len4;

			for (c_int i = 0; i < len4; i++)
			{
				c_double alpha = (Math.PI / 2.0) * (i + theta) / len4;

				exp[off + i] = new TXComplex
				{
					Re = Rescale(TXSample.CreateChecked(CMath.cos(alpha) * scale)),
					Im = Rescale(TXSample.CreateChecked(CMath.sin(alpha) * scale))
				};
			}

			if (pre_Tab.IsNotNull)
			{
				for (c_int i = 0; i < len4; i++)
					exp[i] = exp[len4 + pre_Tab[i]];
			}

			return 0;
		}
		#endregion
	}
}
