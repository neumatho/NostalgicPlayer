/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Different header flags
	/// </summary>
	[Flags]
	internal enum HeaderFlag
	{
		LinearSlides = 0x0001,
		IsS3M = 0x0002,
		S3MFastSlides = 0x0004,
		IsMTM = 0x0008,						// Actually this is simply "not XM". But if none of the S3M, MOD and IT flags are set, it's an MTM
		S3MAmigaLimits = 0x0010,

		// 0x20 and 0x40 have been used in old versions for things that can be inferred from the file format anyway.
		// The official UNMO3 ignores them
		Unused1 = 0x0020,
		Unused2 = 0x0040,

		IsMod = 0x0080,
		IsIT = 0x0100,
		InstrumentMode = 0x0200,
		ItCompactGxx = 0x0400,
		ItOldFx = 0x0800,
		ModPlugMode = 0x10000,
		Unknown = 0x20000,					// Always set (internal BASS flag to designate modules)
		ModVBlank = 0x80000,
		HasPlugins = 0x100000,
		ExtFilterRange = 0x200000
	}
}
