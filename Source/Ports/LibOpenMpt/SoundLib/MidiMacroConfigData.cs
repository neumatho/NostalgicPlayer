/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Algorithm = Polycode.NostalgicPlayer.Kit.C.Std.Algorithm;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal class MidiMacroConfigData
	{
		#region Macro class
		public class Macro : IEquatable<Macro>, IDeepCloneable<Macro>//XX TODO: Når XM/IT loader laves, bliver disse loadet ind direkte som een stor blok. Dette kan ikke lade sig gøre her, men vi kan have vores egen Load metode (både i Macro og i MidiMacroConfigData), som loader een Macro af gangen ind i array
		{
			private readonly array<uint8> m_Data;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Macro()
			{
				m_Data = new array<uint8>(MidiMacros.MacroLength);
			}



			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			private Macro(array<uint8> data)
			{
				m_Data = data;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public size_t Length()
			{
				return (size_t)Iterator.distance(m_Data.begin(), Algorithm.find(m_Data.begin(), m_Data.end(), (uint8)'\0'));
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Sanitize()
			{
				m_Data.back() = (uint8)'\0';

				size_t length = Length();
				Algorithm.fill(m_Data.data().Begin() + length, m_Data.end(), (uint8)'\0');

				for (size_t i = 0; i < length; i++)
				{
					if ((m_Data[i] < 32) || (m_Data[i] >= 127))
						m_Data[i] = (uint8)' ';
				}
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void UpgradeLegacyMacro()
			{
				for (size_t i = 0; i < m_Data.size(); i++)
				{
					uint8 c = m_Data[i];

					if ((c >= 'a') && (c <= 'f'))	// Both A-F and a-f were treated as hex constants
						c = (uint8)(c - 'a' + 'A');
					else if ((c == 'K') || (c == 'k'))	// Channel was K or k
						c = (uint8)'c';
					else if ((c == 'X') || (c == 'x') || (c == 'Y') || (c == 'y'))	// Those were pointless
						c = (uint8)'z';

					m_Data[i] = c;
				}
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator Macro(string str)
			{
				Macro result = new Macro();
				CPointer<uint8> other = new CPointer<uint8>(Encoding.Latin1.GetBytes(str));

				c_int searchResult = other.IndexOf((uint8)'\0');
				size_t copyLength = Math.Min(result.m_Data.size() - 1U, Math.Min(other.Size(), searchResult == -1 ? size_t.MaxValue : (size_t)searchResult));
				Algorithm.copy<CPointer<uint8>, forward_iterator<uint8>, uint8>(other.Begin(), other.Begin() + copyLength, result.m_Data.begin());
				result.m_Data[copyLength] = (uint8)'\0';

				result.Sanitize();

				return result;
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool operator == (Macro me, Macro other)
			{
				if (ReferenceEquals(me, other))
					return true;

				if ((me is null) || (other is null))
					return false;

				return me.m_Data == other.m_Data;
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool operator != (Macro me, Macro other)
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
				return (obj is Macro other) && (this == other);
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Equals(Macro other)
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
				HashCode hash = new HashCode();

				hash.Add(m_Data);

				return hash.ToHashCode();
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator MptSpan<uint8>(Macro macro)
			{
				return new MptSpan<uint8>(macro.m_Data.data(), macro.Length());
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator string(Macro macro)
			{
				return macro?.ToString() ?? string.Empty;
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public override string ToString()
			{
				return Encoding.Latin1.GetString(m_Data.data().AsSpan(0, Length()));
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public Macro MakeDeepClone()
			{
				return new Macro(new array<uint8>(m_Data));
			}
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		public readonly array<Macro> Global = new array<Macro>(MidiMacros.GlobalMacros);

		/// <summary>
		/// Parametered macros for Z00...Z7F
		/// </summary>
		public readonly array<Macro> SFx = new array<Macro>(MidiMacros.SFxMacros);

		/// <summary>
		/// Fixed macros Z80...ZFF
		/// </summary>
		public readonly array<Macro> Zxx = new array<Macro>(MidiMacros.ZxxMacros);
	}
}
