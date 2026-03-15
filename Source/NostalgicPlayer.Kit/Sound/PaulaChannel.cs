/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Sound
{
	/// <summary>
	/// Make the same interface as the Amiga Paula sound chip
	/// around an IChannel
	/// </summary>
	public class PaulaChannel
	{
		private readonly IChannel channel;

		private bool dmaState;

		private short number;
		private sbyte[] sample;
		private uint offset;
		private bool retrig;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PaulaChannel(IChannel channel)
		{
			this.channel = channel;
			channel.Interrupt = Interrupt;

			dmaState = false;
		}



		/********************************************************************/
		/// <summary>
		/// Change the sample address
		/// </summary>
		/********************************************************************/
		public void SetAddress(short sampleNumber, sbyte[] sampleData, uint startOffset, bool retrigSample)
		{
			number = sampleNumber;
			sample = sampleData;
			offset = startOffset;
			retrig = retrigSample;
		}



		/********************************************************************/
		/// <summary>
		/// Set the length of the sample in words
		/// </summary>
		/********************************************************************/
		public ushort Length { private get; set; }



		/********************************************************************/
		/// <summary>
		/// Set the period
		/// </summary>
		/********************************************************************/
		public ushort Period
		{
			set => channel.SetAmigaPeriod(value);
		}



		/********************************************************************/
		/// <summary>
		/// Set the volume
		/// </summary>
		/********************************************************************/
		public ushort Volume
		{
			set => channel.SetAmigaVolume(value);
		}



		/********************************************************************/
		/// <summary>
		/// Set the DMA flag
		/// </summary>
		/********************************************************************/
		public void SetDma(bool enabled)
		{
			if (enabled)
			{
				if (!dmaState || !channel.IsActive)
				{
					channel.PlaySample(number, sample, offset, Length * 2U);
					channel.SetLoop(offset, Length * 2U);
				}
			}
			else
			{
				if (dmaState)
					channel.Mute();
			}

			dmaState = enabled;
		}



		/********************************************************************/
		/// <summary>
		/// This is automatic called when the current sample has been played.
		/// 
		/// If your player require its own audio interrupt, call this at the
		/// end of your method
		/// </summary>
		/********************************************************************/
		public InterruptResult Interrupt()
		{
			if ((number != -1) && retrig)
			{
				channel.SetSampleNumber(number);
				channel.SetLoop(sample, offset, Length * 2U);
			}

			return new InterruptResult
			{
				NewSampleAddress = sample,
				StartOffset = offset,
				Length = Length * 2U
			};
		}
	}
}
