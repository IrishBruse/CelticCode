namespace CelticCode.Extension;

using System.Threading.Tasks;

public interface IExtension
{
    public ValueTask EnableAsync();
    public ValueTask DisableAsync();
    public ValueTask ReloadAsync();
}
