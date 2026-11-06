using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPSDK
{
    public class Scope
    {

            ///
            private int _Max;
            ///
            ///
            private int _Min;
        ///

        string _AA;

            public int Max
            {
                get { return _Max; }
                set { _Max = value; }
            }
            public int Min
            {
                get { return _Min; }
                set { _Min = value; }
            }

        public string AA
        {
            get { return _AA; }
            set { _AA = value; }
        }



    }
}
