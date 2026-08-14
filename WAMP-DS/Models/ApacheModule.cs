using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WAMP_DS.Models
{
    public class ApacheModule : INotifyPropertyChanged
    {
        private bool isEnabled;


        public string Name { get; set; } = "";

        public string Directive { get; set; } = "";


        public List<string> Dependencies { get; set; }
            = new();


        public bool IsEnabled
        {
            get => isEnabled;

            set
            {
                if (isEnabled == value)
                    return;

                isEnabled = value;

                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(IsEnabled))
                );
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
    }
}