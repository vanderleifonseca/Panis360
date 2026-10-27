using System.Globalization;
using System.Text.Json;
using Panis360.Contracts;
namespace Panis360.App;
public class MainPage : ContentPage {
 readonly MobileApi api=new();readonly LocalStore store=new();
 readonly VerticalStackLayout root=new(){Padding=20,Spacing=12};
 List<CustomerDto> customers=[];List<ProductDto> products=[];List<TermDto> terms=[];List<OrderDto> orders=[];List<ReceivableDto> titles=[];
 readonly List<OrderLineInput> cart=[];Guid? cartCustomer;bool loaded,busy;
 string Key(string name)=>api.Session!.UserId+":"+name;
 public MainPage(){Title="Panis360 · Vendas";BackgroundColor=Color.FromArgb("#F5F4EF");Content=new ScrollView{Content=root};}
 protected override async void OnAppearing(){base.OnAppearing();if(loaded)return;loaded=true;await Run(async()=>{
  var json=await SecureStorage.Default.GetAsync("session");
  if(json!=null){api.Session=JsonSerializer.Deserialize<LoginResult>(json);if(api.Session?.ExpiresAt<=DateTime.UtcNow)api.Session=null;}
  if(api.Session is null)ShowLogin();else{await ReadCache();await ShowHome();}
 });}
 void Heading(string text)=>root.Children.Add(new Label{Text=text,FontSize=23,FontAttributes=FontAttributes.Bold});
 void Text(string text)=>root.Children.Add(new Label{Text=text});
 Entry Field(string placeholder,bool password=false,Keyboard? keyboard=null){var e=new Entry{Placeholder=placeholder,IsPassword=password,Keyboard=keyboard??Keyboard.Default};if(!password){root.Children.Add(e);return e;}
  var toggle=new Button{Text="Mostrar",VerticalOptions=LayoutOptions.Center};toggle.Clicked+=(_,_)=>{e.IsPassword=!e.IsPassword;toggle.Text=e.IsPassword?"Mostrar":"Ocultar";};
  var row=new Grid{ColumnDefinitions=new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)),ColumnSpacing=8};row.Add(e,0);row.Add(toggle,1);root.Children.Add(row);return e;}
 void Action(string text,Func<Task> action){var b=new Button{Text=text};b.Clicked+=async(_,_)=>await Run(action);root.Children.Add(b);}
 async Task Run(Func<Task> action){if(busy)return;busy=true;try{await action();}catch(Exception e){await DisplayAlertAsync("Panis360",e.Message,"OK");}finally{busy=false;}}
 void ShowLogin(){root.Clear();Heading("Entrar no Panis360");Text("Conecte-se à API para o primeiro acesso.");var url=Field("Endereço da API");url.Text=api.Url;var email=Field("E-mail",keyboard:Keyboard.Email);var password=Field("Senha",true);
  Action("Entrar",async()=>{api.Url=url.Text?.Trim()??"";var session=await api.Login(email.Text??"",password.Text??"");if(session.Role is not ("Administrador" or "Vendedor"))throw new Exception("Aplicativo destinado a vendedores e administradores.");api.Session=session;Preferences.Set("api_url",api.Url);await SecureStorage.Default.SetAsync("session",JsonSerializer.Serialize(session));await ReadCache();await Refresh();await ShowHome();});
 }
 async Task ReadCache(){customers=await store.Read<List<CustomerDto>>(Key("customers"))??[];products=await store.Read<List<ProductDto>>(Key("products"))??[];terms=await store.Read<List<TermDto>>(Key("terms"))??[];orders=await store.Read<List<OrderDto>>(Key("orders"))??[];titles=await store.Read<List<ReceivableDto>>(Key("titles"))??[];}
 async Task Refresh(){customers=await api.Get<List<CustomerDto>>("/api/customers");products=await api.Get<List<ProductDto>>("/api/products");terms=await api.Get<List<TermDto>>("/api/terms");orders=await api.Get<List<OrderDto>>("/api/orders");titles=await api.Get<List<ReceivableDto>>("/api/receivables");
  await store.Cache(Key("customers"),customers);await store.Cache(Key("products"),products);await store.Cache(Key("terms"),terms);await store.Cache(Key("orders"),orders);await store.Cache(Key("titles"),titles);await store.Cache(Key("sync"),DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
 }
 async Task ShowHome(){root.Clear();Heading("Olá, "+api.Session!.Name);Text("Última atualização: "+(await store.Read<string>(Key("sync"))??"ainda não realizada"));Text("Dados locais podem estar desatualizados. Preços e disponibilidade serão conferidos no servidor.");
  Action("Atualizar carteira e catálogo",async()=>{await Refresh();await ShowHome();});
  Action("Clientes e roteiro",()=>{ShowCustomers();return Task.CompletedTask;});
  Action("Pedidos e cortes",()=>{root.Clear();Heading("Pedidos enviados");foreach(var o in orders){Text($"{o.CustomerName} · {o.Status} · R$ {o.Total:N2}");foreach(var i in o.Items)Text($"{i.ProductName}: solicitado {i.Requested}, atendido {i.Accepted}");Text(o.CutReason);}Back();return Task.CompletedTask;});
  var pending=await store.Pending(api.Session.UserId.ToString());Action($"Sincronizar pedidos pendentes ({pending.Count})",Sync);
  foreach(var p in pending)Text($"Pendente: {p.CustomerName} · {p.Error}");
  Action("Sair",async()=>{try{await api.Post("/api/logout",new{});}catch{}SecureStorage.Default.Remove("session");api.Session=null;customers=[];products=[];terms=[];orders=[];titles=[];cart.Clear();cartCustomer=null;ShowLogin();});
 }
 void Back()=>Action("Voltar",ShowHome);
 void ShowCustomers(){root.Clear();Heading("Carteira e sequência de visitas");Action("Cadastrar novo cliente (online)",()=>{ShowNewCustomer();return Task.CompletedTask;});foreach(var c in customers.OrderBy(x=>x.VisitOrder))Action($"{c.VisitOrder}. {c.Name} · {c.City}",()=>{ShowCustomer(c);return Task.CompletedTask;});Back();}
 void ShowCustomer(CustomerDto c){root.Clear();Heading(c.Name);Text($"{c.Document} · {c.Phone}\n{c.City}/{c.State}");Text(c.Blocked?"Compras bloqueadas":"Cliente ativo");Text($"Saldo em aberto: R$ {titles.Where(t=>t.CustomerId==c.Id).Sum(t=>t.Balance):N2}");
  foreach(var t in titles.Where(t=>t.CustomerId==c.Id && t.Balance>0))Text($"Vence {t.DueDate:dd/MM/yyyy}: R$ {t.Balance:N2}");
  var last=orders.FirstOrDefault(o=>o.CustomerId==c.Id && o.Status=="Vendido");if(last!=null){Text("Última compra:");foreach(var i in last.Items)Text($"{i.ProductName} · {i.Accepted} · R$ {i.UnitPrice:N2}");}
  Action("Novo pedido",()=>{if(c.Blocked)throw new Exception("Cliente bloqueado.");if(cartCustomer!=c.Id){cart.Clear();cartCustomer=c.Id;}ShowOrder(c);return Task.CompletedTask;});
  var note=Field("Observação da visita");Action("Registrar visita (online)",async()=>{await api.Post("/api/visits",new VisitInput(c.Id,note.Text??""));await DisplayAlertAsync("Visita","Registrada com sucesso.","OK");});Back();
 }
 void ShowNewCustomer(){root.Clear();Heading("Novo cliente");var name=Field("Nome / razão social");var doc=Field("CPF ou CNPJ",keyboard:Keyboard.Numeric);var phone=Field("Telefone",keyboard:Keyboard.Telephone);var street=Field("Logradouro");var number=Field("Número");var neighborhood=Field("Bairro");var city=Field("Cidade");city.Text="Foz do Iguaçu";var state=Field("UF");state.Text="PR";var zip=Field("CEP",keyboard:Keyboard.Numeric);var sequence=Field("Sequência de visita",keyboard:Keyboard.Numeric);
  Action("Salvar cliente",async()=>{await api.Post("/api/customers",new CustomerInput{Name=name.Text??"",Document=doc.Text??"",Phone=phone.Text??"",Street=street.Text??"",Number=number.Text??"",Neighborhood=neighborhood.Text??"",City=city.Text??"",State=state.Text??"",ZipCode=zip.Text??"",SellerId=api.Session!.UserId,VisitOrder=int.TryParse(sequence.Text,out var n)?n:0});await Refresh();ShowCustomers();});Back();
 }
 void ShowOrder(CustomerDto c){root.Clear();Heading("Pedido · "+c.Name);var picker=new Picker{Title="Produto",ItemsSource=products.Select(p=>$"{p.Name} · R$ {p.Price:N2}").ToList()};root.Children.Add(picker);var qty=Field("Quantidade",keyboard:Keyboard.Numeric);qty.Text="1";
  Action("Adicionar item",()=>{if(picker.SelectedIndex<0)throw new Exception("Selecione um produto.");if(!decimal.TryParse(qty.Text,NumberStyles.Number,CultureInfo.CurrentCulture,out var q)||q<=0)throw new Exception("Quantidade inválida.");var p=products[picker.SelectedIndex];if(cart.Any(x=>x.ProductId==p.Id))throw new Exception("Produto já incluído. Limpe os itens para refazer.");cart.Add(new(p.Id,q));ShowOrder(c);return Task.CompletedTask;});
  foreach(var item in cart)Text($"{products.First(p=>p.Id==item.ProductId).Name}: {item.Quantity}");
  Text($"Total estimado: R$ {cart.Sum(i=>Math.Round(i.Quantity*products.First(p=>p.Id==i.ProductId).Price,2,MidpointRounding.AwayFromZero)):N2}");
  var term=new Picker{Title="Condição de pagamento",ItemsSource=terms.Select(t=>t.Name).ToList()};root.Children.Add(term);var method=new Picker{Title="Forma de pagamento",ItemsSource=new List<string>{"Boleto","PIX","Dinheiro","Cartão"},SelectedIndex=0};root.Children.Add(method);
  Action("Salvar pedido na fila local",async()=>{if(cart.Count==0||term.SelectedIndex<0)throw new Exception("Adicione itens e selecione a condição.");var input=new OrderInput{CustomerId=c.Id,PaymentTermId=terms[term.SelectedIndex].Id,PaymentMethod=method.SelectedItem?.ToString()??"Boleto",Items=cart.ToList()};await store.Save(new PendingOrder{Id=input.ClientRequestId.ToString(),OwnerId=api.Session!.UserId.ToString(),CustomerName=c.Name,Json=JsonSerializer.Serialize(input)});cart.Clear();await ShowHome();});
  Action("Limpar itens",()=>{cart.Clear();ShowOrder(c);return Task.CompletedTask;});Back();
 }
 async Task Sync(){var pending=await store.Pending(api.Session!.UserId.ToString());int sent=0;foreach(var p in pending){try{var input=JsonSerializer.Deserialize<OrderInput>(p.Json)!;await api.Post("/api/orders",input);await store.Delete(p.Id);sent++;}catch(Exception e){p.Error=e.Message;await store.Save(p);}}
  try{await Refresh();}catch(Exception e){await DisplayAlertAsync("Atualização",e.Message,"OK");}await ShowHome();await DisplayAlertAsync("Sincronização",$"{sent} de {pending.Count} pedidos enviados. Confira as pendências na tela inicial.","OK");
 }
}
