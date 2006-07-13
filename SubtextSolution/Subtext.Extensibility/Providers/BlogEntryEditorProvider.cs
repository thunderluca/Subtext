#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at SourceForge at http://sourceforge.net/projects/subtext
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Configuration.Provider;
using System.Web.Configuration;

namespace Subtext.Extensibility.Providers
{
	/// <summary>
	/// Provider for classes that implement the rich text editor 
	/// to edit text visually.
	/// </summary>
	public abstract class BlogEntryEditorProvider : System.Configuration.Provider.ProviderBase
	{

        private static BlogEntryEditorProvider _provider = null;
        private static GenericProviderCollection<BlogEntryEditorProvider> _providers = null;
        private static object _lock = new object();

        public static BlogEntryEditorProvider Instance()
        {
            LoadProviders();
            return _provider;
        }

        private static void LoadProviders()
        {
            // Avoid claiming lock if providers are already loaded
            if (_provider == null)
            {
                lock (_lock)
                {
                    // Do this again to make sure _provider is still null
                    if (_provider == null)
                    {
                        // Get a reference to the <BlogEntryEditor> section
                        ProviderSectionHandler section = (ProviderSectionHandler)
                            WebConfigurationManager.GetSection
                            ("BlogEntryEditor");

                        // Load registered providers and point _provider
                        // to the default provider
                        _providers = new GenericProviderCollection<BlogEntryEditorProvider>();
                        ProvidersHelper.InstantiateProviders
                            (section.Providers, _providers,
                            typeof(BlogEntryEditorProvider));
                        _provider = _providers[section.DefaultProvider];

                        if (_provider == null)
                            throw new ProviderException
                                ("Unable to load default BlogEntryEditorProvider");
                    }
                }
            }
        }



		/// <summary>
		/// Return the RichTextEditorControl to be displayed inside the page
		/// </summary>
		public abstract Control RichTextEditorControl{get;}
        /// <summary>
        /// Id of the control
        /// </summary>
		public abstract String ControlID{get;set;}
        /// <summary>
        /// The content of the area
        /// </summary>
		public abstract String Text{get;set;}
        /// <summary>
        /// The content of the area, but XHTML converted
        /// </summary>
		public abstract String Xhtml{get;}
        /// <summary>
        /// Width of the editor
        /// </summary>
		public abstract System.Web.UI.WebControls.Unit Width{get;set;}
        /// <summary>
        /// Height of the editor
        /// </summary>
		public abstract System.Web.UI.WebControls.Unit Height{get;set;}

        /// <summary>
        /// Initializes the Control to be displayed
        /// </summary>
		public abstract void InitializeControl();

	}
}