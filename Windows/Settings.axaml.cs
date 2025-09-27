using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.PropertyGrid.Controls;
using NMS_EnglishAlienWordsMod_Avalonia.Logic;

namespace NMS_EnglishAlienWordsMod_Avalonia.Windows;

public partial class Settings : Window
{
	public Settings()
	{
		InitializeComponent();
		DataContext = AppProperties.SettingsModel;
		SettingsGrid.DefaultOptionsButton.IsVisible = false;
		SetWindowMinSize();
	}

	// Display only GeneratorSettings category
	private void OnCustomPropertyDescriptorFilter(object sender, RoutedEventArgs args)
	{
		if (args is CustomPropertyDescriptorFilterEventArgs { TargetObject: SettingsObject} e)
		{
			if (e.PropertyDescriptor.Category == "GeneratorSettings") {
				e.IsVisible = true;
			} else {
				e.IsVisible = false;
			}
			e.Handled = true;
		}
	}
	private void SetWindowMinSize()
	{
		//probably may be calculated, но мне лень
		this.MinWidth = 655;
		this.MinHeight = 520;
	}
	
}