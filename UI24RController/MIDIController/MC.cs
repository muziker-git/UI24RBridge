﻿using System;
using System.Collections.Generic;
using System.Text;
using Commons.Music.Midi;
using System.Linq;
using System.Threading.Tasks;

namespace UI24RController.MIDIController
{
    public class MC : IMIDIController, IDisposable
    {
        /// <summary>
        /// Store every fader setted value of the faders, key is the channel number (z in the message)
        /// </summary>
        protected Dictionary<byte, double> faderValues = new Dictionary<byte, double>();
            
        IMidiInput _input = null;
        IMidiOutput _output = null;

        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<FaderEventArgs> FaderEvent;
        public event EventHandler<EventArgs> PresetUp;
        public event EventHandler<EventArgs> PresetDown;
        public event EventHandler<GainEventArgs> GainEvent;
        public event EventHandler<ChannelEventArgs> SelectChannelEvent;

        protected void OnMessageReceived(string message)
        {
            if (MessageReceived != null)
            {
                var messageEventArgs = new MessageEventArgs();
                messageEventArgs.Message = message;
                MessageReceived(this, messageEventArgs);
            }
        }

        protected void OnFaderEvent(int channelNumber, double faderValue)
        {
            if (FaderEvent != null)
            {
                var faderEventArgs = new FaderEventArgs(channelNumber, faderValue);
                FaderEvent(this, faderEventArgs);
            }
        }

        protected void OnGainEvent(int channelNumber, int gainDirection)
        {
            if (GainEvent != null)
            {
                var gainEventArgs = new GainEventArgs(channelNumber, gainDirection);
                GainEvent(this, gainEventArgs);
            }
        }

        protected void OnSelectEvent(int channelNumber)
        {
            if (SelectChannelEvent != null)
            {
                var channelArgs = new ChannelEventArgs(channelNumber);
                SelectChannelEvent(this, channelArgs);
            }
        }

        protected void OnPresetUp()
        {
            if (PresetUp != null)
            {
                PresetUp(this, new EventArgs());
            }
        }
        protected void OnPresetDown()
        {
            if (PresetDown != null)
            {
                PresetDown(this, new EventArgs());
            }
        }

        public void Dispose()
        {
            if (_input != null)
            {
                _input.Dispose();
                _input = null;
            }
            if (_output != null)
            {
                _output.Dispose();
                _output = null;
            }
        }

        public  bool ConnectInputDevice(string deviceName)
        {
            var access = MidiAccessManager.Default;
            var deviceNumber = access.Inputs.Where(i => i.Name == deviceName).FirstOrDefault();
            if (deviceNumber != null)
            {
                _input = access.OpenInputAsync(deviceNumber.Id).Result;
                _input.MessageReceived += (obj, e) =>
                {
                    if (e.Data.Length>2)
                    {
                        OnMessageReceived( $"{e.Data[0].ToString("x2")} - {e.Data[1].ToString("x2")} - {e.Data[2].ToString("x2")}");
                        ProcessMidiMessage(e);
                    }
                };
                return true;
            }
            else
                return false;            
        }

        private void ProcessMidiMessage(MidiReceivedEventArgs e)
        {
            var message = e.Data;

            if (message[0] == 0x90) //button pressed, released, fader released 
            {
                if (message.MIDIEqual(0x90, 0x00, 0x00, 0xff, 0x00, 0xff) && (message[1] >= 0x68) && (message[1] <= 0x70)) //release fader (0x90 [0x68-0x70] 0x00)
                {
                    byte channelNumber = (byte)(message[1] - 0x68);
                    if (faderValues.ContainsKey(channelNumber))
                    {
                        SetFader(channelNumber, faderValues[channelNumber]);
                    }
                }
                else if (message.MIDIEqual(0x90, 0x2f, 0x7f)) //fader bank right press
                {
                    OnPresetUp();
                }
                else if (message.MIDIEqual(0x90, 0x2e, 0x7f)) //fader bank left press
                {
                    OnPresetDown();
                }
                else if  (message[1] >= 0x18 && message[1] <= 0x1f && message[2] == 0x7f) //select button
                {
                    byte channelNumber = (byte)(message[1] - 0x18);
                    OnSelectEvent(channelNumber);
                }

            }
            else if (message[0] >= 0xe0 && (message[0] <= 0xe8)) //move fader
            {
                byte channelNumber = (byte)(message[0] - 0xe0);
                //int data2 = (int)Math.Round(faderValue * 1023); //10 bit
                int upper = message[2] << 7; //upper 7 bit
                int lower = message[1]; // lower 7 bit

                var faderValue = (upper + lower) / 16383.0;
                faderValues.AddOrSet(channelNumber, faderValue);
                OnFaderEvent(channelNumber, faderValue);

            } 
            else if(message[0] == 0xb0 && (message[1] >= 0x10 && message[1] <= 0x17)) //channel knobb turning
            {
                byte channelNumber = (byte)(message[1] - 0x10);
                if (message[2] >= 0x01 && message[2] <= 0x03)
                    OnGainEvent(channelNumber, message[2]);
                if (message[2] >= 0x41 && message[2] <= 0x43)
                    OnGainEvent(channelNumber, -1*(message[2]&0x03));
            }
        }

        public  bool ConnectOutputDevice(string deviceName)
        {
            var access = MidiAccessManager.Default;
            var deviceNumber = access.Outputs.Where(i => i.Name == deviceName).FirstOrDefault();
            if (deviceNumber != null)
            {
                _output = access.OpenOutputAsync(deviceNumber.Id).Result;
                return true;
            }
            return false;
        }



        public  string[] GetInputDeviceNames()
        {
            var access = MidiAccessManager.Default;
            return access.Inputs.Select(port => port.Name).ToArray();
        }

        public  string[] GetOutputDeviceNames()
        {
            var access = MidiAccessManager.Default;
            return access.Outputs.Select(port => port.Name).ToArray();
        }

        public  bool SetFader(int channelNumber, double faderValue)
        {
            if (_output != null && channelNumber < 9)
            {

                byte data0 = 0xb0;
                byte z = Convert.ToByte(channelNumber);
                int data2 = (int)Math.Round(faderValue * 16383); //14 bit
                byte upper = (byte)(data2 >> 7); //upper 7 bit
                byte lower = (byte)((data2) & 0x7f); // lower 7 bit

                _output.Send(new byte[] { (byte)(0xe0 + channelNumber), lower, upper }, 0, 3, 0);


                faderValues.AddOrSet(z, faderValue);

                return true;
            }
            return false;
        }

        public bool SetGainLed(int channelNumber, double gainValue)
        {
            if (_output != null && channelNumber < 9)
            {
                var segment = Convert.ToByte( Math.Round(gainValue * 12));
                if (segment == 0)
                    segment = 1;
                if (segment == 0x0c)
                    segment = 0x0b;
                _output.Send(new byte[] {0xb0, (byte)(0x30 + channelNumber), segment }, 0, 3, 0);
                return true;
            }
            return false;
        }

        public void SetSelectLed(int channelNumber, bool turnOn)
        {
            //turn off all select led
            for (byte i = 0; i < 8; i++)
            {
                _output.Send(new byte[] { 0x90, (byte)(0x18 + i), 0x00 }, 0, 3, 0);
            }
            if (turnOn)
            {
                _output.Send(new byte[] { 0x90, (byte)(0x18 + channelNumber), 0x7f }, 0, 3, 0);
            }
        }
    }
}