using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot {
    /// <summary>
    /// A class to represent config values that are hardcoded, but can be edited from a single location.<para/>
    /// This is purely for those who wish to modify values pertaining to Katsuki in the event of someone wanting to self-host their own instance, but would almost never be edited otherwise.<para/>
    /// Almost.
    /// </summary>
    class HardConfig {
        /// <summary>
        /// The URL that Katsuki is going to use to accept API requests.
        /// </summary>
        public static readonly string APIURL = "http://*:6969";
    }
}
