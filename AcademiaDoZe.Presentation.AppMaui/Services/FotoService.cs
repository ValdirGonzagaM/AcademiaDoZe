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
        var windows = OperatingSystem.IsWindows();
        var options = new MediaPickerOptions
        {
            Title = camera ? "Tirar foto do cadastro" : "Selecionar foto da galeria",
            SelectionLimit = 1,
            // Evita por completo o ImageProcessor do MAUI no Windows: desativar
            // apenas os metadados não evita todos os caminhos de stream fechado.
            MaximumWidth = windows ? null : 1600,
            MaximumHeight = windows ? null : 1600,
            CompressionQuality = windows ? 100 : 85,
            RotateImage = false,
            PreserveMetaData = false
        };
        var file = camera ? await MediaPicker.Default.CapturePhotoAsync(options)
            : (await MediaPicker.Default.PickPhotosAsync(options)).FirstOrDefault();
        if (file == null) return null; // Cancelar preserva a foto anterior.
        // No Windows, reabre o arquivo original em vez de reutilizar o stream do picker.
        return await FotoConteudo.LerAsync(windows ? file.FullPath : null, file.OpenReadAsync);
    }
}
