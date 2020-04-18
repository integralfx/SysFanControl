namespace GPUFanControl.Models
{
    public class Fan
    {
        public int index, speed;

        public Fan(int index = 0, int speed = 0)
        {
            this.index = index;
            this.speed = speed;
        }
    }
}
