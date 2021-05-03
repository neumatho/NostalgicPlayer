/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Used to tell visual agents about new sample data
	/// </summary>
	public class NewSampleData
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NewSampleData(int[] buffer, bool stereo)
		{
			SampleData = buffer;
			Stereo = stereo;
		}



		/********************************************************************/
		/// <summary>
		/// An array with the sample data
		/// </summary>
		/********************************************************************/
		public int[] SampleData
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Tells if the sample data is in stereo or mono
		/// </summary>
		/********************************************************************/
		public bool Stereo
		{
			get;
		}
	}
}
