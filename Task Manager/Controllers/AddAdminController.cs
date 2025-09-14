using Application.Features.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    
    [ApiController]
    public class AddAdminController : BaseController
    {
        private readonly AddAdminService _service;
        public AddAdminController(AddAdminService service)
        {
            _service = service;
        }

        [HttpPost("add/Admin")]
        public async Task<IActionResult> AddAdminAsync()
        {
            return await HandleServiceCallAsync(() =>  _service.AddAdminAsync());
        }
    }
}
