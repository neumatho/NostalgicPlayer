/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// 
	/// </summary>
	internal class EffectGroup
	{
		private readonly SubSong subSong;

		private sbyte stereoSeparator;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EffectGroup(SubSong ss)
		{
			subSong = ss;
		}



		/********************************************************************/
		/// <summary>
		/// Set the stereo separation parameter
		/// </summary>
		/********************************************************************/
		public void SetStereoSeparation(sbyte separator)
		{
			stereoSeparator = separator;
		}



		/********************************************************************/
		/// <summary>
		/// Will add DSP effect to the mixed output
		/// </summary>
		/********************************************************************/
		public void DoEffects(int[] dest, int todo, bool stereo)
		{
			if (stereo && (stereoSeparator != 0) && subSong.GetStereo())
				DoStereoSeparation(dest, todo);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Apply stereo separation to the output
		/// </summary>
		/********************************************************************/
		private void DoStereoSeparation(int[] dest, int todo)
		{
			todo /= 2;

			if (stereoSeparator < 0)
			{
				int shift = stereoSeparator + 5;

				for (int i = 0; i < todo; i += 2)
				{
					int left = dest[i];
					int right = dest[i + 1];

					dest[i] += right >> shift;
					dest[i + 1] += left >> shift;
				}
			}
			else
			{
				int shift = 5 - stereoSeparator;

				for (int i = 0; i < todo; i += 2)
				{
					int left = dest[i];
					int right = dest[i + 1];

					dest[i] -= right >> shift;
					dest[i + 1] -= left >> shift;
				}
			}
		}
		#endregion
	}
}
