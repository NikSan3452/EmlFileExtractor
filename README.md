# EmlExtractor

## Описание

**EmlExtractor** - это приложение на WPF, предназначенное для извлечения вложений из файлов электронной почты в формате `.eml`, а также из архивов `.zip`, `.rar`, `.tgz`. Программа позволяет выбрать файл или архив, содержащий `.eml` файлы, выбрать папку для сохранения извлеченных вложений, и запустить процесс извлечения. Прогресс выполнения операции отображается в реальном времени, а также ведется подсчет времени, затраченного на выполнение задачи.

## Функциональные возможности

*   **Извлечение вложений из одиночных `.eml` файлов.**
*   **Извлечение вложений из архивов (`.zip`, `.rar`, `.tgz`), содержащих `.eml` файлы.**
*   **Отображение прогресса операции:**
    *   Процент выполнения.
    *   Текущая операция (распаковка, извлечение, упаковка, очистка).
*   **Подсчет времени выполнения операции.**
*   **Возможность отмены операции.**
*   **Настраиваемая временная директория.**
*   **Опция удаления исходного файла после обработки.**
*   **Сохранение извлеченных вложений в отдельный `.zip` архив.**
*   **Интуитивно понятный пользовательский интерфейс.**

## Используемые технологии

*   **C#**
*   **WPF (Windows Presentation Foundation)**
*   **Prism Library** (для реализации паттерна MVVM и команд)
*   **Ookii.Dialogs.Wpf** (для диалогов выбора папки)
*   **OpenPop.NET** (для парсинга `.eml` файлов)
*   **SharpCompress** (для работы с архивами)

## Сборка и запуск

1. **Клонируйте репозиторий:**
    ```bash
    git clone https://github.com/NikSan3452/EmlFileExtractor.git
    ```
2. **Откройте решение** в Visual Studio.
3. **Восстановите NuGet пакеты:**
    *   В Visual Studio перейдите в `Tools` -> `NuGet Package Manager` -> `Manage NuGet Packages for Solution...`
    *   Нажмите кнопку `Restore`.
4. **Соберите проект:**
    *   В Visual Studio перейдите в `Build` -> `Build Solution` (или нажмите `Ctrl + Shift + B`).
5. **Запустите приложение:**
    *   В Visual Studio перейдите в `Debug` -> `Start Debugging` (или нажмите `F5`).

## Использование

1. **Выбор файла/архива с email-сообщениями:**
    *   Нажмите кнопку "Выбрать файл .eml, .rar, .zip или .tgz".
    *   В открывшемся диалоговом окне выберите файл с расширением `.eml`, `.zip`, `.rar` или `.tgz`.
    *   Выбранный путь отобразится в поле "Путь к файлу .eml".
2. **Выбор папки для сохранения вложений:**
    *   Нажмите кнопку "Выбрать папку".
    *   В открывшемся диалоговом окне выберите папку, в которую будут сохранены извлеченные вложения.
    *   Выбранный путь отобразится в поле "Папка для сохранения".
3. **Запуск процесса извлечения:**
    *   Нажмите кнопку "Старт".
    *   Начнется процесс извлечения вложений. Прогресс выполнения будет отображаться в полях "Прогресс" и "Операция".
    *   Время, затраченное на выполнение операции, будет отображаться в поле "Время выполнения".
4. **Отмена процесса извлечения:**
    *   Чтобы отменить процесс извлечения, нажмите кнопку "Отмена".

**Результат:**

*   После успешного завершения операции, в выбранной папке для сохранения будет создан `.zip` архив с извлеченными вложениями.
*   В случае отмены операции, временные файлы и папки будут удалены.

## Лицензия

Этот проект распространяется под лицензией MIT.
