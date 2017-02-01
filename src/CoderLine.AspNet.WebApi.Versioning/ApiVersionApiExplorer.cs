using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using CoderLine.AspNet.WebApi.Versioning.Routing;
using Microsoft.Web.Http;
using Microsoft.Web.Http.Routing;

namespace CoderLine.AspNet.WebApi.Versioning
{
    /// <summary>
    /// Explores the URI space of the service based on routes, controllers and actions available in the system.
    /// </summary>
    public class ApiVersionApiExplorer : IApiExplorer
    {
        #region Lambdas for internal method access

        // ReSharper disable InconsistentNaming

        private static readonly Func<IHttpRoute, object> HttpDataExtensions_GetDirectRouteCandidates;
        private static readonly Func<object, HttpControllerDescriptor> ApiExplorer_GetDirectRouteController;
        private static readonly Func<ApiExplorer, HttpControllerDescriptor, object, IHttpRoute, Collection<ApiDescription>> ApiExplorer_ExploreDirectRoute;
        private static readonly Func<ApiExplorer, IDictionary<string, HttpControllerDescriptor>, IHttpRoute, Collection<ApiDescription>> ApiExplorer_ExploreRouteControllers;
        private static readonly Action<ApiDescription, ResponseDescription> ApiDescription_ResponseDescription;

        // ReSharper restore InconsistentNaming

        static ApiVersionApiExplorer()
        {
            var candidateActionType = typeof(ApiExplorer).Assembly.GetType("System.Web.Http.Routing.CandidateAction");

            // HttpRouteDataExtensions.GetDirectRouteCandidates
            {
                var getDirectRouteCandidates = typeof(ApiExplorer).Assembly.GetType("System.Web.Http.Routing.HttpRouteExtensions").GetMethod("GetDirectRouteCandidates", BindingFlags.Static | BindingFlags.Public);
                var routeParameter = Expression.Parameter(typeof(IHttpRoute), "route");
                HttpDataExtensions_GetDirectRouteCandidates = Expression.Lambda<Func<IHttpRoute, object>>(Expression.Call(getDirectRouteCandidates, routeParameter), routeParameter).Compile();
            }

            // ApiExplorer.GetDirectRouteController
            {
                var getDirectRouteController = typeof(ApiExplorer).GetMethod("GetDirectRouteController", BindingFlags.Static | BindingFlags.NonPublic);
                var directRouteCandidatesParameter = Expression.Parameter(typeof(object), "directRouteCandidates");
                ApiExplorer_GetDirectRouteController = Expression.Lambda<Func<object, HttpControllerDescriptor>>(
                    Expression.Call(getDirectRouteController, Expression.Convert(directRouteCandidatesParameter, candidateActionType.MakeArrayType())), directRouteCandidatesParameter
                ).Compile();
            }

            // ApiExplorer.ExploreDirectRoute
            {
                var apiExplorerParameter = Expression.Parameter(typeof(ApiExplorer), "apiExplorer");
                var controllerDescriptorParameter = Expression.Parameter(typeof(HttpControllerDescriptor), "controllerDescriptor");
                var candidatesParameter = Expression.Parameter(typeof(object), "candidates");
                var routeParameter = Expression.Parameter(typeof(IHttpRoute), "route");

                var exploreDirectRoute = typeof(ApiExplorer).GetMethod("ExploreDirectRoute", BindingFlags.Instance | BindingFlags.NonPublic);
                ApiExplorer_ExploreDirectRoute = Expression.Lambda<Func<ApiExplorer, HttpControllerDescriptor, object, IHttpRoute, Collection<ApiDescription>>>(
                    Expression.Call(apiExplorerParameter, exploreDirectRoute, controllerDescriptorParameter, Expression.Convert(candidatesParameter, candidateActionType.MakeArrayType()), routeParameter),
                    apiExplorerParameter, controllerDescriptorParameter, candidatesParameter, routeParameter
                ).Compile();
            }

            // ApiExplorer.ExploreRouteControllers
            {
                var apiExplorerParameter = Expression.Parameter(typeof(ApiExplorer), "apiExplorer");
                var controllerMappingsParameter = Expression.Parameter(typeof(IDictionary<string, HttpControllerDescriptor>), "controllerMappings");
                var routeParameter = Expression.Parameter(typeof(IHttpRoute), "route");

                var exploreRouteControllers = typeof(ApiExplorer).GetMethod("ExploreRouteControllers", BindingFlags.Instance | BindingFlags.NonPublic);
                ApiExplorer_ExploreRouteControllers = Expression.Lambda<Func<ApiExplorer, IDictionary<string, HttpControllerDescriptor>, IHttpRoute, Collection<ApiDescription>>>(
                    Expression.Call(apiExplorerParameter, exploreRouteControllers, controllerMappingsParameter, routeParameter),
                    apiExplorerParameter, controllerMappingsParameter, routeParameter
                ).Compile();
            }

            // ApiDescription.ResponseDescription
            {
                var apiDescriptionParameter = Expression.Parameter(typeof(ApiDescription), "apiDescription");
                var responseDescriptionParameter = Expression.Parameter(typeof(ResponseDescription), "responseDescription");

                var responseDescriptionProperty = typeof(ApiDescription).GetProperty("ResponseDescription");
                ApiDescription_ResponseDescription = Expression.Lambda<Action<ApiDescription, ResponseDescription>>(
                    Expression.Assign(Expression.MakeMemberAccess(apiDescriptionParameter, responseDescriptionProperty), responseDescriptionParameter),
                    apiDescriptionParameter, responseDescriptionParameter
                ).Compile();
            }
        }

