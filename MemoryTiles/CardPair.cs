using System;
using System.Windows.Media.Imaging;

namespace MemoryTiles
{
    public class CardPair
    {
        public Card Card1 { get; }
        public Card Card2 { get; }
        
        public string ImagePath { get; }

        public CardPair(string imagePath, Card card1, Card card2)
        {
            Card1 = card1;
            Card2 = card2;
            
            ImagePath = imagePath;

            card1.Face = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            card2.Face = new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }
    }
}