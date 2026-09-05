/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Common functions for module loaders
	/// </summary>
	internal static class Loaders
	{
		/********************************************************************/
		/// <summary>
		/// Read 'howMany' order items from an array.
		/// 'stopIndex' is treated as '---', 'ignoreIndex' is treated as
		/// '+++'. If the format doesn't support such indices, just pass
		/// uint16_max
		/// </summary>
		/********************************************************************/
		public static bool ReadOrderFromArray<T>(ModSequence order, array<T> orders, size_t? howMany_ = null, uint16 stopIndex = uint16.MaxValue, uint16 ignoreIndex = uint16.MaxValue) where T : INumberBase<T>
		{
			size_t howMany = howMany_ ?? orders.size();

			OpenMpt.LimitMax(ref howMany, orders.size());
			OpenMpt.LimitMax(ref howMany, (size_t)Snd_Def.Max_Orders);
			OrderIndex readEntries = (OrderIndex)howMany;

			order.resize(readEntries);

			for (c_int i = 0; i < readEntries; i++)
			{
				PatternIndex pat = PatternIndex.CreateTruncating(orders[i]);

				if (pat == stopIndex)
					pat = Snd_Def.PatternIndex_Invalid;
				else if (pat == ignoreIndex)
					pat = Snd_Def.PatternIndex_Skip;

				order.at(i) = pat;
			}

			return true;
		}
	}
}
