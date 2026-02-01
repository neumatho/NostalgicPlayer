/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	public static class Metadata
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_Metadata_Conv(ref AvDictionary pm, CPointer<AvMetadataConv> d_Conv, CPointer<AvMetadataConv> s_Conv)//XX 26
		{
			// TODO: use binary search to look up the two conversion tables
			// if the tables are getting big enough that it would matter speed wise
			AvDictionary dst = null;

			if ((d_Conv == s_Conv) || (pm == null))
				return;

			foreach (AvDictionaryEntry mTag in Dict.Av_Dict_Iterate(pm))
			{
				CPointer<char> key = mTag.Key;

				if (s_Conv != null)
				{
					for (c_int i = 0; i < s_Conv.Length; i++)
					{
						AvMetadataConv sc = s_Conv[i];

						if (AvString.Av_Strcasecmp(key, sc.Native) == 0)
						{
							key = sc.Generic;
							break;
						}
					}
				}

				if (d_Conv != null)
				{
					for (c_int i = 0; i < d_Conv.Length; i++)
					{
						AvMetadataConv dc = d_Conv[i];

						if (AvString.Av_Strcasecmp(key, dc.Generic) == 0)
						{
							key = dc.Native;
							break;
						}
					}
				}

				Dict.Av_Dict_Set(ref dst, key, mTag.Value, AvDict.MultiKey | AvDict.Dedup);
			}

			Dict.Av_Dict_Free(ref pm);
			pm = dst;
		}
	}
}
