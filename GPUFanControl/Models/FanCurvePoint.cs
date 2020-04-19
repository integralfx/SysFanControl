using System;

namespace GPUFanControl.Models
{
    public class FanCurvePoint
    {
        private int percent;

        public int Temperature { get; set; }
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
            }
        }
    }
}
