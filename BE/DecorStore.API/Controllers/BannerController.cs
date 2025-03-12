using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly IBannerService _bannerService;
        
        public BannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }
        
        // GET: api/Banner
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BannerDTO>>> GetBanners()
        {
            var banners = await _bannerService.GetAllBannersAsync();
            return Ok(banners);
        }
        
        // GET: api/Banner/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BannerDTO>>> GetActiveBanners()
        {
            var banners = await _bannerService.GetActiveBannersAsync();
            return Ok(banners);
        }
        
        // GET: api/Banner/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BannerDTO>> GetBanner(int id)
        {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            
            if (banner == null)
            {
                return NotFound();
            }
            
            return banner;
        }
        
        // POST: api/Banner
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Banner>> CreateBanner([FromForm] CreateBannerDTO bannerDto)
        {
            try
            {
                var banner = await _bannerService.CreateBannerAsync(bannerDto);
                return CreatedAtAction(nameof(GetBanner), new { id = banner.Id }, banner);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        // PUT: api/Banner/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBanner(int id, [FromForm] UpdateBannerDTO bannerDto)
        {
            try
            {
                await _bannerService.UpdateBannerAsync(id, bannerDto);
                return NoContent();
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        // DELETE: api/Banner/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            try
            {
                await _bannerService.DeleteBannerAsync(id);
                return NoContent();
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound();
            }
        }
    }
} 