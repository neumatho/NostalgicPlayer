/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Tempo swing determines how much every row in modern tempo mode contributes to a beat
	/// </summary>
	internal class TempoSwing : vector<uint32>, IDeepCloneable<TempoSwing>, ICopyTo<TempoSwing>
	{
		public const uint32 Unity = 1U << 24;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public new void resize(size_t newSize, uint32 val = Unity)
		{
			base.resize(newSize, val);
			Normalize();
		}



		/********************************************************************/
		/// <summary>
		/// Normalize the tempo swing coefficients so that they add up to
		/// exactly the specified tempo again
		/// </summary>
		/********************************************************************/
		public void Normalize()
		{
			if (empty())
				return;

			uint64 sum = 0;

			for (size_t idx = 0; idx < size(); idx++)
			{
				uint32 i = this[idx];

				OpenMpt.Limit(ref i, Unity / 4U, Unity * 4U);
				sum += i;

				this[idx] = i;
			}

			sum /= size();

			int64 remain = (int64)(Unity * size());

			for (size_t idx = 0; idx < size(); idx++)
			{
				uint32 i = this[idx];

				i = Util.MulDivR_Unsigned(i, Unity, (uint32)sum);
				remain -= i;

				this[idx] = i;
			}

			at(0) += (uint32)remain;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public override TempoSwing MakeDeepClone()
		{
			TempoSwing clone = new TempoSwing();

			CopyTo(clone);

			return clone;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(TempoSwing destination)
		{
			base.CopyTo(destination);
		}
	}
}
