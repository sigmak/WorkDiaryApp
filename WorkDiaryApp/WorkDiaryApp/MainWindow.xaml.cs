/*
 * SharpDevelop으로 작성되었습니다.
 * 사용자: RYZEN3200G
 * 날짜: 2025-10-22
 * 시간: 오전 11:27
 * 
 * 이 템플리트를 변경하려면 [도구->옵션->코드 작성->표준 헤더 편집]을 이용하십시오.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using WorkDiaryApp.ViewModel;

namespace WorkDiaryApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 업무 일지 탭에 ViewModel 설정
            WorkDiaryTab.DataContext = new WorkDiaryViewModel("업무일지");
            
            // 프로그래밍 팁 탭에 ViewModel 설정
            ProgrammingTipTab.DataContext = new WorkDiaryViewModel("프로그래밍 팁");
        }		
	}
}