/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	public static class Protocols
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvClass> FF_UrlContext_Child_Class_Iterate()//XX 85
		{
			foreach (UrlProtocol prot in Supported.Url_Protocols)
			{
				AvClass ret = prot.Priv_Data_Class;
				if (ret != null)
					yield return ret;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Construct a list of protocols matching a given whitelist and/or
		/// blacklist
		/// </summary>
		/********************************************************************/
		public static IEnumerable<UrlProtocol> FFUrl_Get_Protocols(CPointer<char> whitelist, CPointer<char> blacklist)//XX 125
		{
			foreach (UrlProtocol up in Supported.Url_Protocols)
			{
				if (whitelist.IsNotNull && (AvString.Av_Match_Name(up.Name, whitelist) == 0))
					continue;

				if (blacklist.IsNotNull && (AvString.Av_Match_Name(up.Name, blacklist) != 0))
					continue;

				yield return up;
			}
		}
	}
}
