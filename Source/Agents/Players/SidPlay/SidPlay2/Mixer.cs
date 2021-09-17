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

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2
{
	/// <summary>
	/// Sids mixer methods
	/// </summary>
	internal partial class Player
	{
		private const int VolumeMax = 255;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MixerReset()
		{
			// Fixed point 16.16
			sampleClock = samplePeriod & 0x0ffff;

			// Schedule next sample event
			mixerEvent.Schedule(myC64.Context, samplePeriod >> 16, EventPhase.ClockPhi1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Mixer()
		{
			sampleClock += samplePeriod;

			// Fixed point 16.16
			uint cycles = sampleClock >> 16;
			sampleClock &= 0x0ffff;
			sampleIndex += output(leftSampleBuffer, rightSampleBuffer, sampleIndex);

			// Schedule next sample event
			mixerEvent.Schedule(myC64.Context, cycles, EventPhase.ClockPhi1);

			// Filled buffer
			if (sampleIndex >= sampleCount)
				running = -1;
		}

		#region Generic sound output generation routines
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int MonoOutGenericLeftIn(byte bits)
		{
			return sid[0].Obj.Output(bits) * leftVolume / VolumeMax;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int MonoOutGenericStereoIn(byte bits)
		{
			// Convert to mono
			return ((sid[0].Obj.Output(bits) * leftVolume) + (sid[1].Obj.Output(bits) * rightVolume)) / (VolumeMax * 2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int MonoOutGenericRightIn(byte bits)
		{
			return sid[1].Obj.Output(bits) * rightVolume / VolumeMax;
		}
		#endregion

		#region 8 bit sound output generation routines
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint MonoOut8MonoIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			leftBuffer[offset] = (sbyte)(MonoOutGenericLeftIn(8) ^ 0x80);
			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint MonoOut8StereoIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			leftBuffer[offset] = (sbyte)(MonoOutGenericStereoIn(8) ^ 0x80);
			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint MonoOut8StereoRIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			leftBuffer[offset] = (sbyte)(MonoOutGenericRightIn(8) ^ 0x80);
			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint StereoOut8MonoIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			sbyte sample = (sbyte)(MonoOutGenericLeftIn(8) ^ 0x80);

			// Change from stereo pair to separate buffers
			leftBuffer[offset] = sample;
			rightBuffer[offset] = sample;

			return 1;

/*			buffer[offset] = sample;
			buffer[offset + 1] = sample;

			return 2;*/
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint StereoOut8StereoIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			// Change from stereo pair to separate buffers
			leftBuffer[offset] = (sbyte)(MonoOutGenericLeftIn(8) ^ 0x80);
			rightBuffer[offset] = (sbyte)(MonoOutGenericRightIn(8) ^ 0x80);

			return 1;

/*			buffer[offset] = (sbyte)(MonoOutGenericLeftIn(8) ^ 0x80);
			buffer[offset + 1] = (sbyte)(MonoOutGenericRightIn(8) ^ 0x80);

			return 2;*/
		}
		#endregion

		#region 16 bit sound output generation routines
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint MonoOut16MonoIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			Endian.Endian16(leftBuffer, offset, (ushort)(MonoOutGenericLeftIn(16)));
			return 2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint MonoOut16StereoIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			Endian.Endian16(leftBuffer, offset, (ushort)(MonoOutGenericStereoIn(16)));
			return 2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint MonoOut16StereoRIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			Endian.Endian16(leftBuffer, offset, (ushort)(MonoOutGenericRightIn(16)));
			return 2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint StereoOut16MonoIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			ushort sample = (ushort)MonoOutGenericLeftIn(16);

			// Change from stereo pair to separate buffers
			Endian.Endian16(leftBuffer, offset, sample);
			Endian.Endian16(rightBuffer, offset, sample);

			return 2;

/*			Endian.Endian16(buffer, offset, sample);
			Endian.Endian16(buffer, offset + 2, sample);

			return 4;*/
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint StereoOut16StereoIn(sbyte[] leftBuffer, sbyte[] rightBuffer, uint offset)
		{
			// Change from stereo pair to separate buffers
			Endian.Endian16(leftBuffer, offset, (ushort)(MonoOutGenericLeftIn(16)));
			Endian.Endian16(rightBuffer, offset, (ushort)(MonoOutGenericRightIn(16)));

			return 2;

/*			Endian.Endian16(buffer, offset, (ushort)(MonoOutGenericLeftIn(16)));
			Endian.Endian16(buffer, offset + 2, (ushort)(MonoOutGenericRightIn(16)));

			return 4;*/
		}
		#endregion
	}
}
