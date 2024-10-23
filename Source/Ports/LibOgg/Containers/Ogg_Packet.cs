/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOgg.Containers
{
	/// <summary>
	/// Is used to encapsulate the data and metadata belonging
	/// to a single raw Ogg packet
	/// </summary>
	public class Ogg_Packet : IDeepCloneable<Ogg_Packet>
	{
		/// <summary></summary>
		public Pointer<byte> Packet;
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

		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Ogg_Packet MakeDeepClone()
		{
			return new Ogg_Packet
			{
				Packet = Packet.MakeDeepClone(),
				Bytes = Bytes,
				Bos = Bos,
				Eos = Eos,
				GranulePos = GranulePos,
				PacketNo = PacketNo
			};
		}
	}
}
