using Microsoft.Xna.Framework;
using SoLoud;
using System;

namespace GaoooRuntime
{
    public partial class Game1 : Game
    {
        private void playSe(string id, string audio, float volume, double fadetime)
        {
            var wav = new Wav();
            wav.load(getWsPath(audio + ".wav"));

            var handle = _soloud.play(wav);
            if (fadetime > 0.0)
            {
                _soloud.setVolume(handle, 0.0f);
                _soloud.fadeVolume(handle, volume, fadetime);
            }
            else
            {
                _soloud.setVolume(handle, volume);
            }

            _handle[id] = handle;
        }

        private void stopSe(string id, double fadetime)
        {
            uint handle;
            if (_handle.TryGetValue(id, out handle))
            {
                if (fadetime > 0.0)
                {
                    _soloud.fadeVolume(handle, 0.0f, fadetime);
                }
                else
                {
                    _soloud.stop(handle);
                }
                _handle.Remove(id);
            }
        }

        private bool isPlayingSe(string id)
        {
            uint handle;
            if (_handle.TryGetValue(id, out handle))
            {
                var result = 0 != _soloud.isValidVoiceHandle(handle);
                if (result)
                {
                    Console.Write($"Se is playing: {id}" );
                }
                return result;
            }
            return false;
        }
    }
}
