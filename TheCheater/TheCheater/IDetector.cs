using EloBuddy;
using SharpDX;

namespace WhoIsYourCheater.TheCheater
{
    public enum DetectorSetting { Preferred, Safe, AntiHumanizer}
    interface IDetector
    {
        void Initialize(AIHeroClient hero, DetectorSetting setting = DetectorSetting.Safe);
        void ApplySetting(DetectorSetting setting);
        void FeedData(Vector3 targetPos);
        int GetScriptDetections();
        string GetName();
    }
}
