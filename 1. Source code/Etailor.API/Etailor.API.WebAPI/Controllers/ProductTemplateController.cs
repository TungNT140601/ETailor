using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/template-management")]
    [ApiController]
    public class ProductTemplateController : ControllerBase
    {
        private readonly IProductTemplateService productTemplateService;

        public ProductTemplateController(IProductTemplateService productTemplateService)
        {
            this.productTemplateService = productTemplateService;
        }

        [HttpGet("get-all-template")]
        public IActionResult GetAllTemplate()
        {
            try
            {
                return Ok();
            }
            catch (UserException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SystemsException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
