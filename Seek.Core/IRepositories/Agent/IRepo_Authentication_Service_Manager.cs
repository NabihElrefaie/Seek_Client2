using Seek.Core.Dtos;
using Seek.Core.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.IRepositories.Agent
{
    public interface IRepo_Authentication_Service_Manager
    {
        Task<auth_model> RegisterAsync(auth_register_dto dto, CancellationToken cancellationToken = default);
        Task<auth_model> LoginAsync(auth_login_dto dto, CancellationToken cancellationToken = default);
        //internal request
        Task<auth_model> UpdateUserAsync(string token, auth_responses_dto dto, CancellationToken cancellationToken = default);
        Task<auth_responses_dto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
        Task LogoutAsync(int userId, CancellationToken cancellationToken = default);
    }
}
