﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UI24RController
{
    public class BridgeSettings
    {
        public enum RecButtonBehaviorEnum
        {
            OnlyTwoTrack,
            OnlyMTK,
            TwoTrackAndMTK
        }

        public enum AuxButtonBehaviorEnum
        {
            Release, Lock
        }
        public enum ChannelRecButtonBehaviorEnum
        {
            Rec,
            Phantom
        }

        //address, controller, messageWriter, syncID, defaultRecButtonIsMtk
        public string Address { get; set; }
        public IMIDIController Controller { get; set; }
        public Action<string,bool> MessageWriter { get; set; }
        public string SyncID { get; set; }
        public RecButtonBehaviorEnum RecButtonBehavior { get; set; }
        public ChannelRecButtonBehaviorEnum ChannelRecButtonBehavior { get; set; }
        public AuxButtonBehaviorEnum AuxButtonBehavior { get; set; }

        public BridgeSettings(string address, IMIDIController controller, Action<string, bool> messageWriter) 
            : this(address, controller, messageWriter, "SYNC_ID", RecButtonBehaviorEnum.TwoTrackAndMTK, ChannelRecButtonBehaviorEnum.Rec)
        {
        }

        public BridgeSettings(string address, IMIDIController controller, Action<string, bool> messageWriter, string syncID)
            : this(address, controller, messageWriter, syncID, RecButtonBehaviorEnum.TwoTrackAndMTK, ChannelRecButtonBehaviorEnum.Rec)
        {
        }

        public BridgeSettings(string address, IMIDIController controller, Action<string, bool> messageWriter, RecButtonBehaviorEnum recButtonBehavior)
            : this(address, controller, messageWriter, "SYNC_ID", recButtonBehavior, ChannelRecButtonBehaviorEnum.Rec)
        {
        }

        public BridgeSettings(string address, IMIDIController controller, Action<string, bool> messageWriter, RecButtonBehaviorEnum recButtonBehavior, ChannelRecButtonBehaviorEnum channelRecButtonBehavior)
            : this(address, controller, messageWriter, "SYNC_ID", recButtonBehavior, channelRecButtonBehavior)
        {
        }

        public BridgeSettings(string address, IMIDIController controller, Action<string, bool> messageWriter, string syncID,
                RecButtonBehaviorEnum recButtonBehavior, ChannelRecButtonBehaviorEnum channelRecButtonBehavior)
        {
            this.Address = address;
            this.Controller = controller;
            this.MessageWriter = messageWriter;
            this.SyncID = syncID;
            this.RecButtonBehavior = recButtonBehavior;
            this.ChannelRecButtonBehavior = channelRecButtonBehavior;
            this.AuxButtonBehavior = AuxButtonBehaviorEnum.Release;
        }

    }
}
