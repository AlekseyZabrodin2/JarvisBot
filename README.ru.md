## JarvisBot — Telegram-бот‑помощник для Windows

English version: [README.md](README.md)

JarvisBot — это .NET 8 Windows Service, который работает как Telegram‑бот для личной автоматизации на ПК. Бот сообщает курсы валют (с авто‑обновлением по таймеру), погоду с Yandex Weather, управляет AnyDesk, а также умеет перезагружать/выключать/блокировать компьютер по командам из Telegram. Конфигурация разделена по средам, секреты поддерживаются через шифрование и User Secrets, логирование — через NLog.

### Возможности
- **Курсы валют**: загрузка InSync-курсов Альфа‑Банка; ручной запрос и авто‑обновление по таймеру
- **Погода**: получение текущей погоды через Yandex Weather API
- **Клавиатуры Telegram**: контекстные кнопки меню, валют, авто‑режима и управления
- **Управление AnyDesk**: запуск и корректное завершение процессов AnyDesk
- **Управление ПК**: перезагрузка, выключение, блокировка рабочей станции из Telegram (для администратора)
- **Логирование**: NLog в файл с ротацией
- **Конфигурация и секреты**: `appsettings.*.json`, `UserSecrets` (Dev), шифрование (Prod)

### Архитектура
- `JarvisMind` — точка входа; настраивает Host, конфигурации, DI, запускает сервис
- `JarvisBackgroundService` — фоновый сервис, инициализирует Telegram Bot, принимает апдейты, делегирует обработку
- `CommunicationMethods` — обработчики команд и callback‑ов: меню, валюты/таймеры, погода, AnyDesk, команды управления ПК
- `Exchange/AlfaBankInSyncRates/*` — загрузка курсов, таймер и модели данных
- `Weather/*` — загрузка погоды (через Yandex Weather)
- `SecureService/*` — шифрование секретов (Prod)

## Быстрый старт (Dev)

Требования:
- .NET SDK 8.0+
- Windows (для сценариев Windows Service и управления ПК)

1) Клонировать репозиторий
```bash
git clone <your-repo-url>
cd JarvisBot
```

2) Настроить конфигурацию Development
- Значения по умолчанию хранятся в `JarvisBot/appsettings.json` и перекрываются `appsettings.Development.json`
- Для секретов используйте `UserSecrets`:
```bash
cd JarvisBot
dotnet user-secrets init
dotnet user-secrets set "JarvisClientSettings:TelegramBotClient" "<TELEGRAM_BOT_TOKEN>"
dotnet user-secrets set "JarvisClientSettings:AdminChatId" "<YOUR_TELEGRAM_USER_ID>"
dotnet user-secrets set "JarvisClientSettings:YandexKey" "<YANDEX_API_KEY>"
dotnet user-secrets set "JarvisClientSettings:AlfabankRate" "https://www.alfabank.by/personal/currency/office/insync/"
dotnet user-secrets set "JarvisClientSettings:YandexWeather" "https://api.weather.yandex.ru/v2/informers?lat=<LAT>&lon=<LON>&lang=ru_RU"
dotnet user-secrets set "JarvisClientSettings:OldExchangeRatesPath" "<PATH_TO_OldExchangeRates.json>"
```

3) Запуск
```bash
dotnet build
dotnet run --project JarvisBot
```

Ожидаемое поведение: бот запустится, отправит приветственное сообщение администратору, начнет принимать команды.

## Продакшн

### Конфигурация и шифрование
В Production чувствительные значения секции `JarvisClientSettings` рекомендуется хранить в зашифрованном виде и расшифровывать на старте через `SecureService`.

- Пример секции в `appsettings.Production.json`:
```json
{
  "JarvisClientSettings": {
    "TelegramBotClient": "<ENCRYPTED>",
    "AdminChatIdString": "<ENCRYPTED>",
    "YandexKey": "<ENCRYPTED>",
    "AlfabankRate": "<ENCRYPTED>",
    "YandexWeather": "<ENCRYPTED>",
    "OldExchangeRatesPath": "<ENCRYPTED>"
  },
  "EncryptionSettings": {
    "EncryptionKey": "<PROD_KEY>",
    "EncryptionSalt": "<PROD_SALT>"
  }
}
```

### Установка как Windows Service
Сервис можно развернуть как обычное .NET приложение, использующее `UseWindowsService()`.

Вариант 1. С помощью `sc.exe` (после публикации):
```powershell
dotnet publish JarvisBot -c Release -o .\publish
sc create JarvisBot binPath= "C:\path\to\publish\JarvisBot.exe" start= auto
sc start JarvisBot
```

Вариант 2. С помощью PowerShell `New-Service`:
```powershell
dotnet publish JarvisBot -c Release -o .\publish
New-Service -Name JarvisBot -BinaryPathName "C:\path\to\publish\JarvisBot.exe" -StartupType Automatic
Start-Service JarvisBot
```

Остановка/удаление:
```powershell
Stop-Service JarvisBot
sc delete JarvisBot
```

## Конфигурация

- `JarvisBot/appsettings.json` — базовые настройки (в репозитории значения-заглушки)
- `appsettings.{Environment}.json` — настройки для среды
- `UserSecrets` — секреты для Development
- `EnvironmentVariables` — поддерживаются все ключи конфигурации

Ключевые секции:
- **JarvisClientSettings**
  - `TelegramBotClient` — токен бота Telegram
  - `AdminChatId`/`AdminChatIdString` — ID администратора (в Prod может храниться зашифрованным как строка)
  - `YandexKey` — ключ API Yandex Weather
  - `AlfabankRate` — URL источника курсов
  - `YandexWeather` — URL для вызова информера погоды
  - `OldExchangeRatesPath` — путь к файлу с предыдущими курсами (для сравнения/авто‑обновления)
- **EncryptionSettings**
  - `EncryptionKey`, `EncryptionSalt` — параметры для шифрования секретов в Prod

## Логирование

Используется NLog, конфиг в `JarvisBot/NLog.config`. По умолчанию логи пишутся в `${basedir}/logs` с ежедневной архивацией. При необходимости включите консольную цель.

## Безопасность

- Команды управления ПК (перезагрузка/выключение/блокировка) выполняются на локальной машине. Разрешайте доступ только администратору — проверка должна опираться на корректный `AdminChatId`.
- Не коммитьте реальные токены/ключи в репозиторий. Для Dev — `UserSecrets`, для Prod — шифрование/переменные окружения.

## Частые проблемы и их решения

- **Сервис сразу завершается после старта**: убедитесь, что фоновый сервис не возвращает из `ExecuteAsync` до остановки и передает системный `stoppingToken` в слушатель Telegram.
- **NLog ошибка конфигурации**: если цель `logconsole` закомментирована, уберите её из правила `writeTo` или раскомментируйте саму цель.
- **401/403 от Telegram**: проверьте токен бота и что бот не заблокирован у получателя.
- **Проблемы с AnyDesk**: проверьте путь к `AnyDesk.exe` и права запуска (в некоторых сценариях требуется запуск от администратора/службы с интерактивным доступом недоступен).

## Сценарии использования

- В Telegram отправьте «Меню»/«Menu», выберите раздел: «Курсы валют», «Auto», «Погода», «Device», «Help» и т.д.
- Для админ‑команд требуется корректный `AdminChatId`.


