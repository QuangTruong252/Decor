using System;
using System.Threading.Tasks;
using DecorStore.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthCheckController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HealthCheck
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Check database connection
                bool canConnect = await _context.Database.CanConnectAsync();
                
                // Connection information
                var connectionInfo = new
                {
                    DatabaseConnected = canConnect,
                    Provider = _context.Database.ProviderName,
                    ConnectionString = "***Hidden for security***",
                    ServerVersion = canConnect ? "Version information not available" : null,
                    Timestamp = DateTime.UtcNow
                };
                
                return Ok(connectionInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Database connection error", Message = ex.Message, InnerException = ex.InnerException?.Message });
            }
        }
    }
} 