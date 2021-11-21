namespace QuipuTest.Model
{
    public class Url
    {
        private string strUrl;
        private int numberTags;

        public string StrUrl
        {
            get
            {
                return strUrl;
            }
        }
        public int NumberTags
        {
            get
            {
                return numberTags;
            }
            set
            {
                numberTags = value;
            }
        }

        public Url(string strUrl, int numberTags)
        {
            this.strUrl = strUrl;
            this.numberTags = numberTags;
        }
    }
}
