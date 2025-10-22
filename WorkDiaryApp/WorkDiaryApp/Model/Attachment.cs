/*
 * SharpDevelop으로 작성되었습니다.
 * 사용자: RYZEN3200G
 * 날짜: 2025-10-22
 * 시간: 오전 11:32
 * 
 * 이 템플리트를 변경하려면 [도구->옵션->코드 작성->표준 헤더 편집]을 이용하십시오.
 */
// Model/Attachment.cs


namespace WorkDiaryApp.Model
{
	/// <summary>
	/// Description of Attachment.
	/// </summary>
	public class Attachment
	{
	    public int Id { get; set; }
	    public int WorkItemId { get; set; }
	    public string FileName { get; set; }
	    public byte[] FileData { get; set; } // BLOB
	    public bool IsImage { get; set; }
	}	
}
