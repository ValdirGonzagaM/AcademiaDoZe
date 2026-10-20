using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Converters;
using AcademiaDoZe.Presentation.AppMaui.Services;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class PessoaEditPage : ContentPage
{
    private readonly PessoaCadastro _cadastro;
    private readonly FotoService _fotos;
    private readonly bool _colaborador;
    private PessoaDto? _original;
    private readonly Entry _nome = new() { Placeholder = "Nome completo", AutomationId = "NomePessoa" };
    private readonly Entry _cpf = new() { Placeholder = "CPF (11 dígitos)", Keyboard = Keyboard.Numeric, AutomationId = "CpfPessoa" };
    private readonly Entry _telefone = new() { Placeholder = "Telefone com DDD", Keyboard = Keyboard.Telephone };
    private readonly Entry _email = new() { Placeholder = "E-mail", Keyboard = Keyboard.Email };
    private readonly Entry _numero = new() { Placeholder = "Número" };
    private readonly Entry _complemento = new() { Placeholder = "Complemento (opcional)" };
    private readonly Entry _senha = new() { Placeholder = "Senha (mínimo de 6 caracteres)", IsPassword = true };
    private readonly Entry _confirmarSenha = new() { Placeholder = "Confirme a senha", IsPassword = true, AutomationId = "ConfirmarSenhaPessoa" };
    private readonly Entry _cep = new() { Placeholder = "CEP (8 dígitos)", Keyboard = Keyboard.Numeric, AutomationId = "CepPessoa" };
    private readonly Label _enderecoDetalhes = new() { Text = "Selecione um logradouro ou busque pelo CEP.", AutomationId = "EnderecoPessoa" };
    private readonly DatePicker _nascimento = new() { Date = DateTime.Today.AddYears(-18), MaximumDate = DateTime.Today, Format = "dd/MM/yyyy" };
    private readonly DatePicker _admissao = new() { Date = DateTime.Today, MaximumDate = DateTime.Today, Format = "dd/MM/yyyy" };
    private readonly Picker _logradouro = new() { Title = "Selecione o logradouro", ItemDisplayBinding = new Binding(nameof(LogradouroDto.Nome)) };
    private readonly Picker _tipo = new() { Title = "Tipo", ItemsSource = Enum.GetValues<AppColaboradorTipo>(), ItemDisplayBinding = new Binding(".", converter: new EnumDisplayConverter()), SelectedIndex = 1 };
    private readonly Picker _vinculo = new() { Title = "Vínculo", ItemsSource = Enum.GetValues<AppColaboradorVinculo>(), ItemDisplayBinding = new Binding(".", converter: new EnumDisplayConverter()), SelectedIndex = 0 };
    private readonly Image _foto = new() { HeightRequest = 160, WidthRequest = 160, Aspect = Aspect.AspectFit, Source = "logo_aluno.png" };
    private readonly Label _fotoStatus = new() { Text = "Nenhuma foto selecionada", HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _erro = new() { TextColor = Color.FromArgb("#A33232"), AutomationId = "ErroCadastro" };
    private readonly VerticalStackLayout _form = new() { Padding = DeviceInfo.Idiom == DeviceIdiom.Phone ? 16 : 28, Spacing = 12, MaximumWidthRequest = 960 };
    private byte[]? _fotoBytes;
    private bool _ocupado;
    private bool _loaded;

    public PessoaEditPage(PessoaCadastro cadastro, FotoService fotos, bool colaborador, PessoaDto? pessoa = null)
    {
        _cadastro = cadastro; _fotos = fotos; _colaborador = colaborador; _original = pessoa;
        Title = (pessoa == null ? "Novo " : "Editar ") + (colaborador ? "colaborador" : "aluno");
        if (pessoa != null) CarregarPessoa(pessoa);
        _logradouro.SelectedIndexChanged += (_, _) => AtualizarEndereco();
        var buscarCpf = new Button { Text = "Buscar CPF", AutomationId = "BuscarCpfPessoa" };
        buscarCpf.Clicked += async (_, _) => await BuscarCpfAsync();
        _cpf.Completed += async (_, _) => await BuscarCpfAsync();
        var buscarCep = new Button { Text = "Buscar CEP", AutomationId = "BuscarCepPessoa" };
        buscarCep.Clicked += async (_, _) => await BuscarCepAsync();
        _cep.Completed += async (_, _) => await BuscarCepAsync();
        var camera = new Button { Text = "Tirar foto", AutomationId = "CameraPessoa" };
        var galeria = new Button { Text = "Galeria", AutomationId = "GaleriaPessoa" };
        camera.Clicked += async (_, _) => await EscolherFotoAsync(true);
        galeria.Clicked += async (_, _) => await EscolherFotoAsync(false);
        var salvar = new Button { Text = "Salvar cadastro", AutomationId = "SalvarPessoa" };
        salvar.Clicked += async (_, _) => await SalvarAsync();
        var cancelar = new Button { Text = "Cancelar", BackgroundColor = Color.FromArgb("#526469") };
        cancelar.Clicked += async (_, _) => { if (!_ocupado) await Navigation.PopAsync(); };
        _form.Children.Add(_foto); _form.Children.Add(_fotoStatus);
        _form.Children.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Spacing = 12, Children = { camera, galeria } });
        Campo("CPF", _cpf); _form.Children.Add(buscarCpf);
        Campo("Nome completo", _nome); Campo("Nascimento", _nascimento);
        Campo("Telefone", _telefone); Campo("E-mail", _email);
        Campo("CEP", _cep); _form.Children.Add(buscarCep);
        Campo("Logradouro", _logradouro); _form.Children.Add(_enderecoDetalhes);
        _form.Children.Add(new Label { Text = "Cadastre o endereço no menu Logradouros antes de selecionar.", FontSize = 12 });
        Campo("Número", _numero); Campo("Complemento", _complemento); Campo("Senha", _senha);
        Campo("Confirmar senha", _confirmarSenha);
        _form.Children.Add(new Label { Text = "Na edição, deixe senha e confirmação em branco para manter a senha atual.", FontSize = 12 });
        if (colaborador) { Campo("Admissão", _admissao); Campo("Tipo de colaborador", _tipo); Campo("Vínculo", _vinculo); }
        _form.Children.Add(_erro); _form.Children.Add(salvar); _form.Children.Add(cancelar);
        Content = new ScrollView { Content = _form };
    }
    private void CarregarPessoa(PessoaDto pessoa)
    {
        _original = pessoa;
        Title = "Editar " + (_colaborador ? "colaborador" : "aluno");
        _nome.Text = pessoa.Nome; _cpf.Text = pessoa.Cpf; _telefone.Text = pessoa.Telefone;
        _email.Text = pessoa.Email; _numero.Text = pessoa.Numero; _complemento.Text = pessoa.Complemento;
        _nascimento.Date = pessoa.DataNascimento.ToDateTime(TimeOnly.MinValue);
        _senha.Text = ""; _confirmarSenha.Text = "";
        _senha.Placeholder = "Nova senha (em branco mantém a atual)";
        _fotoBytes = pessoa.Foto?.Conteudo;
        if (_fotoBytes is { Length: > 0 }) MostrarFoto("Foto do cadastro");
        else { _foto.Source = "logo_aluno.png"; _fotoStatus.Text = "Nenhuma foto selecionada"; }
        if (pessoa is ColaboradorDto c)
        {
            _admissao.Date = c.DataAdmissao.ToDateTime(TimeOnly.MinValue);
            _tipo.SelectedItem = c.Tipo; _vinculo.SelectedItem = c.Vinculo;
        }
        if (_logradouro.ItemsSource is IEnumerable<LogradouroDto> enderecos)
            _logradouro.SelectedItem = enderecos.FirstOrDefault(l => l.Id == pessoa.Endereco?.Id);
        _cep.Text = pessoa.Endereco?.Cep;
        AtualizarEndereco();
    }
    private void AtualizarEndereco()
    {
        if (_logradouro.SelectedItem is LogradouroDto endereco)
        {
            _cep.Text = endereco.Cep;
            _enderecoDetalhes.Text = $"Rua: {endereco.Nome}\nBairro: {endereco.Bairro}\nCidade: {endereco.Cidade} / {endereco.Estado}\nPaís: {endereco.Pais}";
        }
        else _enderecoDetalhes.Text = "Selecione um logradouro cadastrado ou busque pelo CEP.";
    }
    private async Task BuscarCpfAsync() => await ExecutarAsync(async () =>
    {
        var pessoa = await _cadastro.PorCpfAsync(_colaborador, _cpf.Text ?? "");
        if (pessoa == null)
        {
            await DisplayAlertAsync("Busca por CPF", "CPF não encontrado. Você pode preencher um novo cadastro.", "OK");
            return;
        }
        if (!await DisplayAlertAsync("Cadastro encontrado", "Carregar os dados para edição? Isso substituirá os campos e a foto deste formulário.", "Carregar", "Cancelar")) return;
        _logradouro.ItemsSource = (await _cadastro.LogradourosAsync()).ToList();
        CarregarPessoa(pessoa);
    });
    private async Task BuscarCepAsync() => await ExecutarAsync(async () =>
    {
        var endereco = await _cadastro.PorCepAsync(_cep.Text ?? "");
        if (endereco == null) throw new ArgumentException("CEP não encontrado. Cadastre o logradouro no menu Logradouros.");
        if (_logradouro.ItemsSource is not IEnumerable<LogradouroDto> enderecos || !enderecos.Any(l => l.Id == endereco.Id))
            _logradouro.ItemsSource = (await _cadastro.LogradourosAsync()).ToList();
        _logradouro.SelectedItem = ((IEnumerable<LogradouroDto>)_logradouro.ItemsSource).First(l => l.Id == endereco.Id);
        AtualizarEndereco();
    });
    private void Campo(string label, View view)
    {
        _form.Children.Add(new Label { Text = label, FontAttributes = FontAttributes.Bold }); _form.Children.Add(view);
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing(); if (_loaded) return;
        await ExecutarAsync(async () =>
        {
            var enderecos = (await _cadastro.LogradourosAsync()).ToList();
            _logradouro.ItemsSource = enderecos;
            _logradouro.SelectedItem = enderecos.FirstOrDefault(l => l.Id == _original?.Endereco?.Id);
            _loaded = true;
            if (enderecos.Count == 0) _erro.Text = "Cadastre um logradouro antes de salvar esta pessoa.";
        });
    }
    private async Task EscolherFotoAsync(bool camera) => await ExecutarAsync(async () =>
    {
        var bytes = await _fotos.ObterAsync(camera);
        if (bytes == null) return;
        _fotoBytes = bytes; MostrarFoto(camera ? "Foto tirada pela câmera" : "Foto selecionada da galeria");
    });
    private void MostrarFoto(string origem)
    {
        var bytes = _fotoBytes!;
        _foto.Source = ImageSource.FromStream(() => new MemoryStream(bytes)); _fotoStatus.Text = origem;
    }
    private async Task SalvarAsync() => await ExecutarAsync(async () =>
    {
        PessoaCadastro.ValidarConfirmacaoSenha(_senha.Text, _confirmarSenha.Text, _original != null);
        if (_logradouro.SelectedItem is not LogradouroDto endereco) throw new ArgumentException("Selecione um logradouro ou busque pelo CEP.");
        if (new string((_cep.Text ?? "").Where(char.IsDigit).ToArray()) != endereco.Cep)
            throw new ArgumentException("O CEP foi alterado. Use Buscar CEP ou selecione o logradouro correto antes de salvar.");
        if (_nascimento.Date is not DateTime nascimento) throw new ArgumentException("Informe a data de nascimento.");
        if (_fotoBytes == null) throw new ArgumentException("Tire ou selecione uma foto para o cadastro.");
        PessoaDto pessoa;
        if (_colaborador)
        {
            if (_admissao.Date is not DateTime admissao) throw new ArgumentException("Informe a data de admissão.");
            pessoa = new ColaboradorDto { Nome = _nome.Text ?? "", Cpf = _cpf.Text ?? "", DataNascimento = DateOnly.FromDateTime(nascimento),
                Telefone = _telefone.Text ?? "", Numero = _numero.Text ?? "", DataAdmissao = DateOnly.FromDateTime(admissao),
                Tipo = (AppColaboradorTipo)_tipo.SelectedItem, Vinculo = (AppColaboradorVinculo)_vinculo.SelectedItem };
        }
        else pessoa = new AlunoDto { Nome = _nome.Text ?? "", Cpf = _cpf.Text ?? "", DataNascimento = DateOnly.FromDateTime(nascimento),
            Telefone = _telefone.Text ?? "", Numero = _numero.Text ?? "" };
        pessoa.Id = _original?.Id ?? 0; pessoa.Email = _email.Text?.Trim(); pessoa.Endereco = endereco;
        pessoa.Complemento = _complemento.Text; pessoa.Senha = _senha.Text;
        pessoa.Foto = new ArquivoDto { Conteudo = _fotoBytes };
        await _cadastro.SalvarAsync(pessoa);
        await DisplayAlertAsync("Cadastro salvo", "Dados e foto foram salvos com sucesso.", "OK");
        await Navigation.PopAsync();
    });
    private async Task ExecutarAsync(Func<Task> action)
    {
        if (_ocupado) return;
        _ocupado = true; _form.IsEnabled = false; _erro.Text = "";
        try { await action(); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PessoaEditPage] {ex}");
            _erro.Text = ex.Message;
            await DisplayAlertAsync("Atenção", ex.Message, "OK");
        }
        finally { _ocupado = false; _form.IsEnabled = true; }
    }
}
