/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using RowIndex = System.UInt32;
global using ChannelIndex = System.UInt16;
global using OrderIndex = System.UInt16;
global using PatternIndex = System.UInt16;
global using PlugIndex = System.Byte;
global using SampleIndex = System.UInt16;
global using InstrumentIndex = System.UInt16;
global using SequenceIndex = System.Byte;
global using SmpLength = System.UInt32;
global using samplecount_t = System.UInt32;		// Number of rendered samples
global using PlugParamIndex = System.UInt32;
global using PlugParamValue = System.Single;

using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal class Snd_Def
	{
		public const RowIndex RowIndex_Invalid = uint32.MaxValue;
		public const ChannelIndex ChannelIndex_Invalid = uint16.MaxValue;
		public const OrderIndex OrderIndex_Invalid = uint16.MaxValue;
		public const OrderIndex OrderIndex_Max = uint16.MaxValue - 1;
		public const PatternIndex PatternIndex_Invalid = uint16.MaxValue;		// "---" in order list
		public const PatternIndex PatternIndex_Skip = uint16.MaxValue - 1;		// "+++" in order list
		public const SequenceIndex SequenceIndex_Invalid = uint8.MaxValue;

		/// <summary>
		/// Sample length in frames. Sample size in bytes can be more than this (= 256 MB)
		/// </summary>
		public const SmpLength Max_Sample_Length = 0x10000000;

		public const RowIndex Max_Rows_Per_Measure = 65536;
		public const RowIndex Max_Rows_Per_Beat = 65536;
		public const RowIndex Default_Rows_Per_Beat = 4;
		public const RowIndex Default_Rows_Per_Measure = 16;

		public const RowIndex Max_Pattern_Rows = 4096;
		public const OrderIndex Max_Orders = OrderIndex_Max + 1;
		public const SampleIndex Max_Samples = 4000;
		public const InstrumentIndex Max_Instruments = 256;
		public const PlugIndex Max_MixPlugins = 250;

		public const SequenceIndex Max_Sequences = 50;

		/// <summary>
		/// Maximum pattern channels
		/// </summary>
		public const ChannelIndex Max_BaseChannels = 192;

		// Envelope value boundaries

		/// <summary>
		/// Vertical max value of a point
		/// </summary>
		public const uint8 Envelope_Max = 64;

		/// <summary>
		/// Maximum length of each instrument envelope
		/// </summary>
		public const uint8 Max_EnvPoints = 240;

		public const uint32 Max_Global_Volume = 256;
		public const uint32 Max_PreAmp = 2000;

		/// <summary>
		/// Maximum number of mixing channels
		/// </summary>
		public const ChannelIndex Max_Channels = 256;

		/// <summary>
		/// Number of fractional bits in return value of CSoundFile::GetFreqFromPeriod()
		/// </summary>
		public const c_int Freq_FracBits = 4;

		// String lengths (including trailing null char)
		public const c_int Max_SampleName = 32;
		public const c_int Max_SampleFileName = 22;
		public const c_int Max_InstrumentName = 32;
		public const c_int Max_InstrumentFileName = 32;
		public const c_int Max_ChannelName = 20;

		public const ChannelFlags Chn_SampleFlags = ChannelFlags.Chn_16Bit | ChannelFlags.Chn_Loop | ChannelFlags.Chn_PingPongLoop | ChannelFlags.Chn_SustainLoop | ChannelFlags.Chn_PingPongSustain | ChannelFlags.Chn_Panning | ChannelFlags.Chn_Stereo | ChannelFlags.Chn_PingPongFlag | ChannelFlags.Chn_Reverse | ChannelFlags.Chn_Surround | ChannelFlags.Chn_Adlib;
		public const ChannelFlags Chn_ChannelFlags = ~Chn_SampleFlags | ChannelFlags.Chn_Surround;

		/// <summary>
		/// ModSample.uFlags is held in a 16-bit FlagSet in OpenMPT
		/// (FlagSet‹ChannelFlags, uint16›), which truncates on assignment,
		/// so it can never contain a flag above 0xffff. A C# enum has only
		/// one underlying type, so that window is applied where the sample
		/// flags are merged into the channel flags instead.
		///
		/// Note that this is deliberately wider than Chn_SampleFlags:
		/// Smp_Modified, Smp_KeepOnDisk and Smp_NoDefaultVolume are outside
		/// of it, and OpenMPT does merge those into the channel flags, where
		/// they alias Chn_Wrapped_Loop, Chn_AmigaFilter and Chn_Filter
		/// </summary>
		public const ChannelFlags Smp_FlagsMask = (ChannelFlags)0xffff;

		public const c_int Env_Release_Node_Unset = 0xff;
		public const c_int Not_Yet_Released = -1;

		public const uint8 MidiNoChannel = 0;
		public const uint8 MidiFirstChannel = 1;
		public const uint8 MidiLastChannel = 16;
		public const uint8 MidiMappedChannel = 17;
	}
}
