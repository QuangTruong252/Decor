using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Services;
using DecorStore.API.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : BaseController
    {
        private readonly IBannerService _bannerService;
        
        public BannerController(IBannerService bannerService, ILogger<BannerController> logger) : base(logger)
        {
            _bannerService = bannerService;
        }
        
        // GET: api/Banner
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BannerDTO>>> GetBanners()
        {
            var result = await _bannerService.GetAllBannersAsync();
            return HandleResult(result);
        }
        
        // GET: api/Banner/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BannerDTO>>> GetActiveBanners()
        {
            var result = await _bannerService.GetActiveBannersAsync();
            return HandleResult(result);
        }
        
        // GET: api/Banner/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BannerDTO>> GetBanner(int id)
        {
            var result = await _bannerService.GetBannerByIdAsync(id);
            return HandleResult(result);
        }
        
        // POST: api/Banner
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BannerDTO>> CreateBanner([FromForm] CreateBannerDTO bannerDto)        {
            var modelValidation = ValidateModelState();
            if (modelValidation.IsFailure) return BadRequest(modelValidation.Error);

            var result = await _bannerService.CreateBannerAsync(bannerDto);
            return HandleCreateResult(result, nameof(GetBanner), new { id = result.Data?.Id });
        }
        
        // PUT: api/Banner/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]        public async Task<IActionResult> UpdateBanner(int id, [FromForm] UpdateBannerDTO bannerDto)
        {
            var modelValidation = ValidateModelState();
            if (modelValidation.IsFailure) return BadRequest(modelValidation.Error);

            var result = await _bannerService.UpdateBannerAsync(id, bannerDto);
            return Ok(result.Data);
        }
        
        // DELETE: api/Banner/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var result = await _bannerService.DeleteBannerAsync(id);
            return HandleResult(result);
        }
    }
}
