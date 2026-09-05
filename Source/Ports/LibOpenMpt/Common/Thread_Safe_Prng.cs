/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Threading;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common
{
	/// <summary>
	/// PRNG
	/// </summary>
	internal class Thread_Safe_Prng<TRng, TResult> : IEngine_Traits<TResult> where TRng : IEngine_Traits<TResult>
	{
		private readonly Lock m = new Lock();
		private readonly TRng myRng;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Thread_Safe_Prng(TRng rng)
		{
			myRng = rng;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public TResult min()
		{
			lock (m)
			{
				return myRng.min();
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public TResult max()
		{
			lock (m)
			{
				return myRng.max();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int Result_Bits
		{
			get
			{
				lock (m)
				{
					return myRng.Result_Bits;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public TResult Invoke()
		{
			lock (m)
			{
				return myRng.Invoke();
			}
		}
	}
}
