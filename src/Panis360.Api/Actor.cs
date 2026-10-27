using System.Security.Claims;
namespace Panis360.Api;
public static class Actor { public static Guid Id(ClaimsPrincipal actor)=>Guid.Parse(actor.FindFirstValue(ClaimTypes.NameIdentifier)!); }
