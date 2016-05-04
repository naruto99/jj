using System;
using EloBuddy.SDK.Events;

namespace WhoIsYourCheater
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            TheCheater.TheCheater.Load();
        }
    }
}
