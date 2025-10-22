/*
 * SharpDevelop으로 작성되었습니다.
 * 사용자: RYZEN3200G
 * 날짜: 2025-10-22
 * 시간: 오전 11:32
 * 
 * 이 템플리트를 변경하려면 [도구->옵션->코드 작성->표준 헤더 편집]을 이용하십시오.
 */
// DataAccess/SQLiteRepository.cs
using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using WorkDiaryApp.Model;

namespace WorkDiaryApp.DataAccess
{
	/// <summary>
	/// Description of SQLiteRepository.
	/// </summary>
	public class SQLiteRepository
	{
	    private const string DbDirectory = "Data";
	
	    public SQLiteRepository()
	    {
	        if (!Directory.Exists(DbDirectory))
	        {
	            Directory.CreateDirectory(DbDirectory);
	        }
	    }
	
	    private string GetConnectionString(DateTime date)
	    {
	        string fileName = date.ToString("yyyy-MM-dd") + ".sqlite";
	        string filePath = Path.Combine(DbDirectory, fileName);
	        return $"Data Source={filePath};Version=3;";
	    }
	
	    private void EnsureDatabaseExists(DateTime date)
	    {
	        string fileName = date.ToString("yyyy-MM-dd") + ".sqlite";
	        string filePath = Path.Combine(DbDirectory, fileName);
	        
	        if (!File.Exists(filePath))
	        {
	            SQLiteConnection.CreateFile(filePath);
	            using (var conn = new SQLiteConnection(GetConnectionString(date)))
	            {
	                conn.Open();
	                string createWorkItemTable = @"
	                    CREATE TABLE WorkItems (
	                        Id INTEGER PRIMARY KEY AUTOINCREMENT, Type TEXT NOT NULL, Title TEXT,
	                        CompanyName TEXT, Writer TEXT, Content TEXT, WriteDate TEXT, Memo TEXT
	                    );";
	                string createAttachmentTable = @"
	                    CREATE TABLE Attachments (
	                        Id INTEGER PRIMARY KEY AUTOINCREMENT, WorkItemId INTEGER,
	                        FileName TEXT, FileData BLOB, IsImage INTEGER
	                    );";
	                new SQLiteCommand(createWorkItemTable, conn).ExecuteNonQuery();
	                new SQLiteCommand(createAttachmentTable, conn).ExecuteNonQuery();
	            }
	        }
	    }
	
	    // WorkItem 로드
	    public List<WorkItem> GetWorkItemsByDate(DateTime date, string type, string query)
	    {
	        string fileName = date.ToString("yyyy-MM-dd") + ".sqlite";
	        string filePath = Path.Combine(DbDirectory, fileName);
	
	        if (!File.Exists(filePath)) return new List<WorkItem>();
	
	        var items = new List<WorkItem>();
	        using (var conn = new SQLiteConnection(GetConnectionString(date)))
	        {
	            conn.Open();
	            string sql = "SELECT Id, Type, Title, CompanyName, Writer, Content, WriteDate, Memo FROM WorkItems WHERE Type = @type AND (Title LIKE @query OR Content LIKE @query) ORDER BY Id DESC;";
	            using (var cmd = new SQLiteCommand(sql, conn))
	            {
	                cmd.Parameters.AddWithValue("@type", type);
	                cmd.Parameters.AddWithValue("@query", $"%{query}%");
	                using (var reader = cmd.ExecuteReader())
	                {
	                    while (reader.Read())
	                    {
	                        var item = new WorkItem
	                        {
	                            Id = reader.GetInt32(0),
	                            Type = reader.GetString(1),
	                            Title = reader.GetString(2),
	                            CompanyName = reader[3] as string, // NULL 허용
	                            Writer = reader[4] as string, // NULL 허용
	                            Content = reader[5] as string, // NULL 허용
	                            WriteDate = DateTime.Parse(reader.GetString(6)),
	                            Memo = reader[7] as string, // NULL 허용
	                        };
	                        item.AttachmentCount = GetAttachmentCount(item.Id, date);
	                        items.Add(item);
	                    }
	                }
	            }
	        }
	        return items;
	    }
	
