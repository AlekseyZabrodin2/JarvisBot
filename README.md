## JarvisBot — Telegram Assistant for Windows

This README is in English. Русская версия: [README.ru.md](README.ru.md)

JarvisBot is a .NET 8 Windows Service that acts as a Telegram bot for personal automation on a Windows PC. It reports currency rates (with optional auto‑updates via timer), weather from Yandex Weather, controls AnyDesk, and can reboot/shutdown/lock the machine via Telegram commands. Config is environment‑aware; secrets are supported via encryption and User Secrets; logging is done with NLog.

### Features
- **Currency rates**: Alfa‑Bank InSync rates; manual query and timer‑based auto‑update
- **Weather**: Yandex Weather API current conditions
- **Telegram keyboards**: contextual menu, currency, auto‑mode, and device control
- **AnyDesk control**: start and gracefully stop AnyDesk processes
- **PC control**: reboot, shutdown, and lock from Telegram (admin only)
- **Logging**: NLog file target with rotation
- **Config & secrets**: `appsettings.*.json`, `UserSecrets` (Dev), encryption (Prod)

### Architecture
- `JarvisMind` — entry point; configures Host, Configuration, DI, starts the service
- `JarvisBackgroundService` — initializes Telegram Bot, receives updates, delegates handling
- `CommunicationMethods` — message and callback handlers: menus, rates/timers, weather, AnyDesk, PC commands
- `Exchange/AlfaBankInSyncRates/*` — currency fetching, timer, and data models
- `Weather/*` — weather loading (Yandex Weather)
- `SecureService/*` — secret encryption (Prod)

## Quick start (Dev)

Requirements:
- .NET SDK 8.0+
- Windows (for Windows Service and PC control scenarios)

1) Clone repository
```bash
git clone <your-repo-url>
cd JarvisBot
```

2) Configure Development
- Defaults live in `JarvisBot/appsettings.json` and are overridden by `appsettings.Development.json`
- Use `UserSecrets` for secrets:
```bash
cd JarvisBot
dotnet user-secrets init
dotnet user-secrets set "JarvisClientSettings:TelegramBotClient" "<TELEGRAM_BOT_TOKEN>"
dotnet user-secrets set "JarvisClientSettings:AdminChatId" "<YOUR_TELEGRAM_USER_ID>"
dotnet user-secrets set "JarvisClientSettings:YandexKey" "<YANDEX_API_KEY>"
dotnet user-secrets set "JarvisClientSettings:AlfabankRate" "https://www.alfabank.by/personal/currency/office/insync/"
dotnet user-secrets set "JarvisClientSettings:YandexWeather" "https://api.weather.yandex.ru/v2/informers?lat=<LAT>&lon=<LON>&lang=en_US"
dotnet user-secrets set "JarvisClientSettings:OldExchangeRatesPath" "<PATH_TO_OldExchangeRates.json>"
```

3) Run
```bash
dotnet build
dotnet run --project JarvisBot
```

Expected: the bot starts, sends a greeting to admin, and begins handling commands.

## Production

### Config & encryption
In Production, store sensitive `JarvisClientSettings` fields encrypted and decrypt on startup via `SecureService`.

- Example for `appsettings.Production.json`:
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

### Install as Windows Service
App is built with `UseWindowsService()`.

Option 1 — `sc.exe` (after publish):
```powershell
dotnet publish JarvisBot -c Release -o .\publish
sc create JarvisBot binPath= "C:\path\to\publish\JarvisBot.exe" start= auto
sc start JarvisBot
```

Option 2 — PowerShell `New-Service`:
```powershell
dotnet publish JarvisBot -c Release -o .\publish
New-Service -Name JarvisBot -BinaryPathName "C:\path\to\publish\JarvisBot.exe" -StartupType Automatic
Start-Service JarvisBot
```

Stop/remove:
```powershell
Stop-Service JarvisBot
sc delete JarvisBot
```

## Configuration

- `JarvisBot/appsettings.json` — base settings (placeholders in repo)
- `appsettings.{Environment}.json` — environment overrides
- `UserSecrets` — secrets for Development
- `EnvironmentVariables` — supported for all keys

Key sections:
- **JarvisClientSettings**
  - `TelegramBotClient` — Telegram bot token
  - `AdminChatId` / `AdminChatIdString` — admin user ID (Prod may store encrypted string)
  - `YandexKey` — Yandex Weather API key
  - `AlfabankRate` — source URL for rates
  - `YandexWeather` — weather informer URL
  - `OldExchangeRatesPath` — path to previous rates file (for diff/auto‑update)
- **EncryptionSettings**
  - `EncryptionKey`, `EncryptionSalt` — encryption parameters for Prod

## Logging

NLog configuration lives in `JarvisBot/NLog.config`. By default logs go to `${basedir}/logs` with daily archiving. Enable console target if required.

## Security

- PC control commands execute locally. Restrict access to admin only — ensure `AdminChatId` check is correct.
- Do not commit real tokens/keys. Use `UserSecrets` (Dev) and encryption/env variables (Prod).

## Troubleshooting

- **Service exits right after start**: ensure the background service does not return from `ExecuteAsync` before shutdown and passes `stoppingToken` to Telegram listener.
- **NLog config error**: if `logconsole` target is commented, remove it from rules or uncomment the target.
- **401/403 from Telegram**: verify bot token and that the user hasn't blocked the bot.
- **AnyDesk issues**: verify `AnyDesk.exe` path and elevation (Windows services may lack interactive desktop access).

## Usage

- In Telegram, send “Menu”, then choose: “Currency rates”, “Auto”, “Weather”, “Device”, “Help”, etc.
- Admin‑only commands require a valid `AdminChatId`.



