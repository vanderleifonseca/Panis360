namespace Panis360.App;
public static class MauiProgram {
 public static MauiApp CreateMauiApp() {
  SQLitePCL.Batteries_V2.Init();
  var builder=MauiApp.CreateBuilder();builder.UseMauiApp<App>();
  return builder.Build();
 }
}
