using AutoMapper;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productService;
        private readonly IProductTemplateService productTemplateService;
        private readonly IOrderService orderService;
        private readonly IMapper mapper;

        public ProductController(IProductService productService, IProductTemplateService productTemplateService, IOrderService orderService, IMapper mapper)
        {
            this.productService = productService;
            this.productTemplateService = productTemplateService;
            this.orderService = orderService;
            this.mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id Product không tồn tại");
                }
                else
                {
                    var product = productService.GetProduct(id);
                    return product != null ? Ok(mapper.Map<ProductVM>(product)) : NotFound(id);
                }
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
        [HttpGet]
        public async Task<IActionResult> GetProductsByOrderId(string? search)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<ProductVM>>(productService.GetProductsByOrderId(search)));
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
