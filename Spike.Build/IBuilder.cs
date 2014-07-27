namespace Spike.Build
{
    internal interface IBuilder
    {
        void Build(Model model, string output = null);        
    }
}
