using AcademiaDoZe.Presentation.AppMaui.Services;
using Xunit;

namespace AcademiaDoZe.Application.Tests;

public sealed class FotoConteudoTests
{
    [Fact]
    public async Task ArquivoOriginal_Evita_StreamFechado_DoPicker()
    {
        var path = Path.GetTempFileName();
        var expected = new byte[] { 255, 216, 255, 42 };
        try
        {
            await File.WriteAllBytesAsync(path, expected);
            using var closed = new MemoryStream(expected);
            closed.Dispose();
            var pickerOpened = false;
            var bytes = await FotoConteudo.LerAsync(path, () =>
            {
                pickerOpened = true;
                return Task.FromResult<Stream>(closed);
            });
            Assert.False(pickerOpened);
            Assert.Equal(expected, bytes);
            // A leitura terminou e liberou o arquivo, inclusive no Windows.
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task SemCaminhoLocal_MaterializaFoto_E_LiberaStream()
    {
        var expected = new byte[] { 255, 216, 255, 43 };
        var stream = new MemoryStream(expected);
        var bytes = await FotoConteudo.LerAsync(null, () => Task.FromResult<Stream>(stream));
        Assert.False(stream.CanRead);
        Assert.Equal(expected, bytes); // Continua disponível depois de fechar a origem.
    }

    [Fact]
    public async Task FotoVazia_E_Rejeitada()
    {
        var stream = new MemoryStream();
        var error = await Assert.ThrowsAsync<ArgumentException>(() =>
            FotoConteudo.LerAsync(null, () => Task.FromResult<Stream>(stream)));
        Assert.Equal("A foto selecionada está vazia.", error.Message);
        Assert.False(stream.CanRead);
    }

    [Theory]
    [InlineData(10 * 1024 * 1024, true)]
    [InlineData(10 * 1024 * 1024 + 1, false)]
    public async Task AplicaLimiteDeTamanho_SemDependerDeArquivoLocal(int size, bool accepted)
    {
        var stream = new MemoryStream(new byte[size]);
        if (accepted)
        {
            var bytes = await FotoConteudo.LerAsync(null, () => Task.FromResult<Stream>(stream));
            Assert.Equal(size, bytes.Length);
        }
        else
        {
            var error = await Assert.ThrowsAsync<ArgumentException>(() =>
                FotoConteudo.LerAsync(null, () => Task.FromResult<Stream>(stream)));
            Assert.Equal("Escolha uma foto de até 10 MB.", error.Message);
        }
        Assert.False(stream.CanRead);
    }
}
