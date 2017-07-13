using System.Windows;

namespace WpfCrowdDetection.Services
{
    public interface ICustomDialogService
    {
        MessageBoxResult ShowMessageBox(string content, string title, MessageBoxButton buttons);
    }

    public class DialogService : ICustomDialogService
    {
        public MessageBoxResult ShowMessageBox(string content, string title, MessageBoxButton buttons)
        {
            return MessageBox.Show(content, title, buttons);
        }
    }
}