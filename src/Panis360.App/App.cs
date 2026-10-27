namespace Panis360.App;
public class App : Application {
 public App() {
  UserAppTheme=AppTheme.Light;
  Resources=new ResourceDictionary();
  Resources.Add(new Style(typeof(Entry)){Setters={new Setter{Property=Entry.TextColorProperty,Value=Color.FromArgb("#173D32")},new Setter{Property=Entry.PlaceholderColorProperty,Value=Color.FromArgb("#7A8A84")}}});
  Resources.Add(new Style(typeof(Picker)){Setters={new Setter{Property=Picker.TextColorProperty,Value=Color.FromArgb("#173D32")},new Setter{Property=Picker.TitleColorProperty,Value=Color.FromArgb("#7A8A84")}}});
  Resources.Add(new Style(typeof(Label)){Setters={new Setter{Property=Label.TextColorProperty,Value=Color.FromArgb("#173D32")}}});
  Resources.Add(new Style(typeof(Button)){Setters={new Setter{Property=Button.BackgroundColorProperty,Value=Color.FromArgb("#23674E")},new Setter{Property=Button.TextColorProperty,Value=Colors.White},new Setter{Property=Button.CornerRadiusProperty,Value=8}}});
 }
 protected override Window CreateWindow(IActivationState? activationState)=>new(new NavigationPage(new MainPage()){BarBackgroundColor=Color.FromArgb("#173D32"),BarTextColor=Colors.White});
}
