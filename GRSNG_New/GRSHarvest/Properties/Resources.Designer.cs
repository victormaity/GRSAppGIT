﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GRSHarvest.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GRSHarvest.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to &lt;html xmlns:v=&quot;urn:schemas-microsoft-com:vml&quot;
        ///xmlns:o=&quot;urn:schemas-microsoft-com:office:office&quot;
        ///xmlns:w=&quot;urn:schemas-microsoft-com:office:word&quot;
        ///xmlns:m=&quot;http://schemas.microsoft.com/office/2004/12/omml&quot;
        ///xmlns=&quot;http://www.w3.org/TR/REC-html40&quot;&gt;
        ///
        ///&lt;head&gt;
        ///&lt;meta http-equiv=Content-Type content=&quot;text/html; charset=unicode&quot;&gt;
        ///&lt;meta name=ProgId content=Word.Document&gt;
        ///&lt;meta name=Generator content=&quot;Microsoft Word 14&quot;&gt;
        ///&lt;meta name=Originator content=&quot;Microsoft Word 14&quot;&gt;
        ///&lt;title&gt;GLOBAL REPORTING SYSTEM - Automa [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string EmailTemplate {
            get {
                return ResourceManager.GetString("EmailTemplate", resourceCulture);
            }
        }
    }
}
