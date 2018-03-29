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
                gi.channelInfo = new List<ChannelInfo>();

                EditorData.GameList.Add(gi);
                EditorData.GameList = EditorData.GameList;

                TextBox_GameName.Text = "";
            }
        }

        private void Button_ClickDelete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int index = (int)btn.Tag;

            EditorData.GameList.RemoveAt(index);
            EditorData.GameList = EditorData.GameList;
        }
    }
}
