/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class BitStream_Filters
	{
		private static readonly FFBitStreamFilter[] bitStream_Filters =
		[
		];

		/********************************************************************/
		/// <summary>
		/// Iterate over all registered bitstream filters
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvBitStreamFilter> Av_Bsf_Iterate()//XX 79
		{
			return bitStream_Filters.Select(x => x.P);
		}



		/********************************************************************/
		/// <summary>
		/// Return a bitstream filter with the specified name or NULL if no
		/// such bitstream filter exists
		/// </summary>
		/********************************************************************/
		public static AvBitStreamFilter Av_Bsf_Get_By_Name(CPointer<char> name)//XX 91
		{
			if (name.IsNull)
				return null;

			foreach (AvBitStreamFilter f in Av_Bsf_Iterate())
			{
				if (CString.strcmp(f.Name, name) == 0)
					return f;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static IEnumerable<AvClass> FF_Bsf_Child_Class_Iterate()//XX 107
		{
			foreach (AvBitStreamFilter f in Av_Bsf_Iterate())
			{
				if (f.Priv_Class != null)
					yield return f.Priv_Class;
			}
		}
	}
}
