using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.SimpleExportImportModule.Core;


namespace VirtoCommerce.SimpleExportImportModule.Web.Controllers.Api
{
    [Route("api/VirtoCommerceSimpleExportImport")]
    public class VirtoCommerceSimpleExportImportModuleController : Controller
    {
        // GET: api/VirtoCommerceSimpleExportImport
        /// <summary>
        /// Get message
        /// </summary>
        /// <remarks>Return "Hello world!" message</remarks>
        [HttpGet]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public ActionResult<string> Get()
        {
            return Ok(new { result = "Hello world!" });
        }
    }
}
