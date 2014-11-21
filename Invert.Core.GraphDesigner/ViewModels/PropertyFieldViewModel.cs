using System;
using Invert.uFrame.Editor.ViewModels;

public class PropertyFieldViewModel : ItemViewModel
{
    public Type Type { get; set; }
    public override string Name { get; set; }
    public Func<object> Getter { get; set; }
    public Action<object> Setter { get; set; }
    public override bool IsEditing { get; set; }

    public PropertyFieldViewModel(DiagramNodeViewModel nodeViewModel) : base(nodeViewModel)
    {
      
    }
}