using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Messages;

public class BancoPreferencesUpdatedMessage : ValueChangedMessage<string>
{
    public BancoPreferencesUpdatedMessage(string tipoBanco) : base(tipoBanco)
    {
    }
}