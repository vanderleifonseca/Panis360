using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Panis360.Contracts;
namespace Panis360.App;
public class MobileApi {
 public string Url {get;set;}=Preferences.Get("api_url","http://127.0.0.1:5207");
 public LoginResult? Session{get;set;}
 public async Task<LoginResult> Login(string email,string password)=>await Send<LoginResult>(HttpMethod.Post,"/auth/login",new LoginRequest(email,password));
 public Task<T> Get<T>(string path)=>Send<T>(HttpMethod.Get,path,null);
 public Task<object> Post(string path,object body)=>Send<object>(HttpMethod.Post,path,body);
 async Task<T> Send<T>(HttpMethod method,string path,object? body){
  using var client=new HttpClient{BaseAddress=new Uri(Url.TrimEnd('/')),Timeout=TimeSpan.FromSeconds(25)};
  using var req=new HttpRequestMessage(method,path);
  if(Session!=null)req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",Session.Token);
  if(body!=null)req.Content=JsonContent.Create(body);
  using var response=await client.SendAsync(req);
  if(!response.IsSuccessStatusCode){
   if(response.StatusCode==System.Net.HttpStatusCode.Unauthorized)throw new Exception(path=="/auth/login"?"E-mail ou senha inválidos.":"Sessão inválida ou expirada. Entre novamente para sincronizar.");
   var raw=await response.Content.ReadAsStringAsync();string error="Operação recusada: "+response.StatusCode;
   try{error=JsonDocument.Parse(raw).RootElement.GetProperty("error").GetString()??error;}catch(JsonException){}catch(KeyNotFoundException){}
   throw new Exception(error);
  }
  if(response.StatusCode==System.Net.HttpStatusCode.NoContent)return default!;
  return (await response.Content.ReadFromJsonAsync<T>())!;
 }
}
