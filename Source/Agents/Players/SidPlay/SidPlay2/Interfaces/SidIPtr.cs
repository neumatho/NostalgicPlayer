/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Reflection;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces
{
	/// <summary>
	/// An interface ptr class to automatically
	/// handle querying and releasing of the interface
	/// </summary>
	internal class SidIPtr<TInterface> : ISidUnknown where TInterface : class, ISidUnknown
	{
		private readonly IId iid;
		private TInterface @if;
		private ISidUnknown unknown;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidIPtr(SidIPtr<TInterface> unknown)
		{
			iid = (IId)typeof(TInterface).GetMethod("IId", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
			Init(unknown);
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidIPtr(ISidUnknown unknown)
		{
			iid = (IId)typeof(TInterface).GetMethod("IId", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
			Init(unknown);
		}

		#region ISidUnknown implementation
		/********************************************************************/
		/// <summary>
		/// Return an unique ID for this implementation
		/// </summary>
		/********************************************************************/
		public IId IId()
		{
			return iid;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ISidUnknown IUnknown()
		{
			return unknown;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool IQuery(IId iid, out object implementation)
		{
			implementation = null;

			if (iid == this.iid)
			{
				if (unknown != null)
				{
					implementation = @if;
					return true;
				}

				return false;
			}

			if (unknown != null)
				return unknown.IQuery(iid, out implementation);

			return false;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return the assigned object
		/// </summary>
		/********************************************************************/
		public TInterface Obj => @if;

		#region Operators
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator ==(SidIPtr<TInterface> obj1, ISidUnknown obj2)
		{
			return obj1?.unknown == obj2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator !=(SidIPtr<TInterface> obj1, ISidUnknown obj2)
		{
			return obj1?.unknown != obj2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator ==(ISidUnknown obj1, SidIPtr<TInterface> obj2)
		{
			return obj1 == obj2?.unknown;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator !=(ISidUnknown obj1, SidIPtr<TInterface> obj2)
		{
			return obj1 != obj2?.unknown;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator ==(SidIPtr<TInterface> obj1, SidIPtr<TInterface> obj2)
		{
			return obj1?.unknown == obj2?.unknown;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator !=(SidIPtr<TInterface> obj1, SidIPtr<TInterface> obj2)
		{
			return obj1?.unknown != obj2?.unknown;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize the object
		/// </summary>
		/********************************************************************/
		private void Init(ISidUnknown unknown)
		{
			this.unknown = null;
			Assign(unknown);
		}



		/********************************************************************/
		/// <summary>
		/// Assign the given object to the pointer
		/// </summary>
		/********************************************************************/
		private void Assign(ISidUnknown unknown)
		{
			this.unknown = null;
			@if = null;

			if (unknown != null)
			{
				ISidUnknown u = unknown;

				for (;;)
				{
					@if = null;

					bool result = u.IQuery(iid, out object obj);
					@if = obj as TInterface;

					if (result)
					{
						if (@if != null)
							break;

						throw new Exception("No interface present");
					}
					else if (@if == null)
						return;

					u = @if.IUnknown();		// Chain
				}

				this.unknown = unknown.IUnknown();
			}
		}
		#endregion
	}
}
