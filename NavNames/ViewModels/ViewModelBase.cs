using CommunityToolkit.Mvvm.ComponentModel;

namespace NavNames.ViewModels;

/// <summary>Base for all ViewModels; gets INotifyPropertyChanged from ObservableObject.</summary>
public abstract class ViewModelBase : ObservableObject;
