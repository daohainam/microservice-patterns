namespace WebHook.DeliveryService;
public interface ISecretKeyService
{
    string GetKey();
}

public class SecretKeyService(Func<string> keyGenerator) : ISecretKeyService
{
    private readonly Func<string> keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));

    public string GetKey()
    {
        return keyGenerator();
    }
}
