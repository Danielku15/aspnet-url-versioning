using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.Web.Http.Routing;

namespace CoderLine.AspNet.WebApi.Versioning.Routing
{
    /// <summary>
    /// Represents the logic for creating the default version routes to actions which have the 
    /// the version within URL segments.
    /// </summary>
    public class ApiVersionDirectRouteProvider : DefaultDirectRouteProvider
    {
        internal const string IsDefaultVersionRouteKey = "IsDefaultVersionRoute";

        /// <summary>
        /// Creates <see cref="T:System.Web.Http.Routing.RouteEntry" /> instances based on the provided factories and action. The route entries provide direct routing to the provided action.
        /// </summary>
        /// <returns>A set of route entries.</returns>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <param name="factories">The direct route factories.</param>
        /// <param name="constraintResolver">The constraint resolver.</param>
        protected override IReadOnlyList<RouteEntry> GetActionDirectRoutes(HttpActionDescriptor actionDescriptor,
            IReadOnlyList<IDirectRouteFactory> factories, IInlineConstraintResolver constraintResolver)
        {
            Contract.Assume(actionDescriptor != null);

            // let the DefaultDirectRouteProvider create the normal routes based on all factories
            var routes =
                new List<RouteEntry>(base.GetActionDirectRoutes(actionDescriptor, factories, constraintResolver));

            // check if the current action is part of the default API version and the action is not version neutral
            var defaultVersion = actionDescriptor.Configuration.GetApiVersioningOptions().DefaultApiVersion;
            var versionModel = actionDescriptor.ControllerDescriptor.GetApiVersionModel();
            var isDefaultVersionRoute = !versionModel.IsApiVersionNeutral && versionModel.DeclaredApiVersions.Any(v => v == defaultVersion);

            if (isDefaultVersionRoute)
            {
                // find all routes for the action that have a version constraint
                var versionizedRoutes = routes.Where(r => r.Route.Constraints.Any(c => c.Value is ApiVersionRouteConstraint)).ToArray();
                foreach (var versionizedRoute in versionizedRoutes)
                {
                    // separate the URL template into the path segments
                    var chunks = versionizedRoute.Route.RouteTemplate.Split('/');

                    // build the {version} string based on the route constraint name
                    var versionConstraint = "{" +
                                            versionizedRoute.Route.Constraints.Where(
                                                c => c.Value is ApiVersionRouteConstraint).Select(c => c.Key).First() +
                                            "}";

                    // join the segments again together but remove the chunks containing the URL constraint
                    var newRouteTemplate = string.Join("/", chunks.Where(c => !c.Contains(versionConstraint)));

                    // build a new route for the new URL template
                    var context = new DirectRouteFactoryContext(string.Empty, new[] { actionDescriptor },
                        constraintResolver, false);
                    var builder = context.CreateBuilder(newRouteTemplate);
                    if (!string.IsNullOrEmpty(versionizedRoute.Name))
                    {
                        builder.Name = versionizedRoute.Name + "DefaultVersion";
                    }
                    builder.DataTokens = new Dictionary<string, object>
                    {
                        {IsDefaultVersionRouteKey, true}
                    };
                    routes.Add(builder.Build());
                }
            }

            return routes;
        }
    }
}
