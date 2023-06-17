using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace MemoryTiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static ObservableCollection<User>? Users { get; private set; }
        public static User? SelectedUser { get; private set; }
        private Menu? Menu { get; set; }
        private NewUserWindow? NewUser { get; set; }

        private readonly MediaPlayer _mediaPlayer = new();

        public MainWindow()
        {
            InitializeComponent();

            Users = DeserializeUsers();

            UserListBox.ItemsSource = Users;

            DataContext = this;

            _mediaPlayer.Open(new Uri("assets/music/readySetRelax.mp3", UriKind.Relative));
            _mediaPlayer.MediaEnded += (sender, args) =>
            {
                _mediaPlayer.Position = TimeSpan.Zero;

                _mediaPlayer.Play();
            };
            _mediaPlayer.Play();
        }

        private static ObservableCollection<User> DeserializeUsers()
        {
            ObservableCollection<User> users = new();

            if (!Directory.Exists("users"))
            {
                return users;
            }

            foreach (var file in Directory.GetFiles("users"))
            {
                XmlSerializer serializer = new(typeof(User));

                using var xmlReader = XmlReader.Create(file);
                if (serializer.Deserialize(xmlReader) is User user)
                {
                    users.Add(user);
                }
            }

            return users;
        }

        private void UserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Menu is { IsLoaded: true })
            {
                MessageBox.Show("Close the menu before selecting another user");
                return;
            }

            if (UserListBox.SelectedItem == null)
            {
                return;
            }

            SelectedUser = (User)UserListBox.SelectedItem;
            ProfilePicture.Source = new BitmapImage(new Uri(SelectedUser.ProfilePicturePath, UriKind.Relative));
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Select a user first");
                return;
            }

            if (Menu is not { IsLoaded: true })
            {
                Menu = new Menu();
                Menu.Show();
            }

            Menu.Focus();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NewUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewUser is not { IsLoaded: true })
            {
                NewUser = new NewUserWindow();
                NewUser.Show();
            }

            NewUser.Focus();
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserListBox.SelectedItem == null)
            {
                return;
            }

            SelectedUser = (User)UserListBox.SelectedItem;
            Users?.Remove(SelectedUser);
            File.Delete($"users/{SelectedUser.Name}.xml");
            File.Delete($"saves/{SelectedUser.Name}.xml");
        }

        public static void SerializeUsers()
        {
            if (Users == null)
            {
                return;
            }

            foreach (var user in Users)
            {
                user.SerializeUser();
            }
        }
    }
}