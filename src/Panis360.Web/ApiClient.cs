using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Panis360.Contracts;
namespace Panis360.Web;
public sealed class ApiClient(IHttpClientFactory factory) {
 public LoginResult? Session { get; private set; }
 public bool HasRole(params string[] roles)=>Session is not null && roles.Contains(Session.Role);
 public async Task Login(string email,string password) { Session=await Send<LoginResult>(HttpMethod.Post,"/auth/login",new LoginRequest(email,password)); }
 public async Task Logout() { try { await Post("/api/logout",new{}); } finally { Session=null; } }
 public Task<T> Get<T>(string path)=>Send<T>(HttpMethod.Get,path,null);
 public Task Post(string path,object body)=>Send<object>(HttpMethod.Post,path,body);
 public Task Put(string path,object body)=>Send<object>(HttpMethod.Put,path,body);
 async Task<T> Send<T>(HttpMethod method,string path,object? body) {
  using var client=factory.CreateClient("Api");using var req=new HttpRequestMessage(method,path);
  if(Session!=null)req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",Session.Token);
  if(body!=null)req.Content=JsonContent.Create(body);
  using var response=await client.SendAsync(req);
  if(!response.IsSuccessStatusCode) {
   if(response.StatusCode==System.Net.HttpStatusCode.Unauthorized){Session=null;throw new Exception("Acesso inválido ou sessão expirada. Entre novamente.");}
   var raw=await response.Content.ReadAsStringAsync();string error="Operação recusada ("+(int)response.StatusCode+").";
   try {error=JsonDocument.Parse(raw).RootElement.GetProperty("error").GetString()??error;}catch(JsonException){}catch(KeyNotFoundException){}
   throw new Exception(error);
  }
  if(response.StatusCode==System.Net.HttpStatusCode.NoContent)return default!;
  return (await response.Content.ReadFromJsonAsync<T>())!;
 }
}
