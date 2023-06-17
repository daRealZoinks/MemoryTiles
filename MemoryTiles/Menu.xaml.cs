using System;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace MemoryTiles
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu
    {
        private bool _standard;

        private Game? _game;
        private Statistics? _statistics;

        public Menu()
        {
            InitializeComponent();

            UsernameLabel.Content = MainWindow.SelectedUser?.Name;
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var rows = 5;
            var columns = 5;

            if (!_standard)
            {
                try
                {
                    rows = int.Parse(M.Text);
                    columns = int.Parse(N.Text);

                    if (rows < 1 || columns < 1 || rows > 5 || columns > 5)
                    {
                        throw new InvalidOperationException();
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("M and N need to be whole numbers");
                    return;
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("M and N need to be between 1 and 5");
                    return;
                }
                catch (Exception)
                {
                    MessageBox.Show("Unknown error");
                    return;
                }
            }

            if (_game is not { IsLoaded: true })
            {
                _game = new Game(rows, columns);
                _game.Show();
            }

            _game.Focus();
        }

        private void OpenGameButton_Click(object sender, RoutedEventArgs e)
        {
            Deserialize();
        }

        private void SaveGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_game is not { IsLoaded: true })
            {
                return;
            }
            _game.Serialize();

            MessageBox.Show("Game saved");
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_statistics is not { IsLoaded: true })
            {
                _statistics = new();
                _statistics.Show();
            }

            _statistics.Focus();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StandardButton_Checked(object sender, RoutedEventArgs e)
        {
            _standard = true;
        }

        private void CustomButton_Checked(object sender, RoutedEventArgs e)
        {
            _standard = false;
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            const string nume = "Murarasu Vlad";
            const string grupa = "10LF212";
            const string specializare = "Informatica";

            MessageBox.Show(
                $"Realizat de {nume} din grupa {grupa} din specializarea {specializare} (datimi va rog doamna 10 nu stiti prin cate am trecut si nu stiti ce mult merit o durat asa mult sa fac tema va rog io frumos)");
        }

        private void Deserialize()
        {
            if (_game is not { IsLoaded: true })
            {
                XmlSerializer serializer = new(typeof(Game));

                using var xmlReader = XmlReader.Create($"saves/{MainWindow.SelectedUser?.Name}.xml");
                if (serializer.Deserialize(xmlReader) is Game game)
                {
                    _game = game;
                    _game.Show();
                }
            }

            _game?.Focus();
        }
    }
}