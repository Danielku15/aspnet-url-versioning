using System.IO;
using System.Net.Http;
using System.Web.Http;
using FluentAssertions;
using Microsoft.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using Xunit;

namespace CoderLine.AspNet.WebApi.Versioning.Tests
{
    public class ApiVersionControllerSelectorTest
    {
        [Theory]
        [InlineData("http://localhost/api/v3/agreements/test")]
        [InlineData("http://localhost/api/v3.0/agreements/test")]
        [InlineData("http://localhost/api/v4/agreements/test")]
        [InlineData("http://localhost/api/v4.0/agreements/test")]
        [InlineData("http://localhost/api/agreements/test")]
        public void select_controller_should_select_default_version(string uri)
        {
            // arrange
            var controllerType = typeof(TestControllers.V3.AgreementsController);
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            configuration.AddApiVersioningWithUrlSupport(o =>
            {
                o.VersioningOptions.DefaultApiVersion = new ApiVersion(3, 0);
            });
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData(request);

            request.SetConfiguration(configuration);
            request.SetRouteData(routeData);

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController(request);

            // assert
            controller.ControllerType.Should().Be(controllerType);
        }
    }
}
