using System.Web.Http;
using Microsoft.Web.Http;

namespace CoderLine.AspNet.WebApi.Versioning.Tests.TestControllers
{
    namespace V1
    {
        [ApiVersion("1.0")]
        [RoutePrefix("api/v{version:apiVersion}/agreements")]
        public class AgreementsController : ApiController {[Route("{accountId}")] public IHttpActionResult Get(string accountId) => Ok(); }
    }
    namespace V2
    {
        [ApiVersion("2.0")]
        [RoutePrefix("api/v{version:apiVersion}/agreements")]
        public class AgreementsController : ApiController {[Route("{accountId}")] public IHttpActionResult Get(string accountId) => Ok(); }
    }
    namespace V3
    {
        [ApiVersion("3.0")]
        [ApiVersion("4.0")]
        [RoutePrefix("api/v{version:apiVersion}/agreements")]
        public class AgreementsController : ApiController {[Route("{accountId}")] public IHttpActionResult Get(string accountId) => Ok(); }
    }
}
