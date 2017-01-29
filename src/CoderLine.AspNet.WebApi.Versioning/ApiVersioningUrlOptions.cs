using System.Web.Http.Routing;
using Microsoft.Web.Http.Versioning;

namespace CoderLine.AspNet.WebApi.Versioning
{
    /// <summary>
    /// Represents the possible options related to URL based API versioning. 
    /// </summary>
    public class ApiVersioningUrlOptions
    {
        /// <summary>
        /// The Gets or sets the options for initializing the API versioning module. 
        /// </summary>
        public ApiVersioningOptions VersioningOptions { get; }

        /// <summary>
        /// Gets or sets the constraint resoler that should be used for initializing the 
        /// attribute routing. If <code>null</code> a default constraint resolver with {apiVersion} as 
        /// route constraint will be created.  
        /// </summary>
        public IInlineConstraintResolver ConstraintResolver { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersioningUrlOptions"/> class.
        /// </summary>
        /// <param name="versioningOptions">The API versioning options</param>
        public ApiVersioningUrlOptions(ApiVersioningOptions versioningOptions)
        {
            VersioningOptions = versioningOptions;
        }
    }
}