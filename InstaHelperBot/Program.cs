using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Telegram.Bot.Types.Enums;
using Npgsql;
using InstaBotHelper;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text;
using System.Text.RegularExpressions;
using InstagramApiSharp.API.Processors;
using InstagramApiSharp.Logger;
using InstagramApiSharp.Classes.SessionHandlers;
using System.Diagnostics;

using static InstaHelperBot.MultipleHelper;
using InstaHelperBot;
using System.ComponentModel;

namespace TelegramBotExperiments
{

    public class Program
    {

        public static ITelegramBotClient bot = new TelegramBotClient(token: Environment.GetEnvironmentVariable("token"));

        public string connString = "Host=dbb;Username=insta;Password=botinsat2003;Database=botinstanalis";

        private Task<IInstaApi> _login;
        public Task<IInstaApi> LoginApi => _login ??= LoginAsync(АccountList().First().UserName, АccountList().First().Password);

        public string tokenAccess = "56037E081114472B954E86E9B75D39AF";
        public bool isAdminPanel = false;

        public List<Posts> Posts => GetPosts();
        public List<InstaBotHelper.User> UsersList => GetUsers();
        public List<TelegramGroup> TelegramGroupList => GetNameCodeGroup();

        public List<Аccount> АccountList1 = new List<Аccount>();

        public List<Аccount> АccountList()
        {
            return АccountList1 is not null ? GetАccount() : new List<Аccount>();

        }

        public async Task<InstaMediaList> GetInstaPostList() 
        { 
            return await GetInstaPost(); 
        }

        public async Task<IInstaApi> LoginAsync(string user, string password)
        {
            var userSession = new UserSessionData
            {
                UserName = user,
                Password = password
            };

            var api = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .Build();
            await api.LoginAsync();

            return api;
        }

        public List<DictionaryReplace> DictionaryReplaceList => GetDictionaryReplace();
        public string nameProfilInstagram;

        private static bool isLoading = false;
        private static bool isLoadingAll = false;
        private int replaceF = 0;
        private DictionaryReplace _dictionaryReplace = new DictionaryReplace { };

        private long chatIdCh { get; set; }
        private long chatIdCh1 { get; set; }
        private bool setupTG = false;
        private Аccount account = new Аccount() { };

        private bool isNameChanal = false;
        private bool isAccaunt = false;

        public static bool isStopProces = false;

        private string NameAccaunt()
        {
            return account.TypeAcc ?? (АccountList().Count > 0 ? АccountList().First().TypeAcc : null) ?? "Не задан аккаунт инстаграма";
        }

        static readonly ManualResetEventSlim ExitEvent = new ManualResetEventSlim();

        public  async Task<IResult<InstaMediaList>> GetUserMediaAsync(
             PaginationParameters paginationParameters, IResult<InstaUser> instaUser, IUserProcessor userProcessor)
        {
            if (!instaUser.Succeeded)
                return Result.Fail<InstaMediaList>("Unable to get user to load media");
            
            var res = await userProcessor.GetUserMediaByIdAsync(instaUser.Value.Pk, paginationParameters);
           
            return  res;
        }

       
        public  List<InstaMedia> GetUserMedia( IResult<InstaUser> instaUser, IUserProcessor userProcessor)
        {
            bool isAction = true;
            List<InstaMedia> mediaList = new List<InstaMedia>();
            int count = 1;
            
            while (isAction)
            {
                Console.WriteLine($"Add media {count}");
                var mediaResult =  GetUserMediaAsync( PaginationParameters.MaxPagesToLoad(count), instaUser, userProcessor).Result;
               
                if (mediaResult.Succeeded)
                {
                    Console.WriteLine($"Succeeded");
                    mediaList.AddRange(mediaResult.Value);
                    count++;
                    
                }
                else
                {
                    Console.WriteLine($"Failed to get user media: {mediaResult.Info.Message}");
                    isAction = false;
                }
            }


            return mediaList;
        }



        async Task RunBot()
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            var isActionLoading = false;

