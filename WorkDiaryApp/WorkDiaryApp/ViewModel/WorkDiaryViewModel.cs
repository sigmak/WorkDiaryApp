/*
 * SharpDevelop으로 작성되었습니다.
 * 사용자: RYZEN3200G
 * 날짜: 2025-10-22
 * 시간: 오전 11:33
 * 
 * 이 템플리트를 변경하려면 [도구->옵션->코드 작성->표준 헤더 편집]을 이용하십시오.
 */
// 파일: ViewModel/WorkDiaryViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WorkDiaryApp.Model;
using WorkDiaryApp.DataAccess;

namespace WorkDiaryApp.ViewModel
{
	/// <summary>
	/// Description of WorkDiaryViewModel.
	/// </summary>
	public class WorkDiaryViewModel : ViewModelBase
	{
	    private readonly SQLiteRepository _repository = new SQLiteRepository();
	    private readonly string _itemType;
	
	    // 데이터 속성 (초기화)
	    public ObservableCollection<WorkItem> ItemList { get; set; }
	    public ObservableCollection<Attachment> Attachments { get; set; }
	
	    private DateTime _selectedDate = DateTime.Today;
	    public DateTime SelectedDate
	    {
	        get { return _selectedDate; }
	        set
	        {
	            if (SetProperty(ref _selectedDate, value))
	            {
	                ExecuteSearch(null);
	            }
	        }
	    }
	
	    private string _searchQuery;
	    public string SearchQuery
	    {
	        get { return _searchQuery; }
	        set { SetProperty(ref _searchQuery, value); }
	    }
	
	    private WorkItem _selectedItem;
	    public WorkItem SelectedItem
	    {
	        get { return _selectedItem; }
	        set
	        {
	            if (SetProperty(ref _selectedItem, value))
	            {
	                int id = 0;
	                if (_selectedItem != null)
	                    id = _selectedItem.Id;
	
	                LoadAttachments(id);
	
	                RelayCommand saveCmd = SaveCommand as RelayCommand;
	                RelayCommand deleteCmd = DeleteCommand as RelayCommand;
	                RelayCommand attachCmd = AttachFileCommand as RelayCommand;
	
	                if (saveCmd != null) saveCmd.RaiseCanExecuteChanged();
	                if (deleteCmd != null) deleteCmd.RaiseCanExecuteChanged();
	                if (attachCmd != null) attachCmd.RaiseCanExecuteChanged();
	
	                SelectedAttachment = null;
	            }
	        }
	    }
	
	    private Attachment _selectedAttachment;
	    public Attachment SelectedAttachment
	    {
	        get { return _selectedAttachment; }
	        set { SetProperty(ref _selectedAttachment, value); }
	    }
	
	    // 커맨드
	    public ICommand SearchCommand { get; private set; }
	    public ICommand SaveCommand { get; private set; }
	    public ICommand NewCommand { get; private set; }
	    public ICommand AttachFileCommand { get; private set; }
	    public ICommand DeleteCommand { get; private set; }
	
	    public WorkDiaryViewModel(string itemType)
	    {
	        _itemType = itemType;
	
	        // 속성 초기화
	        ItemList = new ObservableCollection<WorkItem>();
	        Attachments = new ObservableCollection<Attachment>();
	
	        SearchCommand = new RelayCommand(ExecuteSearch);
	        SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
	        NewCommand = new RelayCommand(ExecuteNew);
	        DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteSave);
	        AttachFileCommand = new RelayCommand(ExecuteAttachFile, CanExecuteSave);
	
	        ExecuteSearch(null);
	    }
	
	    // --- Command Logic ---
	
	    private void ExecuteSearch(object parameter)
	    {
	        try
	        {
	            string query = SearchQuery ?? string.Empty;
	            var items = _repository.GetWorkItemsByDate(SelectedDate, _itemType, query);
	
	            ItemList.Clear();
	            foreach (var item in items)
	            {
	                ItemList.Add(item);
	            }
	            // SelectedItem = null; // 주석 처리된 대로 유지
	        }
	        catch (Exception ex)
	        {
	            MessageBox.Show("검색 중 오류가 발생했습니다: " + ex.Message);
	        }
	    }
	
	    private void ExecuteNew(object parameter)
	    {
	        SelectedItem = new WorkItem
	        {
	            Type = _itemType,
	            Writer = "사용자",
	            WriteDate = SelectedDate,
	            Title = string.Empty
	        };
	    }
	
