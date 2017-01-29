using System.Web.Http;
using FluentAssertions;
using Microsoft.Web.Http;
using Xunit;

namespace CoderLine.AspNet.WebApi.Versioning.Tests
{
    public class ApiVersionApiExplorerTests
    {
        [Fact]
        public void apidescriptions_must_contain_versionized_entries()
        {
            // arrange
            var configuration = new HttpConfiguration();
            configuration.AddApiVersioningWithUrlSupport(o =>
            {
                o.VersioningOptions.DefaultApiVersion = new ApiVersion(3, 0);
            });
            configuration.AddApiVersioningAwareApiExplorer();
            configuration.EnsureInitialized();
            var apiExplorer = configuration.Services.GetApiExplorer();

            // act
            var apiDescriptions = apiExplorer.ApiDescriptions;

            // assert
            apiDescriptions.Count.ShouldBeEquivalentTo(5);

            var v1 = apiDescriptions[0];
            v1.RelativePath.ShouldBeEquivalentTo("api/v1.0/agreements/{accountId}");
            v1.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V1.AgreementsController));

            var v2 = apiDescriptions[1];
            v2.RelativePath.ShouldBeEquivalentTo("api/v2.0/agreements/{accountId}");
            v2.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V2.AgreementsController));

            var v3 = apiDescriptions[2];
            v3.RelativePath.ShouldBeEquivalentTo("api/v3.0/agreements/{accountId}");
            v3.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V3.AgreementsController));

            var v4 = apiDescriptions[3];
            v4.RelativePath.ShouldBeEquivalentTo("api/v4.0/agreements/{accountId}");
            v4.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V3.AgreementsController));

            var vDefault = apiDescriptions[4];
            vDefault.RelativePath.ShouldBeEquivalentTo("api/agreements/{accountId}");
            vDefault.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V3.AgreementsController));
        }

        [Fact]
        public void apidescriptions_must_not_contain_default_version()
        {
            // arrange
            var configuration = new HttpConfiguration();
            configuration.AddApiVersioningWithUrlSupport(o =>
            {
                o.VersioningOptions.DefaultApiVersion = new ApiVersion(3, 0);
            });
            configuration.AddApiVersioningAwareApiExplorer(o =>
            {
                o.IncludeDefaultVersion = false;
            });
            configuration.EnsureInitialized();
            var apiExplorer = configuration.Services.GetApiExplorer();

            // act
            var apiDescriptions = apiExplorer.ApiDescriptions;

            // assert
            apiDescriptions.Count.ShouldBeEquivalentTo(4);

            var v1 = apiDescriptions[0];
            v1.RelativePath.ShouldBeEquivalentTo("api/v1.0/agreements/{accountId}");
            v1.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V1.AgreementsController));

            var v2 = apiDescriptions[1];
            v2.RelativePath.ShouldBeEquivalentTo("api/v2.0/agreements/{accountId}");
            v2.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V2.AgreementsController));

            var v3 = apiDescriptions[2];
            v3.RelativePath.ShouldBeEquivalentTo("api/v3.0/agreements/{accountId}");
            v3.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V3.AgreementsController));

            var v4 = apiDescriptions[3];
            v4.RelativePath.ShouldBeEquivalentTo("api/v4.0/agreements/{accountId}");
            v4.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V3.AgreementsController));
        }

        [Fact]
        public void apidescriptions_must_contain_shorthanded_versionized_entries()
        {
            // arrange
            var configuration = new HttpConfiguration();
            configuration.AddApiVersioningWithUrlSupport(o =>
            {
                o.VersioningOptions.DefaultApiVersion = new ApiVersion(3, 0);
            });
            configuration.AddApiVersioningAwareApiExplorer(o =>
            {
                o.PreferShortHandVersion = true;
            });
            configuration.EnsureInitialized();
            var apiExplorer = configuration.Services.GetApiExplorer();

            // act
            var apiDescriptions = apiExplorer.ApiDescriptions;

            // assert
            apiDescriptions.Count.ShouldBeEquivalentTo(5);

            var v1 = apiDescriptions[0];
            v1.RelativePath.ShouldBeEquivalentTo("api/v1/agreements/{accountId}");
            v1.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V1.AgreementsController));

            var v2 = apiDescriptions[1];
            v2.RelativePath.ShouldBeEquivalentTo("api/v2/agreements/{accountId}");
            v2.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V2.AgreementsController));

            var v3 = apiDescriptions[2];
            v3.RelativePath.ShouldBeEquivalentTo("api/v3/agreements/{accountId}");
            v3.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V3.AgreementsController));

            var v4 = apiDescriptions[3];
            v4.RelativePath.ShouldBeEquivalentTo("api/v4/agreements/{accountId}");
            v4.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V3.AgreementsController));

            var vDefault = apiDescriptions[4];
            vDefault.RelativePath.ShouldBeEquivalentTo("api/agreements/{accountId}");
            vDefault.ActionDescriptor.ControllerDescriptor.ControllerType.Should().Be(typeof(TestControllers.V3.AgreementsController));
        }
    }
}
