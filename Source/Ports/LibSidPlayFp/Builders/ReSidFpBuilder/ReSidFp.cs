/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;
using Polycode.NostalgicPlayer.Ports.LibReSidFp;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Containers;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Builders.ReSidFpBuilder
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

			Reset(0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Filter6581Curve(double filterCurve)
		{
			sid.SetFilter6581Curve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Filter6581Range(double adjustment)
		{
			sid.SetFilter6581Range(adjustment);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Filter8580Curve(double filterCurve)
		{
			sid.SetFilter8580Curve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// Set the emulated SID combined waveforms
		/// </summary>
		/********************************************************************/
		public void CombinedWaveforms(SidConfig.sid_cw_t cws)
		{
			CombinedWaveforms combinedWaveforms;

			switch (cws)
			{
				case SidConfig.sid_cw_t.AVERAGE:
				{
					combinedWaveforms = LibReSidFp.Containers.CombinedWaveforms.AVERAGE;
					break;
				}

				case SidConfig.sid_cw_t.WEAK:
				{
					combinedWaveforms = LibReSidFp.Containers.CombinedWaveforms.WEAK;
					break;
				}

				case SidConfig.sid_cw_t.STRONG:
				{
					combinedWaveforms = LibReSidFp.Containers.CombinedWaveforms.STRONG;
					break;
				}

				default:
				{
					status = false;
					error = Resources.IDS_SID_ERR_INVALID_CW;
					return;
				}
			}

			sid.SetCombinedWaveforms(combinedWaveforms);
			status = true;
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
				sid.SetSamplingParameters(systemClock, samplingMethod, freq);
			}
			catch (SidErrorException)
			{
				status = false;
				error = Resources.IDS_SID_ERR_UNSUPPORTED_OUTPUT_FREQ;
				return;
			}

			// 20 ms buffer
			int bufferSize = (int)Math.Ceiling((freq / 1000.0f) * 20.0f);
			buffer = new short[bufferSize];

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
					sid.Input(0);
					break;
				}

				case SidConfig.sid_model_t.MOS8580:
				{
					chipModel = ChipModel.MOS8580;
					sid.Input(digiBoost ? -32768 : 0);
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
		protected override void Reset(uint8_t volume)
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