        #endregion

        private readonly HttpConfiguration _configuration;
        private readonly ApiExplorerUrlOptions _options;
        private readonly Lazy<Collection<ApiDescription>> _apiDescriptions;
        private readonly ApiExplorer _apiExplorer;

        /// <summary>
        /// Gets the API descriptions. The descriptions are initialized on the first access.
        /// </summary>
        public Collection<ApiDescription> ApiDescriptions => _apiDescriptions.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionApiExplorer"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="options">The generation options</param>
        public ApiVersionApiExplorer(HttpConfiguration configuration, ApiExplorerUrlOptions options)
        {
            _configuration = configuration;
            _options = options;
            _apiDescriptions = new Lazy<Collection<ApiDescription>>(InitializeApiDescriptions);
            _apiExplorer = new ApiExplorer(configuration);
        }

        private Collection<ApiDescription> InitializeApiDescriptions()
        {
            // Build an unfiltered list of API descriptions
            var apiDescriptions = new List<ApiDescription>();
            var controllerSelector = _configuration.Services.GetHttpControllerSelector();
            var controllerMappings = controllerSelector.GetControllerMapping();
            if (controllerMappings != null)
            {
                apiDescriptions = FlattenRoutes(_configuration.Routes)
                    .SelectMany(r => GetDescriptionsFromRoute(r, controllerMappings))
                    .ToList();
            }

            // Build real api descriptions based on the unfiltered list of API descriptions
            return BuildVersionizedApiDescriptions(apiDescriptions);
        }

