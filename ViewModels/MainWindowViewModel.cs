using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using EmailParsing.Parsers;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Timer = System.Timers.Timer;

namespace EmlExtractor.ViewModels;

/// <summary>
/// ViewModel для главного окна приложения EmlExtractor.
/// </summary>
public class MainWindowViewModel : BindableBase
{
    /// <summary>
    ///     Расширения файла архива.
    /// </summary>
    private readonly string[] _archiveExtensions = { ".zip", ".rar", ".tgz" };

    /// <summary>
    ///     Парсер для обработки email-файлов.
    /// </summary>
    private readonly IEmailParser _parser;

    /// <summary>
    ///     Источник токена отмены для асинхронных операций.
    /// </summary>
    private CancellationTokenSource _cts = new();

    /// <summary>
    ///     Секундомер для измерения времени выполнения операции.
    /// </summary>
    private Stopwatch _stopwatch;

    /// <summary>
    ///     Таймер для обновления времени выполнения в пользовательском интерфейсе.
    /// </summary>
    private Timer _timer;

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="MainWindowViewModel"/>.
    /// </summary>
    /// <param name="parser">Парсер для обработки email-файлов.</param>
    public MainWindowViewModel(IEmailParser parser)
    {
        _parser = parser;
        _parser.ProgressChanged += OnParserProgressChanged;

        SelectEmailFolderCommand = new DelegateCommand(SelectEmailFolder);
        SelectOutputFolderCommand = new DelegateCommand(SelectOutputFolder);
        _startCommand = new DelegateCommand(Start);
        _cancelCommand = new DelegateCommand(Cancel);
        CurrentCommand = _startCommand;
        ButtonContent = "Старт";
    }

    #region Commands

    #region SelectEmailFolderCommand

    /// <summary>
    ///     Команда выбора файла или архива с email-сообщениями.
    /// </summary>
    public DelegateCommand SelectEmailFolderCommand { get; }

    /// <summary>
    ///     Обработчик команды выбора файла или архива с email-сообщениями.
    /// </summary>
    private void SelectEmailFolder()
    {
        SelectEmailFile();
    }

    #endregion

    #region SelectOutputFolderCommand

    /// <summary>
    ///     Команда выбора папки для сохранения извлеченных вложений.
    /// </summary>
    public DelegateCommand SelectOutputFolderCommand { get; }

    /// <summary>
    ///     Обработчик команды выбора папки для сохранения извлеченных вложений.
    /// </summary>
    private void SelectOutputFolder()
    {
        SelectOutputDirectory();
    }

    #endregion

    #region CurrentCommand

    /// <summary>
    ///     Команда "Старт" для запуска процесса извлечения вложений.
    /// </summary>
    private readonly DelegateCommand _startCommand;

    /// <summary>
    ///     Команда "Отмена" для отмены процесса извлечения вложений.
    /// </summary>
    private readonly DelegateCommand _cancelCommand;

    /// <summary>
    ///     Текущая команда, выполняемая при нажатии на кнопку.
    /// </summary>
    private DelegateCommand _currentCommand;

    /// <summary>
    ///     Свойство для доступа к текущей команде.
    /// </summary>
    public DelegateCommand CurrentCommand
    {
        get => _currentCommand;
        set => SetProperty(ref _currentCommand, value);
    }

