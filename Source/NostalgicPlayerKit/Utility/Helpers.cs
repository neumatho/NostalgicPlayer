/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Different helper methods
	/// </summary>
	public static class Helpers
	{
		/********************************************************************/
		/// <summary>
		/// Create an array of the type given and fill it will newly created
		/// objects of same type
		/// </summary>
		/********************************************************************/
		public static T[] InitializeArray<T>(int length) where T : new()
		{
			T[] array = new T[length];
			for (int i = 0; i < length; i++)
				array[i] = new T();

			return array;
		}
	}
}
