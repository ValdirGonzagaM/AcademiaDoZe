namespace AcademiaDoZe.Presentation.AppMaui.Services;

public sealed class FotoService
{
    public async Task<byte[]?> ObterAsync(bool camera)
    {
        if (camera)
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                throw new InvalidOperationException("Este dispositivo não oferece captura por câmera.");
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
                throw new InvalidOperationException("Autorize a câmera nas configurações do aplicativo para tirar a foto.");
        }
        var options = new MediaPickerOptions { Title = camera ? "Tirar foto do cadastro" : "Selecionar foto da galeria", SelectionLimit = 1, MaximumWidth = 1600, MaximumHeight = 1600, CompressionQuality = 85 };
        var file = camera ? await MediaPicker.Default.CapturePhotoAsync(options)
            : (await MediaPicker.Default.PickPhotosAsync(options)).FirstOrDefault();
        if (file == null) return null; // Cancelar preserva a foto anterior.
        await using var stream = await file.OpenReadAsync();
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
