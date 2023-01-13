using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CrudBot.DAL.Repository;
using DAL.Model;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
using User = Telegram.Bot.Types.User;

namespace DAL
{
    internal class Program
    {
        // Token;
        private static readonly TelegramBotClient Bot = new TelegramBotClient("Your tokken!");

        private static readonly string JsonFilePath = Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location);

        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["DBConection"].ConnectionString;

        private static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();
        private static readonly CancellationToken _cancellationToken = TokenSource.Token;

        private static UserDto _userDto;

        private static readonly UserRepository UserRepository = new UserRepository(ConnectionString);

        private static void Main(string[] args)
        {
            var jsonData = File.ReadAllText(Path.Combine(JsonFilePath, "users.json"));

            _userDto = JsonConvert.DeserializeObject<UserDto>(jsonData);


            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Console.WriteLine("Starting Ledrunner Bot.....");

            var me = Bot.GetMeAsync().Result;

            Console.Title = me.Username;
            Bot.StartReceiving();
            Console.WriteLine("Press any key to stop the programm");
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        private static void BotOnChosenInlineResultReceived(object sender,
            ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine(
                $"Received choosen inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            InlineQueryResult[] results =
            {
                new InlineQueryResultLocation
                {
                    Id = "1",
                    Latitude = 40.7058316f, // displayed result
                    Longitude = -74.2581888f,
                    Title = "New York",
                    InputMessageContent = new InputLocationMessageContent // message if result is selected
                    {
                        Latitude = 40.7058316f,
                        Longitude = -74.2581888f
                    }
                },

                new InlineQueryResultLocation
                {
                    Id = "2",
                    Longitude = 52.507629f, // displayed result
                    Latitude = 13.1449577f,
                    Title = "Berlin",
                    InputMessageContent = new InputLocationMessageContent // message if result is selected
                    {
                        Longitude = 52.507629f,
                        Latitude = 13.1449577f
                    }
                }
            };

            await Bot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, results, isPersonal: true,
                cacheTime: 0);
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage)
            {
                return;
            }

            if (message.Text.StartsWith("/inline")) // send inline keyboard
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] // first row
                    {
                        new InlineKeyboardButton("1.1"),
                        new InlineKeyboardButton("1.2")
                    },
                    new[] // second row
                    {
                        new InlineKeyboardButton("2.1"),
                        new InlineKeyboardButton("2.2")
                    }
                });

                await Task.Delay(500); // simulate longer running task

                await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
                    replyMarkup: keyboard);
            }
            else if (message.Text.StartsWith("/keyboard")) // send custom keyboard
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[] // first row
                    {
                        new KeyboardButton("1.1"),
                        new KeyboardButton("1.2")
                    },
                    new[] // last row
                    {
                        new KeyboardButton("2.1"),
                        new KeyboardButton("2.2")
                    }
                });

                await Bot.SendTextMessageAsync(message.Chat.Id, "Choose",
                    replyMarkup: keyboard);
            }
            else if (message.Text.StartsWith("/photo")) // send a photo
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                const string file = @"your path...";

                var fileName = file.Split('\\').Last();

                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var fts = new FileToSend(fileName, fileStream);

                    await Bot.SendPhotoAsync(message.Chat.Id, fts, "Nice Picture");
                }
            }
            else if (message.Text.StartsWith("/request")) // request location or contact
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton("Location")
                    {
                        RequestLocation = true
                    },
                    new KeyboardButton("Contact")
                    {
                        RequestContact = true
                    }
                });

                await Bot.SendTextMessageAsync(message.Chat.Id, "Who or Where are you?", replyMarkup: keyboard);
            }

            else if (message.Text.StartsWith("/getusers")) // request DBUsers;
            {
                var users = await UserRepository.ReadUsersAsync(_cancellationToken);
                var telegramUsers = new User();
                foreach (var user in users)
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id,
                        Convert.ToString(user.Id + " " + user.FirstName + " " + user.LastName + "\n"),
                        replyMarkup: new ReplyKeyboardHide());
                }

                foreach (var item in users)
                {
                    Console.WriteLine(item.Id + " " + item.FirstName + " " + item.LastName);
                }
            }

            else if (message.Text.StartsWith("/filldata"))
            {
                foreach (var user in _userDto.Users)
                {
                    await UserRepository.AddUserAsync(user.Name, user.LastName, _cancellationToken);
                }

                await Bot.SendTextMessageAsync(message.Chat.Id, "Database has been filled!",
                    replyMarkup: new ReplyKeyboardHide());
            }

            else if (message.Text.StartsWith("/cleardatabase"))
            {
                await UserRepository.DeleteAllAsync(_cancellationToken);

                await Bot.SendTextMessageAsync(message.Chat.Id, "Database has been cleared!",
                    replyMarkup: new ReplyKeyboardHide());
            }
            else
            {
                var usage = @"Usage:
                            /inline   - send inline keyboard
                            /keyboard - send custom keyboard
                            /photo    - send a photo
                            /request  - request location or contact
                            /filldata - filling database
                            /getusers - get users from database
                            /cleardatabase - delete all data from data base
                            ";

                await Bot.SendTextMessageAsync(message.Chat.Id, usage,
                    replyMarkup: new ReplyKeyboardHide());
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender,
            CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}