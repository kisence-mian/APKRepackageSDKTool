using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;


public class AudioTool
{
    public static void PlayAudio(string uri, UriKind uriKind)
    {
        MediaPlayer player = new MediaPlayer();
        player.Open(new Uri(uri, uriKind));
        player.Play();

        //player.Volume = 1;
    }
}
