using System;
using ProtoBuf;

namespace ShinyBluetoothTest.Models
{
    [ProtoContract]
    public class TestRequest
    {
        public TestRequest()
        {
        }

        [ProtoMember(1)]
        public string Data { get; set; }

        [ProtoMember(2)]
        public TestType Type { get; set; }
    }
}
