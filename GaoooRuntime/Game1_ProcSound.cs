using Gaooo;
using Microsoft.Xna.Framework;
using SoLoud;
using System;

namespace GaoooRuntime
{
    public partial class Game1 : Game
    {
        private GaoooTask onPlayBgm(GaoooTag tag)
        {
            // TODO: Play Bgm
            return null;
        }

        private GaoooTask onStopBgm(GaoooTag tag)
        {
            // TODO: Stop Bgm
            return null;
        }

        private GaoooTask onPlaySe(GaoooTag tag)
        {
            stopSe(tag.Id, 0.0);
            playSe(tag.Id, tag.GetAttrValue<string>("audio"), tag.GetAttrValue<float>("volume", 1.0f), tag.GetAttrValue<double>("fadetime", 0.0));
            return null;
        }

        private GaoooTask onStopSe(GaoooTag tag)
        {
            stopSe(tag.Id, tag.GetAttrValue<double>("fadetime", 0.0));
            return null;
        }

        private GaoooTask onWaitSe(GaoooTag tag)
        {
            return new TaskWaitSe((x) => { return isPlayingSe(x.Id); }, tag); ;
        }
    }
}
