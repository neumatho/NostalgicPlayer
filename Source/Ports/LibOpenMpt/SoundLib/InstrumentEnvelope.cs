/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Instrument envelopes
	/// </summary>
	internal class InstrumentEnvelope : vector<EnvelopeNode>, IDeepCloneable<InstrumentEnvelope>, ICopyTo<InstrumentEnvelope>
	{
		/// <summary>
		/// Envelope flags
		/// </summary>
		public EnvelopeFlags dwFlags;

		/// <summary>
		/// Loop start node
		/// </summary>
		public uint8 nLoopStart = 0;

		/// <summary>
		/// Loop end node
		/// </summary>
		public uint8 nLoopEnd = 0;

		/// <summary>
		/// Sustain start node
		/// </summary>
		public uint8 nSustainStart = 0;

		/// <summary>
		/// Sustain end node
		/// </summary>
		public uint8 nSustainEnd = 0;

		/// <summary>
		/// Release node
		/// </summary>
		public uint8 nReleaseNode = Snd_Def.Env_Release_Node_Unset;

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 LastPoint()
		{
			return (uint8)(Math.Max(size(), 1) - 1);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void push_back(EnvelopeNode_tick_t tick, EnvelopeNode_value_t value)
		{
			emplace_back(new EnvelopeNode(tick, value));
		}



		/********************************************************************/
		/// <summary>
		/// Convert envelope data between various formats
		/// </summary>
		/********************************************************************/
		public void Convert(ModType fromType, ModType toType)//XX 20
		{
			if (((fromType & ModType.Xm) == 0) && ((toType & ModType.Xm) != 0))
			{
				// IT / MPTM -> XM: Expand loop by one tick, convert sustain loops to sustain points, remove carry flag
				nSustainStart = nSustainEnd;
				dwFlags.Reset(EnvelopeFlags.Carry);

				if ((nLoopEnd > nLoopStart) && dwFlags.Test(EnvelopeFlags.Loop))
				{
					for (uint32 node = nLoopEnd; node < size(); node++)
						at(node).Tick++;
				}
			}
			else if (((fromType & ModType.Xm) != 0) && ((toType & ModType.Xm) == 0))
			{
				if ((nSustainStart > nLoopEnd) && dwFlags.Test(EnvelopeFlags.Loop))
				{
					// In the IT format, the sustain loop is always considered before the envelope loop.
					// In the XM format, whichever of the two is encountered first is considered.
					// So we have to disable the sustain loop if it was behind the normal loop
					dwFlags.Reset(EnvelopeFlags.Sustain);
				}

				if (!dwFlags.Test(EnvelopeFlags.Loop | EnvelopeFlags.Sustain))
				{
					// XM has no automatic fade-out behaviour at the end of the envelope
					dwFlags.Set(EnvelopeFlags.Sustain);
					nSustainStart = nSustainEnd = LastPoint();
				}

				// XM -> IT / MPTM: Shorten loop by one tick by inserting bogus point
				if ((nLoopEnd > nLoopStart) && dwFlags.Test(EnvelopeFlags.Loop) && (nLoopEnd < size()))
				{
					if ((at(nLoopEnd).Tick - 1) > at(nLoopEnd - 1).Tick)
					{
						// Insert an interpolated point just before the loop point
						EnvelopeNode_tick_t tick = (EnvelopeNode_tick_t)(at(nLoopEnd).Tick - 1U);
						EnvelopeNode_value_t interpolatedValue = (EnvelopeNode_value_t)GetValueFromPosition(tick, 64);
						insert(begin() + nLoopEnd, new EnvelopeNode(tick, interpolatedValue));
					}
					else
					{
						// There is already a point before the loop point: Use it as new loop end
						nLoopEnd--;
					}
				}
			}

			if (toType != ModType.Mpt)
				nReleaseNode = Snd_Def.Env_Release_Node_Unset;
		}



		/********************************************************************/
		/// <summary>
		/// Get envelope value at a given tick. Assumes that the envelope
		/// data is in rage [0, rangeIn], returns value in range [0, rangeOut]
		/// </summary>
		/********************************************************************/
		public int32 GetValueFromPosition(c_int position, int32 rangeOut, int32 rangeIn = Snd_Def.Envelope_Max)//XX 77
		{
			if (empty())
				return 0;

			uint32 pt = LastPoint();
			const int32 Env_Precision = 1 << 16;

			// Checking where current 'tick' is relative to the envelope points
			for (uint32 i = 0; i < LastPoint(); i++)
			{
				if (position <= at(i).Tick)
				{
					pt = i;
					break;
				}
			}

			c_int x2 = at(pt).Tick;
			int32 value = 0;

			if (position >= x2)
			{
				// Case: current 'tick' is on a envelope point
				value = at(pt).Value * Env_Precision / rangeIn;
			}
			else
			{
				// Case: current 'tick' is between two envelope points
				c_int x1 = 0;

				if (pt != 0)
				{
					// Get previous node's value and tick
					value = at(pt - 1).Value * Env_Precision / rangeIn;
					x1 = at(pt - 1).Tick;
				}

				if ((x2 > x1) && (position > x1))
				{
					// Linear approximation between the points;
					// f(x + d) ~ f(x) + f'(x) * d, where f'(x) = (y2 - y1) / (x2 - x1)
					value += Util.MulDiv(position - x1, ((at(pt).Value * Env_Precision / rangeIn) - value), x2 - x1);
				}
			}

			OpenMpt.Limit(ref value, 0, Env_Precision);

			return ((value * rangeOut) + (Env_Precision / 2)) / Env_Precision;
		}



		/********************************************************************/
		/// <summary>
		/// Ensure that ticks are ordered in increasing order and values are
		/// within allowed range
		/// </summary>
		/********************************************************************/
		public void Sanitize(uint8 maxValue = Snd_Def.Envelope_Max)//XX 127
		{
			if (!empty())
			{
				front().Tick = 0;
				OpenMpt.LimitMax(ref front().Value, maxValue);

				for (var it = begin() + 1; it != end(); it++)
				{
					it[0].Tick = Math.Max(it[0].Tick, it[-1].Tick);
					OpenMpt.LimitMax(ref it[0].Value, maxValue);
				}

				OpenMpt.LimitMax(ref nLoopEnd, LastPoint());
				OpenMpt.LimitMax(ref nLoopStart, nLoopEnd);
				OpenMpt.LimitMax(ref nSustainEnd, LastPoint());
				OpenMpt.LimitMax(ref nSustainStart, nSustainEnd);

				if (nReleaseNode != Snd_Def.Env_Release_Node_Unset)
					OpenMpt.LimitMax(ref nReleaseNode, LastPoint());
			}
			else
			{
				nLoopStart = 0;
				nLoopEnd = 0;
				nSustainStart = 0;
				nSustainEnd = 0;
				nReleaseNode = Snd_Def.Env_Release_Node_Unset;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public override InstrumentEnvelope MakeDeepClone()
		{
			InstrumentEnvelope clone = new InstrumentEnvelope();

			CopyTo(clone);

			return clone;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(InstrumentEnvelope destination)
		{
			destination.dwFlags = dwFlags;
			destination.nLoopStart = nLoopStart;
			destination.nLoopEnd = nLoopEnd;
			destination.nSustainStart = nSustainStart;
			destination.nSustainEnd = nSustainEnd;
			destination.nReleaseNode = nReleaseNode;

			base.CopyTo(destination);
		}
	}
}
