//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Vigilant
{
    using System;
    using System.Collections.Generic;
    
    public partial class Strike
    {
        public string ServerId { get; set; }
        public string ChannelId { get; set; }
        public string ReportedId { get; set; }
        public string ReporterId { get; set; }
        public byte Type { get; set; }
        public System.DateTime Time { get; set; }
    }

    public enum StrikeType : byte
    {
        Kick = 1,
        Mute = 2,
        Ban = 3
    }
}