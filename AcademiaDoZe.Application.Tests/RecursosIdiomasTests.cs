using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace AcademiaDoZe.Application.Tests;

public sealed class RecursosIdiomasTests
{
    [Fact]
    public void Idiomas_PossuemMesmasChavesSemDuplicatas_E_FormatosCompativeis()
    {
        Dictionary<string, string> Ler(string arquivo) => XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Localizacao", arquivo))
            .Root!.Elements("data").ToDictionary(e => e.Attribute("name")!.Value, e => e.Element("value")!.Value, StringComparer.OrdinalIgnoreCase);
        var portugues = Ler("Textos.resx");
        Assert.True(portugues.Count >= 300);
        foreach (var arquivo in new[] { "Textos.en-US.resx", "Textos.es-ES.resx" })
        {
            var idioma = Ler(arquivo); Assert.Equal(portugues.Keys.Order(), idioma.Keys.Order());
            foreach (var (chave, texto) in portugues)
            {
                Assert.False(string.IsNullOrWhiteSpace(idioma[chave]));
                string[] Campos(string valor) => Regex.Matches(valor, @"(?<!\{)\{(\d+)(?::[^}]*)?\}(?!\})").Select(m => m.Groups[1].Value).Order().ToArray();
                Assert.Equal(Campos(texto), Campos(idioma[chave]));
            }
        }
    }
}
