/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using EnvelopeNode_tick_t = System.UInt16;
global using EnvelopeNode_value_t = System.Byte;

using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Instrument nodes
	/// </summary>
	internal class EnvelopeNode : IEquatable<EnvelopeNode>, IDeepCloneable<EnvelopeNode>
	{
		/// <summary>
		/// Envelope node position (x axis)
		/// </summary>
		public EnvelopeNode_tick_t Tick = 0;

		/// <summary>
		/// Envelope node value (y axis)
		/// </summary>
		public EnvelopeNode_value_t Value = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EnvelopeNode()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EnvelopeNode(EnvelopeNode_tick_t tick, EnvelopeNode_value_t value)
		{
			Tick = tick;
			Value = value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (EnvelopeNode me, EnvelopeNode other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return (me.Tick == other.Tick) && (me.Value == other.Value);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (EnvelopeNode me, EnvelopeNode other)
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
			return (obj is EnvelopeNode other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(EnvelopeNode other)
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

			hash.Add(Tick);
			hash.Add(Value);

			return hash.ToHashCode();
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public EnvelopeNode MakeDeepClone()
		{
			return (EnvelopeNode)MemberwiseClone();
		}
	}
}
