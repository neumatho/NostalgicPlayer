/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal class ModSequenceSet
	{
		protected readonly vector<ModSequence> m_Sequences = new vector<ModSequence>();
		protected readonly CSoundFile m_SndFile;
		protected SequenceIndex m_CurrentSeq = 0;		// Index of current sequence

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModSequenceSet(CSoundFile sndFile)//XX 330
		{
			m_SndFile = sndFile;

			Initialize();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ModSequence Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => m_Sequences[m_CurrentSeq];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ModSequence this[SequenceIndex seq]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => m_Sequences[seq];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceIndex GetNumSequences()
		{
			return (SequenceIndex)m_Sequences.size();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceIndex GetCurrentSequenceIndex()
		{
			return m_CurrentSeq;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Initialize()//XX 350
		{
			m_CurrentSeq = 0;
			m_Sequences.assign(1, new ModSequence(m_SndFile));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetSequence(SequenceIndex n)//XX 357
		{
			if (n < m_Sequences.size())
				m_CurrentSeq = n;
		}



		/********************************************************************/
		/// <summary>
		/// Returns an enumerator over the sequences, so that the container
		/// can be used in a C# foreach loop (C++ begin()/end())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Buffer_Enumerator<ModSequence> GetEnumerator()
		{
			return m_Sequences.GetEnumerator();
		}
	}
}
