/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Holds the tables for all available resamplers
	/// </summary>
	internal struct CResamplerSettings : IEquatable<CResamplerSettings>
	{
		public ResamplingMode SrcMode = Resampling.Default();
		public c_double gdWFirCutoff = 0.97;
		public WFirType gbWFirType = WFirType.Kaiser4T;
		public Resampling.AmigaFilter EmulateAmiga = Resampling.AmigaFilter.Off;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CResamplerSettings()
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator == (CResamplerSettings cmp1, CResamplerSettings cmp2)
		{
			return (cmp1.SrcMode == cmp2.SrcMode) && (cmp1.gdWFirCutoff == cmp2.gdWFirCutoff) && (cmp1.gbWFirType == cmp2.gbWFirType) && (cmp1.EmulateAmiga == cmp2.EmulateAmiga);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator != (CResamplerSettings cmp1, CResamplerSettings cmp2)
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
			return (obj is CResamplerSettings other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool Equals(CResamplerSettings other)
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

			hash.Add(SrcMode);
			hash.Add(gdWFirCutoff);
			hash.Add(gbWFirType);
			hash.Add(EmulateAmiga);

			return hash.ToHashCode();
		}
	}
}
