using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AccountManagerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Net.Http.Headers;

namespace AccountManagerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfosController : ControllerBase
    {
        private readonly AccountDbContext _context;

        public UserInfosController(AccountDbContext context)
        {
            _context = context;
        }

        //Get Claim from headers
        public string GetClaim(string token, string claimType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            var stringClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return stringClaimValue;
        }

        // GET: api/UserInfos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserInfo>>> GetUserInfo()
        {
            //Gets the user token and claims
            var jwt = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", string.Empty);
            
            var roleclaim = GetClaim(jwt, "Role");
            var groupclaim = GetClaim(jwt, "Group");
            var Ownerclaim = GetClaim(jwt, "OwnerID");

            //Get current user id AND infos
            var userId = Int32.Parse(GetClaim(jwt, "Id"));
            var LoggeduserInfo = await _context.UserInfo.FindAsync(userId);

            //Create a separate list of all user infos for filtering
            var userList = _context.UserInfo.ToList();
            var newUserList = new List<UserInfo>();
            for (int i=0; i < userList.Count(); i++)
            {
                //if normal user, return his/her infos only
                if (groupclaim.Contains("User") || roleclaim != "0")
                {
                    newUserList.Add(LoggeduserInfo);
                    return newUserList;
                }

                if (userList[i].UserOwnerId != null && userList[i].UserOwnerId == LoggeduserInfo.UserId.ToString() && roleclaim == "1")
                {
                    newUserList.Add(userList[i]);
                    continue;
                }

                //else return everything to admin and super users
                newUserList.Add(userList[i]);

            }
            return newUserList;
        }

        // GET: api/UserInfos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserInfo>> GetUserInfo(int id)
        {
            //Gets the user token and claims
            var jwt = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", string.Empty);

            var roleclaim = GetClaim(jwt, "Role");
            var groupclaim = GetClaim(jwt, "Group");
            var userId = Int32.Parse(GetClaim(jwt, "Id"));
            // Get the current logged id
            var LoggeduserInfo = await _context.UserInfo.FindAsync(userId);
            var userInfo = await _context.UserInfo.FindAsync(id);
            //Return normal user infos if not admin
            if (groupclaim.Contains("User") || roleclaim != "0" && userId != id)
            {
                return LoggeduserInfo;
            }

            if (userInfo == null)
            {
                return NotFound();
            }

            return userInfo;
        }

        // PUT: api/UserInfos/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserInfo(int id, UserInfo userInfo)
        {
            //Gets the user token and claims
            var jwt = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", string.Empty);

            var roleclaim = GetClaim(jwt, "Role");
            var groupclaim = GetClaim(jwt, "Group");
            var userId = Int32.Parse(GetClaim(jwt, "Id"));
            var ownerId = GetClaim(jwt, "OwnerID");

            // Verifies the user claims through the Tokken before anything
            if (roleclaim != "1" || roleclaim != "0" && userId != userInfo.UserId)
            {
                return Forbid();
            }

            if (id != userInfo.UserId)
            {
                return BadRequest();
            }

            if (roleclaim != "0")
            {
                userInfo.UserOwnerId = ownerId;
                userInfo.UserRole = Int32.Parse(roleclaim);
                userInfo.UserGroup = groupclaim;
                userInfo.UserId = userId;
            }
            _context.Entry(userInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserInfos
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<UserInfo>> PostUserInfo(UserInfo userInfo)
        {
            //Gets the user token and claims
            var jwt = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", string.Empty);

            var roleclaim = GetClaim(jwt, "Role");
            var groupclaim = GetClaim(jwt, "Group");
            var userId = Int32.Parse(GetClaim(jwt, "Id"));

            // Verifies the user claims through the Tokken before anything
            if (roleclaim != "1" || roleclaim != "0")
            {
                return Forbid();
            }

            userInfo.UserOwnerId = userId.ToString();
            _context.UserInfo.Add(userInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserInfo", new { id = userInfo.UserId }, userInfo);
        }

        // DELETE: api/UserInfos/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserInfo>> DeleteUserInfo(int id)
        {
            //Gets the user token and claims
            var jwt = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", string.Empty);

            var roleclaim = GetClaim(jwt, "Role");
            var groupclaim = GetClaim(jwt, "Group");
            // Verifies the user claims through the Tokken before anything
            if (groupclaim.Contains("User") || roleclaim != "0")
            {
                return Forbid();
            }

            var userInfo = await _context.UserInfo.FindAsync(id);
            if (userInfo == null)
            {
                return NotFound();
            }

            _context.UserInfo.Remove(userInfo);
            await _context.SaveChangesAsync();

            return userInfo;
        }

        private bool UserInfoExists(int id)
        {
            return _context.UserInfo.Any(e => e.UserId == id);
        }
    }
}
