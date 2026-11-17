namespace FitnessClubIS.Services;

/// <summary>Простой контейнер служб (одиночка): словарь «тип -> экземпляр».</summary>
public sealed class AppServices
{
    private static readonly Lazy<AppServices> _instance = new(() => new AppServices());
    public static AppServices Instance => _instance.Value;

    private readonly Dictionary<Type, object> _services = new();
    private AppServices() { }

    public void Add<T>(T service) => _services[typeof(T)] = service;

    public T Get<T>() => _services.TryGetValue(typeof(T), out var s)
        ? (T)s
        : throw new InvalidOperationException($"Служба {typeof(T).Name} не зарегистрирована");
}
