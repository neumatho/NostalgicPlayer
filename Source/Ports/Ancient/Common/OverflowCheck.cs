/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal static class OverflowCheck
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static size_t Sum(size_t a, size_t b)
		{
			// TODO: Add type traits to handle signed integers
			size_t ret = a + b;
			if (ret < a)
				throw new OverflowException();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32_t Sum(uint32_t a, uint32_t b)
		{
			// TODO: Add type traits to handle signed integers
			uint32_t ret = a + b;
			if (ret < a)
				throw new OverflowException();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint Sum(uint a, uint b, params uint[] args)
		{
			uint ret = Sum(a, b);

			foreach (uint v in args)
				ret = Sum(ret, v);

			return ret;
		}
	}
}
