/*
 * SharpDevelop으로 작성되었습니다.
 * 사용자: RYZEN3200G
 * 날짜: 2025-10-22
 * 시간: 오전 11:32
 * 
 * 이 템플리트를 변경하려면 [도구->옵션->코드 작성->표준 헤더 편집]을 이용하십시오.
 */
// Model/WorkItem.cs
using System;
using System.ComponentModel; // INotifyPropertyChanged 사용 (선택 사항, ViewModel에서 처리 권장)


namespace WorkDiaryApp.Model
{
	/// <summary>
	/// Description of WorkItem.
	/// </summary>
	public class WorkItem : INotifyPropertyChanged
	{
	    // DB Column
	    public int Id { get; set; }
	    public string Type { get; set; } // "업무일지" 또는 "프로그래밍 팁"
	    public string Title { get; set; }
	    public string CompanyName { get; set; }
	    public string Writer { get; set; }
	    public string Content { get; set; }
	
	    private DateTime _writeDate;
	    public DateTime WriteDate
	    {
	        get { return _writeDate; }
	        set { _writeDate = value; }
	    }
	
	    public string Memo { get; set; }
	
	    // UI/List용 (DB Column 아님)
	    private int _attachmentCount;
	    public int AttachmentCount
	    {
	        get { return _attachmentCount; }
	        set
	        {
	            if (_attachmentCount != value)
	            {
	                _attachmentCount = value;
	                OnPropertyChanged("AttachmentCount");
	            }
	        }
	    }
	
	    // 생성자에서 WriteDate 초기화
	    public WorkItem()
	    {
	        _writeDate = DateTime.Now;
	    }
	
	    // INotifyPropertyChanged 구현
	    public event PropertyChangedEventHandler PropertyChanged;
	
	    protected void OnPropertyChanged(string propertyName)
	    {
	        PropertyChangedEventHandler handler = PropertyChanged;
	        if (handler != null)
	        {
	            handler(this, new PropertyChangedEventArgs(propertyName));
	        }
	    }
	}
}
