/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// An AVChannelLayout holds information about the channel layout of audio data.
	///
	/// A channel layout here is defined as a set of channels ordered in a specific
	/// way (unless the channel order is AV_CHANNEL_ORDER_UNSPEC, in which case an
	/// AVChannelLayout carries only the channel count).
	/// All orders may be treated as if they were AV_CHANNEL_ORDER_UNSPEC by
	/// ignoring everything but the channel count, as long as av_channel_layout_check()
	/// considers they are valid.
	///
	/// Unlike most structures in FFmpeg, sizeof(AVChannelLayout) is a part of the
	/// public ABI and may be used by the caller. E.g. it may be allocated on stack
	/// or embedded in caller-defined structs.
	///
	/// AVChannelLayout can be initialized as follows:
	///  - default initialization with {0}, followed by setting all used fields
	///    correctly;
	///  - by assigning one of the predefined AV_CHANNEL_LAYOUT_* initializers;
	///  - with a constructor function, such as av_channel_layout_default(),
	///    av_channel_layout_from_mask() or av_channel_layout_from_string().
	///
	/// The channel layout must be uninitialized with av_channel_layout_uninit()
	///
	/// Copying an AVChannelLayout via assigning is forbidden,
	/// av_channel_layout_copy() must be used instead (and its return value should
	/// be checked)
	///
	/// No new fields may be added to it without a major version bump, except for
	/// new elements of the union fitting in sizeof(uint64_t)
	/// </summary>
	public class AvChannelLayout : IClearable, IDeepCloneable<AvChannelLayout>, ICopyTo<AvChannelLayout>
	{
		/// <summary>
		/// Channel order used in this layout.
		/// This is a mandatory field
		/// </summary>
		public AvChannelOrder Order;

		/// <summary>
		/// Number of channels in this layout. Mandatory field
		/// </summary>
		public c_int Nb_Channels;

		/// <summary>
		/// Details about which channels are present in this layout.
		/// For AV_CHANNEL_ORDER_UNSPEC, this field is undefined and must not be
		/// used
		/// </summary>
		public (
			// This member must be used for AV_CHANNEL_ORDER_NATIVE, and may be used
			// for AV_CHANNEL_ORDER_AMBISONIC to signal non-diegetic channels.
			// It is a bitmask, where the position of each set bit means that the
			// AVChannel with the corresponding value is present.
			//
			// I.e. when (mask & (1 << AV_CHAN_FOO)) is non-zero, then AV_CHAN_FOO
			// is present in the layout. Otherwise it is not present.
			//
			// Note when a channel layout using a bitmask is constructed or
			// modified manually (i.e. not using any of the av_channel_layout_*
			// functions), the code doing it must ensure that the number of set bits
			// is equal to nb_channels
			AvChannelMask Mask,

			// This member must be used when the channel order is
			// AV_CHANNEL_ORDER_CUSTOM. It is a nb_channels-sized array, with each
			// element signalling the presence of the AVChannel with the
			// corresponding value in map[i].id.
			// 
			// I.e. when map[i].id is equal to AV_CHAN_FOO, then AV_CH_FOO is the
			// i-th channel in the audio data.
			// 
			// When map[i].id is in the range between AV_CHAN_AMBISONIC_BASE and
			// AV_CHAN_AMBISONIC_END (inclusive), the channel contains an ambisonic
			// component with ACN index (as defined above)
			// n = map[i].id - AV_CHAN_AMBISONIC_BASE.
			// 
			// map[i].name may be filled with a 0-terminated string, in which case
			// it will be used for the purpose of identifying the channel with the
			// convenience functions below. Otherwise it must be zeroed
			CPointer<AvChannelCustom> Map
		) U;

		/// <summary>
		/// For some private data of the user
		/// </summary>
		public IOpaque Opaque;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Order = AvChannelOrder.Unspec;
			Nb_Channels = 0;
			U.Mask = 0;
			U.Map.SetToNull();
			Opaque = null;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public AvChannelLayout MakeDeepClone()
		{
			AvChannelLayout clone = new AvChannelLayout
			{
				Order = Order,
				Nb_Channels = Nb_Channels,
				Opaque = Opaque
			};

			clone.U.Mask = U.Mask;

			if (U.Map.IsNotNull)
			{
				clone.U.Map = new CPointer<AvChannelCustom>(U.Map.Length);

				for (c_int i = U.Map.Length - 1; i >= 0; i--)
					clone.U.Map[i] = U.Map[i].MakeDeepClone();
			}

			return clone;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(AvChannelLayout destination)
		{
			destination.Order = Order;
			destination.Nb_Channels = Nb_Channels;
			destination.Opaque = Opaque;
			destination.U.Mask = U.Mask;

			if (U.Map.IsNotNull)
			{
				destination.U.Map = new CPointer<AvChannelCustom>(U.Map.Length);

				for (c_int i = U.Map.Length - 1; i >= 0; i--)
					destination.U.Map[i] = U.Map[i].MakeDeepClone();
			}
		}
	}
}
