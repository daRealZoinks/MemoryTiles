using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MemoryTiles
{
    /// <summary>
    /// Interaction logic for NewUser.xaml
    /// </summary>
    public partial class NewUserWindow
    {
        private readonly List<BitmapImage> _images;
        private int _selectedImage;

        private int SelectedImage
        {
            get => _selectedImage;
            set
            {
                if (value < 0)
                {
                    _selectedImage = _images.Count - 1;
                }
                else
                {
                    _selectedImage = value >= _images.Count ? 0 : value;
                }

                ProfilePicture.Source = _images[_selectedImage];
            }
        }

        public NewUserWindow()
        {
            InitializeComponent();

            _images = new List<BitmapImage>();

            var imageNames = Directory.GetFiles("assets/images/profile_pictures");

            foreach (var imagePath in imageNames)
            {
                _images.Add(new BitmapImage(new Uri(imagePath, UriKind.Relative)));
            }

            ProfilePicture.Source = _images[0];
        }

        private void PreviousImageButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedImage--;
        }

        private void NextImageButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedImage++;
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;

            if (username == string.Empty)
            {
                MessageBox.Show("Please enter a username");
                return;
            }

            if (MainWindow.Users != null)
            {
                if (MainWindow.Users.Any(user => user.Name == username))
                {
                    MessageBox.Show("Username already taken");
                    return;
                }

                User newUser = new(_images[_selectedImage].UriSource.ToString(),
                    UsernameTextBox.Text.Replace(" ", string.Empty), 0, 0);

                newUser.SerializeUser();

                MainWindow.Users.Add(newUser);
            }

            MessageBox.Show("Account successfully created");
        }
    }
}
