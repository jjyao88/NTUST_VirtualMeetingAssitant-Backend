using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VirturlMeetingAssitant.Backend.Db;
using Microsoft.EntityFrameworkCore;

namespace VirturlMeetingAssitant.Backend.DTO
{
    public class UpdateUserPasswordDTO
    {
        public string NewPassword { get; set; }
    }
}

namespace VirturlMeetingAssitant.Backend.Controllers
{
    using VirturlMeetingAssitant.Backend.DTO;

    [ApiController]
    [Route("api/[controller]")]
    public class OTPController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;
        private readonly IOneTimePasswordRepository _otpRepository;
        private readonly IUserRepository _userRepository;
        public OTPController(ILogger<RoomController> logger, IOneTimePasswordRepository otpRepository, IUserRepository userRepository)
        {
            _logger = logger;
            _otpRepository = otpRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<bool>> CheckOTPValid(string otp)
        {
            var result = await _otpRepository.CheckOTPValid(otp);
            return Ok(result.isValid);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserPassword(UpdateUserPasswordDTO dto, string otp)
        {
            var updateResult = await _otpRepository.UpdateUserPassword(otp, dto.NewPassword);

            if (updateResult)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Cannot update user password. OTP chould be invalid");
            }
        }
    }
}
