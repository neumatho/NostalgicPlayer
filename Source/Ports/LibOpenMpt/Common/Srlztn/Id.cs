/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn
{
	/// <summary>
	/// 
	/// </summary>
	internal class Id : IEquatable<Id>
	{
		/// <summary>
		/// NOTE: can contain null characters ('\0')
		/// </summary>
		private StdString m_Id;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Id()
		{
			m_Id = new StdString();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Id(CPointer<uint8> id)
		{
			m_Id = new StdString(id.IsNotNull ? id : string.Empty.ToPointer());
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Id(CPointer<uint8> str, size_t len)
		{
			m_Id = new StdString(str, len);
		}



		/********************************************************************/
		/// <summary>
		/// Create an Id from the given string
		/// </summary>
		/********************************************************************/
		public static implicit operator Id(string str)
		{
			return new Id(str.ToPointer());
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Id FromInt<T>(T val) where T : IBinaryInteger<T>
		{
			uint8[] valle = new uint8[val.GetByteCount()];
			val.WriteLittleEndian(valle);

			return new Id(valle, (size_t)valle.Length);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (Id me, Id other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return me.m_Id == other.m_Id;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (Id me, Id other)
		{
			return !(me == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is Id other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Id other)
		{
			return this == other;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			return m_Id.GetHashCode();
		}
	}
}
