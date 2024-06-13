using Microsoft.AspNetCore.Components;

public interface IComponentModel<T> : IComponentModel where T : IComponent
{
    Type IComponentModel.ComponentType => typeof(T);
}

public interface IComponentModel
{
    Type ComponentType { get; }
}
