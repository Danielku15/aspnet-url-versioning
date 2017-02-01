using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using FluentAssertions;
using Microsoft.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
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

        [Fact]
        public void apidescriptors_must_not_contain_version_parameter()
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

            foreach (var apiDescription in apiDescriptions)
            {
                apiDescription.ParameterDescriptions.Should().NotContain(desc => desc.Name == "version");
            }
        }
        [Fact]
        public void swagger_must_not_contain_version_parameter()
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
                o.PreferShortHandVersion = true;
            });
            SwaggerDocsConfig swaggerDocsConfig = null;
            configuration.EnableSwagger(c =>
                {
                    swaggerDocsConfig = c;
                    c.MultipleApiVersions((description, version) => description.RelativePath.StartsWith("api/" + version), vc =>
                    {
                        vc.Version("v3", "API v3");
                    });
                    c.GroupActionsBy(m =>
                    {
                        var name = m.ActionDescriptor.ControllerDescriptor.ControllerName;
                        if (name.EndsWith(DefaultHttpControllerSelector.ControllerSuffix))
                        {
                            name = name.Substring(0, name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length);
                        }
                        return name;
                    });
                });

            configuration.EnsureInitialized();

            // act
            var swaggerDocument = GetSwaggerProvider(configuration, swaggerDocsConfig).GetSwagger("http://localhost", "v3");

            // assert
            Action<Operation> validateOperation = op => op?.parameters?.Should().NotContain(p => p.name == "version");

            foreach (var path in swaggerDocument.paths)
            {
                validateOperation(path.Value.head);
                validateOperation(path.Value.delete);
                validateOperation(path.Value.get);
                validateOperation(path.Value.options);
                validateOperation(path.Value.post);
                validateOperation(path.Value.patch);
                validateOperation(path.Value.put);
            }

            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                Converters = new JsonConverter[] { new VendorExtensionsConverter() }
            };
            var swaggerJson = JsonConvert.SerializeObject(swaggerDocument.paths, Formatting.Indented, serializerSettings);
            swaggerJson.Should().NotContain("version");
        }

        private ISwaggerProvider GetSwaggerProvider(HttpConfiguration configuration, SwaggerDocsConfig swaggerDocsConfig)
        {
            var method = swaggerDocsConfig.GetType()
                .GetMethod("GetSwaggerProvider", BindingFlags.Instance | BindingFlags.NonPublic);

            var request = new HttpRequestMessage();
            request.SetRequestContext(new HttpRequestContext
            {
                Configuration = configuration
            });
            return (ISwaggerProvider)method.Invoke(swaggerDocsConfig, new object[] { request });
        }
    }
}
