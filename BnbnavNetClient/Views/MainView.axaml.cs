using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using BnbnavNetClient.Controls;
using BnbnavNetClient.I18Next.Services;
using BnbnavNetClient.Settings;
using BnbnavNetClient.ViewModels;

namespace BnbnavNetClient.Views;

public partial class MainView : UserControl
{
    readonly Style _whiteTextStyle;
    readonly ISettingsManager _settings;

    public MainView()
    {
        FlowDirection = AvaloniaLocator.Current.GetRequiredService<IAvaloniaI18Next>().IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        _whiteTextStyle = new Style(static x => x.OfType<TextBlock>());
        _whiteTextStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.White)));
        _settings = AvaloniaLocator.Current.GetRequiredService<ISettingsManager>();
        InitializeComponent();
    }

    public async void ViewLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        await vm.InitMapService();

        MapPanel.Children.Add(new MapView() { DataContext = vm.MapViewModel });

        if (_settings.Settings.NightMode)
        {
            DayNightButton.IsNightMode = true;
            ColorModeSwitch(null, null);
        }
    }

    public async void ColorModeSwitch(object? _, RoutedEventArgs? __)
    {
        var button = DayNightButton;

        ((FluentTheme)App.Current!.Styles[0]).Mode = button.IsNightMode ? FluentThemeMode.Dark : FluentThemeMode.Light;
    
        if (button.IsNightMode)
        {
            //WASM seems to need a little help setting the textblock styles. hopefully they fix this sometime!
            App.Current!.Styles.Add(_whiteTextStyle);
        }
        else
        {
            App.Current!.Styles.Remove(_whiteTextStyle);
        }

        ((MapThemeResources)App.Current!.Resources.MergedDictionaries[0]).Theme = button.IsNightMode ? MapTheme.Night : MapTheme.Day;

        _settings.Settings.NightMode = button.IsNightMode;
        await _settings.SaveAsync();
    }
}