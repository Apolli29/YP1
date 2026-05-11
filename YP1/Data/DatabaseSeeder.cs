using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace YP1.Data
{
    public class DatabaseSeeder
    {
        private readonly string _connectionString;

        public DatabaseSeeder()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["LibraryConnection"].ConnectionString;
        }

        public void SeedDemoData()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand checkCommand = new SqlCommand("SELECT COUNT(1) FROM dbo.Users;", connection))
                {
                    int usersCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (usersCount > 0)
                    {
                        return;
                    }
                }

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    int adminId = InsertUser(connection, transaction, "Дарья Климова", "admin", "admin@books.local", "administrator", false, null, "admin123");
                    int authorId = InsertUser(connection, transaction, "Илья Ветров", "author1", "author1@books.local", "author", false, null, "author123");
                    int secondAuthorId = InsertUser(connection, transaction, "Марина Орлова", "author2", "author2@books.local", "author", false, null, "author123");
                    int readerId = InsertUser(connection, transaction, "Анна Смирнова", "reader1", "reader1@books.local", "reader", false, null, "reader123");
                    int frozenUserId = InsertUser(connection, transaction, "Кирилл Левин", "frozen1", "frozen1@books.local", "reader", true, "слишком много жалоб на токсичное поведение в отзывах", "frozen123");
                    int futureAuthorId = InsertUser(connection, transaction, "Олеся Белова", "futureauthor", "futureauthor@books.local", "reader", false, null, "author123");

                    Dictionary<string, int> genres = LoadGenres(connection, transaction);

                    int bookOneId = InsertBook(
                        connection,
                        transaction,
                        "Туманный берег",
                        "Приключенческое фэнтези о девушке, которая нашла на старом берегу карту исчезнувшего города.",
                        GetLongText("Лена возвращалась к морю каждый вечер. Ей казалось, что в шуме волн кто-то снова и снова зовёт её по имени. Когда на мокром песке появилась старинная карта, обычное лето закончилось и началось путешествие в город, которого нет на современных картах."),
                        "#C56F57",
                        authorId,
                        false,
                        null);
                    AttachGenres(connection, transaction, bookOneId, genres["Фэнтези"], genres["Приключения"]);

                    int bookTwoId = InsertBook(
                        connection,
                        transaction,
                        "Ночное дело № 7",
                        "Детектив о пропавшем архиве, найденной записке и следователе, который не верит в случайности.",
                        GetLongText("Следователь Артём Серов не любил ночные дежурства, но именно в эту ночь он получил дело, которое тянулось ещё со студенческих лет. Чем дальше он разбирался в старых бумагах, тем яснее становилось: кто-то очень старается стереть одно имя из памяти целого города."),
                        "#516C8B",
                        secondAuthorId,
                        false,
                        null);
                    AttachGenres(connection, transaction, bookTwoId, genres["Детектив"], genres["Драма"]);

                    int bookThreeId = InsertBook(
                        connection,
                        transaction,
                        "Комната для черновиков",
                        "Роман о начинающем авторе, который записывает чужие истории и внезапно становится их участником.",
                        GetLongText("Максим снимал крошечную комнату с окном во двор и считал, что его жизни не хватит ни на один настоящий роман. Всё меняется, когда в подъезде начинает появляться блокнот без подписи, а каждая новая запись в нём неожиданно сбывается."),
                        "#8B5A72",
                        authorId,
                        true,
                        "в книге нашли слишком много оскорбительных фрагментов");
                    AttachGenres(connection, transaction, bookThreeId, genres["Роман"], genres["Драма"]);

                    int bookFourId = InsertBook(
                        connection,
                        transaction,
                        "Орбита тишины",
                        "Научно-фантастическая история о станции, которая потеряла связь с Землёй и начала жить по своим правилам.",
                        GetLongText("Когда связь с Землёй оборвалась, экипаж станции решил, что это временно. Через неделю стало ясно, что временно было прежнее спокойствие. Каждый из шести человек по-своему понимает, что значит ждать помощи, если ты уже не уверен, что её кто-то отправит."),
                        "#5A7860",
                        secondAuthorId,
                        false,
                        null);
                    AttachGenres(connection, transaction, bookFourId, genres["Научная фантастика"], genres["Драма"]);

                    int bookFiveId = InsertBook(
                        connection,
                        transaction,
                        "Тепло после письма",
                        "Небольшой роман о письмах, которые меняют сразу две жизни.",
                        GetLongText("Катя случайно получает письмо, адресованное женщине, которая давно уехала из города. Она решает ответить от её имени, просто чтобы не бросать чью-то боль без ответа. Но каждое следующее письмо делает эту ложь всё более настоящей."),
                        "#D39D54",
                        authorId,
                        false,
                        null);
                    AttachGenres(connection, transaction, bookFiveId, genres["Роман"], genres["Психология"]);

                    InsertReview(connection, transaction, bookOneId, readerId, 5, "Очень уютная история, особенно понравилась атмосфера у моря.", false, null);
                    InsertReview(connection, transaction, bookOneId, futureAuthorId, 4, "Хорошо написано, но финал показался немного поспешным.", false, null);
                    InsertReview(connection, transaction, bookTwoId, readerId, 4, "Люблю такие городские детективы, сюжет затянул с первых страниц.", false, null);
                    InsertReview(connection, transaction, bookThreeId, frozenUserId, 2, "Не мой формат, местами слишком резко и мрачно.", true, "отзыв заморозили за грубые формулировки");
                    InsertReview(connection, transaction, bookFourId, futureAuthorId, 5, "Сильная научная фантастика без перегруза терминами.", false, null);

                    UpsertBookList(connection, transaction, readerId, bookOneId, "reading");
                    UpsertBookList(connection, transaction, readerId, bookTwoId, "planned");
                    UpsertBookList(connection, transaction, readerId, bookFourId, "completed");
                    UpsertBookList(connection, transaction, futureAuthorId, bookFiveId, "abandoned");

                    InsertReport(connection, transaction, readerId, "book", bookThreeId, "в некоторых местах слишком резкая подача текста");
                    InsertReport(connection, transaction, futureAuthorId, "author", authorId, "автор грубо отвечал на отзывы и жалобы");
                    InsertReport(connection, transaction, authorId, "review", 4, "в отзыве были оскорбления и переход на личности");

                    InsertAuthorApplication(connection, transaction, futureAuthorId, "Пишу небольшие рассказы и хочу публиковать свои тексты в каталоге.");
                    InsertFreezeAppeal(connection, transaction, frozenUserId, "user", frozenUserId, "Понимаю, что сорвался в комментариях. Больше так не буду, прошу снять заморозку.");
                    InsertFreezeAppeal(connection, transaction, authorId, "book", bookThreeId, "Я уже поправил спорные фрагменты книги и готов отправить обновлённую версию.");

                    transaction.Commit();
                }
            }
        }

        private static int InsertUser(SqlConnection connection, SqlTransaction transaction, string fullName, string login, string email, string roleName, bool isFrozen, string freezeReason, string password)
        {
            const string sql = @"INSERT INTO dbo.Users (FullName, Login, Email, PasswordHash, RoleName, IsFrozen, FreezeReason)
                                 VALUES (@FullName, @Login, @Email, @PasswordHash, @RoleName, @IsFrozen, @FreezeReason);
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@FullName", fullName);
                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(password));
                command.Parameters.AddWithValue("@RoleName", roleName);
                command.Parameters.AddWithValue("@IsFrozen", isFrozen);
                command.Parameters.AddWithValue("@FreezeReason", (object)freezeReason ?? DBNull.Value);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static Dictionary<string, int> LoadGenres(SqlConnection connection, SqlTransaction transaction)
        {
            Dictionary<string, int> result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            using (SqlCommand command = new SqlCommand("SELECT GenreId, Name FROM dbo.Genres;", connection, transaction))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result[reader["Name"].ToString()] = Convert.ToInt32(reader["GenreId"]);
                }
            }

            return result;
        }

        private static int InsertBook(SqlConnection connection, SqlTransaction transaction, string title, string description, string bookText, string coverColor, int authorId, bool isFrozen, string freezeReason)
        {
            const string sql = @"INSERT INTO dbo.Books (Title, Description, BookText, CoverColor, AuthorId, IsFrozen, FreezeReason)
                                 VALUES (@Title, @Description, @BookText, @CoverColor, @AuthorId, @IsFrozen, @FreezeReason);
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@Description", description);
                command.Parameters.AddWithValue("@BookText", bookText);
                command.Parameters.AddWithValue("@CoverColor", coverColor);
                command.Parameters.AddWithValue("@AuthorId", authorId);
                command.Parameters.AddWithValue("@IsFrozen", isFrozen);
                command.Parameters.AddWithValue("@FreezeReason", (object)freezeReason ?? DBNull.Value);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static void AttachGenres(SqlConnection connection, SqlTransaction transaction, int bookId, params int[] genreIds)
        {
            foreach (int genreId in genreIds)
            {
                using (SqlCommand command = new SqlCommand("INSERT INTO dbo.BookGenres (BookId, GenreId) VALUES (@BookId, @GenreId);", connection, transaction))
                {
                    command.Parameters.AddWithValue("@BookId", bookId);
                    command.Parameters.AddWithValue("@GenreId", genreId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void InsertReview(SqlConnection connection, SqlTransaction transaction, int bookId, int userId, int rating, string text, bool isFrozen, string freezeReason)
        {
            const string sql = @"INSERT INTO dbo.Reviews (BookId, UserId, Rating, ReviewText, IsFrozen, FreezeReason)
                                 VALUES (@BookId, @UserId, @Rating, @ReviewText, @IsFrozen, @FreezeReason);";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@BookId", bookId);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Rating", rating);
                command.Parameters.AddWithValue("@ReviewText", text);
                command.Parameters.AddWithValue("@IsFrozen", isFrozen);
                command.Parameters.AddWithValue("@FreezeReason", (object)freezeReason ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        private static void UpsertBookList(SqlConnection connection, SqlTransaction transaction, int userId, int bookId, string listName)
        {
            const string sql = @"IF EXISTS (SELECT 1 FROM dbo.BookListItems WHERE UserId = @UserId AND BookId = @BookId)
                                 BEGIN
                                     UPDATE dbo.BookListItems SET ListName = @ListName, UpdatedAt = GETDATE()
                                     WHERE UserId = @UserId AND BookId = @BookId;
                                 END
                                 ELSE
                                 BEGIN
                                     INSERT INTO dbo.BookListItems (UserId, BookId, ListName)
                                     VALUES (@UserId, @BookId, @ListName);
                                 END";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@BookId", bookId);
                command.Parameters.AddWithValue("@ListName", listName);
                command.ExecuteNonQuery();
            }
        }

        private static void InsertReport(SqlConnection connection, SqlTransaction transaction, int reporterUserId, string targetType, int targetId, string reason)
        {
            const string sql = @"INSERT INTO dbo.Reports (ReporterUserId, TargetType, TargetId, Reason)
                                 VALUES (@ReporterUserId, @TargetType, @TargetId, @Reason);";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@ReporterUserId", reporterUserId);
                command.Parameters.AddWithValue("@TargetType", targetType);
                command.Parameters.AddWithValue("@TargetId", targetId);
                command.Parameters.AddWithValue("@Reason", reason);
                command.ExecuteNonQuery();
            }
        }

        private static void InsertAuthorApplication(SqlConnection connection, SqlTransaction transaction, int userId, string message)
        {
            using (SqlCommand command = new SqlCommand("INSERT INTO dbo.AuthorApplications (UserId, Message) VALUES (@UserId, @Message);", connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Message", message);
                command.ExecuteNonQuery();
            }
        }

        private static void InsertFreezeAppeal(SqlConnection connection, SqlTransaction transaction, int userId, string entityType, int entityId, string appealText)
        {
            const string sql = @"INSERT INTO dbo.FreezeAppeals (UserId, EntityType, EntityId, AppealText)
                                 VALUES (@UserId, @EntityType, @EntityId, @AppealText);";

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@EntityType", entityType);
                command.Parameters.AddWithValue("@EntityId", entityId);
                command.Parameters.AddWithValue("@AppealText", appealText);
                command.ExecuteNonQuery();
            }
        }

        private static string GetLongText(string baseText)
        {
            return baseText + Environment.NewLine + Environment.NewLine
                + "Во второй части истории герой сталкивается с последствиями своего выбора и вынужден пересмотреть отношение к людям рядом. "
                + "Текст написан спокойно, с упором на настроение, внутренние переживания и небольшие детали повседневности." + Environment.NewLine + Environment.NewLine
                + "Финальные главы подводят персонажей к развязке без резких скачков. Остаётся ощущение, что история закончилась честно и по-человечески, а не просто потому, что нужно было поставить точку.";
        }
    }
}
