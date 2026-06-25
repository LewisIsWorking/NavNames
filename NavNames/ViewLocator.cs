using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using NavNames.ViewModels;

namespace NavNames;

/// <summary>
/// Given a ViewModel, reflects to find the matching View (XxxViewModel -> XxxView)
/// and instantiates it. Convention copied from the ClaudeDesktopLauncher app.
/// </summary>
[RequiresUnreferencedCode("ViewLocator uses reflection which may be trimmed.")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        return type is not null
            ? (Control)Activator.CreateInstance(type)!
            : new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
