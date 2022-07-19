using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Bookmaster3000.Utils;

namespace Bookmaster3000
{
	public class Author
	{
		public string Key { get; set; }
		public string Name { get; set; }
		public string Bio { get; set; }
		public string BirthDate { get; set; }
		public string DeathDate { get; set; }
		public string Wikipedia { get; set; }

		public override string ToString() => Name;
	}

	public class Book<TAuthor> where TAuthor : Author, new()
	{
		public string Key { get; set; }
		public string Title { get; set; }
		public string Subtitle { get; set; }
		public string FirstPublishedDate { get; set; }
		public string Description { get; set; }
		public List<TAuthor> Authors { get; set; } = new List<TAuthor>();
		public string AuthorsStr => string.Join(",", Authors);
		public string Subjects { get; set; }
		public List<string> CoverFiles { get; set; } = new List<string>();

		public override string ToString() => Title;
	}

	public class DbConn
	{
		public static readonly DbConn I = new DbConn();

		private SqlConnection conn;

		private DbConn() => Connect();

		private static void ShowExceptionMBox(string message, Exception ex)
		{
			Console.WriteLine(ex);
			var reason = ex is SqlException ? "Database error" : "Unknown";
			ShowErrorMBox($"{message}. (Reason: {reason})");
		}

		private void Connect()
		{
			try
			{
				var connStrBuilder = new SqlConnectionStringBuilder()
				{
					DataSource = "DESKTOP-9N3PQ6U",
					UserID = "root",
					Password = "password",
					InitialCatalog = "BookMasters"
				};
				conn = new SqlConnection(connStrBuilder.ConnectionString);
				conn.Open();
			}
			catch (Exception ex)
			{
				ShowExceptionMBox("Failed to connect to database", ex);
				Environment.Exit(-1);
			}
		}

		public List<TBook> GetBooks<TBook, TAuthor>(string titleContains, string authorsListContains, string subjectsContains)
			where TBook : Book<TAuthor>, new()
			where TAuthor : Author, new()
		{
			Dictionary<string, TBook> books = new Dictionary<string, TBook>();

			try
			{
				using (var cmd = new SqlCommand(@"SELECT [b].[Key]
	  ,[b].[Title]
	  ,[b].[Subtitle]
	  ,[b].[FirstPublishDate]
	  ,[b].[Description]
	  ,[bs].[subject]
  FROM [Books] [b]
JOIN [BookAuthors] [ba]
  ON [ba].[BookKey] = [b].[Key]
JOIN [Authors] [a]
  ON [a].[Key] = [ba].[AuthorKey]
LEFT JOIN [BookSubjects] [bs]
	   ON [bs].[Book_Key] = [b].[Key]
WHERE 1 = 1", conn))
				{
					if (!string.IsNullOrEmpty(titleContains))
					{
						cmd.CommandText += "AND [b].[Title] LIKE @TitlePattern";
						cmd.Parameters.AddWithValue("TitlePattern", $"%{titleContains}%");
					}
					if (!string.IsNullOrEmpty(authorsListContains))
					{
						cmd.CommandText += "AND [a].[Name] LIKE @AuthorsListPattern";
						cmd.Parameters.AddWithValue("AuthorsListPattern", $"%{authorsListContains}%");
					}
					if (!string.IsNullOrEmpty(subjectsContains))
					{
						cmd.CommandText += "AND [bs].[subject] LIKE @SubjectPattern";
						cmd.Parameters.AddWithValue("SubjectPattern", $"%{subjectsContains}%");
					}
					using (SqlDataReader r = cmd.ExecuteReader())
					{
						while (r.Read())
						{
							var key = r.GetString(0);
							if (books.ContainsKey(key)) continue;
							books[key] = new TBook()
							{
								Key = key,
								Title = r.GetString(1),
								Subtitle = r.GetString(2, null),
								FirstPublishedDate = r.GetString(3, null),
								Description = r.GetString(4, null),
								Subjects = r.GetString(5, null)
							};
						}
					}
				}

				foreach (var keyBook in books)
				{
					var key = keyBook.Key;
					var book = keyBook.Value;
					using (var cmd = new SqlCommand(@"SELECT [a].[Key]
	  ,[a].[Name]
	  ,[a].[Bio]
	  ,[a].[BirthDate]
	  ,[a].[DeathDate]
	  ,[a].[wikipedia]
  FROM [Authors] [a]
  JOIN [BookAuthors] [ba]
	ON [ba].[AuthorKey] = [a].[Key]
 WHERE [ba].[BookKey] = @BookKey;", conn))
					{
						cmd.Parameters.AddWithValue("BookKey", key);
						using (SqlDataReader r = cmd.ExecuteReader())
						{
							while (r.Read())
							{
								book.Authors.Add(new TAuthor()
								{
									Key = r.GetString(0),
									Name = r.GetString(1),
									Bio = r.GetString(2, null),
									BirthDate = r.GetString(3, null),
									DeathDate = r.GetString(4, null),
									Wikipedia = r.GetString(5, null)
								});
							}
						}
					}
					using (var cmd = new SqlCommand(@"SELECT [CoverFile]
  FROM [BookCovers]
 WHERE [BookKey] = @BookKey;", conn))
					{
						cmd.Parameters.AddWithValue("BookKey", key);
						using (SqlDataReader r = cmd.ExecuteReader())
						{
							while (r.Read())
							{
								if(r.IsDBNull(0))continue;
								book.CoverFiles.Add($"covers/{r.GetInt32(0)}.jpg");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ShowExceptionMBox("Failed to get books listing", ex);
			}

			return books.Values.ToList();
		}
	}
}
