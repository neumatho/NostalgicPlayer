/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.Builders.ReSidFpBuilder
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class ReSidFp : SidEmu
	{
		private readonly Sid sid;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReSidFp(SidBuilder builder) : base(builder)
		{
			sid = new Sid();

			buffer = new short[OUTPUTBUFFERSIZE];
			Reset(0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Filter(bool enable)
		{
			sid.EnableFilter(enable);
		}

		#region SidEmu overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Clock()
		{
			event_clock_t cycles = eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI1) - accessClk;
			accessClk += cycles;
			bufferPos += sid.Clock((uint)cycles, buffer, bufferPos);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Sampling(float systemClock, float freq, SidConfig.sampling_method_t method, bool fast)
		{
			SamplingMethod samplingMethod;

			switch (method)
			{
				case SidConfig.sampling_method_t.INTERPOLATE:
				{
					samplingMethod = SamplingMethod.DECIMATE;
					break;
				}

				case SidConfig.sampling_method_t.RESAMPLE_INTERPOLATE:
				{
					samplingMethod = SamplingMethod.RESAMPLE;
					break;
				}

				default:
				{
					status = false;
					error = Resources.IDS_SID_ERR_INVALID_SAMPLING;
					return;
				}
			}

			try
			{
				int halfFreq = (freq > 44000) ? 20000 : 9 * (int)freq / 20;
				sid.SetSamplingParameters(systemClock, samplingMethod, freq, halfFreq);
			}
			catch (SidErrorException)
			{
				status = false;
				error = Resources.IDS_SID_ERR_UNSUPPORTED_OUTPUT_FREQ;
				return;
			}

			status = true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the emulated SID model
		/// </summary>
		/********************************************************************/
		public override void Model(SidConfig.sid_model_t model, bool digiBoost)
		{
			ChipModel chipModel;

			switch (model)
			{
				case SidConfig.sid_model_t.MOS6581:
				{
					chipModel = ChipModel.MOS6581;
					break;
				}

				case SidConfig.sid_model_t.MOS8580:
				{
					chipModel = ChipModel.MOS8580;
					if (digiBoost)
						sid.Input(-32768);

					break;
				}

				default:
				{
					status = false;
					error = Resources.IDS_SID_ERR_INVALID_CHIP;
					return;
				}
			}

			sid.SetChipModel(chipModel);
			status = true;
		}
		#endregion

		#region C64Sid overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Reset(uint8_t volume)
		{
			accessClk = 0;
			sid.Reset();
			sid.Write(0x18, volume);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override uint8_t Read(uint_least8_t addr)
		{
			Clock();
			return sid.Read(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void Write(uint_least8_t addr, uint8_t data)
		{
			Clock();
			sid.Write(addr, data);
		}
		#endregion
	}
}
