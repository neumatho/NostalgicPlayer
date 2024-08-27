/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOgg.Containers
{
	/// <summary>
	/// Contains the current encode/decode state of a logical Ogg bitstream
	/// </summary>
	internal class Ogg_Stream_State
	{
		/// <summary>
		/// Bytes from packet bodies
		/// </summary>
		public byte[] BodyData;

		/// <summary>
		/// Storage elements allocated
		/// </summary>
		public c_long BodyStorage;

		/// <summary>
		/// Elements stored; fill mark
		/// </summary>
		public c_long BodyFill;

		/// <summary>
		/// Elements of fill returned
		/// </summary>
		public c_long BodyReturned;

		/// <summary>
		/// The values that will go to the segment table
		/// </summary>
		public c_int[] LacingVals;

		/// <summary>
		/// Granulepos values for headers. Not compact
		/// this way, but it is simple coupled to the
		/// lacing fifo
		/// </summary>
		public ogg_int64_t[] GranuleVals;

		/// <summary></summary>
		public c_long LacingStorage;
		/// <summary></summary>
		public c_long LacingFill;
		/// <summary></summary>
		public c_long LacingPacket;
		/// <summary></summary>
		public c_long LacingReturned;

		/// <summary>
		/// Working space for header encode
		/// </summary>
		public byte[] Header = new byte[282];
		/// <summary></summary>
		public c_int HeaderFill;

		/// <summary>
		/// Set when we have buffered the last packet in the
		/// logical bitstream
		/// </summary>
		public bool Eos;

		/// <summary>
		/// Set after we've written the initial page
		/// of a logical bitstream
		/// </summary>
		public bool Bos;
		/// <summary></summary>
		public c_long SerialNo;
		/// <summary></summary>
		public c_long PageNo;

		/// <summary>
		/// Sequence number for decode; the framing
		/// knows where there's a hole in the data,
		/// but we need coupling so that the codec
		/// (which is in a separate abstraction
		/// layer) also knows about the gap
		/// </summary>
		public ogg_int64_t PacketNo;
		public ogg_int64_t GranulePos;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			BodyData = null;
			BodyStorage = 0;
			BodyFill = 0;
			BodyReturned = 0;
			LacingVals = null;
			GranuleVals = null;

			LacingStorage = 0;
			LacingFill = 0;
			LacingPacket = 0;
			LacingReturned = 0;

			Array.Clear(Header);
			HeaderFill = 0;

			Eos = false;
			Bos = false;
			SerialNo = 0;
			PageNo = 0;

			PacketNo = 0;
			GranulePos = 0;
		}
	}
}
