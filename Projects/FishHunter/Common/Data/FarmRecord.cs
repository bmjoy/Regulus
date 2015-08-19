﻿
using System;

using ProtoBuf;

namespace VGame.Project.FishHunter.Common.Data
{
    [ProtoContract]
    public class FarmRecord
    {
        [ProtoContract]
        public class HitHistory
        {
            [ProtoMember(1)]
            public Guid Id { get; set; }

            [ProtoMember(2)]
            public FISH_TYPE FishType { get; private set; }

            [ProtoMember(3)]
            public int KillCount { get; set; }

            public HitHistory()
            {
                Id = new Guid();
            }
        }

        [ProtoMember(1)]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        public int FarmId { get; set; }

        [ProtoMember(3)]
        public int PlayTotal { get; set; } // 0

        [ProtoMember(4)]
        public int WinScore { get; set; }

        [ProtoMember(5)]
        public int PlayTimes { get; set; }

        [ProtoMember(6)]
        public int WinFrequency { get; set; }

        [ProtoMember(7)]
        public int AsnTimes { get; set; }

        [ProtoMember(8)]
        public int AsnWin { get; set; }

        [ProtoMember(9)]
        public FishPocket FishHitReuslt { get; set; }

        public FarmRecord(int farm_id)
        {
            Id = new Guid();
            FarmId = farm_id;
            FishHitReuslt = new FishPocket();
        }

        public FarmRecord()
        {
        }
    }
}