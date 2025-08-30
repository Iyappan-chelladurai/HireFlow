using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HireFlow_API.Services
{
    public class JwtTokenService
    {
        private  readonly JwtSettings _jwtSettings;

        public JwtTokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public  string GenerateToken(UserAccount user,CandidateDetail candidate , string role)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var candidateid = candidate == null ? "" : candidate.CandidateId.ToString();


            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.ToString()),  
                new Claim(ClaimTypes.Name, user.UserName),                 
                new Claim(ClaimTypes.Email, user.Email),                    
                new Claim("FullName", user.FullName),
                new Claim("candidateid",candidateid),    
                new Claim(ClaimTypes.Role, role),                      
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
