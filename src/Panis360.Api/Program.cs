using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Panis360.Api;
using Panis360.Application;
using Panis360.Contracts;
using Panis360.Domain;
using Panis360.Infrastructure;
var builder=WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json",optional:true).AddEnvironmentVariables();
builder.Services.AddDbContext<AppDb>(o=> {
 if(builder.Configuration["Database:Provider"]=="PostgreSQL") o.UseNpgsql(builder.Configuration.GetConnectionString("Main") ?? throw new InvalidOperationException("Configure ConnectionStrings:Main."));
 else o.UseSqlite(builder.Configuration.GetConnectionString("Main") ?? "Data Source=panis360.db");
});
builder.Services.AddScoped<Operations>();
builder.Services.AddSingleton<IPasswordHasher<User>,PasswordHasher<User>>();
builder.Services.AddAuthentication("Session").AddScheme<AuthenticationSchemeOptions,SessionAuth>("Session",_=>{});
builder.Services.AddAuthorization(o=>{
 o.AddPolicy("Admin",p=>p.RequireRole("Administrador"));
 o.AddPolicy("Commercial",p=>p.RequireRole("Administrador","Vendedor"));
 o.AddPolicy("Warehouse",p=>p.RequireRole("Administrador","Estoque"));
 o.AddPolicy("Finance",p=>p.RequireRole("Administrador","Financeiro"));
});
builder.Services.AddRateLimiter(o=> {
 o.RejectionStatusCode=429;
 o.AddPolicy("login",context=>RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString()??"local",_=>new FixedWindowRateLimiterOptions{PermitLimit=10,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));
});
var app=builder.Build();
if(!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.Use(async(context,next)=> {
 try { await next(context); }
 catch(BusinessException e) { context.Response.StatusCode=400;await context.Response.WriteAsJsonAsync(new{error=e.Message}); }
 catch(DbUpdateConcurrencyException) { context.Response.StatusCode=409;await context.Response.WriteAsJsonAsync(new{error="Dados alterados por outro usuário. Atualize e tente novamente."}); }
 catch(DbUpdateException e) { app.Logger.LogWarning(e,"Conflito de persistência");context.Response.StatusCode=409;await context.Response.WriteAsJsonAsync(new{error="Operação em conflito ou registro duplicado. Atualize os dados antes de repetir."}); }
 catch(Exception e) when(e is not OperationCanceledException) {app.Logger.LogError(e,"Erro na operação");context.Response.StatusCode=500;await context.Response.WriteAsJsonAsync(new{error="Falha interna. Consulte o log da API."});}
});
app.UseRateLimiter(); app.UseAuthentication();app.UseAuthorization();
using(var scope=app.Services.CreateScope()) {
 var db=scope.ServiceProvider.GetRequiredService<AppDb>();
 if(app.Environment.IsDevelopment() && builder.Configuration.GetValue("Database:CreateSchema",true)) { await db.Database.EnsureCreatedAsync(); await SchemaUpgrade.Apply(db); }
 if(!await db.Users.AnyAsync()) {
  var password=builder.Configuration["Bootstrap:Password"];
  if(string.IsNullOrWhiteSpace(password)||password.Length<12) throw new InvalidOperationException("Primeiro acesso: configure Bootstrap:Password com pelo menos 12 caracteres. Consulte README.md.");
  var admin=new User{Name="Administrador",Email=(builder.Configuration["Bootstrap:Email"]??"admin@panis360.local").Trim().ToLowerInvariant(),Role="Administrador"};
  admin.PasswordHash=scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>().HashPassword(admin,password);
  db.Users.Add(admin);
  db.Terms.AddRange(new PaymentTerm{Name="À vista",Parts=[new(){Days=0,Percent=100}]},new PaymentTerm{Name="7/14 dias",Parts=[new(){Days=7,Percent=50},new(){Days=14,Percent=50}]},new PaymentTerm{Name="7/14/21 dias",Parts=[new(){Days=7,Percent=33.33m},new(){Days=14,Percent=33.33m},new(){Days=21,Percent=33.34m}]});
  await db.SaveChangesAsync();
 }
}
app.MapGet("/health",()=>Results.Ok(new{status="ok",system="Panis360"}));
app.MapPost("/auth/login",async(LoginRequest input,AppDb db,IPasswordHasher<User> hasher)=> {
 var user=await db.Users.SingleOrDefaultAsync(x=>x.Email==input.Email.Trim().ToLowerInvariant() && x.Active);
 if(user is null || hasher.VerifyHashedPassword(user,user.PasswordHash,input.Password)==PasswordVerificationResult.Failed) return Results.Unauthorized();
 var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));var expiry=DateTime.UtcNow.AddHours(12);
 db.Sessions.Add(new Session{UserId=user.Id,TokenHash=SessionAuth.Hash(token),ExpiresAt=expiry});await db.SaveChangesAsync();
 return Results.Ok(new LoginResult(token,user.Id,user.Name,user.Role,expiry));
}).RequireRateLimiting("login");
var api=app.MapGroup("/api").RequireAuthorization();
api.MapPost("/logout",async(HttpContext c,AppDb db)=>{
 var token=c.Request.Headers.Authorization.ToString()[7..];var hash=SessionAuth.Hash(token);
 var sessions=await db.Sessions.Where(x=>x.TokenHash==hash).ToListAsync();db.Sessions.RemoveRange(sessions);await db.SaveChangesAsync();return Results.NoContent();
});
api.MapGet("/sellers",async(AppDb db)=>await db.Users.Where(x=>x.Active && (x.Role=="Vendedor"||x.Role=="Administrador")).Select(x=>new SellerDto(x.Id,x.Name)).ToListAsync()).RequireAuthorization("Commercial");
api.MapPost("/users",async(UserInput input,AppDb db,IPasswordHasher<User> hasher,ClaimsPrincipal actor,Operations ops)=>{
 Validate(input);Rules.Require(Rules.Roles.Contains(input.Role),"Perfil inválido.");
 var user=new User{Name=input.Name.Trim(),Email=input.Email.Trim().ToLowerInvariant(),Role=input.Role};user.PasswordHash=hasher.HashPassword(user,input.Password);db.Users.Add(user);ops.Audit(Actor.Id(actor),"Usuário cadastrado",user.Id);await db.SaveChangesAsync();return Results.Ok(new SellerDto(user.Id,user.Name));
}).RequireAuthorization("Admin");
api.MapGet("/customers",async(AppDb db,ClaimsPrincipal actor)=>{
 var q=db.Customers.AsNoTracking();if(actor.IsInRole("Vendedor"))q=q.Where(x=>x.SellerId==Actor.Id(actor));
 return await q.OrderBy(x=>x.VisitOrder).ThenBy(x=>x.Name).Select(x=>new CustomerDto(x.Id,x.Name,x.Document,x.Phone,x.City,x.State,x.SellerId,x.VisitOrder,x.Blocked)).ToListAsync();
});
api.MapPost("/customers",async(CustomerInput input,AppDb db,ClaimsPrincipal actor,Operations ops)=>{
 Validate(input);var seller=actor.IsInRole("Vendedor")?Actor.Id(actor):input.SellerId;
 Rules.Require(await db.Users.AnyAsync(x=>x.Id==seller && x.Active && (x.Role=="Vendedor"||x.Role=="Administrador")),"Vendedor inválido.");
 var doc=new string(input.Document.Where(char.IsDigit).ToArray());Rules.Require(doc.Length is 11 or 14,"Informe CPF com 11 ou CNPJ com 14 dígitos.");
 var c=new Customer{Name=input.Name.Trim(),Document=doc,Phone=input.Phone,Street=input.Street,Number=input.Number,Neighborhood=input.Neighborhood,City=input.City,State=input.State.ToUpperInvariant(),ZipCode=input.ZipCode,SellerId=seller,VisitOrder=input.VisitOrder,Blocked=actor.IsInRole("Administrador")&&input.Blocked};
 db.Customers.Add(c);ops.Audit(Actor.Id(actor),"Cliente cadastrado",c.Id);await db.SaveChangesAsync();return Results.Ok(c.Id);
}).RequireAuthorization("Commercial");
api.MapPut("/customers/{id:guid}/block",async(Guid id,bool blocked,AppDb db,ClaimsPrincipal actor,Operations ops)=>{
 var c=await db.Customers.FindAsync(id)??throw new BusinessException("Cliente inexistente.");c.Blocked=blocked;ops.Audit(Actor.Id(actor),blocked?"Cliente bloqueado":"Cliente desbloqueado",id);await db.SaveChangesAsync();return Results.NoContent();
}).RequireAuthorization("Admin");
api.MapGet("/products",async(AppDb db)=>{
 var products=await db.Products.Where(x=>x.Active).OrderBy(x=>x.Name).ToListAsync();var lots=await db.Lots.Where(x=>x.ExpiresOn>=Operations.Today).ToListAsync();
 return products.Select(x=>new ProductDto(x.Id,x.Name,x.Group,x.Unit,x.Price,lots.Where(l=>l.ProductId==x.Id).Sum(l=>l.Quantity)));
});
api.MapPost("/products",async(ProductInput input,AppDb db,ClaimsPrincipal actor,Operations ops)=>{
 Validate(input);Rules.Require(new[]{"UN","KG","L"}.Contains(input.Unit),"Unidade inválida.");Rules.Require(Rules.Money(input.Price)==input.Price,"Preço deve ter até 2 casas decimais.");
 var p=new Product{Name=input.Name.Trim(),Group=input.Group,Unit=input.Unit,Price=input.Price,Active=input.Active};db.Products.Add(p);ops.Audit(Actor.Id(actor),"Produto cadastrado",p.Id);await db.SaveChangesAsync();return Results.Ok(p.Id);
}).RequireAuthorization("Admin");
api.MapGet("/lots",async(AppDb db)=>await (from lot in db.Lots join p in db.Products on lot.ProductId equals p.Id orderby lot.ExpiresOn select new LotDto(lot.Id,p.Id,p.Name,lot.Code,lot.ExpiresOn,lot.Quantity,p.Unit,lot.Revision)).ToListAsync()).RequireAuthorization("Warehouse");
api.MapGet("/lots/{id:guid}/movements",async(Guid id,AppDb db)=>{
 Rules.Require(await db.Lots.AnyAsync(x=>x.Id==id),"Lote inexistente.");var balance=0m;
 var moves=(await db.Movements.Where(x=>x.StockLotId==id).ToListAsync()).OrderBy(x=>x.Date);
 var ids=moves.Where(x=>x.ActorId!=null).Select(x=>x.ActorId!.Value).Distinct().ToList();var names=await db.Users.Where(x=>ids.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.Name);
 return moves.Select(x=>new MovementDto(DateTime.SpecifyKind(x.Date,DateTimeKind.Utc),x.Quantity,balance+=x.Quantity,x.Reason,x.SaleId,x.ActorId is Guid a && names.TryGetValue(a,out var n)?n:"—")).Reverse().ToList();
}).RequireAuthorization("Warehouse");
api.MapPut("/lots/{id:guid}",async(Guid id,LotEditInput input,AppDb db,ClaimsPrincipal actor,Operations ops)=>{
 var lot=await db.Lots.FindAsync(id)??throw new BusinessException("Lote inexistente.");var p=await db.Products.FindAsync(lot.ProductId);var code=(input.Code??"").Trim();var reason=(input.Reason??"").Trim();
 Rules.Require(code.Length>0,"Informe o código do lote.");Rules.Require(reason.Length>0 && reason.Length<=200,"Informe o motivo do ajuste (até 200 caracteres).");
 Rules.Require(input.Quantity>=0 && input.Quantity<=1000000 && decimal.Round(input.Quantity,3)==input.Quantity,"Saldo deve ser de 0 a 1.000.000, com até 3 casas decimais.");Rules.Require(p!.Unit!="UN"||decimal.Truncate(input.Quantity)==input.Quantity,"UN exige quantidade inteira.");
 Rules.Require(!await db.Lots.AnyAsync(x=>x.ProductId==lot.ProductId && x.Code==code && x.Id!=id),"Já existe outro lote com este código para o produto.");
 db.Entry(lot).Property(x=>x.Revision).OriginalValue=input.Revision;var delta=input.Quantity-lot.Quantity;
 if(delta!=0)db.Movements.Add(new StockMovement{StockLotId=lot.Id,ActorId=Actor.Id(actor),Quantity=delta,Reason="Ajuste: "+reason});
 lot.Code=code;lot.ExpiresOn=input.ExpiresOn;lot.Quantity=input.Quantity;lot.Revision++;ops.Audit(Actor.Id(actor),"Lote ajustado",lot.Id);await db.SaveChangesAsync();return Results.NoContent();
}).RequireAuthorization("Warehouse");
api.MapPost("/lots",async(LotInput input,AppDb db,ClaimsPrincipal actor,Operations ops)=>{
 Rules.Quantity(input.Quantity);Rules.Require(input.ExpiresOn>=Operations.Today && !string.IsNullOrWhiteSpace(input.Code),"Informe lote e validade não vencida.");
 var p=await db.Products.FindAsync(input.ProductId)??throw new BusinessException("Produto inválido.");Rules.Require(p.Unit!="UN"||decimal.Truncate(input.Quantity)==input.Quantity,"UN exige quantidade inteira.");
 var lot=new StockLot{ProductId=p.Id,Code=input.Code.Trim(),ExpiresOn=input.ExpiresOn,Quantity=input.Quantity};db.Lots.Add(lot);db.Movements.Add(new StockMovement{StockLotId=lot.Id,ActorId=Actor.Id(actor),Quantity=input.Quantity,Reason="Entrada inicial"});ops.Audit(Actor.Id(actor),"Lote recebido",lot.Id);await db.SaveChangesAsync();return Results.Ok(lot.Id);
}).RequireAuthorization("Warehouse");
api.MapGet("/terms",async(AppDb db)=>(await db.Terms.Include(x=>x.Parts).ToListAsync()).Select(x=>new TermDto(x.Id,x.Name,x.Parts.OrderBy(p=>p.Days).Select(p=>new TermPartDto(p.Days,p.Percent)).ToList())));
api.MapPost("/terms",async(TermInput input,AppDb db)=>{
 Rules.Require(!string.IsNullOrWhiteSpace(input.Name),"Nome obrigatório.");Rules.Split(100000m,input.Parts);
 var t=new PaymentTerm{Name=input.Name.Trim(),Parts=input.Parts.Select(x=>new PaymentTermPart{Days=x.Days,Percent=x.Percent}).ToList()};db.Terms.Add(t);await db.SaveChangesAsync();return Results.Ok(t.Id);
}).RequireAuthorization("Admin");
api.MapGet("/orders",async(AppDb db,ClaimsPrincipal actor)=>{
 var q=db.Orders.AsNoTracking().Include(x=>x.Items).AsQueryable();if(actor.IsInRole("Vendedor"))q=q.Where(x=>x.SellerId==Actor.Id(actor));
 var list=await q.OrderByDescending(x=>x.CreatedAt).ToListAsync();var customers=await db.Customers.ToDictionaryAsync(x=>x.Id,x=>x.Name);
 return list.Select(x=>new OrderDto(x.Id,x.CustomerId,customers[x.CustomerId],x.CreatedAt,x.Status,x.CutReason,x.Items.Sum(i=>Rules.Money(i.Accepted*i.UnitPrice)),x.Items.Select(i=>new OrderLineDto(i.Id,i.ProductId,i.ProductName,i.Requested,i.Accepted,i.UnitPrice)).ToList()));
});
api.MapPost("/orders",async(OrderInput input,Operations ops,ClaimsPrincipal actor)=>Results.Ok(await ops.CreateOrder(input,Actor.Id(actor),actor.IsInRole("Administrador")))).RequireAuthorization("Commercial");
api.MapPut("/orders/{id:guid}/cut",async(Guid id,CutInput input,Operations ops,ClaimsPrincipal actor)=>{await ops.Cut(id,input,Actor.Id(actor));return Results.NoContent();}).RequireAuthorization("Warehouse");
api.MapPost("/orders/{id:guid}/sell",async(Guid id,Operations ops,ClaimsPrincipal actor)=>Results.Ok(await ops.ConfirmSale(id,Actor.Id(actor)))).RequireAuthorization("Warehouse");
api.MapPost("/orders/{id:guid}/cancel",async(Guid id,AppDb db,Operations ops,ClaimsPrincipal actor)=>{
 var order=await db.Orders.FindAsync(id)??throw new BusinessException("Pedido inexistente.");Rules.Require(order.Status=="Recebido","Somente pedido não vendido pode ser cancelado.");order.Status="Cancelado";order.Revision++;ops.Audit(Actor.Id(actor),"Pedido cancelado",id);await db.SaveChangesAsync();return Results.NoContent();
}).RequireAuthorization("Admin");
api.MapGet("/receivables",async(AppDb db,ClaimsPrincipal actor)=>{
 var customers=db.Customers.AsNoTracking();if(actor.IsInRole("Vendedor"))customers=customers.Where(x=>x.SellerId==Actor.Id(actor));
 return await (from r in db.Receivables join c in customers on r.CustomerId equals c.Id orderby r.DueDate select new ReceivableDto(r.Id,c.Id,c.Name,r.Number,r.DueDate,r.Amount,r.Paid,r.PaymentMethod)).ToListAsync();
}).RequireAuthorization(p=>p.RequireRole("Administrador","Vendedor","Financeiro"));
api.MapPost("/receivables/{id:guid}/receive",async(Guid id,ReceiptInput input,Operations ops,ClaimsPrincipal actor)=>{await ops.Receive(id,input,Actor.Id(actor));return Results.NoContent();}).RequireAuthorization("Finance");
api.MapPost("/visits",async(VisitInput input,AppDb db,ClaimsPrincipal actor,Operations ops)=>{
 Rules.Require(await db.Customers.AnyAsync(x=>x.Id==input.CustomerId && (actor.IsInRole("Administrador")||x.SellerId==Actor.Id(actor))),"Cliente fora da carteira.");var v=new Visit{CustomerId=input.CustomerId,SellerId=Actor.Id(actor),Note=input.Note};db.Visits.Add(v);ops.Audit(Actor.Id(actor),"Visita registrada",v.Id);await db.SaveChangesAsync();return Results.Ok(v.Id);
}).RequireAuthorization("Commercial");
api.MapGet("/visits",async(AppDb db,ClaimsPrincipal actor)=>await db.Visits.Where(x=>actor.IsInRole("Administrador")||x.SellerId==Actor.Id(actor)).OrderByDescending(x=>x.Date).ToListAsync()).RequireAuthorization("Commercial");
api.MapGet("/dashboard",async(AppDb db)=> {
 var sales=await db.Sales.ToListAsync();var titles=await db.Receivables.ToListAsync();var lots=await db.Lots.ToListAsync();
 return new DashboardDto(await db.Customers.CountAsync(),await db.Orders.CountAsync(),sales.Sum(x=>x.Total),titles.Sum(x=>x.Amount-x.Paid),lots.Count(x=>x.Quantity>0 && x.ExpiresOn<=Operations.Today.AddDays(7)));
}).RequireAuthorization("Admin");
api.MapGet("/audit",async(AppDb db)=>await db.Audits.AsNoTracking().OrderByDescending(x=>x.Date).Take(200).ToListAsync()).RequireAuthorization("Admin");
app.Run();

static void Validate(object input) {
 var errors=new List<ValidationResult>();
 if(!Validator.TryValidateObject(input,new ValidationContext(input),errors,true))throw new BusinessException(string.Join(" ",errors.Select(x=>x.ErrorMessage)));
}
