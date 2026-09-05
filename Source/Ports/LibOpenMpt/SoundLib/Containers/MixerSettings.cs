/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// A struct containing settings for the mixer of soundlib
	/// </summary>
	internal struct MixerSettings
	{
		public const int32 StereoSeparationScale = 128;

		public int32 m_nStereoSeparation;

		public uint32 m_nMaxMixChannels;
		public DspFlags DSPMask;
		public MixerFlags MixerFlags;
		public uint32 gdwMixingFreq;
		public uint32 gnChannels;
		public uint32 m_nPreAmp;
		public size_t NumInputChannels;

		public int32 VolumeRampUpMicroseconds;
		public int32 VolumeRampDownMicroseconds;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MixerSettings()
		{
			// SNDMIX: These are global flags for playback control
			m_nStereoSeparation = 128;
			m_nMaxMixChannels = Snd_Def.Max_Channels;

			DSPMask = DspFlags.None;
			MixerFlags = MixerFlags.None;

			// Mixing configuration
			gnChannels = 2;
			gdwMixingFreq = 48000;

			m_nPreAmp = 128;

			VolumeRampUpMicroseconds = 363;		// 16 @44100
			VolumeRampDownMicroseconds = 952;	// 42 @44100

			NumInputChannels = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int32_t GetVolumeRampUpMicroseconds()
		{
			return VolumeRampUpMicroseconds;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int32_t GetVolumeRampDownMicroseconds()
		{
			return VolumeRampDownMicroseconds;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetVolumeRampUpMicroseconds(int32 rampUpMicroseconds)
		{
			VolumeRampUpMicroseconds = rampUpMicroseconds;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetVolumeRampDownMicroseconds(int32 rampDownMicroseconds)
		{
			VolumeRampDownMicroseconds = rampDownMicroseconds;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32 GetVolumeRampUpSamples()
		{
			return Util.MulDivR(VolumeRampUpMicroseconds, (int32)gdwMixingFreq, 1000000);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32 GetVolumeRampDownSamples()
		{
			return Util.MulDivR(VolumeRampDownMicroseconds, (int32)gdwMixingFreq, 1000000);
		}
	}
}
