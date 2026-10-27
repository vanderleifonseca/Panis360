using SQLite;
using System.Text.Json;
namespace Panis360.App;
public class CachedData {
 [PrimaryKey] public string Key {get;set;}="";
 public string Json {get;set;}="";
}
public class PendingOrder {
 [PrimaryKey] public string Id {get;set;}="";
 public string OwnerId {get;set;}="";
 public string CustomerName {get;set;}="";
 public string Json {get;set;}="";
 public string Error {get;set;}="";
}
public class LocalStore {
 readonly SQLiteAsyncConnection db=new(Path.Combine(FileSystem.AppDataDirectory,"panis360-local.db"));
 readonly SemaphoreSlim init=new(1,1);bool ready;
 async Task Ensure(){await init.WaitAsync();try{if(ready)return;await db.CreateTableAsync<CachedData>();await db.CreateTableAsync<PendingOrder>();ready=true;}finally{init.Release();}}
 public async Task Cache<T>(string key,T value){await Ensure();await db.InsertOrReplaceAsync(new CachedData{Key=key,Json=JsonSerializer.Serialize(value)});}
 public async Task<T?> Read<T>(string key){await Ensure();var c=await db.FindAsync<CachedData>(key);return c is null?default:JsonSerializer.Deserialize<T>(c.Json);}
 public async Task Save(PendingOrder p){await Ensure();await db.InsertOrReplaceAsync(p);}
 public async Task<List<PendingOrder>> Pending(string owner){await Ensure();return await db.Table<PendingOrder>().Where(x=>x.OwnerId==owner).ToListAsync();}
 public async Task Delete(string id){await Ensure();await db.DeleteAsync<PendingOrder>(id);}
}
