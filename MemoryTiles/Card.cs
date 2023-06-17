using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MemoryTiles
{
    public class Card : Button
    {
        public bool IsFlipped
        {
            set => Background = new ImageBrush(value ? Face : Back);
        }

        public int Row { get; init; }
        public int Column { get; init; }

        public BitmapImage? Face { get; set; }
        public BitmapImage? Back { get; set; }
    }
}