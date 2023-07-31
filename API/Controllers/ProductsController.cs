using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  
using Infrastructure.Data;
using Core.Interfaces;
using Core.Entities;
using Core.Specifications;
 
namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IGenericRepository<Product> _productsRepo;
        private readonly IGenericRepository<ProductBrand> _productBrandRepo;

        private readonly IGenericRepository<ProductType> _productTypeRepo;

        public ProductsController(IGenericRepository<Product> productsRepo
        ,IGenericRepository<ProductBrand> productBrandRepo
        ,IGenericRepository<ProductType> productTypeRepo)
        {
          _productsRepo = productsRepo;
          _productBrandRepo = productBrandRepo;
          _productTypeRepo = productTypeRepo;
          
        }
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts()
        {
            var spec = new ProductsWithTypesAndBrandsSpecification();
            var products = await _productsRepo.ListAsync(spec);
            return Ok(products);
        }
          
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
             var spec = new ProductsWithTypesAndBrandsSpecification(id);
             return  await _productsRepo.GetEntityWithSpec(spec);
        }

         [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
        {
            var productBrands = await _productBrandRepo.ListAllAsync();
            return Ok(productBrands);
        }
         [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
        {
            var productTypes = await _productTypeRepo.ListAllAsync();
            return Ok(productTypes);
        }


    }
}