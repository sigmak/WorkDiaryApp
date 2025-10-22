/*
 * SharpDevelop으로 작성되었습니다.
 * 사용자: RYZEN3200G
 * 날짜: 2025-10-22
 * 시간: 오전 11:35
 * 
 * 이 템플리트를 변경하려면 [도구->옵션->코드 작성->표준 헤더 편집]을 이용하십시오.
 */
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace WorkDiaryApp.Utility.Converters
{
	/// <summary>
	/// Description of ByteArrayToImageConverter.
	/// </summary>
	public class ByteArrayToImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || !(value is byte[]))
			    return null;
			
			byte[] bytes = (byte[])value;
			if (bytes.Length == 0)
			    return null;
		
		    try
		    {
		        var image = new BitmapImage();
		        image.BeginInit();
		        image.CacheOption = BitmapCacheOption.OnLoad;
		        image.StreamSource = new MemoryStream(bytes);
		        image.EndInit();
		        image.Freeze();
		        return image;
		    }
		    catch
		    {
		        return null;
		    }
		}
	
	    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	    {
	        // 이미지 → 바이트 배열 변환은 일반적으로 ViewModel에서 처리
	        throw new NotSupportedException("ByteArrayToImageConverter는 단방향 변환만 지원합니다.");
	    }
	}
}
