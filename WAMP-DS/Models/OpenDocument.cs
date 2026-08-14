using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WAMP_DS.Models
{
    public class OpenDocument : INotifyPropertyChanged
    {
        private string filePath = string.Empty;
        private string fileName = string.Empty;
        private string content = string.Empty;
        private bool isModified;
        private bool isActive;


        public string FilePath
        {
            get => filePath;
            set
            {
                if (filePath == value)
                    return;

                filePath = value;

                OnPropertyChanged();
            }
        }


        public string FileName
        {
            get => fileName;
            set
            {
                if (fileName == value)
                    return;

                fileName = value;

                OnPropertyChanged();
            }
        }


        public string Content
        {
            get => content;
            set
            {
                if (content == value)
                    return;

                content = value;

                OnPropertyChanged();
            }
        }


        public bool IsModified
        {
            get => isModified;
            set
            {
                if (isModified == value)
                    return;

                isModified = value;

                OnPropertyChanged();
            }
        }


        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive == value)
                    return;

                isActive = value;

                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        protected virtual void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName)
            );
        }
    }
}