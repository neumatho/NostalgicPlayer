/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOgg.Containers
{
	/// <summary>
	/// Is used to encapsulate the data and metadata belonging
	/// to a single raw Ogg packet
	/// </summary>
	public class Ogg_Packet
	{
		/// <summary></summary>
		public byte[] Packet;
		/// <summary></summary>
		public int Offset;
		/// <summary></summary>
		public c_long Bytes;
		/// <summary></summary>
		public bool Bos;
		/// <summary></summary>
		public bool Eos;

		/// <summary></summary>
		public ogg_int64_t GranulePos;

		/// <summary>
		/// Sequence number for decode; the framing
		/// knows where there's a hole in the data,
		/// but we need coupling so that the codec
		/// (which is in a separate abstraction
		/// layer) also knows about the gap
		/// </summary>
		public ogg_int64_t PacketNo;
	}
}
