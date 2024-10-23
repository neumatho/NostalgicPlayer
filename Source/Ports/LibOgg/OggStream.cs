/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;
using Polycode.NostalgicPlayer.Ports.LibOgg.Internal;

namespace Polycode.NostalgicPlayer.Ports.LibOgg
{
	/// <summary>
	/// Interface to OggStream methods
	/// </summary>
	public class OggStream
	{
		private readonly Ogg_Stream_State state;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private OggStream(Ogg_Stream_State s)
		{
			state = s;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current state
		/// </summary>
		/********************************************************************/
		public Ogg_Stream_State State => state;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Init(out OggStream os, c_int serialNo)
		{
			if (Framing.Ogg_Stream_Init(out Ogg_Stream_State s, serialNo) != 0)
			{
				os = null;
				return -1;
			}

			os = new OggStream(s);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Check()
		{
			return Framing.Ogg_Stream_Check(state);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Clear()
		{
			return Framing.Ogg_Stream_Clear(state);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int IoVecIn(Ogg_IoVec[] iov, c_int count, bool eos, ogg_int64_t granulePos)
		{
			return Framing.Ogg_Stream_IoVecIn(state, iov, count, eos, granulePos);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int PacketIn(Ogg_Packet op)
		{
			return Framing.Ogg_Stream_PacketIn(state, op);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int PageOut(out OggPage og)
		{
			if (Framing.Ogg_Stream_PageOut(state, out Ogg_Page p) == 0)
			{
				og = null;
				return 0;
			}

			og = new OggPage(p);

			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int PageIn(OggPage og)
		{
			return Framing.Ogg_Stream_PageIn(state, og.Page);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Reset()
		{
			return Framing.Ogg_Stream_Reset(state);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Reset_SerialNo(c_int serialNo)
		{
			return Framing.Ogg_Stream_Reset_SerialNo(state, serialNo);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int PacketOut(out Ogg_Packet op)
		{
			return Framing.Ogg_Stream_PacketOut(state, out op);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int PacketPeek(out Ogg_Packet op)
		{
			return Framing.Ogg_Stream_PacketPeek(state, out op);
		}
	}
}
