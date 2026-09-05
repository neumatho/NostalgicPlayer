/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Crc;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal class Prng_Random_Device_Time_Seeder
	{
		private static readonly Dictionary<Type, Func<CPointer<c_byte>, CPointer<c_byte>, ICrc>> default_Random_Seed_Hash = new()
		{
			[typeof(uint8)] = (b, e) => new Crc16(b, e),
			[typeof(uint16)] = (b, e) => new Crc16(b, e),
			[typeof(uint32)] = (b, e) => new Crc32c(b, e),
			[typeof(uint64)] = (b, e) => new Crc64_Jones(b, e)
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Generate_Seed<T>() where T : IUnsignedNumber<T>
		{
			// Note: CRC is actually not that good a choice here, but it is simple and we
			// already have an implementation available. Better choices for mixing entropy
			// would be a hash function with proper avalanche characteristics or a block
			// or stream cipher with any pre-choosen random key and IV. The only aspect we
			// really need here is whitening of the bits
			ICrc hash;

			uint64be time = (uint64)CTime.time(out _);
			CPointer<c_byte> bytes = new CPointer<c_byte>(Marshal.SizeOf(time));
			CMemory.memcpy(bytes, MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref time, 1)), (size_t)Marshal.SizeOf(time));

			hash = default_Random_Seed_Hash[typeof(T)](bytes.Begin(), bytes.End());

			return T.CreateTruncating(hash.Result());
		}
	}
}
