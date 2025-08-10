using AutoMapper;
using Seek.Core.Dtos;
using Seek.Core.IRepositories.Agent;
using Seek.Core.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.EF.Repositories.Agent
{
    public class Repo_Authentication_Service_Manager : IRepo_Authentication_Service_Manager
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public Repo_Authentication_Service_Manager(ApplicationDbContext db, IMapper mapper)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public Task<auth_model> LoginAsync(auth_login_dto dto, CancellationToken cancellationToken = default)
        {
            // Implement the login logic here
            // This is a placeholder implementation.
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrEmpty(dto.Login) || string.IsNullOrEmpty(dto.Password))
                throw new ArgumentException("Login and Password must be provided.");
            // Here you would typically check the credentials against a database
            // and return an auth_model if successful.
            // For now, we will return a dummy auth_model.
            
            throw new NotImplementedException();
        }

        public Task LogoutAsync(int userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<auth_responses_dto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<auth_model> RegisterAsync(auth_register_dto dto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<auth_model> UpdateUserAsync(string token, auth_responses_dto dto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
