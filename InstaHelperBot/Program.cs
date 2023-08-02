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
using System.Threading;

namespace TelegramBotExperiments
{

    public class Program
    {

        public ITelegramBotClient bot = new TelegramBotClient(token: Environment.GetEnvironmentVariable("token"));

        public string connString = "Host=db;Username=insta;Password=botinsat2003;Database=botinstanalis";

        public Task<InstaMediaList> InstaPostSource => GetInstaPost();

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

        public List<DictionaryReplace> DictionaryReplaceList => GetDictionaryReplace();
        public string nameProfilInstagram;

        private static bool isLoading = false;
        private int replaceF = 0;
        private DictionaryReplace _dictionaryReplace = new DictionaryReplace { };

        private long chatIdCh { get; set; }
        private bool setupTG = false;
        private Аccount account = new Аccount() { };

        private bool isNameChanal = false;
        private bool isAccaunt = false;

        private string NameAccaunt()
        {
            return account.TypeAcc ?? (АccountList().Count > 0 ? АccountList().First().TypeAcc : null) ?? "Не задан аккаунт инстаграма";
        }


        public async static Task Main(string[] args)
        {
            Program pr = new Program();

            Migration migration = new(pr.connString);

            pr.nameProfilInstagram = pr.АccountList().Count > 0 ? pr.АccountList().First().TypeAcc : string.Empty;

            Console.WriteLine("Запущен бот ");

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            await Task.Run(() => pr.RunBot());

            
            Console.ReadLine();

        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text is null) return;

                if (chatIdCh == 0) chatIdCh = message.Chat.Id;

                if (message.Text.ToLower() == "/start" || message.Text.ToLower() == "перезагрузка".ToLower())
                {
                    isLoading = false;
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
                                                                          new KeyboardButton[] { $"Канал 📸{NameAccaunt()}", "Логин/Пароль Инстаграмма" }})
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
                                                                          new KeyboardButton[] { $"Канал 📸{NameAccaunt()}", "Логин/Пароль Инстаграмма" }})
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

                        chatIdCh = Convert.ToInt64(message.Text);
                        await botClient.SendTextMessageAsync(message.Chat, "Успешно установили ID канала");
                        return;

                    }
                    if (message.Text.ToLower() == "Указать канал ТГ".ToLower())
                    {
                        QueryTruncate("TelegramGroup");
                        setupTG = true;
                        await botClient.SendTextMessageAsync(message.Chat, "Введите Id канала");
                        return;
                    }

                    if (message.Text.ToLower() == "Удалить канал ТГ".ToLower())
                    {
                        QueryTruncate("TelegramGroup");
                        chatIdCh = message.Chat.Id;
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

                        QueryTruncate("Аccount");
                        nameProfilInstagram = account.TypeAcc;

                        if (account.TypeAcc != null && account.UserName != null && account.Password != null)
                        {
                            QueryInsertАccount(account.TypeAcc, account.UserName, account.Password);

                            try
                            {
                                _login = null;
                                var Islogin = LoginApi.Result.UserProcessor.GetUserMediaAsync(nameProfilInstagram, PaginationParameters.MaxPagesToLoad(1)).Result.Succeeded;
                                if (!Islogin)
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

                            try
                            {
                                _login = null;
                                var Islogin = LoginApi.Result.UserProcessor.GetUserMediaAsync(nameProfilInstagram, PaginationParameters.MaxPagesToLoad(1)).Result.Succeeded;
                                if (!Islogin)
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


        public async Task<InstaMediaList> GetInstaPost()
        {
            var userResult = await LoginApi.Result.UserProcessor.GetUserInfoByUsernameAsync(nameProfilInstagram);

            Console.WriteLine("Подключились к каналу");

            var user = userResult.Value.Username;
            var mediaResult = await LoginApi.Result.UserProcessor.GetUserMediaAsync(user, PaginationParameters.Empty);
            return mediaResult.Value;
        }

        async Task<List<InstaMedia>> GetLatestUserPosts(string username)
        {
            var userPosts = await LoginApi.Result.UserProcessor.GetUserMediaAsync(username, PaginationParameters.MaxPagesToLoad(1));

            return userPosts.Value;
        }

        public async Task RunBot()
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            if (TelegramGroupList.Count != 0)
                chatIdCh = Convert.ToInt64(TelegramGroupList.First().NameCodeGroup.ToString());

            

            var count = 0;

             var timer = new Timer(async _ =>
            {
                try
                {
                    Console.WriteLine($"{isLoading} {АccountList().Count}");
                    if (isLoading && АccountList().Count != 0)
                    {
                        var latestPosts = LoginApi.Result.UserProcessor.GetUserMediaAsync(nameProfilInstagram, PaginationParameters.Empty);
                        foreach (var item in latestPosts.Result.Value.OrderBy(x => x.TakenAt).ToList())
                        {
                            if (!Posts.Select(x => x.IdPosts.ToString()).ToList().Contains(item.Pk))
                            {
                                SentMessagePostInBot(item, bot, chatIdCh, cancellationToken);
                                QueryInsertPost(Convert.ToInt64(item.Pk), "true", item.ProductType);
                                Console.WriteLine(count++);
                                Thread.Sleep(10000);
                            }
                        }


                        var userResult = LoginApi.Result.UserProcessor.GetUserAsync(nameProfilInstagram);
                        try
                        {
                            if (userResult.Result.Value != null)
                            {
                                var storyResult = LoginApi.Result.StoryProcessor.GetUserStoryFeedAsync(userResult.Result.Value.Pk);
                                if (!storyResult.Result.Succeeded)
                                {
                                    Console.WriteLine($"Ошибка получения сторис пользователя: {storyResult.Result.Info.Message}");
                                }
                                else
                                {
                                    foreach (var item in storyResult.Result.Value.Items)
                                    {
                                        if (!Posts.Select(x => x.IdPosts.ToString()).ToList().Contains(item.Pk.ToString()))
                                        {
                                            try
                                            {
                                                SentMessagePostInBot(item, bot, chatIdCh, cancellationToken);
                                                QueryInsertPost(Convert.ToInt64(item.Pk), "true", "storis");
                                                Console.WriteLine(count++);
                                            }
                                            catch
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }


                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                }

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(300));

            await bot.ReceiveAsync(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

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
