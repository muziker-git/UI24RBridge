﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UI24RController.UI24RChannels
{
    public class Mixer
    {

        public Mixer()
        {
            initLayers();
        }

        #region ViewGroup
        private List<int[]> _layers = new List<int[]>();
        private int _selectedLayer;
        private int _selectedBank;
        private int _numLayersPerBank;
        private int _numBanks;
        private int _numFaders;

        private void initLayers()
        {
            _numLayersPerBank = 6;
            _numBanks = 3;
            _selectedLayer = 0;
            _selectedBank = 0;
            _numFaders = 9;

            //Inititalize Initial Layers
            for (int i = 0; i < _numLayersPerBank; ++i)
            {
                int[] channelLayer = new int[9];
                for (int j = 0; j < _numFaders - 1; ++j)
                    channelLayer[j] = j + i * (_numFaders - 1);
                channelLayer[_numFaders - 1] = 54;
                _layers.Add(channelLayer);
            }

            //initialize View group layers
            for (int i = 0; i < _numLayersPerBank; ++i)
            {
                int[] channelLayer = new int[9];
                channelLayer[0] = -1;
                channelLayer[_numFaders - 1] = 54;
                _layers.Add(channelLayer);
            }

            //initialize User layers
            for (int i = 0; i < _numLayersPerBank; ++i)
            {
                int[] channelLayer = new int[9];
                for (int j = 0; j < _numFaders - 1; ++j)
                    channelLayer[j] = j + i * (_numFaders - 1);
                channelLayer[_numFaders - 1] = 54;
                _layers.Add(channelLayer);
            }
        }

        public void setUserLayerFromArray(int[][] input)
        {
            for (int i = 0; i < input.Length && i < _numLayersPerBank; ++i)
                if (input[i].Length >= _numFaders - 1)
                    for (int j = 0; j < _numFaders - 1; ++j)
                        _layers[i+2*_numLayersPerBank][j] = input[i][j];
        }
        public int[][] getUserLayerToArray()
        {
            int[][] output = new int[_numLayersPerBank][];
            for (int i = 0; i < _numLayersPerBank; ++i)
            {
                output[i] = new int[_numFaders - 1];
                for (int j = 0; j < _numFaders - 1; ++j)
                    output[i][j] = _layers[i + 2 * _numLayersPerBank][j];
            }
            return output;
        }

        public void setChannelToViewLayerAndPosition(int channel, int layer, int position)
        {
            setChannelToBankLayerAndPosition(channel, 1, layer, position);
        }

        public void setChannelToUserLayerAndPosition(int channel, int layer, int position)
        {
            setChannelToBankLayerAndPosition(channel, 3, layer, position);
        }

        public void setChannelToBankLayerAndPosition(int channel, int bank, int layer, int position)
        {
            if (position < 0 || position >= 8)
                return;
            if (bank < 0 || bank >= _numBanks)
                return;
            if (layer < 0 || layer >= _numLayersPerBank)
                return;
            _layers[bank * _numLayersPerBank + layer][position] = channel;
        }

        private void skipUnusedLayerUp()
        {
            int initialLayer = _selectedLayer;
            //skip unused layers
            while (_layers[_selectedLayer][0] < 0)
            {
                _selectedLayer = (_selectedLayer + 1) % (_numLayersPerBank * _numBanks);
                if (_selectedLayer % _numLayersPerBank == 0 && initialLayer >= 0)
                {
                    _selectedLayer = _selectedBank * _numLayersPerBank;
                    initialLayer = -1;
                }
            }
            //update bank if needed
            _selectedBank = _selectedLayer / _numLayersPerBank;
        }

        private void skipUnusedLayerDown()
        {
            int initialLayer = _selectedLayer;
            //skip unused layers
            while (_layers[_selectedLayer][0] < 0)
            {
                _selectedLayer = (_numLayersPerBank * _numBanks + _selectedLayer - 1) % (_numLayersPerBank * _numBanks);
                if (_selectedLayer % _numLayersPerBank == _numLayersPerBank-1 && initialLayer >= 0)
                {
                    _selectedLayer = _selectedBank * _numLayersPerBank;
                    initialLayer = -1;
                }
            }
                
            //update bank if needed
            _selectedBank = _selectedLayer / _numLayersPerBank;
        }

        public void setLayerUp()
        {
            _selectedLayer = (_selectedLayer + 1) % _numLayersPerBank + _numLayersPerBank * _selectedBank;
            skipUnusedLayerUp();
        }
        public void setLayerDown()
        {
            _selectedLayer = (_selectedLayer + _numLayersPerBank - 1) % _numLayersPerBank + _numLayersPerBank * _selectedBank;
            skipUnusedLayerDown();
        }

        public void setBankUp()
        {
            _selectedBank = (_selectedBank + 1) % _numBanks;
            _selectedLayer = _selectedBank * _numLayersPerBank;
            skipUnusedLayerUp();
        }
        public void setBankDown()
        {
            _selectedBank = (_selectedBank + _numBanks - 1) % _numBanks;
            _selectedLayer = _selectedBank * _numLayersPerBank;
            skipUnusedLayerDown();
        }

        public char getBankChar(int bank)
        {
            switch(bank)
            {
                case 0:
                    return 'I'; // Initial Layer
                case 1:
                    return 'V'; // View Layer
                case 2:
                default:
                    return 'U';
            }
        }
        public string getCurrentLayerString()
        {
            return getBankChar(_selectedBank).ToString() + ((_selectedLayer%_numLayersPerBank)+1).ToString();
        }

        public int[] getCurrentLayer()
        {
            if (_selectedLayer < _layers.Count)
                return _layers[_selectedLayer];
            return new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 54 };
        }

        public int getChannelNumberInCurrentLayer(int ch)
        {
            ch = ch % _numFaders;
            if (ch < 0) ch = 0;
            return getCurrentLayer()[ch];
        }
        #endregion


        public bool IsMultitrackRecordingRun { get; set; }
        public bool IsTwoTrackRecordingRun { get; set; }

        public string GetStartMTKRecordMessage()
        {
            return "3:::MTK_REC_TOGGLE";
        }

        public string GetStopMTKRecordMessage()
        {
            return "3:::MTK_REC_TOGGLE";
        }

        public string GetStartRecordMessage()
        {
            return "3:::RECTOGGLE";
        }
        public string GetStopRecordMessage()
        {
            return "3:::RECTOGGLE";
        }

        public string Get2TrackPlayMessage()
        {
            return "3:::MEDIA_PLAY";
        }
        public string Get2TrackStopMessage()
        {
            return "3:::MEDIA_STOP";
        }

        public string Get2TrackNextMessage()
        {
            return "3:::MEDIA_NEXT";
        }
        public string Get2TrackPrevMessage()
        {
            return "3:::MEDIA_PREV";
        }

    }
}
