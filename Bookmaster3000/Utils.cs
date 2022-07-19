using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bookmaster3000
{
	public static class Utils
	{
		public const string Title = "Bookmaster3000";

		public static void ShowErrorMBox(string msg) =>
			MessageBox.Show(msg, Title, MessageBoxButton.OK, MessageBoxImage.Error);

		public static void ShowWarnMBox(string msg) =>
			MessageBox.Show(msg, Title, MessageBoxButton.OK, MessageBoxImage.Warning);

		public static void ShowWarnConfirmMBox(string msg) =>
			MessageBox.Show(msg, Title, MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);

		public static void ShowInfoMBox(string msg) =>
			MessageBox.Show(msg, Title, MessageBoxButton.OK, MessageBoxImage.Information);

		public static string GetString(this SqlDataReader r, int i, string defaultValue) => r.IsDBNull(i) ? defaultValue : r.GetString(i);
	}
}
