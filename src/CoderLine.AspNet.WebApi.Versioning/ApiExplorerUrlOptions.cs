using System.Web.Http.Description;

namespace CoderLine.AspNet.WebApi.Versioning
{
    /// <summary>
    /// Represents the possible options related to the <see cref="IApiExplorer"/> providing the descriptions for 
    /// the defined API versions
    /// </summary>
    public class ApiExplorerUrlOptions
    {
        /// <summary>
        /// Gets or sets whether the <see cref="IApiExplorer"/> will provide descriptors for the 
        /// default version API actions. (default: true)
        /// </summary>
        /// <remarks>
        /// If set to <code>true</code> default version routes as they are added by <see cref="WebApiExtensions.AddApiVersioningWithUrlSupport(System.Web.Http.HttpConfiguration)"/>
        /// are added to the API descriptions.
        /// </remarks>
        public bool IncludeDefaultVersion { get; set; }

        /// <summary>
        /// Gets or sets whether the <see cref="IApiExplorer"/> should prefer a short version of the version 
        /// instead of the full &lt;Major&gt;.&lt;Minor&gt; variant. (default: false)
        /// </summary>
        /// <remarks>
        /// If set to <code>true</code> the <see cref="IApiExplorer"/> will provide API descriptions preferring 
        /// shortened versions by omitting the minor part. e.g. /v3.0/ will be shortened to /v3/
        /// </remarks>
        public bool PreferShortHandVersion { get; set; }

        /// <summary>
        /// Initialize a new instance of the <see cref="ApiExplorerUrlOptions"/> class. 
        /// </summary>
        public ApiExplorerUrlOptions()
        {
            IncludeDefaultVersion = true;
        }
    }
}