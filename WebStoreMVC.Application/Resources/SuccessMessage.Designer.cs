﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebStoreMVC.Application.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SuccessMessage {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SuccessMessage() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WebStoreMVC.Application.Resources.SuccessMessage", typeof(SuccessMessage).Assembly);
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
        ///   Looks up a localized string similar to Создание ролей выполнено.
        /// </summary>
        internal static string CreatingRolesIsDone {
            get {
                return ResourceManager.GetString("CreatingRolesIsDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Пользователь создан успешно.
        /// </summary>
        internal static string CreatingUserIsDone {
            get {
                return ResourceManager.GetString("CreatingUserIsDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Админ стал обычным пользователем.
        /// </summary>
        internal static string DowngradeAdminToUser {
            get {
                return ResourceManager.GetString("DowngradeAdminToUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Выход выполнен.
        /// </summary>
        internal static string LogoutIsDone {
            get {
                return ResourceManager.GetString("LogoutIsDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Товары получены.
        /// </summary>
        internal static string ProductsAreReceived {
            get {
                return ResourceManager.GetString("ProductsAreReceived", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Пользователь стал админом.
        /// </summary>
        internal static string UpgradeUserToAdmin {
            get {
                return ResourceManager.GetString("UpgradeUserToAdmin", resourceCulture);
            }
        }
    }
}