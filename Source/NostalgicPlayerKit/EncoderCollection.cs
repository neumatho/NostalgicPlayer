/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Text;
using Polycode.NostalgicPlayer.Kit.Encoders;

namespace Polycode.NostalgicPlayer.Kit
{
	/// <summary>
	/// This class in the entry point to retrieve different character encoders
	/// </summary>
	public static class EncoderCollection
	{
		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode Amiga characters
		/// </summary>
		/********************************************************************/
		public static Encoding Amiga
		{
			get;
		} = new Amiga();



		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode code page 850 (Western Europe (DOS))
		/// characters
		/// </summary>
		/********************************************************************/
		public static Encoding Ibm850
		{
			get;
		} = new Ibm850();



		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode Macintosh Roman characters
		/// </summary>
		/********************************************************************/
		public static Encoding MacintoshRoman
		{
			get;
		} = new MacintoshRoman();
	}
}