	    // WorkItem 저장/수정 (새 ID 반환 로직 완성)
	    public int SaveWorkItem(WorkItem item)
	    {
	        EnsureDatabaseExists(item.WriteDate);
	        
	        using (var conn = new SQLiteConnection(GetConnectionString(item.WriteDate)))
	        {
	            conn.Open();
	            if (item.Id == 0) // INSERT
	            {
	                string sql = "INSERT INTO WorkItems (Type, Title, CompanyName, Writer, Content, WriteDate, Memo) VALUES (@Type, @Title, @CompanyName, @Writer, @Content, @WriteDate, @Memo); SELECT last_insert_rowid();";
	                using (var cmd = new SQLiteCommand(sql, conn))
	                {
	                    cmd.Parameters.AddWithValue("@Type", item.Type);
	                    cmd.Parameters.AddWithValue("@Title", item.Title);
	                    cmd.Parameters.AddWithValue("@CompanyName", item.CompanyName ?? (object)DBNull.Value);
	                    cmd.Parameters.AddWithValue("@Writer", item.Writer ?? (object)DBNull.Value);
	                    cmd.Parameters.AddWithValue("@Content", item.Content ?? (object)DBNull.Value);
	                    cmd.Parameters.AddWithValue("@WriteDate", item.WriteDate.ToString("yyyy-MM-dd HH:mm:ss"));
	                    cmd.Parameters.AddWithValue("@Memo", item.Memo ?? (object)DBNull.Value);
	                    return Convert.ToInt32(cmd.ExecuteScalar()); // 새 ID 반환
	                }
	            }
	            else // UPDATE
	            {
	                string sql = "UPDATE WorkItems SET Title = @Title, CompanyName = @CompanyName, Writer = @Writer, Content = @Content, Memo = @Memo WHERE Id = @Id;";
	                using (var cmd = new SQLiteCommand(sql, conn))
	                {
	                    cmd.Parameters.AddWithValue("@Id", item.Id);
	                    cmd.Parameters.AddWithValue("@Title", item.Title);
	                    cmd.Parameters.AddWithValue("@CompanyName", item.CompanyName ?? (object)DBNull.Value);
	                    cmd.Parameters.AddWithValue("@Writer", item.Writer ?? (object)DBNull.Value);
	                    cmd.Parameters.AddWithValue("@Content", item.Content ?? (object)DBNull.Value);
	                    cmd.Parameters.AddWithValue("@Memo", item.Memo ?? (object)DBNull.Value);
	                    cmd.ExecuteNonQuery();
	                }
	                return item.Id;
	            }
	        }
	    }
	    
	    // WorkItem 삭제
	    public void DeleteWorkItem(int id, DateTime date)
	    {
	        using (var conn = new SQLiteConnection(GetConnectionString(date)))
	        {
	            conn.Open();
	            new SQLiteCommand($"DELETE FROM WorkItems WHERE Id = {id}", conn).ExecuteNonQuery();
	            new SQLiteCommand($"DELETE FROM Attachments WHERE WorkItemId = {id}", conn).ExecuteNonQuery();
	        }
	    }
	
	    // Attachment 저장
	    public void SaveAttachment(Attachment attachment, DateTime itemDate)
	    {
	        using (var conn = new SQLiteConnection(GetConnectionString(itemDate)))
	        {
	            conn.Open();
	            string sql = "INSERT INTO Attachments (WorkItemId, FileName, FileData, IsImage) VALUES (@WorkItemId, @FileName, @FileData, @IsImage);";
	            using (var cmd = new SQLiteCommand(sql, conn))
	            {
	                cmd.Parameters.AddWithValue("@WorkItemId", attachment.WorkItemId);
	                cmd.Parameters.AddWithValue("@FileName", attachment.FileName);
	                cmd.Parameters.AddWithValue("@FileData", attachment.FileData);
	                cmd.Parameters.AddWithValue("@IsImage", attachment.IsImage ? 1 : 0);
	                cmd.ExecuteNonQuery();
	            }
	        }
	    }
	    
	    // Attachment 로드
	    public List<Attachment> GetAttachments(int workItemId, DateTime itemDate)
	    {
	        var attachments = new List<Attachment>();
	        string fileName = itemDate.ToString("yyyy-MM-dd") + ".sqlite";
	        string filePath = Path.Combine(DbDirectory, fileName);
	
	        if (!File.Exists(filePath)) return attachments;
	        
	        using (var conn = new SQLiteConnection(GetConnectionString(itemDate)))
	        {
	            conn.Open();
	            string sql = "SELECT Id, WorkItemId, FileName, FileData, IsImage FROM Attachments WHERE WorkItemId = @Id;";
	            using (var cmd = new SQLiteCommand(sql, conn))
	            {
	                cmd.Parameters.AddWithValue("@Id", workItemId);
	                using (var reader = cmd.ExecuteReader())
	                {
	                    while (reader.Read())
	                    {
	                        attachments.Add(new Attachment
	                        {
	                            Id = reader.GetInt32(0),
	                            WorkItemId = reader.GetInt32(1),
	                            FileName = reader.GetString(2),
	                            FileData = (byte[])reader["FileData"],
	                            IsImage = reader.GetInt32(4) == 1
	                        });
	                    }
	                }
	            }
	        }
	        return attachments;
	    }
	    
	    // 첨부 파일 개수 조회
	    public int GetAttachmentCount(int workItemId, DateTime date)
	    {
	        string fileName = date.ToString("yyyy-MM-dd") + ".sqlite";
	        string filePath = Path.Combine(DbDirectory, fileName);
	        if (!File.Exists(filePath)) return 0;
	        
	        using (var conn = new SQLiteConnection(GetConnectionString(date)))
	        {
	            conn.Open();
	            string sql = "SELECT COUNT(*) FROM Attachments WHERE WorkItemId = @Id;";
	            using (var cmd = new SQLiteCommand(sql, conn))
	            {
	                cmd.Parameters.AddWithValue("@Id", workItemId);
	                return Convert.ToInt32(cmd.ExecuteScalar());
	            }
	        }
	    }
	}
}
