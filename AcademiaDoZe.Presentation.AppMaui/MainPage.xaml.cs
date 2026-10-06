namespace AcademiaDoZe.Presentation.AppMaui;
public partial class MainPage : ContentPage
{
    public MainPage() => InitializeComponent();
    private async void OnColaboradoresClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//ColaboradorListPage");
    private async void OnAlunosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//AlunoListPage");
    private async void OnLogradourosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//LogradouroListPage");
}
