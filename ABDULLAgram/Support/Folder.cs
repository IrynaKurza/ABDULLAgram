using System;

namespace ABDULLAgram.Support
{
    public class Folder
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("folder Name cannot be empty");
                _name = value;
            }
        }
    }
} 