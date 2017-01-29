using System;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using CoderLine.AspNet.WebApi.Versioning.Routing;
using Microsoft.Web.Http.Routing;

namespace CoderLine.AspNet.WebApi.Versioning
{
    /// <summary>
    /// Provides extension methods for the Web API related classes.
    /// </summary>
    public static class WebApiExtensions
    {
        /// <summary>
        /// Initializes API versioning with extended URL based versioning support. 
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use use service versioning.</param>
        public static void AddApiVersioningWithUrlSupport(this HttpConfiguration configuration) => configuration.AddApiVersioning(_ => { });

        /// <summary>
        /// Initializes API versioning with extended URL based versioning support. 
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use use service versioning.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        public static void AddApiVersioningWithUrlSupport(this HttpConfiguration configuration, Action<ApiVersioningUrlOptions> setupAction)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            configuration.AddApiVersioning(o =>
            {
                var options = new ApiVersioningUrlOptions(o);
                o.AssumeDefaultVersionWhenUnspecified = true;
                setupAction(options);

                if (options.VersioningOptions.AssumeDefaultVersionWhenUnspecified)
                {
                    var constraintResolver = options.ConstraintResolver ??
                                             new DefaultInlineConstraintResolver
                                             {
                                                 ConstraintMap = { ["apiVersion"] = typeof(ApiVersionRouteConstraint) }
                                             };

                    configuration.MapHttpAttributeRoutes(constraintResolver, new ApiVersionDirectRouteProvider());
                }
            });
        }

        /// <summary>
        /// Adds an <see cref="IApiExplorer"/> to the specified configuration that is aware of versionized routes. 
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use use service versioning.</param>
        public static void AddApiVersioningAwareApiExplorer(this HttpConfiguration configuration) => configuration.AddApiVersioningAwareApiExplorer(_ => { });

        /// <summary>
        /// Adds an <see cref="IApiExplorer"/> to the specified configuration that is aware of versionized routes. 
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use use service versioning.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        public static void AddApiVersioningAwareApiExplorer(this HttpConfiguration configuration, Action<ApiExplorerUrlOptions> setupAction)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            var options = new ApiExplorerUrlOptions();
            var services = configuration.Services;

            setupAction(options);
            services.Replace(typeof(IApiExplorer), new ApiVersionApiExplorer(configuration, options));
        }
    }
}
