/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;
using Polycode.NostalgicPlayer.Ports.LibOgg.Internal;

namespace Polycode.NostalgicPlayer.Ports.LibOgg
{
	/// <summary>
	/// Interface to OggSync methods
	/// </summary>
	public class OggSync
	{
		private readonly Ogg_Sync_State state;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private OggSync(Ogg_Sync_State s)
		{
			state = s;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current state
		/// </summary>
		/********************************************************************/
		public Ogg_Sync_State State => state;



		/********************************************************************/
		/// <summary>
		/// Return a state in known state
		/// </summary>
		/********************************************************************/
		public static c_int Init(out OggSync oy)
		{
			if (Framing.Ogg_Sync_Init(out Ogg_Sync_State s) != 0)
			{
				oy = null;
				return -1;
			}

			oy = new OggSync(s);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Clear()
		{
			return Framing.Ogg_Sync_Clear(state);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Check()
		{
			return Framing.Ogg_Sync_Check(state);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<byte> Buffer(c_long size)
		{
			return Framing.Ogg_Sync_Buffer(state, size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Wrote(c_long bytes)
		{
			return Framing.Ogg_Sync_Wrote(state, bytes);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long PageSeek(out OggPage og)
		{
			c_long ret = Framing.Ogg_Sync_PageSeek(state, out Ogg_Page p);
			og = p == null ? null : new OggPage(p);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int PageOut(out OggPage og)
		{
			c_long ret = Framing.Ogg_Sync_PageOut(state, out Ogg_Page p);
			og = p == null ? null : new OggPage(p);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Reset()
		{
			return Framing.Ogg_Sync_Reset(state);
		}
	}
}
