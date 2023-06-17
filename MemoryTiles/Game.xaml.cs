using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MemoryTiles
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : IXmlSerializable
    {
        private List<CardPair>? _cardPairs;

        private Card? _selectedCard1;
        private Card? _selectedCard2;

        private int _rows;
        private int _columns;

        private int _minutes;
        private int _seconds;

        private readonly DispatcherTimer _dispatcherTimer = new();

        private int _level = 1;

        private int Level
        {
            get => _level;

            set
            {
                LevelLabel.Content = "Level: " + value.ToString();
                _level = value;
            }
        }

        public Game()
        {
            InitializeComponent();
        }

        public Game(int rows, int columns)
        {
            InitializeComponent();

            _rows = rows;
            _columns = columns;

            _dispatcherTimer.Tick += DispatcherTimer_Tick!;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

            GenerateGame();
        }

        private void GenerateGame()
        {
            List<Card> cards = new();

            for (var i = 0; i < _rows; i++)
            {
                for (var j = 0; j < _columns; j++)
                {
                    if (_rows * _columns % 2 != 0)
                    {
                        if (i == _rows / 2 && j == _columns / 2)
                        {
                            continue;
                        }
                    }

                    Card card = new()
                    {
                        Row = i,
                        Column = j
                    };

                    cards.Add(card);
                }
            }

            var imageNames = Directory.GetFiles("assets/images/cards/face").ToList();

            _cardPairs = new List<CardPair>();
            Random random = new();

            while (cards.Count > 1)
            {
                var randomIndex = random.Next(imageNames.Count);
                var randomImage = imageNames[randomIndex];
                imageNames.RemoveAt(randomIndex);

                var card1 = cards[random.Next(cards.Count)];
                cards.Remove(card1);

                var card2 = cards[random.Next(cards.Count)];
                cards.Remove(card2);

                _cardPairs.Add(new CardPair(randomImage, card1, card2));
            }

            CardPreparation();

            _minutes = 3;
            _seconds = 00;
            TimerLabel.Content = "Timer: 3:00";

            _dispatcherTimer.Start();
        }

        private void CardPreparation()
        {
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            for (var i = 0; i < _rows; i++)
            {
                RowDefinition row = new();
                GameGrid.RowDefinitions.Add(row);
            }

            for (var i = 0; i < _columns; i++)
            {
                ColumnDefinition col = new();
                GameGrid.ColumnDefinitions.Add(col);
            }

            List<Card> cards = new();

            if (_cardPairs != null)
            {
                foreach (var cardPair in _cardPairs)
                {
                    cards.Add(cardPair.Card1);
                    cards.Add(cardPair.Card2);
                }
            }

            foreach (var card in cards)
            {
                GameGrid.Children.Add(card);
                Grid.SetRow(card, card.Row);
                Grid.SetColumn(card, card.Column);

                card.Click += Card_Click;
                card.Margin = new Thickness(10);

                card.Back = new BitmapImage(new Uri("assets/images/cards/back/cardback.jpg", UriKind.Relative));
                card.IsFlipped = false;
            }
        }

        public void Serialize()
        {
            XmlSerializer serializer = new(typeof(Game));

            if (!Directory.Exists("saves"))
            {
                Directory.CreateDirectory("saves");
            }

            using var writer = XmlWriter.Create($"saves/{MainWindow.SelectedUser?.Name}.xml");
            serializer.Serialize(writer, this);
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            var card = (Card)sender;

            if (card == _selectedCard1 && _selectedCard2 == null)
            {
                return;
            }

            if (_selectedCard1 != null && _selectedCard2 != null)
            {
                _selectedCard1.IsFlipped = false;
                _selectedCard1 = null;
                _selectedCard2.IsFlipped = false;
                _selectedCard2 = null;
            }

            if (_selectedCard1 == null)
            {
                _selectedCard1 = card;
                _selectedCard1.IsFlipped = true;
            }
            else
            {
                if (_selectedCard2 == null)
                {
                    _selectedCard2 = card;
                    _selectedCard2.IsFlipped = true;

                    CardPair? validCardPair = null;

                    if (_cardPairs != null)
                    {
                        foreach (var cardPair in _cardPairs.Where(cardPair =>
                                     (cardPair.Card1 == _selectedCard2 && cardPair.Card2 == _selectedCard1) ||
                                     (cardPair.Card2 == _selectedCard2 && cardPair.Card1 == _selectedCard1)))
                        {
                            validCardPair = cardPair;
                        }

                        if (validCardPair != null)
                        {
                            validCardPair.Card1.IsEnabled = false;
                            validCardPair.Card2.IsEnabled = false;
                            _cardPairs.Remove(validCardPair);

                            _selectedCard1 = null;
                            _selectedCard2 = null;
                        }
                    }
                }
            }

            if (_cardPairs != null && _cardPairs.Count != 0)
            {
                return;
            }

            Victory();
        }

        private void Victory()
        {
            _dispatcherTimer.Stop();

            if (Level < 3)
            {
                MessageBox.Show($"Level {Level} completed!");
                Level++;
            }
            else
            {
                MessageBox.Show("You won!");
                Level = 1;

                if (MainWindow.SelectedUser != null)
                {
                    MainWindow.SelectedUser.GamesPlayed++;
                    MainWindow.SelectedUser.GamesWon++;
                }

                MainWindow.SerializeUsers();
            }

            GenerateGame();
        }

        protected override void OnClosed(EventArgs e)
        {
            _dispatcherTimer.Stop();

            base.OnClosed(e);
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_seconds == 0)
            {
                if (_minutes == 0)
                {
                    Loss();
                    return;
                }

                _minutes--;
                _seconds = 59;
            }
            else
            {
                _seconds--;
            }

            TimerLabel.Content = _seconds < 10 ? $"Timer: {_minutes}:0{_seconds}" : $"Timer: {_minutes}:{_seconds}";
        }

        private void Loss()
        {
            _dispatcherTimer.Stop();
            MessageBox.Show("Time's up!");
            Level = 1;

            if (MainWindow.SelectedUser != null)
            {
                MainWindow.SelectedUser.GamesPlayed++;
            }

            MainWindow.SerializeUsers();

            GenerateGame();
        }

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            _rows = int.Parse(reader.ReadElementString("Rows"));
            _columns = int.Parse(reader.ReadElementString("Columns"));
            Level = int.Parse(reader.ReadElementString("Level"));
            _minutes = int.Parse(reader.ReadElementString("Minutes"));
            _seconds = int.Parse(reader.ReadElementString("Seconds"));

            _cardPairs = new List<CardPair>();
            while (reader.Name == "CardPair")
            {
                reader.ReadStartElement();
                reader.ReadStartElement("Card1");
                var card1Row = int.Parse(reader.ReadElementString("Row"));
                var card1Column = int.Parse(reader.ReadElementString("Column"));
                reader.ReadEndElement();
                reader.ReadStartElement("Card2");
                var card2Row = int.Parse(reader.ReadElementString("Row"));
                var card2Column = int.Parse(reader.ReadElementString("Column"));
                reader.ReadEndElement();
                var imagePath = reader.ReadElementString("Image");

                Card card1 = new()
                {
                    Row = card1Row,
                    Column = card1Column
                };

                Card card2 = new()
                {
                    Row = card2Row,
                    Column = card2Column
                };

                var cardPair = new CardPair(imagePath, card1, card2);

                _cardPairs.Add(cardPair);

                reader.ReadEndElement();
            }
            reader.ReadEndElement();

            CardPreparation();

            TimerLabel.Content = _seconds < 10 ? $"Timer: {_minutes}:0{_seconds}" : $"Timer: {_minutes}:{_seconds}";

            _dispatcherTimer.Tick += DispatcherTimer_Tick!;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Rows", _rows.ToString());
            writer.WriteElementString("Columns", _columns.ToString());
            writer.WriteElementString("Level", Level.ToString());
            writer.WriteElementString("Minutes", _minutes.ToString());
            writer.WriteElementString("Seconds", _seconds.ToString());

            if (_cardPairs == null)
            {
                return;
            }

            foreach (var cardPair in _cardPairs)
            {
                writer.WriteStartElement("CardPair");

                writer.WriteStartElement("Card1");
                writer.WriteElementString("Row", cardPair.Card1.Row.ToString());
                writer.WriteElementString("Column", cardPair.Card1.Column.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("Card2");
                writer.WriteElementString("Row", cardPair.Card2.Row.ToString());
                writer.WriteElementString("Column", cardPair.Card2.Column.ToString());
                writer.WriteEndElement();

                writer.WriteElementString("Image", cardPair.ImagePath);
                writer.WriteEndElement();
            }
        }
    }
}