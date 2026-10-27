using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Panis360.Infrastructure;
namespace Panis360.Api;
public class SessionAuth(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder,AppDb db)
 : AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder) {
 public static string Hash(string token)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
 protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
  var header=Request.Headers.Authorization.ToString();
  if(!header.StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase)) return AuthenticateResult.NoResult();
  var hash=Hash(header[7..]);
  var session=await db.Sessions.AsNoTracking().SingleOrDefaultAsync(x=>x.TokenHash==hash && x.ExpiresAt>DateTime.UtcNow);
  if(session is null) return AuthenticateResult.Fail("Sessão expirada ou inválida.");
  var user=await db.Users.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==session.UserId && x.Active);
  if(user is null) return AuthenticateResult.Fail("Usuário inativo.");
  var identity=new ClaimsIdentity([new(ClaimTypes.NameIdentifier,user.Id.ToString()),new(ClaimTypes.Name,user.Name),new(ClaimTypes.Role,user.Role)],Scheme.Name);
  return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity),Scheme.Name));
 }
}
