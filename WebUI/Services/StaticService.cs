namespace WebAPI.Services
{
    public static class StaticService
    {
        public static string GeneratePhoneNumber()
        {
            Random random = new Random();
            string r = "09";
            for (var i = 1; i < 10; i++)
            {
                r += random.Next(0, 9).ToString();
            }
            return r;
        }
        public static string GeneratePassword()
        {
            Random random = new Random();
            string r = "";
            for (var i = 1; i < 6; i++)
            {
                r += random.Next(0, 9).ToString();
            }
            return r;
        }
    }
}