        /// <summary>
        /// Generates versionized api descriptions based on an unfiltered list of of api descriptions. 
        /// </summary>
        /// <param name="apiDescriptions">The unfiltered list of API descriptions. </param>
        /// <returns></returns>
        protected virtual Collection<ApiDescription> BuildVersionizedApiDescriptions(List<ApiDescription> apiDescriptions)
        {
            var versionizedDescriptions = new Collection<ApiDescription>();
            foreach (var apiDescription in apiDescriptions)
            {
                // check whether route is versionized
                var versionModel = apiDescription.ActionDescriptor.ControllerDescriptor.GetApiVersionModel();
                var versionConstraints = new HashSet<string>(apiDescription.Route.Constraints.Where(r => r.Value is ApiVersionRouteConstraint).Select(r => r.Key));
                if (versionConstraints.Count == 0 || versionModel.IsApiVersionNeutral)
                {
                    // add default verison routes, or routes that have nothing to do with versioning
                    if (versionModel.IsApiVersionNeutral || _options.IncludeDefaultVersion || !apiDescription.Route.DataTokens.ContainsKey(ApiVersionDirectRouteProvider.IsDefaultVersionRouteKey))
                    {
                        versionizedDescriptions.Add(apiDescription);
                    }
                }
                else
                {
                    foreach (var version in versionModel.DeclaredApiVersions)
                    {
                        var versionString = _options.PreferShortHandVersion ? ToShortHandString(version) : ToLongString(version);

                        var finalPath = apiDescription.RelativePath;
                        foreach (var constraint in versionConstraints)
                        {
                            finalPath = finalPath.Replace("{" + constraint + "}", versionString);
                        }

                        var versionizedDescription = new ApiDescription
                        {
                            Documentation = apiDescription.Documentation,
                            HttpMethod = apiDescription.HttpMethod,
                            RelativePath = finalPath,
                            ActionDescriptor = apiDescription.ActionDescriptor,
                            Route = apiDescription.Route,
                        };

                        foreach (var formatter in apiDescription.SupportedResponseFormatters)
                        {
                            versionizedDescription.SupportedResponseFormatters.Add(formatter);
                        }
                        foreach (var formatter in apiDescription.SupportedRequestBodyFormatters)
                        {
                            versionizedDescription.SupportedRequestBodyFormatters.Add(formatter);
                        }
                        foreach (var parameterDescription in apiDescription.ParameterDescriptions)
                        {
                            // prevent the version constraints of the URI to be added to the api descriptor
                            if (parameterDescription.Source != ApiParameterSource.FromUri ||
                                !versionConstraints.Contains(parameterDescription.Name))
                            {
                                versionizedDescription.ParameterDescriptions.Add(parameterDescription);
                            }
                        }

                        ApiDescription_ResponseDescription(versionizedDescription, new ResponseDescription
                        {
                            Documentation = apiDescription.ResponseDescription.Documentation,
                            DeclaredType = apiDescription.ResponseDescription.DeclaredType,
                            ResponseType = apiDescription.ResponseDescription.ResponseType
                        });
                        versionizedDescription.ResponseDescription.Documentation = apiDescription.ResponseDescription.Documentation;
                        versionizedDescription.ResponseDescription.DeclaredType = apiDescription.ResponseDescription.DeclaredType;
                        versionizedDescription.ResponseDescription.ResponseType = apiDescription.ResponseDescription.ResponseType;

                        versionizedDescriptions.Add(versionizedDescription);
                    }
                }
            }
            return versionizedDescriptions;
        }

        private string ToLongString(ApiVersion version)
        {
            return version.ToString();
        }

        private string ToShortHandString(ApiVersion version)
        {
            var text = new StringBuilder();
            if (version.MajorVersion != null)
            {
                text.Append(version.MajorVersion.Value);

                if (version.MinorVersion != null && version.MinorVersion.Value != 0)
                {
                    text.Append('.');
                    text.Append(version.MinorVersion.Value);
                }
            }
            else if (version.MinorVersion != null)
            {
                text.Append("0.");
                text.Append(version.MinorVersion.Value);
            }
            return text.ToString();
        }

        private IEnumerable<ApiDescription> GetDescriptionsFromRoute(IHttpRoute route, IDictionary<string, HttpControllerDescriptor> controllerMappings)
        {
            // based on the default API explorer implementation. we use compiled lambdas to call internal methods
            var directRouteCandidates = HttpDataExtensions_GetDirectRouteCandidates(route);
            var directRouteController = ApiExplorer_GetDirectRouteController(directRouteCandidates);
            return directRouteController != null && directRouteCandidates != null
                    ? ApiExplorer_ExploreDirectRoute(_apiExplorer, directRouteController, directRouteCandidates, route)
                    : ApiExplorer_ExploreRouteControllers(_apiExplorer, controllerMappings, route);
        }

        private IEnumerable<IHttpRoute> FlattenRoutes(IEnumerable<IHttpRoute> routes)
        {
            foreach (var route in routes)
            {
                var nested = route as IEnumerable<IHttpRoute>;
                if (nested != null)
                {
                    foreach (var subRoute in FlattenRoutes(nested))
                    {
                        yield return subRoute;
                    }
                }
                else
                {
                    yield return route;
                }
            }
        }
    }
}
