using IdentityService.Api.Application.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Api.Application.Services
{
    public class IdentityService : IIdentityService
    {
        public Task<LoginResponseModel> Login(LoginRequestModel loginRequestModel)
        {
            // Db proccesses will be here ,Check if user info valid and get info

            var claims = new Claim[]
            {
                new(ClaimTypes.NameIdentifier ,loginRequestModel.UserName),
                new(ClaimTypes.Name ,"Javid Ibra")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TechBuddySeecretKeyShouldBeLongForTestingPurpose"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(claims:claims,expires:expiry,signingCredentials : creds,notBefore : DateTime.Now);

            var encodedJwt =new JwtSecurityTokenHandler().WriteToken(token);

            LoginResponseModel response = new()
            {
                UserName = loginRequestModel.UserName,
                UserToken = encodedJwt,
            }; 

           return Task.FromResult(response);
        }
    }
}
