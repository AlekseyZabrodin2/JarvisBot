﻿using JarvisBot.Data;
using JarvisBot.Exchange.AlfaBankInSyncRates;
using JarvisBot.KeyboardButtons;
using JarvisBot.Weather;
using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace JarvisBot
{
    public class CommunicationMethods
    {
        private static readonly ChatId _adminChatId = new(TelegramBotConfiguration.LoadChatIdConfiguration());
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private static JarvisKeyboardButtons _keyboardButtons = new();
        private static Message _botMessage = new();
        private static ExchangeRateLoder _exchangeRateLoder = new();
        private Process? _anyDeskProcess;


        public async Task ProcessingMessage(ITelegramBotClient botClient, Message message, User botUsername)
        {
            await HandleGreetingAsync(botClient, message);
            await HandleMenuAsync(botClient, message);
            await HandleCurrencyAsync(botClient, message);
            await HandleRatesAsync(botClient, message);
            await HandleWeatherAsync(botClient, message);
            await HandleBackToMenuAsync(botClient, message);

            await HandleHelpButtonAsync(botClient, message);
            await HandleDeviceButtonAsync(botClient, message);

            await HandleRebootButtonAsync(botClient, message);

            if (_botMessage.Text == null || _botMessage.Text == string.Empty)
            {
                await HandleUnknownMessageAsync(botClient, message);             
            }
            
            WriteAnswerInBotConsole(botUsername, _botMessage);
        }

        public async Task ProcessingCallback(ITelegramBotClient botClient, CallbackQuery query)
        {
            await HandleStartAnyDeskAsync(botClient, query);
            await HandleRebootPCAsync(botClient, query);
        }


        private static void WriteAnswerInBotConsole(User botUsername, Message message)
        {
            if (message.Chat.Username == null)
            {
                Console.WriteLine($"Ответ - {botUsername.Username} --> {message.Chat.FirstName} {message.Chat.LastName}({message.Chat.Id})) || сообщение - '{message.Text}' ");
                _logger.Info($"Ответ - {botUsername.Username} --> {message.Chat.FirstName} {message.Chat.LastName}({message.Chat.Id})) || сообщение - '{message.Text}' ");
            }
            else
            {
                Console.WriteLine($"Ответ - {botUsername.Username} --> {message.Chat.Username}({message.Chat.Id})) || сообщение - '{message.Text}' ");
                _logger.Info($"Ответ - {botUsername.Username} --> {message.Chat.Username}({message.Chat.Id})) || сообщение - '{message.Text}' ");
            }

            _botMessage.Text = string.Empty;
        }

        public async Task HandleGreetingAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Привет", StringComparison.CurrentCultureIgnoreCase))
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Privet");
            }
        }

        public async Task HandleBackToMenuAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "< Back")
            {
                if (message.Text == "< Back" && message.Chat.Id == _adminChatId)
                {
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Вы в МЕНЮ",
                        replyMarkup: _keyboardButtons.GetAdminMenuButtons());
                }
                else
                {
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Вы в МЕНЮ",
                        replyMarkup: _keyboardButtons.GetMenuButtons());
                }                
            }
        }

        public async Task HandleMenuAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Меню", StringComparison.CurrentCultureIgnoreCase) ||
                message.Text.Contains("Menu", StringComparison.CurrentCultureIgnoreCase))
            {
                if (message.Text.Contains("Меню", StringComparison.CurrentCultureIgnoreCase) ||
                    message.Text.Contains("Menu", StringComparison.CurrentCultureIgnoreCase) && message.Chat.Id == _adminChatId)
                {
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose",
                        replyMarkup: _keyboardButtons.GetAdminMenuButtons());
                }
                else
                {
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose",
                        replyMarkup: _keyboardButtons.GetMenuButtons());
                }
            }
        }

        public async Task HandleCurrencyAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Курсы валют", StringComparison.CurrentCultureIgnoreCase))
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Выберите валюту", 
                    replyMarkup: _keyboardButtons.GetMoneyButtons());
            }
        }

        public async Task HandleRatesAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "USD" || message.Text == "EUR" || message.Text == "RUB")
            {
                var rateMessage = _exchangeRateLoder.RatesResponse(message.Text);
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, rateMessage);
            }
        }

        public async Task HandleWeatherAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "Погода")
            {
                var weatuerMessage = WeatherLoder.WeatherResponse();
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, await weatuerMessage);
            }
        }

        public async Task HandleHelpButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Help", StringComparison.CurrentCultureIgnoreCase) && message.Chat.Id == _adminChatId)
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Что-то включить?", 
                    replyMarkup: _keyboardButtons.GetHelpSubmenuButtons());
            }
        }

        public async Task HandleDeviceButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "Device" && message.Chat.Id == _adminChatId)
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Вы в меню управления программой - [AnyDesk]", 
                    replyMarkup: _keyboardButtons.GetStartAnyDeskButtons());

                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Что сделать с программой, Сэр?",
                    replyMarkup: _keyboardButtons.GetBackButtons());
            }
        }

        public async Task HandleStartAnyDeskAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == "Start_AnyDesk")
            {
                await botClient.SendTextMessageAsync(_botMessage.Chat.Id, "AnyDesk включается...");
                StartAnyDesk(botClient, _botMessage);
            }
            else if (callbackQuery.Data == "Cancel_AnyDesk")
            {
                await botClient.SendTextMessageAsync(_botMessage.Chat.Id, "Выключение AnyDesk.", replyMarkup: _keyboardButtons.GetAdminMenuButtons());
                await StopAnyDesk(botClient, _botMessage);
            }
        }

        public void StartAnyDesk(ITelegramBotClient botClient, Message message)
        {
            if (!Process.GetProcessesByName("AnyDesk").Any())
            {
                string processName = @"C:\Program Files (x86)\AnyDesk\AnyDesk.exe";

                _anyDeskProcess = new()
                {
                    StartInfo =
                    {
                        FileName = processName,
                        Verb = "runas" // "runas" указывает на запуск с правами администратора
                    }
                };

                try
                {
                    _anyDeskProcess.Start();

                    if (Process.GetProcessesByName("AnyDesk").Any())
                    {
                        botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk запущен");
                        Console.WriteLine("AnyDesk запущен");
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat.Id, "Проблемы с запуском...");
                    }
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk уже запущен");
                Console.WriteLine("AnyDesk уже запущен");
            }
        }

        public async Task<bool> StopAnyDesk(ITelegramBotClient botClient, Message message)
        {
            if (_anyDeskProcess != null)
            {
                await CloseAnyDeskProcesses(botClient, message);
            }

            await Task.Delay(1000);

            if (!Process.GetProcessesByName("AnyDesk").Any())
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk закрыт");
                Console.WriteLine("AnyDesk закрыт");
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "... закрываем AnyDesk повторно");
                Console.WriteLine("... закрываем AnyDesk повторно");

                await CloseAnyDeskProcesses(botClient, message);

                await botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk закрыт");
                Console.WriteLine("AnyDesk закрыт");
            }

            return true;
        }

        private async Task CloseAnyDeskProcesses(ITelegramBotClient botClient, Message message)
        {
            foreach (var process in Process.GetProcessesByName("AnyDesk"))
            {
                process.Kill();

                await botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk закрывается...");
                Console.WriteLine("AnyDesk закрывается...");
            }
        }

        public async Task HandleRebootButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "Something" && message.Chat.Id == _adminChatId)
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "ВНИМАНИЕ !!! \r\nВы вошли в настройки управления компьютером:", 
                    replyMarkup: _keyboardButtons.GetRebootButtons());

                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Еще не поздно вернуться назад, Сэр.",
                    replyMarkup: _keyboardButtons.GetBackButtons());
            }            
        }

        public async Task HandleRebootPCAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == "PC_Reboot")
            {
                await botClient.SendTextMessageAsync(_botMessage.Chat.Id, "Ждите компьютер ПЕРЕЗАГРУЖАЕТСЯ...");
                RebootPcClick(botClient, _botMessage);
            }
            else if (callbackQuery.Data == "PC_PowerOFF")
            {
                await botClient.SendTextMessageAsync(_botMessage.Chat.Id, "ВЫКЛЮЧЕНИЕ компьютера...");
                PowerOffPcClick(botClient, _botMessage);
            }
            else if (callbackQuery.Data == "PC_Lock")
            {
                await botClient.SendTextMessageAsync(_botMessage.Chat.Id, "Компьютер ЗАБЛОКИРОВАН, сэр !");
                LockPcClick(botClient, _botMessage);
            }
        }

        public void RebootPcClick(ITelegramBotClient botClient, Message message)
        {
            string rebootPC = @"D:\Develop\Reboot.bat";
            Process.Start(rebootPC);

            botClient.SendTextMessageAsync(message.Chat.Id, "Ждите Я скоро ..!");
            Console.WriteLine("Ждите Я скоро ..!");
        }

        public void PowerOffPcClick(ITelegramBotClient botClient, Message message)
        {
            string powerOffPC = @"D:\Develop\PowerOFF.bat";
            Process.Start(powerOffPC);
        }

        public void LockPcClick(ITelegramBotClient botClient, Message message)
        {
            string lockPC = @"D:\Develop\LockPC.bat";
            Process.Start(lockPC);
        }

        public async Task HandleUnknownMessageAsync(ITelegramBotClient botClient, Message message)
        {
            _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Я отправлю эту информацию в архив, Сэр !");            
        }
    }
}
