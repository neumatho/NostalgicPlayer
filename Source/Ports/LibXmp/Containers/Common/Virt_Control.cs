/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Virt_Control : IDeepCloneable<Virt_Control>
	{
		/// <summary>
		/// Number of tracks
		/// </summary>
		public c_int Num_Tracks { get; set; }

		/// <summary>
		/// Number of virtual channels
		/// </summary>
		public c_int Virt_Channels { get; set; }

		/// <summary>
		/// Number of voices currently in use
		/// </summary>
		public c_int Virt_Used { get; set; }

		/// <summary>
		/// Number of sound card voices
		/// </summary>
		public c_int MaxVoc { get; set; }

		public Virt_Channel[] Virt_Channel { get; set; }

		public Mixer_Voice[] Voice_Array { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Virt_Control MakeDeepClone()
		{
			Virt_Control clone = (Virt_Control)MemberwiseClone();

			clone.Virt_Channel = ArrayHelper.CloneObjectArray(Virt_Channel);
			clone.Voice_Array = ArrayHelper.CloneObjectArray(Voice_Array);

			return clone;
		}
	}
}
