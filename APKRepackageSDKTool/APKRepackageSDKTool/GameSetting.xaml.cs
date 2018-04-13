using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace APKRepackageSDKTool
{
    /// <summary>
    /// GameSetting.xaml 的交互逻辑
    /// </summary>
    public partial class GameSetting : Window
    {
        public GameSetting()
        {
            InitializeComponent();

            ListBox_gameList.ItemsSource = EditorData.GameList;
        }

        private void Button_ClickAdd(object sender, RoutedEventArgs e)
        {
            if (TextBox_GameName.Text != "")
            {
                GameInfo gi = new GameInfo();
                gi.GameName = TextBox_GameName.Text;

                EditorData.GameList.Add(gi);
                EditorData.GameList = EditorData.GameList;

                TextBox_GameName.Text = "";
            }
        }

        private void Button_ClickDelete(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("确定要移除这个项目吗？\n所有填写的配置都会丢失", "警告", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                Button btn = sender as Button;
                string name = (string)btn.Tag;

                for (int i = 0; i < EditorData.GameList.Count; i++)
                {
                    if(EditorData.GameList[i].GameName == name)
                    {
                        EditorData.GameList.RemoveAt(i);
                        EditorData.GameList = EditorData.GameList;
                        break;
                    }
                }
            }
        }
    }
}