            // Создаем таймер
            await Task.Run(() => new Timer(async (state) =>
            {

                if (!isActionLoading)
                {
                    Console.WriteLine("Таймер сработал");
                    isActionLoading = true;
                    try
                    {
                        Console.WriteLine($"{isLoading} {pr.АccountList().Count} {state}");
                        if (isLoading && pr.АccountList().Count != 0)
                        {
                            await Task.Run(() => getloginAsync());



                            try
                            {
                                foreach (var item in _mediaList.Value.OrderBy(x => x.TakenAt).ToList())
                                {
                                    if (!isStopProces)
                                        if (!pr.Posts.Select(x => x.IdPosts.ToString()).ToList().Contains(item.Pk))
                                        {
                                            pr.SentMessagePostInBot(item, bot, pr.chatIdCh, cancellationToken);
                                            pr.QueryInsertPost(Convert.ToInt64(item.Pk), "true", item.ProductType);
                                            await Task.Delay(20000);
                                        }
                                }
                            }

                            catch
                            {

                            }

                            


                            foreach (var itemAcc in ApiList)
                            {
                                bool isRepAcc = false;

                                try
                                {
                                    var v1 =  itemAcc.UserProcessor.GetUserMediaAsync(pr.nameProfilInstagram, PaginationParameters.Empty);
                                    
                                    foreach (var item in v1.Result.Value.OrderBy(x => x.TakenAt).ToList())
                                    {
                                        if (!isStopProces)
                                            if (!pr.Posts.Select(x => x.IdPosts.ToString()).ToList().Contains(item.Pk))
                                            {
                                                pr.SentMessagePostInBot(item, bot, pr.chatIdCh, cancellationToken);
                                                pr.QueryInsertPost(Convert.ToInt64(item.Pk), "true", item.ProductType);
                                                await Task.Delay(20000);
                                            }
                                    }
                                }

                                catch
                                {
                                    isRepAcc = true;
                                }
                                //try
                                //{

                                //    var latestPosts = await itemAcc.UserProcessor.GetUserMediaAsync(nameProfilInstagram, PaginationParameters.MaxPagesToLoad(1));
                                //    var c1 = latestPosts.Value;

                                //    foreach (var item in latestPosts.Value.OrderBy(x => x.TakenAt).ToList())
                                //    {
                                //        if (!isStopProces)
                                //            if (!pr.Posts.Select(x => x.IdPosts.ToString()).ToList().Contains(item.Pk))
                                //            {
                                //                pr.SentMessagePostInBot(item, bot, pr.chatIdCh, cancellationToken);
                                //                pr.QueryInsertPost(Convert.ToInt64(item.Pk), "true", item.ProductType);
                                //                await Task.Delay(10000);
                                //            }
                                //    }
                                //}
                                //catch { isRepAcc = true; }

                                var userResult = await itemAcc.UserProcessor.GetUserAsync(pr.nameProfilInstagram);
                                try
                                {
                                    if (userResult.Value != null)
                                    {
                                        var storyResult = itemAcc.StoryProcessor.GetUserStoryFeedAsync(userResult.Value.Pk);
                                        if (!storyResult.Result.Succeeded)
                                        {
                                            Console.WriteLine($"Ошибка получения сторис пользователя: {storyResult.Result.Info.Message}");
                                        }
                                        else
                                        {
                                            foreach (var item in storyResult.Result.Value.Items)
                                            {
                                                if (!isStopProces)
                                                    if (!pr.Posts.Select(x => x.IdPosts.ToString()).ToList().Contains(item.Pk.ToString()))
                                                    {
                                                        try
                                                        {
                                                            pr.SentMessagePostInBot(item, bot, pr.chatIdCh, cancellationToken);
                                                            pr.QueryInsertPost(Convert.ToInt64(item.Pk), "true", "storis");

                                                            await Task.Delay(1000);

                                                        }
                                                        catch
                                                        {
                                                            Console.WriteLine("2");
                                                        }
                                                    }
                                            }
                                        }
                                    }
                                }
                                catch { isRepAcc = true; }

                                if (!isRepAcc) break;

                                try
                                {
                                    bot.SendTextMessageAsync(pr.chatIdCh1, @$"Аккаунт {itemAcc.GetCurrentUserAsync().Result.Value.UserName} не доступен");
                                }
                                catch { }
                            }//перебирает список аккаунтов
                        }
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine("4");
                        Console.WriteLine(x.Message);
                    }
                    isActionLoading = false;
                }

            }, isLoading, TimeSpan.Zero, TimeSpan.FromSeconds(200)));

            bot.ReceiveAsync(
                   pr.HandleUpdateAsync,
                   pr.HandleErrorAsync,
                   receiverOptions,
                   cancellationToken
               );

            // Ожидаем сигнал завершения приложения
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                ExitEvent.Set();
            };

