/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// Callback event
	/// </summary>
	internal class EventCallback : Event
	{
		public delegate void Callback();

		private readonly Callback callback;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EventCallback(string name, Callback callback) : base(name)
		{
			this.callback = callback;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the event
		/// </summary>
		/********************************************************************/
		public override void DoEvent()
		{
			callback();
		}
	}
}
