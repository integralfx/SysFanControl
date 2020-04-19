using GPUFanControl.ViewModels;
using System;

namespace GPUFanControl.Models
{
    public class FanCurvePoint : BaseViewModel
    {
        private int temperature = 0, percent = 0;

        public int Temperature
        {
            get => temperature;
            set => SetProperty(ref temperature, value);
        }
        public int Percent
        {
            get => percent;
            set
            {
                if (value < 0 || value > 100)
                {
                    throw new ArgumentException("Percent set");
                }

                percent = value;
                PropertyUpdated();
            }
        }
    }
}
