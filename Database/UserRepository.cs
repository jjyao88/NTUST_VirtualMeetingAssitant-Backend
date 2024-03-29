using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirturlMeetingAssitant.Backend.DTO;
using BC = BCrypt.Net.BCrypt;
namespace VirturlMeetingAssitant.Backend.Db
{
    /// <summary>The user repository contains users from database.</summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Add a new user from DTO.
        /// </summary>
        Task<User> AddFromDTOAsync(UserAddDTO dto);
        /// <summary>
        /// Update a existing user from DTO.
        /// </summary>
        Task<User> UpdateFromDTOAsync(UserUpdateDTO dto);
        /// <summary>
        /// Update a existing user with new password.
        /// </summary>
        Task<bool> UpdatePasswordFromDTO(UserPasswordUpdateDTO dto);
        /// <summary>
        /// Update a existing user with new password from DTO.
        /// </summary>
        Task<User> UpdatePassword(User user, string newPassword);
    }
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly IDepartmentRepository _departmentRepository;
        public UserRepository(MeetingContext dbContext, IDepartmentRepository departmentRepository) : base(dbContext)
        {
            _departmentRepository = departmentRepository;
        }

        /// <summary>
        /// Check if there's a user with that email. If not, return false.
        /// </summary>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            var count = await this.Find(x => x.Email == email).CountAsync();

            return count > 0 ? true : false;
        }

        public async Task<User> AddFromDTOAsync(UserAddDTO dto)
        {
            if (await this.IsEmailExistsAsync(dto.Email))
            {
                throw new Exception($"The mail ({dto.Email}) is already in use.");
            }

            var user = new User()
            {
                Name = dto.Name,
                Password = BC.HashPassword(dto.Password),
                Email = dto.Email,
                Department = await _departmentRepository.Find(x => x.Name == dto.DepartmentName).FirstAsync(),
            };

            return await this.Add(user);
        }

        public async Task<User> UpdateFromDTOAsync(UserUpdateDTO dto)
        {
            var user = await this.Find(u => u.ID == dto.ID).FirstAsync();

            if (user == null)
            {
                throw new Exception($"The user {dto.ID} was not found.");
            }

            if (dto.Name != null)
            {
                user.Name = dto.Name;
            }

            if (dto.Email != null)
            {
                user.Email = dto.Email;
            }

            if (dto.DepartmentName != null)
            {
                var department = await _departmentRepository.Find(d => d.Name == dto.DepartmentName).FirstAsync();
                if (department == null)
                {
                    throw new Exception($"Department {dto.DepartmentName} is not found.");
                }

                user.Department = department;
            }

            return await this.Update(user);
        }

        public async Task<User> UpdatePassword(User user, string newPassword)
        {
            user.Password = BC.HashPassword(newPassword);

            return await this.Update(user);
        }

        public async Task<bool> UpdatePasswordFromDTO(UserPasswordUpdateDTO dto)
        {
            var user = await this.Find(u => u.ID == dto.ID).FirstAsync();

            await this.UpdatePassword(user, dto.NewPassword);

            return true;
        }
    }
}
