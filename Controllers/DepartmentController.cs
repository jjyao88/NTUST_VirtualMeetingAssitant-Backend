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
    public class DepartmentUpdateDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public IEnumerable<int> Ids { get; set; }
    }

    public class DepartmentDTO
    {
        public string Name { get; set; }
        public IEnumerable<int> Attendees { get; set; }
    }
}

namespace VirturlMeetingAssitant.Backend.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using VirturlMeetingAssitant.Backend.DTO;

    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly ILogger<DepartmentController> _logger;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUserRepository _userRepository;

        public DepartmentController(ILogger<DepartmentController> logger, IDepartmentRepository departmentRepository, IUserRepository userRepository)
        {
            _departmentRepository = departmentRepository;
            _userRepository = userRepository;
            _logger = logger;
        }
        /// <summary>
        /// Get all departments
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<DepartmentDTO>> GetAll()
        {
            var departments = await _departmentRepository.GetAll();
            return departments.Select(d =>
            {
                return new DepartmentDTO()
                {
                    Name = d.Name,
                    Attendees = d.Users.Select(u =>
                    {
                        return u.ID;
                    }),
                };
            });
        }

        /// <summary>
        /// Add or update a department depends on DTO.
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddOrUpdate(DepartmentUpdateDTO dto)
        {
            try
            {
                var department = await _departmentRepository.Find(d => d.Name == dto.Name).FirstOrDefaultAsync();
                if (department == null)
                {
                    department = new Department() { Name = dto.Name };
                }

                var users = await _userRepository.Find(u => dto.Ids.Contains(u.ID)).ToListAsync();
                department.Users = users;
                await _departmentRepository.Update(department);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(string departmentName)
        {
            try
            {
                var department = await _departmentRepository.Find(d => d.Name == departmentName).FirstOrDefaultAsync();
                await _departmentRepository.Remove(department);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return Ok();
        }
    }
}
