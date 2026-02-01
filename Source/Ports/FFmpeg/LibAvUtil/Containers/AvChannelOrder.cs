/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvChannelOrder
	{
		/// <summary>
		/// Only the channel count is specified, without any further information
		/// about the channel order
		/// </summary>
		Unspec,

		/// <summary>
		/// The native channel order, i.e. the channels are in the same order in
		/// which they are defined in the AVChannel enum. This supports up to 63
		/// different channels
		/// </summary>
		Native,

		/// <summary>
		/// The channel order does not correspond to any other predefined order and
		/// is stored as an explicit map. For example, this could be used to support
		/// layouts with 64 or more channels, or with empty/skipped (AV_CHAN_UNUSED)
		/// channels at arbitrary positions
		/// </summary>
		Custom,

		/// <summary>
		/// The audio is represented as the decomposition of the sound field into
		/// spherical harmonics. Each channel corresponds to a single expansion
		/// component. Channels are ordered according to ACN (Ambisonic Channel
		/// Number).
		///
		/// The channel with the index n in the stream contains the spherical
		/// harmonic of degree l and order m given by
		/// 
		///   l   = floor(sqrt(n)),
		///   m   = n - l * (l + 1).
		///
		/// Conversely given a spherical harmonic of degree l and order m, the
		/// corresponding channel index n is given by
		///
		///   n = l * (l + 1) + m.
		/// 
		/// Normalization is assumed to be SN3D (Schmidt Semi-Normalization)
		/// as defined in AmbiX format $ 2.1
		/// </summary>
		Ambisonic,

		/// <summary>
		/// Number of channel orders, not part of ABI/API
		/// </summary>
		Nb
	}
}
