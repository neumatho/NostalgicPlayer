/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Sound;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Represents a single voice in a TFMX audio playback system
	/// </summary>
	internal class TfmxVoice : PaulaVoice
	{
		private readonly PaulaChannel paulaChannel;
		private ushort loopCount;
		private bool isOn;
		private bool retrig;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TfmxVoice(IChannel channel)
		{
			paulaChannel = new PaulaChannel(channel);
			channel.Interrupt = HandleInterrupt;
			isOn = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void On()
		{
			retrig = true;
			TakeNextBuf();
			retrig = false;

			paulaChannel.SetDma(true);
			isOn = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Off()
		{
			paulaChannel.SetDma(false);

			isOn = false;
			Paula.Period = 0;
			Paula.Volume = 0;
			loopCount = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Take parameters from paula.* (or just repeat.*)
		/// </summary>
		/********************************************************************/
		public override void TakeNextBuf()
		{
			if (!isOn)
				loopCount = 0;

			ReadOnlyMemory<byte> sampleBuffer = Paula.Start.AsMemory();

			if (MemoryMarshal.TryGetArray(sampleBuffer, out ArraySegment<byte> segment))
			{
				ushort length = Paula.Length;
				if (length == 0)
					length = 1;

				paulaChannel.SetAddress(0, segment.Array, (uint)segment.Offset, retrig);
				paulaChannel.Length = length;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override ushort GetLoopCount()
		{
			return loopCount;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void UpdateRegisters()
		{
			paulaChannel.Period = Paula.Period;
			paulaChannel.Volume = Paula.Volume > 64 ? (ushort)64 : Paula.Volume;
		}



		/********************************************************************/
		/// <summary>
		/// This is automatic called when the current sample has been played
		/// </summary>
		/********************************************************************/
		private InterruptResult HandleInterrupt()
		{
			loopCount++;

			return paulaChannel.Interrupt();
		}
	}
}
