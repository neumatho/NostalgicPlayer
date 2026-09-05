/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers
{
	/// <summary>
	/// 
	/// </summary>
	internal class DithersOpenMpt : variant<
		MultiChannelDither<Dither_None, Dither_None.Prng_Type>,
		MultiChannelDither<Dither_Default, Fast_Engine>,
		MultiChannelDither<Dither_ModPlug, Modplug_Dither>,
		MultiChannelDither<Dither_Simple, Fast_Engine>>
	{
	}

	/// <summary>
	/// TNE: These are the non-type template arguments the original hands to
	/// Dithers. C# has no non-type type arguments, and the members reading
	/// them are static, so they are declared here instead of in the base
	/// </summary>
	internal class DithersWrapperOpenMpt : Dithers<Good_Prng, DithersOpenMpt>
	{
		/// <summary></summary>
		public const size_t NoDither = 0;
		/// <summary></summary>
		public const size_t DefaultDither = 1;
		/// <summary></summary>
		public const size_t DefaultChannels = 4;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private DithersWrapperOpenMpt(Good_Prng prng, size_t mode, size_t channels) : base(prng, mode, channels)
		{
		}



		/********************************************************************/
		/// <summary>
		/// TNE: Stands in for the original constructor, which is a template
		/// on the random device. A constructor cannot be generic in C#, but
		/// a method can, so callers can hand in any random device here
		/// </summary>
		/********************************************************************/
		public static DithersWrapperOpenMpt Create<TRd_Result>(IEngine_Traits<TRd_Result> rd, size_t mode = DefaultDither, size_t channels = DefaultChannels) where TRd_Result : IBinaryInteger<TRd_Result>, IUnsignedNumber<TRd_Result>
		{
			return new DithersWrapperOpenMpt(Seed.Make_Prng<Good_Prng, TRd_Result>(rd), mode, channels);
		}



		/********************************************************************/
		/// <summary>
		/// TNE: The original reads this from std::variant_size, so the count
		/// has to match the number of alternatives in DithersOpenMpt above
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t GetNumDithers()
		{
			return 4;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static size_t GetDefaultDither()
		{
			return DefaultDither;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void Set_Mode(size_t mode, size_t channels)
		{
			switch (mode)
			{
				case 0:
				{
					m_Dithers.emplace(new MultiChannelDither<Dither_None, Dither_None.Prng_Type>(Dither_None.Prng_Init(m_Prng), channels));
					break;
				}

				case 1:
				{
					m_Dithers.emplace(new MultiChannelDither<Dither_Default, Fast_Engine>(Dither_Default.Prng_Init(m_Prng), channels));
					break;
				}

				case 2:
				{
					m_Dithers.emplace(new MultiChannelDither<Dither_ModPlug, Modplug_Dither>(Dither_ModPlug.Prng_Init(m_Prng), channels));
					break;
				}

				case 3:
				{
					m_Dithers.emplace(new MultiChannelDither<Dither_Simple, Fast_Engine>(Dither_Simple.Prng_Init(m_Prng), channels));
					break;
				}

				default:
				{
					m_Dithers.emplace(new MultiChannelDither<Dither_Default, Fast_Engine>(Dither_Default.Prng_Init(m_Prng), channels));
					break;
				}
			}
		}
	}
}
