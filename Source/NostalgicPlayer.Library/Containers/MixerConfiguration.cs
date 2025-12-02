/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Library.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Containers
{
	/// <summary>
	/// Different configuration settings for the mixer
	/// </summary>
	public class MixerConfiguration
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MixerConfiguration()
		{
			ChannelsEnabled = new bool[IChannel.MaxNumberOfChannels];
			Array.Fill(ChannelsEnabled, true);

			// Initialize equalizer with flat response (0 dB on all bands)
			EqualizerBands = new double[10];
			Array.Fill(EqualizerBands, 0.0);

			// Initialize pre-amp with 0 dB (no attenuation/boost)
			EqualizerPreAmp = 0.0;
		}



		/********************************************************************/
		/// <summary>
		/// The stereo separation in percent
		/// </summary>
		/********************************************************************/
		public int StereoSeparator
		{
			get; set;
		} = 100;



		/********************************************************************/
		/// <summary>
		/// The visuals latency in milliseconds
		/// </summary>
		/********************************************************************/
		public int VisualsLatency
		{
			get; set;
		} = 0;



		/********************************************************************/
		/// <summary>
		/// Indicate if interpolation is enabled
		/// </summary>
		/********************************************************************/
		public InterpolationMode InterpolationMode
		{
			get; set;
		} = InterpolationMode.None;



		/********************************************************************/
		/// <summary>
		/// Swap left and right speakers
		/// </summary>
		/********************************************************************/
		public bool SwapSpeakers
		{
			get; set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Indicate if Amiga filter should be emulated
		/// </summary>
		/********************************************************************/
		public bool EnableAmigaFilter
		{
			get; set;
		} = true;



		/********************************************************************/
		/// <summary>
		/// Indicate if equalizer is enabled
		/// </summary>
		/********************************************************************/
		public bool EnableEqualizer
		{
			get; set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Equalizer band gains in dB (-12 to +12 dB)
		/// 10 bands: 60, 170, 310, 600, 1k, 3k, 6k, 12k, 14k, 16k Hz
		/// </summary>
		/********************************************************************/
		public double[] EqualizerBands
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Equalizer pre-amp gain in dB (-12 to +12 dB)
		/// </summary>
		/********************************************************************/
		public double EqualizerPreAmp
		{
			get; set;
		} = 0.0;



		/********************************************************************/
		/// <summary>
		/// Holds an array telling which channels to enable/disable. If null,
		/// all channels are enabled
		/// </summary>
		/********************************************************************/
		public bool[] ChannelsEnabled
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// An instance that implements extra channels to the mixer
		/// </summary>
		/********************************************************************/
		public IExtraChannels ExtraChannels
		{
			get; set;
		} = null;
	}
}
