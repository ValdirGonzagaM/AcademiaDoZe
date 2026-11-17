using AcademiaDoZe.Application.Services;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class RelatoriosPage : ContentPage
{
    private readonly RelatorioService _service;
    private readonly DatePicker _inicio = new() { Date = DateTime.Today.AddDays(-7) };
    private readonly DatePicker _fim = new() { Date = DateTime.Today };
    private readonly VerticalStackLayout _resultado = new() { Spacing = 12 };
    public RelatoriosPage(RelatorioService service)
    {
        _service = service; Title = "Relatórios";
        var gerar = new Button { Text = "Atualizar relatório" };
        gerar.Clicked += async (_, _) => await CarregarAsync();
        Content = new ScrollView { Content = new VerticalStackLayout { Padding = 24, Spacing = 16, MaximumWidthRequest = 1000,
            Children = { new Label { Text = "Visão da academia", FontSize = 28, FontAttributes = FontAttributes.Bold },
                new Label { Text = "Início" }, _inicio, new Label { Text = "Fim" }, _fim, gerar, _resultado } } };
    }
    protected override async void OnAppearing() { base.OnAppearing(); await CarregarAsync(); }
    private async Task CarregarAsync()
    {
        try
        {
            if (_inicio.Date is not DateTime inicio || _fim.Date is not DateTime fim) return;
            var dados = await _service.GerarAsync(DateOnly.FromDateTime(inicio), DateOnly.FromDateTime(fim));
            _resultado.Clear();
            Card(Texto.F("Entradas de alunos: {0}\nEntradas de colaboradores: {1}\nPessoas na academia agora: {2}\nPermanência média de alunos: {3}",
                dados.EntradasAlunos, dados.EntradasColaboradores, dados.Presentes, AcessoPage.Duracao(dados.MediaPermanencia)));
            _resultado.Add(new Label { Text = Texto.T("Horários mais procurados"), FontSize = 20, FontAttributes = FontAttributes.Bold });
            foreach (var item in dados.Horarios) Card(Texto.F("{0:00}h — {1} entrada(s)", item.Hora, item.Entradas));
            _resultado.Add(new Label { Text = Texto.T("Horas trabalhadas por dia"), FontSize = 20, FontAttributes = FontAttributes.Bold });
            foreach (var dia in dados.Jornadas) Card(Texto.F("{0} · {1:d} · {2}", dia.Nome, dia.Dia, AcessoPage.Duracao(dia.Tempo)));
            _resultado.Add(new Label { Text = Texto.T("Evolução dos contratos"), FontSize = 20, FontAttributes = FontAttributes.Bold });
            _resultado.Add(new Label { Text = Texto.T("Situação dos alunos matriculados até o fim de cada período; não inclui alunos que nunca se matricularam.") });
            foreach (var mes in dados.Contratos)
            {
                var baseAlunos = mes.Vigentes + mes.Encerrados;
                var percentual = baseAlunos == 0 ? 0 : (double)mes.Vigentes / baseAlunos;
                Card(Texto.F("{0:MMM yyyy}\nCom contrato vigente: {1}\nSem contrato vigente: {2}\nRetenção contratual: {3:P0}", mes.Mes, mes.Vigentes, mes.Encerrados, percentual));
            }
        }
        catch (Exception ex) { await this.AvisoAsync("Atenção", ex.Message, "OK"); }
    }
    private void Card(string texto) => _resultado.Add(new Border { Padding = 18, Content = new Label { Text = texto } });
}
