/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cia
{
	/// <summary>
	/// This is the timer A of this CIA
	///
	/// Author: Ken Händel
	/// </summary>
	internal sealed class TimerA : Timer
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TimerA(EventScheduler scheduler, Mos652x parent) : base("CIA Timer A", scheduler, parent)
		{
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Signal timer underflow
		/// </summary>
		/********************************************************************/
		protected override void Underflow()
		{
			parent.UnderflowA();
		}



		/********************************************************************/
		/// <summary>
		/// Handle the serial port
		/// </summary>
		/********************************************************************/
		protected override void SerialPort()
		{
			parent.HandleSerialPort();
		}
		#endregion
	}
}
