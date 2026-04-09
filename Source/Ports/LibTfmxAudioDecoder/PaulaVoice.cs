/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder
{
	/// <summary>
	/// 
	/// </summary>
	public class PaulaVoice
	{
		private const uword Volume_Max = 64;

		#region Paula class
		/// <summary>
		/// 
		/// </summary>
		public class _Paula
		{
			/// <summary></summary>
			public CPointer<ubyte> Start;	// Start address
			/// <summary></summary>
			public uword Length;			// Length as number of 16-bit words
			/// <summary></summary>
			public uword Period;			// clock/frequency
			/// <summary></summary>
			public uword Volume;			// 0-64
		}
		#endregion

		/// <summary></summary>
		public readonly _Paula Paula = new _Paula();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void On()
		{
			// Intentionally left blank
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void Off()
		{
			// Intentionally left blank
		}



		/********************************************************************/
		/// <summary>
		/// Take parameters from paula.* (or just repeat.*)
		/// </summary>
		/********************************************************************/
		public virtual void TakeNextBuf()
		{
			// Intentionally left blank
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual uword GetLoopCount()
		{
			// Intentionally left blank
			return 0;
		}
	}
}
