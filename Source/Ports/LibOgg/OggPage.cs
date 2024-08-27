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
	/// Interface to OggPage methods
	/// </summary>
	public class OggPage
	{
		private readonly Ogg_Page page;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal OggPage(Ogg_Page p)
		{
			page = p;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current page
		/// </summary>
		/********************************************************************/
		public Ogg_Page Page => page;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Version()
		{
			return Framing.Ogg_Page_Version(page);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool Continued()
		{
			return Framing.Ogg_Page_Continued(page);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool Bos()
		{
			return Framing.Ogg_Page_Bos(page);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool Eos()
		{
			return Framing.Ogg_Page_Eos(page);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ogg_int64_t GranulePos()
		{
			return Framing.Ogg_Page_GranulePos(page);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int SerialNo()
		{
			return Framing.Ogg_Page_SerialNo(page);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long PageNo()
		{
			return Framing.Ogg_Page_PageNo(page);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Checksum_Set()
		{
			Framing.Ogg_Page_Checksum_Set(page);
		}
	}
}
