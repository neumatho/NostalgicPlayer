/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundDsp.Containers
{
	/// <summary>
	/// ID3DL2 reverb presets
	/// </summary>
	internal class SndMixReverbProperties : IEquatable<SndMixReverbProperties>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SndMixReverbProperties(int32 lRoom, int32 lRoomHF, c_float flDecayTime, c_float flDecayHFRatio, int32 lReflections, c_float flReflectionsDelay, int32 lReverb, c_float flReverbDelay, c_float flDiffusion, c_float flDensity)
		{
			this.lRoom = lRoom;
			this.lRoomHF = lRoomHF;
			this.flDecayTime = flDecayTime;
			this.flDecayHFRatio = flDecayHFRatio;
			this.lReflections = lReflections;
			this.flReflectionsDelay = flReflectionsDelay;
			this.lReverb = lReverb;
			this.flReverbDelay = flReverbDelay;
			this.flDiffusion = flDiffusion;
			this.flDensity = flDensity;
		}



		/********************************************************************/
		/// <summary>
		/// [-10000, 0]      default: -10000 mB
		/// </summary>
		/********************************************************************/
		public int32 lRoom { get; }



		/********************************************************************/
		/// <summary>
		/// [-10000, 0]      default: 0 mB
		/// </summary>
		/********************************************************************/
		public int32 lRoomHF { get; }



		/********************************************************************/
		/// <summary>
		/// [0.1, 20.0]      default: 1.0 s
		/// </summary>
		/********************************************************************/
		public c_float flDecayTime { get; }



		/********************************************************************/
		/// <summary>
		/// [0.1, 2.0]       default: 0.5
		/// </summary>
		/********************************************************************/
		public c_float flDecayHFRatio { get; }



		/********************************************************************/
		/// <summary>
		/// [-10000, 1000]   default: -10000 mB
		/// </summary>
		/********************************************************************/
		public int32 lReflections { get; }



		/********************************************************************/
		/// <summary>
		/// [0.0, 0.3]       default: 0.02 s
		/// </summary>
		/********************************************************************/
		public c_float flReflectionsDelay { get; }



		/********************************************************************/
		/// <summary>
		/// [-10000, 2000]   default: -10000 mB
		/// </summary>
		/********************************************************************/
		public int32 lReverb { get; }



		/********************************************************************/
		/// <summary>
		/// [0.0, 0.1]       default: 0.04 s
		/// </summary>
		/********************************************************************/
		public c_float flReverbDelay { get; }



		/********************************************************************/
		/// <summary>
		/// [0.0, 100.0]     default: 100.0 %
		/// </summary>
		/********************************************************************/
		public c_float flDiffusion { get; }



		/********************************************************************/
		/// <summary>
		/// [0.0, 100.0]     default: 100.0 %
		/// </summary>
		/********************************************************************/
		public c_float flDensity { get; }



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator == (SndMixReverbProperties cmp1, SndMixReverbProperties cmp2)
		{
			if (ReferenceEquals(cmp1, cmp2))
				return true;

			if ((cmp1 is null) || (cmp2 is null))
				return false;

			return (cmp1.lRoom == cmp2.lRoom) && (cmp1.lRoomHF == cmp2.lRoomHF) && (cmp1.flDecayTime == cmp2.flDecayTime) && (cmp1.flDecayHFRatio == cmp2.flDecayHFRatio) && (cmp1.lReflections == cmp2.lReflections) && (cmp1.flReflectionsDelay == cmp2.flReflectionsDelay) && (cmp1.lReverb == cmp2.lReverb) && (cmp1.flReverbDelay == cmp2.flReverbDelay) && (cmp1.flDiffusion == cmp2.flDiffusion) && (cmp1.flDensity == cmp2.flDensity);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator != (SndMixReverbProperties cmp1, SndMixReverbProperties cmp2)
		{
			return !(cmp1 == cmp2);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is SndMixReverbProperties other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool Equals(SndMixReverbProperties other)
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

			hash.Add(lRoom);
			hash.Add(lRoomHF);
			hash.Add(flDecayTime);
			hash.Add(flDecayHFRatio);
			hash.Add(lReflections);
			hash.Add(flReflectionsDelay);
			hash.Add(lReverb);
			hash.Add(flReverbDelay);
			hash.Add(flDiffusion);
			hash.Add(flDensity);

			return hash.ToHashCode();
		}
	}
}
