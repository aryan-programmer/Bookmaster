using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bookmaster3000
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private List<Book<Author>> books;
		private Book<Author> selectedBook;
		private int selectedImageIndex;

		public List<Book<Author>> Books
		{
			get => books; set
			{
				books = value;
				NotifyPropertyChanged();
			}
		}
		public Book<Author> SelectedBook
		{
			get => selectedBook; set
			{
				selectedBook = value;
				NotifyPropertyChanged();
			}
		}
		public int SelectedImageIndex
		{
			get => selectedImageIndex; set
			{
				selectedImageIndex = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(SelectedImageFile));
			}
		}
		public string SelectedImageFile
		{
			get
			{
				if (SelectedBook == null || SelectedBook.CoverFiles.Count <= SelectedImageIndex) return "resources/warning symbol.png";
				return SelectedBook.CoverFiles[SelectedImageIndex];
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		public MainWindow()
		{
			InitializeComponent();
			PropertyChanged += MainWindow_PropertyChanged;
			NextImageBtn.IsEnabled = PrevImageBtn.IsEnabled = false;
		}

		private void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SelectedBook))
			{
				if (SelectedBook == null)
				{
					SelectedImageIndex = -1;
					return;
				}
				SelectedImageIndex = 0;
				NextImageBtn.IsEnabled = PrevImageBtn.IsEnabled = SelectedBook.CoverFiles.Count > 1;
			}
		}

		private void Search_Click(object sender, RoutedEventArgs e)
		{
			Books = DbConn.I.GetBooks<Book<Author>, Author>(TitleBox.Text, AuthorBox.Text, SubjectBox.Text);
			SelectedBook = null;
		}

		private void BooksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectedBook = (Book<Author>)BooksDataGrid.SelectedItem;
		}

		private void PrevImageBtn_Click(object sender, RoutedEventArgs e)
		{
			SelectedImageIndex = (SelectedImageIndex + SelectedBook.CoverFiles.Count - 1) % SelectedBook.CoverFiles.Count;
		}

		private void NextImageBtn_Click(object sender, RoutedEventArgs e)
		{
			SelectedImageIndex = (SelectedImageIndex + 1) % SelectedBook.CoverFiles.Count;
		}
	}
}
