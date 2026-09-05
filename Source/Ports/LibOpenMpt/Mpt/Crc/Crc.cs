/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Crc
{
	/// <summary>
	/// 
	/// </summary>
	internal class Crc<T, TSpec> : ICrc where T : IBinaryInteger<T>, IUnsignedNumber<T> where TSpec : ICrcSpec<T>
	{
		private static readonly int size_Bits = Marshal.SizeOf<T>() * 8;
		private static readonly T top_Bit = T.One << ((Marshal.SizeOf<T>() * 8) - 1);
		private static readonly T[] table = Calculate_Table();

		private T value;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Crc()
		{
			value = TSpec.Initial;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Crc(CPointer<c_byte> beg, CPointer<c_byte> end) : this()
		{
			for (CPointer<c_byte> it = beg; it != end; ++it)
				Process(it[0]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ProcessByte(uint8 @byte)
		{
			if (TSpec.ReverseData)
				value = (value >> 8) ^ Read_Table(uint8.CreateTruncating(uint8.CreateTruncating(value) ^ @byte));
			else
				value = (value << 8) ^ Read_Table(uint8.CreateTruncating(uint8.CreateTruncating(value >> (size_Bits - 8)) ^ @byte));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ulong Result()
		{
			return ulong.CreateTruncating(value ^ TSpec.ResultXor);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ICrc Process(c_byte c)
		{
			ProcessByte(c);

			return this;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static TInt Reverse<TInt>(TInt value) where TInt : IBinaryInteger<TInt>, IBitwiseOperators<TInt, TInt, TInt>
		{
			size_t bits = (size_t)Marshal.SizeOf<TInt>() * 8;
			TInt result = TInt.Zero;

			for (size_t i = 0; i < bits; ++i)
			{
				result <<= 1;
				result |= (value & TInt.CreateTruncating(0x1));

				value >>= 1;
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static T Calculate_Table_Entry(uint8 pos)
		{
			T value = T.CreateTruncating(TSpec.ReverseData ? Reverse(pos) : pos) << (size_Bits - 8);

			for (size_t bit = 0; bit < 8; ++bit)
			{
				if ((value & top_Bit) != T.Zero)
					value = (value << 1) ^ TSpec.Polynomial;
				else
					value = value << 1;
			}

			value = TSpec.ReverseData ? Reverse(value) : value;

			return value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static T[] Calculate_Table()
		{
			T[] t = Array_.Init_Array<T>(256);

			for (size_t i = 0; i < 256; ++i)
				t[i] = Calculate_Table_Entry((uint8)i);

			return t;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private T Read_Table(uint8 pos)
		{
			return table[pos];
		}
		#endregion
	}
}
