using System.Threading;
using System.Windows;
using EmailParsing.Parsers;
using EmlExtractor.ViewModels;
using EmlExtractor.Views;
using Prism.Ioc;

namespace EmlExtractor;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly CancellationTokenSource _cts = new();

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IEmailParser, EmailParser>();
        containerRegistry.Register<MainWindowViewModel>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        var parser = Container.Resolve<IEmailParser>();

        parser.DeleteSourceFile = false;

        _cts.Cancel();

        await parser.CleanupAsync();

        base.OnExit(e);
    }
}