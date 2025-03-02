/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Utility
{
	/// <summary>
	/// Helper class to sample data
	/// </summary>
	public static class SampleHelper
	{
		/********************************************************************/
		/// <summary>
		/// Convert a sample array to 8-bit format
		/// </summary>
		/********************************************************************/
		public static Span<sbyte> ConvertSampleTypeTo8Bit(Array sample, uint offset)
		{
			Type type = sample.GetType().GetElementType();

			if (type == typeof(sbyte))
				return ((sbyte[])sample).AsSpan((int)offset);

			if (type == typeof(short))
				return MemoryMarshal.Cast<short, sbyte>((short[])sample).Slice((int)offset);

			return MemoryMarshal.Cast<byte, sbyte>((byte[])sample).Slice((int)offset);
		}



		/********************************************************************/
		/// <summary>
		/// Convert a sample array to 16-bit format
		/// </summary>
		/********************************************************************/
		public static Span<short> ConvertSampleTypeTo16Bit(Array sample, uint offset)
		{
			Type type = sample.GetType().GetElementType();

			if (type == typeof(short))
				return ((short[])sample).AsSpan((int)offset);

			if (type == typeof(sbyte))
				return MemoryMarshal.Cast<sbyte, short>((sbyte[])sample).Slice((int)offset);

			return MemoryMarshal.Cast<byte, short>((byte[])sample).Slice((int)offset);
		}
	}
}
