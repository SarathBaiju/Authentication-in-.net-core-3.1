using JWTAuthentication.Constants;
using JWTAuthentication.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWTAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel registerViewModel)
        {
            try
            {
                if (await _userManager.FindByNameAsync(registerViewModel.UserName) != null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { status = "failed", message = "user already exist" });
                }

                var user = new IdentityUser
                {
                    UserName = registerViewModel.UserName,
                    Email = registerViewModel.Email
                };

                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    return Ok(new ResponseViewModel { Status = "Success", Message = "user created successfully" });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Ok();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginViewModel)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(loginViewModel.UserName);

                if (user != null && await _userManager.CheckPasswordAsync(user, loginViewModel.Password))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                    var token = new JwtSecurityToken
                        (
                            issuer: _configuration["JWT:ValidIssuer"],
                            audience: _configuration["JWT:ValidAudience"],
                            expires: DateTime.Now.AddHours(3),
                            claims: authClaims,
                            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), expiration = token.ValidTo });
                }
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseViewModel { Status = "failed", Message = "Invalid credentials" });
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterViewModel registerViewModel)
        {
            try
            {
                if (await _userManager.FindByNameAsync(registerViewModel.UserName) != null)
                {
                    return Ok(new ResponseViewModel { Status = "failed", Message = "user already exist" });
                }

                var user = new IdentityUser
                {
                    Email = registerViewModel.Email,
                    UserName = registerViewModel.UserName
                };

                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if(!await _roleManager.RoleExistsAsync(UserRole.ADMIN))
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRole.ADMIN));
                }

                if(await _roleManager.RoleExistsAsync(UserRole.ADMIN))
                {
                    await _userManager.AddToRoleAsync(user, UserRole.ADMIN);
                }

                return Ok(new ResponseViewModel { Status ="success", Message ="register admin user role completed"});
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
