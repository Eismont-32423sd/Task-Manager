using Application.Services.DTOs.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers
{
    [ApiController]
    public class BaseController : Controller
    {
        protected async Task<IActionResult> HandleServiceCallAsync<T>(Func<Task<ServiceResult<T>>> serviceCall)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await serviceCall();

            if (!result.IsSucceded)
            {
                return BadRequest(new
                {
                    Message = result.Message,
                    Errors = result.Errors
                });
            }

            return Ok(result.Data is null
                    ? new { Message = result.Message }
                    : new { Message = result.Message, Data = result.Data });

        }
    }
}
