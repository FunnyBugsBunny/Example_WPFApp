using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuipuTest.Model
{
    public class Url
    {
        public string strUrl;
        public int numberTagA;
        public Url(string strUrl)
        {
            this.strUrl = strUrl;
            this.numberTagA = CalcNumberTagA();
        }
        private static int CalcNumberTagA()
        {
            int number = 0;
            try
            {
                //using ()
                //{

                //}
            }
            catch (Exception)
            {

                throw;
            }
            return number;
        }
    }
}
