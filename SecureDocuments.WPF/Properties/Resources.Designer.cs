﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SecureDocuments.WPF.Properties {
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
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SecureDocuments.WPF.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to &lt;html&gt;
        ///	&lt;head&gt;
        ///		&lt;meta content=&quot;text/html; charset=utf-8&quot; http-equiv=&quot;Content-Type&quot; /&gt;
        ///		&lt;meta http-equiv=&quot;Content-Security-Policy&quot; content=&quot;default-src &apos;self&apos;; script-src &apos;self&apos; ; style-src &apos;self&apos; &apos;unsafe-inline&apos; ; img-src * data:; font-src * data:;&quot;&gt;
        ///		&lt;style type=&quot;text/css&quot;&gt;
        ///			.circle {
        ///				height: 50px;
        ///				width: 50px;
        ///				background-color: #555;
        ///				border-radius: 50%;
        ///			}
        ///			.center {
        ///				display: block;
        ///				margin-left: auto;
        ///				margin-right: auto;
        ///			}
        ///		&lt;/style&gt;
        ///	&lt;/head&gt;
        ///
        ///	&lt;b [rest of string was truncated]&quot;;.
        /// </summary>
        public static string EMAIL_TAMPLATE {
            get {
                return ResourceManager.GetString("EMAIL_TAMPLATE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        /// </summary>
        public static System.Drawing.Icon Icon {
            get {
                object obj = ResourceManager.GetObject("Icon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Application folder path.
        /// </summary>
        public static string LoginView_ApplicationFolderPathText {
            get {
                return ResourceManager.GetString("LoginView_ApplicationFolderPathText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Indicate the application folder.
        /// </summary>
        public static string LoginView_ChooseApplicationFolderToolTipText {
            get {
                return ResourceManager.GetString("LoginView_ChooseApplicationFolderToolTipText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Login.
        /// </summary>
        public static string LoginView_LoginButtonText {
            get {
                return ResourceManager.GetString("LoginView_LoginButtonText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to At least 5 characters.
        /// </summary>
        public static string LoginView_PasswordHintText {
            get {
                return ResourceManager.GetString("LoginView_PasswordHintText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password.
        /// </summary>
        public static string LoginView_PasswordText {
            get {
                return ResourceManager.GetString("LoginView_PasswordText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type your credentials.
        /// </summary>
        public static string LoginView_TypeYourCredentialsLabelText {
            get {
                return ResourceManager.GetString("LoginView_TypeYourCredentialsLabelText", resourceCulture);
            }
        }
    }
}
