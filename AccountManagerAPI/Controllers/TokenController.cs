using AccountManagerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace AccountManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly AccountDbContext _context;

        public TokenController(IConfiguration config, AccountDbContext context)
        {
            _configuration = config;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post(UserInfo _userData)
        {

            if (_userData != null && _userData.Email != null && _userData.Password != null)
            {
                var user = await GetUser(_userData.Email, _userData.Password);

                if (user != null)
                {
                    //create claims details based on the user information
                    Claim[] claims;
                    if (user.UserRole == 0 || user.UserRole == 1)
                    {
                        claims = new[] {
                            new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                            new Claim("Id", user.UserId.ToString()),
                            new Claim("FirstName", user.FirstName),
                            new Claim("LastName", user.LastName),
                            new Claim("UserName", user.UserName),
                            new Claim("Email", user.Email),
                            new Claim("Role",user.UserRole.ToString()),
                            new Claim("Group",user.UserGroup),
                        };
                    }
                    else
                    {
                        claims = new[] {
                            new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                            new Claim("Id", user.UserId.ToString()),
                            new Claim("FirstName", user.FirstName),
                            new Claim("LastName", user.LastName),
                            new Claim("UserName", user.UserName),
                            new Claim("Email", user.Email),
                            new Claim("Role",user.UserRole.ToString()),
                            new Claim("OwnerID",user.UserOwnerId.ToString()),
                            new Claim("Group",user.UserGroup),
                        };
                    }
                    
                        

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    //var vRsaSecurityKey = new Microsoft.IdentityModel.Tokens.RsaSecurityKey(RSA.Create(2048));
                    //var vSigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(vRsaSecurityKey, SecurityAlgorithms.RsaSha256Signature);

                    var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: signIn);

                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                else
                {
                    return BadRequest("Invalid credentials");
                }
            }
            else
            {
                return BadRequest();
            }
        }

        private async Task<UserInfo> GetUser(string email, string password)
        {
            return await _context.UserInfo.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }
    }
}
