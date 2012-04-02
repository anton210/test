namespace Xsf
{
    public interface IXsfView
    {
        string Name { get; }
        string Caption { get; }
        string PrintViewName { get; }
        string TransformFileName { get; }
    }
}