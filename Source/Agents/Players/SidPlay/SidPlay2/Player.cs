/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.C64;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Mos6510;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Mos6526;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Roms;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2
{
	/// <summary>
	/// Main library code
	/// </summary>
	internal partial class Player : ICoUnknown, ISidPlay2, ICoAggregate, ISidTimer
	{
		private const int MaxSids = 2;
		private const int MapperSize = 32;
		private const int TimeBas = 10;

		private const double ClockFreqNtsc = 1022727.14;
		private const double ClockFreqPal = 985248.4;
		private const double VicFreqPal = 50.0;
		private const double VicFreqNtsc = 60.0;

		// Maximum values
		private const int MaxPrecision = 16;
		private const byte MaxOptimization = 2;

		// Delays <= MAX produce constant results
		// Delays >  MAX produce random results
		private const ushort MaxPowerOnDelay = 0x1fff;

		// Default settings
		private const uint DefaultSamplingFreq = 44100;
		private const byte DefaultPrecision = 16;
		private const byte DefaultOptimization = 1;
		private const ushort DefaultPowerOnDelay = MaxPowerOnDelay + 1;

		#region C64Environment class implementation
		private class MyC64Environment : C64Environment
		{
			private readonly Player parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyC64Environment(Player parent)
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override void EnvReset()
			{
				parent.EnvReset(true);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override byte EnvReadMemByte(ushort addr)
			{
				// Read from plain only to prevent execution of rom code
				return parent.readMemByte(addr);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override void EnvWriteMemByte(ushort addr, byte data)
			{
				// Writes must be passed to env version
				parent.writeMemByte(addr, data);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override bool EnvCheckBankJump(ushort addr)
			{
				return parent.EnvCheckBankJump(addr);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override byte EnvReadMemDataByte(ushort addr)
			{
				// Read from plain only to prevent execution of rom code
				return parent.readMemDataByte(addr);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override void EnvSleep()
			{
				parent.EnvSleep();
			}
		}
		#endregion

		#region C64Env class implementation
		private class MyC64Env : C64Env
		{
			private readonly Player parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyC64Env(Player parent, IEventContext context) : base(context)
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void InterruptIrq(bool state)
			{
				parent.InterruptIrq(state);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void InterruptNmi()
			{
				parent.cpu.TriggerNmi();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void SignalAec(bool state)
			{
				parent.cpu.AecSignal(state);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override byte ReadMemRamByte(ushort addr)
			{
				return parent.ram[addr];
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void Sid2Crc(byte data)
			{
				parent.Sid2Crc(data);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void Lightpen()
			{
				parent.vic.LightPen();
			}
		}
		#endregion

		#region CoUnknown class implementation
		private class MyCoUnknown : CoUnknown
		{
			private readonly Player parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyCoUnknown(Player parent, string name) : base(name)
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override bool IQuery(IId iid, out object implementation)
			{
				return parent.IQuery(iid, out implementation);
			}
		}
		#endregion

		#region CoAggregate class implementation
		private class MyCoAggregate : CoAggregate
		{
			private readonly Player parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyCoAggregate(Player parent, ISidUnknown unknown) : base(unknown)
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override ISidUnknown IUnknown()
			{
				return parent.IUnknown();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override bool IQuery(IId iid, out object implementation)
			{
				return parent.IQuery(iid, out implementation);
			}
		}
		#endregion

		#region EventRtc class
		private class EventRtc : Event.Event
		{
			private readonly IEventContext eventContext;
			private uint seconds;
			private uint period;
			private uint clk;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public EventRtc(IEventContext context) : base("RTC")
			{
				eventContext = context;
				seconds = 0;
			}



			/********************************************************************/
			/// <summary>
			/// Handle the event
			/// </summary>
			/********************************************************************/
			public override void DoEvent()
			{
				clk += period;

				// Fixed point 25.7 (approx 2 dp)
				uint cycles = clk >> 7;
				clk &= 0x7f;
				seconds++;

				Schedule(eventContext, cycles, EventPhase.ClockPhi1);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public uint GetTime()
			{
				return seconds;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Reset()
			{
				// Fixed point 25.7
				seconds = 0;
				clk = period & 0x7f;

				Schedule(eventContext, period >> 7, EventPhase.ClockPhi1);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Clock(double per)
			{
				// Fixed point 25.7
				period = (uint)(per / 10.0 * (1 << 7));
				Reset();
			}
		}
		#endregion

		#region Port structure
		private struct Port
		{
			public byte PrOut;
			public byte Ddr;
			public byte PrIn;
		}
		#endregion

		#region CRC table
		// Used for sid2crc (tracking sid register writes)
		private static readonly uint[] crc32Table = new uint[0x100]
		{
		    0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA,
		    0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3,
		    0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988,
		    0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91,
		    0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE,
		    0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7,
		    0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC,
		    0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5,
		    0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
		    0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B,
		    0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940,
		    0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59,
		    0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116,
		    0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F,
		    0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924,
		    0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D,
		    0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A,
		    0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
		    0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818,
		    0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01,
		    0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E,
		    0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457,
		    0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA, 0xFCB9887C,
		    0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
		    0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2,
		    0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB,
		    0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
		    0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9,
		    0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086,
		    0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F,
		    0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4,
		    0x59B33D17, 0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD,
		    0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A,
		    0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683,
		    0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8,
		    0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
		    0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE,
		    0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7,
		    0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC,
		    0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5,
		    0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252,
		    0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B,
		    0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60,
		    0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79,
		    0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
		    0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F,
		    0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04,
		    0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D,
		    0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A,
		    0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713,
		    0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38,
		    0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21,
		    0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E,
		    0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
		    0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C,
		    0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45,
		    0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2,
		    0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB,
		    0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 0x37D83BF0,
		    0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
		    0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6,
		    0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF,
		    0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
		    0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
		};
		#endregion

		#region Power on data
		private static readonly byte[] powerOn =
		{
			/* addr,   off,  rle, values */
			/*$0003*/ 0x83, 0x04, 0xaa, 0xb1, 0x91, 0xb3, 0x22,
			/*$000b*/ 0x03,       0x4c,
			/*$000f*/ 0x03,       0x04,
			/*$0016*/ 0x86, 0x05, 0x19, 0x16, 0x00, 0x0a, 0x76, 0xa3,
			/*$0022*/ 0x86, 0x03, 0x40, 0xa3, 0xb3, 0xbd,
			/*$002b*/ 0x85, 0x01, 0x01, 0x08,
			/*$0034*/ 0x07,       0xa0,
			/*$0038*/ 0x03,       0xa0,
			/*$003a*/ 0x01,       0xff,
			/*$0042*/ 0x07,       0x08,
			/*$0047*/ 0x04,       0x24,
			/*$0053*/ 0x8b, 0x01, 0x03, 0x4c,
			/*$0061*/ 0x0c,       0x8d,
			/*$0063*/ 0x02,       0x10,
			/*$0069*/ 0x84, 0x02, 0x8c, 0xff, 0xa0,
			/*$0071*/ 0x85, 0x1e, 0x0a, 0xa3, 0xe6, 0x7a, 0xd0, 0x02, 0xe6, 0x7b, 0xad, 0x00, 0x08, 0xc9, 0x3a, 0xb0, 0x0a, 0xc9, 0x20, 0xf0, 0xef, 0x38, 0xe9, 0x30, 0x38, 0xe9, 0xd0, 0x60, 0x80, 0x4f, 0xc7, 0x52, 0x58,
			/*$0091*/ 0x01,       0xff,
			/*$009a*/ 0x08,       0x03,
			/*$00b2*/ 0x97, 0x01, 0x3c, 0x03,
			/*$00c2*/ 0x8e, 0x03, 0xa0, 0x30, 0xfd, 0x01,
			/*$00c8*/ 0x82, 0x82, 0x03,
			/*$00cb*/ 0x80, 0x81, 0x01,
			/*$00ce*/ 0x01,       0x20,
			/*$00d1*/ 0x82, 0x01, 0x18, 0x05,
			/*$00d5*/ 0x82, 0x02, 0x27, 0x07, 0x0d,
			/*$00d9*/ 0x81, 0x86, 0x84,
			/*$00e0*/ 0x80, 0x85, 0x85,
			/*$00e6*/ 0x80, 0x86, 0x86,
			/*$00ed*/ 0x80, 0x85, 0x87,
			/*$00f3*/ 0x80, 0x03, 0x18, 0xd9, 0x81, 0xeb,
			/*$0176*/ 0x7f,       0x00,
			/*$01f6*/ 0x7f,       0x00,
			/*$0276*/ 0x7f,       0x00,
			/*$0282*/ 0x8b, 0x0a, 0x08, 0x00, 0xa0, 0x00, 0x0e, 0x00, 0x04, 0x0a, 0x00, 0x04, 0x10,
			/*$028f*/ 0x82, 0x01, 0x48, 0xeb,
			/*$0300*/ 0xef, 0x0b, 0x8b, 0xe3, 0x83, 0xa4, 0x7c, 0xa5, 0x1a, 0xa7, 0xe4, 0xa7, 0x86, 0xae,
			/*$0310*/ 0x84, 0x02, 0x4c, 0x48, 0xb2,
			/*$0314*/ 0x81, 0x1f, 0x31, 0xea, 0x66, 0xfe, 0x47, 0xfe, 0x4a, 0xf3, 0x91, 0xf2, 0x0e, 0xf2, 0x50, 0xf2, 0x33, 0xf3, 0x57, 0xf1, 0xca, 0xf1, 0xed, 0xf6, 0x3e, 0xf1, 0x2f, 0xf3, 0x66, 0xfe, 0xa5, 0xf4, 0xed, 0xf5

			/*Total 217*/
		};
		#endregion

		private enum PlayerType
		{
			Playing = 0,
			Paused,
			Stopped
		}

		private readonly MyC64Environment myC64Environment;
		private readonly MyC64Env myC64;
		private readonly MyCoUnknown myCoUnknown;
		private readonly MyCoAggregate myCoAggregate;

		private EventScheduler scheduler;

		private Sid6510 cpu;
		private NullSid nullSid;
		private C64XSid xsid;
		private C64Cia1 cia;
		private C64Cia2 cia2;
		private Sid6526 sid6526;
		private C64Vic vic;
		private SidLazyIPtr<ISidEmulation>[] sid;
		private int[] sidMapper;					// Mapping table in d4xx-d7xx

		private EventCallback mixerEvent;
		private EventRtc rtc;

		// User configuration settings
		private SidTuneInfo tuneInfo;
		private SidTune.SidTune tune;
		private byte[] ram;
		private byte[] rom;
		private Sid2Info info;
		private Sid2Config config;

		private double fastForwardFactor;
		private uint mileage;
		private int leftVolume;
		private int rightVolume;
		private volatile PlayerType playerState;
		private volatile int running;
		private int rand;
		private uint sid2Crc;
		private uint sid2CrcCount;
		private bool emulateStereo;

		// Mixer settings
		private uint sampleClock;
		private uint samplePeriod;
		private uint sampleCount;
		private uint sampleIndex;
		private sbyte[] leftSampleBuffer;
		private sbyte[] rightSampleBuffer;

		private Func<sbyte[], sbyte[], uint, uint> output;

		// Use pointers to please requirements of all the provided environments
		private Func<ushort, byte> readMemByte;
		private Action<ushort, byte> writeMemByte;
		private Func<ushort, byte> readMemDataByte;

		private Port port;

		private byte playBank;

		// Temp stuff
		private bool isKernel;
		private bool isBasic;
		private bool isIo;
		private bool isChar;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Player()
		{
			scheduler = new EventScheduler("SIDPlay 2");

			myC64Environment = new MyC64Environment(this);
			myC64 = new MyC64Env(this, scheduler);
			myCoUnknown = new MyCoUnknown(this, "SIDPlay 2");
			myCoAggregate = new MyCoAggregate(this, IUnknown());

			nullSid = new NullSid();

			info = new Sid2Info();
			config = new Sid2Config();

			cpu = new Sid6510(scheduler);
			xsid = new C64XSid(myC64);
			cia = new C64Cia1(myC64);
			cia2 = new C64Cia2(myC64);
			sid6526 = new Sid6526(myC64);
			vic = new C64Vic(myC64);

			mixerEvent = new EventCallback("Mixer", Mixer);
			rtc = new EventRtc(scheduler);

			tune = null;
			ram = null;
			rom = null;

			fastForwardFactor = 1.0;
			mileage = 0;
			playerState = PlayerType.Stopped;
			running = 0;
			sid2Crc = 0xffffffff;
			sid2CrcCount = 0;
			emulateStereo = true;

			sampleCount = 0;

			rand = new Random((int)DateTime.Now.Ticks).Next();

			// Set the ICs to use this environment
			cpu.SetEnvironment(myC64Environment);

			// SID initialize
			sid = new SidLazyIPtr<ISidEmulation>[MaxSids];

			for (int i = 0; i < MaxSids; i++)
				sid[i] = new SidLazyIPtr<ISidEmulation>(nullSid.IUnknown());

			xsid.Emulation(sid[0]);
			sid[0] = new SidLazyIPtr<ISidEmulation>(xsid.IUnknown());

			// Setup sid mapping table
			sidMapper = new int[MapperSize];

			for (int i = 0; i < MapperSize; i++)
				sidMapper[i] = 0;

			// Setup exported info
			info.Channels = 1;
			info.CpuFrequency = ClockFreqPal;
			info.DriverAddr = 0;
			info.DriverLength = 0;
			info.TuneInfo = null;
			info.EventContext = myC64.Context;

			// Number of SIDs support by this library
			info.MaxSids = MaxSids;
			info.Environment = Sid2Env.EnvR;
			info.Sid2Crc = 0;
			info.Sid2CrcCount = 0;

			// Configure default settings
			config.ClockDefault = Sid2Clock.Correct;
			config.ClockForced = false;
			config.ClockSpeed = Sid2Clock.Correct;
			config.Environment = info.Environment;
			config.ForceDualSids = false;
			config.EmulateStereo = emulateStereo;
			config.Frequency = DefaultSamplingFreq;
			config.Optimization = DefaultOptimization;
			config.PlayBack = Sid2PlayBack.Mono;
			config.Precision = DefaultPrecision;
			config.SidDefault = Sid2Model.ModelCorrect;
			config.SidEmulation = null;
			config.SidModel = Sid2Model.ModelCorrect;
			config.SidSamples = true;
			config.LeftVolume = 255;
			config.RightVolume = 255;
			config.SampleFormat = Sid2Sample.LittleSigned;
			config.PowerOnDelay = DefaultPowerOnDelay;
			config.Sid2CrcCount = 0;

			Config(config);
		}



		/********************************************************************/
		/// <summary>
		/// Create the player instance
		/// </summary>
		/********************************************************************/
		public static ISidUnknown Create()
		{
			ISidPlay2 sidPlay2 = new Player();

			return sidPlay2.IUnknown();
		}

		#region ISidUnknown implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ISidUnknown IUnknown()
		{
			return myCoUnknown.IUnknown();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool IQuery(IId iid, out object implementation)
		{
			if ((iid == ISidPlay2.IId()) || (iid == ISidUnknown.IId()))
				implementation = this.StaticCast<ISidPlay2>();
			else if (iid == ISidTimer.IId())
				implementation = this.StaticCast<ISidTimer>();
			else
			{
				implementation = null;
				return false;
			}

			return true;
		}
		#endregion

		#region C64Env implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void InterruptIrq(bool state)
		{
			if (state)
			{
				if (info.Environment == Sid2Env.EnvR)
					cpu.TriggerIrq();
				else
					FakeIrq();
			}
			else
				cpu.ClearIrq();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Sid2Crc(byte data)
		{
			if (sid2CrcCount < config.Sid2CrcCount)
			{
				info.Sid2CrcCount = ++sid2CrcCount;
				sid2Crc = (sid2Crc >> 8) ^ crc32Table[(sid2Crc & 0xff) ^ data];
				info.Sid2Crc = sid2Crc ^ 0xffffffff;
			}
		}
		#endregion

		#region ISidTimer implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint Mileage()
		{
			return mileage + Time();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint TimeBase()
		{
			return TimeBas;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint Time()
		{
			return rtc.GetTime();
		}
		#endregion

		#region ISidPlay2 implementation
		/********************************************************************/
		/// <summary>
		/// Holds the configuration
		/// </summary>
		/********************************************************************/
		public Sid2Config Configuration
		{
			get
			{
				return config;
			}

			set
			{
				Config(value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the emulator information
		/// </summary>
		/********************************************************************/
		public Sid2Info GetInfo()
		{
			return info;
		}



		/********************************************************************/
		/// <summary>
		/// Load the SID tune into the C64 environment
		/// </summary>
		/********************************************************************/
		public void Load(SidTune.SidTune sidTune)
		{
			tune = sidTune;

			if (sidTune == null)
			{
				// Unload tune
				info.TuneInfo = null;
				return;
			}

			info.TuneInfo = tuneInfo;

			// Un-mute all voices
			xsid.Mute(false);

			for (int i = 0; i < MaxSids; i++)
			{
				SidIPtr<ISidMixer> mixer = new SidIPtr<ISidMixer>(sid[i]);
				if (mixer.Obj != null)
				{
					byte v = 3;
					while (v-- != 0)
						mixer.Obj.Mute(v, false);
				}
			}

			// Must re-configure on the fly for stereo support!
			Config(config);
		}



		/********************************************************************/
		/// <summary>
		/// Run the emulators until the given buffer is filled
		/// </summary>
		/********************************************************************/
		public uint Play(sbyte[] leftBuffer, sbyte[] rightBuffer, uint length)
		{
			// Make sure a tune is loaded
			if (tune == null)
				return 0;

			// Setup sample information
			sampleIndex = 0;
			sampleCount = length;
			leftSampleBuffer = leftBuffer;
			rightSampleBuffer = rightBuffer;

			// Start the player loop
			playerState = PlayerType.Playing;
			running = 1;

			while (running > 0)
				scheduler.Clock();

			if (playerState == PlayerType.Stopped)
				Initialize();

			running = 0;

			return sampleIndex;
		}



		/********************************************************************/
		/// <summary>
		/// Stop the emulation
		/// </summary>
		/********************************************************************/
		public void Stop()
		{
			playerState = PlayerType.Stopped;
		}
		#endregion

		#region Memory methods
		/********************************************************************/
		/// <summary>
		/// Temporary hack till real bank switching code added
		///
		/// Input: A 16-bit effective address
		/// Output: A default bank-select value for $01
		/// </summary>
		/********************************************************************/
		private byte IoMap(ushort addr)
		{
			if (info.Environment != Sid2Env.EnvPs)
			{
				// Force real C64 compatibility
				switch (tuneInfo.Compatibility)
				{
					case Compatibility.R64:
					case Compatibility.Basic:
					{
						// Special case, converted to 0x37 later
						return 0;
					}
				}

				if (addr == 0)
				{
					// Special case, converted to 0x37 later
					return 0;
				}

				if (addr < 0xa000)
				{
					// Basic-ROM, Kernel-ROM, I/O
					return 0x37;
				}

				if (addr < 0xd000)
				{
					// Kernel-ROM, I/O
					return 0x36;
				}

				if (addr >= 0xe000)
				{
					// I/O only
					return 0x35;
				}
			}

			// RAM only (special I/O in PlaySID mode)
			return 0x34;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void EvalBankSelect(byte data)
		{
			// Determine new memory configuration
			port.PrOut = data;
			port.PrIn = (byte)((data & port.Ddr) | (~port.Ddr & (port.PrIn | 0x17) & 0xdf));

			data |= (byte)~port.Ddr;
			data &= 7;

			isBasic = (data & 3) == 3;
			isIo = data > 4;
			isKernel = (data & 2) != 0;
			isChar = (data ^ 4) > 4;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private byte ReadMemByte_Plain(ushort addr)
		{
			// Bank select register value DOES NOT get to ram
			if (addr > 1)
				return ram[addr];

			if (addr != 0)
				return port.PrIn;

			return port.Ddr;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private byte ReadMemByte_Io(ushort addr)
		{
			ushort tempAddr = (ushort)(addr & 0xfc1f);

			// Not SID?
			if ((tempAddr & 0xff00) != 0xd400)
			{
				if (info.Environment == Sid2Env.EnvR)
				{
					switch (Endian.Endian16Hi8(addr))
					{
						case 0:
						case 1:
							return ReadMemByte_Plain(addr);

						case 0xdc:
							return cia.Read((byte)(addr & 0x0f));

						case 0xdd:
							return cia2.Read((byte)(addr & 0x0f));

						case 0xd0:
						case 0xd1:
						case 0xd2:
						case 0xd3:
							return vic.Read((byte)(addr & 0x3f));

						default:
							return rom[addr];
					}
				}
				else
				{
					switch (Endian.Endian16Hi8(addr))
					{
						case 0:
						case 1:
							return ReadMemByte_Plain(addr);

						// SidPlay1 Random Extension CIA
						case 0xdc:
							return sid6526.Read((byte)(addr & 0xf));

						// SidPlay1 Random Extension VIC
						case 0xd0:
						{
							switch (addr & 0x3f)
							{
								case 0x11:
								case 0x12:
									return sid6526.Read((byte)((addr - 13) & 0x0f));
							}
							goto default;
						}

						default:
							return rom[addr];
					}
				}
			}

			// Read real sid for these
			int i = sidMapper[(addr >> 5) & (MapperSize - 1)];
			return sid[i].Obj.Read((byte)(tempAddr & 0xff));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private byte ReadMemByte_SidPlayTp(ushort addr)
		{
			if (addr < 0xd000)
				return ReadMemByte_Plain(addr);

			// Get high nibble address
			switch (addr >> 12)
			{
				case 0xd:
				{
					if (isIo)
						return ReadMemByte_Io(addr);

					return ram[addr];
				}

				case 0xe:
				case 0xf:
				default:
				{
					return ram[addr];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private byte ReadMemByte_SidPlayBs(ushort addr)
		{
			if (addr < 0xa000)
				return ReadMemByte_Plain(addr);

			// Get high nibble address
			switch (addr >> 12)
			{
				case 0xa:
				case 0xb:
				{
					if (isBasic)
						return rom[addr];

					return ram[addr];
				}

				case 0xc:
					return ram[addr];

				case 0xd:
				{
					if (isIo)
						return ReadMemByte_Io(addr);

					// Internal relocated to free ROM
					if (isChar)
						return rom[addr & 0x4fff];

					return ram[addr];
				}

				case 0xe:
				case 0xf:
				default:
				{
					if (isKernel)
						return rom[addr];

					return ram[addr];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void WriteMemByte_Plain(ushort addr, byte data)
		{
			if (addr > 1)
				ram[addr] = data;
			else if (addr != 0)
			{
				// Determine new memory configuration
				EvalBankSelect(data);
			}
			else
			{
				port.Ddr = data;
				EvalBankSelect(port.PrOut);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void WriteMemByte_PlaySid(ushort addr, byte data)
		{
			ushort tempAddr = (ushort)(addr & 0xfc1f);

			// Not SID?
			if ((tempAddr & 0xff00) != 0xd400)
			{
				if (info.Environment == Sid2Env.EnvR)
				{
					switch (Endian.Endian16Hi8(addr))
					{
						case 0:
						case 1:
						{
							WriteMemByte_Plain(addr, data);
							return;
						}

						case 0xdc:
						{
							cia.Write((byte)(addr & 0x0f), data);
							return;
						}

						case 0xdd:
						{
							cia2.Write((byte)(addr & 0x0f), data);
							return;
						}

						case 0xd0:
						case 0xd1:
						case 0xd2:
						case 0xd3:
						{
							vic.Write((byte)(addr & 0x3f), data);
							return;
						}

						default:
						{
							rom[addr] = data;
							return;
						}
					}
				}
				else
				{
					switch (Endian.Endian16Hi8(addr))
					{
						case 0:
						case 1:
						{
							WriteMemByte_Plain(addr, data);
							return;
						}

						// SidPlay1 CIA
						case 0xdc:
						{
							sid6526.Write((byte)(addr & 0xf), data);
							return;
						}

						default:
						{
							rom[addr] = data;
							return;
						}
					}
				}
			}

			// $D41D/1E/1F, $D43D/3E/3F, ...
			// Map to real address to support PlaySID
			// Extended SID chip registers
			Sid2Crc(data);

			if ((tempAddr & 0x00ff) >= 0x001d)
				xsid.Write16((ushort)(addr & 0x01ff), data);
			else
			{
				// Mirrored SID
				int i = sidMapper[(addr >> 5) & (MapperSize - 1)];

				// Convert address to that acceptable by resid
				sid[i].Obj.Write((byte)(tempAddr & 0xff), data);

				// Support dual sid
				if (emulateStereo)
					sid[1].Obj.Write((byte)(tempAddr & 0xff), data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void WriteMemByte_SidPlay(ushort addr, byte data)
		{
			if (addr < 0xa000)
				WriteMemByte_Plain(addr, data);
			else
			{
				// Get high nibble of address
				switch (addr >> 12)
				{
					case 0xa:
					case 0xb:
					case 0xc:
					{
						ram[addr] = data;
						break;
					}

					case 0xd:
					{
						if (isIo)
							WriteMemByte_PlaySid(addr, data);
						else
							ram[addr] = data;

						break;
					}

					case 0xe:
					case 0xf:
					default:
					{
						ram[addr] = data;
						break;
					}
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Initialize()
		{
			// Fix the mileage counter if just finished another song
			MileageCorrect();
			mileage += Time();

			Reset();

			{
				uint page = (tuneInfo.LoadAddr + tuneInfo.C64DataLen - 1) >> 8;
				if (page > 0xff)
					throw new Exception(Resources.IDS_SID_ERR_TOO_BIG);
			}

			PSidDrvReloc(tuneInfo, info);

			tune.PlaceSidTuneInC64Mem(ram);

			PSidDrvInstall(info);

			rtc.Reset();
			EnvReset(false);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MileageCorrect()
		{
			// Calculate 1 bit below the time base, so we can round the
			// mileage count
			if ((((sampleCount * 2 * TimeBas) / config.Frequency) & 1) != 0)
				mileage++;

			sampleCount = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Reset()
		{
			running = -1;	// Pending stop
			playerState = PlayerType.Stopped;
			sid2Crc = 0xffffffff;
			info.Sid2Crc = sid2Crc ^ 0xffffffff;
			sid2CrcCount = info.Sid2CrcCount = 0;

			// Select SidPlay1 compatible CPU or real thing
			cpu.Environment(info.Environment);

			scheduler.Reset();

			for (int i = 0; i < MaxSids; i++)
			{
				SidIPtr<ISidEmulation> s = new SidIPtr<ISidEmulation>(sid[i]);
				s.Obj.Reset(0x0f);

				// Synchronize the wave form generators
				// (must occur after reset)
				s.Obj.Write(0x04, 0x08);
				s.Obj.Write(0x0b, 0x08);
				s.Obj.Write(0x12, 0x08);
				s.Obj.Write(0x04, 0x00);
				s.Obj.Write(0x0b, 0x00);
				s.Obj.Write(0x12, 0x00);
			}

			// Must be after SID writes to ensure asynchronous state
			// queries do not return we have completed. This can be
			// problematic when reseting things real hardware e.g.
			// HardSID and fixes issues supporting the Acid64 API
			running = 0;

			if (info.Environment == Sid2Env.EnvR)
			{
				cia.Reset();
				cia2.Reset();
				vic.Reset();
			}
			else
			{
				sid6526.Reset(config.PowerOnDelay <= MaxPowerOnDelay);
				sid6526.Write(0x0e, 1);		// Start timer

				if (tuneInfo.SongSpeed == Speed.Vbi)
					sid6526.Lock();
			}

			// Initialize memory
			port.PrIn = 0;
			Array.Clear(ram, 0, 0x10000);

			switch (info.Environment)
			{
				case Sid2Env.EnvPs:
					break;

				case Sid2Env.EnvR:
				{
					// Initialize RAM with powerup pattern
					for (int i = 0x07c0; i < 0x10000; i += 128)
						Array.Fill<byte>(ram, 0xff, i, 64);

					Array.Clear(rom, 0, 0x10000);
					break;
				}

				default:
				{
					Array.Clear(rom, 0, 0x10000);
					Array.Fill(rom, Opcodes.RTSn, 0xa000, 0x2000);
					break;
				}
			}

			if (info.Environment == Sid2Env.EnvR)
			{
				Array.Copy(Kernel.Data, 0, rom, 0xe000, Kernel.Data.Length);

				// ROM should be at 0xd000 but have internally relocated
				// it here to unused ROM space (does not effect C64 progs)
				Array.Copy(Character.Data, 0, rom, 0x4000, Character.Data.Length);

				rom[0xfd69] = 0x9f;		// Bypass memory check
				rom[0xe55f] = 0x00;		// Bypass screen clear
				rom[0xfdc4] = 0xea;		// Ignore sid volume reset to avoid DC
				rom[0xfdc5] = 0xea;		// click (potentially incompatibility)!!
				rom[0xfdc6] = 0xea;

				if (tuneInfo.Compatibility == Compatibility.Basic)
					Array.Copy(Basic.Data, 0, rom, 0xa000, Basic.Data.Length);

				// Copy in power on settings. These were created by running
				// the kernel reset routine and storing the useful values
				// from $0000-$03ff. Format is:
				//
				// -offset byte (bit 7 indicates presence rle byte)
				// -rle count byte (bit 7 indicates compression used)
				// data (single byte) or quantity represented by uncompressed count
				// -all counts and offsets are 1 less than they should be
				{
					ushort addr = 0;

					for (int i = 0; i < powerOn.Length; )
					{
						byte off = powerOn[i++];
						byte count = 0;
						bool compressed = false;

						// Determine data count/compression
						if ((off & 0x80) != 0)
						{
							// Fixup offset
							off &= 0x7f;
							count = powerOn[i++];

							if ((count & 0x80) != 0)
							{
								// Fixup count
								count &= 0x7f;
								compressed = true;
							}
						}

						// Fix count off by ones (see format details)
						count++;
						addr += off;

						if (compressed)
						{
							// Extract compressed data
							byte data = powerOn[i++];

							while (count-- > 0)
								ram[addr++] = data;
						}
						else
						{
							// Extract uncompressed data
							while (count-- > 0)
								ram[addr++] = powerOn[i++];
						}
					}
				}
			}
			else
			{
				Array.Fill<byte>(rom, Opcodes.RTSn, 0xe000, 0x2000);

				// Fake VBI-interrupts that do $d019, BMI ...
				rom[0xd019] = 0xff;

				if (info.Environment == Sid2Env.EnvPs)
				{
					ram[0xff48] = Opcodes.JMPi;
					Endian.EndianLittle16(ram, 0xff49, 0x0314);
				}

				// Software vectors
				Endian.EndianLittle16(ram, 0x0314, 0xea31);		// IRQ
				Endian.EndianLittle16(ram, 0x0316, 0xfe66);		// BRK
				Endian.EndianLittle16(ram, 0x0318, 0xfe47);		// NMI

				// Hardware vectors
				if (info.Environment == Sid2Env.EnvPs)
					Endian.EndianLittle16(ram, 0xfffa, 0xfffa);	// NMI
				else
					Endian.EndianLittle16(ram, 0xfffa, 0xfe43);	// NMI

				Endian.EndianLittle16(ram, 0xfffc, 0xfce2);		// Reset
				Endian.EndianLittle16(ram, 0xfffe, 0xff48);		// IRQ

				Array.Copy(rom, 0xfffa, ram, 0xfffa, 6);
			}

			// Will get done later if can't now
			if (tuneInfo.ClockSpeed == Clock.Pal)
				ram[0x02a6] = 1;
			else
				ram[0x02a6] = 0;
		}



		/********************************************************************/
		/// <summary>
		/// This resets the CPU once the program is loaded to begin running.
		/// Also called when the emulation crashes
		/// </summary>
		/********************************************************************/
		private void EnvReset(bool safe)
		{
			if (safe)
			{
				// Emulation crashed so run in safe mode
				if (info.Environment == Sid2Env.EnvR)
				{
					byte[] prg = { Opcodes.LDAb, 0x7f, Opcodes.STAa, 0x0d, 0xdc, Opcodes.RTSn };

					// Install driver
					tuneInfo.RelocStartPage = 0x09;
					tuneInfo.RelocPages = 0x20;
					tuneInfo.InitAddr = 0x0800;
					tuneInfo.SongSpeed = Speed.Cia_1A;

					Sid2Info inf = new Sid2Info();
					inf.Environment = info.Environment;

					PSidDrvReloc(tuneInfo, inf);

					// Install prg & driver
					Array.Copy(prg, 0, ram, 0x0800, prg.Length);
					PSidDrvInstall(inf);
				}
				else
				{
					// If there no irqs, song wont continue
					sid6526.Reset();
				}

				// Make sids silent
				for (int i = 0; i < MaxSids; i++)
					sid[i].Obj.Reset(0);
			}

			port.Ddr = 0x2f;

			// Defaults: Basic-ROM on, Kernel-ROM on, I/O on
			if (info.Environment != Sid2Env.EnvR)
			{
				byte song = (byte)(tuneInfo.CurrentSong - 1);
				byte bank = IoMap(tuneInfo.InitAddr);
				EvalBankSelect(bank);

				playBank = IoMap(tuneInfo.PlayAddr);

				if (info.Environment != Sid2Env.EnvPs)
					cpu.Reset(tuneInfo.InitAddr, song, 0, 0);
				else
					cpu.Reset(tuneInfo.InitAddr, song, song, song);
			}
			else
			{
				EvalBankSelect(0x37);
				cpu.Reset();
			}

			MixerReset();
			xsid.Suppress(true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool EnvCheckBankJump(ushort addr)
		{
			switch (info.Environment)
			{
				case Sid2Env.EnvBs:
				{
					if (addr >= 0xa000)
					{
						// Get high nibble of address
						switch (addr >> 12)
						{
							case 0xa:
							case 0xb:
							{
								if (isBasic)
									return false;

								break;
							}

							case 0xc:
								break;

							case 0xd:
							{
								if (isIo)
									return false;

								break;
							}

							case 0xe:
							case 0xf:
							{
								if (isKernel)
									return false;

								break;
							}
						}
					}
					break;
				}

				case Sid2Env.EnvTp:
				{
					if ((addr >= 0xd000) && isKernel)
						return false;

					break;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void EnvSleep()
		{
			if (info.Environment != Sid2Env.EnvR)
			{
				// Start the sample sequence
				xsid.Suppress(false);
				xsid.Suppress(true);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Makes the next sequence of notes available. For SidPlay
		/// compatibility this function should be called from interrupt event
		/// </summary>
		/********************************************************************/
		private void FakeIrq()
		{
			// Check to see if the play address has been provided or whether
			// we should pick it up from an IRQ vector
			ushort playAddr = tuneInfo.PlayAddr;

			// We have to reload the new play address
			if (playAddr != 0)
				EvalBankSelect(playBank);
			else
			{
				if (isKernel)
				{
					// Setup the entry point from hardware IRQ
					playAddr = Endian.EndianLittle16(ram, 0x0314);
				}
				else
				{
					// Setup the entry point from software IRQ
					playAddr = Endian.EndianLittle16(ram, 0xfffe);
				}
			}

			// Setup the entry point and restart the cpu
			cpu.TriggerIrq();
			cpu.Reset(playAddr, 0, 0, 0);
		}
		#endregion
	}
}
