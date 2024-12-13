using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace pweb_eas.Controllers
{
    [Route("api/transaction")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            if (role == "Admin")
            {
                return Ok(new
                {
                    message = "admin"
                });
            }

            if (role == "User")
            {
                return Ok(new
                {
                    message = "user"
                });
            }

            return Unauthorized(new
            {
                message = "invalid roles",
                role
            });
        }
    }
}