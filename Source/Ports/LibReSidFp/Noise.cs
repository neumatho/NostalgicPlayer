/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Noise
	{
		private uint32_t m_wNoise = 34653463U;
		private int32_t m_hpNoise = 0;

		private uint32_t m_Mask;
		private int32_t m_Middle;

		private const int DefLevel = 5;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Noise()
		{
			SetLevel(DefLevel);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetLevel(int level)
		{
			m_Mask = (1U << level) - 1;
			m_Middle = 1 << (level - 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int32_t Output()
		{
			// White noise
			m_wNoise = (m_wNoise * 1664525U) + 1013904223U;

			// Reduce to 8bit signed
			int32_t n = (int32_t)((m_wNoise >> 20) & m_Mask) - m_Middle;

			// High passed noise
			m_hpNoise = m_hpNoise + ((n - m_hpNoise) / 8);

			return n - m_hpNoise;
		}
	}
}
