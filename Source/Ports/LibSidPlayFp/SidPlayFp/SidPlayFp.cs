﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp
{
	/// <summary>
	/// Main entry point to the API
	/// </summary>
	public class SidPlayFp
	{
		private readonly Player sidPlayer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidPlayFp()
		{
			sidPlayer = new Player();
		}



		/********************************************************************/
		/// <summary>
		/// Set hook for VICE tests
		/// </summary>
		/********************************************************************/
		internal void SetTestHook(C64CpuBus.TestHookHandler handler)
		{
			sidPlayer.SetTestHook(handler);
		}



		/********************************************************************/
		/// <summary>
		/// Configure the engine
		/// Check Error for detailed message if something goes wrong
		/// </summary>
		/********************************************************************/
		public bool Config(SidConfig cfg, bool force = false)
		{
			return sidPlayer.Config(cfg, force);
		}



		/********************************************************************/
		/// <summary>
		/// Get the current engine configuration
		/// </summary>
		/********************************************************************/
		public SidConfig Config()
		{
			return sidPlayer.Config();
		}



		/********************************************************************/
		/// <summary>
		/// Stop the engine
		/// </summary>
		/********************************************************************/
		public void Stop()
		{
			sidPlayer.Stop();
		}



		/********************************************************************/
		/// <summary>
		/// Run emulation and produce samples to play if a buffer is given
		/// </summary>
		/********************************************************************/
		public uint_least32_t Play(short[] leftBuffer, short[] rightBuffer, uint_least32_t count)
		{
			return sidPlayer.Play(leftBuffer, rightBuffer, count);
		}



		/********************************************************************/
		/// <summary>
		/// Get the buffer pointers for each of the installed SID chip
		/// </summary>
		/********************************************************************/
		public void Buffers(short[][] buffers)
		{
			sidPlayer.Buffers(buffers);
		}



		/********************************************************************/
		/// <summary>
		/// Run the emulation for selected number of cycles.
		/// The value will be limited to a reasonable amount if too large
		/// </summary>
		/********************************************************************/
		public c_int Play(uint cycles)
		{
			return sidPlayer.Play(cycles);
		}



		/********************************************************************/
		/// <summary>
		/// Load a tune
		/// Check Error for detailed message if something goes wrong
		/// </summary>
		/********************************************************************/
		public bool Load(SidTune tune)
		{
			return sidPlayer.Load(tune);
		}



		/********************************************************************/
		/// <summary>
		/// Get the current player information
		/// </summary>
		/********************************************************************/
		public SidInfo Info()
		{
			return sidPlayer.Info();
		}



		/********************************************************************/
		/// <summary>
		/// Get the current playing time in seconds
		/// </summary>
		/********************************************************************/
		public uint_least32_t Time()
		{
			return sidPlayer.TimeMs() / 1000;
		}



		/********************************************************************/
		/// <summary>
		/// Error message
		/// </summary>
		/********************************************************************/
		public string Error()
		{
			return sidPlayer.Error();
		}



		/********************************************************************/
		/// <summary>
		/// Mute/unmute a SID channel
		/// </summary>
		/********************************************************************/
		public void Mute(uint sidNum, uint voice, bool enable)
		{
			sidPlayer.Mute(sidNum, voice, enable);
		}



		/********************************************************************/
		/// <summary>
		/// Enable/disable SID filter
		/// </summary>
		/********************************************************************/
		public void Filter(uint sidNum, bool enable)
		{
			sidPlayer.Filter(sidNum, enable);
		}



		/********************************************************************/
		/// <summary>
		/// Set kernal ROM
		/// </summary>
		/********************************************************************/
		public void SetKernal(uint8_t[] rom)
		{
			sidPlayer.SetKernal(rom);
		}



		/********************************************************************/
		/// <summary>
		/// Set basic ROM
		/// </summary>
		/********************************************************************/
		public void SetBasic(uint8_t[] rom)
		{
			sidPlayer.SetBasic(rom);
		}



		/********************************************************************/
		/// <summary>
		/// Set character ROM
		/// </summary>
		/********************************************************************/
		public void SetCharGen(uint8_t[] rom)
		{
			sidPlayer.SetCharGen(rom);
		}



		/********************************************************************/
		/// <summary>
		/// Set ROM images
		/// </summary>
		/********************************************************************/
		public void SetRoms(uint8_t[] kernal, uint8_t[] basic, uint8_t[] character)
		{
			SetKernal(kernal);
			SetBasic(basic);
			SetCharGen(character);
		}



		/********************************************************************/
		/// <summary>
		/// Get the CIA 1 Timer A programmed value
		/// </summary>
		/********************************************************************/
		public uint_least16_t GetCia1TimerA()
		{
			return sidPlayer.GetCia1TimerA();
		}



		/********************************************************************/
		/// <summary>
		/// Get the number of installed SID chips
		/// </summary>
		/********************************************************************/
		public uint InstalledSids()
		{
			return sidPlayer.InstalledSids();
		}
	}
}
