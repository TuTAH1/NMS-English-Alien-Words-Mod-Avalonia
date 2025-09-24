using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NMS_EnglishAlienWordsMod_Avalonia.Logic;

namespace NMS_EnglishAlienWordsMod_Avalonia.Windows;

public partial class Settings : Window
{
    public Settings()
    {
        InitializeComponent();
        DataContext = AppProperties.Settings;
    }
    
}