    /// <summary>
    ///     Обработчик команды "Старт".
    /// </summary>
    private async void Start()
    {
        if (!string.IsNullOrEmpty(EmailFilePath) && !string.IsNullOrEmpty(OutputDirectory))
        {
            _cts = new CancellationTokenSource();
            StartTimer();

            _parser.ZipFilePath = $"{OutputDirectory}\\{Guid.NewGuid()}.zip";
            _parser.DeleteSourceFile = false;
            _parser.SourceFilePath = EmailFilePath;
            var fileExtension = Path.GetExtension(EmailFilePath);

            CurrentCommand = _cancelCommand;
            ButtonContent = "Отмена";

            try
            {
                if (_archiveExtensions.Any(ext =>
                        string.Equals(fileExtension, ext, StringComparison.OrdinalIgnoreCase)))
                    await _parser.ParseArchiveAsync(_cts.Token);
                else
                    await _parser.ParseEmlFileAsync(_cts.Token);

                if (!_cts.IsCancellationRequested)
                    MessageBox.Show($"Вложения успешно извлечены по следующему пути {_parser.ZipFilePath}.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Операция отменена пользователем.", "Отмена", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            finally
            {
                StopTimer();
                CurrentCommand = _startCommand;
                ButtonContent = "Старт";
                ProgressValue = "0";
                ProgressInfo = "...";
            }
        }
        else
        {
            MessageBox.Show("Пожалуйста, выберите путь к папке с файлами " +
                            "и папку для сохранения вложений.", "Предупреждение", MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    /// <summary>
    ///     Обработчик команды "Отмена".
    /// </summary>
    private void Cancel()
    {
        _cts.Cancel();
        CurrentCommand = _startCommand;
        ButtonContent = "Старт";
    }

    #endregion

    #endregion

    #region ButtonContent

    /// <summary>
    ///     Текст на кнопке запуска/отмены процесса.
    /// </summary>
    private string _buttonContent;

    /// <summary>
    ///     Свойство для доступа к тексту на кнопке.
    /// </summary>
    public string ButtonContent
    {
        get => _buttonContent;
        set => SetProperty(ref _buttonContent, value);
    }

    #endregion

    #region EmailFilePath

    /// <summary>
    ///     Путь к файлу или архиву с email-сообщениями.
    /// </summary>
    private string _emailFilePath;

    /// <summary>
    ///     Свойство для доступа к пути к файлу с email-сообщениями.
    /// </summary>
    public string EmailFilePath
    {
        get => _emailFilePath;
        set => SetProperty(ref _emailFilePath, value);
    }

    /// <summary>
    ///     Открывает диалог выбора файла и устанавливает <see cref="EmailFilePath"/>.
    /// </summary>
    private void SelectEmailFile()
    {
        var fileDialog = new OpenFileDialog
        {
            Filter = "Email and Archive Files (*.eml, *.rar, *.zip, *.tgz)|*.eml;*.rar;*.zip;*.tgz",
            Title = "Выберите файл .eml, .rar, .zip или .tgz",
            Multiselect = false
        };

        EmailFilePath = fileDialog.ShowDialog() == true ? fileDialog.FileName : null;
    }

    #endregion

    #region Title

    /// <summary>
    ///     Заголовок главного окна.
    /// </summary>
    private string _title = "EmlExtractor";

    /// <summary>
    ///     Свойство для доступа к заголовку главного окна.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    #endregion

    #region OutputDirectory

    /// <summary>
    ///     Путь к папке для сохранения извлеченных вложений.
    /// </summary>
    private string _outputDirectory;

    /// <summary>
    ///     Свойство для доступа к пути к папке для сохранения вложений.
    /// </summary>
    public string OutputDirectory
    {
        get => _outputDirectory;
        set => SetProperty(ref _outputDirectory, value);
    }

    /// <summary>
    ///     Открывает диалог выбора папки и устанавливает <see cref="OutputDirectory"/>.
    /// </summary>
    private void SelectOutputDirectory()
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку для сохранения вложений",
            UseDescriptionForTitle = true
        };

        OutputDirectory = dialog.ShowDialog() == true ? dialog.SelectedPath : null;
    }

    #endregion

    #region Progress

    /// <summary>
    ///     Информация о текущей операции.
    /// </summary>
    private string _progressInfo;

    /// <summary>
    ///     Свойство для доступа к информации о текущей операции.
    /// </summary>
    public string ProgressInfo
    {
        get => _progressInfo;
        set => SetProperty(ref _progressInfo, value);
    }

    /// <summary>
    ///     Значение прогресса выполнения операции (в процентах).
    /// </summary>
    private string _progressValue;

    /// <summary>
    ///     Свойство для доступа к значению прогресса.
    /// </summary>
    public string ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    /// <summary>
    ///     Обработчик события изменения прогресса парсера.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="progress">Значение прогресса (в процентах).</param>
    private void OnParserProgressChanged(object sender, int progress)
    {
        if (Application.Current != null)
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ProgressValue = progress.ToString();
                UpdateProgressInfo();
            });
    }

    /// <summary>
    ///     Обновляет информацию о текущей операции в <see cref="ProgressInfo"/>.
    /// </summary>
    private void UpdateProgressInfo()
    {
        ProgressInfo = _parser.CurrentOperation switch
        {
            OperationType.Unpacking => "Распаковка архива...",
            OperationType.Extraction => "Извлечение вложений...",
            OperationType.Packing => "Подготовка архива с извлеченными вложениями...",
            OperationType.Cleanup => "Очистка...",
            OperationType.Complete => "Готово!",
            _ => string.Empty
        };
    }

    #endregion

    #region ExecutionTime

    /// <summary>
    ///     Время выполнения операции.
    /// </summary>
    private string _executionTime;

    /// <summary>
    ///     Свойство для доступа ко времени выполнения операции.
    /// </summary>
    public string ExecutionTime
    {
        get => _executionTime;
        private set => SetProperty(ref _executionTime, value);
    }

    /// <summary>
    ///     Запускает таймер и секундомер для отслеживания времени выполнения.
    /// </summary>
    private void StartTimer()
    {
        _stopwatch = Stopwatch.StartNew();
        _timer = new Timer(1000);
        _timer.Elapsed += UpdateExecutionTime;
        _timer.Start();
    }

    /// <summary>
    ///     Останавливает таймер и секундомер.
    /// </summary>
    private void StopTimer()
    {
        _stopwatch?.Stop();
        _timer?.Stop();
        UpdateExecutionTime(this, null);
    }

    /// <summary>
    ///     Обработчик события таймера для обновления <see cref="ExecutionTime"/>.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события.</param>
    private void UpdateExecutionTime(object sender, ElapsedEventArgs e)
    {
        ExecutionTime = _stopwatch?.Elapsed.ToString(@"hh\:mm\:ss");
    }

    #endregion
}