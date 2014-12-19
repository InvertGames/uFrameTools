namespace Invert.Core.GraphDesigner
{
    public interface IClassTemplate
    {
        void TemplateSetup();
    }

    public interface IClassTemplate<TData> : IClassTemplate
    {
        TemplateContext<TData> Ctx { get; set; }
        
    }
}