using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class DashboardListPage : ContentPage
{
    public DashboardListPage(DashboardListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}