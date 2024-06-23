/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Different sample information flags
	/// </summary>
	[Flags]
	internal enum SampleInfoFlag : ushort
	{
		_16Bit = 0x01,
		Loop = 0x10,
		PingPongLoop = 0x20,
		Sustain = 0x100,
		SustainPingPong = 0x200,
		Stereo = 0x400,
		CompressionMpeg = 0x1000,				// MPEG 1.0 / 2.0 / 2.5 sample
		CompressionOgg = 0x1000 | 0x2000,		// Ogg sample
		SharedOgg = 0x1000 | 0x2000 | 0x4000,	// Ogg sample with shared vorbis header
		DeltaCompression = 0x2000,				// Deltas + compression
		DeltaPrediction = 0x4000,				// Delta prediction + compression
		OplInstrument = 0x8000,					// OPL patch data
		CompressionMask = 0x1000 | 0x2000 | 0x4000 | 0x8000
	}
}
