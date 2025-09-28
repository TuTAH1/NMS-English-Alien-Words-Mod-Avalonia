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
		DataContext = Logic.AppGlobals.SettingsModel;
		SettingsGrid.DefaultOptionsButton.IsVisible = false;
		SetWindowMinSize();
		InitializeDebugTools();
	}

	// Display only GeneratorSettings category
	private void OnCustomPropertyDescriptorFilter(object sender, RoutedEventArgs args)
	{
		if (args is CustomPropertyDescriptorFilterEventArgs { TargetObject: SettingsObject} e)
		{
			if (AppGlobals.HiddenSettingsWindowCategories
				.Contains(e.PropertyDescriptor.Category))  {
				e.IsVisible = false;
			} else {
				e.IsVisible = true;
			}
			e.Handled = true;
		}
	}
	private void SetWindowMinSize()
	{
		//probably may be calculated, но мне лень
		this.MinWidth = 655;
		this.MinHeight = 400;
	}

	private void InitializeDebugTools()
	{
		#if DEBUG
		this.AttachDevTools();
		#endif
	}
	
}