	    private void ExecuteSave(object parameter)
	    {
	        if (!CanExecuteSave(null)) return;
	        try
	        {
	            SelectedItem.WriteDate = SelectedDate;
	
	            int savedId = _repository.SaveWorkItem(SelectedItem);
	            if (SelectedItem.Id == 0)
	                SelectedItem.Id = savedId;
	
	            ExecuteSearch(null);
	            SelectedItem = ItemList.FirstOrDefault(i => i.Id == savedId);
	
	            MessageBox.Show("저장 완료.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
	        }
	        catch (Exception ex)
	        {
	            MessageBox.Show("저장 중 오류가 발생했습니다: " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
	        }
	    }
	
	    private void ExecuteDelete(object parameter)
	    {
	        if (SelectedItem == null || SelectedItem.Id == 0) return;
	
	        if (MessageBox.Show("'" + SelectedItem.Title + "'을(를) 삭제하시겠습니까? 관련 첨부파일도 모두 삭제됩니다.", "확인", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
	        {
	            try
	            {
	                _repository.DeleteWorkItem(SelectedItem.Id, SelectedDate);
	                ItemList.Remove(SelectedItem);
	                SelectedItem = null;
	                MessageBox.Show("삭제 완료.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
	            }
	            catch (Exception ex)
	            {
	                MessageBox.Show("삭제 오류: " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
	            }
	        }
	    }
	
	    private bool CanExecuteSave(object parameter)
	    {
	        return SelectedItem != null && !string.IsNullOrWhiteSpace(SelectedItem.Title);
	    }
	
	    private void LoadAttachments(int workItemId)
	    {
	        Attachments.Clear();
	        if (workItemId > 0)
	        {
	            var loadedAttachments = _repository.GetAttachments(workItemId, SelectedDate);
	            foreach (var att in loadedAttachments)
	            {
	                Attachments.Add(att);
	            }
	            SelectedAttachment = Attachments.FirstOrDefault(a => a.IsImage);
	        }
	    }
	
	    private void ExecuteAttachFile(object parameter)
	    {
	        if (SelectedItem == null || SelectedItem.Id == 0)
	        {
	            MessageBox.Show("항목을 먼저 저장해주세요.", "경고", MessageBoxButton.OK, MessageBoxImage.Warning);
	            return;
	        }
	
	        OpenFileDialog openFileDialog = new OpenFileDialog();
	        openFileDialog.Filter = "All Files (*.*)|*.*|Images (*.jpg;*.png;*.jpeg;*.gif)|*.jpg;*.png;*.jpeg;*.gif";
	        openFileDialog.Multiselect = true;
	
	        bool? result = openFileDialog.ShowDialog();
	        if (result == true)
	        {
	            foreach (string filePath in openFileDialog.FileNames)
	            {
	                try
	                {
	                    byte[] fileBytes = File.ReadAllBytes(filePath);
	                    string ext = Path.GetExtension(filePath);
	                    if (ext != null)
	                        ext = ext.ToLowerInvariant();
	                    else
	                        ext = "";
	
	                    bool isImage = ext == ".jpg" || ext == ".png" || ext == ".jpeg" || ext == ".gif";
	
	                    var attachment = new Attachment
	                    {
	                        WorkItemId = SelectedItem.Id,
	                        FileName = Path.GetFileName(filePath),
	                        FileData = fileBytes,
	                        IsImage = isImage
	                    };
	
	                    _repository.SaveAttachment(attachment, SelectedDate);
	                    Attachments.Add(attachment);
	                    
	                    // 1. 💡 AttachmentCount 갱신 (SelectedDate 전달)
	                    SelectedItem.AttachmentCount = _repository.GetAttachmentCount(SelectedItem.Id, SelectedDate);
	
	                    if (isImage && SelectedAttachment == null)
	                    {
	                        SelectedAttachment = attachment;
	                    }
	                }
	                catch (Exception ex)
	                {
	                    MessageBox.Show("파일 첨부 중 오류 발생: " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
	                }
	            }
				// ------------------------------------------------------------------
		        // 2. 💡 최종 수정/추가: 목록 갱신 및 UI 알림 (중요)
		        // ------------------------------------------------------------------
		
		        // DataGrid에 AttachmentCount가 즉시 반영되도록 SelectedItem의 PropertyChanged 이벤트를 강제 발생시킵니다.
		        // DataGridRow가 WorkItem 전체를 바인딩하고 있으므로, 이 방법을 사용합니다.
		        OnPropertyChanged(nameof(SelectedItem)); 
		        
		        // 목록 전체를 새로고침하여 (ExecuteSearch) DataGrid의 정렬 및 내용이 완전히 갱신되도록 합니다.
		        // ExecuteSearch(null); // 이 방법은 목록의 포커스를 잃게 할 수 있습니다. 위의 OnPropertyChanged를 권장합니다.
		        
		        // 작업 완료 알림
		        MessageBox.Show("파일 첨부가 완료되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);	            
	            
	        }
	    }
	}
	
	
}