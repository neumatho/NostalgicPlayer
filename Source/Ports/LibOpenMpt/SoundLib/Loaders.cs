/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;

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



		/********************************************************************/
		/// <summary>
		/// Read 'howMany' order items as integers with defined endianness
		/// from a file.
		/// 'stopIndex' is treated as '---', 'ignoreIndex' is treated as
		/// '+++'. If the format doesn't support such indices, just pass
		/// uint16_max
		/// </summary>
		/********************************************************************/
		public static bool ReadOrderFromFile<T>(ModSequence order, FileReader file, size_t howMany, uint16 stopIndex = uint16.MaxValue, uint16 ignoreIndex = uint16.MaxValue) where T : unmanaged, INumberBase<T>
		{
			return ReadOrderFromFile<T>(order, file, howMany, stopIndex, ignoreIndex, PatternIndex.CreateTruncating);
		}



		/********************************************************************/
		/// <summary>
		/// Read 'howMany' order items as integers with defined endianness
		/// from a file.
		/// 'stopIndex' is treated as '---', 'ignoreIndex' is treated as
		/// '+++'. If the format doesn't support such indices, just pass
		/// uint16_max
		/// </summary>
		/********************************************************************/
		public static bool ReadOrderFromFile<T, TValue>(ModSequence order, FileReader file, size_t howMany, uint16 stopIndex = uint16.MaxValue, uint16 ignoreIndex = uint16.MaxValue) where T : unmanaged, IEndianNumber<TValue> where TValue : unmanaged, INumberBase<TValue>
		{
			return ReadOrderFromFile<T>(order, file, howMany, stopIndex, ignoreIndex, x => PatternIndex.CreateTruncating(x.Value));
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read 'howMany' order items from a file and convert each of them
		/// to a pattern index using the given converter
		/// </summary>
		/********************************************************************/
		private static bool ReadOrderFromFile<T>(ModSequence order, FileReader file, size_t howMany, uint16 stopIndex, uint16 ignoreIndex, Func<T, PatternIndex> convert) where T : unmanaged
		{
			if (!file.CanRead(howMany * (size_t)Marshal.SizeOf<T>()))
				return false;

			OpenMpt.LimitMax(ref howMany, (size_t)Snd_Def.Max_Orders);
			OrderIndex readEntries = (OrderIndex)howMany;

			order.resize(readEntries);

			T patF = default;

			for (size_t i = 0; i < order.size(); i++)
			{
				file.Read(ref patF);
				PatternIndex pat = convert(patF);

				if (pat == stopIndex)
					pat = Snd_Def.PatternIndex_Invalid;
				else if (pat == ignoreIndex)
					pat = Snd_Def.PatternIndex_Skip;

				order[i] = pat;
			}

			return true;
		}
		#endregion
	}
}
