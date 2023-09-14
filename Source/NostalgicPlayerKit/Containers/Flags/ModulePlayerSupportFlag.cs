/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Flags
{
	/// <summary>
	/// Different flags indicating what a module player supports
	/// </summary>
	[Flags]
	public enum ModulePlayerSupportFlag
	{
		/// <summary>
		/// Nothing
		/// </summary>
		None = 0,

		/// <summary>
		/// Set this if your player can change to a certain position. You also
		/// need to implement SetSongPosition() in the IDurationPlayer interface
		/// </summary>
		SetPosition = 0x0001,

		/// <summary>
		/// If this flag is set, the player is switched to buffer mode,
		/// which means your Play() method will only be called, when a
		/// new buffer needs to be set
		/// </summary>
		BufferMode = 0x1000,

		/// <summary>
		/// If this flag is set together with BufferMode, the buffer mode
		/// goes into direct mode.
		///
		/// That means, your output goes around NostalgicPlayer mixer, so the
		/// output samples need to be in the right frequency and either in
		/// mono or stereo. You will get that information via the SetOutputFormat()
		/// method in your player
		/// </summary>
		BufferDirect = 0x2000,

		/// <summary>
		/// This flag can only be set together with BufferDirect. It indicate, that
		/// you have your own enable/disable channel implementation
		/// </summary>
		EnableChannels = 0x4000
	}
}
