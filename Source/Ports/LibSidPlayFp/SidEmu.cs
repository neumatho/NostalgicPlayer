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
		/// Buffer size. 5000 is roughly 50 ms at 96 kHz
		/// </summary>
		public const uint OUTPUTBUFFERSIZE = 5000;

		private readonly SidBuilder builder;

		protected EventScheduler eventScheduler = null;

		protected event_clock_t accessClk = 0;

		/// <summary>
		/// The sample buffer
		/// </summary>
		protected short[] buffer = null;

		/// <summary>
		/// Current position in buffer
		/// </summary>
		protected int bufferPos = 0;

		protected bool status = true;
		private bool isLocked = false;

		private bool isFilterDisabled = false;

		// Flags for muted voices
		private bool[] isMuted;

		protected string error;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected SidEmu(SidBuilder builder)
		{
			this.builder = builder;
			error = Resources.IDS_SID_NA;
			isMuted = new bool[4];
		}



		/********************************************************************/
		/// <summary>
		/// Clock the SID chip
		/// </summary>
		/********************************************************************/
		public abstract void Clock();



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void WriteReg(uint_least8_t addr, uint8_t data)
		{
			switch (addr)
			{
				case 0x04:
				{
					// Ignore writes to control register to mute voices.
					// Leave test/ring/sync bits untouched
					if (isMuted[0])
						data &= 0x0e;

					break;
				}

				case 0x0b:
				{
					if (isMuted[1])
						data &= 0x0e;

					break;
				}

				case 0x12:
				{
					if (isMuted[2])
						data &= 0x0e;

					break;
				}

				case 0x17:
				{
					// Ignore writes to filter to disable filter
					if (isFilterDisabled)
						data &= 0xf0;

					break;
				}

				case 0x18:
				{
					// Ignore writes to volume register to mute samples.
					// Works only for volume based digis.
					// Trick suggested by LMan
					if (isMuted[3])
						data |= 0x0f;

					break;
				}
			}

			Write(addr, data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Voice(uint voice, bool mute)
		{
			if (voice < 4)
				isMuted[voice] = mute;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Filter(bool enable)
		{
			isFilterDisabled = !enable;
		}



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
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract void Write(uint_least8_t addr, uint8_t data);



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
