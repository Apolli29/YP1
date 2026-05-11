using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using YP1.Models;

namespace YP1.Data
{
    public class LibraryRepository
    {
        private readonly string _connectionString;

        public LibraryRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["LibraryConnection"].ConnectionString;
        }

        public UserModel Authenticate(string login, string password)
        {
            const string sql = @"SELECT UserId, FullName, Login, Email, RoleName, IsFrozen, FreezeReason
                                 FROM dbo.Users
                                 WHERE Login = @Login AND PasswordHash = @PasswordHash;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(password));
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapUser(reader);
                    }
                }
            }

            return null;
        }

        public bool RegisterUser(string fullName, string login, string email, string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Нужно заполнить все поля регистрации.";
                return false;
            }

            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                if (Exists(connection, "SELECT COUNT(1) FROM dbo.Users WHERE Login = @Value;", login))
                {
                    errorMessage = "Пользователь с таким логином уже существует.";
                    return false;
                }

                if (Exists(connection, "SELECT COUNT(1) FROM dbo.Users WHERE Email = @Value;", email))
                {
                    errorMessage = "Пользователь с такой почтой уже существует.";
                    return false;
                }

                const string sql = @"INSERT INTO dbo.Users (FullName, Login, Email, PasswordHash, RoleName, IsFrozen)
                                     VALUES (@FullName, @Login, @Email, @PasswordHash, N'reader', 0);";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@FullName", fullName.Trim());
                    command.Parameters.AddWithValue("@Login", login.Trim());
                    command.Parameters.AddWithValue("@Email", email.Trim());
                    command.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(password));
                    command.ExecuteNonQuery();
                }
            }

            return true;
        }

        public UserModel GetUserById(int userId)
        {
            const string sql = @"SELECT UserId, FullName, Login, Email, RoleName, IsFrozen, FreezeReason
                                 FROM dbo.Users
                                 WHERE UserId = @UserId;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapUser(reader);
                    }
                }
            }

            return null;
        }

        public List<GenreModel> GetGenres()
        {
            List<GenreModel> result = new List<GenreModel>();

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT GenreId, Name FROM dbo.Genres ORDER BY Name;", connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GenreModel genre = new GenreModel();
                        genre.GenreId = Convert.ToInt32(reader["GenreId"]);
                        genre.Name = reader["Name"].ToString();
                        result.Add(genre);
                    }
                }
            }

            return result;
        }

        public List<BookModel> GetCatalogBooks(string searchText, string sortMode, int? genreId)
        {
            string sql = @"SELECT b.BookId, b.Title, b.Description, b.BookText, b.CoverColor, b.AuthorId,
                                  u.FullName AS AuthorName, b.IsFrozen, b.FreezeReason,
                                  ISNULL((SELECT AVG(CAST(r.Rating AS DECIMAL(10,2))) FROM dbo.Reviews r WHERE r.BookId = b.BookId AND r.IsFrozen = 0), 0) AS AverageRating
                           FROM dbo.Books b
                           INNER JOIN dbo.Users u ON u.UserId = b.AuthorId
                           WHERE 1 = 1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                sql += " AND (b.Title LIKE @Search OR u.FullName LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + searchText.Trim() + "%"));
            }

            if (genreId.HasValue)
            {
                sql += " AND EXISTS (SELECT 1 FROM dbo.BookGenres bg WHERE bg.BookId = b.BookId AND bg.GenreId = @GenreId)";
                parameters.Add(new SqlParameter("@GenreId", genreId.Value));
            }

            sql += BuildBookOrderBy(sortMode);

            return ReadBooks(sql, parameters);
        }

        public BookModel GetBookById(int bookId)
        {
            const string sql = @"SELECT b.BookId, b.Title, b.Description, b.BookText, b.CoverColor, b.AuthorId,
                                        u.FullName AS AuthorName, b.IsFrozen, b.FreezeReason,
                                        ISNULL((SELECT AVG(CAST(r.Rating AS DECIMAL(10,2))) FROM dbo.Reviews r WHERE r.BookId = b.BookId AND r.IsFrozen = 0), 0) AS AverageRating
                                 FROM dbo.Books b
                                 INNER JOIN dbo.Users u ON u.UserId = b.AuthorId
                                 WHERE b.BookId = @BookId;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@BookId", bookId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        BookModel book = MapBook(reader);
                        FillBookGenres(book);
                        return book;
                    }
                }
            }

            return null;
        }

        public List<ReviewModel> GetReviewsByBook(int bookId)
        {
            List<ReviewModel> result = new List<ReviewModel>();
            const string sql = @"SELECT r.ReviewId, r.BookId, r.UserId, u.FullName AS UserName, r.Rating, r.ReviewText,
                                        r.IsFrozen, r.FreezeReason, r.CreatedAt, b.Title AS BookTitle
                                 FROM dbo.Reviews r
                                 INNER JOIN dbo.Users u ON u.UserId = r.UserId
                                 INNER JOIN dbo.Books b ON b.BookId = r.BookId
                                 WHERE r.BookId = @BookId
                                 ORDER BY r.CreatedAt DESC;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@BookId", bookId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MapReview(reader));
                    }
                }
            }

            return result;
        }

        public void AddBookToList(int userId, int bookId, string listName)
        {
            const string sql = @"IF EXISTS (SELECT 1 FROM dbo.BookListItems WHERE UserId = @UserId AND BookId = @BookId)
                                 BEGIN
                                     UPDATE dbo.BookListItems
                                     SET ListName = @ListName, UpdatedAt = GETDATE()
                                     WHERE UserId = @UserId AND BookId = @BookId;
                                 END
                                 ELSE
                                 BEGIN
                                     INSERT INTO dbo.BookListItems (UserId, BookId, ListName)
                                     VALUES (@UserId, @BookId, @ListName);
                                 END";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@BookId", bookId);
                command.Parameters.AddWithValue("@ListName", listName);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<BookModel> GetBooksFromUserList(int userId, string listName, string searchText, string sortMode, int? genreId)
        {
            string sql = @"SELECT b.BookId, b.Title, b.Description, b.BookText, b.CoverColor, b.AuthorId,
                                  u.FullName AS AuthorName, b.IsFrozen, b.FreezeReason, bli.ListName,
                                  ISNULL((SELECT AVG(CAST(r.Rating AS DECIMAL(10,2))) FROM dbo.Reviews r WHERE r.BookId = b.BookId AND r.IsFrozen = 0), 0) AS AverageRating
                           FROM dbo.BookListItems bli
                           INNER JOIN dbo.Books b ON b.BookId = bli.BookId
                           INNER JOIN dbo.Users u ON u.UserId = b.AuthorId
                           WHERE bli.UserId = @UserId AND bli.ListName = @ListName";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@UserId", userId));
            parameters.Add(new SqlParameter("@ListName", listName));

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                sql += " AND (b.Title LIKE @Search OR u.FullName LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + searchText.Trim() + "%"));
            }

            if (genreId.HasValue)
            {
                sql += " AND EXISTS (SELECT 1 FROM dbo.BookGenres bg WHERE bg.BookId = b.BookId AND bg.GenreId = @GenreId)";
                parameters.Add(new SqlParameter("@GenreId", genreId.Value));
            }

            sql += BuildBookOrderBy(sortMode);

            return ReadBooks(sql, parameters);
        }

        public void SaveReview(int userId, int bookId, int rating, string reviewText)
        {
            const string sql = @"IF EXISTS (SELECT 1 FROM dbo.Reviews WHERE UserId = @UserId AND BookId = @BookId)
                                 BEGIN
                                     UPDATE dbo.Reviews
                                     SET Rating = @Rating, ReviewText = @ReviewText, CreatedAt = GETDATE(), IsFrozen = 0, FreezeReason = NULL
                                     WHERE UserId = @UserId AND BookId = @BookId;
                                 END
                                 ELSE
                                 BEGIN
                                     INSERT INTO dbo.Reviews (UserId, BookId, Rating, ReviewText)
                                     VALUES (@UserId, @BookId, @Rating, @ReviewText);
                                 END";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@BookId", bookId);
                command.Parameters.AddWithValue("@Rating", rating);
                command.Parameters.AddWithValue("@ReviewText", reviewText);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SaveReport(int userId, string targetType, int targetId, string reason)
        {
            const string sql = @"INSERT INTO dbo.Reports (ReporterUserId, TargetType, TargetId, Reason)
                                 VALUES (@ReporterUserId, @TargetType, @TargetId, @Reason);";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ReporterUserId", userId);
                command.Parameters.AddWithValue("@TargetType", targetType);
                command.Parameters.AddWithValue("@TargetId", targetId);
                command.Parameters.AddWithValue("@Reason", reason);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<ReviewModel> GetUserReviews(int userId)
        {
            List<ReviewModel> result = new List<ReviewModel>();
            const string sql = @"SELECT r.ReviewId, r.BookId, b.Title AS BookTitle, r.UserId, u.FullName AS UserName, r.Rating, r.ReviewText,
                                        r.IsFrozen, r.FreezeReason, r.CreatedAt
                                 FROM dbo.Reviews r
                                 INNER JOIN dbo.Books b ON b.BookId = r.BookId
                                 INNER JOIN dbo.Users u ON u.UserId = r.UserId
                                 WHERE r.UserId = @UserId
                                 ORDER BY r.CreatedAt DESC;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MapReview(reader));
                    }
                }
            }

            return result;
        }

        public bool HasPendingAuthorApplication(int userId)
        {
            return ExistsScalar("SELECT COUNT(1) FROM dbo.AuthorApplications WHERE UserId = @UserId AND Status = N'pending';", new SqlParameter("@UserId", userId));
        }

        public void CreateAuthorApplication(int userId, string message)
        {
            const string sql = @"INSERT INTO dbo.AuthorApplications (UserId, Message)
                                 VALUES (@UserId, @Message);";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Message", message);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool HasPendingFreezeAppeal(int userId, string entityType, int entityId)
        {
            return ExistsScalar(
                "SELECT COUNT(1) FROM dbo.FreezeAppeals WHERE UserId = @UserId AND EntityType = @EntityType AND EntityId = @EntityId AND Status = N'pending';",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@EntityType", entityType),
                new SqlParameter("@EntityId", entityId));
        }

        public void CreateFreezeAppeal(int userId, string entityType, int entityId, string appealText)
        {
            const string sql = @"INSERT INTO dbo.FreezeAppeals (UserId, EntityType, EntityId, AppealText)
                                 VALUES (@UserId, @EntityType, @EntityId, @AppealText);";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@EntityType", entityType);
                command.Parameters.AddWithValue("@EntityId", entityId);
                command.Parameters.AddWithValue("@AppealText", appealText);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<BookModel> GetAuthorBooks(int authorId, bool onlyFrozen)
        {
            string sql = @"SELECT b.BookId, b.Title, b.Description, b.BookText, b.CoverColor, b.AuthorId,
                                  u.FullName AS AuthorName, b.IsFrozen, b.FreezeReason,
                                  ISNULL((SELECT AVG(CAST(r.Rating AS DECIMAL(10,2))) FROM dbo.Reviews r WHERE r.BookId = b.BookId AND r.IsFrozen = 0), 0) AS AverageRating
                           FROM dbo.Books b
                           INNER JOIN dbo.Users u ON u.UserId = b.AuthorId
                           WHERE b.AuthorId = @AuthorId";

            if (onlyFrozen)
            {
                sql += " AND b.IsFrozen = 1";
            }

            sql += " ORDER BY b.Title;";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@AuthorId", authorId));

            return ReadBooks(sql, parameters);
        }

        public void SaveBook(BookModel book)
        {
            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    if (book.BookId == 0)
                    {
                        const string insertSql = @"INSERT INTO dbo.Books (Title, Description, BookText, CoverColor, AuthorId)
                                                   VALUES (@Title, @Description, @BookText, @CoverColor, @AuthorId);
                                                   SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (SqlCommand insertCommand = new SqlCommand(insertSql, connection, transaction))
                        {
                            FillBookCommandParameters(insertCommand, book);
                            book.BookId = Convert.ToInt32(insertCommand.ExecuteScalar());
                        }
                    }
                    else
                    {
                        const string updateSql = @"UPDATE dbo.Books
                                                   SET Title = @Title,
                                                       Description = @Description,
                                                       BookText = @BookText,
                                                       CoverColor = @CoverColor
                                                   WHERE BookId = @BookId;";

                        using (SqlCommand updateCommand = new SqlCommand(updateSql, connection, transaction))
                        {
                            FillBookCommandParameters(updateCommand, book);
                            updateCommand.Parameters.AddWithValue("@BookId", book.BookId);
                            updateCommand.ExecuteNonQuery();
                        }
                    }

                    using (SqlCommand deleteCommand = new SqlCommand("DELETE FROM dbo.BookGenres WHERE BookId = @BookId;", connection, transaction))
                    {
                        deleteCommand.Parameters.AddWithValue("@BookId", book.BookId);
                        deleteCommand.ExecuteNonQuery();
                    }

                    foreach (int genreId in book.GenreIds)
                    {
                        using (SqlCommand insertGenreCommand = new SqlCommand("INSERT INTO dbo.BookGenres (BookId, GenreId) VALUES (@BookId, @GenreId);", connection, transaction))
                        {
                            insertGenreCommand.Parameters.AddWithValue("@BookId", book.BookId);
                            insertGenreCommand.Parameters.AddWithValue("@GenreId", genreId);
                            insertGenreCommand.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        public List<ReportModel> GetReports()
        {
            List<ReportModel> result = new List<ReportModel>();
            const string sql = @"SELECT r.ReportId, r.ReporterUserId, u.FullName AS ReporterName, r.TargetType, r.TargetId, r.Reason, r.Status, r.CreatedAt
                                 FROM dbo.Reports r
                                 INNER JOIN dbo.Users u ON u.UserId = r.ReporterUserId
                                 ORDER BY CASE WHEN r.Status = N'pending' THEN 0 ELSE 1 END, r.CreatedAt DESC;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ReportModel report = new ReportModel();
                        report.ReportId = Convert.ToInt32(reader["ReportId"]);
                        report.ReporterUserId = Convert.ToInt32(reader["ReporterUserId"]);
                        report.ReporterName = reader["ReporterName"].ToString();
                        report.TargetType = reader["TargetType"].ToString();
                        report.TargetId = Convert.ToInt32(reader["TargetId"]);
                        report.Reason = reader["Reason"].ToString();
                        report.Status = reader["Status"].ToString();
                        report.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                        report.TargetName = GetEntityName(report.TargetType, report.TargetId);
                        result.Add(report);
                    }
                }
            }

            return result;
        }

        public void ResolveReport(int reportId, bool approve)
        {
            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    string targetType = string.Empty;
                    int targetId = 0;
                    string reason = string.Empty;

                    using (SqlCommand loadCommand = new SqlCommand("SELECT TargetType, TargetId, Reason FROM dbo.Reports WHERE ReportId = @ReportId;", connection, transaction))
                    {
                        loadCommand.Parameters.AddWithValue("@ReportId", reportId);

                        using (SqlDataReader reader = loadCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                targetType = reader["TargetType"].ToString();
                                targetId = Convert.ToInt32(reader["TargetId"]);
                                reason = reader["Reason"].ToString();
                            }
                        }
                    }

                    if (approve)
                    {
                        ApplyFreeze(connection, transaction, targetType, targetId, reason);
                    }

                    using (SqlCommand updateCommand = new SqlCommand("UPDATE dbo.Reports SET Status = @Status WHERE ReportId = @ReportId;", connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@Status", approve ? "accepted" : "rejected");
                        updateCommand.Parameters.AddWithValue("@ReportId", reportId);
                        updateCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public List<FreezeAppealModel> GetFreezeAppeals()
        {
            List<FreezeAppealModel> result = new List<FreezeAppealModel>();
            const string sql = @"SELECT a.AppealId, a.UserId, u.FullName AS UserName, a.EntityType, a.EntityId, a.AppealText, a.Status, a.CreatedAt
                                 FROM dbo.FreezeAppeals a
                                 INNER JOIN dbo.Users u ON u.UserId = a.UserId
                                 ORDER BY CASE WHEN a.Status = N'pending' THEN 0 ELSE 1 END, a.CreatedAt DESC;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FreezeAppealModel appeal = new FreezeAppealModel();
                        appeal.AppealId = Convert.ToInt32(reader["AppealId"]);
                        appeal.UserId = Convert.ToInt32(reader["UserId"]);
                        appeal.UserName = reader["UserName"].ToString();
                        appeal.EntityType = reader["EntityType"].ToString();
                        appeal.EntityId = Convert.ToInt32(reader["EntityId"]);
                        appeal.AppealText = reader["AppealText"].ToString();
                        appeal.Status = reader["Status"].ToString();
                        appeal.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                        appeal.EntityName = GetEntityName(appeal.EntityType, appeal.EntityId);
                        result.Add(appeal);
                    }
                }
            }

            return result;
        }

        public void ResolveFreezeAppeal(int appealId, bool approve)
        {
            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    string entityType = string.Empty;
                    int entityId = 0;

                    using (SqlCommand loadCommand = new SqlCommand("SELECT EntityType, EntityId FROM dbo.FreezeAppeals WHERE AppealId = @AppealId;", connection, transaction))
                    {
                        loadCommand.Parameters.AddWithValue("@AppealId", appealId);

                        using (SqlDataReader reader = loadCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                entityType = reader["EntityType"].ToString();
                                entityId = Convert.ToInt32(reader["EntityId"]);
                            }
                        }
                    }

                    if (approve)
                    {
                        ApplyUnfreeze(connection, transaction, entityType, entityId);
                    }

                    using (SqlCommand updateCommand = new SqlCommand("UPDATE dbo.FreezeAppeals SET Status = @Status, ReviewedAt = GETDATE() WHERE AppealId = @AppealId;", connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@Status", approve ? "approved" : "rejected");
                        updateCommand.Parameters.AddWithValue("@AppealId", appealId);
                        updateCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public List<AuthorApplicationModel> GetAuthorApplications()
        {
            List<AuthorApplicationModel> result = new List<AuthorApplicationModel>();
            const string sql = @"SELECT a.ApplicationId, a.UserId, u.FullName AS UserName, u.Login, a.Message, a.Status, a.CreatedAt
                                 FROM dbo.AuthorApplications a
                                 INNER JOIN dbo.Users u ON u.UserId = a.UserId
                                 ORDER BY CASE WHEN a.Status = N'pending' THEN 0 ELSE 1 END, a.CreatedAt DESC;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AuthorApplicationModel application = new AuthorApplicationModel();
                        application.ApplicationId = Convert.ToInt32(reader["ApplicationId"]);
                        application.UserId = Convert.ToInt32(reader["UserId"]);
                        application.UserName = reader["UserName"].ToString();
                        application.Login = reader["Login"].ToString();
                        application.Message = reader["Message"].ToString();
                        application.Status = reader["Status"].ToString();
                        application.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                        result.Add(application);
                    }
                }
            }

            return result;
        }

        public void ResolveAuthorApplication(int applicationId, bool approve)
        {
            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    int userId = 0;

                    using (SqlCommand loadCommand = new SqlCommand("SELECT UserId FROM dbo.AuthorApplications WHERE ApplicationId = @ApplicationId;", connection, transaction))
                    {
                        loadCommand.Parameters.AddWithValue("@ApplicationId", applicationId);
                        object result = loadCommand.ExecuteScalar();

                        if (result != null)
                        {
                            userId = Convert.ToInt32(result);
                        }
                    }

                    if (approve && userId > 0)
                    {
                        using (SqlCommand roleCommand = new SqlCommand("UPDATE dbo.Users SET RoleName = N'author' WHERE UserId = @UserId;", connection, transaction))
                        {
                            roleCommand.Parameters.AddWithValue("@UserId", userId);
                            roleCommand.ExecuteNonQuery();
                        }
                    }

                    using (SqlCommand updateCommand = new SqlCommand("UPDATE dbo.AuthorApplications SET Status = @Status, ReviewedAt = GETDATE() WHERE ApplicationId = @ApplicationId;", connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@Status", approve ? "approved" : "rejected");
                        updateCommand.Parameters.AddWithValue("@ApplicationId", applicationId);
                        updateCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public List<FrozenItemModel> GetFrozenItems()
        {
            List<FrozenItemModel> result = new List<FrozenItemModel>();
            const string sql = @"SELECT N'user' AS EntityType, u.UserId AS EntityId, u.FullName AS Title, u.Login AS OwnerName, u.FreezeReason
                                 FROM dbo.Users u
                                 WHERE u.IsFrozen = 1
                                 UNION ALL
                                 SELECT N'book' AS EntityType, b.BookId AS EntityId, b.Title AS Title, u.FullName AS OwnerName, b.FreezeReason
                                 FROM dbo.Books b
                                 INNER JOIN dbo.Users u ON u.UserId = b.AuthorId
                                 WHERE b.IsFrozen = 1
                                 UNION ALL
                                 SELECT N'review' AS EntityType, r.ReviewId AS EntityId, LEFT(r.ReviewText, 60) AS Title, u.FullName AS OwnerName, r.FreezeReason
                                 FROM dbo.Reviews r
                                 INNER JOIN dbo.Users u ON u.UserId = r.UserId
                                 WHERE r.IsFrozen = 1;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FrozenItemModel item = new FrozenItemModel();
                        item.EntityType = reader["EntityType"].ToString();
                        item.EntityId = Convert.ToInt32(reader["EntityId"]);
                        item.Title = reader["Title"].ToString();
                        item.OwnerName = reader["OwnerName"].ToString();
                        item.FreezeReason = ReadNullableString(reader, "FreezeReason");
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public List<UserModel> GetUsers()
        {
            List<UserModel> result = new List<UserModel>();

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT UserId, FullName, Login, Email, RoleName, IsFrozen, FreezeReason FROM dbo.Users ORDER BY FullName;", connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MapUser(reader));
                    }
                }
            }

            return result;
        }

        public void UpdateUserRole(int userId, string roleName)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand("UPDATE dbo.Users SET RoleName = @RoleName WHERE UserId = @UserId;", connection))
            {
                command.Parameters.AddWithValue("@RoleName", roleName);
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void ChangePassword(int userId, string newPassword)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand("UPDATE dbo.Users SET PasswordHash = @PasswordHash WHERE UserId = @UserId;", connection))
            {
                command.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(newPassword));
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void FreezeBook(int bookId, string reason)
        {
            UpdateFreezeState("UPDATE dbo.Books SET IsFrozen = 1, FreezeReason = @Reason WHERE BookId = @EntityId;", bookId, reason);
        }

        public void FreezeReview(int reviewId, string reason)
        {
            UpdateFreezeState("UPDATE dbo.Reviews SET IsFrozen = 1, FreezeReason = @Reason WHERE ReviewId = @EntityId;", reviewId, reason);
        }

        public void FreezeUser(int userId, string reason)
        {
            UpdateFreezeState("UPDATE dbo.Users SET IsFrozen = 1, FreezeReason = @Reason WHERE UserId = @EntityId;", userId, reason);
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private static UserModel MapUser(IDataRecord record)
        {
            UserModel user = new UserModel();
            user.UserId = Convert.ToInt32(record["UserId"]);
            user.FullName = record["FullName"].ToString();
            user.Login = record["Login"].ToString();
            user.Email = record["Email"].ToString();
            user.RoleName = record["RoleName"].ToString();
            user.IsFrozen = Convert.ToBoolean(record["IsFrozen"]);
            user.FreezeReason = ReadNullableString(record, "FreezeReason");
            return user;
        }

        private static BookModel MapBook(IDataRecord record)
        {
            BookModel book = new BookModel();
            book.BookId = Convert.ToInt32(record["BookId"]);
            book.Title = record["Title"].ToString();
            book.Description = record["Description"].ToString();
            book.BookText = record["BookText"].ToString();
            book.CoverColor = record["CoverColor"].ToString();
            book.AuthorId = Convert.ToInt32(record["AuthorId"]);
            book.AuthorName = record["AuthorName"].ToString();
            book.IsFrozen = Convert.ToBoolean(record["IsFrozen"]);
            book.FreezeReason = ReadNullableString(record, "FreezeReason");
            book.AverageRating = Convert.ToDecimal(record["AverageRating"]);

            if (HasColumn(record, "ListName"))
            {
                book.ListName = ReadNullableString(record, "ListName");
            }

            return book;
        }

        private static ReviewModel MapReview(IDataRecord record)
        {
            ReviewModel review = new ReviewModel();
            review.ReviewId = Convert.ToInt32(record["ReviewId"]);
            review.BookId = Convert.ToInt32(record["BookId"]);
            review.BookTitle = HasColumn(record, "BookTitle") ? ReadNullableString(record, "BookTitle") : string.Empty;
            review.UserId = Convert.ToInt32(record["UserId"]);
            review.UserName = record["UserName"].ToString();
            review.Rating = Convert.ToInt32(record["Rating"]);
            review.ReviewText = record["ReviewText"].ToString();
            review.IsFrozen = Convert.ToBoolean(record["IsFrozen"]);
            review.FreezeReason = ReadNullableString(record, "FreezeReason");
            review.CreatedAt = Convert.ToDateTime(record["CreatedAt"]);
            return review;
        }

        private static string ReadNullableString(IDataRecord record, string columnName)
        {
            int ordinal = record.GetOrdinal(columnName);

            if (record.IsDBNull(ordinal))
            {
                return string.Empty;
            }

            return record.GetString(ordinal);
        }

        private static bool HasColumn(IDataRecord record, string columnName)
        {
            for (int index = 0; index < record.FieldCount; index++)
            {
                if (record.GetName(index) == columnName)
                {
                    return true;
                }
            }

            return false;
        }

        private List<BookModel> ReadBooks(string sql, List<SqlParameter> parameters)
        {
            List<BookModel> result = new List<BookModel>();

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                foreach (SqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MapBook(reader));
                    }
                }
            }

            foreach (BookModel book in result)
            {
                FillBookGenres(book);
            }

            return result;
        }

        private void FillBookGenres(BookModel book)
        {
            book.GenreIds.Clear();
            List<string> genreNames = new List<string>();

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(@"SELECT g.GenreId, g.Name
                                                         FROM dbo.BookGenres bg
                                                         INNER JOIN dbo.Genres g ON g.GenreId = bg.GenreId
                                                         WHERE bg.BookId = @BookId
                                                         ORDER BY g.Name;", connection))
            {
                command.Parameters.AddWithValue("@BookId", book.BookId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        book.GenreIds.Add(Convert.ToInt32(reader["GenreId"]));
                        genreNames.Add(reader["Name"].ToString());
                    }
                }
            }

            book.GenresText = genreNames.Count == 0 ? "без жанра" : string.Join(", ", genreNames.ToArray());
        }

        private static string BuildBookOrderBy(string sortMode)
        {
            if (sortMode == "rating")
            {
                return " ORDER BY AverageRating DESC, b.Title ASC";
            }

            return " ORDER BY b.Title ASC";
        }

        private void FillBookCommandParameters(SqlCommand command, BookModel book)
        {
            command.Parameters.AddWithValue("@Title", book.Title);
            command.Parameters.AddWithValue("@Description", book.Description);
            command.Parameters.AddWithValue("@BookText", book.BookText);
            command.Parameters.AddWithValue("@CoverColor", string.IsNullOrWhiteSpace(book.CoverColor) ? "#5E6C84" : book.CoverColor);
            command.Parameters.AddWithValue("@AuthorId", book.AuthorId);
        }

        private bool Exists(SqlConnection connection, string sql, string value)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Value", value);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private bool ExistsScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                foreach (SqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private string GetEntityName(string entityType, int entityId)
        {
            string sql;

            switch (entityType)
            {
                case "book":
                    sql = "SELECT Title FROM dbo.Books WHERE BookId = @EntityId;";
                    break;
                case "review":
                    sql = "SELECT LEFT(ReviewText, 60) FROM dbo.Reviews WHERE ReviewId = @EntityId;";
                    break;
                case "author":
                case "user":
                    sql = "SELECT FullName FROM dbo.Users WHERE UserId = @EntityId;";
                    break;
                default:
                    return "неизвестная сущность";
            }

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@EntityId", entityId);
                connection.Open();
                object result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    return "удалённый объект";
                }

                return result.ToString();
            }
        }

        private void ApplyFreeze(SqlConnection connection, SqlTransaction transaction, string entityType, int entityId, string reason)
        {
            string sql = string.Empty;

            if (entityType == "book")
            {
                sql = "UPDATE dbo.Books SET IsFrozen = 1, FreezeReason = @Reason WHERE BookId = @EntityId;";
            }
            else if (entityType == "review")
            {
                sql = "UPDATE dbo.Reviews SET IsFrozen = 1, FreezeReason = @Reason WHERE ReviewId = @EntityId;";
            }
            else if (entityType == "author" || entityType == "user")
            {
                sql = "UPDATE dbo.Users SET IsFrozen = 1, FreezeReason = @Reason WHERE UserId = @EntityId;";
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@Reason", reason);
                command.Parameters.AddWithValue("@EntityId", entityId);
                command.ExecuteNonQuery();
            }
        }

        private void ApplyUnfreeze(SqlConnection connection, SqlTransaction transaction, string entityType, int entityId)
        {
            string sql = string.Empty;

            if (entityType == "book")
            {
                sql = "UPDATE dbo.Books SET IsFrozen = 0, FreezeReason = NULL WHERE BookId = @EntityId;";
            }
            else if (entityType == "review")
            {
                sql = "UPDATE dbo.Reviews SET IsFrozen = 0, FreezeReason = NULL WHERE ReviewId = @EntityId;";
            }
            else if (entityType == "author" || entityType == "user")
            {
                sql = "UPDATE dbo.Users SET IsFrozen = 0, FreezeReason = NULL WHERE UserId = @EntityId;";
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@EntityId", entityId);
                command.ExecuteNonQuery();
            }
        }

        private void UpdateFreezeState(string sql, int entityId, string reason)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Reason", reason);
                command.Parameters.AddWithValue("@EntityId", entityId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
