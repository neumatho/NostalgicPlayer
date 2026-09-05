/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Container class for managing patterns
	/// </summary>
	internal class CPatternContainer
	{
		public interface IForeach<TWorker, T> : IDeepCloneable<TWorker>
		{
			void Invoke(CPointer<T> o);
		}

		private readonly vector<CPattern> m_Patterns = new vector<CPattern>();
		private readonly CSoundFile m_rSndFile;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CPatternContainer(CSoundFile sndFile)
		{
			m_rSndFile = sndFile;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPattern this[c_int pat]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => m_Patterns[pat];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PatternIndex Size()
		{
			return (PatternIndex)m_Patterns.size();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CSoundFile GetSoundFile()
		{
			return m_rSndFile;
		}



		/********************************************************************/
		/// <summary>
		/// Return true if pattern can be accessed with operator[](iPat),
		/// false otherwise
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidIndex(PatternIndex iPat)
		{
			return iPat < Size();
		}




		/********************************************************************/
		/// <summary>
		/// Return true if IsValidIndex() is true and the corresponding
		/// pattern has allocated modcommand array, false otherwise
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidPat(PatternIndex iPat)
		{
			return IsValidIndex(iPat) && m_Patterns[iPat].IsValid();
		}




		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEachModCommand<TWorker, T>(IForeach<TWorker, T> obj) where T : ModCommand
		{
			foreach (CPattern pattern in m_Patterns)
			{
				IForeach<TWorker, ModCommand> workObj = (IForeach<TWorker, ModCommand>)obj.MakeDeepClone();
				CPointer<ModCommand> arr = pattern.m_ModCommands.data();

				for (size_t i = 0; i < pattern.m_ModCommands.size(); i++)
					workObj.Invoke(arr.Slice((c_int)i));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void DestroyPatterns()//XX 40
		{
			m_Patterns.clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool Insert(PatternIndex index, RowIndex rows)//XX 76
		{
			if ((rows > Snd_Def.Max_Pattern_Rows) || (rows == 0) || (index >= Snd_Def.PatternIndex_Invalid))
				return false;

			if (IsValidPat(index))
				return false;

			try
			{
				if (index >= m_Patterns.size())
					m_Patterns.resize((size_t)index + 1, new CPattern(this));

				m_Patterns[index].AllocatePattern(rows);
				m_Patterns[index].RemoveSignature();
				m_Patterns[index].SetName(string.Empty);
			}
			catch (Exception)
			{
				return false;
			}

			return m_Patterns[index].IsValid();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ResizeArray(PatternIndex newSize)//XX 121
		{
			m_Patterns.resize(newSize, new CPattern(this));
		}



		/********************************************************************/
		/// <summary>
		/// Returns index of last valid pattern + 1, zero if no such pattern
		/// exists
		/// </summary>
		/********************************************************************/
		public PatternIndex GetNumPatterns()//XX 145
		{
			for (PatternIndex pat = Size(); pat > 0; pat--)
			{
				if (IsValidPat((PatternIndex)(pat - 1)))
					return pat;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns an enumerator over the patterns, so that the container
		/// can be used in a C# foreach loop (C++ begin()/end())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Buffer_Enumerator<CPattern> GetEnumerator()
		{
			return m_Patterns.GetEnumerator();
		}
	}
}
