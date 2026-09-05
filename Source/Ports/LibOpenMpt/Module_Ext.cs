/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Extensions;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt
{
	/// <summary>
	/// 
	/// </summary>
	public class Module_Ext : Module
	{
		private readonly Module_Ext_Impl ext_Impl;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Module_Ext(Stream stream, Guid? formatId = null, map<string, string> ctls = null)
		{
			ext_Impl = new Module_Ext_Impl(stream, formatId, ctls ?? new map<string, string>());
			Set_Impl(ext_Impl);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve a libopenmpt extension
		/// </summary>
		/********************************************************************/
		public IExtension Get_Interface(string interface_Id)
		{
			return ext_Impl.Get_Interface(interface_Id);
		}
	}
}