            ExitEvent.Wait();
            Console.ReadLine();
        }

        public static Program pr = new Program();

        private static IResult<InstaMediaList> _mediaList { get; set; }
        public async static Task Main(string[] args)
        {
            Migration migration = new(pr.connString);

            pr.nameProfilInstagram = pr.АccountList().Count > 0 ? pr.АccountList().First().TypeAcc : string.Empty;

            Console.WriteLine("Запущен бот ");

            LoadSessions();

            await Task.Run(() => getloginAsync());

            foreach (var item in ApiList)
            {
                try
                {
                    _mediaList = await item.UserProcessor.GetUserMediaAsync(pr.nameProfilInstagram, PaginationParameters.Empty);
                    InstaApi = item;
                    break;
                }
                catch
                {
                    Console.WriteLine("not item");
                }
            }

            if (_mediaList.Value is not null)
            {
                Console.WriteLine(_mediaList.Value.Count);
            }
            else
            {
                Console.WriteLine("0");
            }
            // InstaApi.LogoutAsync();
            await Task.Run(() => pr.RunBot());

        }

        private static IInstaApi InstaApi;

        //public async static Task CreateAccInsta()
        //{

        //    var api = InstaApiBuilder.CreateBuilder().Build();

        //    var email = "ramtinak@live.com";
        //    var username = "gogager3";
        //    var password = "Dima159874";

        //    var checkEmail = await api.CheckEmailAsync(email);

        //    if (checkEmail.Succeeded && checkEmail.Value.Available)
        //    {
        //        var create = await api.CreateNewAccountAsync(username, password, email);
        //    }

        //}


        public async static Task getloginAsync()
        {
            if (pr.АccountList().Count != 0)
            {
                foreach (var itemAcc in pr.АccountList())
                {
                    var username = itemAcc.UserName;
                    var api = BuildApi(username, itemAcc.Password);
                    var sessionHandler = new FileSessionHandler { FilePath = username.GetAccountPath(), InstaApi = api };

                    api.SessionHandler = sessionHandler;
                    var loginResult = await api.LoginAsync();
                    if (loginResult.Succeeded)
                    {
                        LoggedInUsers.Add(api.GetLoggedUser().LoggedInUser.UserName.ToLower());
                        //ApiList.Clear();
                        ApiList.Add(api);
                        api.SessionHandler.Save();
                    }
                    else
                    {
                        Console.WriteLine($"Error:\r\n{loginResult.Info.Message}\r\n\r\n" +
                            $"Please check ChallengeExample for handling two factor or challenge...");
                    }
                }

                //InstaApi = api;
            }
            else
            { 
            
            }

            //const string StateFile = "state113.bin";
            //var user = "";
            //var password = "";

            //if (!pr.АccountList().Any())
            //{
            //    user = "gogager3";
            //    password = "Dima159874";
            //}
            //else
            //{
            //    user = pr.АccountList().First().UserName;
            //    password = pr.АccountList().First().Password;
            //}


            //var userSession = new UserSessionData
            //{
            //    UserName = user,
            //    Password = password
            //};

            //InstaApi = InstaApiBuilder.CreateBuilder()
            //   .SetUser(userSession)
            //   .UseLogger(new DebugLogger(LogLevel.All))
            //   .SetRequestDelay(RequestDelay.FromSeconds(0, 1))
            //   // Session handler, set a file path to save/load your state/session data
            //   .SetSessionHandler(new FileSessionHandler() { FilePath = StateFile })
            //   .Build();

            ////Load session
            //LoadSession();
            //if (!InstaApi.IsUserAuthenticated)
            //{
            //    // Call this function before calling LoginAsync
            //    await InstaApi.SendRequestsBeforeLoginAsync();
            //    // wait 5 seconds
            //    await Task.Delay(5000);
            //    var logInResult = await InstaApi.LoginAsync();
            //    Debug.WriteLine(logInResult.Value);
            //    if (logInResult.Succeeded)
            //    {
            //        // Call this function after a successful login
            //        await InstaApi.SendRequestsAfterLoginAsync();

            //        // Save session 
            //        SaveSession();
            //    }
            //    else
            //    {
            //        if (logInResult.Value == InstaLoginResult.ChallengeRequired)
            //        {
            //            var challenge = await InstaApi.GetChallengeRequireVerifyMethodAsync();
            //            if (challenge.Succeeded)
            //            {
            //                Console.WriteLine("4");
            //            }
            //            else
            //                Console.WriteLine("3");
            //        }
            //        else if (logInResult.Value == InstaLoginResult.TwoFactorRequired)
            //        {
            //            Console.WriteLine("2");
            //        }
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("1");
            //}

            //void LoadSession()
            //{
            //    InstaApi?.SessionHandler?.Load();

            //}
            //void SaveSession()
            //{
            //    if (InstaApi == null)
            //        return;
            //    if (!InstaApi.IsUserAuthenticated)
            //        return;
            //    InstaApi.SessionHandler.Save();

            //}

        }

        public  async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            Console.WriteLine($"{isLoading}");
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text is null) return;

                if (chatIdCh == 0) chatIdCh = message.Chat.Id;

                if (message.Text.ToLower() == "/start" || message.Text.ToLower() == "перезагрузка".ToLower())
                {
                    isLoading = false;
                    isLoadingAll = false;
                    isStopProces = true;
                    await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать!");
                    if (!UsersList.Any(x => x.IdUniq == message.From.Username))
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Введите токен для подключения");
                    }
                    else
                    {
                        isAdminPanel = true;
                        await botClient.SendTextMessageAsync(message.Chat, @$"Добрый день, {message.From.Username}. Вы уже зарегистрированы");

                        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "Перезагрузка", "Выгрузить посты" },
                                                                          new KeyboardButton[] { "Замена слов", "Удалить посты" },
                                                                          new KeyboardButton[] { "Указать канал ТГ", "Удалить канал ТГ" },
                                                                          new KeyboardButton[] { $"Канал 📸{NameAccaunt()}", "Логин/Пароль Инстаграмма" },
                                                                          new KeyboardButton[] { $"Очистить таблицу аккаунтов" },
                                                                          new KeyboardButton[] { $"Выгрузить аккаунты" }})
                        {
                            ResizeKeyboard = true
                        };
                        Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Вам доступно меню с командами👑",
                        replyMarkup: replyKeyboardMarkup,
                        cancellationToken: cancellationToken);
                    }

                    replaceF = 0;
                    return;
                }

                if (message.Text.ToLower() == tokenAccess.ToLower() || UsersList.Any(x => x.IdUniq == message.From.Username && message.Text.ToLower() == tokenAccess.ToLower()))
                {
                    QueryInsertUser(message.From.Username ?? tokenAccess, "Admin", tokenAccess.ToLower());

                    await botClient.SendTextMessageAsync(message.Chat, "Вход в админку успешен");

                    isAdminPanel = true;
                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "Перезагрузка", "Выгрузить посты" },
                                                                          new KeyboardButton[] { "Замена слов", "Удалить посты" },
                                                                          new KeyboardButton[] { "Указать канал ТГ", "Удалить канал ТГ" },
                                                                          new KeyboardButton[] { $"Канал 📸{NameAccaunt()}", "Логин/Пароль Инстаграмма" },
                                                                          new KeyboardButton[] { $"Очистить таблицу аккаунтов" },
                                                                          new KeyboardButton[] { $"Выгрузить аккаунты" }})
                    {
                        ResizeKeyboard = true
                    };
                    Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Теперь вам доступно меню с командами👑",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                }



                if (isAdminPanel)
                {
                    if (setupTG)
                    {
                        QueryTelegramGroup(message.Text);
                        setupTG = false;

                        chatIdCh1 = Convert.ToInt64(message.Text);
                        await botClient.SendTextMessageAsync(message.Chat, "Успешно установили ID канала для уведомлений");
                        return;

                    }
                    if (message.Text.ToLower() == "Указать канал ТГ".ToLower())
                    {
                        QueryTruncate("TelegramGroup");
                        setupTG = true;
                        await botClient.SendTextMessageAsync(message.Chat, "Введите Id канала");
                        return;
                    }

                    if (message.Text.ToLower() == "Выгрузить аккаунты".ToLower())
                    {
                        foreach (var itemAcc in АccountList())
                        {
                            await botClient.SendTextMessageAsync(message.Chat, @$"Название {itemAcc.UserName}");
                        }
                        return;
                    }

                    if (message.Text.ToLower() == "Удалить канал ТГ".ToLower())
                    {
                        QueryTruncate("TelegramGroup");
                        chatIdCh1 = message.Chat.Id;
                        await botClient.SendTextMessageAsync(message.Chat, "Канал ТГ удален");
                        return;

                    }

                    if (message.Text.ToLower() == "Логин/Пароль Инстаграмма".ToLower())
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Введите логин инстаграмма");

                        account.UserName = string.Empty;
                        account.Password = string.Empty;
                        isAccaunt = true;
                        isLoading = false;
                        isLoadingAll = false;

                        return;
                    }

                    if (message.Text.ToLower() == "Очистить таблицу аккаунтов".ToLower())
                    {

                        QueryTruncate("Аccount");
                        await botClient.SendTextMessageAsync(message.Chat, "Аккаунты почищены");

                        return;
                    }


                    if (account.UserName == string.Empty && account.Password == string.Empty && isAccaunt )
                    {
                        account.UserName = message.Text;
                        await botClient.SendTextMessageAsync(message.Chat, "Введите свой пароль инстаграмма");
                        return;
                    }


                    if (account.UserName != string.Empty && account.Password == string.Empty && isAccaunt)
                    {
                        isAccaunt = false;

                        account.Password = message.Text;
                        await botClient.SendTextMessageAsync(message.Chat, "Успешно сохранили данные для инстаграма");

                        //QueryTruncate("Аccount");
                        nameProfilInstagram = account.TypeAcc;

                        if ((account.TypeAcc != null || NameAccaunt() != "Не задан аккаунт инстаграма") && account.UserName != null && account.Password != null)
                        {
                            QueryInsertАccount(account.TypeAcc, account.UserName, account.Password);

                            try
                            {

                                var userSession = new UserSessionData
                                {
                                    UserName = account.UserName,
                                    Password = account.Password
                                };

                                InstaApi = InstaApiBuilder.CreateBuilder()
                                   .SetUser(userSession)                                   
                                   .Build();
                               
                                if (!InstaApi.IsUserAuthenticated)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, $"Не залогинились, повтори попытку");

                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Успешно получили доступ к инстаграму");
                                    isLoading = true; 
                                }
                            }
                            catch
                            {
                                await botClient.SendTextMessageAsync(message.Chat, $"Не залогинились, повтори попытку ");
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, $"Не задана страница инстаграмма");
                        }

                        
                        return;
                    }

                                      

                    if (message.Text.ToLower() == $"Канал 📸{NameAccaunt()}".ToLower())
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Введите название канала");

                        account.TypeAcc = string.Empty;
                        isNameChanal = true;
                        isLoading = false;
                        isLoading = isLoadingAll;

                        return;
                    }

                    if (account.TypeAcc == string.Empty && isNameChanal)
                    {
                        account.TypeAcc = message.Text.Replace(" ", "") == string.Empty ? "Канал не задан" : message.Text;
                        await botClient.SendTextMessageAsync(message.Chat, $"Канал {account.TypeAcc} задан"); ;
                        isNameChanal = false;


                        if (account.TypeAcc != null && account.UserName != null && account.Password != null)
                        {
                            QueryInsertАccount(account.TypeAcc, account.UserName, account.Password);

                              
                            await botClient.SendTextMessageAsync(message.Chat, "Успешно получили доступ к инстаграму");
                                
                            
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, $"Не заданы учетные данные для входа в инстаграмм");
                        }
                        return;
                    }

                    
                    if (message.Text.ToLower() == "Удалить посты".ToLower())
                    {
                        QueryTruncate("Posts");
                        await botClient.SendTextMessageAsync(message.Chat, "Посты удалены");
                        return;
                    }

                    if (message.Text.ToLower() == "Выгрузить посты".ToLower() && АccountList().Count != 0)
                    {

                        await botClient.SendTextMessageAsync(message.Chat, "Запущен процесс анализа постов📣");
                        if (TelegramGroupList.Count == 0)
                            chatIdCh = message.Chat.Id;
                        isLoading = true;
                        isStopProces = false;
                        return;
                    }

                    if (message.Text.ToLower() == "Выгрузить все посты".ToLower() && АccountList().Count != 0)
                    {

                        await botClient.SendTextMessageAsync(message.Chat, "Запущен процесс анализа всех постов постов📣");
                        if (TelegramGroupList.Count == 0)
                            chatIdCh = message.Chat.Id;
                        isLoadingAll = true;
                        isStopProces = false;
                        return;
                    }

                    if (replaceF == 2)
                    {
                        _dictionaryReplace.Replace = message.Text;
                        await botClient.SendTextMessageAsync(message.Chat, "Связка слов добавлена");
                        replaceF = -1;

                        QueryInsertDictionaryReplace(_dictionaryReplace.Regular, _dictionaryReplace.Replace);
                        return;
                    }

                    if (replaceF == 1)
                    {
                        _dictionaryReplace.Regular = message.Text;
                        await botClient.SendTextMessageAsync(message.Chat, "Введите на что заменить");
                        replaceF = 2;
                        return;
                    }


                    if (message.Text.ToLower() == "Замена слов".ToLower())
                    {
                        InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Выгрузить отчет", callbackData: "order"),
                            InlineKeyboardButton.WithCallbackData(text: "Загрузить данные", callbackData: "loadData"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Очистить таблицу с правилами", callbackData: "clearDataRegex")
                        }
                    });

                        Message s1entMessage = await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Что сделать с правилами по замене?",
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cancellationToken);
                    }
                }
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                var nameCommand = callbackQuery.Data;
                if (nameCommand == "order")
                {
                    StringBuilder build = new StringBuilder();
                    build.AppendLine("Словарь:");
                    foreach (var dictionary in GetDictionaryReplace())
                    {
                        build.AppendLine($@"РегВыражение: {dictionary.Regular}; Заменяем на слово: {dictionary.Replace}");
                    }
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, build.ToString());
                }

                if (nameCommand == "loadData")
                {
                    replaceF = 1;
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введите заменяемое слово или регулярное выражение");
                }

                if (nameCommand == "clearDataRegex")
                {
                    QueryTruncate("DictionaryReplace");
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Таблица очищена");
                }
            }
        }

        public async void SentMessagePostInBot(InstaMedia post, ITelegramBotClient botClient, long Id, CancellationToken cancellationToken)
        {
            string postCaption = post.Caption?.Text.Replace("#", " #") ?? "";

            if (postCaption != "")
            {
                foreach (var replace in DictionaryReplaceList)
                {
                    if (Regex.IsMatch(postCaption, replace.Regular))
                    {
                        postCaption = Regex.Replace(postCaption, replace.Regular, replace.Replace ?? "");
                    }
                }
            }

            int lenght = postCaption.Length;


            if (post.Images.Count != 0 && post.Videos.Count == 0)
            {
                try
                {
                    if (post.ProductType == "carousel_container")

                    {
                        List<IAlbumInputMedia> albom = new List<IAlbumInputMedia> { };
                        foreach (var itemImg in post.Carousel)
                        {
                            if (postCaption.Length >= 1024)
                            {
                                lenght = 1023;
                            }
                            albom.Add(new InputMediaPhoto(InputFile.FromUri(itemImg.Images.First().Uri))
                            {
                                Caption = postCaption.Substring(0, lenght)
                            });

                        }
                        Message[] messages = await botClient.SendMediaGroupAsync(
                                  chatId: Id,
                                  media: albom.AsEnumerable(),
                                  cancellationToken: cancellationToken);

                    }
                    if (post.ProductType == "feed")
                    {

                        if (postCaption.Length >= 1024)
                        {
                            lenght = 1023;
                        }

                        Message message1 = await botClient.SendPhotoAsync(
                        chatId: Id,
                        photo: InputFile.FromUri(post.Images.First().Uri),
                        caption: postCaption.Substring(0, lenght),
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);


                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                }
            }
            if (post.Videos.Count != 0)
            {
                if (postCaption.Length >= 1024)
                {
                    lenght = 1023;
                }
                try
                {
                    Message mesVideo = await botClient.SendVideoAsync(
                        chatId: Id,
                        video: InputFile.FromUri(post.Videos.First().Uri),
                        thumbnail: InputFile.FromUri(post.Images.First().Uri),
                        caption: postCaption.Substring(0, lenght),
                        supportsStreaming: true,
                        cancellationToken: cancellationToken);
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                }
            }
        }

        public async void SentMessagePostInBot(InstaStoryItem post, ITelegramBotClient botClient, long Id, CancellationToken cancellationToken)
        {
            if (post.VideoList.Count != 0)
            {
                Message mesVideo = await botClient.SendVideoAsync(
                        chatId: Id,
                        video: InputFile.FromUri(post.VideoList.First().Uri),
                        thumbnail: InputFile.FromUri(post.ImageList.First().Uri),
                        supportsStreaming: true,
                        cancellationToken: cancellationToken);
            }
            else
            {
                Message message1 = await botClient.SendPhotoAsync(
                        chatId: Id,
                        photo: InputFile.FromUri(post.ImageList.First().Uri),
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
            }

        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        


        public async Task<InstaMediaList> GetInstaPost()
        {
            var mediaResult = await InstaApi.UserProcessor.GetUserMediaAsync(nameProfilInstagram, PaginationParameters.Empty);
            return mediaResult.Value;
        }


        public void QueryInsertPost(long idPosts, string status, string type)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO public.\"Posts\"(\r\n\t\"IdPosts\", \"Status\", \"Type\")\r\n\tVALUES (@idPosts,@status, @type);", connection))
                {
                    command.Parameters.AddWithValue("idPosts", idPosts);
                    command.Parameters.AddWithValue("status", status);
                    command.Parameters.AddWithValue("type", type);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }

        public List<Аccount> GetАccount()
        {
            var list = new List<Аccount>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM \"Аccount\";", connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            list.Add(
                                new Аccount
                                {
                                    TypeAcc = (string)reader["TypeAcc"],
                                    UserName = (string)reader["UserName"],
                                    Password = (string)reader["Password"],
                                }
                                );
                        }
                    }
                }
            }

            return list;
        }


        public void QueryTelegramGroup(string nameTG)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO public.\"TelegramGroup\"(\r\n\t\"NameCodeGroup\")\r\n\tVALUES (@NameCodeGroup);", connection))
                {
                    command.Parameters.AddWithValue("NameCodeGroup", nameTG);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }

        public List<TelegramGroup> GetNameCodeGroup()
        {
            var list = new List<TelegramGroup>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM \"TelegramGroup\";", connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            list.Add(
                                new TelegramGroup
                                {
                                    NameCodeGroup = (string)reader["NameCodeGroup"],
                                }
                                );
                        }
                    }
                }
            }

            return list;
        }


        public void QueryInsertАccount(string typeAcc, string userName, string password)
        {

            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO public.\"Аccount\"(\r\n\t\"TypeAcc\", \"UserName\", \"Password\")\r\n\tVALUES (@TypeAcc,@UserName, @Password);", connection))
                {
                    command.Parameters.AddWithValue("TypeAcc", typeAcc);
                    command.Parameters.AddWithValue("UserName", userName);
                    command.Parameters.AddWithValue("Password", password);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }


        public void QueryInsertUser(string idUniq, string role, string token)
        {

            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO public.\"User\"(\r\n\t\"IdUniq\", \"Role\", \"Token\")\r\n\tVALUES (@idUniq,@role, @token);", connection))
                {
                    command.Parameters.AddWithValue("idUniq", idUniq);
                    command.Parameters.AddWithValue("role", role);
                    command.Parameters.AddWithValue("token", token);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }

        public List<InstaBotHelper.User> GetUsers()
        {
            var list = new List<InstaBotHelper.User>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM \"User\";", connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            list.Add(
                                new InstaBotHelper.User
                                {
                                    IdUniq = (string)reader["IdUniq"],
                                    Role = (string)reader["Role"],
                                    Token = (string)reader["Token"],
                                }
                                );
                        }
                    }
                }
            }

            return list;
        }


        public void QueryInsertDictionaryReplace(string regular, string replace)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO public.\"DictionaryReplace\"(\r\n\t\"Regular\", \"Replace\")\r\n\tVALUES (@regular,@replace);", connection))
                {
                    command.Parameters.AddWithValue("regular", regular);
                    command.Parameters.AddWithValue("replace", replace);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }

        public List<DictionaryReplace> GetDictionaryReplace()
        {
            var list = new List<DictionaryReplace>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM \"DictionaryReplace\";", connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            list.Add(
                                new DictionaryReplace
                                {
                                    Regular = (string)reader["Regular"],
                                    Replace = (string)reader["Replace"],
                                }
                                );
                        }
                    }
                }
            }

            return list;
        }

        public List<Posts> GetPosts()
        {
            var list = new List<Posts>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM \"Posts\";", connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            list.Add(
                                new Posts
                                {
                                    IdPosts = (long)reader["IdPosts"],
                                    Status = (string)reader["Status"],
                                    Type = (string)reader["Type"]
                                }
                                );
                        }
                    }
                }
            }

            return list;
        }

        public void QueryTruncate(string table)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(@$"TRUNCATE public.""{table}"" RESTART IDENTITY;", connection))
                {
                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
        }


    }


}
