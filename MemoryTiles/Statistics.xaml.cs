namespace MemoryTiles
{
    /// <summary>
    /// Interaction logic for Statistics.xaml
    /// </summary>
    public partial class Statistics
    {
        public Statistics()
        {
            InitializeComponent();

            UserList.ItemsSource = MainWindow.Users;
        }
    }
}
