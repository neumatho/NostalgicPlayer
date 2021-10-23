/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.Builders
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class ReSid : SidEmu
	{
		private readonly Sid sid;
		private uint8_t voiceMask;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReSid(SidBuilder builder) : base(builder)
		{
			sid = new Sid();
			voiceMask = 0x07;

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



		/********************************************************************/
		/// <summary>
		/// Return array of default spline interpolation points to map FC to
		/// filter cutoff frequency
		/// </summary>
		/********************************************************************/
		public void FcDefault(Spline.FCPoint[] points, out int count)
		{
			sid.FcDefault(points, out count);
		}



		/********************************************************************/
		/// <summary>
		/// Return FC spline plotter object
		/// </summary>
		/********************************************************************/
		public Spline.PointPlotter FcPlotter()
		{
			return sid.FcPlotter();
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
			bufferPos += sid.Clock((int)cycles, buffer, bufferPos, OUTPUTBUFFERSIZE - bufferPos, 1);
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
					samplingMethod = fast ? SamplingMethod.Fast : SamplingMethod.Interpolate;
					break;
				}

				case SidConfig.sampling_method_t.RESAMPLE_INTERPOLATE:
				{
					samplingMethod = fast ? SamplingMethod.ResampleFast : SamplingMethod.ResampleInterpolate;
					break;
				}

				default:
				{
					status = false;
					error = Resources.IDS_SID_ERR_INVALID_SAMPLING;
					return;
				}
			}

			if (!sid.SetSamplingParameters(systemClock, samplingMethod, freq))
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
			short sample = 0;
			voiceMask &= 0x07;

			switch (model)
			{
				case SidConfig.sid_model_t.MOS6581:
				{
					chipModel = ChipModel.Mos6581;
					break;
				}

				case SidConfig.sid_model_t.MOS8580:
				{
					chipModel = ChipModel.Mos8580;
					if (digiBoost)
					{
						voiceMask |= 0x08;
						sample = -32768;
					}
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
//			sid.SetVoiceMask(voiceMask);
			sid.Input(sample);

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
			return (uint8_t)sid.Read(addr);
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
