using System.Web.Http;
using FluentAssertions;
using Microsoft.Web.Http.Controllers;
using Microsoft.Web.Http.Dispatcher;
using Xunit;

namespace CoderLine.AspNet.WebApi.Versioning.Tests
{
    public class WebApiExtensionsTests
    {
        private const string AttributeRouteName = "MS_attributerouteWebApi";

        [Fact]
        public void add_api_versioning_should_setup_configuration_with_default_options()
        {
            // arrange
            var configuration = new HttpConfiguration();

            // act
            configuration.AddApiVersioningWithUrlSupport();

            // assert
            configuration.Services.GetHttpControllerSelector().Should().BeOfType<ApiVersionControllerSelector>();
            configuration.Services.GetActionSelector().Should().BeOfType<ApiVersionActionSelector>();
            configuration.Filters.Should().HaveCount(0);
            configuration.Routes.ContainsKey(AttributeRouteName).Should().BeTrue();
        }

        [Fact]
        public void add_api_versioning_should_setup_attribute_routes_with_custom_setting()
        {
            // arrange
            var configuration = new HttpConfiguration();

            // act
            configuration.AddApiVersioningWithUrlSupport(o =>
            {
                o.VersioningOptions.AssumeDefaultVersionWhenUnspecified = false;
            });

            // assert
            configuration.Services.GetHttpControllerSelector().Should().BeOfType<ApiVersionControllerSelector>();
            configuration.Services.GetActionSelector().Should().BeOfType<ApiVersionActionSelector>();
            configuration.Filters.Should().HaveCount(0);
            configuration.Routes.ContainsKey(AttributeRouteName).Should().BeFalse();
        }

        [Fact]
        public void add_api_version_aware_api_explorer_registers_new_explorer()
        {
            // arrange
            var configuration = new HttpConfiguration();

            // act
            configuration.AddApiVersioningAwareApiExplorer(o =>
            {
                o.IncludeDefaultVersion = !o.IncludeDefaultVersion;
                o.PreferShortHandVersion = !o.PreferShortHandVersion;
            });

            // assert
            configuration.Services.GetApiExplorer().Should().BeOfType<ApiVersionApiExplorer>();
        }
    }
}
