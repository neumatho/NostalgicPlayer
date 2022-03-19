/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Sequences;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// PlayPosition interface
	/// </summary>
	internal struct PlayPosition
	{
		public enum AdvMode
		{
			AdvSong,
			AdvBlock
		}

		public enum PosDef
		{
			SongStart,
			SongCurrPos,
			FirstBlock,
			PrevBlock,
			NextBlock,
			LastBlock,
			FirstLine,
			LastLine,
			NextLine,
			PrevLine
		}

		public delegate bool CmdHandler(OctaMedWorker worker, PlaySeqEntry pse);

		private readonly OctaMedWorker worker;

		private BlockNum pBlock;
		private LineNum pLine;
		private PSeqNum psq;
		private uint pSecPos;
		private uint pSeqPos;
		private AdvMode advMode;
		private SubSong parentSS;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayPosition(OctaMedWorker worker)
		{
			this.worker = worker;
			pBlock = 0;
			pLine = 0;
			psq = 0;
			pSecPos = 0;
			pSeqPos = 0;
			parentSS = null;
			advMode = AdvMode.AdvSong;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetParent(SubSong parent)
		{
			parentSS = parent;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Block(BlockNum blk)
		{
			pBlock = blk;
			VerifyRange();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Line(LineNum line)
		{
			pLine = line;
			VerifyRange();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void PSeqPos(uint pos)
		{
			pSeqPos = pos;
			VerifyRange();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void PSectPos(uint pos)
		{
			pSecPos = pos;
			VerifyRange();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public BlockNum Block()
		{
			return pBlock;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public LineNum Line()
		{
			return pLine;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint PSeqPos()
		{
			return pSeqPos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint PSectPos()
		{
			return pSecPos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void AdvancePos(CmdHandler cmdHandler)
		{
			pLine++;

			if (pLine >= parentSS.Block(pBlock).Lines())
			{
				pLine = 0;

				if (advMode == AdvMode.AdvSong)
				{
					AdvanceSongPosition(pSeqPos + 1, cmdHandler);

					// Tell NostalgicPlayer that the position has changed
					worker.ChangePosition();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void PatternBreak(LineNum newLNum, CmdHandler cmdHandler)
		{
			if (advMode == AdvMode.AdvSong)
				AdvanceSongPosition(pSeqPos + 1, cmdHandler);

			pLine = newLNum;
			if (pLine >= parentSS.Block(pBlock).Lines())
				pLine = 0;

			// Tell NostalgicPlayer that the position has changed
			worker.ChangePosition();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void PositionJump(uint newPSeqPos, CmdHandler cmdHandler)
		{
			if (advMode == AdvMode.AdvSong)
			{
				if (newPSeqPos <= pSeqPos)
					worker.SetEndReached();

				AdvanceSongPosition(newPSeqPos, cmdHandler);
			}

			pLine = 0;

			// Tell NostalgicPlayer that the position has changed
			worker.ChangePosition();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetAdvMode(AdvMode mode)
		{
			advMode = mode;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetPosition(PosDef pos)
		{
			switch (pos)
			{
				// Restart from the beginning of the song
				case PosDef.SongStart:
				{
					pSecPos = 0;
					pSeqPos = 0;
					pLine = 0;
					goto case PosDef.SongCurrPos;
				}

				// From current section and sequence positions
				case PosDef.SongCurrPos:
				{
					psq = parentSS.Sect(pSecPos).Value;
					AdvanceSongPosition(pSeqPos, null);
					VerifyRange();
					break;
				}

				case PosDef.NextBlock:
				{
					pBlock++;
					pLine = 0;
					VerifyRange();
					break;
				}

				case PosDef.PrevBlock:
				{
					if (pBlock > 0)
					{
						pBlock--;
						pLine = 0;
						VerifyRange();
					}
					break;
				}

				case PosDef.FirstBlock:
				{
					pBlock = 0;
					pLine = 0;
					break;
				}

				case PosDef.LastBlock:
				{
					pLine = 0;
					pBlock = parentSS.NumBlocks() - 1;
					break;
				}

				case PosDef.FirstLine:
				{
					pLine = 0;
					break;
				}

				case PosDef.PrevLine:
				{
					if (pLine > 0)
						pLine--;

					break;
				}

				case PosDef.NextLine:
				{
					pLine++;
					VerifyRange();
					break;
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AdvanceSongPosition(uint newPSeqPos, CmdHandler cmdHandler)
		{
			uint jumpCount = 0;
			uint songRollOver = 0;

			pSeqPos = newPSeqPos;

			for (;;)
			{
				if (pSeqPos >= parentSS.PSeq(psq).Count)
				{
					if (songRollOver++ > 3)
					{
						// Extreme case hang-prevention
						pBlock = 0;
						break;
					}

					pSeqPos = 0;
					pSecPos++;

					if (pSecPos >= parentSS.NumSections())
					{
						pSecPos = 0;
						worker.SetEndReached();
					}

					psq = parentSS.Sect(pSecPos).Value;
				}

				PlaySeqEntry pse = parentSS.PSeq(psq)[(int)pSeqPos];
				BlockNum newBlk = pse.Value;

				if (pse.IsCmd())
				{
					switch (pse.GetCmd())
					{
						case PSeqCmd.PosJump:
						{
							if (jumpCount++ < 10)
							{
								pSeqPos = newBlk;
								continue;
							}
							goto default;
						}

						default:
						{
							if ((cmdHandler != null) && cmdHandler(worker, pse))
								break;

							pSeqPos++;
							continue;
						}
					}
				}

				pBlock = newBlk;
				if (pBlock < parentSS.NumBlocks())
					break;

				pSeqPos++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VerifyRange()
		{
			if (parentSS != null)
			{
				CheckBound<uint>(ref pSecPos, 0, parentSS.NumSections());
				CheckBound<PSeqNum>(ref psq, 0, parentSS.NumPlaySeqs());
				CheckBound<uint>(ref pSeqPos, 0, (uint)parentSS.PSeq(psq).Count);
				CheckBound<BlockNum>(ref pBlock, 0, parentSS.NumBlocks());
				CheckBound<LineNum>(ref pLine, 0, parentSS.Block(pBlock).Lines());
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CheckBound<T>(ref T value, T minimum, T maximum) where T : struct, IComparable
		{
			if (value.CompareTo(minimum) < 0)
				value = minimum;
			else
			{
				if (value.CompareTo(maximum) >= 0)
					value = (dynamic)maximum - 1;
			}
		}
		#endregion
	}
}
