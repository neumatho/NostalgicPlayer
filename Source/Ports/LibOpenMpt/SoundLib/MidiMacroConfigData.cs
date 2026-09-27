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
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
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
		/// <summary>
		/// Size of the structure as it is stored in the module files
		/// </summary>
		private const size_t StructSize = (MidiMacros.GlobalMacros + MidiMacros.SFxMacros + MidiMacros.ZxxMacros) * MidiMacros.MacroLength;

		#region Macro class
		public class Macro : IEquatable<Macro>, IDeepCloneable<Macro>
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
			/// Read a single macro from the file. Only the first "size" bytes
			/// are taken from the file, the rest of the macro is cleared. The
			/// file position is always advanced by "size" bytes
			/// </summary>
			/********************************************************************/
			public void Load(FileReader file, size_t size)
			{
				m_Data.fill(0);

				file.GetRaw(m_Data.data().AsSpan(size));
				file.Skip(size);
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
			public StdString NormalizedString()
			{
				StdString sanitizedMacro = this;

				size_t pos = 0;

				while ((pos = sanitizedMacro.find_first_not_of("0123456789ABCDEFabchmnopsuvxyz", pos)) != StdString.npos)
					sanitizedMacro.erase(pos, 1);

				return sanitizedMacro;
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
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator StdString(Macro macro)
			{
				return new StdString(macro?.ToString() ?? string.Empty);
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

		/********************************************************************/
		/// <summary>
		/// Read the macros from the file. In the original code, the whole
		/// structure is read as one big block of memory, which is not
		/// possible here, so the macros are read one at a time instead.
		///
		/// At most "StructSize" bytes are used, any macro not covered by
		/// them is cleared. The file position is always advanced by
		/// "size" bytes, unless the end of the file is reached.
		///
		/// Returns the number of bytes actually read
		/// </summary>
		/********************************************************************/
		public size_t Load(FileReader file, uint32 size)
		{
			size_t copyBytes = Math.Min(size, StructSize);

			if (!file.CanRead(copyBytes))
				copyBytes = size_t.CreateSaturating(file.BytesLeft());

			size_t bytesLeft = copyBytes;
			array<Macro>[] allMacros = [ Global, SFx, Zxx ];

			foreach (array<Macro> macros in allMacros)
			{
				for (size_t i = 0; i < macros.size(); i++)
				{
					size_t macroBytes = Math.Min(bytesLeft, (size_t)MidiMacros.MacroLength);

					// Do not reuse the existing macro object, since the
					// same instance may be shared between several entries
					Macro macro = new Macro();
					macro.Load(file, macroBytes);

					macros[i] = macro;
					bytesLeft -= macroBytes;
				}
			}

			file.Skip(size - copyBytes);

			return copyBytes;
		}
	}
}
