namespace CelticCode.ExtensionExample;

using CelticCode.Extension;

public class Test : IExtension
{
    public async ValueTask EnableAsync()
    {
        await Task.Delay(1000);
    }

    public async ValueTask DisableAsync()
    {
        await Task.Delay(1000);
    }

    public async ValueTask ReloadAsync()
    {
        await Task.Delay(1000);
    }
}
