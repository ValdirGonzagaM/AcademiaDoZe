using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Messages;

public class TemaPreferencesUpdatedMessage : ValueChangedMessage<string>
{
    public TemaPreferencesUpdatedMessage(string novoTema) : base(novoTema)
    {
    }
}