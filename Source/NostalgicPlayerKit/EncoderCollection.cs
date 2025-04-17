/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
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
		/// Return the encoder to decode PC DOS characters
		/// </summary>
		/********************************************************************/
		public static Encoding Dos
		{
			get;
		} = new Ibm865();



		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode Macintosh characters
		/// </summary>
		/********************************************************************/
		public static Encoding Macintosh
		{
			get;
		} = new MacintoshRoman();



		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode Atari characters
		/// </summary>
		/********************************************************************/
		public static Encoding Atari
		{
			get;
		} = new Atascii();



		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode Acorn Archimedes characters
		/// </summary>
		/********************************************************************/
		public static Encoding Archimedes
		{
			get;
		} = new RiskOs();



		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode Microsoft Windows code page 1252
		/// characters
		/// </summary>
		/********************************************************************/
		public static Encoding Win1252
		{
			get;
		} = new Win1252();



		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode ISO-8859-1 characters
		/// </summary>
		/********************************************************************/
		public static Encoding Iso8859_1
		{
			get;
		} = new Iso8859_1();
	}
}
