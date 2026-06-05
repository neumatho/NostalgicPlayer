/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using PartEffectEntry = Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.EffectEntry<Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.PartEffect, byte>;
global using ArpeggioEffectEntry = Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.EffectEntry<Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.ArpeggioEffect, byte>;
global using InstrumentEffect1Entry = Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.EffectEntry<Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.InstrumentEffect1, Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.InstrumentFlag>;
global using InstrumentEffect2Entry = Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.EffectEntry<Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.InstrumentEffect2, Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers.MixFlag>;

using System;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// A single effect with argument
	/// </summary>
	internal class EffectEntry<E, A> : IDeepCloneable<EffectEntry<E, A>> where E : Enum where A : struct
	{
		/// <summary>
		/// 
		/// </summary>
		public E Effect { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public A Argument { get; set; }



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public EffectEntry<E, A> MakeDeepClone()
		{
			return (EffectEntry<E, A>)MemberwiseClone();
		}
	}
}
