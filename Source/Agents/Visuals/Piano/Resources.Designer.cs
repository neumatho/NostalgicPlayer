﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Polycode.NostalgicPlayer.Agent.Visual.Piano {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Polycode.NostalgicPlayer.Agent.Visual.Piano.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap IDB_OCTAVE {
            get {
                object obj = ResourceManager.GetObject("IDB_OCTAVE", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Piano visual agent.
        ///Written by Thomas Neumann.
        ///
        ///This visual will show a piano. Each sample in the module will be assigned a unique color. When a sample is played, the color will be placed on the right key depending on the note playing.
        ///
        ///Note that this visual will not work for all kind of modules, only those where the player tells NostalgicPlayer what to play instead of giving the sample data directly..
        /// </summary>
        internal static string IDS_PIANO_DESCRIPTION_AGENT1 {
            get {
                return ResourceManager.GetString("IDS_PIANO_DESCRIPTION_AGENT1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Piano.
        /// </summary>
        internal static string IDS_PIANO_NAME {
            get {
                return ResourceManager.GetString("IDS_PIANO_NAME", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Piano.
        /// </summary>
        internal static string IDS_PIANO_NAME_AGENT1 {
            get {
                return ResourceManager.GetString("IDS_PIANO_NAME_AGENT1", resourceCulture);
            }
        }
    }
}
