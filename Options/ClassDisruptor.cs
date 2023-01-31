using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Straddle.AppClasses;
using Disruptor.Dsl;
using Disruptor;

namespace Straddle.AppClasses
{
    class ClassDisruptor
    {
        /// <summary>
        /// 
        /// </summary>
        //public static Connection connection;
        /// <summary>
        /// 
        /// </summary>
        public static Disruptor<Straddle.AppClasses.PacketProcess> RequestDisruptor;
        /// <summary>
        /// 
        /// </summary>
        public static RingBuffer<Straddle.AppClasses.PacketProcess> ringBufferRequest;
    }
}
