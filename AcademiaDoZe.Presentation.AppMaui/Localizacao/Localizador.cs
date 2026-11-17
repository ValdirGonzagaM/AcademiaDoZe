using System.ComponentModel;
using AcademiaDoZe.Application.Enums;
using System.Globalization;
using System.Resources;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.Localizacao;

public sealed class Localizador : INotifyPropertyChanged
{
    public static Localizador Atual { get; } = new();
    private readonly ResourceManager _resources = new("AcademiaDoZe.Presentation.AppMaui.Resources.Strings.Textos", typeof(Localizador).Assembly);
    public CultureInfo Cultura { get; private set; } = CultureInfo.GetCultureInfo("pt-BR");
    public event PropertyChangedEventHandler? PropertyChanged;
    public string this[string chave] => Traduzir(chave);
    public string Traduzir(string chave) => _resources.GetString(chave, Cultura) ?? chave;
    public bool Possui(string chave) => _resources.GetString(chave, CultureInfo.GetCultureInfo("pt-BR")) != null;
    public void Alterar(string cultura)
    {
        if (cultura is not ("pt-BR" or "en-US" or "es-ES")) cultura = "pt-BR";
        Cultura = CultureInfo.GetCultureInfo(cultura);
#if WINDOWS
        global::Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Cultura.Name;
#endif
        CultureInfo.CurrentCulture = Cultura; CultureInfo.CurrentUICulture = Cultura;
        CultureInfo.DefaultThreadCurrentCulture = Cultura; CultureInfo.DefaultThreadCurrentUICulture = Cultura;
        Preferences.Default.Set("IdiomaApp", cultura);
        PropertyChanged?.Invoke(this, new(nameof(Cultura)));
        PropertyChanged?.Invoke(this, new("Item[]"));
    }
    public void Aplicar(IVisualTreeElement raiz)
    {
        var visitados = new HashSet<IVisualTreeElement>();
        void Visitar(IVisualTreeElement item)
        {
            if (!visitados.Add(item)) return;
            if (item is BindableObject controle)
            {
                if (item is Label) Vincular(controle, Label.TextProperty);
                if (item is Button) Vincular(controle, Button.TextProperty);
                if (item is Page) Vincular(controle, Page.TitleProperty);
                if (item is Entry) Vincular(controle, Entry.PlaceholderProperty);
                if (item is Editor) Vincular(controle, Editor.PlaceholderProperty);
                if (item is SearchBar) Vincular(controle, SearchBar.PlaceholderProperty);
                if (item is Picker picker)
                {
                    Vincular(picker, Picker.TitleProperty);
                    if (picker.ItemsSource is System.Collections.IEnumerable valores && valores.Cast<object>().All(v => v is string or Enum)) picker.ItemDisplayBinding = new Binding(".", converter: new TextoConverter());
                }
                if (item is DatePicker data)
                {
                    data.Format = Cultura.DateTimeFormat.ShortDatePattern;
                    data.Handler?.UpdateValue("Idioma");
                }
            }
            foreach (var filho in item.GetVisualChildren()) Visitar(filho);
        }
        Visitar(raiz);
    }
    public void Vincular(BindableObject objeto, BindableProperty propriedade)
    {
        if (objeto.BindingContext is AcademiaDoZe.Presentation.AppMaui.ViewModels.LogradouroViewModel
            && (propriedade == Page.TitleProperty || objeto is Label && Equals(objeto.GetValue(propriedade), ((AcademiaDoZe.Presentation.AppMaui.ViewModels.LogradouroViewModel)objeto.BindingContext).Titulo))) return;
        if (objeto is Label && objeto.BindingContext is AcademiaDoZe.Application.DTOs.PessoaDto
            or AcademiaDoZe.Application.DTOs.MatriculaDto or AcademiaDoZe.Application.DTOs.LogradouroDto or AcademiaDoZe.Domain.Entities.Visita) return;
        if (objeto.GetValue(propriedade) is not string texto || texto.Length == 0 || !Possui(texto)) return;
        objeto.SetBinding(propriedade, new Binding(nameof(Cultura), source: this, converter: new TextoConverter(), converterParameter: texto));
    }
}
public sealed class TextoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => Localizador.Atual.Traduzir(parameter as string ?? (value is Enum item ? item.GetDisplayName() : value?.ToString() ?? ""));
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
public static class Texto
{
    public static string T(string chave) => Localizador.Atual.Traduzir(chave);
    public static string F(string chave, params object[] valores) => string.Format(Localizador.Atual.Cultura, T(chave), valores);
    private static string Mensagem(string mensagem) => string.Join("\n", mensagem.Split('\n').Select(l => string.Join("; ", l.Split("; ").Select(T))));
    public static Task AvisoAsync(this Page page, string titulo, string mensagem, string cancelar) => page.DisplayAlertAsync(T(titulo), Mensagem(mensagem), T(cancelar));
    public static Task<bool> AvisoAsync(this Page page, string titulo, string mensagem, string aceitar, string cancelar) => page.DisplayAlertAsync(T(titulo), Mensagem(mensagem), T(aceitar), T(cancelar));
}
