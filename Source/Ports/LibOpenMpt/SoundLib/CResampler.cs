/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using Sinc_Type = System.Int16;

using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal class CResampler
	{
		public const c_int Sinc_Width = 8;

		public const c_int Sinc_Phases_Bits = 12;
		public const c_int Sinc_Phases = 1 << Sinc_Phases_Bits;

		public const c_int Sinc_Mask = Sinc_Phases - 1;

		public const c_int Sinc_QuantShift = 15;

		public CResamplerSettings m_Settings = new CResamplerSettings();
		public readonly CWindowedFir m_WindowedFir = new CWindowedFir();

		private CResamplerSettings m_OldSettings = new CResamplerSettings();

		private static readonly CResampler _s_CachedResampler = new CResampler(true);

		public readonly Sinc_Type[] gKaiserSinc = new Sinc_Type[Sinc_Phases * 8];		// Upsampling
		public readonly Sinc_Type[] gDownSample13x = new Sinc_Type[Sinc_Phases * 8];	// Downsample 1.333x
		public readonly Sinc_Type[] gDownSample2x = new Sinc_Type[Sinc_Phases * 8];		// Downsample 2x

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CResampler(bool fresh_Generate = false)
		{
			if (fresh_Generate)
				InitializeTablesFromScratch(true);
			else
				InitializeTables();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void InitializeTables()
		{
			InitializeTablesFromCache();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void UpdateTables()
		{
			InitializeTablesFromScratch(false);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void InitFloatMixerTables()
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void InitializeTablesFromScratch(bool force)
		{
			bool initParameterIndependentTables = false;

			if (force)
				initParameterIndependentTables = true;

			if (initParameterIndependentTables)
			{
				InitFloatMixerTables();

//XX				blepTables.Initialize();

				Tables.GetSinc(gKaiserSinc, 9.6377, 0.97);
				Tables.GetSinc(gDownSample13x, 8.5, 0.5);
				Tables.GetSinc(gDownSample2x, 7.0, 0.425);
			}

			if ((m_OldSettings == m_Settings) && !force)
				return;

			m_WindowedFir.InitTable(m_Settings.gdWFirCutoff, m_Settings.gbWFirType);

			m_OldSettings = m_Settings;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private CResampler GetCachedResampler()
		{
			return _s_CachedResampler;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void InitializeTablesFromCache()
		{
			CResampler s_CachedResampler = GetCachedResampler();

			InitFloatMixerTables();

			CMemory.copy(s_CachedResampler.gKaiserSinc, new CPointer<Sinc_Type>(s_CachedResampler.gKaiserSinc) + (Sinc_Phases * 8), gKaiserSinc);
			CMemory.copy(s_CachedResampler.gDownSample13x, new CPointer<Sinc_Type>(s_CachedResampler.gDownSample13x) + (Sinc_Phases * 8), gDownSample13x);
			CMemory.copy(s_CachedResampler.gDownSample2x, new CPointer<Sinc_Type>(s_CachedResampler.gDownSample2x) + (Sinc_Phases * 8), gDownSample2x);
			CMemory.copy(s_CachedResampler.m_WindowedFir.Lut, new CPointer<WFir_Type>(s_CachedResampler.m_WindowedFir.Lut) + (WindowedFir.WFir_LutLen * WindowedFir.WFir_Width), m_WindowedFir.Lut);

//XX			blepTables = s_CachedResampler.blepTables;
		}
		#endregion
	}
}
