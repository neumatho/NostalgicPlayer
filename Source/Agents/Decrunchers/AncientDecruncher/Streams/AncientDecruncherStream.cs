/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Streams
{
	/// <summary>
	/// Implementation of the decruncher stream
	/// </summary>
	internal class AncientDecruncherStream : DecruncherStream
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AncientDecruncherStream(AncientStream wrapperStream) : base(wrapperStream, false)
		{
		}

		#region DecruncherStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the decrunched data
		/// </summary>
		/********************************************************************/
		public override int GetDecrunchedLength()
		{
			return (int)wrapperStream.Length;
		}
		#endregion
	}
}
