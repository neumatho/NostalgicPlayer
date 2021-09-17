/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.XSid
{
	internal partial class XSid
	{
		/// <summary>
		/// 
		/// </summary>
		private class Channel
		{
			private enum FmMode
			{
				None = 0,
				Huels,
				Galway
			}

			private enum SampleOrder : byte
			{
				LowHigh = 0,
				HighLow = 1
			}

			private string name;
			private IEventContext context;
			private EventPhase phase;
			private XSid xsid;
			private Event.Event xsidEvent;

			private EventCallback sampleEvent;
			private EventCallback galwayEvent;

			private byte[] reg;
			private FmMode mode;
			internal bool active;
			private ushort address;
			private ushort cycleCount;			// Counts to zero and triggers!
			private byte volShift;
			private byte sampleLimit;
			private sbyte sample;

			// Sample section
			private byte samRepeat;
			private byte samScale;
			private SampleOrder samOrder;
			private byte samNibble;
			private ushort samEndAddr;
			private ushort samRepeatAddr;
			private ushort samPeriod;

			// Galway section
			private byte galTones;
			private byte galInitLength;
			private byte galLength;
			private byte galVolume;
			private byte galLoopWait;
			private byte galNullWait;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Channel(string name, IEventContext context, XSid xsid, Event.Event xsidEvent)
			{
				this.name = name;
				this.context = context;
				phase = EventPhase.ClockPhi1;
				this.xsid = xsid;
				this.xsidEvent = xsidEvent;
				sampleEvent = new EventCallback("xSID Sample", SampleClock);
				galwayEvent = new EventCallback("xSID Galway", GalwayClock);

				Reset();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public byte Limit()
			{
				return sampleLimit;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool IsGalway()
			{
				return mode == FmMode.Galway;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public sbyte Output()
			{
				return sample;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Reset()
			{
				reg = new byte[0x10];
				galVolume = 0;			// This is left to free run until reset
				mode = FmMode.None;
				Free();

				// Remove outstanding events
				xsidEvent.Cancel();
				sampleEvent.Cancel();
				galwayEvent.Cancel();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void CheckForInit()
			{
				// Check to see mode of operation
				// See xsid documentation
				switch (reg[ConvertAddr(0x1d)])
				{
					case 0xff:
					case 0xfe:
					case 0xfc:
					{
						SampleInit();
						break;
					}

					case 0xfd:
					{
						if (!active)
							return;

						Free();		// Stop

						// Calculate the sample offset
						xsid.SampleOffsetCalc();
						break;
					}

					case 0x00:
						break;

					default:
					{
						GalwayInit();
						break;
					}
				}
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Write(byte addr, byte data)
			{
				reg[ConvertAddr(addr)] = data;
			}

			#region Sample
			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private void SampleInit()
			{
				if (active && (mode == FmMode.Galway))
					return;

				// Check all important parameters are legal
				byte r = ConvertAddr(0x1d);
				volShift = (byte)((0 - reg[r]) >> 1);
				reg[r] = 0;

				// Use endian 16 as can't g
				r = ConvertAddr(0x1e);
				address = Endian.Endian16(reg[r + 1], reg[r]);

				r = ConvertAddr(0x3d);
				samEndAddr = Endian.Endian16(reg[r + 1], reg[r]);

				if (samEndAddr <= address)
					return;

				samScale = reg[ConvertAddr(0x5f)];

				r = ConvertAddr(0x5d);
				samPeriod = (ushort)(Endian.Endian16(reg[r + 1], reg[r]) >> samScale);

				if (samPeriod == 0)
				{
					// Stop this channel
					reg[ConvertAddr(0x1d)] = 0xfd;
					CheckForInit();
					return;
				}

				// Load the other parameters
				samNibble = 0;
				samRepeat = reg[ConvertAddr(0x3f)];
				samOrder = (SampleOrder)reg[ConvertAddr(0x7d)];

				r = ConvertAddr(0x7e);
				samRepeatAddr = Endian.Endian16(reg[r + 1], reg[r]);

				cycleCount = samPeriod;

				// Support Galway samples, but that
				// mode it setup only when as Galway
				// Noise sequence begins
				if (mode == FmMode.None)
					mode = FmMode.Huels;

				active = true;

				sampleLimit = (byte)(8 >> volShift);
				sample = SampleCalculate();

				// Calculate the sample offset
				xsid.SampleOffsetCalc();

				// Schedule a sample update
				if (xsidEvent.Pending())
					xsidEvent.Schedule(context, 0, phase);

				sampleEvent.Schedule(context, cycleCount, phase);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private void SampleClock()
			{
				cycleCount = samPeriod;
				if (address >= samEndAddr)
				{
					if (samRepeat != 0xff)
					{
						if (samRepeat != 0)
							samRepeat--;
						else
							samRepeatAddr = address;
					}

					address = samRepeatAddr;
					if (address >= samEndAddr)
					{
						// The sequence has completed
						ref byte status = ref reg[ConvertAddr(0x1d)];

						if (status == 0)
							status = 0xfd;

						if (status != 0xfd)
							active = false;

						CheckForInit();
						return;
					}
				}

				// We have reached the required sample.
				// So now we need to extract the right nibble
				sample = SampleCalculate();

				// Schedule a sample update
				if (!xsidEvent.Pending())
					xsidEvent.Schedule(context, 0, phase);

				sampleEvent.Schedule(context, cycleCount, phase);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private sbyte SampleCalculate()
			{
				byte tempSample = xsid.ReadMemByte(address);
				if (samOrder == SampleOrder.LowHigh)
				{
					if (samScale == 0)
					{
						if (samNibble != 0)
							tempSample >>= 4;
					}

					// AND 15 further below
				}
				else
				{
					if (samScale == 0)
					{
						if (samNibble == 0)
							tempSample >>= 4;
					}
					else
						tempSample >>= 4;

					// AND 15 further below
				}

				// Move to next address
				address += samNibble;
				samNibble ^= 1;

				return (sbyte)(((tempSample & 0x0f) - 0x08) >> volShift);
			}
			#endregion

			#region Galway
			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private void GalwayInit()
			{
				if (active)
					return;

				// Check all important parameters are legal
				byte r = ConvertAddr(0x1d);
				galTones = reg[r];
				reg[r] = 0;

				galInitLength = reg[ConvertAddr(0x3d)];
				if (galInitLength == 0)
					return;

				galLoopWait = reg[ConvertAddr(0x3f)];
				if (galLoopWait == 0)
					return;

				galNullWait = reg[ConvertAddr(0x5d)];
				if (galNullWait == 0)
					return;

				// Load other parameters
				r = ConvertAddr(0x1e);
				address = Endian.Endian16(reg[r + 1], reg[r]);

				volShift = (byte)(reg[ConvertAddr(0x3e)] & 0x0f);
				mode = FmMode.Galway;
				active = true;

				sampleLimit = 8;
				sample = (sbyte)(galVolume - 8);
				GalwayTonePeriod();

				// Calculate sample offset
				xsid.SampleOffsetCalc();

				// Schedule a sample update
				if (!xsidEvent.Pending())
					xsidEvent.Schedule(context, 0, phase);

				galwayEvent.Schedule(context, cycleCount, phase);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private void GalwayClock()
			{
				if (--galLength != 0)
					cycleCount = samPeriod;
				else if (galTones == 0xff)
				{
					// The sequence has completed
					ref byte status = ref reg[ConvertAddr(0x1d)];

					if (status == 0)
						status = 0xfd;

					if (status != 0xfd)
						active = false;

					CheckForInit();
					return;
				}
				else
					GalwayTonePeriod();

				// See Galway example...
				galVolume += volShift;
				galVolume &= 0x0f;
				sample = (sbyte)(galVolume - 8);

				if (!xsidEvent.Pending())
					xsidEvent.Schedule(context, 0, phase);

				galwayEvent.Schedule(context, cycleCount, phase);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private void GalwayTonePeriod()
			{
				// Calculate the number of cycles over which sample should last
				galLength = galInitLength;
				samPeriod = xsid.ReadMemByte((ushort)(address + galTones));
				samPeriod *= galLoopWait;
				samPeriod += galNullWait;
				cycleCount = samPeriod;

				galTones--;
			}
			#endregion

			#region Private methods
			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private void Free()
			{
				active = false;
				cycleCount = 0;
				sampleLimit = 0;

				// Set XSID to stopped state
				reg[ConvertAddr(0x1d)] = 0;
				Silence();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private void Silence()
			{
				sample = 0;
				sampleEvent.Cancel();
				galwayEvent.Cancel();
				xsidEvent.Schedule(context, 0, phase);
			}



			/********************************************************************/
			/// <summary>
			/// Compress address to not leave so many spaces
			/// </summary>
			/********************************************************************/
			private byte ConvertAddr(byte addr)
			{
				return (byte)((addr & 0x3) | (addr >> 3) & 0x0c);
			}
			#endregion
		}
	}
}
