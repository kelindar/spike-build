namespace Spike.Build
{
    internal sealed class Member(string name, string type)
    {
        internal string Name { get; } = name;
        internal string Type { get; } = type;
    }
}
