using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NXOpen;
using NXOpen.UF;


namespace MoldQuote
{
    public class MoldQuote
    {
        public static int Main(string[] args)
        {
            if (args[0] == "MENU_MoldQuote")
            {
                MoldQuoteUI.MoldQuote mq = new MoldQuoteUI.MoldQuote();
                mq.Show();
            }

            return 0;
        }


        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
            // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }

    }

}
