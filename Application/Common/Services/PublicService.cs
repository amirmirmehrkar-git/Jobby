namespace Application.Common.Services
{
    public static class PublicService
    {
        public static string PriceToToman(string Price)
        {
            string MomayezConvertedText = Price;
            int doubleStatus = 0;
            for (int j = 0; j < MomayezConvertedText.Length; j++)
            {
                if (MomayezConvertedText[j] == '.')
                {
                    doubleStatus = (MomayezConvertedText.Length - j) - 1;
                }
            }

            if (doubleStatus == 0)
            {
                MomayezConvertedText += ".00";
            }
            if (doubleStatus == 1)
            {
                MomayezConvertedText += '0';
            }
            if (doubleStatus == 2)
            {

            }
            else
            {
                for (int i = 0; i < MomayezConvertedText.Length; i++)
                {
                    if (MomayezConvertedText[i] == '.')
                    {
                        MomayezConvertedText = MomayezConvertedText.Substring(0, i + 3);
                    }
                }
            }

            string Ashar = MomayezConvertedText.Substring(MomayezConvertedText.Length - 3, 3);
            string Rond = MomayezConvertedText.Substring(0, MomayezConvertedText.Length - 3);

            if (Rond.Length <= 3)
            {
                return Rond;
            }

            int counter = 0;
            string final = "";

            for (int i = Rond.Length - 1; i >= 0; i--)
            {
                counter++;
                final += Rond[i];
                if (counter % 3 == 0)
                {
                    if (i != 0)
                    {
                        final += ',';

                    }
                }
            }

            string final2 = "";
            for (int i = final.Length - 1; i >= 0; i--)
            {
                final2 += final[i];
            }
            return final2;
        }
    }
}
