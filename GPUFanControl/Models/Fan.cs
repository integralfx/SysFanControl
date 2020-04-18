namespace GPUFanControl.Models
{
    public class Fan
    {
        public int index, speed;

        public int Index
        {
            get => index;
            set => index = value;
        }
        public int Speed
        {
            get => speed;
            set => speed = value;
        }
    }
}
