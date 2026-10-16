namespace AcademiaDoZe.Presentation.AppMaui.Services;

internal static class FotoConteudo
{
    public static async Task<byte[]> LerAsync(string? caminhoOriginal, Func<Task<Stream>> abrirStream)
    {
        await using var stream = !string.IsNullOrWhiteSpace(caminhoOriginal)
            && Path.IsPathFullyQualified(caminhoOriginal) && File.Exists(caminhoOriginal)
            ? File.OpenRead(caminhoOriginal)
            : await abrirStream();
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(chunk)) > 0)
        {
            if (buffer.Length + read > 10 * 1024 * 1024)
                throw new ArgumentException("Escolha uma foto de até 10 MB.");
            await buffer.WriteAsync(chunk.AsMemory(0, read));
        }
        var bytes = buffer.ToArray();
        if (bytes.Length == 0) throw new ArgumentException("A foto selecionada está vazia.");
        return bytes;
    }
}
