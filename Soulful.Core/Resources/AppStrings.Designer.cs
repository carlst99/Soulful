﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Soulful.Core.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class AppStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Soulful.Core.Resources.AppStrings", typeof(AppStrings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You&apos;ve been exposed.
        /// </summary>
        public static string App_Subtitle {
            get {
                return ResourceManager.GetString("App_Subtitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Soulful.
        /// </summary>
        public static string App_Title {
            get {
                return ResourceManager.GetString("App_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Browse Cards.
        /// </summary>
        public static string Command_BrowseCards {
            get {
                return ResourceManager.GetString("Command_BrowseCards", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Join Game.
        /// </summary>
        public static string Command_JoinGame {
            get {
                return ResourceManager.GetString("Command_JoinGame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Start Game.
        /// </summary>
        public static string Command_StartGame {
            get {
                return ResourceManager.GetString("Command_StartGame", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Black Cards.
        /// </summary>
        public static string Header_BlackCards {
            get {
                return ResourceManager.GetString("Header_BlackCards", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Card Browser.
        /// </summary>
        public static string Header_CardBrowser {
            get {
                return ResourceManager.GetString("Header_CardBrowser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to White Cards.
        /// </summary>
        public static string Header_WhiteCards {
            get {
                return ResourceManager.GetString("Header_WhiteCards", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Go Back.
        /// </summary>
        public static string ToolTip_NavigateBack {
            get {
                return ResourceManager.GetString("ToolTip_NavigateBack", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Refresh Pin.
        /// </summary>
        public static string ToolTip_RefreshGamePin {
            get {
                return ResourceManager.GetString("ToolTip_RefreshGamePin", resourceCulture);
            }
        }
    }
}
