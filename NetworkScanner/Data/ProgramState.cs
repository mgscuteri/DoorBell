using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkScanner.Data
{
    public enum ProgramState : short
    {

        /// <summary>
        /// The program has not yet launched any ping threads
        /// </summary>
        programStart = 0,

        /// <summary>
        /// The program has began launching ping threads.
        /// </summary>
        PingAllStarted = 1,

        /// <summary>
        /// The program has launched all ping threads, and waited for responses. All responses *should* be recieved already
        /// </summary>
        PingAllCompleted = 2,

        /// <summary>
        /// Processing data from pings
        /// </summary>
        ProcessingPingResponses = 4,

        /// <summary>
        /// Done processessing pings. Ready to ping again. 
        /// </summary>
        DoneProcessingPingResponses = 5,
    }
}
