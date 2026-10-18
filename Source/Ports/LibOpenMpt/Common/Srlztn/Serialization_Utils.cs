/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn
{
	/// <summary>
	/// Serializing data to and from MPTM files
	/// </summary>
	internal static class Serialization_Utils
	{
		/// <summary>
		/// 
		/// </summary>
		public const uint8 HeaderId_FlagByte = 0;

		/// <summary>
		/// 
		/// </summary>
		public const size_t InvalidDataSize = size_t.MaxValue;

		/// <summary>
		/// 
		/// </summary>
		public const uint16 IdSizeVariable = uint16.MaxValue;

		public static readonly Status Snr_ZeroEntryCount = new Status { Level = StatusLevel.Note, Messages = StatusMessages.ZeroEntryCount };
		public static readonly Status Snr_No_EntryIds_With_CustomId_Defined = new Status { Level = StatusLevel.Note, Messages = StatusMessages.No_EntryIds_With_CustomId_Defined };
		public static readonly Status Snr_Loading_Object_With_Larger_Version = new Status { Level = StatusLevel.Note, Messages = StatusMessages.Loading_Object_With_Larger_Version };
		public static readonly Status Snr_BadStream_After_MapHeaderSeek = new Status { Level = StatusLevel.Failure, Messages = StatusMessages.BadStream_After_MapHeaderSeek };
		public static readonly Status Snr_StartByte_Mismatch = new Status { Level = StatusLevel.Failure, Messages = StatusMessages.StartByte_Mismatch };
		public static readonly Status Snr_BadStream_At_Map_Read = new Status { Level = StatusLevel.Failure, Messages = StatusMessages.BadStream_At_Map_Read };
		public static readonly Status Snr_Insufficient_Stream_OffType = new Status { Level = StatusLevel.Failure, Messages = StatusMessages.Insufficient_Stream_OffType };
		public static readonly Status Snr_ObjectClass_IdMismatch = new Status { Level = StatusLevel.Failure, Messages = StatusMessages.ObjectClass_IdMismatch };
		public static readonly Status Snr_Too_Many_Entries_To_Read = new Status { Level = StatusLevel.Failure, Messages = StatusMessages.Too_Many_Entries_To_Read };
		public static readonly Status Snr_Insufficient_RPosType = new Status { Level = StatusLevel.Failure, Messages = StatusMessages.Insufficient_RPosType };

		/********************************************************************/
		/// <summary>
		/// Indexing starts from 0
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TestBit(uint8 val, uint8 bitIndex)
		{
			return (val & (1 << bitIndex)) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read a string item. This is the specialization of ReadItem() for
		/// strings
		/// </summary>
		/********************************************************************/
		public static void ReadItemString(ReaderStream iStrm, ref StdString str, size_t _)
		{
			// bits 0,1: Bytes per char type: 1,2,3,4.
			// bits 2,3: Bytes in size indicator, 1,2,3,4
			BinaryRead(iStrm, out uint32 id, 1);
			uint8 nSizeBytes = (uint8)((id & 12) >> 2);	// 12 == 1100b

			if (nSizeBytes > 0)
			{
				uint8 bytes = Math.Min((uint8)3, nSizeBytes);
				uint8 v2 = 0;
				uint8 v3 = 0;
				uint8 v4 = 0;

				if (bytes >= 1)
					BinaryRead(iStrm, out v2);

				if (bytes >= 2)
					BinaryRead(iStrm, out v3);

				if (bytes >= 3)
					BinaryRead(iStrm, out v4);

				id &= 0xff;
				id |= ((uint32)v2 << 8) | ((uint32)v3 << 16) | ((uint32)v4 << 24);
			}

			// Limit to 1 MB
			str.resize(Math.Min(id >> 4, 1000000U));

			for (size_t i = 0; i < str.size(); i++)
				iStrm.ReadInto(str.data().AsSpan(i, 1));

			id = (id >> 4) - (uint32)str.size();
			if (id > 0)
				iStrm.Seek(id, SeekOrigin.Current);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void BinaryRead<T>(Stream iStrm, out T data)
		{
			Io_.ReadIntLE(iStrm, out data);
		}



		/********************************************************************/
		/// <summary>
		/// Read only given number of bytes to the beginning of data; data
		/// bytes are memset to 0 before reading
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void BinaryRead<T>(Stream iStrm, out T data, size_t byteCount)
		{
			Io_.ReadBinaryTruncatedLE(iStrm, out data, byteCount);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReadItem<T>(Stream iStrm, ref T data, size_t nSize)
		{
			if ((nSize == (size_t)Marshal.SizeOf<T>()) || (nSize == InvalidDataSize))
				BinaryRead(iStrm, out data);
			else
				BinaryRead(iStrm, out data, nSize);
		}
	}
}
