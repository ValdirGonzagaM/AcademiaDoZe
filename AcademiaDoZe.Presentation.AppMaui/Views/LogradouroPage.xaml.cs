using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroPage : ContentPage, IQueryAttributable
{
    private readonly LogradouroViewModel _viewModel;
    private int _id;

    public LogradouroPage(LogradouroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _id = query.TryGetValue("id", out var value) && int.TryParse(value?.ToString(), out var id) && id > 0
            ? id : 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarAsync(_id);
    }
}
