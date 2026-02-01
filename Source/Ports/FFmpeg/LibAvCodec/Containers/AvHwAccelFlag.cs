/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvHwAccelFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Hardware acceleration should be used for decoding even if the codec level
		/// used is unknown or higher than the maximum supported level reported by the
		/// hardware driver.
		///
		/// It's generally a good idea to pass this flag unless you have a specific
		/// reason not to, as hardware tends to under-report supported levels
		/// </summary>
		Ignore_Level = 1 << 0,

		/// <summary>
		/// Hardware acceleration can output YUV pixel formats with a different chroma
		/// sampling than 4:2:0 and/or other than 8 bits per component
		/// </summary>
		Allow_High_Depth = 1 << 1,

		/// <summary>
		/// Hardware acceleration should still be attempted for decoding when the
		/// codec profile does not match the reported capabilities of the hardware.
		///
		/// For example, this can be used to try to decode baseline profile H.264
		/// streams in hardware - it will often succeed, because many streams marked
		/// as baseline profile actually conform to constrained baseline profile.
		///
		/// Warning: If the stream is actually not supported then the behaviour is
		///          undefined, and may include returning entirely incorrect output
		///          while indicating success
		/// </summary>
		Allow_Profile_Mismatch = 1 << 2,

		/// <summary>
		/// Some hardware decoders (namely nvdec) can either output direct decoder
		/// surfaces, or make an on-device copy and return said copy.
		/// There is a hard limit on how many decoder surfaces there can be, and it
		/// cannot be accurately guessed ahead of time.
		/// For some processing chains, this can be okay, but others will run into the
		/// limit and in turn produce very confusing errors that require fine tuning of
		/// more or less obscure options by the user, or in extreme cases cannot be
		/// resolved at all without inserting an avfilter that forces a copy.
		///
		/// Thus, the hwaccel will by default make a copy for safety and resilience.
		/// If a users really wants to minimize the amount of copies, they can set this
		/// flag and ensure their processing chain does not exhaust the surface pool
		/// </summary>
		Unsafe_Output = 1 << 3,
	}
}
