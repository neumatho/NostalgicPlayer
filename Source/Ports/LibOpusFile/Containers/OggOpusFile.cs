/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus;

namespace Polycode.NostalgicPlayer.Ports.LibOpusFile.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class OggOpusFile
	{
		/// <summary>
		/// The callbacks used to access the stream
		/// </summary>
		public OpusFileCallbacks callbacks;

		/// <summary>
		/// A FILE *, memory buffer, etc.
		/// </summary>
		public object stream;

		/// <summary>
		/// Whether or not we can seek with this stream
		/// </summary>
		public bool seekable;

		/// <summary>
		/// The number of links in this chained Ogg Opus file
		/// </summary>
		public c_int nlinks;

		/// <summary>
		/// The cached information from each link in a chained Ogg Opus file.
		/// If stream isn't seekable (e.g., it's a pipe), only the current
		/// link appears
		/// </summary>
		public CPointer<OggOpusLink> links;

		/// <summary>
		/// The number of serial numbers from a single link
		/// </summary>
		public c_int nserialnos;

		/// <summary>
		/// The capacity of the list of serial numbers from a single link
		/// </summary>
		public c_int cserialnos;

		/// <summary>
		/// Storage for the list of serial numbers from a single link.
		/// This is a scratch buffer used when scanning the BOS pages at the
		/// start of each link
		/// </summary>
		public CPointer<ogg_uint32_t> serialnos;

		/// <summary>
		/// This is the current offset of the data processed by the
		/// ogg_sync_state. After a seek, this should be set to the target
		/// offset so that we can track the byte offsets of subsequent pages.
		/// After a call to op_get_next_page(), this will point to the first
		/// byte after that page
		/// </summary>
		public opus_int64 offset;

		/// <summary>
		/// The total size of this stream, or -1 if it's unseekable
		/// </summary>
		public opus_int64 end;

		/// <summary>
		/// Used to locate pages in the stream
		/// </summary>
		public OggSync oy;

		/// <summary>
		/// 
		/// </summary>
		public State ready_state;

		/// <summary>
		/// The current link being played back
		/// </summary>
		public c_int cur_link;

		/// <summary>
		/// The number of decoded samples to discard from the start of
		/// decoding
		/// </summary>
		public opus_int32 cur_discard_count;

		/// <summary>
		/// The granule position of the previous packet (current packet start
		/// time)
		/// </summary>
		public ogg_int64_t prev_packet_gp;

		/// <summary>
		/// The stream offset of the most recent page with completed packets,
		/// or -1. This is only needed to recover continued packet data in
		/// the seeking logic, when we use the current position as one of
		/// our bounds, only to later discover it was the correct starting
		/// point
		/// </summary>
		public opus_int64 prev_page_offset;

		/// <summary>
		/// The number of bytes read since the last bitrate query, including
		/// framing
		/// </summary>
		public opus_int64 bytes_tracked;

		/// <summary>
		/// The number of samples decoded since the last bitrate query
		/// </summary>
		public ogg_int64_t samples_tracked;

		/// <summary>
		/// Takes physical pages and welds them into a logical stream of
		/// packets
		/// </summary>
		public OggStream os;

		/// <summary>
		/// Re-timestamped packets from a single page.
		/// Buffering these relies on the undocumented libogg behavior that
		/// ogg_packet pointers remain valid until the next page is submitted
		/// to the ogg_stream_state they came from
		/// </summary>
		public Ogg_Packet[] op = new Ogg_Packet[255];

		/// <summary>
		/// The index of the next packet to return
		/// </summary>
		public c_int op_pos;

		/// <summary>
		/// The total number of packets available
		/// </summary>
		public c_int op_count;

		/// <summary>
		/// Central working state for the packet-to-PCM decoder
		/// </summary>
		public OpusMsDecoder od;

		/// <summary>
		/// The application-provided packet decode callback
		/// </summary>
		public OpusFile.Op_Decode_Cb_Func decode_cb;

		/// <summary>
		/// The application-provided packet decode callback context
		/// </summary>
		public object decode_cb_ctx;

		/// <summary>
		/// The stream count used to initialize the decoder
		/// </summary>
		public c_int od_stream_count;

		/// <summary>
		/// The coupled stream count used to initialize the decoder
		/// </summary>
		public c_int od_coupled_count;

		/// <summary>
		/// The channel count used to initialize the decoder
		/// </summary>
		public c_int od_channel_count;

		/// <summary>
		/// The channel mapping used to initialize the decoder
		/// </summary>
		public byte[] od_mapping = new byte[Constants.Op_NChannels_Max];

		/// <summary>
		/// The buffered data for one decoded packet
		/// </summary>
		public CPointer<op_sample> od_buffer;

		/// <summary>
		/// The current position in the decoded buffer
		/// </summary>
		public c_int od_buffer_pos;

		/// <summary>
		/// The number of valid samples in the decoded buffer
		/// </summary>
		public c_int od_buffer_size;

		/// <summary>
		/// The type of gain offset to apply
		/// </summary>
		public GainType gain_type;

		/// <summary>
		/// The offset to apply to the gain
		/// </summary>
		public opus_int32 gain_offset_q8;

		/// <summary>
		/// Internal state for soft clipping and dithering float to short
		/// output
		/// </summary>
		public c_float[] clip_state = new c_float[Constants.Op_NChannels_Max];

		public c_float[] dither_a = new c_float[Constants.Op_NChannels_Max * 4];
		public c_float[] dither_b = new c_float[Constants.Op_NChannels_Max * 4];
		public opus_uint32 dither_seed;
		public c_int dither_mute;
		public c_int dither_disabled;

		/// <summary>
		/// The number of channels represented by the internal state.
		/// This gets set to 0 whenever anything that would prevent state
		/// propagation occurs (switching between the float/short APIs, or
		/// between the stereo/multistream APIs)
		/// </summary>
		public c_int state_channel_count;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			callbacks = null;
			stream = null;
			seekable = false;
			nlinks = 0;
			links.SetToNull();
			nserialnos = 0;
			cserialnos = 0;
			serialnos.SetToNull();
			offset = 0;
			end = 0;
			oy = null;
			ready_state = State.NotOpen;
			cur_link = 0;
			cur_discard_count = 0;
			prev_packet_gp = 0;
			prev_page_offset = 0;
			bytes_tracked = 0;
			samples_tracked = 0;
			os = null;
			op_pos = 0;
			op_count = 0;
			od = null;
			decode_cb = null;
			decode_cb_ctx = null;
			od_stream_count = 0;
			od_coupled_count = 0;
			od_channel_count = 0;
			od_buffer.SetToNull();
			od_buffer_pos = 0;
			od_buffer_size = 0;
			gain_type = 0;
			gain_offset_q8 = 0;
			dither_seed = 0;
			dither_mute = 0;
			dither_disabled = 0;
			state_channel_count = 0;

			Array.Clear(op);
			Array.Clear(od_mapping);
			Array.Clear(clip_state);
			Array.Clear(dither_a);
			Array.Clear(dither_b);
		}
	}
}
