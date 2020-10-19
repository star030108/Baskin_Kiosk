﻿using Prism.Mvvm;
using System;

namespace Baskin_Kiosk.Common
{
    public class Food : BindableBase
    {
        public Category category { get; set; }
        public String imageSrc { get; set; }
        public String foodName { get; set; }
        public int page { get; set; }
        public int price { get; set; }

        private int _count = 1;
        public int count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }
    }
}
