/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp
{
	/// <summary>
	/// Base class for SID builders
	/// </summary>
	public abstract class SidBuilder
	{
		private readonly string name;

		/// <summary></summary>
		protected string errorBuffer;

		internal readonly HashSet<SidEmu> sidObjs = new HashSet<SidEmu>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected SidBuilder(string name)
		{
			this.name = name;
			errorBuffer = Resources.IDS_SID_NA;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal abstract SidEmu Create();
		#endregion

		/********************************************************************/
		/// <summary>
		/// Find a free SID of the required specs
		/// </summary>
		/********************************************************************/
		internal SidEmu Lock(EventScheduler scheduler, SidConfig.sid_model_t model, bool digiBoost)
		{
			// Create new emu
			SidEmu sid = Create();
			if (sid != null)
			{
				if (sid.Lock(scheduler))
				{
					sid.Model(model, digiBoost);
					sidObjs.Add(sid);
					return sid;
				}
			}

			// Unable to locate a free SID
			errorBuffer = string.Format(Resources.IDS_SID_ERR_NO_SIDS, Name());

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Release this SID
		/// </summary>
		/********************************************************************/
		internal void Unlock(SidEmu device)
		{
			if (sidObjs.TryGetValue(device, out SidEmu so))
			{
				so.Unlock();

				// Should we cache these for later use?
				sidObjs.Remove(device);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Error message
		/// </summary>
		/********************************************************************/
		public string Error()
		{
			return errorBuffer;
		}



		/********************************************************************/
		/// <summary>
		/// Get the builder's name
		/// </summary>
		/********************************************************************/
		protected string Name()
		{
			return name;
		}
	}
}
