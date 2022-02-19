/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cia;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Vic_II;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64
{
	/// <summary>
	/// Commodore 64 emulator core.
	///
	/// It consists of the following chips:
	/// - CPU 6510
	/// - VIC-II 6567/6569/6572/6573
	/// - CIA 6526/8521
	/// - SID 6581/8580
	/// - PLA 7700/82S100
	/// - Color RAM 2114
	/// - System RAM 4164-20/50464-150
	/// - Character ROM 2332
	/// - Basic ROM 2364
	/// - Kernal ROM 2364
	/// </summary>
	internal sealed class C64
	{
		/// <summary>
		/// C64 model
		/// </summary>
		public enum model_t
		{
			/// <summary>
			/// PAL C64
			/// </summary>
			PAL_B = 0,

			/// <summary>
			/// NTSC C64
			/// </summary>
			NTSC_M,

			/// <summary>
			/// Old NTSC C64
			/// </summary>
			OLD_NTSC_M,

			/// <summary>
			/// C64 Drean
			/// </summary>
			PAL_N,

			/// <summary>
			/// C64 Brasil
			/// </summary>
			PAL_M
		}

		/// <summary>
		/// CIA chip model
		/// </summary>
		public enum cia_model_t
		{
			/// <summary>
			/// Old CIA
			/// </summary>
			OLD = 0,

			/// <summary>
			/// New CIA
			/// </summary>
			NEW,

			/// <summary>
			/// Old CIA, special batch labeled 4485
			/// </summary>
			OLD_4485
		}

		private class model_data_t
		{
			public model_data_t(double colorBurst, double divider, double powerFreq, Mos656x.model_t vicModel)
			{
				this.colorBurst = colorBurst;
				this.divider = divider;
				this.powerFreq = powerFreq;
				this.vicModel = vicModel;
			}

			/// <summary>
			/// Colorburst frequency in Hertz
			/// </summary>
			public double colorBurst;

			/// <summary>
			/// Clock frequency divider
			/// </summary>
			public double divider;

			/// <summary>
			/// Power line frequency in Hertz
			/// </summary>
			public double powerFreq;

			/// <summary>
			/// Video chip model
			/// </summary>
			public Mos656x.model_t vicModel;
		}

		private class cia_model_data_t
		{
			public cia_model_data_t(Mos652x.model_t ciaModel)
			{
				this.ciaModel = ciaModel;
			}

			/// <summary>
			/// CIA chip model
			/// </summary>
			public Mos652x.model_t ciaModel;
		}

		// Color burst frequencies:
		//
		// NTSC  - 3.579545455 MHz = 315/88 MHz
		// PAL-B - 4.43361875 MHz = 283.75 * 15625 Hz + 25 Hz
		// PAL-M - 3.57561149 MHz
		// PAL-N - 3.58205625 MHz

		private static readonly model_data_t[] modelData =
		{
			new model_data_t(4433618.75, 18.0, 50.0, Mos656x.model_t.MOS6569),		// PAL-B
			new model_data_t(3579545.455, 14.0, 60.0, Mos656x.model_t.MOS6567R8),		// NTSC-M
			new model_data_t(3579545.455, 14.0, 60.0, Mos656x.model_t.MOS6567R56A),	// Old NTSC-M
			new model_data_t(3582056.25, 14.0, 50.0, Mos656x.model_t.MOS6572),		// PAL-N
			new model_data_t(3575611.49, 14.0, 50.0, Mos656x.model_t.MOS6573)			// PAL-M
		};

		private static readonly cia_model_data_t[] ciaModelData =
		{
			new cia_model_data_t(Mos652x.model_t.MOS6526),			// Old
			new cia_model_data_t(Mos652x.model_t.MOS8521),			// New
			new cia_model_data_t(Mos652x.model_t.MOS6526W4485)		// Old week 4485
		};

		#region Private implementation of C64Env
		private class PrivateC64Env : C64Env
		{
			private readonly C64 parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PrivateC64Env(EventScheduler scheduler, C64 parent) : base(scheduler)
			{
				this.parent = parent;
			}

			#region Overrides
			/********************************************************************/
			/// <summary>
			/// Access memory as seen by CPU
			/// </summary>
			/********************************************************************/
			public override uint8_t CpuRead(uint_least16_t addr)
			{
				return parent.mmu.CpuRead(addr);
			}



			/********************************************************************/
			/// <summary>
			/// Access memory as seen by CPU
			/// </summary>
			/********************************************************************/
			public override void CpuWrite(uint_least16_t addr, uint8_t data)
			{
				parent.mmu.CpuWrite(addr, data);
			}



			/********************************************************************/
			/// <summary>
			/// IRQ trigger signal.
			///
			/// Calls permitted any time, but normally originated by chips at
			/// PHI1
			/// </summary>
			/********************************************************************/
			public override void InterruptIrq(bool state)
			{
				if (state)
				{
					if (parent.irqCount == 0)
						parent.cpu.TriggerIrq();

					parent.irqCount++;
				}
				else
				{
					parent.irqCount--;
					if (parent.irqCount == 0)
						parent.cpu.ClearIrq();
				}
			}



			/********************************************************************/
			/// <summary>
			/// NMI trigger signal.
			///
			/// Calls permitted any time, but normally originated by chips at
			/// PHI1
			/// </summary>
			/********************************************************************/
			public override void InterruptNmi()
			{
				parent.cpu.TriggerNmi();
			}



			/********************************************************************/
			/// <summary>
			/// BA signal.
			///
			/// Calls permitted during PHI1
			/// </summary>
			/********************************************************************/
			public override void SetBa(bool state)
			{
				// Only react to changes in state
				if (state == parent.oldBaState)
					return;

				parent.oldBaState = state;

				// Signal changes in BA to interested parties
				parent.cpu.SetRdy(state);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void Lightpen(bool state)
			{
				if (!state)
					parent.vic.TriggerLightpen();
				else
					parent.vic.ClearLightpen();
			}
			#endregion
		}
		#endregion

		private readonly PrivateC64Env c64EnvObject;

		/// <summary>
		/// System clock frequency
		/// </summary>
		private double cpuFrequency;

		/// <summary>
		/// Number if sources asserting IRQ
		/// </summary>
		private int irqCount;

		/// <summary>
		/// BA state
		/// </summary>
		private bool oldBaState;

		/// <summary>
		/// System event context
		/// </summary>
		private readonly EventScheduler eventScheduler = new EventScheduler();

		/// <summary>
		/// CPU
		/// </summary>
		private readonly C64Cpu cpu;

		/// <summary>
		/// CIA1
		/// </summary>
		private readonly C64Cia1 cia1;

		/// <summary>
		/// CIA2
		/// </summary>
		private readonly C64Cia2 cia2;

		/// <summary>
		/// VIC II
		/// </summary>
		private readonly C64Vic vic;

		/// <summary>
		/// Color RAM
		/// </summary>
		private readonly ColorRamBank colorRamBank = new ColorRamBank();

		/// <summary>
		/// SID
		/// </summary>
		private readonly SidBank sidBank = new SidBank();

		/// <summary>
		/// Extra SIDs
		/// </summary>
		private readonly sidBankMap_t extraSidBanks = new sidBankMap_t();

		/// <summary>
		/// I/O area #1 and #2
		/// </summary>
		private readonly DisconnectedBusBank disconnectedBusBank;

		/// <summary>
		/// I/O area
		/// </summary>
		private readonly IOBank ioBank = new IOBank();

		/// <summary>
		/// MMU chip
		/// </summary>
		private readonly Mmu mmu;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64()
		{
			c64EnvObject = new PrivateC64Env(eventScheduler, this);
			cpuFrequency = GetCpuFreq(model_t.PAL_B);
			cpu = new C64Cpu(c64EnvObject);
			cia1 = new C64Cia1(c64EnvObject);
			cia2 = new C64Cia2(c64EnvObject);
			vic = new C64Vic(c64EnvObject);
			mmu = new Mmu(eventScheduler, ioBank);
			disconnectedBusBank = new DisconnectedBusBank(mmu);

			ResetIoBank();
		}



		/********************************************************************/
		/// <summary>
		/// Set hook for VICE tests
		/// </summary>
		/********************************************************************/
		public void SetTestHook(C64Cpu.TestHookHandler handler)
		{
			cpu.SetTestHook(handler);
		}



		/********************************************************************/
		/// <summary>
		/// Get C64 event scheduler
		/// </summary>
		/********************************************************************/
		public EventScheduler GetEventScheduler()
		{
			return eventScheduler;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint_least32_t GetTime()
		{
			return (uint_least32_t)(((eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI1) * 1000) / cpuFrequency) / 1000);
		}



		/********************************************************************/
		/// <summary>
		/// Clock the emulation
		/// </summary>
		/********************************************************************/
		public void Clock()
		{
			eventScheduler.Clock();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			eventScheduler.Reset();

			cia1.Reset();
			cia2.Reset();
			vic.Reset();
			sidBank.Reset();
			colorRamBank.Reset();
			mmu.Reset();

			foreach (sidBankMap_value_type pair in extraSidBanks)
				pair.Value.Reset();

			irqCount = 0;
			oldBaState = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ResetCpu()
		{
			cpu.Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Set the C64 model
		/// </summary>
		/********************************************************************/
		public void SetModel(model_t model)
		{
			cpuFrequency = GetCpuFreq(model);
			vic.Chip(modelData[(int)model].vicModel);

			uint rate = (uint)(cpuFrequency / modelData[(int)model].powerFreq);
			cia1.SetDayOfTimeRate(rate);
			cia2.SetDayOfTimeRate(rate);
		}



		/********************************************************************/
		/// <summary>
		/// Set the CIA model
		/// </summary>
		/********************************************************************/
		public void SetCiaModel(cia_model_t model)
		{
			cia1.SetModel(ciaModelData[(int)model].ciaModel);
			cia2.SetModel(ciaModelData[(int)model].ciaModel);
		}



		/********************************************************************/
		/// <summary>
		/// Get the CPU clock speed
		/// </summary>
		/********************************************************************/
		public double GetMainCpuSpeed()
		{
			return cpuFrequency;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ISidMemory GetMemInterface()
		{
			return mmu;
		}



		/********************************************************************/
		/// <summary>
		/// Set the base SID
		/// </summary>
		/********************************************************************/
		public void SetBaseSid(C64Sid s)
		{
			sidBank.SetSid(s);
		}



		/********************************************************************/
		/// <summary>
		/// Add an extra SID
		/// </summary>
		/********************************************************************/
		public bool AddExtraSid(C64Sid s, int address)
		{
			// Check for valid address in the IO area range ($dxxx)
			if ((address & 0xf000) != 0xd000)
				return false;

			int idx = (address >> 8) & 0xf;

			// Only allow second SID chip in SID area ($d400-$d7ff)
			// or IO area ($de00-$dfff)
			if ((idx < 0x4) || ((idx > 0x7) && (idx < 0xe)))
				return false;

			// Add new SID bank
			if (extraSidBanks.TryGetValue(idx, out ExtraSidBank extraSidBank))
				extraSidBank.AddSid(s, address);
			else
			{
				extraSidBank = new ExtraSidBank();
				extraSidBanks[idx] = extraSidBank;

				extraSidBank.ResetSidMapper(ioBank.GetBank(idx));
				ioBank.SetBank(idx, extraSidBank);
				extraSidBank.AddSid(s, address);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Remove all the SIDs
		/// </summary>
		/********************************************************************/
		public void ClearSids()
		{
			sidBank.SetSid(null);

			ResetIoBank();

			extraSidBanks.Clear();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private double GetCpuFreq(model_t model)
		{
			// The crystal clock that drives the VIC II chip is four times
			// the color burst frequency
			double crystalFreq = modelData[(int)model].colorBurst * 4.0f;

			// The VIC II produces the two-phase system clock
			// by running the input clock through a divider
			return crystalFreq / modelData[(int)model].divider;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ResetIoBank()
		{
			ioBank.SetBank(0x0, vic);
			ioBank.SetBank(0x1, vic);
			ioBank.SetBank(0x2, vic);
			ioBank.SetBank(0x3, vic);
			ioBank.SetBank(0x4, sidBank);
			ioBank.SetBank(0x5, sidBank);
			ioBank.SetBank(0x6, sidBank);
			ioBank.SetBank(0x7, sidBank);
			ioBank.SetBank(0x8, colorRamBank);
			ioBank.SetBank(0x9, colorRamBank);
			ioBank.SetBank(0xa, colorRamBank);
			ioBank.SetBank(0xb, colorRamBank);
			ioBank.SetBank(0xc, cia1);
			ioBank.SetBank(0xd, cia2);
			ioBank.SetBank(0xe, disconnectedBusBank);
			ioBank.SetBank(0xf, disconnectedBusBank);
		}
		#endregion
	}
}
