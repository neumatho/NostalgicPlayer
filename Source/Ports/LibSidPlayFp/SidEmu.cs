/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// 
	/// </summary>
	internal abstract class SidEmu : C64Sid
	{
		/// <summary>
		/// Buffer size. 5000 is roughly 5 ms at 96 kHz
		/// </summary>
		public const uint OUTPUTBUFFERSIZE = 5000;

		private readonly SidBuilder builder;

		protected EventScheduler eventScheduler;

		protected event_clock_t accessClk;

		/// <summary>
		/// The sample buffer
		/// </summary>
		protected short[] buffer;

		/// <summary>
		/// Current position in buffer
		/// </summary>
		protected int bufferPos;

		protected bool status;
		private bool isLocked;

		protected string error;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected SidEmu(SidBuilder builder)
		{
			this.builder = builder;
			eventScheduler = null;
			buffer = null;
			bufferPos = 0;
			status = true;
			isLocked = false;
			error = Resources.IDS_SID_NA;
		}



		/********************************************************************/
		/// <summary>
		/// Clock the SID chip
		/// </summary>
		/********************************************************************/
		public abstract void Clock();



		/********************************************************************/
		/// <summary>
		/// Set execution environment and lock SID to it
		/// </summary>
		/********************************************************************/
		public bool Lock(EventScheduler scheduler)
		{
			if (isLocked)
				return false;

			isLocked = true;
			eventScheduler = scheduler;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Unlock SID
		/// </summary>
		/********************************************************************/
		public void Unlock()
		{
			isLocked = false;
			eventScheduler = null;
		}



		/********************************************************************/
		/// <summary>
		/// Set SID model
		/// </summary>
		/********************************************************************/
		public abstract void Model(SidConfig.sid_model_t model, bool digiBoost);



		/********************************************************************/
		/// <summary>
		/// Set the sampling method
		/// </summary>
		/********************************************************************/
		public virtual void Sampling(float systemFreq, float outputFreq, SidConfig.sampling_method_t method, bool fast)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public SidBuilder Builder()
		{
			return builder;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current position in buffer
		/// </summary>
		/********************************************************************/
		public int BufferPos()
		{
			return bufferPos;
		}



		/********************************************************************/
		/// <summary>
		/// Set the position in the buffer
		/// </summary>
		/********************************************************************/
		public void BufferPos(int pos)
		{
			bufferPos = pos;
		}



		/********************************************************************/
		/// <summary>
		/// Get the buffer
		/// </summary>
		/********************************************************************/
		public short[] Buffer()
		{
			return buffer;
		}
	}
}
