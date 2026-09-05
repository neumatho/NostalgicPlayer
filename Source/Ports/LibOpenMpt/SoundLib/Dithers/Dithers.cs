/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers
{
	/// <summary>
	/// TNE: The original takes noDither, defaultDither and defaultChannels
	/// as non-type template arguments, and derives the number of dithers
	/// from std::variant_size. C# has no non-type type arguments, and the
	/// members that hand out those values are static, so they cannot be
	/// injected either. They live on the derived class instead.
	/// TNE: The original also takes its name table as a template argument
	/// and inherits it. A C# class cannot inherit from a type argument, so
	/// DitherNamesOpenMpt is named directly
	/// </summary>
	internal abstract class Dithers<Seeding_Random_Engine, AvailableDithers> : DitherNamesOpenMpt where Seeding_Random_Engine : IEngine_Seed_Traits, IEngine_Traits where AvailableDithers : DithersOpenMpt, new()
	{
		protected readonly Seeding_Random_Engine m_Prng;
		protected readonly AvailableDithers m_Dithers;

		/********************************************************************/
		/// <summary>
		/// Constructor.
		/// TNE: The original takes the random device that the seeding prng
		/// is made from. A constructor cannot be generic in C#, so the
		/// derived class makes the prng in a factory method instead
		/// </summary>
		/********************************************************************/
		protected Dithers(Seeding_Random_Engine prng, size_t mode, size_t channels)
		{
			m_Prng = prng;
			m_Dithers = new AvailableDithers();

			Set_Mode(mode, channels);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the underlying variant (C++ AvailableDithers＆ Variant()).
		/// The variant is a reference type now, so returning it gives the
		/// same reference-based access the C++ reference return provides
		/// </summary>
		/********************************************************************/
		public AvailableDithers Variant()
		{
			return m_Dithers;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetMode(size_t mode, size_t channels)
		{
			if ((mode == GetMode()) && (channels == GetChannels()))
			{
				m_Dithers.visit(dither => dither.Reset(), dither => dither.Reset(), dither => dither.Reset(), dither => dither.Reset());
				return;
			}

			Set_Mode(mode, channels);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetMode(size_t mode)
		{
			if (mode == GetMode())
			{
				m_Dithers.visit(dither => dither.Reset(), dither => dither.Reset(), dither => dither.Reset(), dither => dither.Reset());
				return;
			}

			Set_Mode(mode, GetChannels());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t GetMode()
		{
			return m_Dithers.index();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t GetChannels()
		{
			return m_Dithers.visit(dither => dither.GetChannels(), dither => dither.GetChannels(), dither => dither.GetChannels(), dither => dither.GetChannels());
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract void Set_Mode(size_t mode, size_t channels);
		#endregion
	}
}
