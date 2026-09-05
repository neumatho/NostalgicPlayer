/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian
{
	#region Little endian

	#region int16le
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
	internal struct int16le
	{
		private byte _b0, _b1;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int16 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[2];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadInt16LittleEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[2];
				BinaryPrimitives.WriteInt16LittleEndian(data, value);
				this = MemoryMarshal.Read<int16le>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int16 MinValue => int16.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int16 MaxValue => int16.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int16(int16le x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int16le(int16 v)
		{
			return new int16le { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region uint16le
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
	internal struct uint16le
	{
		private byte _b0, _b1;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint16 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[2];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadUInt16LittleEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[2];
				BinaryPrimitives.WriteUInt16LittleEndian(data, value);
				this = MemoryMarshal.Read<uint16le>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint16 MinValue => uint16.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint16 MaxValue => uint16.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint16(uint16le x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint16le(uint16 v)
		{
			return new uint16le { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region int32le
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
	internal struct int32le
	{
		private byte _b0, _b1, _b2, _b3;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[4];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadInt32LittleEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[4];
				BinaryPrimitives.WriteInt32LittleEndian(data, value);
				this = MemoryMarshal.Read<int32le>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int32 MinValue => int32.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int32 MaxValue => int32.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int32(int32le x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int32le(int32 v)
		{
			return new int32le { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region uint32le
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
	internal struct uint32le
	{
		private byte _b0, _b1, _b2, _b3;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[4];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadUInt32LittleEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[4];
				BinaryPrimitives.WriteUInt32LittleEndian(data, value);
				this = MemoryMarshal.Read<uint32le>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 MinValue => uint32.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 MaxValue => uint32.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint32(uint32le x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint32le(uint32 v)
		{
			return new uint32le { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region int64le
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
	internal struct int64le
	{
		private byte _b0, _b1, _b2, _b3, _b4, _b5, _b6, _b7;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int64 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[8];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadInt64LittleEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[8];
				BinaryPrimitives.WriteInt64LittleEndian(data, value);
				this = MemoryMarshal.Read<int64le>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int64 MinValue => int64.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int64 MaxValue => int64.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int64(int64le x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int64le(int64 v)
		{
			return new int64le { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region uint64le
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
	internal struct uint64le
	{
		private byte _b0, _b1, _b2, _b3, _b4, _b5, _b6, _b7;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint64 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[8];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadUInt64LittleEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[8];
				BinaryPrimitives.WriteUInt64LittleEndian(data, value);
				this = MemoryMarshal.Read<uint64le>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint64 MinValue => uint64.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint64 MaxValue => uint64.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint64(uint64le x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint64le(uint64 v)
		{
			return new uint64le { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#endregion

	#region Big endian

	#region int16be
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
	internal struct int16be
	{
		private byte _b0, _b1;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int16 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[2];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadInt16BigEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[2];
				BinaryPrimitives.WriteInt16BigEndian(data, value);
				this = MemoryMarshal.Read<int16be>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int16 MinValue => int16.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int16 MaxValue => int16.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int16(int16be x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int16be(int16 v)
		{
			return new int16be { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region uint16be
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
	internal struct uint16be
	{
		private byte _b0, _b1;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint16 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[2];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadUInt16BigEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[2];
				BinaryPrimitives.WriteUInt16BigEndian(data, value);
				this = MemoryMarshal.Read<uint16be>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint16 MinValue => uint16.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint16 MaxValue => uint16.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint16(uint16be x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint16be(uint16 v)
		{
			return new uint16be { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region int32be
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
	internal struct int32be
	{
		private byte _b0, _b1, _b2, _b3;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[4];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadInt32BigEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[4];
				BinaryPrimitives.WriteInt32BigEndian(data, value);
				this = MemoryMarshal.Read<int32be>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int32 MinValue => int32.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int32 MaxValue => int32.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int32(int32be x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int32be(int32 v)
		{
			return new int32be { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region uint32be
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
	internal struct uint32be
	{
		private byte _b0, _b1, _b2, _b3;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[4];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadUInt32BigEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[4];
				BinaryPrimitives.WriteUInt32BigEndian(data, value);
				this = MemoryMarshal.Read<uint32be>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 MinValue => uint32.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 MaxValue => uint32.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint32(uint32be x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint32be(uint32 v)
		{
			return new uint32be { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region int64be
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
	internal struct int64be
	{
		private byte _b0, _b1, _b2, _b3, _b4, _b5, _b6, _b7;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int64 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[8];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadInt64BigEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[8];
				BinaryPrimitives.WriteInt64BigEndian(data, value);
				this = MemoryMarshal.Read<int64be>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int64 MinValue => int64.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int64 MaxValue => int64.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int64(int64be x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator int64be(int64 v)
		{
			return new int64be { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#region uint64be
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
	internal struct uint64be
	{
		private byte _b0, _b1, _b2, _b3, _b4, _b5, _b6, _b7;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint64 Value
		{
			get
			{
				Span<byte> data = stackalloc byte[8];
				MemoryMarshal.Write(data, in this);

				return BinaryPrimitives.ReadUInt64BigEndian(data);
			}

			set
			{
				Span<byte> data = stackalloc byte[8];
				BinaryPrimitives.WriteUInt64BigEndian(data, value);
				this = MemoryMarshal.Read<uint64be>(data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint64 MinValue => uint64.MinValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint64 MaxValue => uint64.MaxValue;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint64(uint64be x)
		{
			return x.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static implicit operator uint64be(uint64 v)
		{
			return new uint64be { Value = v };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	#endregion

	#endregion
}
